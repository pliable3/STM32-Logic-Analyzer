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
using System.ComponentModel;
using System.Windows.Forms;
using LogicAnalyzer.Controllers;
using LogicAnalyzer.DataAcquisition;

namespace LogicAnalyzer.Test
{
    /// <summary>
    /// Class defining methods to simulate communication from a device for testing the controller/filter code.
    /// </summary>
    public class LaTestDevice : ITestDevice
    {
        private static System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
        private int samplingChannels = 8;
        private DataGrabber.SamplingModes samplingMode = DataGrabber.SamplingModes.TransitionsOnly;
        private int samplingRate = 50000;
        private int samplingTime = 1000;
        private bool samplingCompression = false;

        #region Constructors

        /// <summary>
        /// Creates and initializes a LaTestDevice object.
        /// </summary>
        public LaTestDevice()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Close communication with the test device.
        /// </summary>
        public void Close()
        {
        }

        /// <summary>
        /// Check if the test device is open for communication.
        /// </summary>
        /// <returns>'true' if the device is open</returns>
        public bool IsOpen()
        {
            return true;
        }

        /// <summary>
        /// Open communication with the test device.
        /// </summary>
        /// <returns>'true' if successful</returns>
        public bool Open()
        {
            Copyright();
            return true;
        }

        /// <summary>
        /// Write an array of bytes to the test device. This is mainly used to send commands to
        /// the device - some of which require a response.
        /// </summary>
        /// <param name="Bytes"></param>
        public void Write(byte[] Bytes)
        {
            string cmd = enc.GetString(Bytes);

            //
            // Commands (requests from the controller):
            //
            // START: Start sampling and send sample data back to the controller.
            // PING: Check if the device is active. Response is "pOng".
            // COPY: Respond with a firmware revision and copyright message.
            // CHAN=: Set the number of channels to sample.
            // RATE=: Set the sampling rate (in samples per second).
            // COMP=: Set compression Y/N.
            // TIME=: Set the total sampling time (in milliseconds).
            // MODE=: Set the sampling mode (continuous or transitions-only).
            //
            if (cmd.Equals("START\r\n"))
            {
                BackgroundWorker worker;

                // Run the test samples in a thread -- mainly so the DataGrabber
                // can get its modes and timers set before data starts arriving.
                worker = new BackgroundWorker();
                worker.DoWork += GenerateSamples;
                worker.RunWorkerAsync();
            }
            else if (cmd.Equals("PING\r\n"))
            {
                BackgroundWorker worker;

                // Respond to the PING in a thread -- mainly so the DataGrabber
                // can get its modes and timers set before data starts arriving.
                worker = new BackgroundWorker();
                worker.DoWork += PingResponse;
                worker.RunWorkerAsync();
            }
            else if (cmd.Equals("COPY\r\n"))
                Copyright();
            else if (cmd.StartsWith("CHAN="))
                samplingChannels = Convert.ToInt32(cmd.Substring(5, cmd.Length - 7));
            else if (cmd.StartsWith("RATE="))
                samplingRate = Convert.ToInt32(cmd.Substring(5, cmd.Length - 7));
            else if (cmd.StartsWith("COMP="))
                samplingCompression = (cmd[5] == 'Y');
            else if (cmd.StartsWith("TIME="))
                samplingTime = Convert.ToInt32(cmd.Substring(5, cmd.Length - 7));
            else if (cmd.StartsWith("MODE="))
                samplingMode = (cmd[5] == 'T' ? DataGrabber.SamplingModes.TransitionsOnly : DataGrabber.SamplingModes.Continuous);
        }

        /// <summary>
        /// Broadcast a firmware revision and copyright message.
        /// </summary>
        private void Copyright()
        {
            BroadcastDataReceived("Logic Analyzer Test Controller by Bob Foley\r\nversion 0.50  (rev. 17-Mar-2014 12:50 p.m.)\r\n\r\n");
        }

        /// <summary>
        /// Broadcast a response to a PING request.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PingResponse(object sender, DoWorkEventArgs e)
        {
            BroadcastDataReceived("pOnG\r\n");
        }

        // Note that the TestController currently only works non-compressed mode.

        // Simulated signals emulate this pattern (sampling rate dependent).
        // 0: 1 KHz
        // 1: 500 Hz
        // 2: 1ms pulse every 20ms (19ms, then 1ms)
        // 3: 2ms pulse every 20ms (18ms, then 2ms)
        // 4: 30 Hz
        // 5: 20ms pulse every 80ms (60ms, then 20ms)
        // 6: 100 Hz
        // 7: 800 Hz
        //
        // Note that rates are divided by 2 because we are working in half-periods with transition in the middle.
        private double[] periods = { 1.0 / (1000 * 2), 1.0 / (500 * 2), 0.019, 0.018, 1.0 / (30 * 2), 0.06, 1.0 / (100 * 2), 1.0 / (800 * 2) };
        private double[] clocks = new double[8];
        private List<byte> sampleData;

