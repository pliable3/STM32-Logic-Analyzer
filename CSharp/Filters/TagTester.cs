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
    /// Class defining methods to dynamically search for delimiters in a data stream filter. When a delimiter
    /// is suspected, values are cached until it is determined that the delimiter is genuine or not.
    /// </summary>
    /// <typeparam name="T">Data type of the filter elements</typeparam>
    internal class TagTester<T> : IDisposable
    {
        private Queue<T> tagQueue = new Queue<T>();

        private ITagTesterWriter<T> tagQueueWriter;

        #region Constructors

        /// <summary>
        /// Creates and initializes a TagTester object.
        /// </summary>
        /// <param name="TagQueueWriter">An object that implements the ITagTesterWriter interface</param>
        public TagTester(ITagTesterWriter<T> TagQueueWriter)
        {
            if (TagQueueWriter == null)
                throw new Exception("TagTester: ITagTesterWriter cannot be null");
            this.tagQueueWriter = TagQueueWriter;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the length of the delimiter queue.
        /// </summary>
        public int Length
        {
            get
            {
                return tagQueue.Count;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Write the queued delimiter values to the filter.
        /// </summary>
        private void WriteTagQueue()
        {
            while (tagQueue.Count > 0)
                this.tagQueueWriter.WriteTagTesterValue(tagQueue.Dequeue());
        }

        /// <summary>
        /// Clear the delimiter queue.
        /// </summary>
        public void Clear()
        {
            tagQueue.Clear();
        }

        /// <summary>
        /// Determine if a value is part of a delimiter.
        /// </summary>
        /// <param name="TagCode">The delimiter values</param>
        /// <param name="Data">The data value being tested</param>
        /// <returns></returns>
        public bool ValueInTagCode(T[] TagCode, T Data)
        {
            if (TagCode[tagQueue.Count].Equals(Data))
            {
                // The data item appears to be part of the delimiter. Add it to the
                // tag queue in case it later turns out not to be the delimiter.
                tagQueue.Enqueue(Data);
                return true;
            }

            // The data item is not part of the delimiter. Write the imposter delimiter
            // back out to the filter.
            if (tagQueue.Count > 0)
            {
                // The bytes that we thought might be part of a start/stop tag are not, so
                // send them on their way.
                WriteTagQueue();

                // The value that we're testing was not the next one in the tag code.
                // BUT, it is possible that it is the *first* character of the tag.
                // So, recurse back around (just once) to check.
                return ValueInTagCode(TagCode, Data);
            }

            return false;
        }

        /// <summary>
        /// Dispose of a TagTester object.
        /// </summary>
        public void Dispose()
        {
            tagQueue.Clear();
            tagQueue = null;
        }

        #endregion
    }
}
