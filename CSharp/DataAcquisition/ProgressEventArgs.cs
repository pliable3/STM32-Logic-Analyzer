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

namespace LogicAnalyzer.DataAcquisition
{
    /// <summary>
    /// Class defining EventArgs for progress events.
    /// </summary>
    public class ProgressEventArgs : EventArgs
    {
        #region Constructors

        /// <summary>
        /// Creates and initializes a ProgressEventArgs object.
        /// </summary>
        public ProgressEventArgs()
        {
        }

        /// <summary>
        /// Creates and initializes a ProgressEventArgs object.
        /// </summary>
        /// <param name="PercentComplete">Percent progress made</param>
        /// <param name="BytesReceived">Number of bytes received</param>
        /// <param name="CompressionPercent">Data compression rate so far</param>
        public ProgressEventArgs(int PercentComplete, int BytesReceived, int CompressionPercent)
        {
            this.PercentComplete = PercentComplete;
            this.BytesReceived = BytesReceived;
            this.CompressionPercent = CompressionPercent;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the number of bytes received
        /// </summary>
        public int BytesReceived
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the data compression rate so far
        /// </summary>
        public int CompressionPercent
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the percent progress made
        /// </summary>
        public int PercentComplete
        {
            get;
            private set;
        }

        #endregion
    }
}