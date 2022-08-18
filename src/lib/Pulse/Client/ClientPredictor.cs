/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Flood;
using Piot.MonotonicTime;
using Piot.Surge.LogicalInput;
using Piot.Surge.Snapshot;

namespace Piot.Surge.Pulse.Client
{
    public class ClientPredictor : IClientPredictorCorrections
    {
        private readonly IInputPackFetch inputPackFetch;
        private readonly LogicalInputQueue predictedInputs = new();
        private readonly TimeTicker.TimeTicker predictionTicker;
        private readonly ILog log;
        private TickId predictTickId;

        public ClientPredictor(IInputPackFetch inputPackFetch, Milliseconds now, Milliseconds targetDeltaTimeMs, ILog log)
        {
            this.log = log;
            this.inputPackFetch = inputPackFetch;
            predictionTicker = new(now, PredictionTick, targetDeltaTimeMs,
                log.SubLog("PredictionTick"));
        }

        public void Update(Milliseconds now)
        {
            predictionTicker.Update(now);
        }

        void IClientPredictorCorrections.ReadCorrections(IOctetReader snapshotReader)
        {
            /*
              var correctionStates = SnapshotCorrectionsReader.Read(snapshotReader, predictedEntities);
             
            foreach (var correctionState in correctionStates)
            {
                if (correctionState.Checksum == previousCorrectionState.Checksum)
                {
                    continue;
                }

                // Undo all local prediction up to the point where the mis-predict was detected.
                predictedEntity.RewindToJustBefore(correctionState.tickId);
                predictedStates.DiscardToJustBefore(correctionState.tickId);
                predictedInputs.DiscardUpToAndIncluding(correctionState.TickId);
                
                foreach (var predictedInput in predictedInputs.Collection)
                {
                    predictedEntity.SimulateWithUndo(predictedInput);
                    var captureWriter = new OctetWriter(1200);
                    predictedEntity.SerializeAll(captureWriter);
                    predictedStates.StorePredictionState(predictedInput.appliedAtTickId, captureWriter.Octets);
                }
            }
            */
        }
        
        private void PredictionTick()
        {
            log.Debug("Prediction Tick!");
            var inputOctets = inputPackFetch.Fetch();
            var logicalInput = new LogicalInput.LogicalInput
            {
                appliedAtTickId = predictTickId,
                payload = inputOctets.ToArray(),
            };
            predictedInputs.AddLogicalInput(logicalInput);
            predictTickId = new TickId(predictTickId.tickId + 1);
        }
    }
}