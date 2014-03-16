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
using LogicAnalyzer.DataAcquisition;

namespace LogicAnalyzer
{
    /// <summary>
    /// Class defining EventArgs for SamplePlot objects.
    /// </summary>
    public class PlotEventArgs : EventArgs
    {
        /// <summary>
        /// Creates and initializes a PlotEventArgs object.
        /// </summary>
        /// <param name="Samples">A SamplePlot object representing several arrays of samples</param>
        public PlotEventArgs(SamplePlot Samples)
        {
            this.Samples = Samples;
        }

        /// <summary>
        /// >A SamplePlot object representing several arrays of samples
        /// </summary>
        public SamplePlot Samples
        {
            get;
            internal set;
        }
    }
}
