/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Clog
{
    public enum LogLevel
    {
        /// <summary>Something that is logged very frequently and/or on a very low level.</summary>
        LowLevel,
        /// <summary>Normal operation, but logged for debugging purposes</summary>
        Debug,
        /// <summary>Normal operation, information that is useful to the user</summary>
        Info,
        /// <summary>Unusual state, but is expected to happen due to the nature of the operation, e.g. network or file deserialization</summary>
        Notice,
        /// <summary>Something that is most likely a problem, but is handled</summary>
        Warning,
        /// <summary>Error state and it is not expected, nor handled</summary>
        Error
    }
}