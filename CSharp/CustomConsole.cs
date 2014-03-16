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
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using LogicAnalyzer;

namespace LogicAnalyzer
{
    /// <summary>
    /// Class defining methods for a UserControl that uses a color (rich text) console window.
    /// </summary>
    public partial class CustomConsole : UserControl
    {
        /// <summary>
        /// Messages are queued for display here (unused at the moment).
        /// </summary>
        private Queue<MessageEventArgs> consoleQueue = new Queue<MessageEventArgs>(1000);

        /// <summary>
        /// Creates and initializes a CustomConsole (UserControl) object.
        /// </summary>
        public CustomConsole()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Add a message to the console window.
        /// </summary>
        /// <param name="Msg">The message object to send to the console window</param>
        public void AddMessage(MessageEventArgs Msg)
        {
            consoleQueue.Enqueue(Msg);
            DisplayMessage(Msg);
        }

        /// <summary>
        /// Add a message to the console window.
        /// </summary>
        /// <param name="Message">The message text to send to the console window</param>
        /// <param name="MessageType">The type of message to send to the console window (mostly defining the way that the text displays)</param>
        public void AddMessage(string Message, MessageEventArgs.MessageTypes MessageType)
        {
            var msg = new MessageEventArgs(Message, MessageType);

            consoleQueue.Enqueue(msg);
            DisplayMessage(msg);
        }

        /// <summary>
        /// Clear the console window.
        /// </summary>
        public void Clear()
        {
            this.consoleQueue.Clear();
            this.consoleText.Text = "";
        }

        /// <summary>
        /// Append a message to the console window.
        /// </summary>
        /// <param name="Msg">The message object display to the console window</param>
        private void DisplayMessage(MessageEventArgs Msg)
        {
            Color clr;

            // For now, the message type only defines the color of the message.
            switch (Msg.MessageType)
            {
                case MessageEventArgs.MessageTypes.Error:
                    clr = Color.Red;
                    break;
                case MessageEventArgs.MessageTypes.Warning:
                    clr = Color.Pink;
                    break;
                case MessageEventArgs.MessageTypes.Important:
                    clr = Color.DarkGreen;
                    break;
                default:
                    clr = Color.Black;
                    break;
            }

            // These messages mostly arrive on a thread other than the UI thread.
            // If that it the case, use Invoke to run on the UI thread.
            if (this.consoleText.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    this.consoleText.SelectionColor = clr;
                    this.consoleText.AppendText(Msg.Message);
                    this.consoleText.ScrollToCaret();
                });
            }
            else
            {
                this.consoleText.SelectionColor = clr;
                this.consoleText.AppendText(Msg.Message);
                this.consoleText.ScrollToCaret();
            }
        }
    }
}
