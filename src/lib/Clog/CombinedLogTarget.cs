/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;

namespace Piot.Clog
{
    public sealed class CombinedLogTarget : ILogTarget
    {
        readonly IEnumerable<ILogTarget> logTargets;

        public CombinedLogTarget(IEnumerable<ILogTarget> logTargets)
        {
            this.logTargets = logTargets;
        }

        public void Log(LogLevel level, string prefix, string message, object[] args)
        {
            foreach (var target in logTargets)
            {
                target.Log(level, prefix, message, args);
            }
        }
    }
}