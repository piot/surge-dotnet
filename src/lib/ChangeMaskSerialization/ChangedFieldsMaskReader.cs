/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;
using Piot.Surge.ChangeMask;

namespace Piot.Surge.ChangeMaskSerialization
{
    public static class ChangedFieldsMaskReader
    {
        /// <summary>
        ///     Reads a <see cref="ChangedFieldsMask" /> from an octet stream.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static ChangedFieldsMask ReadChangedFieldMask(IOctetReader reader)
        {
#if DEBUG
            if (reader.ReadUInt8() != Constants.FullChangeMaskSync)
            {
                throw new Exception("out of sync");
            }
#endif
            var v = reader.ReadUInt16();
            if ((v & 0x8000) == 0)
            {
                return new ChangedFieldsMask { mask = v };
            }

            var completeMask = (ulong)(v & 0x7fff);

            var next = reader.ReadUInt16();
            completeMask <<= 15;
            completeMask |= next;

            return new ChangedFieldsMask { mask = completeMask };
        }
    }
}