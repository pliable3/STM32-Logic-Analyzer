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
    /// Interface defining methods that a test device must implement. This is used
    /// by the TestController class to allow it to communicate with a test device.
    /// </summary>
    public interface ITestDevice
    {
        void Close();
        bool IsOpen();
        bool Open();
        void Write(byte[] Bytes);

        event EventHandler<TestControllerEventArgs> OnDataReceived;
        event EventHandler<System.IO.ErrorEventArgs> OnError;
    }
}