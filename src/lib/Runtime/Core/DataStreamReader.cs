/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/



using Piot.Flood;

namespace Piot.Surge.Core
{
    public static class DataStreamReader
    {
        public static T CreateAndRead<T>(IBitReader bitReader) where T : struct
        {
            if (DataReader<T>.read is null)
            {
                throw new("generated code has not set delegate read");
            }

            return DataReader<T>.read(bitReader);
        }

        public static uint ReadMask<T>(IBitReader bitReader, ref T data) where T : struct
        {
            if (DataReader<T>.readMask is null)
            {
                throw new("generated code has not set delegate readMask");
            }

            return DataReader<T>.readMask(bitReader, ref data);
        }
    }
}