        /// <summary>
        /// Broadcast sample data for the requested amount of time. Note, however, that sampling time it simulated.
        /// We know the duration and rate, so we know how many samples we need to send.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GenerateSamples(object sender, DoWorkEventArgs e)
        {
            byte[] channelMasks = { 0x01, 0x03, 0x07, 0x0f, 0x1f, 0x3f, 0x7f, 0xff };
            byte channelMask, channelShift;
            int totSamples;
            double samplePeriod;
            byte bits = 0, stackedBits = 0, prevBits = 0;
            byte stackShift = 0;

            if (samplingChannels < 1 || samplingChannels > 8)
                throw new Exception("Invalid value for Sampling Channels in Test Device");
            if (samplingRate <= 0)
                throw new Exception("Invalid value for Sampling Rate in Test Device");

            samplePeriod = 1.0 / samplingRate;
            switch (samplingChannels)
            {
                case 1:
                    channelShift = 1;
                    break;
                case 2:
                    channelShift = 2;
                    break;
                case 3:
                case 4:
                    channelShift = 4;
                    break;
                default:
                    channelShift = 8;
                    break;
            }
            channelMask = channelMasks[samplingChannels - 1];
            totSamples = samplingRate * samplingTime / 1000;
            sampleData = new List<byte>(1004);

            try
            {
                for (int i = 0; i < totSamples; i++)
                {
                    for (int c = 0; c < 8; c++)
                    {
                        if ((clocks[c] += samplePeriod) >= periods[c])
                        {
                            byte mask = (byte)(0x01 << c);

                            if ((bits & mask) != 0)
                                bits &= (byte)~mask;
                            else
                                bits |= mask;

                            clocks[c] = 0;

                            switch (c)
                            {
                                case 2:
                                    periods[2] = (periods[2] == 0.019 ? 0.001 : 0.019);
                                    break;
                                case 3:
                                    periods[3] = (periods[3] == 0.018 ? 0.002 : 0.018);
                                    break;
                                case 5:
                                    periods[5] = (periods[5] == 0.06 ? 0.02 : 0.06);
                                    break;
                            }
                        }
                    }

                    // Mask off the bits of the channels we're not using.
                    bits &= channelMask;

                    // If we're in transition-only mode, don't add a sample if it hasn't changed.
                    if (samplingMode == DataGrabber.SamplingModes.TransitionsOnly)
                    {
                        // If the sample number as rolled-over 16 bits, send a roll-over marker.
                        if (i > 0 && (i & 0xffff) == 0)
                        {
                            sampleData.Add((byte)LogicAnalyzer.Filters.TimestampFilter.Markers.Rollover);
                            sampleData.Add((byte)((i >> 16) & 0xff));
                            sampleData.Add((byte)((i >> 24) & 0xff));
                            sampleData.Add(0);
                        }

                        if (i > 0 && bits == prevBits)
                            continue;

                        prevBits = bits;

                        sampleData.Add((byte)LogicAnalyzer.Filters.TimestampFilter.Markers.Sample);
                        sampleData.Add((byte)(i & 0xff));
                        sampleData.Add((byte)((i >> 8) & 0xff));
                    }
                    else
                    {
                        // Check if we can stack more samples per byte.
                        // Note that transition-only mode does not stack samples.
                        if (this.samplingChannels <= 4)
                        {
                            stackedBits |= (byte)(bits << stackShift);
                            stackShift += channelShift;

                            // If we don't yet have a full byte, wait for the next sample
                            // before queuing it.
                            if (stackShift < 8)
                                continue;

                            bits = stackedBits;
                            stackShift = 0;
                            stackedBits = 0;
                        }
                    }

                    sampleData.Add(bits);
                    if (sampleData.Count >= 1000)
                    {
                        BroadcastDataReceived(sampleData.ToArray());
                        sampleData.Clear();
                    }
                }

                // Send the last data buffer.
                if (sampleData.Count > 0)
                {
                    BroadcastDataReceived(sampleData.ToArray());
                    sampleData.Clear();
                }
            }
            catch (Exception ex)
            {
                BroadcastError(ex);
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handle this event to receive data from the test device.
        /// </summary>
        public event EventHandler<TestControllerEventArgs> OnDataReceived;

        /// <summary>
        /// Broadcast a message signalling to listeners that data has been received.
        /// </summary>
        /// <param name="Data">A string data to send</param>
        protected void BroadcastDataReceived(string Data)
        {
            EventHandler<TestControllerEventArgs> handler = OnDataReceived;

            if (handler != null)
                handler(this, new TestControllerEventArgs(Data));
        }

        /// <summary>
        /// Broadcast a message signalling to listeners that data has been received.
        /// </summary>
        /// <param name="Data">An array of bytes to send</param>
        protected void BroadcastDataReceived(byte[] Data)
        {
            EventHandler<TestControllerEventArgs> handler = OnDataReceived;

            if (handler != null)
                handler(this, new TestControllerEventArgs(Data));
        }

        /// <summary>
        /// Handle this event to trap asynchronous test device errors.
        /// </summary>
        public event EventHandler<System.IO.ErrorEventArgs> OnError;

        /// <summary>
        /// Broadcast an error message asynchronously to anyone who's listening.
        /// </summary>
        /// <param name="Message">The error message</param>
        protected void BroadcastError(string Message)
        {
            EventHandler<System.IO.ErrorEventArgs> handler = OnError;

            if (handler != null)
                handler(this, new System.IO.ErrorEventArgs(new Exception(Message)));
        }

        /// <summary>
        /// Broadcast an error message asynchronously to anyone who's listening.
        /// </summary>
        /// <param name="Ex">An exception object</param>
        protected void BroadcastError(Exception Ex)
        {
            EventHandler<System.IO.ErrorEventArgs> handler = OnError;

            if (handler != null)
                handler(this, new System.IO.ErrorEventArgs(Ex));
        }
        #endregion
    }
}