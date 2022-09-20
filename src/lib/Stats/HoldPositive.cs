/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Stats
{
    public sealed class HoldPositive
    {
        readonly uint thresholdCount;
        uint continuousOffCount;
        bool lastValue;

        public HoldPositive(uint threshold)
        {
            thresholdCount = threshold;
        }

        public bool IsOrWasTrue { get; private set; }

        public bool Value
        {
            set
            {
                lastValue = value;
                if (value)
                {
                    IsOrWasTrue = true;
                }
                else
                {
                    if (!IsOrWasTrue)
                    {
                        return;
                    }

                    continuousOffCount++;
                    if (continuousOffCount > thresholdCount)
                    {
                        IsOrWasTrue = false;
                    }
                }
            }

            get => lastValue;
        }
    }
}