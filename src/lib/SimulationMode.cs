/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge
{
    public enum TickMode
    {
        Authoritative,
        Ghost,
        Predict
    }

    public readonly struct SimulationMode
    {
        private readonly TickMode tickMode;

        public bool IsAuthoritative => tickMode == TickMode.Authoritative;
        public bool IsGhost => tickMode == TickMode.Ghost;
        public bool IsPredicting => tickMode == TickMode.Predict;

        public SimulationMode(TickMode mode)
        {
            tickMode = mode;
        }
    }
}