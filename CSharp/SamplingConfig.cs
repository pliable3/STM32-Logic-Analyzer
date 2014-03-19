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
using System.IO.Ports;
using LogicAnalyzer.DataAcquisition;

namespace LogicAnalyzer
{
    public partial class SamplingConfig : Form
    {
        private ViewModel viewModel;
        private static string[] rates = {"100 Hz", "200 Hz", "500 Hz", "1 kHz", "2 kHz", "5 kHz", "10 kHz",
                                        "20 kHz", "50 kHz", "100 kHz", "200 kHz", "500 kHz", "800 kHz"};

        public SamplingConfig(ViewModel viewModel)
        {
            string[] portNames;

            InitializeComponent();

            this.viewModel = viewModel;
            portNames = SerialPort.GetPortNames();
            if (portNames != null)
            {
                foreach (string p in portNames)
                    serialPortName.Items.Add(p);
            }

            for (int i = 8; i >= 1; i--)
                this.channels.Items.Add(i);

            foreach (string r in rates)
                this.samplingRate.Items.Add(r);
        }

        private void SamplingConfig_Load(object sender, EventArgs e)
        {
            if (viewModel.Settings.ControllerType == ViewModel.ControllerTypes.Test)
                this.testController.Checked = true;
            else
                this.serialPortController.Checked = true;

            if (viewModel.Settings.SamplingMode == DataAcquisition.DataGrabber.SamplingModes.TransitionsOnly)
                this.dataModeTransitions.Checked = true;
            else
                this.dataModeContinuous.Checked = true;
            this.compression.Checked = viewModel.Settings.SamplingCompression;

            this.serialPortName.SelectedItem = viewModel.Settings.SerialPortName;

            // Since a previously selected serial port may not be currently available...
            // If the selected serial port is not in the list of available ports, add
            // the selected port to the list and make it current.
            if ((this.serialPortName.SelectedIndex < 0) && !viewModel.Settings.SerialPortName.Equals(""))
            {
                this.serialPortName.Items.Insert(0, viewModel.Settings.SerialPortName);
                this.serialPortName.SelectedIndex = 0;
            }

            this.channels.SelectedItem = viewModel.Settings.SamplingChannels;

            string sr;

            if (viewModel.Settings.SamplingRate >= 1000)
                sr = (viewModel.Settings.SamplingRate / 1000) + " kHz";
            else
                sr = viewModel.Settings.SamplingRate + " Hz";
            this.samplingRate.SelectedItem = sr;

            this.samplingTime.Text = Convert.ToString(viewModel.Settings.SamplingTime);

            this.startTrigger.SelectedIndex = 0;
        }

        private void ok_Click(object sender, EventArgs e)
        {
            int v;

            if (!Int32.TryParse(this.samplingTime.Text, out v))
            {
                MessageBox.Show(this, "Invalid Sampling Time");
                return;
            }

            viewModel.Settings.ControllerType = (testController.Checked ? ViewModel.ControllerTypes.Test : ViewModel.ControllerTypes.Serial);
            viewModel.Settings.SamplingMode = (this.dataModeTransitions.Checked ? DataGrabber.SamplingModes.TransitionsOnly : DataGrabber.SamplingModes.Continuous);
            viewModel.Settings.SamplingCompression = this.compression.Checked;
            if (this.serialPortName.SelectedIndex >= 0)
                viewModel.Settings.SerialPortName = this.serialPortName.SelectedItem.ToString();
            viewModel.Settings.SamplingChannels = Convert.ToInt32(this.channels.SelectedItem);

            string p = this.samplingRate.SelectedItem.ToString();

            if (p.EndsWith("kHz"))
                v = Convert.ToInt32(p.Substring(0, p.Length - 3).Trim()) * 1000;
            else // if(p.EndsWith("Hz"))
                v = Convert.ToInt32(p.Substring(0, p.Length - 2).Trim());

            viewModel.Settings.SamplingRate = v;

            viewModel.Settings.SamplingTime = Convert.ToInt32(this.samplingTime.Text);

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void dataModeTransitions_CheckedChanged(object sender, EventArgs e)
        {
            if (dataModeTransitions.Checked)
            {
                compression.Checked = false;
                compression.Enabled = false;
            }
            else
                compression.Enabled = true;
        }

        private void serialPortController_CheckedChanged(object sender, EventArgs e)
        {
            if (testController.Checked)
            {
                compression.Checked = false;
                compression.Enabled = false;
                serialPortName.Enabled = false;
            }
            else
            {
                compression.Enabled = true;
                serialPortName.Enabled = true;
            }
        }
    }
}
