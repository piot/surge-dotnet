/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Core
{
    public static class DataStreamWriter
    {
        public static void Write<T>(T data, IBitWriter bitWriter) where T : struct
        {
            if (DataWriter<T>.write is null)
            {
                throw new($"unknown type {typeof(T).Name}");
            }

            DataWriter<T>.write(bitWriter, data);
        }

        public static void Write<T>(in T data, IBitWriter bitWriter, uint mask) where T : struct
        {
            if (DataWriter<T>.writeMask is null)
            {
                throw new($"unknown type {typeof(T).Name}");
            }

            DataWriter<T>.writeMask(bitWriter, data, mask);
        }
    }

}