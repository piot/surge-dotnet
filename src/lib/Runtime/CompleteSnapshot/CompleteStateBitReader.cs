/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Flood;
using Piot.Surge.Core;
using Piot.Surge.Event;

namespace Piot.Surge.CompleteSnapshot
{
    public static class CompleteStateBitReader
    {
        public static EventSequenceId ReadAndApply(IBitReader reader,
            IDataReceiver entityGhostContainerWithCreator,
            IEventReceiver eventProcessor, ILog log, bool useEvents = true)
        {
#if DEBUG
            BitMarker.AssertMarker(reader, Constants.CompleteSnapshotStartMarker);
#endif
            CompleteStateEntityBitReader.Apply(reader, entityGhostContainerWithCreator, log);

            return useEvents
                ? EventCompleteBitReader.ReadAndApply(eventProcessor, reader)
                : EventCompleteBitReader.Skip(reader);
        }
    }
}