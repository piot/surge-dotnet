/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using Piot.Surge.LogicalInput;

namespace Piot.Surge.Pulse.Client
{
    public static class LocalPlayerLogicalInputBundler
    {
        public static LogicalInputsForAllLocalPlayers BundleInputForAllLocalPlayers(
            LocalPlayerInput[] localPlayerInputs)
        {
            var inputForAllPlayers = new LogicalInputArrayForPlayer[localPlayerInputs.Length];
            var index = 0;
            foreach (var localPlayerInput in localPlayerInputs)
            {
                var allItems = localPlayerInput.AvatarPredictor.EntityPredictor.PredictCollection.Items;
                var inputsForLocal = new List<LogicalInput.LogicalInput>();
                foreach (var item in allItems)
                {
                    if (item.tickId.tickId == 0)
                    {
                        throw new("not good");
                    }

                    inputsForLocal.Add(new(localPlayerInput.LocalPlayerIndex, item.tickId, item.inputPack.Span));
                }

                inputForAllPlayers[index].inputs = inputsForLocal.ToArray();
                index++;
            }

            return new(inputForAllPlayers);
        }
    }
}