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

namespace LogicAnalyzer.Filters
{
    /// <summary>
    /// Class defining methods to filter timestamped data. For example, if a data value arrives with a time stamp of 1
    /// and another data value arrives with a time stamp of 6, then the first value will be replicated 5 times and sent
    /// to the output of the filter. A data stream that rarely changes can be greatly compressed using this method.
    /// </summary>
    public class TimestampFilter : AbstractDataFilter<byte>
    {
        // A small queue to hold the values of a 4-byte data block as it gets passed through the filter.
        private Queue<byte> sampleQueue = new Queue<byte>(4);
        private UInt16 currentRolloverCount = 0;
        private UInt16 currentPeriod = 0;
        private UInt32 currentTimestamp = 0;
        private byte prevSample = 0;

        /// <summary>
        /// Markers used to define the type of data block in the stream.
        /// </summary>
        internal enum Markers
        {
            Period = 0xbd,
            Sample = 0xbf,
            Rollover = 0xbe
        }

        #region Private Classes

        // These classes are never actually used. They are just to show the format of the
        // data in the samples for each marker type. Note that 16-bit integers
        // are stored in lo-hi format.
        private class PeriodMarker
        {
            public byte Marker;
            public UInt16 Period;
            public byte Unused;
        }

        private class SampleMarker
        {
            public byte Marker;
            public UInt16 TimeStamp;
            public byte Sample;
        }

        private class RolloverMarker
        {
            public byte Marker;
            public UInt16 RolloverCount;
            public byte Unused;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates and initializes a TimestampFilter object.
        /// </summary>
        public TimestampFilter()
        {

        }

        #endregion

        #region Overridden Methods

        /// <summary>
        /// Write a value to the timestamp filter.
        /// </summary>
        /// <param name="Data">The data value being sent through the filter</param>
        public override void Write(byte Value)
        {
            if (sampleQueue.Count == 0)
            {
                // First byte in the data block -- the marker.
                switch ((Markers)Value)
                {
                    case Markers.Period:
                    case Markers.Rollover:
                    case Markers.Sample:
                        break;
                    default:
                        throw new Exception("TimestampFilter.Write: Invalid Marker (" + Value.ToString("X2") + ")");
                }
                sampleQueue.Enqueue(Value);
            }
            else if (sampleQueue.Count < 3)
            {
                // Bytes 1-2 of a block are simply stored in the queue.
                sampleQueue.Enqueue(Value);
            }
            else
            {
                Markers marker = (Markers)sampleQueue.Dequeue();
                byte loByte = sampleQueue.Dequeue();
                byte hiByte = sampleQueue.Dequeue();
                UInt32 timestamp;

                // We currently don't use the period. Each timestamp is one
                // period 'tick'. So, if the last sample was taken at tick 10
                // and the current sample was taken at tick 25, then there are
                // 15 identical samples. The time in between samples isn't
                // really needed.
                switch (marker)
                {
                    case Markers.Period:
                        currentPeriod = (ushort)(hiByte << 8 | loByte);
                        break;
                    case Markers.Rollover:
                        // Rollover signifies that the time stamp has rolled over 16 bits.
                        currentRolloverCount = (ushort)(hiByte << 8 | loByte);
                        break;
                    case Markers.Sample:
                        timestamp = ((UInt32)currentRolloverCount << 16) | (ushort)(hiByte << 8 | loByte);

                        // Repeat the previous sample as needed.
                        // NOTE: This generates a lot of data (just like Continuous mode,) and
                        // could probably be done differently.
                        while (currentTimestamp < timestamp)
                        {
                            base.Write(prevSample);
                            currentTimestamp += 1;
                        }
                        prevSample = Value;
                        break;
                }

            }
        }

        #endregion
    }
}
