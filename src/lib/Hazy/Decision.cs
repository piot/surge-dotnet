/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace Piot.Hazy
{
    public readonly struct PartsPerTenThousand
    {
        public readonly uint parts;
        public const uint Divisor = 10000;

        public PartsPerTenThousand(uint parts)
        {
            if (parts > Divisor)
            {
                throw new ArgumentOutOfRangeException(nameof(parts), $"Illegal partsPerTenThousand {parts}");
            }

            this.parts = parts;
        }

        public PartsPerTenThousand(double chance) : this((uint)(chance * Divisor))
        {
        }

        public uint Value => parts;

        public override string ToString()
        {
            return $"[pptt {parts} out of {Divisor}  ({parts / (float)Divisor}]";
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
        public uint threshold;
    }

    public sealed class Decision
    {
        readonly List<Threshold> thresholds = new();

        public Decision(double drop, double tamper, double duplicate, double reorder)
        {
            SetChances(drop, tamper, duplicate, reorder);
        }

        public void SetChances(double dropChance, double tamperChance, double duplicateChance, double reorderChance)
        {
            thresholds.Clear();
            var drop = new PartsPerTenThousand(dropChance).Value;
            var tamper = new PartsPerTenThousand(tamperChance).Value;
            var duplicate = new PartsPerTenThousand(duplicateChance).Value;
            var reorder = new PartsPerTenThousand(reorderChance).Value;

            var sum = drop + tamper + duplicate + reorder;
            if (sum > PartsPerTenThousand.Divisor)
            {
                throw new("illegal sum");
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

            thresholds.Add(new() { packetAction = PacketAction.Normal, threshold = PartsPerTenThousand.Divisor });
        }

        /// <summary>
        ///     Given the value passed in <paramref name="partsPerTenThousand" />, it will return the corresponding
        ///     <see cref="PacketAction" />.
        ///     The value passed to <paramref name="partsPerTenThousand" /> should be random or pseudo-random.
        /// </summary>
        /// <param name="partsPerTenThousand"></param>
        /// <returns></returns>
        public PacketAction Decide(PartsPerTenThousand partsPerTenThousand)
        {
            foreach (var threshold in thresholds)
            {
                if (partsPerTenThousand.parts < threshold.threshold)
                {
                    return threshold.packetAction;
                }
            }

            return PacketAction.Normal;
        }
    }
}