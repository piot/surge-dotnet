/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Flood;
using Piot.Raff.Stream;
using Piot.Surge.Tick.Serialization;

namespace Piot.Surge.Replay.Serialization
{
    public static class CompleteStateScanner
    {
        public static CompleteStateEntry[] ScanForAllCompleteStatePositions(RaffReader tempRaffReader,
            IOctetReaderWithSeekAndSkip readerWithSeek)
        {
            List<CompleteStateEntry> entries = new();

            while (true)
            {
                var positionBefore = readerWithSeek.Position;
                var octetLength = tempRaffReader.ReadChunkHeader(out var icon, out var name);
                if (octetLength == 0)
                {
                    break;
                }

                var positionAfterHeader = readerWithSeek.Position;
                if (icon.Value == Constants.CompleteStateIcon.Value)
                {
                    var packType = readerWithSeek.ReadUInt8();
                    if (packType != 0x02)
                    {
                        throw new Exception("wrong");
                    }

                    var tickId = TickIdReader.Read(readerWithSeek);
                    entries.Add(new(tickId.tickId, positionBefore));
                }

                readerWithSeek.Seek(positionAfterHeader + octetLength);
            }

            return entries.ToArray();
        }
    }
}