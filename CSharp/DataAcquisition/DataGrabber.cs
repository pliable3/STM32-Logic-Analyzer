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
using System.Windows;
using System.Windows.Forms;
using System.IO;
using LogicAnalyzer.Controllers;

namespace LogicAnalyzer.DataAcquisition
{
    /// <summary>
    /// Class defining methods for Logic Analyzer data acquisition.
    /// </summary>
    public class DataGrabber
    {
        private bool pingInProgress;
        private bool pingResponseReceived;
        private Timer sampleTimer;
        private bool samplingInProgress;
        private bool sampleReceived;

        public enum SamplingModes
        {
            Continuous,     // One sample per sampling period
            TransitionsOnly // Only samples that are different than the previous sample
        }

        #region Constructors

        /// <summary>
        /// Creates an initializes a DataGrabber object.
        /// </summary>
        /// <param name="Controller">A controller for the device being sampled</param>
        public DataGrabber(AbstractController Controller)
            : this(Controller, 1000, 4, 1000, SamplingModes.Continuous, true)
        {
        }

        /// <summary>
        /// Creates an initializes a DataGrabber object.
        /// </summary>
        /// <param name="Controller">A controller for the device being sampled</param>
        /// <param name="SamplingRate">The rate at which to sample (in samples/second)</param>
        /// <param name="SamplingChannels">The number of channels to sample</param>
        /// <param name="SamplingTime">The total time to sample (in milliseconds)</param>
        /// <param name="SamplingMode">The sampling mode</param>
        /// <param name="SamplingCompression">'true' if compression is to be used</param>
        public DataGrabber(AbstractController Controller, int SamplingRate, int SamplingChannels, int SamplingTime, SamplingModes SamplingMode, bool SamplingCompression)
        {
            if (Controller == null)
                throw new Exception("DataGrabber: Invalid controller");

            this.Controller = Controller;
            this.SamplingRate = SamplingRate;
            this.SamplingChannels = SamplingChannels;
            this.SamplingMode = SamplingMode;
            this.SamplingTime = SamplingTime;
            this.SamplingCompression = SamplingCompression;
            this.Data = new List<byte>();

            Controller.OnDataReceived += new EventHandler<ControllerEventArgs>(Controller_OnDataReceived);
            Controller.OnError += new EventHandler<ControllerEventArgs>(Controller_OnError);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a controller for the device being sampled
        /// </summary>
        public AbstractController Controller
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets/Sets sampled data.
        /// </summary>
        internal List<byte> Data
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the length of the sampled data.
        /// </summary>
        private int DataLength
        {
            get
            {
                return Data.Count;
            }
        }

        /// <summary>
        /// Gets tje expected length of data to sample.
        /// </summary>
        private int ExpectedDataLength
        {
            get
            {
                // Guess at the amount of data that we will receive in the next sample.
                return (int)((this.SamplingRate * (this.SamplingTime / 1000.0)) / this.SamplesPerByte);
            }
        }

        /// <summary>
        /// Gets the number of samples per byte of raw sample data.
        /// </summary>
        public int SamplesPerByte
        {
            get
            {
                switch (this.SamplingChannels)
                {
                    case 1:
                        return 8;
                    case 2:
                        return 4;
                    case 3:
                    case 4:
                        return 2;
                }
                return 1;
            }
        }

        /// <summary>
        /// Gets/Set the number of channels to sample
        /// </summary>
        public int SamplingChannels
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets if compression is to be used
        /// </summary>
        public bool SamplingCompression
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the sampling mode
        /// </summary>
        public SamplingModes SamplingMode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the rate at which to sample (in samples/second)
        /// </summary>
        public int SamplingRate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the total time to sample (in milliseconds)
        /// </summary>
        public int SamplingTime
        {
            get;
            set;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Close the controller.
        /// </summary>
        public void Close()
        {
            if (this.Controller != null)
                this.Controller.Close();
            samplingInProgress = false;
        }

        /// <summary>
        /// Open the controller.
        /// </summary>
        public void Open()
        {
            Controller.Open();
            //startTime = DateTime.Now;
        }

        /// <summary>
        /// Ask the controller/device to respond with a firmware revision/copyright message.
        /// </summary>
        public void FirmwareRevision()
        {
            // If the controller is not open, attempt to open it.
            if (!Controller.IsOpen())
            {
                // If Open() fails, the error was already broadcast through the event handler
                // so just stop.
                if (!Controller.Open())
                    return;
            }

            this.Data = new List<byte>(64);

            pingInProgress = false;
            sampleReceived = false;
            samplingInProgress = false;

            Controller.ClearFilters();

            // Attach an error filter to the controller input.
            Controller.AddInputFilter(new Filters.ErrorFilter());

            Controller.TotalBytesReceived = 0;
            Controller.TotalUnfilteredBytesReceived = 0;

            try
            {
                // Send a command to the controller/micro to return a firmware revision/copyright message.
                Controller.Write("COPY\r\n");
            }
            catch (Exception ex)
            {
                BroadcastError(ex.Message);
            }
        }

        /// <summary>
        /// Ask the controller/device to respondto a ping.
        /// </summary>
        public void PingController()
        {
            // If the controller is not open, attempt to open it.
            if (!Controller.IsOpen())
            {
                // If Open() fails, the error was already broadcast through the event handler
                // so just stop.
                if (!Controller.Open())
                    return;
            }

            Controller.ClearFilters();

            // Attach an error filter to the controller input.
            Controller.AddInputFilter(new Filters.ErrorFilter());

            Controller.TotalBytesReceived = 0;
            Controller.TotalUnfilteredBytesReceived = 0;

            try
            {
                // Send a command to the controller/micro.
                Controller.Write("PING\r\n");

                pingInProgress = true;
                pingResponseReceived = false;

                // Wait for 1/2 a second for a response.
                sampleTimer = new Timer();
                sampleTimer.Interval = 500;
                sampleTimer.Tick += sampleTimer_Tick;
                sampleTimer.Enabled = true;
            }
            catch (Exception ex)
            {
                BroadcastError(ex.Message);
            }
        }

        /// <summary>
        /// Start a sampling session.
        /// </summary>
        public void StartSampling()
        {
            // If the controller is not open, attempt to open it.
            if (!Controller.IsOpen())
            {
                // If Open() fails, the error was already broadcast through the event handler
                // so just stop.
                if (!Controller.Open())
                    return;
            }

            // Use the expected data length to initialize the data array.
            this.Data = new List<byte>(this.ExpectedDataLength + 16);

            pingInProgress = false;
            sampleReceived = false;

            Controller.ClearFilters();

            // Attach an error filter to the controller input.
            Controller.AddInputFilter(new Filters.ErrorFilter());

            if (this.SamplingMode == SamplingModes.Continuous)
            {
                // If we're in Continuous mode and compression is specified, add a decompression filter to the controller input.
                if (this.SamplingCompression)
                    Controller.AddInputFilter(new Filters.DecompressionFilter());
            }
            else
            {
                // If we're in Transition mode, attach a filter for that.
                Controller.AddInputFilter(new Filters.TimestampFilter());
            }

            Controller.TotalBytesReceived = 0;
            Controller.TotalUnfilteredBytesReceived = 0;

            try
            {
                // Send commands to the controller to set modes on the micro.
                Controller.Write("CHAN=" + this.SamplingChannels + "\r\n");
                Controller.Write("RATE=" + this.SamplingRate + "\r\n");
                Controller.Write("COMP=" + (this.SamplingCompression ? "Y" : "N") + "\r\n");
                Controller.Write("TIME=" + this.SamplingTime + "\r\n");
                Controller.Write("MODE=" + (this.SamplingMode == SamplingModes.TransitionsOnly ? "TRAN" : "CONT") + "\r\n");

                // Start sampling...
                Controller.Write("START\r\n");

                samplingInProgress = true;

                // Wait for a response.
                sampleTimer = new Timer();
                sampleTimer.Interval = this.SamplingTime + 100;
                sampleTimer.Tick += sampleTimer_Tick;
                sampleTimer.Enabled = true;
            }
            catch (Exception ex)
            {
                samplingInProgress = false;

                BroadcastError(ex.Message);
            }
        }

        /// <summary>
        /// Timer tick event handler (used to finish sampling and pinging).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sampleTimer_Tick(object sender, EventArgs e)
        {
            int addlTime;

            sampleTimer.Enabled = false;

            // If we were pinging the device, check for the correct response.
            if (pingInProgress)
            {
                if (!pingResponseReceived)
                    BroadcastError("Ping failed\r\n");
                pingInProgress = false;
                return;
            }

            addlTime = (this.SamplingMode == SamplingModes.TransitionsOnly ? 500 : 100);

            // The sample should be complete, but there could be a lag, so
            // check every 100 ms from now on to look for activity.
            if (sampleTimer.Interval > addlTime)
                sampleTimer.Interval = addlTime;
            else if (!sampleReceived)
            {
                // We received no data in the last 100 ms, assume that we are done.
                samplingInProgress = false;

                //Controller.Write("STOP\r\n");

                Controller.ClearFilters();

                // Tell our listeners that we are finished sampling.
                BroadcastComplete();
                return;
            }

            sampleReceived = false;
            sampleTimer.Enabled = true;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handle this event to identify when sampling has completed.
        /// </summary>
        public event EventHandler<ProgressEventArgs> OnComplete;

        /// <summary>
        /// Broadcast an OnComplete event to anyone who's listening.
        /// </summary>
        /// <param name="Message">The error message</param>
        protected void BroadcastComplete()
        {
            EventHandler<ProgressEventArgs> handler = OnComplete;

            if (handler != null)
                handler(this, new ProgressEventArgs(100, Controller.TotalBytesReceived, Controller.TotalBytesReceived > 0 ? 100 - (100 * Controller.TotalUnfilteredBytesReceived) / Controller.TotalBytesReceived : 0));
            //handler(this, ProgressEventArgs.GetInstance(this.Name, Message));
        }

        /// <summary>
        /// Handle this event to receive console messages.
        /// </summary>
        public event EventHandler<ConsoleMessageEventArgs> OnConsoleMessage;

        /// <summary>
        /// Broadcast an OnConsoleMessage event to anyone who's listening.
        /// </summary>
        /// <param name="Message">The error message</param>
        protected void BroadcastConsoleMessage(string Message)
        {
            EventHandler<ConsoleMessageEventArgs> handler = OnConsoleMessage;

            if (handler != null)
                handler(this, new ConsoleMessageEventArgs(Message));
            //handler(this, ProgressEventArgs.GetInstance(this.Name, Message));
        }

        /// <summary>
        /// Handle this event to trap asynchronous DataGrabber/controller errors.
        /// </summary>
        public event EventHandler<ErrorEventArgs> OnError;

        /// <summary>
        /// Broadcast an error message asynchronously to anyone who's listening.
        /// </summary>
        /// <param name="Message">The error message</param>
        protected void BroadcastError(string Message)
        {
            EventHandler<ErrorEventArgs> handler = OnError;

            if (handler != null)
                handler(this, new ErrorEventArgs(new Exception( Message)));
            //handler(this, ProgressEventArgs.GetInstance(this.Name, Message));
        }

        /// <summary>
        /// Broadcast an error message asynchronously to anyone who's listening.
        /// </summary>
        /// <param name="Ex">The error Exception</param>
        protected void BroadcastError(Exception Ex)
        {
            EventHandler<ErrorEventArgs> handler = OnError;

            if (handler != null)
                handler(this, new ErrorEventArgs(Ex));
            //handler(this, ProgressEventArgs.GetInstance(this.Name, Message));
        }

        /// <summary>
        /// Handle this event to identify when progress is being made when receiving data.
        /// </summary>
        public event EventHandler<ProgressEventArgs> OnProgress;

        /// <summary>
        /// Broadcast an OnProgress event to anyone who's listening.
        /// </summary>
        /// <param name="Ex">The error Exception</param>
        protected void BroadcastProgress(int PercentDone)
        {
            EventHandler<ProgressEventArgs> handler = OnProgress;

            if (handler != null)
                handler(this, new ProgressEventArgs(PercentDone <= 100 ? PercentDone : 100, Controller.TotalBytesReceived, 100 - (100 * Controller.TotalUnfilteredBytesReceived) / Controller.TotalBytesReceived));
            //handler(this, ProgressEventArgs.GetInstance(this.Name, Message));
        }

        #endregion

        #region Controller Event Handlers

        /// <summary>
        /// Handler for controller OnError events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Controller_OnError(object sender, ControllerEventArgs e)
        {
            // Just re-broadcase the error to our listeners.
            BroadcastError(e.Message);

            e.Dispose();  // Recycle the ControllerEventArgs object
        }

        private static System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

        //private void WriteBytes(byte[] buffer)
        //{
        //    StringBuilder sb = new StringBuilder();

        //    foreach (byte b in buffer)
        //    {
        //        if (sb.Length != 0)
        //            sb.Append(" ");
        //        sb.Append(b.ToString("X2"));
        //    }
        //    System.Diagnostics.Debug.WriteLine("GRAB: " + sb.ToString());
        //}

        /// <summary>
        /// Handler for controller OnDataReceived events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Controller_OnDataReceived(object sender, ControllerEventArgs e)
        {
            int count = Controller.BytesToRead;

            //System.Diagnostics.Debug.WriteLine("DATA REC: " + count);

            if (count > 0)
            {
                byte[] buffer = new byte[count];
                Controller.Read(buffer, count);

                if (samplingInProgress)
                {
                    sampleReceived = true;

#if false
                    //System.Diagnostics.Debug.Write(enc.GetString(buffer));
                    WriteBytes(buffer);
#endif

                    // NOTE: Not sure if this is any better than looping through the buffer myself...
                    this.Data.AddRange(buffer);

                    // Tell our fans that we're making progress.
                    if (this.ExpectedDataLength > 0)
                        BroadcastProgress((int)((100.0 * this.DataLength) / this.ExpectedDataLength));
                }
                else if (pingInProgress)
                {
                    pingResponseReceived = true;

                    // Check for a valid ping response.
                    if (enc.GetString(buffer).Equals("pOnG\r\n"))
                        BroadcastConsoleMessage("Ping successful\r\n");
                    else
                        BroadcastError("Ping failed\r\n");
                }
                else
                {
                    // If we're not in 'sample' or 'ping' mode, just send the received data to the console.
                    BroadcastConsoleMessage(new System.Text.ASCIIEncoding().GetString(buffer).Replace("\0", ""));
                }
            }

            e.Dispose();  // Recycle the ControllerEventArgs object
        }

        #endregion
    }
}
