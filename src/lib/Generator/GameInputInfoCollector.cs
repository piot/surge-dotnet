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
    public sealed class GameInputFieldInfo
    {
        public GameInputFieldInfo(FieldInfo fieldInfo, ulong mask)
        {
            FieldInfo = fieldInfo;
            Mask = mask;
        }

        public ulong Mask { get; }

        public FieldInfo FieldInfo { get; }

        public override string ToString()
        {
            return $"[inputField {FieldInfo.Name} {FieldInfo.FieldType}]";
        }
    }

    public sealed class GameInputInfo
    {
        public GameInputInfo(Type type, ILog log)
        {
            Type = type;
            ulong mask = 1;

            var tempList = new List<GameInputFieldInfo>();
            var fieldsInInput = type.GetFields();
            foreach (var fieldInInput in fieldsInInput)
            {
                tempList.Add(new(fieldInInput, mask));
                mask <<= 1;
            }

            FieldInfos = tempList.ToList();
        }

        public IEnumerable<GameInputFieldInfo> FieldInfos { get; }

        public Type Type { get; }

        public override string ToString()
        {
            var fieldInfosString = FieldInfos.Aggregate("\n ", (current, command) => current + "\n " + command);

            return $"[gameInputInfo {Type} {fieldInfosString}]";
        }
    }


    public static class GameInputInfoCollector
    {
        /// <summary>
        ///     Scans the specified <paramref name="types" /> and checks if the type contains a method called Tick.
        /// </summary>
        /// <param name="types"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static IEnumerable<GameInputInfo> Collect(IEnumerable<Type> types, ILog log)
        {
            var gameInputs = new List<GameInputInfo>();
            foreach (var type in types)
            {
                var gameInput = new GameInputInfo(type, log);
                gameInputs.Add(gameInput);
                log.Info("Found gameInput {GameInputInfo}", gameInput);
            }

            return gameInputs;
        }
    }
}