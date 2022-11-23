/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.CecilEx;
using Mono.CecilEx.Cil;
using Mono.CecilEx.Rocks;
using Piot.Clog;
using Piot.Flood;
using Piot.Surge.Generator;
using Piot.Surge.Input;
using Piot.Surge.Types;
using Piot.Surge.Types.Serialization;
using MethodAttributes = Mono.CecilEx.MethodAttributes;
using ParameterAttributes = Mono.CecilEx.ParameterAttributes;
using TypeAttributes = Mono.CecilEx.TypeAttributes;

namespace Piot.Surge.Core.Generator
{
    public class ILStaticClassGenerator
    {
        const string GeneratedNamespace = "Piot.Surge.Generated";
        const string GeneratedClassName = "GeneratedSurge";

        readonly TypeReference bitReaderInterfaceReference;
        readonly TypeReference bitWriterInterfaceReference;
        readonly MethodReference dataReceiverCreateNewMethodReference;
        readonly MethodReference dataReceiverDestroyMethodReference;
        readonly MethodReference dataReceiverGetMethodReference;


        readonly TypeReference dataReceiverInterfaceReference;
        readonly MethodReference dataReceiverUpdateMethodReference;
        readonly List<DataTypeInfo> dataTypeInfos = new();
        readonly TypeDefinition generatedStaticClass;
        readonly MethodReference genericDataStreamReaderCreateAndReadReference;
        readonly MethodReference genericDataStreamReaderReadMaskMethodReference;

        readonly CustomAttribute initializeOnLoadCustomAttribute;
        readonly ModuleDefinition moduleDefinition;

        readonly MethodReference readBitsMethod;
        readonly MethodReference writeBitsMethod;

        DataClassMeta? currentData;

        public readonly List<DataClassMeta> dataClassMetas = new();

        TypeReference dataStreamReaderReadMaskMethodReference;

        TypeReference dataStreamReaderReference;
        public MethodReference? generatedDataReceiverDestroyMethod;

        public MethodReference? generatedDataReceiverNewMethod;
        public MethodReference? generatedDataReceiverUpdateMethod;

        List<TypeReference> importedTypes = new();

        uint lastUniqueId;

        public ILStaticClassGenerator(ModuleDefinition moduleDefinition, CustomAttribute runtimeInitializeOnLoad)
        {
            initializeOnLoadCustomAttribute = runtimeInitializeOnLoad;
            this.moduleDefinition = moduleDefinition;
            generatedStaticClass = CreateRootClass(moduleDefinition);
            moduleDefinition.Types.Add(generatedStaticClass);

            var readBitsMethodInfo = typeof(IBitReader).GetMethod(nameof(IBitReader.ReadBits));
            readBitsMethod = moduleDefinition.ImportReference(readBitsMethodInfo);

            bitReaderInterfaceReference = readBitsMethod.DeclaringType;

            if (readBitsMethod is null)
            {
                throw new("Internal error. Can not find ReadBits");
            }

            var writeBitsMethodInfo = typeof(IBitWriter).GetMethod(nameof(IBitWriter.WriteBits));
            writeBitsMethod = moduleDefinition.ImportReference(writeBitsMethodInfo);
            if (writeBitsMethod is null)
            {
                throw new("Internal error. Can not find WriteBits");
            }

            bitWriterInterfaceReference = writeBitsMethod.DeclaringType;


            var dataReceiverCreateNewMethodInfo = typeof(IDataReceiver).GetMethod(nameof(IDataReceiver.ReceiveNew));
            dataReceiverCreateNewMethodReference = moduleDefinition.ImportReference(dataReceiverCreateNewMethodInfo);
            dataReceiverInterfaceReference = dataReceiverCreateNewMethodReference.DeclaringType;


            var dataReceiverUpdateMethodInfo = typeof(IDataReceiver).GetMethod(nameof(IDataReceiver.Update));
            dataReceiverUpdateMethodReference = moduleDefinition.ImportReference(dataReceiverUpdateMethodInfo);

            var dataReceiverDestroyMethodInfo = typeof(IDataReceiver).GetMethod(nameof(IDataReceiver.DestroyComponent));
            dataReceiverDestroyMethodReference = moduleDefinition.ImportReference(dataReceiverDestroyMethodInfo);


            var dataReceiverGetMethodInfo = typeof(IDataReceiver).GetMethod(nameof(IDataReceiver.Grab));
            dataReceiverGetMethodReference = moduleDefinition.ImportReference(dataReceiverGetMethodInfo);


            var dataStreamReaderCreateAndReadMethodInfo = typeof(DataStreamReader).GetMethod(nameof(DataStreamReader.CreateAndRead));
            genericDataStreamReaderCreateAndReadReference = moduleDefinition.ImportReference(dataStreamReaderCreateAndReadMethodInfo);
            dataStreamReaderReference = genericDataStreamReaderCreateAndReadReference.DeclaringType;


            var dataStreamReaderReadMaskMethodInfo = typeof(DataStreamReader).GetMethod(nameof(DataStreamReader.ReadMask));
            genericDataStreamReaderReadMaskMethodReference = moduleDefinition.ImportReference(dataStreamReaderReadMaskMethodInfo);
            dataStreamReaderReadMaskMethodReference = genericDataStreamReaderReadMaskMethodReference.DeclaringType;

        }

        public void GenerateDataTypes(IEnumerable<TypeDefinition> dataTypeReferences, ILog log)
        {
            foreach (var dataTypeReference in dataTypeReferences)
            {
                GenerateDataType(dataTypeReference, log);
            }

            CreateDataReceiveNew(dataTypeInfos, log);
            CreateDataReceiveUpdate(dataTypeInfos, log);
            CreateDataReceiveDestroy(dataTypeInfos, log);
        }

        void GenerateDataType(TypeDefinition dataTypeReference, ILog log)
        {
            var dataTypeInfo = SetupDataTypeInfo(dataTypeReference, log);
            dataTypeInfos.Add(dataTypeInfo);

            CreateDeserializeAllMethod(dataTypeInfo, log);
            CreateDeserializeAllRefMethod(dataTypeInfo, log);
            CreateDeserializeMaskRefMethod(dataTypeInfo, log);

            CreateSerializeMaskRefMethod(dataTypeInfo, log);
            CreateSerializeFullMethod(dataTypeInfo, log);

            CreateDifferMethod(dataTypeInfo, log);
        }

        public void Close()
        {
            CreateInitOnLoadMethod();
        }

