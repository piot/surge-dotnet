/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Reflection;
using Piot.Clog;

namespace Piot.Surge.Generator
{
    public class GameInputFetchInfo
    {
        public GameInputFetchInfo(MethodInfo methodInfo)
        {
            MethodInfo = methodInfo;
        }

        public MethodInfo MethodInfo { get; }

        public override string ToString()
        {
            return $"[GameInputFetchInfo {MethodInfo}]";
        }
    }

    public static class GameInputFetchInfoCollector
    {
        /// <summary>
        ///     Scans the specified <paramref name="types" /> and checks if the type contains a method called Tick.
        /// </summary>
        /// <param name="types"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static IEnumerable<GameInputFetchInfo> Collect(IEnumerable<MethodInfo> methodInfos, ILog log)
        {
            var gameInputFetchInfos = new List<GameInputFetchInfo>();
            foreach (var methodInfo in methodInfos)
            {
                var gameInputFetch = new GameInputFetchInfo(methodInfo);
                gameInputFetchInfos.Add(gameInputFetch);
                log.Info("Found game input fetcher {GameInputFetchInfo}", gameInputFetch);
            }

            return gameInputFetchInfos;
        }
    }
}