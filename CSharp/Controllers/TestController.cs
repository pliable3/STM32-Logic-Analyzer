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
using System.Windows.Forms;
using LogicAnalyzer.Filters;

namespace LogicAnalyzer.Controllers
{
    /// <summary>
    /// A class defining a test device controller.
    /// </summary>
    public class TestController : AbstractController
    {

        #region Constructors

        /// <summary>
        /// Construct and initialize a test device controller object.
        /// </summary>
        /// <param name="Name">The name of the device</param>
        /// <param name="TestClient">A test device object which will communicate with this controller</param>
        public TestController(string Name, ITestDevice TestClient)
            : base(Name)
        {
            this.TestClient = TestClient;
            this.TestClient.OnDataReceived += TestClient_OnDataReceived;
            this.TestClient.OnError += TestClient_OnError;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a test device object which will communicate with this controller
        /// </summary>
        public ITestDevice TestClient
        {
            get;
            private set;
        }

        #endregion

        #region Overridden Methods

        /// <summary>
        /// Open the test device controller.
        /// </summary>
        /// <returns>'true' if successful.</returns>
        public override bool Open()
        {
            return this.TestClient.Open();
        }

        /// <summary>
        /// Close the test device controller.
        /// </summary>
        public override void Close()
        {
            this.TestClient.Close();
        }

        /// <summary>
        /// Determine if the test device is open.
        /// </summary>
        /// <returns></returns>
        public override bool IsOpen()
        {
            return this.TestClient.IsOpen();
        }

        /// <summary>
        /// Write an array of bytes to the test device. This method is only called internally
        /// when an array of (possibly filtered) data is ready to send to the device.
        /// </summary>
        /// <param name="bytes"></param>
        protected override void WriteToDevice(byte[] Bytes)
        {
            if (Bytes != null && Bytes.Length > 0)
                this.TestClient.Write(Bytes);
        }

        /// <summary>
        /// Dispose of the controller.
        /// </summary>
        public override void Dispose()
        {
            try
            {
                this.TestClient.Close();
            }
            catch { }

            base.Dispose();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler for test device OnDataReceived events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TestClient_OnDataReceived(object sender, TestControllerEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("CTRL REC: " + e.Data.Length);

            if (e.Data.Length > 0)
            {
                foreach (byte b in e.Data)
                    base.ReceiveFromDevice(b);

                // Tell our listeners that data has been received.
                BroadcastDataReceived();
            }
        }

        /// <summary>
        /// Handler for test device OnError events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TestClient_OnError(object sender, ErrorEventArgs e)
        {
            // Just re-broadcast the error to our listeners.
            BroadcastError(e.GetException().Message);
        }

        #endregion
    }
}