        public static string UnityEngineInternalDllDirectory()
        {
            // can maybe use Application.dataPath as well?
            var directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            if (directoryName is null)
            {
                throw new("internal error");
            }

            return directoryName.Replace(@"file:\", "");
        }

        static TypeDefinition CreateRootClass(ModuleDefinition moduleDefinition)
        {
            return new(GeneratedNamespace, GeneratedClassName,
                TypeAttributes.BeforeFieldInit // Static class
                | TypeAttributes.Class
                | TypeAttributes.Public
                | TypeAttributes.Sealed
                | TypeAttributes.Abstract // Needed for static classes
                | TypeAttributes.AutoClass // String stuff
                | TypeAttributes.AnsiClass, // Ansi Strings // TODO: fix it?
                moduleDefinition.ImportReference(typeof(object)));
        }

        public static bool ContainsGeneratedStaticClass(ModuleDefinition moduleDefinition)
        {
            return moduleDefinition.GetTypes().Any(td =>
                td.Namespace == GeneratedNamespace && td.Name == GeneratedClassName);
        }

        static bool IsBool(TypeReference fieldType)
        {
            return fieldType.IsPrimitive && fieldType.Name == nameof(Boolean);
        }

        static bool IsAllowedPrimitive(TypeReference fieldType)
        {
            var name = fieldType.Name;
            var allowed = new[]
            {
                nameof(Byte), nameof(SByte), nameof(UInt16), nameof(Int16), nameof(UInt32), nameof(Int32), nameof(UInt64), nameof(Int64)
            };

            return fieldType.IsPrimitive && allowed.Contains(name) || fieldType.Resolve().IsEnum;
        }

        static int BitCountFromType(TypeReference fieldType, ILog log)
        {
            // IBitReader.ReadBits argument
            var bitCount = 0;
            if (fieldType.IsPrimitive)
            {
                var fieldTypeName = fieldType.Name;
                switch (fieldTypeName)
                {
                    case nameof(Boolean):
                        bitCount = 1;
                        break;
                    case nameof(Byte):
                        bitCount = 8;
                        break;
                    case nameof(SByte):
                        bitCount = 8;
                        break;
                    case nameof(UInt16):
                        bitCount = 16;
                        break;
                    case nameof(Int16):
                        bitCount = 16;
                        break;
                    case nameof(UInt32):
                        bitCount = 32;
                        break;
                    case nameof(Int32):
                        bitCount = 32;
                        break;
                    default:
                        log.Error("Unknown {Primitive} type", fieldTypeName);
                        throw new($"Unknown primitive type {fieldTypeName}");
                }
            }

            return bitCount;
        }


        static void EmitCallMethodWithBitCount(ILProcessor processor, MethodReference bitsMethod, int bitCount)
        {
            processor.Emit(OpCodes.Ldc_I4, bitCount);
            processor.Emit(OpCodes.Callvirt, bitsMethod);
        }

        static void EmitBitCountDependingOnType(ILProcessor processor, TypeReference fieldType, MethodReference bitsMethod, ILog log)
        {
            var bitCount = BitCountFromType(fieldType, log);
            EmitCallMethodWithBitCount(processor, bitsMethod, bitCount);
        }


        void EmitReadBits(ILProcessor processor, TypeReference fieldType, ILog log)
        {
            EmitBitCountDependingOnType(processor, fieldType, readBitsMethod, log);
        }

        void EmitWriteBits(ILProcessor processor, TypeReference fieldType, ILog log)
        {
            EmitBitCountDependingOnType(processor, fieldType, writeBitsMethod, log);
        }


        static void EmitConversionFromUInt32DependingOnType(ILProcessor processor, TypeReference fieldType, ILog log)
        {

            if (!fieldType.IsPrimitive)
            {
                return;
            }

            var fieldTypeName = fieldType.Name;
            switch (fieldTypeName)
            {
                case nameof(Byte):
                    processor.Emit(OpCodes.Conv_U1);
                    break;
                case nameof(Boolean):
                    processor.Emit(OpCodes.Ldc_I4_0); // Compare with zero
                    processor.Emit(OpCodes.Cgt_Un);
                    break;
                case nameof(UInt16):
                    break;
                case nameof(UInt32):
                    break;
                default:
                    log.Error("Unknown {Primitive} type", fieldTypeName);
                    throw new($"Unknown primitive type {fieldTypeName}");
            }
        }

        public static void VerifyDataClassType(TypeDefinition resolvedDataStructType, ILog log)
        {
            if (!resolvedDataStructType.IsValueType)
            {
                log.Error("The type {Type} is not a valid Data Type (must be struct value type)", resolvedDataStructType.Name);
                throw new("The type {Type} is not a valid Data Type (must be struct value type)");
            }

            foreach (var field in resolvedDataStructType.Fields)
            {
                if (field.IsNotSerialized || field.IsStatic || field.IsPrivate)
                {
                    log.Notice("Can not serialize field {FieldName} in type {TypeName}. Must be public, instance field and not marked as [NotSerialized]", field.Name, resolvedDataStructType.Name);
                    throw new($"Can not serialize field {field.Name} in {resolvedDataStructType.Name}");
                }
            }
        }

        public DataTypeInfo SetupDataTypeInfo(TypeReference dataClassType, ILog log)
        {
            var resolvedDataStructType = dataClassType.Resolve();
            VerifyDataClassType(resolvedDataStructType, log);
            moduleDefinition.ImportReference(dataClassType);

            lastUniqueId++;

            currentData = new()
            {
                uniqueId = lastUniqueId, typeReference = dataClassType, typeByReference = new(resolvedDataStructType)
            };
            dataClassMetas.Add(currentData);

            return new()
            {
                resolvedDataStructType = resolvedDataStructType, dataStructTypeReference = dataClassType, resolvedDataStructTypeByReference = new(resolvedDataStructType)
            };
        }

        static string GenerateSafeFullName(TypeDefinition typeDefinition)
        {
            return typeDefinition.FullName.Replace(".", "_");
        }


        static string GenerateSafeFullNameWithPrefix(string prefix, TypeDefinition typeDefinition)
        {
            return $"{prefix}_{GenerateSafeFullName(typeDefinition)}";
        }

        static MethodDefinition CreatePublicStaticMethodForType(string prefix, TypeDefinition dataStructType)
        {
            var generatedMethodName = GenerateSafeFullNameWithPrefix(prefix, dataStructType);
            return new(generatedMethodName,
                MethodAttributes.Public |
                MethodAttributes.Static |
                MethodAttributes.HideBySig,
                dataStructType);
        }

        MethodDefinition CreatePublicStaticMethodForTypeVoidReturn(string prefix, TypeDefinition dataStructType)
        {
            var generatedMethodName = GenerateSafeFullNameWithPrefix(prefix, dataStructType);
            return new(generatedMethodName,
                MethodAttributes.Public |
                MethodAttributes.Static |
                MethodAttributes.HideBySig,
                moduleDefinition.ImportReference(typeof(void)));
        }

        MethodDefinition CreatePublicStaticMethodForTypeUInt32Return(string prefix, TypeDefinition dataStructType)
        {
            var generatedMethodName = GenerateSafeFullNameWithPrefix(prefix, dataStructType);
            return new(generatedMethodName,
                MethodAttributes.Public |
                MethodAttributes.Static |
                MethodAttributes.HideBySig,
                moduleDefinition.ImportReference(typeof(uint)));
        }

        MethodDefinition CreatePublicStaticInitOnLoadMethod()
        {
            return new("InitOnLoad", MethodAttributes.Public |
                                     MethodAttributes.Static,
                moduleDefinition.ImportReference(typeof(void)));

        }

        public MethodDefinition CreateDeserializeAllMethod(DataTypeInfo dataTypeInfo, ILog log)
        {
            var deserializeMethod = CreatePublicStaticMethodForType("Deserialize", dataTypeInfo.resolvedDataStructType);

            deserializeMethod.Parameters.Add(new("reader", ParameterAttributes.In, bitReaderInterfaceReference));
            deserializeMethod.Body.InitLocals = true;

            var variableForNewType = new VariableDefinition(dataTypeInfo.dataStructTypeReference);
            deserializeMethod.Body.Variables.Add(variableForNewType);

            var processor = deserializeMethod.Body.GetILProcessor();
            processor.Emit(OpCodes.Ldloca_S, (byte)0);
            processor.Emit(OpCodes.Initobj, dataTypeInfo.resolvedDataStructType);

            foreach (var field in dataTypeInfo.resolvedDataStructType.Fields)
            {
                EmitDataTypeReader(processor, field, true, log);

                moduleDefinition.ImportReference(field);
            }

            // Load back our instance data type and return it
            processor.Emit(OpCodes.Ldloc_0);
            processor.Emit(OpCodes.Ret);

            AddGeneratedMethod(deserializeMethod);

            currentData!.readFullMethodReference = deserializeMethod;

            return deserializeMethod;
        }

        public MethodDefinition CreateDeserializeAllRefMethod(DataTypeInfo dataTypeInfo, ILog log)
        {
            var deserializeMethod = CreatePublicStaticMethodForTypeVoidReturn("DeserializeAllRef", dataTypeInfo.resolvedDataStructType);

            deserializeMethod.Parameters.Add(new("reader", ParameterAttributes.In, bitReaderInterfaceReference));
            var dataReferenceParameter = new ParameterDefinition("data", ParameterAttributes.None, dataTypeInfo.resolvedDataStructTypeByReference);

            //Assert.IsTrue(dataReferenceParameter.ParameterType.IsByReference);
            deserializeMethod.Parameters.Add(dataReferenceParameter);
            deserializeMethod.Body.InitLocals = true;

            var processor = deserializeMethod.Body.GetILProcessor();

            foreach (var field in dataTypeInfo.resolvedDataStructType.Fields)
            {
                EmitDataTypeReader(processor, field, false, log);

                moduleDefinition.ImportReference(field);
            }

            processor.Emit(OpCodes.Ret);

            AddGeneratedMethod(deserializeMethod);

            return deserializeMethod;
        }


        public MethodDefinition CreateDeserializeMaskRefMethod(DataTypeInfo dataTypeInfo, ILog log)
        {
            var deserializeMethod = CreatePublicStaticMethodForTypeUInt32Return("DeserializeMaskRef", dataTypeInfo.resolvedDataStructType);

            deserializeMethod.Parameters.Add(new("reader", ParameterAttributes.None, bitReaderInterfaceReference));
            var dataReferenceParameter = new ParameterDefinition("data", ParameterAttributes.None, dataTypeInfo.resolvedDataStructTypeByReference);

            //Assert.IsTrue(dataReferenceParameter.ParameterType.IsByReference);
            deserializeMethod.Parameters.Add(dataReferenceParameter);
            deserializeMethod.Body.InitLocals = true;

            var processor = deserializeMethod.Body.GetILProcessor();

            var maskBitCount = dataTypeInfo.resolvedDataStructType.Fields.Count;

            processor.Emit(OpCodes.Ldarg_0);
            EmitCallMethodWithBitCount(processor, readBitsMethod, maskBitCount);

            var index = 0;
            Instruction? skipLabel = null;
            foreach (var field in dataTypeInfo.resolvedDataStructType.Fields)
            {
                if (skipLabel is not null)
                {
                    processor.Append(skipLabel);
                }

                processor.Emit(OpCodes.Dup);

                var valueToCheck = 1 << index;
                processor.Emit(OpCodes.Ldc_I4, valueToCheck);
                processor.Emit(OpCodes.And);
                skipLabel = processor.Create(OpCodes.Nop);
                processor.Emit(OpCodes.Brfalse_S, skipLabel);

                EmitDataTypeReader(processor, field, false, log);

                moduleDefinition.ImportReference(field);
                index++;
            }

            if (skipLabel is not null)
            {
                processor.Append(skipLabel);
            }

            processor.Emit(OpCodes.Ret);

            AddGeneratedMethod(deserializeMethod);

            currentData!.readMaskMethodReference = deserializeMethod;

            return deserializeMethod;
        }

        void AddGeneratedMethod(MethodDefinition methodDefinition)
        {
            generatedStaticClass.Methods.Add(methodDefinition);
        }

        FieldReference SpecializeField(FieldReference self, GenericInstanceType instanceType)
        {
            var reference = new FieldReference(self.Name, self.FieldType, instanceType);
            return moduleDefinition.ImportReference(reference);
        }

        MethodReference SpecializeMethod(MethodReference method, TypeReference specializeForType)
        {
            var instance = new GenericInstanceMethod(method);
            instance.GenericArguments.Add(specializeForType);

            return moduleDefinition.ImportReference(instance);
        }

        MethodReference SpecializeInstanceGenericIntoDeclaringType(MethodReference method, GenericInstanceType instanceType)
        {
            var methodReference = new MethodReference(method.Name, method.ReturnType, instanceType)
            {
                CallingConvention = method.CallingConvention, HasThis = method.HasThis, ExplicitThis = method.ExplicitThis
            };

            foreach (var parameter in method.Parameters)
            {
                methodReference.Parameters.Add(new(parameter.ParameterType));
            }

            foreach (var generic_parameter in method.GenericParameters)
            {
                methodReference.GenericParameters.Add(new(generic_parameter.Name, methodReference));
            }

            return moduleDefinition.ImportReference(methodReference);
        }

        void CreateFuncDelegateBitReaderToDataType(ILProcessor processor, MethodReference functionToBind, TypeReference dataTypeReference)
        {
            processor.Emit(OpCodes.Ldnull);
            processor.Emit(OpCodes.Ldftn, functionToBind);

            var funcWithTwoParametersDelegateReference = moduleDefinition.ImportReference(typeof(Func<,>));
            var funcWithBitReaderAndDataTypeReference = funcWithTwoParametersDelegateReference.MakeGenericInstanceType(bitReaderInterfaceReference, dataTypeReference);

            var funcWithTwoParametersDelegateConstructorReference = moduleDefinition.ImportReference(typeof(Func<,>).GetConstructors()[0]);
            var funcConstructorInstance = SpecializeInstanceGenericIntoDeclaringType(funcWithTwoParametersDelegateConstructorReference, funcWithBitReaderAndDataTypeReference);
            processor.Emit(OpCodes.Newobj, funcConstructorInstance);
        }

        void CreateFuncDelegateBitReaderToDataTypeAndMask(ILProcessor processor, MethodReference functionToBind, TypeReference dataTypeReference, ByReferenceType byReferenceType)
        {
            processor.Emit(OpCodes.Ldnull);
            processor.Emit(OpCodes.Ldftn, functionToBind);


            var genericReadMaskDelegateTypeReference = moduleDefinition.ImportReference(typeof(DataReader<>.ReadMaskDelegate));
            var specializedReadMaskDelegateTypeReference = genericReadMaskDelegateTypeReference.MakeGenericInstanceType(dataTypeReference);


            var genericReadMaskDelegateConstructorReference = moduleDefinition.ImportReference(typeof(DataReader<>.ReadMaskDelegate).GetConstructors()[0]);
            var specializedDelegateConstructorInstance = SpecializeInstanceGenericIntoDeclaringType(genericReadMaskDelegateConstructorReference, specializedReadMaskDelegateTypeReference);

            processor.Emit(OpCodes.Newobj, specializedDelegateConstructorInstance);
        }

        void CreateDataDifferDelegateInstance(ILProcessor processor, MethodReference functionToBind, TypeReference dataTypeReference)
        {
            processor.Emit(OpCodes.Ldnull);
            processor.Emit(OpCodes.Ldftn, functionToBind);


            var genericDiffDelegateTypeReference = moduleDefinition.ImportReference(typeof(DataDiffer<>.DiffDelegate));
            var specializedDiffDelegateTypeReference = genericDiffDelegateTypeReference.MakeGenericInstanceType(dataTypeReference);


            var genericDiffDelegateConstructor = moduleDefinition.ImportReference(typeof(DataDiffer<>.DiffDelegate).GetConstructors()[0]);
            var specializedDelegateConstructorInstance = SpecializeInstanceGenericIntoDeclaringType(genericDiffDelegateConstructor, specializedDiffDelegateTypeReference);

            processor.Emit(OpCodes.Newobj, specializedDelegateConstructorInstance);
        }

        void CreateActionDelegateBitWriterToDataType(ILProcessor processor, MethodReference functionToBind, TypeReference dataTypeReference)
        {
            processor.Emit(OpCodes.Ldnull);
            processor.Emit(OpCodes.Ldftn, functionToBind);

            var funcWithTwoParametersDelegateReference = moduleDefinition.ImportReference(typeof(Action<,>));
            var funcWithBitReaderAndDataTypeReference = funcWithTwoParametersDelegateReference.MakeGenericInstanceType(bitWriterInterfaceReference, dataTypeReference);

            var funcWithTwoParametersDelegateConstructorReference = moduleDefinition.ImportReference(typeof(Action<,>).GetConstructors()[0]);
            var funcConstructorInstance = SpecializeInstanceGenericIntoDeclaringType(funcWithTwoParametersDelegateConstructorReference, funcWithBitReaderAndDataTypeReference);
            processor.Emit(OpCodes.Newobj, funcConstructorInstance);
        }

        void CreateActionDelegateBitWriterToDataTypeAndMask(ILProcessor processor, MethodReference functionToBind, TypeReference dataTypeReference)
        {
            processor.Emit(OpCodes.Ldnull);
            processor.Emit(OpCodes.Ldftn, functionToBind);

            var funcWithTwoParametersAndReturnParametersDelegateReference = moduleDefinition.ImportReference(typeof(Action<,,>));
            var uint32Reference = moduleDefinition.ImportReference(typeof(uint));
            var funcWithBitReaderAndDataTypeReference = funcWithTwoParametersAndReturnParametersDelegateReference.MakeGenericInstanceType(bitWriterInterfaceReference, dataTypeReference, uint32Reference);

            var funcWithTwoParametersAndReturnParametersDelegateConstructorReference = moduleDefinition.ImportReference(typeof(Action<,,>).GetConstructors()[0]);
            var funcConstructorInstance = SpecializeInstanceGenericIntoDeclaringType(funcWithTwoParametersAndReturnParametersDelegateConstructorReference, funcWithBitReaderAndDataTypeReference);
            processor.Emit(OpCodes.Newobj, funcConstructorInstance);
        }

        // Action<IBitReader, uint, uint, IDataReceiver>
        void CreateActionDelegateBitWriterUIntUIntDataReceiver(ILProcessor processor, MethodReference functionToBind)
        {
            processor.Emit(OpCodes.Ldnull);
            processor.Emit(OpCodes.Ldftn, functionToBind);

            var funcWithTwoParametersAndReturnParametersDelegateReference = moduleDefinition.ImportReference(typeof(Action<,,,>));
            var uint32Reference = moduleDefinition.ImportReference(typeof(uint));
            var funcWithBitReaderAndDataTypeReference = funcWithTwoParametersAndReturnParametersDelegateReference.MakeGenericInstanceType(bitReaderInterfaceReference, uint32Reference, uint32Reference, dataReceiverInterfaceReference);

            var funcWithTwoParametersAndReturnParametersDelegateConstructorReference = moduleDefinition.ImportReference(typeof(Action<,,,>).GetConstructors()[0]);
            var funcConstructorInstance = SpecializeInstanceGenericIntoDeclaringType(funcWithTwoParametersAndReturnParametersDelegateConstructorReference, funcWithBitReaderAndDataTypeReference);
            processor.Emit(OpCodes.Newobj, funcConstructorInstance);
        }

        // Action<uint, uint>
        void CreateActionDelegateUIntUIntDataReceiver(ILProcessor processor, MethodReference functionToBind)
        {
            processor.Emit(OpCodes.Ldnull);
            processor.Emit(OpCodes.Ldftn, functionToBind);

            var funcWithTwoParametersAndReturnParametersDelegateReference = moduleDefinition.ImportReference(typeof(Action<,,>));
            var uint32Reference = moduleDefinition.ImportReference(typeof(uint));
            var funcWithBitReaderAndDataTypeReference = funcWithTwoParametersAndReturnParametersDelegateReference.MakeGenericInstanceType(uint32Reference, uint32Reference, dataReceiverInterfaceReference);

            var funcWithTwoParametersAndReturnParametersDelegateConstructorReference = moduleDefinition.ImportReference(typeof(Action<,,>).GetConstructors()[0]);
            var funcConstructorInstance = SpecializeInstanceGenericIntoDeclaringType(funcWithTwoParametersAndReturnParametersDelegateConstructorReference, funcWithBitReaderAndDataTypeReference);
            processor.Emit(OpCodes.Newobj, funcConstructorInstance);
        }


        void CreateInitOnLoadMethod()
        {
            var initOnLoadMethod = CreatePublicStaticInitOnLoadMethod();
            initOnLoadMethod.CustomAttributes.Add(initializeOnLoadCustomAttribute);

            var processor = initOnLoadMethod.Body.GetILProcessor();

            var genericDataIdLookup = moduleDefinition.ImportReference(typeof(DataIdLookup<>));
            if (genericDataIdLookup is null)
            {
                throw new("internal error. can not find DataIdLookup");
            }

            var genericDataIdLookupDef = genericDataIdLookup.Resolve();

            var targetValueFieldInfo = typeof(DataIdLookup<>).GetField("value");
            if (targetValueFieldInfo is null)
            {
                throw new("internal error. can not find DataIdLookup");
            }

            var genericValueFieldRef = moduleDefinition.ImportReference(targetValueFieldInfo);

            foreach (var dataMeta in dataClassMetas)
            {
                processor.Emit(OpCodes.Ldc_I4, (int)dataMeta.uniqueId);
                var specializedDataIdInstanceType = genericDataIdLookupDef.MakeGenericInstanceType(dataMeta.typeReference);
                var specializeField = SpecializeField(genericValueFieldRef, specializedDataIdInstanceType);
                processor.Emit(OpCodes.Stsfld, specializeField);
            }

            var genericDataReaderStaticClassReference = moduleDefinition.ImportReference(typeof(DataReader<>));
            var readDelegateFieldInfo = typeof(DataReader<>).GetField(nameof(DataReader<AnyStruct>.read));
            var readDelegateFieldReference = moduleDefinition.ImportReference(readDelegateFieldInfo);

            var readMaskDelegateFieldInfo = typeof(DataReader<>).GetField(nameof(DataReader<AnyStruct>.readMask));
            var readMaskDelegateFieldReference = moduleDefinition.ImportReference(readMaskDelegateFieldInfo);


            var genericDataWriterStaticClassReference = moduleDefinition.ImportReference(typeof(DataWriter<>));

            var writeFullDelegateFieldInfo = typeof(DataWriter<>).GetField(nameof(DataWriter<AnyStruct>.write));
            var writeFullDelegateFieldReference = moduleDefinition.ImportReference(writeFullDelegateFieldInfo);

            var writeMaskDelegateFieldInfo = typeof(DataWriter<>).GetField(nameof(DataWriter<AnyStruct>.writeMask));
            var writeMaskDelegateFieldReference = moduleDefinition.ImportReference(writeMaskDelegateFieldInfo);


            var genericDataDifferStaticClassReference = moduleDefinition.ImportReference(typeof(DataDiffer<>));
            var diffDelegateFieldInfo = typeof(DataDiffer<>).GetField(nameof(DataDiffer<AnyStruct>.diff));
            var diffDelegateFieldReference = moduleDefinition.ImportReference(diffDelegateFieldInfo);

            foreach (var dataMeta in dataClassMetas)
            {
                {
                    CreateFuncDelegateBitReaderToDataType(processor, dataMeta.readFullMethodReference!, dataMeta.typeReference!);

                    var specializedDataReaderForDataType = genericDataReaderStaticClassReference.MakeGenericInstanceType(dataMeta.typeReference);
                    var specializedDataReaderField = SpecializeField(readDelegateFieldReference, specializedDataReaderForDataType);
                    processor.Emit(OpCodes.Stsfld, specializedDataReaderField);
                }

                {
                    CreateFuncDelegateBitReaderToDataTypeAndMask(processor, dataMeta.readMaskMethodReference!, dataMeta.typeReference!, dataMeta.typeByReference!);

                    var specializedDataReaderForDataType = genericDataReaderStaticClassReference.MakeGenericInstanceType(dataMeta.typeReference);
                    var specializedDataReaderField = SpecializeField(readMaskDelegateFieldReference, specializedDataReaderForDataType);
                    processor.Emit(OpCodes.Stsfld, specializedDataReaderField);
                }

                {
                    CreateActionDelegateBitWriterToDataType(processor, dataMeta.writeFullMethodReference!, dataMeta.typeReference!);

                    var specializedDataWriterForDataType = genericDataWriterStaticClassReference.MakeGenericInstanceType(dataMeta.typeReference);
                    var specializedDataWriterField = SpecializeField(writeFullDelegateFieldReference, specializedDataWriterForDataType);
                    processor.Emit(OpCodes.Stsfld, specializedDataWriterField);
                }

                {
                    CreateActionDelegateBitWriterToDataTypeAndMask(processor, dataMeta.writeMaskMethodReference!, dataMeta.typeReference!);

                    var specializedDataWriterForDataType = genericDataWriterStaticClassReference.MakeGenericInstanceType(dataMeta.typeReference);
                    var specializedDataWriteMaskField = SpecializeField(writeMaskDelegateFieldReference, specializedDataWriterForDataType);
                    processor.Emit(OpCodes.Stsfld, specializedDataWriteMaskField);
                }
            }


            foreach (var dataMeta in dataClassMetas)
            {
                CreateDataDifferDelegateInstance(processor, dataMeta.diffMethodReference!, dataMeta.typeReference!);
                var specializedDataDifferForDataType = genericDataDifferStaticClassReference.MakeGenericInstanceType(dataMeta.typeReference);
                var specializedDataReaderField = SpecializeField(diffDelegateFieldReference, specializedDataDifferForDataType);
                processor.Emit(OpCodes.Stsfld, specializedDataReaderField);
            }

            {
                CreateActionDelegateBitWriterUIntUIntDataReceiver(processor, generatedDataReceiverNewMethod!);

                var dataStreamReceiverReceiveNewFieldInfo = typeof(DataStreamReceiver).GetField(nameof(DataStreamReceiver.receiveNew));
                var dataStreamReceiverReceiveNewField = moduleDefinition.ImportReference(dataStreamReceiverReceiveNewFieldInfo);
                processor.Emit(OpCodes.Stsfld, dataStreamReceiverReceiveNewField);
            }

            {
                CreateActionDelegateBitWriterUIntUIntDataReceiver(processor, generatedDataReceiverUpdateMethod!);

                var dataStreamReceiverReceiveUpdateFieldInfo = typeof(DataStreamReceiver).GetField(nameof(DataStreamReceiver.receiveUpdate));
                var dataStreamReceiverReceiveUpdateField = moduleDefinition.ImportReference(dataStreamReceiverReceiveUpdateFieldInfo);
                processor.Emit(OpCodes.Stsfld, dataStreamReceiverReceiveUpdateField);
            }

            {
                CreateActionDelegateUIntUIntDataReceiver(processor, generatedDataReceiverDestroyMethod!);

                var dataStreamReceiverReceiveDestroyFieldInfo = typeof(DataStreamReceiver).GetField(nameof(DataStreamReceiver.receiveDestroy));
                var dataStreamReceiverReceiveDestroyField = moduleDefinition.ImportReference(dataStreamReceiverReceiveDestroyFieldInfo);
                processor.Emit(OpCodes.Stsfld, dataStreamReceiverReceiveDestroyField);
            }

            var ghostComponentTypeIdsFieldInfo = typeof(DataInfo).GetField(nameof(DataInfo.ghostComponentTypeIds));
            var ghostComponentTypeIdsFieldReference = moduleDefinition.ImportReference(ghostComponentTypeIdsFieldInfo);

            processor.Emit(OpCodes.Ret);

            AddGeneratedMethod(initOnLoadMethod);
        }

        MethodDefinition? CheckIfUserDefinedStaticBitReaderMethodExists(TypeReference dataTypeReference)
        {
            foreach (var module in moduleDefinition.Assembly.Modules)
            {
                foreach (var type in module.Types)
                {
                    if (!ScannerHelper.HasAttribute<BitSerializerAttribute>(type))
                    {
                        continue;
                    }

                    foreach (var method in type.Methods)
                    {
                        //method.IsSpecialName
                        if (method.IsStatic && method.IsPublic && method.Name == "Read" && method.Parameters.Count == 2 && method.Parameters[0].ParameterType.FullName == typeof(IBitReader).FullName &&
                            method.Parameters[1].ParameterType.FullName == dataTypeReference.FullName)
                        {
                            return method;
                        }
                    }
                }
            }

            return null;
        }

        MethodDefinition? CheckIfUserDefinedStaticBitWriterMethodExists(TypeReference dataTypeReference)
        {
            foreach (var module in moduleDefinition.Assembly.Modules)
            {
                foreach (var type in module.Types)
                {
                    if (!ScannerHelper.HasAttribute<BitSerializerAttribute>(type))
                    {
                        continue;
                    }

                    foreach (var method in type.Methods)
                    {
                        //method.IsSpecialName
                        if (method.IsStatic && method.IsPublic && method.Name == "Write" && method.Parameters.Count == 2 && method.Parameters[0].ParameterType.FullName == typeof(IBitWriter).FullName &&
                            method.Parameters[1].ParameterType.FullName == dataTypeReference.FullName)
                        {
                            return method;
                        }
                    }
                }
            }

            return null;
        }

        MethodInfo? FindStaticSurgeBitWriterMethod(TypeReference dataTypeReference)
        {
            if (!dataTypeReference.FullName.StartsWith("Piot.Surge.Types."))
            {
                return null;
            }

            var surgeWriteMethod = dataTypeReference.Name switch
            {
                nameof(Position3) => typeof(Position3Writer).GetMethod(nameof(Position3Writer.Write)),
                nameof(Velocity3) => typeof(Velocity3Writer).GetMethod(nameof(Velocity3Writer.Write)),
                nameof(UnitVector2) => typeof(UnitVector2Writer).GetMethod(nameof(UnitVector2Writer.Write)),
                nameof(Axis) => typeof(AxisWriter).GetMethod(nameof(AxisWriter.Write)),
                nameof(Aiming) => typeof(AimingWriter).GetMethod(nameof(AimingWriter.Write)),
                _ => null
            };

            return surgeWriteMethod;
        }

        MethodInfo? FindStaticSurgeBitReaderMethod(TypeReference dataTypeReference)
        {
            if (!dataTypeReference.FullName.StartsWith("Piot.Surge.Types."))
            {
                return null;
            }

            var surgeWriteMethod = dataTypeReference.Name switch
            {
                nameof(Position3) => typeof(Position3Reader).GetMethod(nameof(Position3Reader.Read)),
                nameof(Velocity3) => typeof(Velocity3Reader).GetMethod(nameof(Velocity3Reader.Read)),
                nameof(UnitVector2) => typeof(UnitVector2Reader).GetMethod(nameof(UnitVector2Reader.Read)),
                nameof(Axis) => typeof(AxisReader).GetMethod(nameof(AxisReader.Read)),
                nameof(Aiming) => typeof(AimingReader).GetMethod(nameof(AimingReader.Read)),
                _ => null
            };

            return surgeWriteMethod;
        }

        MethodReference FindStaticBitWriterMethod(TypeReference dataTypeReference)
        {
            var internalWriteMethod = FindStaticSurgeBitWriterMethod(dataTypeReference);
            if (internalWriteMethod is not null)
            {
                return moduleDefinition.ImportReference(internalWriteMethod);
            }

            var userWriteMethod = CheckIfUserDefinedStaticBitWriterMethodExists(dataTypeReference);
            if (userWriteMethod is null)
            {
                throw new("couldn't find user provided bit writer");
            }

            return userWriteMethod;
        }

        MethodReference FindStaticBitReaderMethod(TypeReference dataTypeReference)
        {
            var internalWriteMethod = FindStaticSurgeBitReaderMethod(dataTypeReference);
            if (internalWriteMethod is not null)
            {
                return moduleDefinition.ImportReference(internalWriteMethod);
            }

            var userWriteMethod = CheckIfUserDefinedStaticBitReaderMethodExists(dataTypeReference);
            if (userWriteMethod is null)
            {
                throw new("couldn't find user provided bit writer");
            }

            return userWriteMethod;
        }

        void EmitDataTypeWriter(ILProcessor processor, TypeReference fieldType, ILog log)
        {
            if (IsAllowedPrimitive(fieldType))
            {
                if (IsBool(fieldType))
                {
                    var trueLabel = processor.Create(OpCodes.Ldc_I4_1);
                    processor.Emit(OpCodes.Brtrue, trueLabel);
                    processor.Emit(OpCodes.Ldc_I4_0); // False value
                    var endLabel = processor.Create(OpCodes.Nop);
                    processor.Emit(OpCodes.Br_S, endLabel);
                    processor.Append(trueLabel);
                    processor.Append(endLabel);
                }

                EmitWriteBits(processor, fieldType, log);
            }
            else
            {
                var foundMethod = FindStaticBitWriterMethod(fieldType);
                if (foundMethod is null)
                {
                    throw new($"couldn't find a bit serializer for {fieldType.FullName}");
                }

                processor.Emit(OpCodes.Call, foundMethod);
            }
        }

        void EmitDataTypeReader(ILProcessor processor, FieldDefinition fieldReference, bool useLocal, ILog log)
        {
            var fieldType = fieldReference.FieldType;

            if (IsAllowedPrimitive(fieldType))
            {
                if (useLocal)
                {
                    // Load data type instance (used later for Stfld)
                    processor.Emit(OpCodes.Ldloca_S, (byte)0);

                    // IBitReader
                    processor.Emit(OpCodes.Ldarg_0);
                }
                else
                {
                    // Load data type instance reference (prepare for later Stfld)
                    processor.Emit(OpCodes.Ldarg_1);

                    // IBitReader
                    processor.Emit(OpCodes.Ldarg_0);
                }

                EmitReadBits(processor, fieldType, log);
                // Cast To type
                EmitConversionFromUInt32DependingOnType(processor, fieldType, log);
                processor.Emit(OpCodes.Stfld, fieldReference);
            }
            else
            {
                var foundMethod = FindStaticBitReaderMethod(fieldType);
                if (foundMethod is null)
                {
                    throw new($"couldnt find a bit serializer for {fieldType.FullName}");
                }

                // IBitReader
                processor.Emit(OpCodes.Ldarg_0);
                if (useLocal)
                {
                    // Load data type instance reference
                    processor.Emit(OpCodes.Ldloca_S, (byte)0);
                }
                else
                {
                    // Load data type instance reference
                    processor.Emit(OpCodes.Ldarg_1);
                }

                // Get reference of the field of the data type instance
                processor.Emit(OpCodes.Ldflda, fieldReference);

                // Call the data specific reader with a `ref`
                processor.Emit(OpCodes.Call, foundMethod);
            }
        }

        public MethodDefinition CreateSerializeMaskRefMethod(DataTypeInfo dataTypeInfo, ILog log)
        {
            var serializeMaskMethod = CreatePublicStaticMethodForTypeVoidReturn("SerializeMaskRef", dataTypeInfo.resolvedDataStructType);

            serializeMaskMethod.Parameters.Add(new("writer", ParameterAttributes.None, bitWriterInterfaceReference));
            var dataReferenceParameter = new ParameterDefinition("data", ParameterAttributes.None, dataTypeInfo.resolvedDataStructType);
            serializeMaskMethod.Parameters.Add(dataReferenceParameter);

            var uint32Reference = moduleDefinition.ImportReference(typeof(uint));
            var fieldMaskParameter = new ParameterDefinition("fieldMask", ParameterAttributes.None, uint32Reference);
            serializeMaskMethod.Parameters.Add(fieldMaskParameter);

            serializeMaskMethod.Body.InitLocals = true;

            var processor = serializeMaskMethod.Body.GetILProcessor();

            var maskBitCount = dataTypeInfo.resolvedDataStructType.Fields.Count;

            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Ldarg_2);
            processor.Emit(OpCodes.Ldc_I4, maskBitCount);
            processor.Emit(OpCodes.Callvirt, writeBitsMethod);

            var index = 0;
            Instruction? skipLabel = null;
            foreach (var field in dataTypeInfo.resolvedDataStructType.Fields)
            {
                if (skipLabel is not null)
                {
                    processor.Append(skipLabel);
                }

                // Load fieldMask
                processor.Emit(OpCodes.Ldarg_2);

                var valueToCheck = 1 << index;
                processor.Emit(OpCodes.Ldc_I4, valueToCheck);
                processor.Emit(OpCodes.And);
                skipLabel = processor.Create(OpCodes.Nop);
                processor.Emit(OpCodes.Brfalse, skipLabel);


                // IBitWriter
                processor.Emit(OpCodes.Ldarg_0);
                // Data 
                processor.Emit(OpCodes.Ldarg_1);

                // Load field
                var fieldReference = moduleDefinition.ImportReference(field);
                processor.Emit(OpCodes.Ldfld, fieldReference);

                var fieldType = field.FieldType;

                EmitDataTypeWriter(processor, fieldType, log);

                index++;
            }

            if (skipLabel is not null)
            {
                processor.Append(skipLabel);
            }

            processor.Emit(OpCodes.Ret);

            AddGeneratedMethod(serializeMaskMethod);

            currentData!.writeMaskMethodReference = serializeMaskMethod;

            return serializeMaskMethod;
        }

