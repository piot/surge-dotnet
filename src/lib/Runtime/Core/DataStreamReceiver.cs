/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;

// ReSharper disable UnassignedField.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Piot.Surge.Core
{
    public static class DataStreamReceiver
    {
        // Generated code sets the delegate on init
        // Do not edit or set this values
        public static Action<IBitReader, uint, uint, IDataReceiver>? receiveNew;
        public static Action<IBitReader, uint, uint, IDataReceiver>? receiveUpdate;
        public static Action<uint, uint, IDataReceiver>? receiveDestroy;

        public static void ReceiveNew(
            IBitReader reader,
            uint entityId,
            uint dataTypeId,
            IDataReceiver receiver)
        {
            if (receiveNew is null)
            {
                throw new ArgumentNullException(nameof(receiver));
            }

            receiveNew(reader, entityId, dataTypeId, receiver);
        }

        public static void ReceiveUpdate(
            IBitReader reader,
            uint entityId,
            uint dataTypeId,
            IDataReceiver receiver)
        {
            if (receiveUpdate is null)
            {
                throw new("generated code has not set delegate");
            }

            receiveUpdate(reader, entityId, dataTypeId, receiver);
        }

        public static void ReceiveDestroy(uint entityId, uint dataTypeId, IDataReceiver dataReceiver)
        {
            if (receiveDestroy is null)
            {
                throw new("generated code has not set delegate receiveDestroy");
            }

            receiveDestroy(entityId, dataTypeId, dataReceiver);
        }
    }
}