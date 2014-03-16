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

namespace LogicAnalyzer
{
    /// <summary>
    /// Class defining EventArgs for Message objects.
    /// </summary>
    public class MessageEventArgs : EventArgs 
    {
        /// <summary>
        /// Message types.
        /// </summary>
        public enum MessageTypes
        {
            Generic,
            Error,
            Warning,
            Important
        }

        /// <summary>
        /// Creates and initializes a MessageEventArgs object.
        /// </summary>
        /// <param name="Message">The message text to send to the console window</param>
        /// <param name="MessageType">The type of message to send to the console window (mostly defining the way that the text displays)</param>
        public MessageEventArgs(string Message, MessageTypes MessageType)
        {
            this.Message = Message;
            this.MessageType = MessageType;

        }

        /// <summary>
        /// The message text to send to the console window
        /// </summary>
        public string Message
        {
            get;
            private set;
        }

        /// <summary>
        /// The type of message to send to the console window (mostly defining the way that the text displays)
        /// </summary>
        public MessageTypes MessageType
        {
            get;
            private set;
        }
    }
}
