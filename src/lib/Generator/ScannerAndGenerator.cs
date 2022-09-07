/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.IO;
using System.Linq;
using Piot.Clog;

namespace Piot.Surge.Generator
{
    public static class ScannerAndGenerator
    {
        public static void ScanAndGenerate(string targetSourceFile, ILog log)
        {
            var allLogics = LogicScanner.ScanForLogics(log);

            var logicInfos = LogicInfoCollector.Collect(allLogics, log);

            var allInputs = GameInputScanner.ScanForGameInputs(log);
            var gameInputInfos = GameInputInfoCollector.Collect(allInputs, log);
            if (gameInputInfos.Count() != 1)
            {
                throw new Exception("must have at least one game input struct");
            }

            var inputFetchers = InputFetchScanner.ScanForInputFetchMethods(log);
            if (inputFetchers.Count() != 1)
            {
                throw new Exception("must have exactly one game input fetcher");
            }

            var inputFetchInfos = GameInputFetchInfoCollector.Collect(inputFetchers, log);
            var code = SourceGenerator.Generate(logicInfos, gameInputInfos.First(), inputFetchInfos.First());

            File.Delete(targetSourceFile);
            var directoryName = Path.GetDirectoryName(targetSourceFile);
            if (directoryName is null)
            {
                return;
            }

            Directory.CreateDirectory(directoryName);
            File.WriteAllText(targetSourceFile, code);
            log.Info("Surge Source Generation Done");
        }
    }
}