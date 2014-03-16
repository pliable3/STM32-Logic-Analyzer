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
using System.IO;
using LogicAnalyzer.Compression;

namespace LogicAnalyzer.Filters
{
    /// <summary>
    /// Class defining methods for a decompression data filter.
    /// </summary>
    public class DecompressionFilter : AbstractDataFilter<byte>
    {
        // These tags signify the beginning and ending of compressed data.
        // The tags will pass through to the input stream and the inCompressionMode
        // flag will be set/reset accordingly.
        internal static byte[] CompressTagStart = System.Text.Encoding.ASCII.GetBytes("<cmp>");
        internal static byte[] CompressTagStop = System.Text.Encoding.ASCII.GetBytes("</cmp>");

        private Decompressor decompressor;
        private bool inCompressionMode;

        #region Constructors

        /// <summary>
        /// Creates and initializes a DecompressionFilter object.
        /// </summary>
        public DecompressionFilter()
        {
            this.Initialize();
        }

        #endregion

        #region Overridden Methods

        /// <summary>
        /// Re-initialize the filter and clear the decompressor.
        /// </summary>
        public override void Initialize()
        {
            decompressor = new Decompressor(ReceiveDecompressedByte);
            inCompressionMode = false;
        }

        /// <summary>
        /// Write a value, that was previously thought to be part of a delimiter, to
        /// the filter output.
        /// </summary>
        /// <param name="Data">The data value being sent through the filter</param>
        public override void WriteTagTesterValue(byte Data)
        {
            if (inCompressionMode)
                decompressor.Decode(Data);
            else
                base.Write(Data);
        }

        /// <summary>
        /// Write a value to the decompression filter. Delimiters will be discarded and
        /// the data within them is decompressed before being sent to the output.
        /// </summary>
        /// <param name="Data">The data value being sent through the filter</param>
        public override void Write(byte Data)
        {
            if (inCompressionMode)
            {
                // Check for the termination tag.
                if (!this.TagTester.ValueInTagCode(CompressTagStop, Data))
                {
                    // In compression mode, send the byte through the decompressor.
                    // The decompressor will generate output bytes through the ReceiveDecompressedByte() callback function below.
                    decompressor.Decode(Data);
                }
                else if (this.TagTester.Length == CompressTagStart.Length)
                {
                    // Found the 'compression stop tag'. Stop compression mode.
                    decompressor.Flush();
                    inCompressionMode = false;

                    // Throw the </cmp> tag away.
                    this.TagTester.Clear();
                }
            }
            else
            {
                // Check for the start tag.
                if (!this.TagTester.ValueInTagCode(CompressTagStart, Data))
                {
                    // If we're not in compression mode, just pass the data through.
                    base.Write(Data);
                }
                else if (this.TagTester.Length == CompressTagStart.Length)
                {
                    // Found the 'compression start tag'. Start compression mode.

                    // Throw the <cmp> tag away.
                    this.TagTester.Clear();
                    inCompressionMode = true;
                }
            }
        }

        /// <summary>
        /// Flush the decompressor.
        /// </summary>
        public override void Flush()
        {
            if (inCompressionMode)
                decompressor.Flush();
            base.Flush();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Callback function for the decompressor to send output bytes to the output of the filter.
        /// </summary>
        /// <param name="Value"></param>
        private void ReceiveDecompressedByte(byte Value)
        {
            base.Write(Value);
        }

        #endregion
    }
}