        MethodDefinition CreateSerializeFullMethod(DataTypeInfo dataTypeInfo, ILog log)
        {
            var serializeFullMethod = CreatePublicStaticMethodForTypeVoidReturn("SerializeFull", dataTypeInfo.resolvedDataStructType);

            serializeFullMethod.Parameters.Add(new("writer", ParameterAttributes.None, bitWriterInterfaceReference));
            var dataReferenceParameter = new ParameterDefinition("data", ParameterAttributes.None, dataTypeInfo.resolvedDataStructType);
            serializeFullMethod.Parameters.Add(dataReferenceParameter);

            serializeFullMethod.Body.InitLocals = true;

            var processor = serializeFullMethod.Body.GetILProcessor();

            var index = 0;
            foreach (var field in dataTypeInfo.resolvedDataStructType.Fields)
            {
                // IBitReader
                processor.Emit(OpCodes.Ldarg_0);
                // Data 
                processor.Emit(OpCodes.Ldarg_1);

                // Load field
                var fieldReference = moduleDefinition.ImportReference(field);
                processor.Emit(OpCodes.Ldfld, fieldReference);

                var fieldType = field.FieldType;


                EmitDataTypeWriter(processor, fieldType, log);


                index++;
            }

            processor.Emit(OpCodes.Ret);

            AddGeneratedMethod(serializeFullMethod);

            currentData!.writeFullMethodReference = serializeFullMethod;

            return serializeFullMethod;
        }

