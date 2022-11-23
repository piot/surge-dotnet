/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/



namespace Piot.Surge.Core
{
    public static class DataInfo
    {
        public static uint[]? inputComponentTypeIds;
        public static uint[]? logicComponentTypeIds;
        public static uint[]? ghostComponentTypeIds;
    }

    public enum DataType
    {
        Logic,
        Ghost,
        Input,
        Client
    }

    public static class DataMetaInfo<T> where T : struct
    {
        public static DataType dataType;
    }
}