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
    /// Class defining methods for an compression data filter.
    /// </summary>
    public class CompressionFilter : AbstractDataFilter<byte>
    {
        private Compressor compressor;
        private bool inCompressionMode;

        #region Constructors

        /// <summary>
        /// Creates and initializes a CompressionFilter object.
        /// </summary>
        public CompressionFilter()
        {
            this.Initialize();
        }

        #endregion

        #region Overridden Methods

        /// <summary>
        /// Re-initialize the filter and clear the compressor.
        /// </summary>
        public override void Initialize()
        {
            compressor = new Compressor(ReceiveCompressedByte);
            inCompressionMode = false;
        }

        /// <summary>
        /// Write a value to the compression filter. Delimiters will be discarded and
        /// the data within them is compressed before being sent to the output.
        /// </summary>
        /// <param name="Data">The data value being sent through the filter</param>
        public override void Write(byte Value)
        {
            // NOTE: This needs more work -- tag writer, etc. CompressionFilter was never tested!

            if(inCompressionMode)
            {
                // If we're in compression mode, send the byte through the compressor.
                // The compressor will generate output bytes through the ReceiveCompressedByte() callback function below.
                compressor.Encode(Value);
            }
            else
            {
                // If we're not in compression mode, just pass the data through.
                base.Write(Value);
            }
        }

        /// <summary>
        /// Flush the compressor.
        /// </summary>
        public override void Flush()
        {
            if(inCompressionMode)
                compressor.Flush();
            base.Flush();
        }

        #endregion

        #region Methods

#if false // NOTE: Don't need...
        public override int Read(byte[] Buffer, int MaxLength)
        {
            int cnt = 0;

            if(!this.DataReady)
                throw new Exception("CompressionFilter.Read: Filter is empty");

            while(cnt < MaxLength && this.DataLength > 0)
                Buffer[cnt++] = this.Read();

            return cnt;
        }

        public override void Write(byte[] Buffer)
        {
            foreach(byte b in Buffer)
                this.Write(b);
        }
#endif

        /// <summary>
        /// Callback function for the compressor to send output bytes to the output of the filter.
        /// </summary>
        /// <param name="Value"></param>
        private void ReceiveCompressedByte(byte value)
        {
            base.Write(value);
        }

        #endregion
    }
}