        void EmitCompareStructFields(ILProcessor processor, FieldReference getRootFieldReference, Instruction modifyMaskLabel, ILog log)
        {
            var rootFieldTypeReference = moduleDefinition.ImportReference(getRootFieldReference.FieldType);
            var resolvedFieldType = rootFieldTypeReference.Resolve();
            var writtenFirst = false;
            foreach (var fieldDefinition in resolvedFieldType.Fields)
            {
                if (!fieldDefinition.IsPublic)
                {
                    log.Warn("Should not have private fields in {Type} {FieldType} {Name}", resolvedFieldType.FullName, fieldDefinition.FieldType.FullName, fieldDefinition.FullName);
                    continue;
                }

                if (fieldDefinition.IsStatic)
                {
                    log.Warn("Should not have constants or static fields in {Type} {FieldType} {Name}", resolvedFieldType.FullName, fieldDefinition.FieldType.FullName, fieldDefinition.FullName);
                    continue;
                }

                if (!IsAllowedPrimitive(fieldDefinition.FieldType))
                {
                    throw new($"Illegal type layers of structs are not allowed! {resolvedFieldType.FullName} {fieldDefinition.FieldType.FullName} {fieldDefinition.Name} {fieldDefinition.IsDefinition} {fieldDefinition.IsSpecialName}");
                }

                var fieldReference = moduleDefinition.ImportReference(fieldDefinition);

                if (writtenFirst)
                {
                    processor.Emit(OpCodes.Bne_Un_S, modifyMaskLabel);
                    // DataType a;
                    processor.Emit(OpCodes.Ldarg_0);
                }

                writtenFirst = true;

                processor.Emit(OpCodes.Ldfld, getRootFieldReference);
                processor.Emit(OpCodes.Ldfld, fieldReference);

                // DataType b;
                processor.Emit(OpCodes.Ldarg_1);
                processor.Emit(OpCodes.Ldfld, getRootFieldReference);
                processor.Emit(OpCodes.Ldfld, fieldReference);

            }
        }

