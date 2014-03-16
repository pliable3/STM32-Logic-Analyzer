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
    /// Class defining state and duration of a digital signal.
    /// </summary>
    public class SampleSignal
    {
        public enum State
        {
            Low,
            High
        }

        #region Constructors

        /// <summary>
        /// Creates and initializes a SampleSignal object.
        /// </summary>
        /// <param name="SampleState">High or Low state of the signale</param>
        /// <param name="Duration">The duration of the signal (time units are arbitrary)</param>
        public SampleSignal(State SampleState, int Duration)
        {
            this.SampleState = SampleState;
            this.Duration = Duration;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets/Sets the High or Low state of the signal
        /// </summary>
        public State SampleState
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Set the duration of the signal (time units are arbitrary)
        /// </summary>
        public int Duration
        {
            get;
            set;
        }

        #endregion
    }
}