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
            AvatarPredictor[] localAvatarPredictors)
        {
            var inputForAllPlayers = new LogicalInputArrayForPlayer[localAvatarPredictors.Length];
            var index = 0;
            foreach (var localAvatarPredictor in localAvatarPredictors)
            {
                var inputForLocal = localAvatarPredictor.PredictedInputs.Collection;
                inputForAllPlayers[index].inputs = inputForLocal;
                index++;
            }

            return new LogicalInputsForAllLocalPlayers(inputForAllPlayers);
        }
    }
}