        public MethodDefinition CreateDifferMethod(DataTypeInfo dataTypeInfo, ILog log)
        {
            var uint32Reference = moduleDefinition.ImportReference(typeof(uint));
            var generatedMethodName = GenerateSafeFullNameWithPrefix("Differ", dataTypeInfo.resolvedDataStructType);
            var diffMethod = new MethodDefinition(generatedMethodName,
                MethodAttributes.Public |
                MethodAttributes.Static |
                MethodAttributes.HideBySig,
                uint32Reference);

            var firstDataReferenceParameter = new ParameterDefinition("a", ParameterAttributes.In, dataTypeInfo.resolvedDataStructType);
            diffMethod.Parameters.Add(firstDataReferenceParameter);
            var secondDataReferenceParameter = new ParameterDefinition("b", ParameterAttributes.In, dataTypeInfo.resolvedDataStructType);
            diffMethod.Parameters.Add(secondDataReferenceParameter);

            var maskVariable = new VariableDefinition(uint32Reference);
            diffMethod.Body.Variables.Add(maskVariable);
            diffMethod.Body.InitLocals = true;

            var processor = diffMethod.Body.GetILProcessor();

            // generate: uint mask = 0;
            processor.Emit(OpCodes.Ldc_I4_0);
            processor.Emit(OpCodes.Stloc_0);

            var index = 0;

            var skipLabel = processor.Create(OpCodes.Ldarg_0);
            foreach (var field in dataTypeInfo.resolvedDataStructType.Fields)
            {
                processor.Append(skipLabel);


                var modifyMaskLabel = processor.Create(OpCodes.Ldloc_0);
                if (IsAllowedPrimitive(field.FieldType))
                {
                    processor.Emit(OpCodes.Ldfld, field);

                    processor.Emit(OpCodes.Ldarg_1);
                    processor.Emit(OpCodes.Ldfld, field);

                }
                else
                {
                    EmitCompareStructFields(processor, field, modifyMaskLabel, log);
                }

                var isLastField = index == dataTypeInfo.resolvedDataStructType.Fields.Count - 1;
                skipLabel = processor.Create(isLastField ? OpCodes.Ldloc_0 : OpCodes.Ldarg_0);

                processor.Emit(OpCodes.Beq_S, skipLabel);

                var maskValue = 1 << index;
                processor.Append(modifyMaskLabel);
                processor.Emit(OpCodes.Ldc_I4, maskValue);
                processor.Emit(OpCodes.Or);
                processor.Emit(OpCodes.Stloc_0);

                index++;
            }

            processor.Append(skipLabel);
            processor.Emit(OpCodes.Ret);

            currentData!.diffMethodReference = diffMethod;

            AddGeneratedMethod(diffMethod);

            return diffMethod;
        }

