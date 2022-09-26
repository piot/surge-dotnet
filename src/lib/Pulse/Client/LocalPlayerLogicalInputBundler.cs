/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

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
                var inputForLocal = localPlayerInput.AvatarPredictor.EntityPredictor.PredictedInputs.Collection;
                inputForAllPlayers[index].inputs = inputForLocal;
                index++;
            }

            return new(inputForAllPlayers);
        }
    }
}