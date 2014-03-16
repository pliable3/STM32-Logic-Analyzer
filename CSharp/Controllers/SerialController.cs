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
using System.IO.Ports;
using LogicAnalyzer.Filters;

namespace LogicAnalyzer.Controllers
{
    /// <summary>
    /// A class defining a serial port controller.
    /// </summary>
    public class SerialController : AbstractController
    {
        private SerialPort serialPort;
        private int _timeOut = 3000;

        #region Constructors

        /// <summary>
        /// Construct and initialize a serial port controller object.
        /// </summary>
        /// <param name="PortName">The name of the serial port (i.e. COM4)</param>
        /// <param name="BaudRate">The baud rate to be used by the serial port</param>
        /// <param name="Parity"></param>
        /// <param name="DataBits"></param>
        /// <param name="StopBits"></param>
        public SerialController(string PortName, int BaudRate, Parity Parity, int DataBits, StopBits StopBits)
            : base(PortName, 8192)
        {
            this.BaudRate = BaudRate;
            this.Parity = Parity;
            this.DataBits = DataBits;
            this.StopBits = StopBits;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the baud rate to be used by the serial port.
        /// </summary>
        public int BaudRate
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the parity to be used by the serial port
        /// </summary>
        public Parity Parity
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the number of data bits to be used by the serial port
        /// </summary>
        public int DataBits
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the number of stop bits to be used by the serial port
        /// </summary>
        public StopBits StopBits
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets/Sets the amount of time (in milliseconds) before a timeout occurs when reading data.
        /// </summary>
        public int Timeout
        {
            get
            {
                return _timeOut;
            }
            set
            {
                _timeOut = value;
                if(serialPort != null && serialPort.IsOpen)
                    serialPort.ReadTimeout = _timeOut;
            }
        }

        #endregion

        #region Overridden Methods

        /// <summary>
        /// Open the serial port controller.
        /// </summary>
        /// <returns>'true' if successful.</returns>
        public override bool Open()
        {
            bool result = true;

            try
            {
                serialPort = new SerialPort(this.Name, this.BaudRate, this.Parity, this.DataBits, this.StopBits);
                serialPort.ReadTimeout = _timeOut;
                serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);
                serialPort.ErrorReceived += new SerialErrorReceivedEventHandler(serialPort_ErrorReceived);
                serialPort.Open();
            }
            catch(IOException iox)
            {
                BroadcastError(iox.Message);
                result = false;
            }
            catch(Exception ex)
            {
                BroadcastError(ex.Message);
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Close the serial port controller.
        /// </summary>
        public override void Close()
        {
            if(serialPort != null)
            {
                try
                {
                    if(serialPort.IsOpen)
                        serialPort.Close();
                }
                catch(IOException iox)
                {
                    BroadcastError(iox.Message);
                }
                catch(Exception ex)
                {
                    BroadcastError(ex.Message);
                }
            }
        }

        /// <summary>
        /// Determine if the serial port is open.
        /// </summary>
        /// <returns>'true' if the serial port is open</returns>
        public override bool IsOpen()
        {
            return (serialPort != null && serialPort.IsOpen);
        }

        /// <summary>
        /// Write an array of bytes to the serial port. This method is only called internally
        /// when an array of (possibly filtered) data is ready to send to the serial port.
        /// </summary>
        /// <param name="bytes"></param>
        protected override void WriteToDevice(byte[] bytes)
        {
            if(serialPort == null)
                throw new Exception("Serial port not initialized");
            if(!serialPort.IsOpen)
                throw new Exception("Serial port not open");

            if(bytes.Length > 0)
                serialPort.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Dispose of the controller and serial port objects.
        /// </summary>
        public override void Dispose()
        {
            if (serialPort != null)
            {
                try
                {
                    this.Close();
                }
                catch { }

                this.serialPort.Dispose();
                this.serialPort = null;
            }

            base.Dispose();
        }

        #endregion

        #region Serial Port Event Handlers

#if true
        private void WriteBytes(byte[] buffer)
        {
            StringBuilder sb = new StringBuilder();

            foreach (byte b in buffer)
            {
                if (sb.Length != 0)
                    sb.Append(" ");
                sb.Append(b.ToString("X2"));
            }
            System.Diagnostics.Debug.WriteLine("CNTR: " + sb.ToString());
        }
#endif

        /// <summary>
        /// Handler for serial port DataReceived events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if(serialPort != null && serialPort.BytesToRead > 0)
            {
                byte[] bytes = new byte[serialPort.BytesToRead];

                serialPort.Read(bytes, 0, bytes.Length);

                //System.Diagnostics.Debug.WriteLine("CTRL REC: " + bytes.Length);

                if(bytes.Length > 0)
                {
#if true
                    WriteBytes(bytes);
#endif
                    foreach(byte b in bytes)
                        base.ReceiveFromDevice(b);

                    // Tell our listeners that data has been received.
                    BroadcastDataReceived();
                }
            }
        }

        /// <summary>
        /// Handler for serial port ErrorReceived events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void serialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            // Just re-broadcast the error to our listeners.
            BroadcastError("Serial error: " + e.EventType.ToString());
        }

        #endregion
    }
}