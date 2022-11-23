/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/



// ReSharper disable UnassignedField.Global
namespace Piot.Surge.Core
{
    public static class DataDiffer<T> where T : struct
    {
        public delegate uint DiffDelegate(in T a, in T b);
        public static DiffDelegate? diff;
    }
}