/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            return t.FullName.Replace('+', '.');
        }

        public static string ActionsName(Type t)
        {
            return Suffix(t.Name, "Actions");
        }

        public static string TitleCase(string input)
        {
            return string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1));
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


        public static void AddEndDeclaration(StringBuilder sb)
        {
            sb.Append(@"
    }
");
        }

        public static void AddEntityCreation(StringBuilder sb, IEnumerable<LogicInfo> logicInfos)
        {
            AddClassDeclaration(sb, "GeneratedEntityCreation", "IEntityCreation");
            sb.Append(@" public IEntity CreateEntity(ArchetypeId archetypeId, EntityId entityId)
            {
                IGeneratedEntity generatedEntity = archetypeId.id switch
                {
");
            foreach (var logicInfo in logicInfos)
                sb.Append(
                        $"        ArchetypeConstants.{logicInfo.Type.Name} => new {EntityGeneratedInternal(logicInfo)}(),")
                    .Append(@"
");

            sb.Append(@"            _ => throw new Exception($""unknown entity to create {archetypeId}""),

                };
                
                return new Entity(entityId, generatedEntity);
            }
        }
");
        }

        public static void AddEngineWorld(StringBuilder sb, IEnumerable<LogicInfo> infos)
        {
            AddSection(sb, "EngineWorld");
            AddClassDeclaration(sb, "EngineWorld");

            foreach (var info in infos)
                sb.Append($"public Action<{EntityExternal(info)}>? OnSpawn{info.Type.Name};").Append(@"
");

            AddEndDeclaration(sb);
        }

        public static void AddEngineNotifier(StringBuilder sb, IEnumerable<LogicInfo> infos)
        {
            AddClassDeclaration(sb, "NotifyEngineWorld");
            sb.Append(@"
        public static void NotifyCreation(IEntity entity, EngineWorld engineWorld)
        {
            switch (entity)
            {
");

            foreach (var info in infos)
                sb.Append($"case {EntityGeneratedInternal(info)} internalEntity:").Append(@"
").Append($"engineWorld.OnSpawn{info.Type.Name}?.Invoke(internalEntity.OutFacing);").Append(@"
    break;
");

            sb.Append(@"
                default:
                    throw new Exception(""Internal error"");
            }
        }
                ");

            AddEndDeclaration(sb);
        }


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
                    sb.Append($"    public {parameter.type} {parameter.name}").Append(@";
");
            }

            sb.Append(@"
}
");
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
            foreach (var commandInfo in commandInfos) AddActionDelegate(sb, commandInfo);
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
            foreach (var fieldInfo in fieldInfos) AddChangeMaskConstant(sb, fieldInfo);

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
            if (type == typeof(bool)) return "reader.ReadUInt8() != 0";
            if (type == typeof(ushort)) return PrimitiveDeSerializer("UInt16");
            if (type == typeof(uint)) return PrimitiveDeSerializer("UInt32");

            if (type == typeof(ulong)) return PrimitiveDeSerializer("UInt64");

            return DeSerializeMethodForValueTypes(type);
        }

        public static string SerializeMethod(Type type, string variableName)
        {
            if (type == typeof(bool)) return $"writer.WriteUInt8({variableName} ? (byte)1 : (byte)0)";
            if (type == typeof(ushort)) return PrimitiveSerializer("UInt16", variableName);
            if (type == typeof(uint)) return PrimitiveSerializer("UInt32", variableName);

            if (type == typeof(ulong)) return PrimitiveSerializer("UInt64", variableName);

            return SerializeMethodForValueTypes(type, variableName);
        }

        public static void AddChangeDelegates(StringBuilder sb, LogicInfo logicInfo,
            IEnumerable<LogicFieldInfo> fieldInfos)
        {
            foreach (var fieldInfo in fieldInfos) AddChangeDelegate(sb, logicInfo, fieldInfo);
        }

        public static void AddDeserialize(StringBuilder sb, IEnumerable<LogicFieldInfo> fieldInfos)
        {
            sb.Append(@"    public void Deserialize(ulong serializeFlags, IOctetReader reader)
    {
");

            foreach (var fieldInfo in fieldInfos)
            {
                var fieldName = fieldInfo.FieldInfo.Name;
                sb.Append(
                    $@"        if ((serializeFlags & {MaskName(fieldInfo)}) != 0) current.{fieldName} = {DeSerializeMethod(fieldInfo.FieldInfo.FieldType)};
");
            }

            sb.Append(@"    }

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

        public static void AddSerialize(StringBuilder sb, IEnumerable<LogicFieldInfo> fieldInfos)
        {
            sb.Append(@"    public void Serialize(ulong serializeFlags, IOctetWriter writer)
    {
");

            foreach (var fieldInfo in fieldInfos)
            {
                var fieldName = fieldInfo.FieldInfo.Name;
                var completeVariable = $"current.{fieldName}";
                sb.Append(
                    $@"        if ((serializeFlags & {MaskName(fieldInfo)}) != 0) {SerializeMethod(fieldInfo.FieldInfo.FieldType, completeVariable)};
");
            }

            sb.Append(@"    }

");
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
                sb.Append(
                        $"    public static readonly ArchetypeId {logicInfo.Type.Name} = new(ArchetypeConstants.{logicInfo.Type.Name});")
                    .Append(@"
");

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
                sb.Append(@"                new() { ")
                    .Append(
                        $"mask = {MaskName(fieldInfo)}, name = new FieldName(nameof(current.{fieldInfo.FieldInfo.Name})), type = typeof({fieldInfo.FieldInfo.FieldType})")
                    .Append(@" },
");

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

");

            foreach (var fieldInfo in fieldInfos)
                sb.Append(
                        $"        if (current.{fieldInfo.FieldInfo.Name} != last.{fieldInfo.FieldInfo.Name}) mask |= {MaskName(fieldInfo)};")
                    .Append(@"
");

            sb.Append(@"
        return mask;

    }
");
        }

        public static void AddInvokeChanges(StringBuilder sb, IEnumerable<LogicFieldInfo> fieldInfos)
        {
            sb.Append(@"    public void FireChanges(ulong serializeFlags)
    {
");
            foreach (var fieldInfo in fieldInfos)
                sb.Append(
                        $"        if ((serializeFlags & {MaskName(fieldInfo)}) != 0) outFacing.On{TitleCase(fieldInfo.FieldInfo.Name)}Changed?.Invoke();")
                    .Append('\n');

            sb.Append(@"
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
                if (parameters.Length > 0) caseSuffix = " thing";

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
            sb.Append(@"
public class ").Append(Suffix(logicInfo.Type.Name, "Entity")).Append(@"
{
    public Action? OnDestroyed;
    public Action? OnSpawned;
");

            AddChangeDelegates(sb, logicInfo, logicInfo.FieldInfos);
            AddActionDelegates(sb, logicInfo.CommandInfos);


            sb.Append(@"
}

");
        }

        public static void AddInternalEntity(StringBuilder sb, LogicInfo logicInfo)
        {
            sb.Append(@"
public class ").Append(EntityGeneratedInternal(logicInfo)).Append(" : IGeneratedEntity").Append(@"
{
");
            AddInternalMembers(sb);


            sb.Append("    ").Append(FullName(logicInfo.Type)).Append(@" current;
    ").Append(FullName(logicInfo.Type)).Append(@" last;

").Append($"    public {FullName(logicInfo.Type)} Self => current;").Append(@"

").Append($" internal {FullName(logicInfo.Type)} Current").Append(@"
            {
                set => current = value;
            }
");

            sb.Append($"     {Suffix(logicInfo.Type.Name, "Entity")} outFacing = new();").Append(@"
").Append($"    public {Suffix(logicInfo.Type.Name, "Entity")} OutFacing => outFacing;").Append(@"

");

            AddArchetypeId(sb, logicInfo);

            AddChangeMaskConstants(sb, logicInfo.FieldInfos);


            // ----- methods ----

            AddInternalMethods(sb);

            DoActions(sb, logicInfo.CommandInfos);
            UnDoActions(sb, logicInfo.CommandInfos);

            AddSerialize(sb, logicInfo.FieldInfos);
            AddSerializeAll(sb, logicInfo.FieldInfos);

            AddDeserialize(sb, logicInfo.FieldInfos);
            AddDeserializeAll(sb, logicInfo.FieldInfos);

            AddTick(sb, logicInfo);

            AddChanges(sb, logicInfo.FieldInfos);
            AddInvokeChanges(sb, logicInfo.FieldInfos);
            AddTypeInformation(sb, logicInfo.FieldInfos);

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

        public static string Generate(IEnumerable<LogicInfo> infos)
        {
            var sb = new StringBuilder();

            sb.Append(@"
using Piot.Surge.OctetSerialize;
using Piot.Surge.TypeSerialization;

namespace Piot.Surge.Internal.Generated
{
");

            AddArchetypeConstants(sb, infos);
            AddEntityCreation(sb, infos);

            AddEngineWorld(sb, infos);
            AddEngineNotifier(sb, infos);

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