        public MethodDefinition CreateCommonDataReceiveMethod(string generatedMethodName)
        {
            var uint32Reference = moduleDefinition.ImportReference(typeof(uint));
            var voidReturnReference = moduleDefinition.ImportReference(typeof(void));
            var dataReceiveNewMethod = new MethodDefinition(generatedMethodName,
                MethodAttributes.Public |
                MethodAttributes.Static |
                MethodAttributes.HideBySig,
                voidReturnReference);

            var bitReaderParameter = new ParameterDefinition("reader", ParameterAttributes.None, bitReaderInterfaceReference);
            dataReceiveNewMethod.Parameters.Add(bitReaderParameter);
            var entityIdParameter = new ParameterDefinition("entityId", ParameterAttributes.None, uint32Reference);
            dataReceiveNewMethod.Parameters.Add(entityIdParameter);
            var dataTypeIdParameter = new ParameterDefinition("dataTypeId", ParameterAttributes.None, uint32Reference);
            dataReceiveNewMethod.Parameters.Add(dataTypeIdParameter);
            var dataReceiverInterfaceParameter = new ParameterDefinition("receiver", ParameterAttributes.None, dataReceiverInterfaceReference);
            dataReceiveNewMethod.Parameters.Add(dataReceiverInterfaceParameter);

            dataReceiveNewMethod.Body.InitLocals = false;

            return dataReceiveNewMethod;
        }

