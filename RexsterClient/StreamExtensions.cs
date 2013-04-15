namespace Rexster
{
    using System;
    using System.IO;

    internal static class StreamExtensions
    {
        public static void Skip(this Stream stream, long bytes)
        {
            var bytesSkipped = 0L;
            var bufferSize = (int)Math.Min(bytes, 8 * 1024 * 1024);

            while (bytesSkipped < bytes)
            {
                var buffer = new byte[bufferSize];
                bytesSkipped += stream.Read(buffer, 0, bufferSize);
            }
        }
    }
}