/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;
using Piot.Surge.LocalPlayer;

namespace Piot.Surge.LogicalInput
{
    public sealed class InputPackFetch<GameInputT> : IInputPackFetch
    {
        readonly OctetWriter cachedWriter = new(256);
        readonly Func<LocalPlayerIndex, GameInputT> gameSpecificFetch;
        readonly Action<IOctetWriter, GameInputT> writer;

        public InputPackFetch(Func<LocalPlayerIndex, GameInputT> gameSpecificFetch,
            Action<IOctetWriter, GameInputT> writer)
        {
            this.gameSpecificFetch = gameSpecificFetch;
            this.writer = writer;
        }

        public ReadOnlySpan<byte> Fetch(LocalPlayerIndex index)
        {
            var gameInput = gameSpecificFetch(index);

            cachedWriter.Reset();

            writer(cachedWriter, gameInput);

            return cachedWriter.Octets;
        }
    }
}