        public MethodDefinition CreateDataReceiveNew(IEnumerable<DataTypeInfo> dataTypeInfos, ILog log)
        {
            var dataReceiveNewMethod = CreateCommonDataReceiveMethod("DataReceiveNew");

            var processor = dataReceiveNewMethod.Body.GetILProcessor();

            // load dataTypeId
            processor.Emit(OpCodes.Ldarg_2);

            var (labels, defaultLabel) = CreateEnumAndReturnLabels(dataTypeInfos, processor, OpCodes.Ldarg_3, OpCodes.Ret);

            processor.Emit(OpCodes.Ret);

            var index = 0;

            foreach (var dataTypeInfo in dataTypeInfos)
            {
                processor.Append(labels[index]);

                // EntityId
                processor.Emit(OpCodes.Ldarg_1);
                // reader
                processor.Emit(OpCodes.Ldarg_0);


                var specializedDataStreamReaderCreateAndReadReference = SpecializeMethod(genericDataStreamReaderCreateAndReadReference, dataTypeInfo.dataStructTypeReference);
                processor.Emit(OpCodes.Call, specializedDataStreamReaderCreateAndReadReference);


                var specializedDataReceiverCreateNewMethodReference = SpecializeMethod(dataReceiverCreateNewMethodReference, dataTypeInfo.dataStructTypeReference);
                processor.Emit(OpCodes.Callvirt, specializedDataReceiverCreateNewMethodReference);


                processor.Emit(OpCodes.Ret);
                index++;
            }

            processor.Append(defaultLabel);

            AddGeneratedMethod(dataReceiveNewMethod);

            generatedDataReceiverNewMethod = dataReceiveNewMethod;

            return dataReceiveNewMethod;
        }

