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
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using LogicAnalyzer.Controllers;
using LogicAnalyzer.DataAcquisition;
using LogicAnalyzer;

namespace LogicAnalyzer
{
    /// <summary>
    /// Class defining methods to communicate between a user-interface and the Logic Analyzer logic.
    /// </summary>
    public class ViewModel
    {
        /// <summary>
        /// Controller types
        /// </summary>
        public enum ControllerTypes
        {
            Serial,
            Ethernet, // unused
            Test
        }

        /// <summary>
        /// Class to define settings that can be saved/loaded via serialization.
        /// </summary>
        public class ConfigSettings : IXmlSerializable
        {
            #region Constructors

            public ConfigSettings()
            {
            }

            #endregion

            #region Properties

            /// <summary>
            /// Gets/Sets the baud rate used by a serial=type controller
            /// </summary>
            public int BaudRate
            {
                get;
                set;
            }

            /// <summary>
            /// Gets/Sets the type of controller
            /// </summary>
            public ControllerTypes ControllerType
            {
                get;
                set;
            }

            /// <summary>
            /// Gets/Sets the number of channels to sample
            /// </summary>
            public int SamplingChannels
            {
                get;
                set;
            }

            /// <summary>
            /// Gets/Sets whether samples are compressed or not
            /// </summary>
            public bool SamplingCompression
            {
                get;
                set;
            }

            /// <summary>
            /// Gets/Set the sampling mode
            /// </summary>
            public DataGrabber.SamplingModes SamplingMode
            {
                get;
                set;
            }

            /// <summary>
            /// Gets/Set the sampling rate (in samples per second)
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

            /// <summary>
            /// Gets/Sets the port name for a serial-type controller
            /// </summary>
            public string SerialPortName
            {
                get;
                set;
            }

            #endregion

            #region XmlSerialization Interface

            public System.Xml.Schema.XmlSchema GetSchema()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Reads our settings from an XML reader
            /// </summary>
            /// <param name="reader">an XML reader</param>
            public void ReadXml(System.Xml.XmlReader reader)
            {
                if (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName.Equals("ConfigSettings"))
                {
                    BaudRate = Convert.ToInt32(reader["BaudRate"]);
                    ControllerType = reader["ControllerType"].Equals(ControllerTypes.Test.ToString()) ? ControllerTypes.Test : ControllerTypes.Serial;
                    SamplingChannels = Convert.ToInt32(reader["SamplingChannels"]);
                    SamplingCompression = Convert.ToBoolean(reader["SamplingCompression"]);
                    SamplingMode = reader["SamplingMode"].Equals(DataGrabber.SamplingModes.TransitionsOnly.ToString()) ? DataGrabber.SamplingModes.TransitionsOnly : DataGrabber.SamplingModes.Continuous;
                    SamplingRate = Convert.ToInt32(reader["SamplingRate"]);
                    SamplingTime = Convert.ToInt32(reader["SamplingTime"]);
                    SerialPortName = reader["SerialPortName"];
                }
            }

            /// <summary>
            /// Writes our settings to an XML writer
            /// </summary>
            /// <param name="writer">an XML writer</param>
            public void WriteXml(System.Xml.XmlWriter writer)
            {
                writer.WriteAttributeString("BaudRate", BaudRate.ToString());
                writer.WriteAttributeString("ControllerType", ControllerType.ToString());
                writer.WriteAttributeString("SamplingChannels", SamplingChannels.ToString());
                writer.WriteAttributeString("SamplingCompression", SamplingCompression.ToString());
                writer.WriteAttributeString("SamplingMode", SamplingMode.ToString());
                writer.WriteAttributeString("SamplingRate", SamplingRate.ToString());
                writer.WriteAttributeString("SamplingTime", SamplingTime.ToString());
                writer.WriteAttributeString("SerialPortName", SerialPortName);
            }

            #endregion
        }

