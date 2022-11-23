/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Transport
{
    public interface ITransportClient
    {
        public void SendToHost(ReadOnlySpan<byte> payload);
        public ReadOnlySpan<byte> ReceiveFromHost();
    }
}