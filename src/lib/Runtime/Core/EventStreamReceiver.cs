/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/



using System;
using Piot.Flood;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global

namespace Piot.Surge.Core
{
    public static class EventStreamReceiver
    {
        // Generated code sets the delegate on init
        // Do not edit or set this values
        public static Action<IBitReader, uint, IEventReceiver>? receiveFull;
        public static Action<IBitReader, uint>? skip;

        public static void ReceiveFull(IBitReader bitReader, uint eventTypeId, IEventReceiver eventReceiver)
        {
            if (receiveFull is null)
            {
                throw new("unknown receive full");
            }

            receiveFull(bitReader, eventTypeId, eventReceiver);
        }

        public static void Skip(IBitReader reader, uint eventTypeId)
        {
            if (skip is null)
            {
                throw new("unknown skip");
            }

            skip(reader, eventTypeId);
        }
    }
}