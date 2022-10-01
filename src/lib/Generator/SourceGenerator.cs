/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// ReSharper disable MemberCanBePrivate.Global

namespace Piot.Surge.Generator
{
    public static class SourceGenerator
    {
        public static string ActionsName(Type t)
        {
            return Generator.Suffix(t.Name, "Actions");
        }

        public static string EntityExternal(LogicInfo info)
        {
            return Generator.Suffix(info.Type.Name, "Entity");
        }

        public static string EntityGeneratedInternal(LogicInfo info)
        {
            return Generator.Suffix(info.Type.Name, "EntityInternal");
        }

        public static string EntityGeneratedInternalSimulationInfo(LogicInfo info)
        {
            return Generator.Suffix(EntityGeneratedInternal(info), "SimulationInfo");
        }

        public static string EntityExternalFullName(LogicInfo info)
        {
            return Generator.Suffix(Generator.FullName(info.Type), "Entity");
        }

        public static string MovementSimulationInfoName(LogicInfo info)
        {
            return Generator.Suffix(Generator.FullName(info.Type), "MovementSimulationInfo");
        }

        public static string LogicFullName(LogicInfo info)
        {
            return Generator.FullName(info.Type);
        }


        public static void DoNotEditComment(StringBuilder sb)
        {
            sb.Append(@"// Code generated by Surge generator. DO NOT EDIT.
// <auto-generated /> This file has been auto generated.
#nullable enable
");
        }


        public static void AddEntityCreation(StringBuilder sb, IEnumerable<LogicInfo> logicInfos)
        {
            Generator.AddClassDeclaration(sb, "GeneratedEntityGhostCreator", "IEntityGhostCreator");
            sb.Append(@" public IEntity CreateGhostEntity(ArchetypeId archetypeId, EntityId entityId)
            {
                ICompleteEntity completeEntity = archetypeId.id switch
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
                
                return new Entity(entityId, completeEntity);
            }
        }
");
        }


        public static void AddEngineSpawner(StringBuilder sb, IEnumerable<LogicInfo> infos)
        {
            const string className = "GeneratedHostEntitySpawner";
            Generator.AddClassDeclaration(sb, className);
            sb.Append($@"
            readonly IAuthoritativeEntityContainer container;
    readonly INotifyEntityCreation notifyWorld;

    public {className}(IAuthoritativeEntityContainer container, INotifyEntityCreation notifyWorld)
    {{
        this.container = container;
        this.notifyWorld = notifyWorld;
    }}
");
            foreach (var info in infos)
            {
                var logicName = Generator.FullName(info.Type);
                sb.Append(@$"
    public (IEntity, {EntityGeneratedInternal(info)}) Spawn{Generator.ShortName(info.Type)}({logicName} logic)
    {{ 
        var internalEntity = new {EntityGeneratedInternal(info)}
        {{
            Current = logic
        }};
        var entity = container.SpawnEntity(internalEntity);
        notifyWorld.CreateGameEngineEntity(entity);
        return (entity, internalEntity);
     }}
");
            }

            Generator.AddEndDeclaration(sb);
        }

        public static void AddEngineWorld(StringBuilder sb, IEnumerable<LogicInfo> infos)
        {
            Generator.AddClassDeclaration(sb, "GeneratedNotifyEntityCreation",
                "INotifyEntityCreation, INotifyContainerReset");

            foreach (var info in infos)
            {
                sb.Append($"public Action<IEntity, {EntityExternal(info)}>? OnSpawn{info.Type.Name};").Append(@"
");
            }

            sb.Append(@"
            public Action? OnReset;

            public void NotifyGameEngineResetNetworkEntities()
            {
                OnReset?.Invoke();
            }
");


            sb.Append(@"
        void INotifyEntityCreation.CreateGameEngineEntity(IEntity entity)
        {
            switch (entity.CompleteEntity)
            {
");

            foreach (var info in infos)
            {
                sb.Append($"case {EntityGeneratedInternal(info)} internalEntity:").Append(@"
").Append($"OnSpawn{info.Type.Name}?.Invoke(entity, internalEntity.OutFacing);").Append(@"
    break;
");
            }

            sb.Append(@"
                default:
                    throw new Exception(""Internal error"");
            }
        }
                ");

            Generator.AddEndDeclaration(sb);
        }


        /*
         * public sealed class GeneratedEntityWorld
{
    public Action<AvatarLogicEntity>? OnSpawnAvatarLogic;
    public Action<FireballLogicEntity>? OnSpawnFireballLogic;
        public void CreateGameEngineEntity(IEntity entity)
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


        public static void AddChangeDelegate(StringBuilder sb, LogicFieldInfo fieldInfo)
        {
            var titledField = Generator.TitleCase(fieldInfo.FieldInfo.Name);
            sb.Append(@$"    public Action? On{titledField}Changed;

");
        }


        public static void AddSection(StringBuilder sb, string name)
        {
            sb.Append($"// --------------- {name} ---------------").Append(@"
");
        }


        public static void AddActionStructs(StringBuilder sb, IEnumerable<CommandInfo> commandInfos)
        {
            AddSection(sb, "Internal Action Structs");

            foreach (var commandInfo in commandInfos)
            {
                var actionName = commandInfo.MethodInfo.Name;
                Generator.AddStructDeclaration(sb, actionName, "IAction");

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
            var actionsImplementationName = Generator.Suffix(info.Type.Name, "Actions");
            var actionInterface = Generator.FullName(info.CommandsInterface!);

            sb.Append($"public sealed class {actionsImplementationName} : {actionInterface}").Append(@"
{
    readonly IActionsContainer actionsContainer;

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
            var delegateName = Generator.Suffix(titledField, "Delegate");
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

        public static string MaskName(string Name)
        {
            return Generator.Suffix(Generator.TitleCase(Name), "Mask");
        }

        public static void AddChangeMaskConstant(StringBuilder sb, LogicFieldInfo fieldInfo)
        {
            sb.Append(
                @$"    public const ulong {MaskName(fieldInfo.FieldInfo.Name)} = 0x{fieldInfo.Mask:x08};
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


        public static void AddChangeDelegates(StringBuilder sb, LogicInfo logicInfo,
            IEnumerable<LogicFieldInfo> fieldInfos)
        {
            foreach (var fieldInfo in fieldInfos)
            {
                AddChangeDelegate(sb, fieldInfo);
            }
        }


        public static void AddDeserialize(StringBuilder sb, IEnumerable<LogicFieldInfo> fieldInfos)
        {
            sb.Append(@"    public ulong Deserialize(IOctetReader reader)
    {

#if DEBUG
        OctetMarker.AssertMarker(reader, Constants.OctetsSerializeMarker);
#endif
");
            var (typeForSerializeFlagString, _) = SmallestPrimitiveForBitCount(fieldInfos.Count());
            var castString = typeForSerializeFlagString == "UInt64" ? "" : "(ulong)";
            sb.Append($@"    var serializeFlags = {castString} reader.Read{typeForSerializeFlagString}();
");
            foreach (var fieldInfo in fieldInfos)
            {
                var fieldName = fieldInfo.FieldInfo.Name;
                sb.Append(
                    $@"        if ((serializeFlags & {MaskName(fieldInfo.FieldInfo.Name)}) != 0) current.{fieldName} = {GenerateSerializers.DeSerializeMethod(fieldInfo.FieldInfo.FieldType)};
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
#if DEBUG
        BitMarker.AssertMarker(reader, Constants.BitSerializeMarker);
#endif

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
                    $@"        if ((serializeFlags & {MaskName(fieldInfo.FieldInfo.Name)}) != 0) current.{fieldName} = {GenerateSerializers.BitDeSerializeMethod(fieldInfo.FieldInfo.FieldType)};
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

#if DEBUG
            OctetMarker.AssertMarker(reader, Constants.OctetsSerializeAllMarker);
#endif

");

            foreach (var fieldInfo in fieldInfos)
            {
                var fieldName = fieldInfo.FieldInfo.Name;
                sb.Append(
                    $@"        current.{fieldName} = {GenerateSerializers.DeSerializeMethod(fieldInfo.FieldInfo.FieldType)};
");
            }

            sb.Append(@"    }

");
        }

        public static void AddBitDeserializeAll(StringBuilder sb, IEnumerable<LogicFieldInfo> fieldInfos)
        {
            sb.Append(@"    public void DeserializeAll(IBitReader reader)
    {

#if DEBUG
            BitMarker.AssertMarker(reader, Constants.BitSerializeAllMarker);
#endif
");

            foreach (var fieldInfo in fieldInfos)
            {
                var fieldName = fieldInfo.FieldInfo.Name;
                sb.Append(
                    $@"        current.{fieldName} = {GenerateSerializers.BitDeSerializeMethod(fieldInfo.FieldInfo.FieldType)};
");
            }

            sb.Append(@"    }

");
        }

        public static void AddTick(StringBuilder sb, LogicInfo info)
        {
            var actionsImplementationName = ActionsName(info.Type);

            if (info.TickMethod.GetParameters().Length == 0)
            {
                sb.Append(@"

    public void Tick()
    {
        current.Tick();
    }

");
            }
            else
            {
                sb.Append(@"

    public void Tick()
    {
        var actions = ").Append($"new {actionsImplementationName}(actionsContainer);").Append(@"
        current.Tick(actions);
    }

");
            }
        }

        public static void AddMovementSimulationTick(StringBuilder sb, LogicInfo logicInfo)
        {
            sb.Append(@"
        public void MovementSimulationTick()
        {
            if (outFacing.OnMovementSimulation is null)
            {
                return;
            }
            var movementSimulationInfo = OutFacing.OnMovementSimulation.Invoke(current);
        
");

            foreach (var fieldInfo in logicInfo.MovementSimulationFieldInfos)
            {
                sb.Append(
                    $@"        current.{fieldInfo.FieldInfo.Name} = movementSimulationInfo.{fieldInfo.FieldInfo.Name};
");
            }

            sb.Append(@"

        }

");
        }

        public static void AddEmptyMovementSimulationTick(StringBuilder sb)
        {
            sb.Append(@"
        public void MovementSimulationTick()
        {
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
#if DEBUG
        OctetMarker.WriteMarker(writer, Constants.OctetsSerializeMarker);
#endif

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
                    $@"        if ((serializeFlags & {MaskName(fieldInfo.FieldInfo.Name)}) != 0) {GenerateSerializers.SerializeMethod(fieldInfo.FieldInfo.FieldType, completeVariable)};
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
#if DEBUG
        BitMarker.WriteMarker(writer, Constants.BitSerializeMarker);
#endif

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
                    $@"        if ((serializeFlags & {MaskName(fieldInfo.FieldInfo.Name)}) != 0) {GenerateSerializers.BitSerializeMethod(fieldInfo.FieldInfo.FieldType, completeVariable)};
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

        public static void AddSerializeAllHelper(StringBuilder sb, IEnumerable<LogicFieldInfo> fieldInfos,
            string methodName, string targetName)
        {
            sb.Append(@$"    public void {methodName}(IOctetWriter writer)
    {{
#if DEBUG
            OctetMarker.WriteMarker(writer, Constants.OctetsSerializeAllMarker);
#endif
");

            foreach (var fieldInfo in fieldInfos)
            {
                var fieldName = fieldInfo.FieldInfo.Name;
                var completeVariable = $"{targetName}.{fieldName}";
                sb.Append(
                    $@"        {GenerateSerializers.SerializeMethod(fieldInfo.FieldInfo.FieldType, completeVariable)};
");
            }

            sb.Append(@"    }

");
        }

        public static void AddSerializeAll(StringBuilder sb, IEnumerable<LogicFieldInfo> fieldInfos)
        {
            AddSerializeAllHelper(sb, fieldInfos, "SerializeAll", "current");
        }

        public static void AddSerializePreviousAll(StringBuilder sb, IEnumerable<LogicFieldInfo> fieldInfos)
        {
            AddSerializeAllHelper(sb, fieldInfos, "SerializePreviousAll", "last");
        }

        public static void AddBitSerializeAll(StringBuilder sb, IEnumerable<LogicFieldInfo> fieldInfos)
        {
            sb.Append(@"    public void SerializeAll(IBitWriter writer)
    {
#if DEBUG
            BitMarker.WriteMarker(writer, Constants.BitSerializeAllMarker);
#endif

");

            foreach (var fieldInfo in fieldInfos)
            {
                var fieldName = fieldInfo.FieldInfo.Name;
                var completeVariable = $"current.{fieldName}";
                sb.Append(
                    $@"        {GenerateSerializers.BitSerializeMethod(fieldInfo.FieldInfo.FieldType, completeVariable)};
");
            }

            sb.Append(@"    }

");
        }

        public static void AddSerializeCorrectionState(StringBuilder sb)
        {
            sb.Append(@"    public void SerializeCorrectionState(IOctetWriter writer)
    {
");

            sb.Append(@"    }

");
        }

        public static void AddDeserializeCorrectionState(StringBuilder sb)
        {
            sb.Append(@"    public void DeserializeCorrectionState(IOctetReader reader)
    {
");

            sb.Append(@"    }

");
        }

        public static void AddBitSerializeCorrectionState(StringBuilder sb)
        {
            sb.Append(@"    public void SerializeCorrectionState(IBitWriter writer)
    {
");

            sb.Append(@"    }

");
        }

        public static void AddBitDeserializeCorrectionState(StringBuilder sb)
        {
            sb.Append(@"    public void DeserializeCorrectionState(IBitReader reader)
    {
");

            sb.Append(@"    }

");
        }

        public static void AddInternalMembers(StringBuilder sb)
        {
            sb.Append(@"
        readonly ActionsContainer actionsContainer = new();


");
        }

        public static void AddInternalMethods(StringBuilder sb)
        {
            sb.Append(@"
    public IAction[] Actions => actionsContainer.Actions;
    
    public ILogic Logic => current;

    public void ClearChanges()
    {
        last = current;
    }
    
    public void ClearActions()
    {
        actionsContainer.Clear();
    }

    public void FireDestroyed()
    {
        outFacing.OnDestroyed?.Invoke();
    }

    public void FireReplicate()
    {
        outFacing.OnReplicated?.Invoke();
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
                        $"mask = {MaskName(fieldInfo.FieldInfo.Name)}, name = new (nameof(current.{fieldInfo.FieldInfo.Name})), type = typeof({fieldInfo.FieldInfo.FieldType})")
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
                        $"        if (current.{fieldInfo.FieldInfo.Name} != last.{fieldInfo.FieldInfo.Name}) mask |= {MaskName(fieldInfo.FieldInfo.Name)};")
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
                        $"        if ((serializeFlags & {MaskName(fieldInfo.FieldInfo.Name)}) != 0) outFacing.On{Generator.TitleCase(fieldInfo.FieldInfo.Name)}Changed?.Invoke();")
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
            var outFacingClassName = Generator.Suffix(logicInfo.Type.Name, "Entity");
            sb.Append(@"
public sealed class ").Append(outFacingClassName).Append($@"
{{
    public EntityRollMode RollMode => internalEntity.RollMode;

    readonly {EntityGeneratedInternal(logicInfo)} internalEntity;
    internal {outFacingClassName}({EntityGeneratedInternal(logicInfo)} internalEntity)
    {{
        this.internalEntity = internalEntity;
    }}

    public {EntityGeneratedInternal(logicInfo)} Internal => internalEntity;

    public override string ToString()
    {{
        return $""[{outFacingClassName} logic:{{Self}}]"";  
    }}

    public void Destroy()
    {{
        internalEntity.Destroy();
    }}

    public {Generator.FullName(logicInfo.Type)} Self => internalEntity.Self;

    public Action? OnDestroyed;
    public Action? OnReplicated;
    public Action? OnPostUpdate;
");
            if (logicInfo.SimulationFieldInfos.Any())
            {
                sb.Append($@"
    public Func<{EntityGeneratedInternalSimulationInfo(logicInfo)}> OnSnapshot
    {{
            set => internalEntity.OnSnapshot = value;
    }}
");
            }

            if (logicInfo.MovementSimulationFieldInfos.Any())
            {
                sb.Append($@"
        public Func<{LogicFullName(logicInfo)}, {MovementSimulationInfoName(logicInfo)}>? OnMovementSimulation;
");
            }


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

        public static void SimulationCaptureInfo(StringBuilder sb, LogicInfo logicInfo)
        {
            sb.Append(@$"
public struct {EntityGeneratedInternalSimulationInfo(logicInfo)}
{{


");
            foreach (var fieldInfo in logicInfo.SimulationFieldInfos)
            {
                sb.Append($@"public {Generator.FullName(fieldInfo.FieldInfo.FieldType)} {fieldInfo.FieldInfo.Name};
");
            }

            sb.Append(@"
}
");
        }

        public static void MovementSimulationInfo(StringBuilder sb, LogicInfo logicInfo)
        {
            sb.Append(@$"
public struct {MovementSimulationInfoName(logicInfo)}
{{


");
            foreach (var fieldInfo in logicInfo.MovementSimulationFieldInfos)
            {
                sb.Append($@"public {Generator.FullName(fieldInfo.FieldInfo.FieldType)} {fieldInfo.FieldInfo.Name};
");
            }

            sb.Append(@"
}
");
        }

        public static void CaptureSnapshot(StringBuilder sb, LogicInfo logicInfo)
        {
            sb.Append($@"             

            public Func<{EntityGeneratedInternalSimulationInfo(logicInfo)}> OnSnapshot;

            void IAuthoritativeEntityCaptureSnapshot.CaptureSnapshot()
            {{
                var capturedState = OnSnapshot();
            ");

            foreach (var fieldInfo in logicInfo.SimulationFieldInfos)
            {
                sb.Append($@"        current.{fieldInfo.FieldInfo.Name} = capturedState.{fieldInfo.FieldInfo.Name};
");
            }

            sb.Append(@"

            }

");
        }

        public static void AddEmptyCaptureSnapshot(StringBuilder sb)
        {
            sb.Append(@"
            void IAuthoritativeEntityCaptureSnapshot.CaptureSnapshot()
            {
            }
");
        }


        public static void AddInternalEntity(StringBuilder sb, LogicInfo logicInfo)
        {
            var inherit = "ICompleteEntity";
            if (logicInfo.CanTakeInput)
            {
                inherit += ", IInputDeserialize";
            }

            sb.Append(@"
public sealed class ").Append(EntityGeneratedInternal(logicInfo)).Append($" : {inherit}").Append(@"
{
");
            AddInternalMembers(sb);

            sb.Append(@$"public {EntityGeneratedInternal(logicInfo)}()
    {{
        outFacing = new(this);
    }}
");


            sb.Append("    ").Append(Generator.FullName(logicInfo.Type)).Append(@" current;
    ").Append(Generator.FullName(logicInfo.Type)).Append(@" last;

").Append($"    public {Generator.FullName(logicInfo.Type)} Self => current;").Append(@"

        public EntityRollMode RollMode { get; set; }

        public void Destroy()
        {
            // TODO: Add implementation
        }



").Append($" internal {Generator.FullName(logicInfo.Type)} Current").Append(@"
            {
                set => current = value;
            }
");

            sb.Append($@"
        public override string ToString()
        {{
            return $""[{EntityGeneratedInternal(logicInfo)} logic={{Self}}]"";
        }}
");

            sb.Append($"     {Generator.Suffix(logicInfo.Type.Name, "Entity")} outFacing;").Append(@"
").Append($"    public {Generator.Suffix(logicInfo.Type.Name, "Entity")} OutFacing => outFacing;").Append(@"

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

            if (logicInfo.SimulationFieldInfos.Any())
            {
                CaptureSnapshot(sb, logicInfo);
            }
            else
            {
                AddEmptyCaptureSnapshot(sb);
            }

            DoActions(sb, logicInfo.CommandInfos);
            UnDoActions(sb, logicInfo.CommandInfos);

            AddSerialize(sb, fieldInfos);
            AddFieldMaskBitSerialize(sb, fieldInfos);

            AddSerializePrevious(sb, fieldInfos);
            AddFieldMaskBitSerializePrevious(sb, fieldInfos);

            AddSerializeAll(sb, fieldInfos);
            AddBitSerializeAll(sb, fieldInfos);

            AddSerializePreviousAll(sb, fieldInfos);

            AddDeserializeAll(sb, fieldInfos);
            AddBitDeserializeAll(sb, fieldInfos);


            AddSerializeCorrectionState(sb);
            AddBitSerializeCorrectionState(sb);

            AddDeserializeCorrectionState(sb);
            AddBitDeserializeCorrectionState(sb);

            AddDeserialize(sb, fieldInfos);
            AddFieldMaskBitDeserialize(sb, fieldInfos);


            AddTick(sb, logicInfo);
            if (logicInfo.MovementSimulationFieldInfos.Any())
            {
                AddMovementSimulationTick(sb, logicInfo);
            }
            else
            {
                AddEmptyMovementSimulationTick(sb);
            }

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
            Generator.AddStaticClassDeclaration(sb, "GameInputReader");
            var gameInputName = Generator.FullName(gameInputInfo.Type);
            sb.Append(@$"    public static {gameInputName} Read(IOctetReader reader)
    {{
        return new()
        {{
");

            foreach (var fieldInfo in gameInputInfo.FieldInfos)
            {
                var fieldName = fieldInfo.FieldInfo.Name;
                sb.Append(
                    $@"             {fieldName} = {GenerateSerializers.DeSerializeMethod(fieldInfo.FieldInfo.FieldType)},
");
            }

            sb.Append("        }; // end of new");

            Generator.AddEndDeclaration(sb);

            Generator.AddEndDeclaration(sb);
        }

        public static void AddGameInputWriter(StringBuilder sb, GameInputInfo gameInputInfo)
        {
            Generator.AddStaticClassDeclaration(sb, "GameInputWriter");
            var gameInputName = Generator.FullName(gameInputInfo.Type);
            sb.Append(@$"    public static void Write(IOctetWriter writer, {gameInputName} input)
    {{
");

            foreach (var fieldInfo in gameInputInfo.FieldInfos)
            {
                var fieldName = fieldInfo.FieldInfo.Name;
                var completeVariable = $"input.{fieldName}";
                sb.Append(
                    $@"       {GenerateSerializers.SerializeMethod(fieldInfo.FieldInfo.FieldType, completeVariable)};
");
            }


            Generator.AddEndDeclaration(sb);

            Generator.AddEndDeclaration(sb);
        }

        public static void AddGameInputFetch(StringBuilder sb, GameInputInfo gameInputInfo)
        {
            var fullGameInputTypeName = Generator.FullName(gameInputInfo.Type);
            sb.Append(@$"

public class GeneratedInputPackFetch : IInputPackFetch
{{
        InputPackFetch<{fullGameInputTypeName}>? inputFetcher;

        public Func<LocalPlayerIndex, {fullGameInputTypeName}> GameSpecificInputFetch
        {{
            set => inputFetcher = new(value, GameInputWriter.Write);
        }}

        public ReadOnlySpan<byte> Fetch(LocalPlayerIndex index)
        {{
            return inputFetcher is null ? ReadOnlySpan<byte>.Empty : inputFetcher.Fetch(index);
        }}
        }}


");
        }


        /// <summary>
        ///     Generates C# source code that handle the serialization of the user specific (game specific) types.
        /// </summary>
        /// <param name="infos"></param>
        /// <param name="shortLivedEventInterface"></param>
        /// <returns></returns>
        public static string Generate(IEnumerable<LogicInfo> infos, GameInputInfo gameInputInfo,
            ShortLivedEventInterface shortLivedEventInterface)
        {
            var sb = new StringBuilder();

            DoNotEditComment(sb);

            sb.Append(@"

using System;
using Piot.Surge.FastTypeInformation;
using Piot.Flood;
using Piot.Surge.Types.Serialization;
using Piot.Surge.Entities;
using Piot.Surge.Event;
using Piot.Surge.LogicalInput;
using Piot.Surge.LocalPlayer;
using Piot.Surge.LogicAction;

namespace Piot.Surge.Internal.Generated
{
");

            var indent = 0;
            AddArchetypeConstants(sb, infos);
            AddEntityCreation(sb, infos);

            AddEngineWorld(sb, infos);
            AddEngineSpawner(sb, infos);

            AddGameInputReader(sb, gameInputInfo);
            AddGameInputWriter(sb, gameInputInfo);
            AddGameInputFetch(sb, gameInputInfo);

            GenerateShortLivedEvent.AddShortLivedEventValueConstants(sb, shortLivedEventInterface.methodInfos);
            GenerateShortLivedEventsEnqueue.AddEventEnqueue(sb, shortLivedEventInterface, indent);
            GenerateShortLivedEventsProcessor.AddEventProcessor(sb, shortLivedEventInterface, indent);

            foreach (var logicInfo in infos)
            {
                if (logicInfo.SimulationFieldInfos.Any())
                {
                    SimulationCaptureInfo(sb, logicInfo);
                }

                if (logicInfo.MovementSimulationFieldInfos.Any())
                {
                    MovementSimulationInfo(sb, logicInfo);
                }
            }


            foreach (var logicInfo in infos)
            {
                if (logicInfo.CommandsInterface is not null)
                {
                    AddActions(sb, logicInfo);
                }

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