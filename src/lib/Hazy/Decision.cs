/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Piot.Hazy
{
    public struct Percentage
    {
        public readonly int percentage;

        public Percentage(int percentage)
        {
            if (percentage < 0 || percentage > 100)
            {
                throw new Exception($"Illegal percentage {percentage}");
            }

            this.percentage = percentage;
        }
    }

    public enum PacketAction
    {
        Drop,
        Tamper,
        Duplicate,
        Reorder,
        Normal
    }

    public struct Threshold
    {
        public PacketAction packetAction;
        public int threshold;
    }

    public class Decision
    {
        private readonly List<Threshold> thresholds = new();
        private int dropPercentage;
        private int duplicatePercentage;
        private int reorderPercentage;
        private int tamperPercentage;

        public Decision(int drop, int tamper, int duplicate, int reorder)
        {
            SetPercentages(drop, tamper, duplicate, reorder);
        }

        public void SetPercentages(int drop, int tamper, int duplicate, int reorder)
        {
            dropPercentage = drop;
            tamperPercentage = tamper;
            duplicatePercentage = duplicate;
            reorderPercentage = reorder;

            var sum = drop + tamper + duplicate + reorder;
            if (sum > 100)
            {
                throw new Exception("illegal values");
            }

            if (drop > 0)
            {
                thresholds.Add(new() { packetAction = PacketAction.Drop, threshold = drop });
            }

            if (tamper > 0)
            {
                thresholds.Add(new() { packetAction = PacketAction.Tamper, threshold = drop + tamper });
            }

            if (duplicate > 0)
            {
                thresholds.Add(new() { packetAction = PacketAction.Duplicate, threshold = drop + tamper + duplicate });
            }

            if (reorder > 0)
            {
                thresholds.Add(new()
                    { packetAction = PacketAction.Reorder, threshold = drop + tamper + duplicate + reorder });
            }

            thresholds.Add(new() { packetAction = PacketAction.Normal, threshold = 100 });
        }

        public PacketAction Decide(Percentage percentage)
        {
            foreach (var threshold in thresholds.Where(threshold => percentage.percentage < threshold.threshold))
            {
                return threshold.packetAction;
            }

            return PacketAction.Normal;
        }
    }
}