        public MethodDefinition CreateDataReceiveUpdate(IEnumerable<DataTypeInfo> dataTypeInfos, ILog log)
        {
            var dataReceiveNewMethod = CreateCommonDataReceiveMethod("DataReceiveUpdate");

            var processor = dataReceiveNewMethod.Body.GetILProcessor();

            foreach (var dataTypeInfo in dataTypeInfos)
            {
                var specificLocal = new VariableDefinition(dataTypeInfo.dataStructTypeReference);
                processor.Body.Variables.Add(specificLocal);
            }

            // load dataTypeId
            processor.Emit(OpCodes.Ldarg_2);

            var (labels, defaultLabel) = CreateEnumAndReturnLabels(dataTypeInfos, processor, OpCodes.Ldarg_3, OpCodes.Ret);

            processor.Emit(OpCodes.Ret);

            var index = 0;

            foreach (var dataTypeInfo in dataTypeInfos)
            {
                processor.Append(labels[index]); // LdArg_3 // IDataReceiver

                // To get an IDataReceiver for the IDataReceiver:Update call below
                // IDataReceiver
                //processor.Emit(OpCodes.Dup);

                // EntityId
                processor.Emit(OpCodes.Ldarg_1);

                var specializedDataReceiverGetMethodReference = SpecializeMethod(dataReceiverGetMethodReference, dataTypeInfo.dataStructTypeReference);
                processor.Emit(OpCodes.Callvirt, specializedDataReceiverGetMethodReference);

                // The stack now has the DataType struct on top of the stack

                // Save the Data type struct it in a local variable and take the address of that
                processor.Emit(OpCodes.Stloc_S, (byte)index);


                // IDataReceiver (prepare for callvirt below)
                processor.Emit(OpCodes.Ldarg_3);

                // reader
                processor.Emit(OpCodes.Ldarg_0);

                // take the address of the local variable and push on stack
                processor.Emit(OpCodes.Ldloca_S, (byte)index);

                var specializedDataStreamReaderReadMaskReference = SpecializeMethod(genericDataStreamReaderReadMaskMethodReference, dataTypeInfo.dataStructTypeReference);
                processor.Emit(OpCodes.Call, specializedDataStreamReaderReadMaskReference);

                // Stack has the previously pushed IDataReceiver and the mask

#if false
                  processor.Emit(OpCodes.Pop);
                  processor.Emit(OpCodes.Pop);
#else

                // EntityId
                processor.Emit(OpCodes.Ldarg_1);

                // take the address of the local variable and push on stack
                processor.Emit(OpCodes.Ldloc_S, (byte)index);

                var specializedDataReceiverUpdateMethodReference = SpecializeMethod(dataReceiverUpdateMethodReference, dataTypeInfo.dataStructTypeReference);
                processor.Emit(OpCodes.Callvirt, specializedDataReceiverUpdateMethodReference);
#endif
                processor.Emit(OpCodes.Ret);
                index++;
            }

            processor.Append(defaultLabel);

            AddGeneratedMethod(dataReceiveNewMethod);

            generatedDataReceiverUpdateMethod = dataReceiveNewMethod;

            return dataReceiveNewMethod;
        }

        (Instruction[], Instruction) CreateEnumAndReturnLabels(IEnumerable<DataTypeInfo> dataTypeInfos, ILProcessor processor, OpCode startLabelOpCode, OpCode startDefaultOpCode)
        {
            var labels = new Instruction[dataTypeInfos.Count() + 1];
            var labelIndex = 0;

            var defaultLabel = processor.Create(OpCodes.Ret);
            labels[labelIndex++] = defaultLabel;

            foreach (var dataTypeInfo in dataTypeInfos)
            {
                // Load `receiver`
                labels[labelIndex] = processor.Create(startLabelOpCode);
                labelIndex++;
            }

            processor.Emit(OpCodes.Switch, labels);

            return (labels.Skip(1).ToArray(), defaultLabel);
        }

        public MethodDefinition CreateDataReceiveDestroy(IEnumerable<DataTypeInfo> dataTypeInfos, ILog log)
        {
            var uint32Reference = moduleDefinition.ImportReference(typeof(uint));
            var voidReturnReference = moduleDefinition.ImportReference(typeof(void));
            var dataReceiveDestroyMethod = new MethodDefinition("DataReceiveDestroy",
                MethodAttributes.Public |
                MethodAttributes.Static |
                MethodAttributes.HideBySig,
                voidReturnReference);

            var entityIdParameter = new ParameterDefinition("entityId", ParameterAttributes.None, uint32Reference);
            dataReceiveDestroyMethod.Parameters.Add(entityIdParameter);
            var dataTypeIdParameter = new ParameterDefinition("dataTypeId", ParameterAttributes.None, uint32Reference);
            dataReceiveDestroyMethod.Parameters.Add(dataTypeIdParameter);
            var dataReceiverInterfaceParameter = new ParameterDefinition("receiver", ParameterAttributes.None, dataReceiverInterfaceReference);
            dataReceiveDestroyMethod.Parameters.Add(dataReceiverInterfaceParameter);

            dataReceiveDestroyMethod.Body.InitLocals = false;

            var processor = dataReceiveDestroyMethod.Body.GetILProcessor();

            // load dataTypeId
            processor.Emit(OpCodes.Ldarg_1);

            var (labels, defaultLabel) = CreateEnumAndReturnLabels(dataTypeInfos, processor, OpCodes.Ldarg_2, OpCodes.Ret);

            processor.Emit(OpCodes.Ret);

            var index = 0;

            foreach (var dataTypeInfo in dataTypeInfos)
            {
                processor.Append(labels[index]); // LdArg_2 // IDataReceiver

                // EntityId
                processor.Emit(OpCodes.Ldarg_0);

                var specializedDataReceiverGetMethodReference = SpecializeMethod(dataReceiverDestroyMethodReference, dataTypeInfo.dataStructTypeReference);
                processor.Emit(OpCodes.Callvirt, specializedDataReceiverGetMethodReference);

                processor.Emit(OpCodes.Ret);
                index++;
            }

            processor.Append(defaultLabel);

            AddGeneratedMethod(dataReceiveDestroyMethod);

            generatedDataReceiverDestroyMethod = dataReceiveDestroyMethod;

            return dataReceiveDestroyMethod;
        }

        public struct DataTypeInfo
        {
            public TypeDefinition resolvedDataStructType;
            public TypeReference dataStructTypeReference;
            public ByReferenceType resolvedDataStructTypeByReference;
        }


        public class DataClassMeta
        {
            public MethodReference? diffMethodReference;
            public MethodReference? readFullMethodReference;
            public MethodReference? readMaskMethodReference;
            public ByReferenceType? typeByReference;
            public TypeReference? typeReference;
            public uint uniqueId;
            public MethodReference? writeFullMethodReference;
            public MethodReference? writeMaskMethodReference;
        }

        struct AnyStruct
        {

        }
    }
}