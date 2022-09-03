/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

// ReSharper disable MemberCanBePrivate.Global

namespace Piot.Surge.Generator
{
    public static class SourceGenerator
    {
        private static string Suffix(string a, string b)
        {
            return a + b;
        }

        public static string FullName(Type t)
        {
            return t.FullName!.Replace('+', '.');
        }

        public static string ShortName(Type t)
        {
            return t.Name;
        }

        public static string FullName(MethodInfo methodInfo)
        {
            return methodInfo.DeclaringType?.FullName + "." + methodInfo.Name;
        }

        public static string ActionsName(Type t)
        {
            return Suffix(t.Name, "Actions");
        }

        public static string TitleCase(string input)
        {
            return string.Concat(input[0].ToString().ToUpper(), input.Substring(1, input.Length - 1));
        }

        public static string EntityExternal(LogicInfo info)
        {
            return Suffix(info.Type.Name, "Entity");
        }

        public static string EntityGeneratedInternal(LogicInfo info)
        {
            return Suffix(info.Type.Name, "EntityInternal");
        }

        public static string EntityExternalFullName(LogicInfo info)
        {
            return Suffix(FullName(info.Type), "Entity");
        }


        public static void DoNotEditComment(StringBuilder sb)
        {
            sb.Append(@"// Code generated by Surge generator. DO NOT EDIT.
// <auto-generated /> This file has been auto generated.
#nullable enable
");
        }

        public static void AddEndDeclaration(StringBuilder sb)
        {
            sb.Append(@"
}
");
        }

        public static void AddEntityCreation(StringBuilder sb, IEnumerable<LogicInfo> logicInfos)
        {
            AddClassDeclaration(sb, "GeneratedEntityGhostCreator", "IEntityGhostCreator");
            sb.Append(@" public IEntity CreateGhostEntity(ArchetypeId archetypeId, EntityId entityId)
            {
                IGeneratedEntity generatedEntity = archetypeId.id switch
                {
");
            foreach (var logicInfo in logicInfos)
            {
                sb.Append(
                        $"        ArchetypeConstants.{logicInfo.Type.Name} => new {EntityGeneratedInternal(logicInfo)}(),")
                    .Append(@"
");
            }

            sb.Append(@"            _ => throw new Exception($""unknown entity to create {archetypeId}""),

                };
                
                return new Entity(entityId, generatedEntity);
            }
        }
");
        }


        public static void AddEngineSpawner(StringBuilder sb, IEnumerable<LogicInfo> infos)
        {
            AddClassDeclaration(sb, "GeneratedEngineSpawner");
            sb.Append(@"
    private readonly IAuthoritativeEntityContainer container;
    private readonly INotifyWorld notifyWorld;

    public GeneratedEngineSpawner(IAuthoritativeEntityContainer container, INotifyWorld notifyWorld)
    {
        this.container = container;
        this.notifyWorld = notifyWorld;
    }
");
            foreach (var info in infos)
            {
                var logicName = FullName(info.Type);
                sb.Append(@$"
    public (IEntity, {EntityGeneratedInternal(info)}) Spawn{ShortName(info.Type)}({logicName} logic)
    {{ 
        var internalEntity = new {EntityGeneratedInternal(info)}
        {{
            Current = logic
        }};
        notifyWorld.NotifyCreation(internalEntity);
        return (container.SpawnEntity(internalEntity), internalEntity);
     }}
");
            }

            AddEndDeclaration(sb);
        }

        public static void AddEngineWorld(StringBuilder sb, IEnumerable<LogicInfo> infos)
        {
            AddClassDeclaration(sb, "GeneratedEngineWorld", "INotifyWorld");

            foreach (var info in infos)
            {
                sb.Append($"public Action<{EntityExternal(info)}>? OnSpawn{info.Type.Name};").Append(@"
");
            }


            sb.Append(@"
        void INotifyWorld.NotifyCreation(IGeneratedEntity entity)
        {
            switch (entity)
            {
");

            foreach (var info in infos)
            {
                sb.Append($"case {EntityGeneratedInternal(info)} internalEntity:").Append(@"
").Append($"OnSpawn{info.Type.Name}?.Invoke(internalEntity.OutFacing);").Append(@"
    break;
");
            }

            sb.Append(@"
                default:
                    throw new Exception(""Internal error"");
            }
        }
                ");

            AddEndDeclaration(sb);
        }


        /*
         * public class GeneratedEntityWorld
{
    public Action<AvatarLogicEntity>? OnSpawnAvatarLogic;
    public Action<FireballLogicEntity>? OnSpawnFireballLogic;
        public void NotifyCreation(IEntity entity)
        {
            switch (entity)
            {
case AvatarLogicEntityInternal internalEntity:
OnSpawnAvatarLogic?.Invoke(internalEntity.OutFacing);
    break;
case FireballLogicEntityInternal internalEntity:
OnSpawnFireballLogic?.Invoke(internalEntity.OutFacing);
    break;

                default:
                    throw new Exception("Internal error");
            }
        }
}
         */


        public static void AddChangeDelegate(StringBuilder sb, LogicInfo logicInfo, LogicFieldInfo fieldInfo)
        {
            var titledField = TitleCase(fieldInfo.FieldInfo.Name);
            sb.Append(@$"    public Action? On{titledField}Changed;

");
        }


        public static void AddSection(StringBuilder sb, string name)
        {
            sb.Append($"// --------------- {name} ---------------").Append(@"
");
        }

        public static void AddClassDeclaration(StringBuilder sb, string className)
        {
            sb.Append($"public class {className}").Append(@"
{
");
        }


        public static void AddStaticClassDeclaration(StringBuilder sb, string className)
        {
            sb.Append($"public static class {className}").Append(@"
{
");
        }

        public static void AddClassDeclaration(StringBuilder sb, string className, string inheritFrom)
        {
            sb.Append($"public class {className} : {inheritFrom}").Append(@"
    {
");
        }


        public static void AddActionStructs(StringBuilder sb, IEnumerable<CommandInfo> commandInfos)
        {
            AddSection(sb, "Internal Action Structs");

            foreach (var commandInfo in commandInfos)
            {
                var actionName = commandInfo.MethodInfo.Name;
                sb.Append($"public struct {actionName} : IAction").Append(@"
{
");
                foreach (var parameter in commandInfo.CommandParameters)
                {
                    sb.Append($"    public {parameter.type} {parameter.name}").Append(@";
");
                }

                sb.Append(@"
}
");
            }
        }

        public static void AddActionImplementation(StringBuilder sb, LogicInfo info)
        {
            AddSection(sb, "Internal Action Implementation");
            var actionsImplementationName = Suffix(info.Type.Name, "Actions");
            var actionInterface = FullName(info.CommandsInterface);

            sb.Append($"public class {actionsImplementationName} : {actionInterface}").Append(@"
{
    private readonly IActionsContainer actionsContainer;

").Append($"    public {actionsImplementationName}(IActionsContainer actionsContainer)").Append(@"
    {
        this.actionsContainer = actionsContainer;
    }
");
            foreach (var command in info.CommandInfos)
            {
                var stringParts = command.CommandParameters
                    .Select(commandInfo => commandInfo.type + " " + commandInfo.name).ToList();
                var parameters = string.Join(", ", stringParts);
                sb.Append($"    public void {command.MethodInfo.Name}({parameters})").Append(@"
    {
        actionsContainer.Add(").Append($"new {command.MethodInfo.Name}()").Append("{");

                var setFromParametersParts = command.CommandParameters
                    .Select(commandInfo => commandInfo.name + " = " + commandInfo.name).ToList();
                var setFromParametersString = string.Join(", ", setFromParametersParts);

                sb.Append(setFromParametersString).Append(@"});
    }");
            }

            sb.Append(@"
}");
        }

        public static void AddActionDelegate(StringBuilder sb, CommandInfo commandInfo)
        {
            var titledField = commandInfo.MethodInfo.Name;
            var delegateName = Suffix(titledField, "Delegate");
            var actionName = commandInfo.MethodInfo.Name;

            var stringParts = commandInfo.CommandParameters
                .Select(commandInfo => commandInfo.type + " " + commandInfo.name).ToList();
            var parameters = string.Join(", ", stringParts);

            sb.Append(@$"    public delegate void {delegateName}({parameters});
    public {delegateName}? Do{actionName};
    public {delegateName}? UnDo{actionName};

");
        }

        public static void AddActionDelegates(StringBuilder sb, IEnumerable<CommandInfo> commandInfos)
        {
            foreach (var commandInfo in commandInfos)
            {
                AddActionDelegate(sb, commandInfo);
            }
        }

        public static string MaskName(LogicFieldInfo fieldInfo)
        {
            return Suffix(TitleCase(fieldInfo.FieldInfo.Name), "Mask");
        }

        public static void AddChangeMaskConstant(StringBuilder sb, LogicFieldInfo fieldInfo)
        {
            sb.Append(
                @$"    public const ulong {MaskName(fieldInfo)} = 0x{fieldInfo.Mask:x08};
");
        }

        public static void AddChangeMaskConstants(StringBuilder sb, IEnumerable<LogicFieldInfo> fieldInfos)
        {
            foreach (var fieldInfo in fieldInfos)
            {
                AddChangeMaskConstant(sb, fieldInfo);
            }

            sb.Append(@"
");
        }

        public static string PrimitiveDeSerializer(string primitiveTypeName)
        {
            return $"reader.Read{primitiveTypeName}()";
        }

        public static string PrimitiveSerializer(string primitiveTypeName, string variableName)
        {
            return $"writer.Write{primitiveTypeName}({variableName})";
        }


        public static string DeSerializeMethodForValueTypes(Type type)
        {
            return $"{type.Name}Reader.Read(reader)";
        }

        public static string SerializeMethodForValueTypes(Type type, string variableName)
        {
            return $"{type.Name}Writer.Write({variableName}, writer)";
        }


        public static string DeSerializeMethod(Type type)
        {
            if (type == typeof(bool))
            {
                return "reader.ReadUInt8() != 0";
            }

            if (type == typeof(ushort))
            {
                return PrimitiveDeSerializer("UInt16");
            }

            if (type == typeof(uint))
            {
                return PrimitiveDeSerializer("UInt32");
            }

            if (type == typeof(ulong))
            {
                return PrimitiveDeSerializer("UInt64");
            }

            return DeSerializeMethodForValueTypes(type);
        }


        public static string SerializeMethod(Type type, string variableName)
        {
            if (type == typeof(bool))
            {
                return $"writer.WriteUInt8({variableName} ? (byte)1 : (byte)0)";
            }

            if (type == typeof(ushort))
            {
                return PrimitiveSerializer("UInt16", variableName);
            }

            if (type == typeof(uint))
            {
                return PrimitiveSerializer("UInt32", variableName);
            }

            if (type == typeof(ulong))
            {
                return PrimitiveSerializer("UInt64", variableName);
            }

            return SerializeMethodForValueTypes(type, variableName);
        }

        public static string PrimitiveBitSerializer(uint bitCount, string variableName)
        {
            return $"writer.WriteBits({variableName}, {bitCount})";
        }

        public static string PrimitiveBitDeSerializer(Type type)
        {
            int bitCount;

            if (type == typeof(byte) || type == typeof(sbyte))
            {
                bitCount = 8;
            }
            else if (type == typeof(ushort) || type == typeof(short))
            {
                bitCount = 16;
            }
            else if (type == typeof(uint) || type == typeof(int))
            {
                bitCount = 32;
            }
            else if (type == typeof(ulong) || type == typeof(long))
            {
                bitCount = 64;
            }
            else
            {
                throw new Exception($"unknown type {type.Name}");
            }

            return $"({type.Name})reader.ReadBits({bitCount})";
        }

        public static string BitSerializeMethod(Type type, string variableName)
        {
            if (type == typeof(bool))
            {
                return $"writer.WriteBits({variableName} ? 1U : 0U, 1)";
            }

            if (type == typeof(ushort))
            {
                return PrimitiveBitSerializer(16, variableName);
            }

            if (type == typeof(uint))
            {
                return PrimitiveBitSerializer(32, variableName);
            }

            if (type == typeof(ulong))
            {
                return PrimitiveBitSerializer(64, variableName);
            }

            return SerializeMethodForValueTypes(type, variableName);
        }

        public static string BitDeSerializeMethod(Type type)
        {
            if (type == typeof(bool))
            {
                return "reader.ReadBits(1) != 0";
            }

            if (type == typeof(ushort))
            {
                return PrimitiveBitDeSerializer(type);
            }

            if (type == typeof(uint))
            {
                return PrimitiveBitDeSerializer(type);
            }

            if (type == typeof(ulong))
            {
                return PrimitiveBitDeSerializer(type);
            }

            return DeSerializeMethodForValueTypes(type);
        }


        public static void AddChangeDelegates(StringBuilder sb, LogicInfo logicInfo,
            IEnumerable<LogicFieldInfo> fieldInfos)
        {
            foreach (var fieldInfo in fieldInfos)
            {
                AddChangeDelegate(sb, logicInfo, fieldInfo);
            }
        }

        public static void AddDeserialize(StringBuilder sb, IEnumerable<LogicFieldInfo> fieldInfos)
        {
            sb.Append(@"    public ulong Deserialize(IOctetReader reader)
    {
");
            var (typeForSerializeFlagString, _) = SmallestPrimitiveForBitCount(fieldInfos.Count());
            var castString = typeForSerializeFlagString == "UInt64" ? "" : "(ulong)";
            sb.Append($@"    var serializeFlags = {castString} reader.Read{typeForSerializeFlagString}();
");
            foreach (var fieldInfo in fieldInfos)
            {
                var fieldName = fieldInfo.FieldInfo.Name;
                sb.Append(
                    $@"        if ((serializeFlags & {MaskName(fieldInfo)}) != 0) current.{fieldName} = {DeSerializeMethod(fieldInfo.FieldInfo.FieldType)};
");
            }

            sb.Append(@"   
            return serializeFlags;
 }

");
        }

        public static void AddFieldMaskBitDeserialize(StringBuilder sb, IEnumerable<LogicFieldInfo> fieldInfos)
        {
            sb.Append(@"    public ulong Deserialize(IBitReader reader)
    {
");

            var fieldCount = fieldInfos.Count();
            var firstCount = fieldCount > 32 ? 32 : fieldCount;
            var secondCount = fieldCount > 32 ? fieldCount - 32 : 0;
            sb.Append(@$"       var serializeFlags = (ulong)reader.ReadBits({firstCount});
");

            if (secondCount > 0)
            {
                sb.Append(@$"       serializeFlags |= (ulong)reader.ReadBits({secondCount}) << 32;
");
            }


            foreach (var fieldInfo in fieldInfos)
            {
                var fieldName = fieldInfo.FieldInfo.Name;
                sb.Append(
                    $@"        if ((serializeFlags & {MaskName(fieldInfo)}) != 0) current.{fieldName} = {BitDeSerializeMethod(fieldInfo.FieldInfo.FieldType)};
");
            }

            sb.Append(@"
                return serializeFlags;   

 }

");
        }

        public static void AddDeserializeAll(StringBuilder sb, IEnumerable<LogicFieldInfo> fieldInfos)
        {
            sb.Append(@"    public void DeserializeAll(IOctetReader reader)
    {
");

            foreach (var fieldInfo in fieldInfos)
            {
                var fieldName = fieldInfo.FieldInfo.Name;
                sb.Append(
                    $@"        current.{fieldName} = {DeSerializeMethod(fieldInfo.FieldInfo.FieldType)};
");
            }

            sb.Append(@"    }

");
        }

        public static void AddBitDeserializeAll(StringBuilder sb, IEnumerable<LogicFieldInfo> fieldInfos)
        {
            sb.Append(@"    public void DeserializeAll(IBitReader reader)
    {
");

            foreach (var fieldInfo in fieldInfos)
            {
                var fieldName = fieldInfo.FieldInfo.Name;
                sb.Append(
                    $@"        current.{fieldName} = {BitDeSerializeMethod(fieldInfo.FieldInfo.FieldType)};
");
            }

            sb.Append(@"    }

");
        }

        public static void AddTick(StringBuilder sb, LogicInfo info)
        {
            var actionsImplementationName = ActionsName(info.Type);
            sb.Append(@"

    public void Tick()
    {
        var actions = ").Append($"new {actionsImplementationName}(actionsContainer);").Append(@"
        current.Tick(actions);
    }

");
        }

        public static (string, string) SmallestPrimitiveForBitCount(int bitCount)
        {
            return bitCount switch
            {
                > 1 and <= 8 => ("UInt8", "byte"),
                <= 16 => ("UInt16", "ushort"),
                <= 32 => ("UInt32", "uint"),
                <= 64 => ("UInt64", ""),
                _ => throw new ArgumentOutOfRangeException(nameof(bitCount),
                    $"bitCount must be between 1-32 {bitCount}")
            };
        }

        public static void AddSerializeHelper(StringBuilder sb, IEnumerable<LogicFieldInfo> fieldInfos,
            string methodName, string targetFieldName)
        {
            sb.Append($@"    public void {methodName}(ulong serializeFlags, IOctetWriter writer)
    {{
");

            var (typeForSerializeFlagString, castTypeString) = SmallestPrimitiveForBitCount(fieldInfos.Count());
            if (castTypeString != "")
            {
                castTypeString = $"({castTypeString})";
            }

            sb.Append($@"    writer.Write{typeForSerializeFlagString}({castTypeString}serializeFlags);
");

            foreach (var fieldInfo in fieldInfos)
            {
                var fieldName = fieldInfo.FieldInfo.Name;
                var completeVariable = $"{targetFieldName}.{fieldName}";
                sb.Append(
                    $@"        if ((serializeFlags & {MaskName(fieldInfo)}) != 0) {SerializeMethod(fieldInfo.FieldInfo.FieldType, completeVariable)};
");
            }

            sb.Append(@"    }

");
        }

        public static void AddSerialize(StringBuilder sb, IEnumerable<LogicFieldInfo> fieldInfos)
        {
            AddSerializeHelper(sb, fieldInfos, "Serialize", "current");
        }


        public static void AddFieldMaskBitSerializeHelper(StringBuilder sb, IEnumerable<LogicFieldInfo> fieldInfos,
            string methodName, string targetFieldName)
        {
            sb.Append($@"    public void {methodName}(ulong serializeFlags, IBitWriter writer)
    {{
");
            var fieldCount = fieldInfos.Count();
            var firstCount = fieldCount > 32 ? 32 : fieldCount;
            var secondCount = fieldCount > 32 ? fieldCount - 32 : 0;

            sb.Append($@"       writer.WriteBits((uint)serializeFlags, {firstCount});
");
            if (secondCount > 0)
            {
                sb.Append($@"       writer.WriteBits((uint)(serializeFlags >> 32), {secondCount});
");
            }

            foreach (var fieldInfo in fieldInfos)
            {
                var fieldName = fieldInfo.FieldInfo.Name;
                var completeVariable = $"{targetFieldName}.{fieldName}";
                sb.Append(
                    $@"        if ((serializeFlags & {MaskName(fieldInfo)}) != 0) {BitSerializeMethod(fieldInfo.FieldInfo.FieldType, completeVariable)};
");
            }

            sb.Append(@"    }

");
        }

        public static void AddFieldMaskBitSerializePrevious(StringBuilder sb, IEnumerable<LogicFieldInfo> fieldInfos)
        {
            AddFieldMaskBitSerializeHelper(sb, fieldInfos, "SerializePrevious", "last");
        }

        public static void AddFieldMaskBitSerialize(StringBuilder sb, IEnumerable<LogicFieldInfo> fieldInfos)
        {
            AddFieldMaskBitSerializeHelper(sb, fieldInfos, "Serialize", "current");
        }


        public static void AddSerializePrevious(StringBuilder sb, IEnumerable<LogicFieldInfo> fieldInfos)
        {
            AddSerializeHelper(sb, fieldInfos, "SerializePrevious", "last");
        }

        public static void AddSerializeAll(StringBuilder sb, IEnumerable<LogicFieldInfo> fieldInfos)
        {
            sb.Append(@"    public void SerializeAll(IOctetWriter writer)
    {
");

            foreach (var fieldInfo in fieldInfos)
            {
                var fieldName = fieldInfo.FieldInfo.Name;
                var completeVariable = $"current.{fieldName}";
                sb.Append(
                    $@"        {SerializeMethod(fieldInfo.FieldInfo.FieldType, completeVariable)};
");
            }

            sb.Append(@"    }

");
        }

        public static void AddBitSerializeAll(StringBuilder sb, IEnumerable<LogicFieldInfo> fieldInfos)
        {
            sb.Append(@"    public void SerializeAll(IBitWriter writer)
    {
");

            foreach (var fieldInfo in fieldInfos)
            {
                var fieldName = fieldInfo.FieldInfo.Name;
                var completeVariable = $"current.{fieldName}";
                sb.Append(
                    $@"        {BitSerializeMethod(fieldInfo.FieldInfo.FieldType, completeVariable)};
");
            }

            sb.Append(@"    }

");
        }

        public static void AddSerializeCorrectionState(StringBuilder sb, IEnumerable<LogicFieldInfo> fieldInfos)
        {
            sb.Append(@"    public void SerializeCorrectionState(IOctetWriter writer)
    {
");

            sb.Append(@"    }

");
        }

        public static void AddDeserializeCorrectionState(StringBuilder sb, IEnumerable<LogicFieldInfo> fieldInfos)
        {
            sb.Append(@"    public void DeserializeCorrectionState(IOctetReader reader)
    {
");

            sb.Append(@"    }

");
        }

        public static void AddBitSerializeCorrectionState(StringBuilder sb, IEnumerable<LogicFieldInfo> fieldInfos)
        {
            sb.Append(@"    public void SerializeCorrectionState(IBitWriter writer)
    {
");

            sb.Append(@"    }

");
        }

        public static void AddBitDeserializeCorrectionState(StringBuilder sb, IEnumerable<LogicFieldInfo> fieldInfos)
        {
            sb.Append(@"    public void DeserializeCorrectionState(IBitReader reader)
    {
");

            sb.Append(@"    }

");
        }

        public static void AddInternalMembers(StringBuilder sb)
        {
            sb.Append(@"    private readonly ActionsContainer actionsContainer = new();


");
        }

        public static void AddInternalMethods(StringBuilder sb)
        {
            sb.Append(@"
    public IAction[] Actions => actionsContainer.Actions.ToArray();
    
    public ILogic Logic => current;

    public void Overwrite()
    {
        last = current;
        actionsContainer.Clear();
    }

    public void FireCreated()
    {
        outFacing.OnSpawned?.Invoke();
    }

    public void FireDestroyed()
    {
        outFacing.OnDestroyed?.Invoke();
    }


");
        }

        public static void AddArchetypeId(StringBuilder sb, LogicInfo logicInfo)
        {
            sb.Append($"    public ArchetypeId ArchetypeId => ArchetypeIdConstants.{logicInfo.Type.Name};").Append(@"
");
        }

        public static void AddArchetypeValueConstants(StringBuilder sb, IEnumerable<LogicInfo> logics)
        {
            sb.Append(@"public static class ArchetypeConstants
{
");

            var i = 1;
            foreach (var logicInfo in logics)
            {
                sb.Append(
                        $"    public const ushort {logicInfo.Type.Name} = {i};")
                    .Append(@"
");
                ++i;
            }

            sb.Append(@"
}
");
        }

        public static void AddArchetypeIdConstants(StringBuilder sb, IEnumerable<LogicInfo> logics)
        {
            sb.Append(@"public static class ArchetypeIdConstants
{
");

            foreach (var logicInfo in logics)
            {
                sb.Append(
                        $"    public static readonly ArchetypeId {logicInfo.Type.Name} = new(ArchetypeConstants.{logicInfo.Type.Name});")
                    .Append(@"
");
            }

            sb.Append(@"
}
");
        }

        public static void AddTypeInformation(StringBuilder sb, IEnumerable<LogicFieldInfo> fieldInfos)
        {
            sb.Append(@"
    public TypeInformation TypeInformation
    {
        get
        {
            return new TypeInformation(new TypeInformationField[]
            {
");

            foreach (var fieldInfo in fieldInfos)
            {
                sb.Append(@"                new() { ")
                    .Append(
                        $"mask = {MaskName(fieldInfo)}, name = new (nameof(current.{fieldInfo.FieldInfo.Name})), type = typeof({fieldInfo.FieldInfo.FieldType})")
                    .Append(@" },
");
            }

            sb.Append(@"
        });
    }

}");
        }


        public static void AddChanges(StringBuilder sb, IEnumerable<LogicFieldInfo> fieldInfos)
        {
            sb.Append(@"    public ulong Changes()
    {
        ulong mask = 0;

        // ReSharper disable EnforceIfStatementBraces

");

            foreach (var fieldInfo in fieldInfos)
            {
                sb.Append(
                        $"        if (current.{fieldInfo.FieldInfo.Name} != last.{fieldInfo.FieldInfo.Name}) mask |= {MaskName(fieldInfo)};")
                    .Append(@"
");
            }

            sb.Append(@"
        return mask;
        // ReSharper restore EnforceIfStatementBraces

    }
");
        }

        public static void AddInvokeChanges(StringBuilder sb, IEnumerable<LogicFieldInfo> fieldInfos)
        {
            sb.Append(@"    public void FireChanges(ulong serializeFlags)
    {
        // ReSharper disable EnforceIfStatementBraces

");
            foreach (var fieldInfo in fieldInfos)
            {
                sb.Append(
                        $"        if ((serializeFlags & {MaskName(fieldInfo)}) != 0) outFacing.On{TitleCase(fieldInfo.FieldInfo.Name)}Changed?.Invoke();")
                    .Append('\n');
            }

            sb.Append(@"
        // ReSharper restore EnforceIfStatementBraces

    }
");
        }

        public static void DoActionsHelper(StringBuilder sb, string prefix, IEnumerable<CommandInfo> commandInfos)
        {
            sb.Append($"    public void {prefix}Action(IAction action)").Append(@"
    {
        switch (action)
        {
");

            foreach (var action in commandInfos)
            {
                var parameterFromFieldParts = action.CommandParameters
                    .Select(commandInfo => "thing." + commandInfo.name).ToList();
                var parameters = string.Join(", ", parameterFromFieldParts);

                var caseSuffix = "";
                if (parameters.Length > 0)
                {
                    caseSuffix = " thing";
                }

                sb.Append($"            case {action.MethodInfo.Name}{caseSuffix}:").Append(@"
                ").Append($"outFacing.{prefix}{action.MethodInfo.Name}?.Invoke({parameters});").Append(@"
            break;
");
            }

            sb.Append(@"
    }
}
");
        }

        public static void DoActions(StringBuilder sb, IEnumerable<CommandInfo> commandInfos)
        {
            DoActionsHelper(sb, "Do", commandInfos);
        }

        public static void UnDoActions(StringBuilder sb, IEnumerable<CommandInfo> commandInfos)
        {
            DoActionsHelper(sb, "UnDo", commandInfos);
        }

        public static void AddOutFacingEntity(StringBuilder sb, LogicInfo logicInfo)
        {
            var outFacingClassName = Suffix(logicInfo.Type.Name, "Entity");
            sb.Append(@"
public class ").Append(outFacingClassName).Append($@"
{{
    public EntityRollMode RollMode => internalEntity.RollMode;

    private readonly {EntityGeneratedInternal(logicInfo)} internalEntity;
    internal {outFacingClassName}({EntityGeneratedInternal(logicInfo)} internalEntity)
    {{
        this.internalEntity = internalEntity;
    }}

    public {FullName(logicInfo.Type)} Self => internalEntity.Self;

    public Action? OnDestroyed;
    public Action? OnSpawned;

    public Action? OnPostUpdate;
");

            AddChangeDelegates(sb, logicInfo, logicInfo.FieldInfos);
            AddActionDelegates(sb, logicInfo.CommandInfos);


            sb.Append(@"
}

");
        }

        public static void AddSetInputMethod(StringBuilder sb)
        {
            sb.Append(@"
            public void SetInput(IOctetReader reader)
            {
                current.SetInput(GameInputReader.Read(reader));
            }
");
        }


        public static void AddInternalEntity(StringBuilder sb, LogicInfo logicInfo)
        {
            var inherit = "IGeneratedEntity";
            if (logicInfo.CanTakeInput)
            {
                inherit += ", IInputDeserialize";
            }

            sb.Append(@"
public class ").Append(EntityGeneratedInternal(logicInfo)).Append($" : {inherit}").Append(@"
{
");
            AddInternalMembers(sb);

            sb.Append(@$"public {EntityGeneratedInternal(logicInfo)}()
    {{
        outFacing = new(this);
    }}
");


            sb.Append("    ").Append(FullName(logicInfo.Type)).Append(@" current;
    ").Append(FullName(logicInfo.Type)).Append(@" last;

").Append($"    public {FullName(logicInfo.Type)} Self => current;").Append(@"

        public EntityRollMode RollMode { get; set; }

").Append($" internal {FullName(logicInfo.Type)} Current").Append(@"
            {
                set => current = value;
            }
");

            sb.Append($"     {Suffix(logicInfo.Type.Name, "Entity")} outFacing;").Append(@"
").Append($"    public {Suffix(logicInfo.Type.Name, "Entity")} OutFacing => outFacing;").Append(@"

");

            var fieldInfos = logicInfo.FieldInfos;
            AddArchetypeId(sb, logicInfo);

            AddChangeMaskConstants(sb, fieldInfos);


            // ----- methods ----

            AddInternalMethods(sb);

            if (logicInfo.CanTakeInput)
            {
                AddSetInputMethod(sb);
            }

            DoActions(sb, logicInfo.CommandInfos);
            UnDoActions(sb, logicInfo.CommandInfos);

            AddSerialize(sb, fieldInfos);
            AddFieldMaskBitSerialize(sb, fieldInfos);

            AddSerializePrevious(sb, fieldInfos);
            AddFieldMaskBitSerializePrevious(sb, fieldInfos);

            AddSerializeAll(sb, fieldInfos);
            AddBitSerializeAll(sb, fieldInfos);

            AddDeserializeAll(sb, fieldInfos);
            AddBitDeserializeAll(sb, fieldInfos);


            AddSerializeCorrectionState(sb, fieldInfos);
            AddBitSerializeCorrectionState(sb, fieldInfos);

            AddDeserializeCorrectionState(sb, fieldInfos);
            AddBitDeserializeCorrectionState(sb, fieldInfos);

            AddDeserialize(sb, fieldInfos);
            AddFieldMaskBitDeserialize(sb, fieldInfos);


            AddTick(sb, logicInfo);

            AddChanges(sb, fieldInfos);
            AddInvokeChanges(sb, fieldInfos);
            AddTypeInformation(sb, fieldInfos);

            sb.Append(@"
}

");
        }

        public static void AddActions(StringBuilder sb, LogicInfo logicInfo)
        {
            AddActionStructs(sb, logicInfo.CommandInfos);
            AddActionImplementation(sb, logicInfo);
        }

        public static void AddArchetypeConstants(StringBuilder sb, IEnumerable<LogicInfo> infos)
        {
            AddArchetypeValueConstants(sb, infos);
            AddArchetypeIdConstants(sb, infos);
        }

        public static void AddGameInputReader(StringBuilder sb, GameInputInfo gameInputInfo)
        {
            AddStaticClassDeclaration(sb, "GameInputReader");
            var gameInputName = FullName(gameInputInfo.Type);
            sb.Append(@$"    public static {gameInputName} Read(IOctetReader reader)
    {{
        return new()
        {{
");

            foreach (var fieldInfo in gameInputInfo.FieldInfos)
            {
                var fieldName = fieldInfo.FieldInfo.Name;
                sb.Append(
                    $@"             {fieldName} = {DeSerializeMethod(fieldInfo.FieldInfo.FieldType)},
");
            }

            sb.Append("        }; // end of new");

            AddEndDeclaration(sb);

            AddEndDeclaration(sb);
        }

        public static void AddGameInputWriter(StringBuilder sb, GameInputInfo gameInputInfo)
        {
            AddStaticClassDeclaration(sb, "GameInputWriter");
            var gameInputName = FullName(gameInputInfo.Type);
            sb.Append(@$"    public static void Write(IOctetWriter writer, {gameInputName} input)
    {{
");

            foreach (var fieldInfo in gameInputInfo.FieldInfos)
            {
                var fieldName = fieldInfo.FieldInfo.Name;
                var completeVariable = $"input.{fieldName}";
                sb.Append(
                    $@"       {SerializeMethod(fieldInfo.FieldInfo.FieldType, completeVariable)};
");
            }


            AddEndDeclaration(sb);

            AddEndDeclaration(sb);
        }

        public static void AddGameInputFetch(StringBuilder sb, GameInputFetchInfo inputFetchInfo)
        {
            var fetchMethodName = FullName(inputFetchInfo.MethodInfo);
            sb.Append(@$"

public class GeneratedInputFetch : IInputPackFetch
{{
    public ReadOnlySpan<byte> Fetch(LocalPlayerIndex index)
    {{
        var gameInput = {fetchMethodName}(index); // Found from scanning
        var writer = new OctetWriter(256);
        GameInputWriter.Write(writer, gameInput);

        return writer.Octets;
    }}
}}

");
        }


        /// <summary>
        ///     Generates C# source code that handle the serialization of the user specific (game specific) types.
        /// </summary>
        /// <param name="infos"></param>
        /// <returns></returns>
        public static string Generate(IEnumerable<LogicInfo> infos, GameInputInfo gameInputInfo,
            GameInputFetchInfo gameInputFetchInfo)
        {
            var sb = new StringBuilder();

            DoNotEditComment(sb);

            sb.Append(@"

using Piot.Surge.FastTypeInformation;
using Piot.Flood;
using Piot.Surge.Types.Serialization;
using Piot.Surge.Entities;
using Piot.Surge.GeneratedEntity;
using Piot.Surge.LogicalInput;
using Piot.Surge.LocalPlayer;
using Piot.Surge.LogicAction;

namespace Piot.Surge.Internal.Generated
{
");

            AddArchetypeConstants(sb, infos);
            AddEntityCreation(sb, infos);

            AddEngineWorld(sb, infos);
            AddEngineSpawner(sb, infos);

            AddGameInputReader(sb, gameInputInfo);
            AddGameInputWriter(sb, gameInputInfo);
            AddGameInputFetch(sb, gameInputFetchInfo);

            foreach (var logicInfo in infos)
            {
                AddActions(sb, logicInfo);

                AddOutFacingEntity(sb, logicInfo);
                AddInternalEntity(sb, logicInfo);
            }

            sb.Append(@"
} // Namespace
");

            return sb.ToString();
        }
    }
}