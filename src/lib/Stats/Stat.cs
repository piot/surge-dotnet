/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Stats
{
    public struct Stat
    {
        public int average;
        public uint count;
        public int min;
        public int max;

        private Func<int, string> formatter;

        public Func<int, string> Formatter
        {
            set => formatter = value;
        }

        public Stat(Func<int, string> formatter)
        {
            this.formatter = formatter;
            average = 0;
            count = 0;
            min = 0;
            max = 0;
        }

        public override string ToString()
        {
            return $"{formatter(average)}/s min:{formatter(min)}, max: {formatter(max)} ({count})";
        }
    }
}