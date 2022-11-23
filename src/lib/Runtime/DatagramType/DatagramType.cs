/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.DatagramType
{
    public enum DatagramType
    {
        Reserved,
        DeltaSnapshots, // Sent from simulating, arbitrating host to client
        PredictedInputs // Sent from client to host
    }
}