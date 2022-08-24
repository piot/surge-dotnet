/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Flood;
using Piot.MonotonicTime;
using Piot.Surge.LogicalInput;
using Piot.Surge.LogicalInputSerialization;
using Piot.Surge.OrderedDatagrams;
using Piot.Surge.Snapshot;
using Piot.Transport;

namespace Piot.Surge.Pulse.Client
{
    /// <summary>
    ///     Predicts the future of an avatar. If misprediction occurs, it will roll back,
    ///     apply the correction, and fast forward (Roll forth).
    /// </summary>
    public class ClientPredictor : IClientPredictorCorrections
    {
        private readonly OrderedDatagramsOut datagramsOut;
        private readonly Milliseconds fixedSimulationDeltaTimeMs;
        private readonly IInputPackFetch inputPackFetch;
        private readonly ILog log;
        private readonly LogicalInputQueue predictedInputs = new();
        private readonly TimeTicker.TimeTicker predictionTicker;
        private readonly ITransportClient transportClient;
        private TickId predictTickId;

        public ClientPredictor(IInputPackFetch inputPackFetch, ITransportClient transportClient, Milliseconds now,
            Milliseconds targetDeltaTimeMs,
            ILog log)
        {
            this.log = log;
            this.transportClient = transportClient;
            this.inputPackFetch = inputPackFetch;
            fixedSimulationDeltaTimeMs = targetDeltaTimeMs;
            predictionTicker = new(now, PredictionTick, targetDeltaTimeMs,
                log.SubLog("PredictionTick"));
        }

        void IClientPredictorCorrections.ReadCorrections(TickId correctionsForTickId, IOctetReader snapshotReader)
        {
            log.DebugLowLevel("we have corrections for {TickId}, clear old predicted inputs", correctionsForTickId);

            predictedInputs.DiscardUpToAndExcluding(correctionsForTickId);
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


        public void AdjustPredictionSpeed(TickId lastReceivedServerTickId, uint roundTripTimeMs)
        {
            var targetPredictionTicks = roundTripTimeMs / fixedSimulationDeltaTimeMs.ms;

            var tickIdThatWeShouldSendNowInTheory = lastReceivedServerTickId.tickId + targetPredictionTicks;
            const int counterJitter = 2;
            const int counterProcessOrder = 1;
            var tickIdThatWeShouldSendNow = tickIdThatWeShouldSendNowInTheory + counterProcessOrder + counterJitter;

            var predictionDiffInTicks = predictTickId.tickId - tickIdThatWeShouldSendNow;

            var newDeltaTimeMs = predictionDiffInTicks switch
            {
                < 0 => fixedSimulationDeltaTimeMs.ms * 100 / 120,
                > 30 => 0,
                > 0 => fixedSimulationDeltaTimeMs.ms * 100 / 80,
                _ => fixedSimulationDeltaTimeMs.ms
            };

            log.DebugLowLevel("New Prediction Speed {Diff} {NewDeltaTimeMs}", predictionDiffInTicks, newDeltaTimeMs);

            predictionTicker.DeltaTime = new(newDeltaTimeMs);
        }

        public void Update(Milliseconds now)
        {
            predictionTicker.Update(now);
        }

        private void PredictionTick()
        {
            var now = predictionTicker.Now;

            log.Debug("--- Prediction Tick {TickId}", predictTickId);
            var inputOctets = inputPackFetch.Fetch();
            var logicalInput = new LogicalInput.LogicalInput
            {
                appliedAtTickId = predictTickId,
                payload = inputOctets.ToArray()
            };

            log.DebugLowLevel("Adding logical input {LogicalInput}", logicalInput);
            predictedInputs.AddLogicalInput(logicalInput);

            var outDatagram =
                LogicInputDatagramPackOut.CreateInputDatagram(datagramsOut, new TickId(42), 0,
                    now, predictedInputs.Collection);
            log.DebugLowLevel("Sending inputs to host");
            transportClient.SendToHost(outDatagram);

            predictTickId = new TickId(predictTickId.tickId + 1);
        }
    }
}