        // Default settings.
        private const string defaultSerialPortName = "COM10";
        private const int defaultBaudRate = 921600;
        private const int defaultSamplingChannels = 8;
        private const DataGrabber.SamplingModes defaultSamplingMode = DataGrabber.SamplingModes.TransitionsOnly;
        private const int defaultSamplingRate = 50000;
        private const int defaultSamplingTime = 1000;
        private const bool defaultSamplingCompression = false;

        private DataGrabber grabber;
        private AbstractController Controller;

        #region Constructors

        /// <summary>
        /// Creates and initializes a Logic Analyzer ViewModel object.
        /// </summary>
        public ViewModel()
        {
            this.Settings = new ConfigSettings();
            this.Settings.ControllerType = ControllerTypes.Serial;
            setDefaults();
        }

        /// <summary>
        /// Set our properties from the defaults.
        /// </summary>
        private void setDefaults()
        {
            this.Settings.SerialPortName = defaultSerialPortName;
            this.Settings.BaudRate = defaultBaudRate;
            this.Settings.SamplingRate = defaultSamplingRate;
            this.Settings.SamplingChannels = defaultSamplingChannels;
            this.Settings.SamplingMode = defaultSamplingMode;
            this.Settings.SamplingTime = defaultSamplingTime;
            this.Settings.SamplingCompression = defaultSamplingCompression;
            this.ConfigChanged = false;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current Logic Analyzer settings
        /// </summary>
        public ConfigSettings Settings
        {
            get;
            internal set;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Opens the selected device controller and prepares for sampling
        /// </summary>
        public void Open()
        {
            Close();

            switch (this.Settings.ControllerType)
            {
                case ControllerTypes.Serial:
                    Controller = new SerialController(this.Settings.SerialPortName, this.Settings.BaudRate, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
                    break;
                case ControllerTypes.Test:
                    Controller = new TestController("Test Controller", new Test.LaTestDevice());
                    break;
                default:
                    throw new NotImplementedException("Unknown controller");
            }

            grabber = new DataGrabber(Controller, this.Settings.SamplingRate, this.Settings.SamplingChannels, this.Settings.SamplingTime, this.Settings.SamplingMode, this.Settings.SamplingCompression);
            grabber.OnComplete += grabber_Complete;
            grabber.OnProgress += grabber_Progress;
            grabber.OnError += grabber_Error;
            grabber.OnConsoleMessage += grabber_ConsoleMessage;
            grabber.Open();
        }
        
        /// <summary>
        /// Closes the device controller, if it is open
        /// </summary>
        public void Close()
        {
            if (grabber != null)
            {
                grabber.Close();
                grabber = null;
            }
        }

        /// <summary>
        /// Requests the firmware version/copyright from the device controller
        /// </summary>
        public void FirmwareRevision()
        {
            if (grabber == null)
                throw new Exception("DataGrabber not initialized");

            grabber.FirmwareRevision();
        }

        /// <summary>
        /// Pings the device controller
        /// </summary>
        public void PingController()
        {
            if (grabber == null)
                throw new Exception("DataGrabber not initialized");

            grabber.PingController();
        }

        /// <summary>
        /// Initiate sampling from the device controller
        /// </summary>
        public void StartSampling()
        {
            if (grabber == null)
                throw new Exception("DataGrabber not initialized");

            grabber.SamplingRate = this.Settings.SamplingRate;
            grabber.SamplingChannels = this.Settings.SamplingChannels;
            grabber.SamplingMode = this.Settings.SamplingMode;
            grabber.SamplingTime = this.Settings.SamplingTime;
            grabber.SamplingCompression = this.Settings.SamplingCompression;
            BroadcastStatusMessage("Sampling Started\r\n", MessageEventArgs.MessageTypes.Important);
            grabber.StartSampling();
        }

        #endregion

        #region Configuration Properties and Methods

        /// <summary>
        /// Get the default data path where configuration settings are saved
        /// </summary>
        private string AppDataPath
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\LogicAnalyzer";
            }
        }

        /// <summary>
        /// Gets/Sets whether the configuration settings have changed
        /// </summary>
        public bool ConfigChanged
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the name of the current config. file
        /// </summary>
        public string ConfigName
        {
            get;
            set;
        }

        /// <summary>
        /// Check if the app data path exists, and if not, create it.
        /// </summary>
        private void CheckAppDataPathExists()
        {
            if (!Directory.Exists(this.AppDataPath))
                Directory.CreateDirectory(this.AppDataPath);
        }

        /// <summary>
        /// Check if the config. settings have been saved.
        /// </summary>
        /// <param name="Parent">the parent form, or null</param>
        /// <returns>'true' if the settings have been saved</returns>
        private bool CheckSave(Form Parent)
        {
            if (ConfigChanged)
            {
                switch (MessageBox.Show("The configuration was changed. Do you want to save it before proceeding?", "Save Configuration?", MessageBoxButtons.YesNoCancel))
                {
                    case DialogResult.OK:
                        if (!SaveConfig(Parent, true))
                            return false;
                        break;
                    case DialogResult.Cancel:
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Clear the configuration settings (but prompt the user if they want to save the old settings)
        /// </summary>
        /// <param name="Parent">the parent form, or null</param>
        /// <returns>'true' is successful</returns>
        public bool NewConfig(Form Parent)
        {
            if (!CheckSave(Parent))
                return false;

            setDefaults();
            return true;
        }

        /// <summary>
        /// Loads the named configuration file
        /// </summary>
        /// <param name="Parent">the parent form, or null</param>
        /// <param name="FileName">the config. file to load</param>
        /// <returns>'true' if successful</returns>
        private bool LoadConfig(Form Parent, string FileName)
        {
            XmlReader reader = null;
            XmlSerializer serializer;
            ControllerTypes ct = Settings.ControllerType;

            try
            {
                reader = XmlReader.Create(FileName);
                serializer = new XmlSerializer(typeof(ConfigSettings));
                this.Settings = (ConfigSettings)serializer.Deserialize(reader);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Parent, "Error doung config load: " + ex.Message);
                return false;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }

            // If the controller type changed, close/re-open the controller.
            if (ct != this.Settings.ControllerType)
            {
                Close();
                Open();
            }

            this.ConfigChanged = false;
            return true;
        }

        /// <summary>
        /// Loads a configuration file after prompting the user to select a file
        /// </summary>
        /// <param name="Parent">the parent form, or null</param>
        public void LoadConfig(Form Parent)
        {
            if (!CheckSave(Parent))
                return;

            CheckAppDataPathExists();

            OpenFileDialog ofd = new OpenFileDialog();

            ofd.InitialDirectory = AppDataPath;
            ofd.FileName = this.ConfigName;
            ofd.DefaultExt = "lacfg";
            if (ofd.ShowDialog(Parent) == DialogResult.OK)
            {
                if (!LoadConfig(Parent, ofd.FileName))
                    return;

                this.ConfigName = ofd.FileName;
            }
        }

        /// <summary>
        /// Saves the named configuration file
        /// </summary>
        /// <param name="Parent">the parent form, or null</param>
        /// <param name="FileName">the config. file to save</param>
        /// <returns>'true' if successful</returns>
        private bool SaveConfig(Form Parent, string FileName)
        {
            XmlWriter writer = null;
            XmlSerializer serializer;

            try
            {
                writer = XmlWriter.Create(FileName);
                serializer = new XmlSerializer(typeof(ConfigSettings));
                serializer.Serialize(writer, this.Settings);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Parent, "Error doung config save: " + ex.Message);
                return false;
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }

            this.ConfigChanged = false;
            return true;
        }

        /// <summary>
        /// Save a configuration settings file (optionally prompted for a file name)
        /// </summary>
        /// <param name="Parent">the parent form, or null</param>
        /// <param name="SaveAs">'true' to prompt the user for a new file name</param>
        /// <returns>'true' if successful</returns>
        public bool SaveConfig(Form Parent, bool SaveAs)
        {
            string fileName;

            if ((ConfigName == null) || ConfigName.Equals("") || SaveAs)
            {
                CheckAppDataPathExists();

                SaveFileDialog sfd = new SaveFileDialog();

                sfd.InitialDirectory = AppDataPath;
                sfd.FileName = this.ConfigName;
                sfd.DefaultExt = "lacfg";
                if (sfd.ShowDialog(Parent) != DialogResult.OK)
                    return false;
                fileName = sfd.FileName;
            }
            else
                fileName = this.ConfigName;

            if (SaveConfig(Parent, fileName))
            {
                this.ConfigName = fileName;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determine if it is ok to exit the program
        /// </summary>
        /// <param name="Parent">the parent form, or null</param>
        /// <returns>'true' if it is ok to exit</returns>
        public bool OkExit(Form Parent)
        {
            return CheckSave(Parent);
        }

        #endregion

        #region Events

        /// <summary>
        /// Handle this event to get status messages from the controller.
        /// </summary>
        public event EventHandler<MessageEventArgs> OnStatusMessage;

        /// <summary>
        /// Broadcast a status event to anyone who's listening
        /// </summary>
        /// <param name="Message"></param>
        /// <param name="MessageType"></param>
        private void BroadcastStatusMessage(string Message, MessageEventArgs.MessageTypes MessageType)
        {
            EventHandler<MessageEventArgs> handler = OnStatusMessage;

            if (handler != null)
                handler(this, new MessageEventArgs(Message, MessageType));
        }

        /// <summary>
        /// Handle this event to get progress messages from the controller.
        /// </summary>
        public event EventHandler<ProgressEventArgs> OnProgress;

        /// <summary>
        /// Broadcast a progress event to anyone who's listening
        /// </summary>
        /// <param name="e"></param>
        private void BroadcastProgress(ProgressEventArgs e)
        {
            EventHandler<ProgressEventArgs> handler = OnProgress;

            // Just pass the progress event through.
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Handle this event to get error messages from the controller.
        /// </summary>
        public event EventHandler<ErrorEventArgs> OnError;

        /// <summary>
        /// Broadcast an error event to anyone who's listening
        /// </summary>
        /// <param name="args"></param>
        protected void BroadcastError(ErrorEventArgs args)
        {
            EventHandler<ErrorEventArgs> handler = OnError;

            // Just pass the error event through.
            if (handler != null)
                handler(this, args);
        }

        /// <summary>
        /// Handle this event to get plot (sampling complete) messages from the controller.
        /// </summary>
        public event EventHandler<PlotEventArgs> OnPlot;

        /// <summary>
        /// Broadcast a plot event to anyone who's listening
        /// </summary>
        /// <param name="samples"></param>
        private void BroadcastPlot(SamplePlot samples)
        {
            EventHandler<PlotEventArgs> handler = OnPlot;

            if (handler != null)
                handler(this, new PlotEventArgs(samples));
        }

        #endregion

        #region DataGrabber Event Handlers

        /// <summary>
        /// Handles the data acquisition 'Complete' event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grabber_Complete(object sender, ProgressEventArgs e)
        {
            // Send a console message...
            BroadcastStatusMessage("Sampling Complete\r\n", MessageEventArgs.MessageTypes.Important);

            // and tell our listeners to plot the data.
            BroadcastPlot(new SamplePlot(grabber.Data.ToArray(), grabber.SamplingChannels));
        }

        /// <summary>
        /// Handles the data acquisition 'ConsoleMessage' event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grabber_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            // Just pass the even on the our listeners
            BroadcastStatusMessage(e.Message, MessageEventArgs.MessageTypes.Generic);
        }

        /// <summary>
        /// Handles the data acquisition 'Error' event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grabber_Error(object sender, ErrorEventArgs e)
        {
            // Just pass the even on the our listeners
            BroadcastError(e);
        }

        /// <summary>
        /// Handles the data acquisition 'Progress' event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grabber_Progress(object sender, ProgressEventArgs e)
        {
            // Just pass the even on the our listeners
            BroadcastProgress(e);
        }

        #endregion
    }
}