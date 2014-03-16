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

namespace LogicAnalyzer.Controllers
{
    /// <summary>
    /// Class defining EventArgs for TestController objects.
    /// </summary>
    public class TestControllerEventArgs : EventArgs
    {
        private static System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

        #region Constructors

        /// <summary>
        /// Creates and initializes a TestControllerEventArgs object.
        /// </summary>
        /// <param name="Data">Data to send to the test device</param>
        public TestControllerEventArgs(string Data)
        {
            this.Data = enc.GetBytes(Data);
        }

        /// <summary>
        /// Creates and initializes a TestControllerEventArgs object.
        /// </summary>
        /// <param name="Data">Data to send to the test device</param>
        public TestControllerEventArgs(byte[] Data)
        {
            this.Data = Data;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets data to send to the test device
        /// </summary>
        public byte[] Data
        {
            get;
            internal set;
        }

        #endregion
    }
}
