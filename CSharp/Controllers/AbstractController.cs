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
using LogicAnalyzer.Filters;

namespace LogicAnalyzer.Controllers
{
    /// <summary>
    /// Abstract class to define base functionality for a controller that can
    /// send and receive data. Descendents of this class might include a serial
    /// port or Ethernet connection.
    /// </summary>
    public abstract class AbstractController : IDisposable
    {
        private AbstractDataFilter<byte> inputFilter;
        private AbstractDataFilter<byte> outputFilter;
        private Queue<byte> dataReceived;
        private Queue<byte> dataToSend;

        #region Constructors

        /// <summary>
        /// Construct a nameless controller object.
        /// </summary>
        public AbstractController()
            : this("", 1024)
        {
        }

        /// <summary>
        /// Construct a named controller.
        /// </summary>
        /// <param name="Name">The name of the controller</param>
        public AbstractController(string Name)
            : this(Name, 1024)
        {
        }

        /// <summary>
        /// Construct a named controller with a default queue length.
        /// </summary>
        /// <param name="Name">The name of the controller</param>
        /// <param name="DefaultQueueLength">The default length of the send/receive queues (default=1024)</param>
        public AbstractController(string Name, int DefaultQueueLength)
        {
            this.Name = Name;
            this.dataReceived = new Queue<byte>(DefaultQueueLength);
            this.dataToSend = new Queue<byte>(DefaultQueueLength);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the number of bytes of data that are ready to be read from 
        /// the controller.
        /// </summary>
        public int BytesToRead
        {
            get
            {
                lock (dataReceived)
                {
                    return dataReceived.Count;
                }
            }
        }

        /// <summary>
        /// Gets/Sets the name of the controller.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the total number of [filtered] bytes received by the controller. Note
        /// that compression/decompression filters will result in different values for
        /// [filtered] and [unfiltered] bytes.
        /// </summary>
        public int TotalBytesReceived
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the total number of [unfiltered] bytes received by the controller. Note
        /// that compression/decompression filters will result in different values for
        /// [filtered] and [unfiltered] bytes.
        /// </summary>
        public int TotalUnfilteredBytesReceived
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the number of [filtered] bytes sent by the controller.  Note
        /// that compression/decompression filters will result in different values for
        /// [filtered] and [unfiltered] bytes.
        /// </summary>
        public int TotalBytesSent
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the number of [unfiltered] bytes sent by the controller.  Note
        /// that compression/decompression filters will result in different values for
        /// [filtered] and [unfiltered] bytes.
        /// </summary>
        public int TotalUnfilteredBytesSent
        {
            get;
            set;
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Open communication with the device.
        /// </summary>
        /// <returns>'true' if the device was opened successfully</returns>
        public abstract bool Open();

        /// <summary>
        /// Close the device controller.
        /// </summary>
        public abstract void Close();

        /// <summary>
        /// Determine if the device is open.
        /// </summary>
        /// <returns></returns>
        public abstract bool IsOpen();

        /// <summary>
        /// Write an array of bytes to the device. This method is only called internally
        /// when an array of (possibly filtered) data is ready to send to the device.
        /// </summary>
        /// <param name="Bytes"></param>
        protected abstract void WriteToDevice(byte[] Bytes);

        #endregion

        #region Methods

        /// <summary>
        /// Remove filters from the controller.
        /// </summary>
        public void ClearFilters()
        {
            this.inputFilter = null;
            this.outputFilter = null;
        }

        /// <summary>
        /// Add an input filter to the controller. All incoming data will be
        /// sent through the filter before being registered. 
        /// </summary>
        /// <param name="Filter">A filter for pre-processing data as it is received by the controller</param>
        public void AddInputFilter(AbstractDataFilter<byte> Filter)
        {
            if (inputFilter == null)
                inputFilter = Filter;
            else
                inputFilter.AddFilter(Filter);
        }

        /// <summary>
        /// Add an output filter to the controller. All outgoing data will be
        /// sent through the filter before being sent to the output. 
        /// </summary>
        /// <param name="Filter">A filter for pre-processing data before it is send by the controller</param>
        public void AddOutputFilter(AbstractDataFilter<byte> Filter)
        {
            if (outputFilter == null)
                outputFilter = Filter;
            else
                outputFilter.AddFilter(Filter);
        }

        /// <summary>
        /// Read a byte of data from the controller. Check for BytesToRead > 0
        /// before calling this method.
        /// </summary>
        /// <returns>A byte from the receive queue of the controller.</returns>
        public byte Read()
        {
            byte b;

            if (this.dataReceived.Count == 0)
                throw new Exception("AbstractController.Read: Read buffer is empty");

            lock (this.dataReceived)
            {
                b = this.dataReceived.Dequeue();
            }

            return b;
        }

        /// <summary>
        /// Read an array of bytes from the controller. Check for BytesToRead > 0
        /// before calling this method.
        /// </summary>
        /// <param name="Buffer">A pre-allocated array (of at least 
        /// size)</param>
        /// <param name="MaxLength">The maximum number of bytes to read from the
        /// controller</param>
        /// <returns>The number of bytes read</returns>
        public int Read(byte[] Buffer, int MaxLength)
        {
            int count = 0;

            if (this.dataReceived.Count == 0)
                throw new Exception("AbstractController.Read: Read buffer is empty");
            if (Buffer == null)
                throw new Exception("AbstractController.Read: Buffer is null");
            if (Buffer.Length < MaxLength)
                throw new Exception("AbstractController.Read: Buffer length < MaxLength");

            lock (this.dataReceived)
            {
                while (count < MaxLength && this.dataReceived.Count > 0)
                    Buffer[count++] = this.dataReceived.Dequeue();
            }

            return count;
        }

        /// <summary>
        /// Function that gets called from descendents to register data received from
        /// a device with the controller - which may include pre-processing filters.
        /// </summary>
        /// <param name="Data">A byte of incoming data to register with the controller</param>
        protected void ReceiveFromDevice(byte Data)
        {
            this.TotalUnfilteredBytesReceived++;

            try
            {
                if (inputFilter != null)
                {
                    // Write data to the first filter. The filters are daisy-chained, so this value
                    // is passed all the way down to the end of the chain and back up again.
                    inputFilter.Write(Data);
                    while (inputFilter.DataReady)
                    {
                        byte b = inputFilter.Read();

                        this.TotalBytesReceived++;

                        lock (this.dataReceived)
                        {
                            this.dataReceived.Enqueue(b);
                        }
                    }
                }
                else
                {
                    // If there are no input filters, just store the value in the receipt queue.
                    lock (this.dataReceived)
                    {
                        this.dataReceived.Enqueue(Data);
                    }
                }
            }
            catch (Exception ex)
            {
                BroadcastError(ex.Message);
            }
        }

        /// <summary>
        /// Convert the send queue to an array of bytes.
        /// </summary>
        /// <returns>An array of bytes from the send queue</returns>
        private byte[] GetSendBuffer()
        {
            byte[] bytes;

            lock (dataToSend)
            {
                bytes = this.dataToSend.ToArray();
                this.dataToSend.Clear();
            }
            return bytes;
        }

        /// <summary>
        /// Write a byte of data to the controller. Note that data may be
        /// pre-processed through filters before being sent to the output device.
        /// </summary>
        /// <param name="Data">A byte of data to send through the controller</param>
        private void WriteByte(byte Data)
        {
            this.TotalUnfilteredBytesSent++;

            if (outputFilter != null)
            {
                // Write data to the first filter. The filters are daisy-chained, so this value
                // is passed all the way down to the end of the chain and back up again.
                outputFilter.Write(Data);
                while (outputFilter.DataReady)
                {
                    byte b = outputFilter.Read();

                    this.TotalBytesSent++;
                    lock (this.dataToSend)
                    {
                        this.dataToSend.Enqueue(b);
                    }
                }
            }
            else
            {
                // If there are no output filters, just store the value in the send queue.
                lock (this.dataToSend)
                {
                    this.dataToSend.Enqueue(Data);
                }
            }
        }

        /// <summary>
        /// Write a byte of data to the controller.
        /// </summary>
        /// <param name="Data">A byte to send to the controller</param>
        public void Write(byte Data)
        {
            WriteByte(Data);

            this.WriteToDevice(GetSendBuffer());
        }

        /// <summary>
        ///  Write an array of bytes to the controller.
        /// </summary>
        /// <param name="Data">An array of bytes to send to the controller</param>
        public void Write(byte[] Data)
        {
            foreach (byte b in Data)
                WriteByte(b);

            this.WriteToDevice(GetSendBuffer());
        }

        /// <summary>
        ///  Write a string of bytes to the controller.
        /// </summary>
        /// <param name="Data">A string of bytes to send to the controller</param>
        public void Write(string Data)
        {
            foreach (char c in Data)
                WriteByte(Convert.ToByte(c));

            this.WriteToDevice(GetSendBuffer());
        }

        /// <summary>
        /// Flush the input filters through the controller.
        /// </summary>
        protected void Flush()
        {
            if (inputFilter != null)
                inputFilter.Flush();
        }

        /// <summary>
        /// Dispose of the controller object.
        /// </summary>
        public virtual void Dispose()
        {
            this.inputFilter = null;
            this.outputFilter = null;
            this.dataToSend.Clear();
            this.dataReceived.Clear();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handle this event to trap asynchronous controller errors.
        /// </summary>
        public event EventHandler<ControllerEventArgs> OnError;

        /// <summary>
        /// Broadcast an error message asynchronously to anyone who's listening.
        /// </summary>
        /// <param name="Message">The error message</param>
        protected void BroadcastError(string Message)
        {
            EventHandler<ControllerEventArgs> handler = OnError;

            // Note that ControllerEventArgs is an Object Pool object to
            // reduce object creation. These objects should call Dispose()
            // when they are no longer needed.
            if (handler != null)
                handler(this, ControllerEventArgs.GetInstance(this.Name, Message));
        }

        /// <summary>
        /// Handle this event to receive data from the controller.
        /// </summary>
        public event EventHandler<ControllerEventArgs> OnDataReceived;

        /// <summary>
        /// Broadcast a message signalling to listeners that data has been received.
        /// </summary>
         protected void BroadcastDataReceived()
        {
            EventHandler<ControllerEventArgs> handler = OnDataReceived;

            // Note that ControllerEventArgs is an Object Pool object to
            // reduce object creation. These objects should call Dispose()
            // when they are no longer needed.
            if (handler != null)
                handler(this, ControllerEventArgs.GetInstance(this.Name, ""));
        }

        #endregion
    }
}