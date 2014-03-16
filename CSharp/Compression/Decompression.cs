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
    /// Class definining methods for LZW-style data decompression. It was experimentally
    /// determined that 13-bit codes provide the best compression for logic analyzer
    /// data.
    /// </summary>
    public class Decompressor
    {
        private ushort freeEntry;
        private Stack<byte> stack;
        private ushort[] prefix;
        private byte[] suffix;
        private Queue<byte> inQueue;
        private ushort outBits;
        private int crc;
        private ushort prevEnt;
        private byte outByte;
        private short inBits;
        private bool firstEntry = true; // NOTE: these could be the reason for compression not working 2 in a row (need initialize method).
        private int curCode = 0; // NOTE: ""    ""
        private ushort ent = 0; // NOTE: ""    ""


        // Definition for callback function for each output byte. An interface is not used
        // here to keep compatibility with the C version of the code running on the Micro.
        public delegate void OutputByte(byte b);
        private OutputByte Callback;

        #region Constructors

        /// <summary>
        /// Creates a Decompressor object for use in decompressing data dynamically.
        /// </summary>
        /// <param name="Callback">A function that will be called for each compressed
        /// output byte.</param>
        public Decompressor(OutputByte Callback)
        {
            if (Callback == null)
                throw new Exception("A Callback function must be specified");

            this.Callback = Callback;

            stack = new Stack<byte>(8192);
            prefix = new ushort[Compressor.MaxCode];
            suffix = new byte[Compressor.MaxCode];

            // Just a small buffer to hold input bytes between calls to Decode().
            inQueue = new Queue<byte>();

            outBits = 0;
            inBits = Compressor.nBits;
            crc = 0;
            freeEntry = Compressor.FirstCode;

            for (short i = 255; i >= 0; i--)
            {
                prefix[i] = 0;
                suffix[i] = (byte)i;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Decodes (decompresses) a sequence of bytes.
        /// </summary>
        /// <param name="Data">An array of bytes to be decoded (decompressed)</param>
        public void Decode(byte[] Data)
        {
            foreach (byte b in Data)
            {
                this.Decode(b);
                curCode++;
            }
        }

        /// <summary>
        /// Decodes (decompresses) a byte of data.
        /// </summary>
        /// <param name="Data">A byte to be decoded (decompressed)</param>
        public void Decode(byte Data)
        {
            inQueue.Enqueue(Data);

            if (getEntry())
            {
                if (ent > Compressor.MaxCode)
                    throw new Exception("Invalid code 1");

                if (firstEntry)
                {
                    outByte = (byte)(ent & 0xff);
                    prevEnt = ent;
                    crc += outByte;
                    Callback(outByte);
                    firstEntry = false;
                }
                else
                {
                    ushort code = ent;

                    if (code == Compressor.ClearCode)
                    {
                        for (short i = 255; i >= 0; i--)
                            prefix[i] = 0;
                        freeEntry = Compressor.FirstCode - 1;
                        return;
                    }

                    if (code >= freeEntry)
                    {
                        stack.Push(outByte);
                        code = prevEnt;
                    }

                    while (code > 0xff)
                    {
                        if (code >= Compressor.MaxCode)
                            throw new Exception("Invalid code 2");
                        stack.Push(suffix[code]);
                        code = prefix[code];
                    }

                    outByte = suffix[code];
                    stack.Push(outByte);

                    while (stack.Count > 0)
                    {
                        byte ch = stack.Pop();
                        Callback(ch);
                        crc += ch;
                    }

                    if (freeEntry < Compressor.MaxCode)
                    {
                        code = freeEntry++;
                        prefix[code] = prevEnt;
                        suffix[code] = outByte;
                    }
                    prevEnt = ent;
                }
            }
        }

        /// <summary>
        /// Flushes any remaining data.
        /// </summary>
        public void Flush()
        {
        }

        /// <summary>
        /// Get the next entry to decode.
        /// </summary>
        /// <returns>Return 'true' if we've read a full entry</returns>
        private bool getEntry()
        {
            byte inByte;

            if (inBits == Compressor.nBits)
                ent = 0;

            while (inQueue.Count > 0)
            {
                inByte = inQueue.Dequeue();
                if (inBits <= (8 - outBits))
                {
                    ent = (ushort)((ent << inBits) | (inByte >> (8 - inBits)));
                    outBits += (ushort)inBits;

                    // If we have bits left over, put the (masked) input byte back in the queue.
                    if (outBits == 8)
                        outBits = 0;
                    else
                    {
                        // If we have bits left over, put the (masked) input byte back in the queue.
                        inQueue.Enqueue((byte)(inByte & Compressor.mask[8 - inBits]));
                    }

                    inBits = Compressor.nBits;
                    return true;
                }
                else
                {
                    ent = (ushort)((ent << (8 - outBits)) | inByte);
                    inBits -= (short)(8 - outBits);
                    outBits = 0;
                }
            }

            return false;
        }

        #endregion
    }
}
