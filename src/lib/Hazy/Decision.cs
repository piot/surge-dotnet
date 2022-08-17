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
        int dropPercentage;
        int tamperPercentage;
        int duplicatePercentage;

        public Decision(int drop, int tamper, int duplicate)
        {
            SetPercentages(drop, tamper, duplicate);
        }

        public void SetPercentages(int drop, int tamper, int duplicate)
        {
            dropPercentage = drop;
            tamperPercentage = tamper;
            duplicatePercentage = duplicate;
            
            var sum = drop + tamper + duplicate;
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