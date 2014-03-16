//
//    8-Channel Logic Analyzer
//    Copyright (C) 2014  Bob Foley
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Text;

namespace LogicAnalyzer.Compression
{
    /// <summary>
    /// Class to wrap the functionality of the Compressor and Decompressor classes.
    /// Though this class is not used in the Logic Analyzer code, it is useful
    /// for demonstrating the use of Compressor/Decompressor.
    /// </summary>
    public class CompressionWrapper
    {
        private Compressor cmp;
        private Decompressor dec;
        private List<byte> compressionResult;
        private List<byte> decompressionResult;

        #region Constructors

        /// <summary>
        /// Creates a CompressionWrapper object.
        /// </summary>
        public CompressionWrapper()
        {
            cmp = new Compressor(CompressedByteOut);
            dec = new Decompressor(DecompressedByteOut);
            compressionResult = new List<byte>(1024);
            decompressionResult = new List<byte>(1024);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the data resulting from calls to the Compress() methods.
        /// </summary>
        public byte[] CompressedResult
        {
            get
            {
                return compressionResult.ToArray();
            }
        }

        /// <summary>
        /// Gets the data resulting from calls to the Decompress methods.
        /// </summary>
        public byte[] DecompressedResult
        {
            get
            {
                return decompressionResult.ToArray();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Add a byte to the compression data stream.
        /// </summary>
        /// <param name="Byte">A byte to be added to the compression data stream</param>
        public void Compress(byte Byte)
        {
            cmp.Encode(Byte);
        }

        /// <summary>
        /// Add an array of bytes to the compression data stream.
        /// </summary>
        /// <param name="Bytes">An array of bytes to be added to the compression stream</param>
        public void Compress(byte[] Bytes)
        {
            cmp.Encode(Bytes);
        }

        /// <summary>
        /// Add a string to the compression data stream.
        /// </summary>
        /// <param name="Str">A string to be added to the compression stream.</param>
        public void Compress(string Str)
        {
            foreach (byte b in Str)
                cmp.Encode(b);
        }

        /// <summary>
        /// Add a byte to the decompression data stream.
        /// </summary>
        /// <param name="Byte">A byte to be added to the decompression stream</param>
        public void Decompress(byte Byte)
        {
            dec.Decode(Byte);
        }

        /// <summary>
        /// Add an array of bytes to the decompression data stream.
        /// </summary>
        /// <param name="Bytes">An array of bytes to be added to the decompression stream</param>
        public void Decompress(byte[] Bytes)
        {
            dec.Decode(Bytes);
        }

        /// <summary>
        /// Add a string to the decompression data stream.
        /// </summary>
        /// <param name="Str">A string to be added to the decompression stream.</param>
        public void Decompress(string Str)
        {
            foreach (byte b in Str)
                dec.Decode(b);
        }

        /// <summary>
        /// Flush the compressor and decompressor outputs.
        /// </summary>
        public void Flush()
        {
            cmp.Flush();
            dec.Flush();
        }

        #endregion

        #region Callback Functions

        /// <summary>
        /// Function which is called from the Compressor with each byte of output.
        /// </summary>
        /// <param name="Byte">A byte of compressed output</param>
        private void CompressedByteOut(byte Byte)
        {
            compressionResult.Add(Byte);
        }

        /// <summary>
        /// Function which is called from the Decompressor with each byte of output.
        /// </summary>
        /// <param name="Byte">A byte of decompressed output</param>
        private void DecompressedByteOut(byte Byte)
        {
            decompressionResult.Add(Byte);
        }

        #endregion
    }
}
