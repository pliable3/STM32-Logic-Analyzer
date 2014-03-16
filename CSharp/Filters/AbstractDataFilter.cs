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
    /// Class defining the base functionality of a generic data filter. Decendents can
    /// define tags that delimit data in some way using the ITagTesterWriter interface.
    /// </summary>
    /// <typeparam name="T">Data type of objects being filtered</typeparam>
    public class AbstractDataFilter<T> : ITagTesterWriter<T>
    {
        private Queue<T> dataOut;

        #region Constructors

        /// <summary>
        /// Creates and initializes an AbstractDataFilter object.
        /// </summary>
        public AbstractDataFilter()
            : this(256)
        {
        }

        /// <summary>
        /// Creates and initializes an AbstractDataFilter object.
        /// </summary>
        /// <param name="InitialQueueSize">Initial size of the filter data queue</param>
        public AbstractDataFilter(int InitialQueueSize)
        {
            dataOut = new Queue<T>(InitialQueueSize);
            TagTester = new TagTester<T>(this);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Daisy-chained filter. Data can be filtered through multple filters
        /// by adding more filters.
        /// </summary>
        private AbstractDataFilter<T> ChildFilter
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the length of the filtered data.
        /// </summary>
        public int DataLength
        {
            get
            {
                int count;

                lock (dataOut)
                {
                    count = dataOut.Count;
                }
                return count;
            }
        }

        /// <summary>
        /// Gets 'true' if data is available.
        /// </summary>
        public bool DataReady
        {
            get
            {
                return this.DataLength > 0;
            }
        }

        /// <summary>
        /// Gets a TagTester object associated with this filter. The TagTester is used
        /// to find and remove delimiters in the data stream.
        /// </summary>
        internal TagTester<T> TagTester
        {
            get;
            private set;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Re-initializes the filter.
        /// </summary>
        public virtual void Initialize()
        {
        }

        /// <summary>
        /// Daisy-chains a filter to this filter.
        /// </summary>
        /// <param name="Filter">A child filter to add to this filter.</param>
        public void AddFilter(AbstractDataFilter<T> Filter)
        {
            if (this.ChildFilter != null)
                this.ChildFilter.AddFilter(Filter);
            else
                this.ChildFilter = Filter;
        }

        /// <summary>
        /// Writes data to the filter. If there are child filters, the data will automaticaly
        /// flow through them before being placed in the output of the filter.
        /// </summary>
        /// <param name="Data">A data value to be passed through the filter</param>
        public virtual void Write(T Data)
        {
            if (ChildFilter != null)
            {
                ChildFilter.Write(Data);
                while (ChildFilter.DataReady)
                {
                    T data = ChildFilter.Read();
                    lock (dataOut)
                    {
                        dataOut.Enqueue(data);
                    }
                }
            }
            else
            {
                lock (dataOut)
                {
                    dataOut.Enqueue(Data);
                }
            }
        }

        /// <summary>
        /// Writes a data array to the filter. If there are child filters, the data will automaticaly
        /// flow through them before being placed in the output of the filter.
        /// </summary>
        /// <param name="Buffer">A byte array to be passed through the filter.</param>
        public virtual void Write(T[] Buffer)
        {
            foreach (T b in Buffer)
                this.Write(b);
        }

        /// <summary>
        /// Reads a data value from the filter. The DataReady property should be checked
        /// prior to calling this method to determine if data is available.
        /// </summary>
        /// <returns>A data value that has been passed through the filter (and any child filters)</returns>
        public virtual T Read()
        {
            T value;

            if (!this.DataReady)
                throw new Exception("AbstractDataFilter.Read: Filter queue is empty");

            lock (dataOut)
            {
                value = dataOut.Dequeue();
            }
            return value;
        }

        /// <summary>
        /// Reads a byte array from the filter.  The DataReady property should be checked
        /// prior to calling this method to determine if data is available.
        /// </summary>
        /// <param name="Buffer">A pre-allocated output byte array</param>
        /// <param name="MaxLength">The maximum length of data to read</param>
        /// <returns></returns>
        public virtual int Read(T[] Buffer, int MaxLength)
        {
            int cnt = 0;

            if (!this.DataReady)
                throw new Exception("AbstractDataFilter.Read: Filter is empty");
            if (Buffer == null)
                throw new Exception("AbstractDataFilter.Read: Buffer is null");
            if (Buffer.Length < MaxLength)
                throw new Exception("AbstractDataFilter.Read: Buffer length < MaxLength");

            while (cnt < MaxLength && this.DataLength > 0)
                Buffer[cnt++] = this.Read();

            return cnt;
        }

        /// <summary>
        /// Flushes data through this filter and all its children.
        /// </summary>
        public virtual void Flush()
        {
            if (this.ChildFilter != null)
            {
                this.ChildFilter.Flush();
                while (ChildFilter.DataReady)
                {
                    T data = ChildFilter.Read();
                    lock (dataOut)
                    {
                        dataOut.Enqueue(data);
                    }
                }
            }
        }

        #endregion

        #region ITagTesterWriter Interface

        /// <summary>
        /// Override to write a data value to the filter output. This gets used when queued data
        /// that was thought to be part of a delimiter needs to be sent through the filter.
        /// </summary>
        /// <param name="Data"></param>
        public virtual void WriteTagTesterValue(T Data)
        {
        }

        #endregion
    }
}