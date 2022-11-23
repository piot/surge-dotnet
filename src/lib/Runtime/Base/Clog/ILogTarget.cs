/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Clog
{
    public interface ILogTarget
    {
        public void Log(LogLevel level, string prefix, string message, object[] args);
    }
}