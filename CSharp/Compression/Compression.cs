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
    /// Class definining methods for LZW-style data compression. It was experimentally
    /// determined that 13-bit codes provide the best compression for logic analyzer
    /// data.
    /// </summary>
    public class Compressor
    {
        internal const ushort MinBits = 9;
        internal const ushort MaxBits = 15;
        internal const ushort ClearCode = 256;
        internal const ushort FirstCode = 257;
        internal const short nBits = 13; // Experimentally determined for best compression.
        internal const int MaxCode = (1 << nBits) - 1;
        //
        internal static ushort[] mask = { 0x00, 0x01, 0x03, 0x07, 0x0f, 0x1f, 0x3f, 0x7f, 0xff, 0x1ff, 0x3ff, 0x7ff, 0xfff, 0x1fff, 0x3fff, 0x7fff, 0xffff };

        // Prime numbers for optimal hash-table size depending on encoded bits length (9-15).
        internal static ushort[] primes = { 601, 1501, 2801, 5003, 9001, 18013, 35023 };

        private int hashSize;
        private int[] hashTable; // 32-bit hash table.
        private ushort[] codeTable; // 16-bit code table.
        private short shift;
        private ushort freeEntry;
        private int crc;
        private ushort ent;
        private bool firstByte = true;
        private ushort outBits;
        private byte outByte;

        // Definition for callback function for each output byte. An interface is not used
        // here to keep compatibility with the C version of the code running on the Micro.
        public delegate void OutputByte(byte b);
        private OutputByte Callback;

        #region Constructors

        /// <summary>
        /// Creates a Compressor object for use in compressing data dynamically.
        /// </summary>
        /// <param name="Callback">A function that will be called for each decompressed
        /// output byte.</param>
        public Compressor(OutputByte Callback)
        {
            if (nBits < MinBits || nBits > MaxBits)
                throw new Exception("Invalid value for 'nBits'");
            if (Callback == null)
                throw new Exception("A Callback function must be specified");

            this.Callback = Callback;

            hashSize = primes[nBits - MinBits];
            hashTable = new int[hashSize];
            codeTable = new ushort[hashSize];

            short j = 0;
            for (int fc = hashSize; fc < 65536L; fc *= 2)
                j++;
            shift = (short)(8 - j);

            ClearHash();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clears the hash table.
        /// </summary>
        private void ClearHash()
        {
            // Note: Use memset() in C.
            // memset(hashTable, 0xff, hashSize * sizeof(int));
            for (int i = 0; i < hashSize; i++)
                hashTable[i] = -1;

            freeEntry = FirstCode;
        }

        /// <summary>
        /// Encodes (compresses) a sequence of bytes.
        /// </summary>
        /// <param name="Data">An array of bytes to be encoded (compressed)</param>
        public void Encode(byte[] Data)
        {
            foreach (byte b in Data)
                this.Encode(b);
        }

        /// <summary>
        /// Encodes (compresses) a byte of data.
        /// </summary>
        /// <param name="b">A byte to be encoded (compressed)</param>
        public void Encode(byte Data)
        {
            crc += Data;

            if (firstByte)
            {
                firstByte = false;
                ent = Data;
            }
            else
            {
                int hashIndex = ((Data << shift) ^ ent);
                int hashCode = (ent << 16 | Data);  // 32-bit hash code.

                if (hashTable[hashIndex] == hashCode)
                {
                    // Found the current code in the hash table, save the result and return.
                    ent = codeTable[hashIndex];
                    return;
                }
                else if (hashTable[hashIndex] >= 0)
                {
                    // A code was found in the hash table, but it is not the one we were looking for.
                    // Probe until the code is found.
                    int hashDisplacement = (short)(hashIndex == 0 ? 1 : hashSize - hashIndex);

                    if (hashDisplacement < 0)
                        hashDisplacement = -hashDisplacement;

                    while (true)
                    {
                        hashIndex -= hashDisplacement;

                        if (hashIndex < 0)
                            hashIndex += hashSize;

                        if (hashTable[hashIndex] == hashCode)
                        {
                            ent = codeTable[hashIndex];
                            return;
                        }

                        if (hashTable[hashIndex] <= 0)
                            break;
                    }
                }

                SendOutputCode(ent);

                ent = Data;
                if (freeEntry < MaxCode)
                {
                    codeTable[hashIndex] = freeEntry++;
                    hashTable[hashIndex] = hashCode;
                }
                else
                {
                    SendOutputCode(ClearCode);
                    ClearHash();
                }
            }
        }

        /// <summary>
        /// Flushes any remaining data and adds a terminator.
        /// </summary>
        public void Flush()
        {
            SendOutputCode(ent);
            SendOutputCode((ushort)(0xffff & mask[nBits]));
        }

        /// <summary>
        /// Check if 'outbits' is full. If it is, send the byte to the output.
        /// </summary>
        private void CheckIfOutbitsFull()
        {
            if (outBits == 8)
            {
                Callback(outByte);
                outBits = 0;
                outByte = 0;
            }
        }

        /// <summary>
        /// Send an output code. This may result in one or more bytes being
        /// send to the output. Left-over bits are retained for the next
        /// call to this function.
        /// </summary>
        /// <param name="ch">Output code</param>
        private void SendOutputCode(ushort ch)
        {
            short b = nBits;

            while (true)
            {
                if (b <= (8 - outBits))
                {
                    outByte = (byte)((outByte << b) | ch);
                    outBits += (ushort)b;
                    CheckIfOutbitsFull();
                    break;
                }
                else
                {
                    outByte = (byte)(outByte << (8 - outBits));
                    b -= (byte)((8 - outBits));
                    outByte |= (byte)(ch >> b);

                    /* Mask off the used bits. */
                    ch &= mask[b];

                    outBits = 8;
                    CheckIfOutbitsFull();
                }
            }
        }

        #endregion
    }
}
