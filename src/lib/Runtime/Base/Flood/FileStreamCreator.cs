/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.IO;

namespace Piot.Flood
{
    public static class FileStreamCreator
    {
        public static IDisposableOctetWriter Create(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            var fileStream = new FileStream(path, FileMode.CreateNew);
            return new StreamOctetWriter(fileStream);
        }

        public static IOctetReader Open(string path)
        {
            return OpenWithSeek(path);
        }

        public static IOctetReaderWithSeekAndSkip OpenWithSeek(string path)
        {
            var fileStream = new FileStream(path, FileMode.Open);
            return new StreamOctetReader(fileStream);
        }
    }
}