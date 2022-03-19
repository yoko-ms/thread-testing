// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace TestThreading.CosmosDB
{
    /// <summary>
    /// Represents a utility that handles compress and decompress work.
    /// </summary>
    public static class CompressionUtil
    {
        /// <summary>
        /// Compress the text to reduce its size.
        /// </summary>
        /// <param name="originalText">The original text.</param>
        /// <returns>The compressed text that is smaller in size.</returns>
        public static string Compress(string originalText)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(originalText);
            using (var memoryStream = new MemoryStream())
            {
                // Compress the buffer and write to memoryStream
                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
                {
                    gZipStream.Write(buffer, offset: 0, count: buffer.Length);
                }

                // Copy the data from memoryStream and paste to compressedData
                memoryStream.Position = 0;
                var compressedData = new byte[memoryStream.Length];
                memoryStream.Read(compressedData, offset: 0, count: compressedData.Length);

                // Create a new array with size 4 + n 
                // The first 4 elements stores the length of compressedData
                // The rest of n elements stores the actual compressedData
                var gZipBuffer = new byte[compressedData.Length + 4];
                Buffer.BlockCopy(src: compressedData, srcOffset: 0, dst: gZipBuffer, dstOffset: 4, count: compressedData.Length);
                Buffer.BlockCopy(src: BitConverter.GetBytes(buffer.Length), srcOffset: 0, dst: gZipBuffer, dstOffset: 0, count: 4);

                return Convert.ToBase64String(gZipBuffer);
            }
        }

        /// <summary>
        /// Decompress the compressed text to its original form.
        /// </summary>
        /// <param name="compressedText">The compressed text.</param>
        /// <returns>The original form of the text.</returns>
        public static string Decompress(string compressedText)
        {
            byte[] gZipBuffer = Convert.FromBase64String(compressedText);
            using (var memoryStream = new MemoryStream())
            {
                // Get the the data length which is represented by the first four bytes 
                int dataLength = BitConverter.ToInt32(gZipBuffer, startIndex: 0);

                // Get the actual compressedData from the rest of elements and write it to memoryStream
                memoryStream.Write(gZipBuffer, offset: 4, count: gZipBuffer.Length - 4);

                // Create the container for the result with the right data length
                var buffer = new byte[dataLength];

                // Write the decompressed data to buffer
                memoryStream.Position = 0;
                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    gZipStream.Read(buffer, offset: 0, count: buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
            }
        }
    }
}
