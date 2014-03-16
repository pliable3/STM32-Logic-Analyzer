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

namespace LogicAnalyzer.Collections
{
    /// <summary>
    /// Interface definining methods that ObjectPool consumers must implement.
    /// </summary>
    public interface IRecyclable : IDisposable
    {
        /// <summary>
        /// A method that allows objects to release resources prior to this
        /// object being recycled. Note: If this object is a collection of
        /// IRecycleable objects, those objects will be recycled during this call.
        /// </summary>
        void RecycleDenit();

        /// <summary>
        /// A method that allows objects to clear settings so that a recycled object
        /// will be the same as a newly constructed object.
        /// </summary>
        void RecycleInit();
    }
}
