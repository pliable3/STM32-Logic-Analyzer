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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using LogicAnalyzer.DataAcquisition;

namespace LogicAnalyzer
{
    /// <summary>
    /// Class defining methods to interact with the Logic Analyzer logic and display results. The form
    /// is only loosely coupled with the logic via the ViewModel object. It is also fairly loosely
    /// coupled with the two custom components (CustomConsole and CustomLaDisplayControl).
    /// Interaction between this form, the viewModel and the components is done with events/listeners.
    /// </summary>
    public partial class MainForm : Form
    {
        /// <summary>
        /// The ViewModel keeps us separated from the program logic. It defines events that we may listen to
        /// in order to make things happen on the screen.
        /// </summary>
        private ViewModel viewModel;

        #region Constructors

        /// <summary>
        /// Creates and initializes a MainForm object.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Form Load event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            // Create our viewModel
            viewModel = new ViewModel();

            // Show the default settings.
            this.activeChannels.Text = Convert.ToString(viewModel.Settings.SamplingChannels);
            this.samplingRate.Text = Convert.ToString(viewModel.Settings.SamplingRate);

            customLaDisplayControl1.SetSamplingRate(viewModel.Settings.SamplingRate);

            // Wire-up the mouse-over event so we can tell when to change the channel and time.
            customLaDisplayControl1.OnMouseOver += customLaDisplayControl1_OnMouseOver;

            // Wire-up the event handlers for status, progress, errors, and plots.
            viewModel.OnStatusMessage += viewModel_StatusMessage;
            viewModel.OnProgress += viewModel_Progress;
            viewModel.OnPlot += viewModel_Plot;
            viewModel.OnError += viewModel_Error;

            // This will attempt to open the controller.
            viewModel.Open();
        }

        /// <summary>
        /// FormClosing event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // If it's not ok to exit, make the user wait.
            if (!viewModel.OkExit(this))
            {
                e.Cancel = true;
                return;
            }

            viewModel.Close();
        }

        /// <summary>
        /// Start sampling from the device.
        /// </summary>
        private void commonStartSampling()
        {
            viewModel.StartSampling();
            this.progressBar.Value = 0;
            this.progressBar.Visible = true;
            this.customLaDisplayControl1.ResetZoom();
        }

        /// <summary>
        /// Start Sampling button event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void startSampling_Click(object sender, EventArgs e)
        {
            commonStartSampling();
        }

        #endregion

        #region ViewModel Event Handlers

        /// <summary>
        /// Status Message event handle.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void viewModel_StatusMessage(object sender, MessageEventArgs e)
        {
            // Just pass the message on to the console.
            this.laConsole.AddMessage(e.Message, e.MessageType);
        }

        /// <summary>
        /// Progress event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void viewModel_Progress(object sender, ProgressEventArgs e)
        {
            // NOTE: If there is an error before we get to 100%, the progress bar will never be hidden.

            // This event likely occurred on a thread other than the UI thread.
            // If that it the case, use Invoke to run on the UI thread.
            if (this.progressBar.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    this.progressBar.Value = e.PercentComplete;
                    this.compressionPercent.Text = e.CompressionPercent + "%";
                });
            }
            else
            {
                this.progressBar.Value = e.PercentComplete;
                this.compressionPercent.Text = e.CompressionPercent + "%";
            }
        }

        /// <summary>
        /// Error event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void viewModel_Error(object sender, ErrorEventArgs e)
        {
            // Display the error.
            this.laConsole.AddMessage(e.GetException().Message + "\r\n", MessageEventArgs.MessageTypes.Error);
        }

        /// <summary>
        /// Plot event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void viewModel_Plot(object sender, PlotEventArgs e)
        {
            this.progressBar.Visible = false;
            customLaDisplayControl1.SetSamplingRate(viewModel.Settings.SamplingRate);
            this.customLaDisplayControl1.Plot(e.Samples);
        }

        #endregion

        #region CustomLaDisplayControl Event Handlers

        /// <summary>
        /// CustomLaDisplayControl Mouse Over event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void customLaDisplayControl1_OnMouseOver(object sender, LaMouseOverEventArgs e)
        {
            this.statChannel.Text = e.Channel.ToString();
            this.statTime.Text = e.Time;

            e.Dispose(); // Recycle.
        }

        /// <summary>
        /// CustomLaDisplayControl Mouse Leave event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void customLaDisplayControl1_MouseLeave(object sender, EventArgs e)
        {
            this.statChannel.Text = "";
            this.statTime.Text = "";
        }

        #endregion

        #region Menu Items

        // No comments here -- these are self-explanatory.

        private void configureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SamplingConfig sc = new SamplingConfig(viewModel);
            ViewModel.ControllerTypes ct = viewModel.Settings.ControllerType;

            if (sc.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                viewModel.ConfigChanged = true;

                // If the controller type changed, close/re-open the controller.
                if (ct != viewModel.Settings.ControllerType)
                {
                    viewModel.Close();
                    viewModel.Open();
                }
            }

            sc.Dispose();

        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (viewModel.NewConfig(this))
                customLaDisplayControl1.Clear();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewModel.LoadConfig(this);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (viewModel.SaveConfig(this, true))
                laConsole.AddMessage("Settings Saved\r\n", MessageEventArgs.MessageTypes.Generic);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (viewModel.SaveConfig(this, false))
                laConsole.AddMessage("Settings Saved\r\n", MessageEventArgs.MessageTypes.Generic);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void firmwareRevisionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewModel.FirmwareRevision();
        }

        private void pingTheControllerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewModel.PingController();
        }

        private void zoomInToolStripButton_Click(object sender, EventArgs e)
        {
            customLaDisplayControl1.ZoomIn();
        }

        private void zoomOutToolStripButton_Click(object sender, EventArgs e)
        {
            customLaDisplayControl1.ZoomOut();
        }

        #endregion
    }
}
