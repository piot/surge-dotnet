/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Piot.Clog;

namespace Piot.Surge.Generator
{
    public class CommandInfo
    {
        public CommandInfo(MethodBase methodInfo)
        {
            CommandParameters = methodInfo.GetParameters().Select(arg => new CommandParameter(arg)).ToList();
            MethodInfo = methodInfo;
        }

        public IEnumerable<CommandParameter> CommandParameters { get; }

        public MethodBase MethodInfo { get; }

        public override string ToString()
        {
            var s = CommandParameters.Aggregate("", (current, arg) => current + $"{arg}, ");

            return $"[commandInfo {MethodInfo.Name} {s}]";
        }

        public class CommandParameter
        {
            public string name;
            private ParameterInfo parameterInfo;
            public Type type;

            public CommandParameter(ParameterInfo parameterInfo)
            {
                this.parameterInfo = parameterInfo;
                name = parameterInfo.Name!;
                type = parameterInfo.ParameterType;
            }

            public override string ToString()
            {
                return $"[param {name} {type}]";
            }
        }
    }

    public class LogicFieldInfo
    {
        public LogicFieldInfo(FieldInfo fieldInfo, ulong mask)
        {
            FieldInfo = fieldInfo;
            Mask = mask;
        }

        public ulong Mask { get; }

        public FieldInfo FieldInfo { get; }

        public override string ToString()
        {
            return $"[field {FieldInfo.Name} {FieldInfo.FieldType}]";
        }
    }

    public class LogicInfo
    {
        public LogicInfo(Type type, MethodInfo tickMethod, MethodInfo? setInputMethod, ILog log)
        {
            Type = type;
            TickMethod = tickMethod;
            SetInputMethod = setInputMethod;
            var parameters = tickMethod.GetParameters();
            if (parameters.Length != 1)
            {
                throw new Exception("we can only allow one parameter to Tick()");
            }

            var commandsInterface = parameters.Single().ParameterType;
            if (!commandsInterface.IsInterface)
            {
                throw new Exception("Tick() must take an interface as a single parameter");
            }

            /*
            if (!commandsInterface.IsAssignableTo(typeof(ILogicActions)))
                throw new Exception($"Interface in Tick {type.Name} must inherit from {nameof(ILogicActions)}");
*/
            var methodsInInterface = commandsInterface.GetMethods();
            CommandInfos = methodsInInterface.Select(method => new CommandInfo(method)).ToList();
            CommandsInterface = commandsInterface;
            var fieldsInLogic = type.GetFields();

            ulong mask = 1;

            var tempList = new List<LogicFieldInfo>();
            foreach (var fieldInLogic in fieldsInLogic)
            {
                tempList.Add(new LogicFieldInfo(fieldInLogic, mask));
                mask <<= 1;
            }

            FieldInfos = tempList.ToList();
        }

        public IEnumerable<CommandInfo> CommandInfos { get; }

        public IEnumerable<LogicFieldInfo> FieldInfos { get; }

        public Type CommandsInterface { get; }

        public MethodInfo TickMethod { get; }
        public MethodInfo? SetInputMethod { get; }

        public bool CanTakeInput => SetInputMethod is not null;

        public Type Type { get; }

        public override string ToString()
        {
            var commandInfosString = CommandInfos.Aggregate("\n ", (current, command) => current + "\n " + command);
            var fieldInfosString = FieldInfos.Aggregate("\n ", (current, command) => current + "\n " + command);

            return $"[logicInfo {Type} {TickMethod}  {fieldInfosString} {commandInfosString}]";
        }
    }

    public static class LogicInfoCollector
    {
        /// <summary>
        ///     Scans the specified <paramref name="types" /> and checks if the type contains a method called Tick.
        /// </summary>
        /// <param name="types"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static IEnumerable<LogicInfo> Collect(IEnumerable<Type> types, ILog log)
        {
            var logicInfos = new List<LogicInfo>();
            foreach (var type in types)
            {
                var tickMethodInfo = ScannerHelper.ImplementedMethod(type, "Tick");
                var setInputMethod = type.GetMethod("SetInput");
                if (setInputMethod is not null)
                {
                    if (setInputMethod.IsAbstract)
                    {
                        throw new Exception($"SetInput can not be abstract in type {type.Name}");
                    }
                }

                var logicInfo = new LogicInfo(type, tickMethodInfo, setInputMethod, log);
                logicInfos.Add(logicInfo);
                log.Info("Found logic {LogicInfo}", logicInfo);
            }

            return logicInfos;
        }
    }
}