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
using LogicAnalyzer.Collections;

namespace LogicAnalyzer.Controllers
{
    /// <summary>
    /// Class defining EventArgs for Controller objects. Note that ControllerEventArgs
    /// is an Object Pool object to reduce object creation. These objects should call
    /// Dispose() when they are no longer needed.
    /// </summary>
    public class ControllerEventArgs : EventArgs, IRecyclable
    {
        // Create a static object pool which will hold references to ControllerEventArgs
        // objects so they can be recycled.
        private static ObjectPool<ControllerEventArgs> objectPool = new ObjectPool<ControllerEventArgs>();

        #region Constructors

        /// <summary>
        /// This constructor is not meant to be called externally. Use the static method GetInstance() instead.
        /// The constructor cannot be made 'private', however, because the ObjectPool needs to be able to
        /// instantiate ControllerEventArgs objects.
        /// </summary>
        public ControllerEventArgs()
        {
        }

        public static ControllerEventArgs GetInstance(string ControllerName, string Message)
        {
            ControllerEventArgs args = objectPool.GetObject();

            args.ControllerName = ControllerName;
            args.Message = Message;
            args.TimeStamp = DateTime.Now;
            return args;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the controller name that initiated the event.
        /// </summary>
        public string ControllerName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a message associated with the event (perhaps an error message).
        /// </summary>
        public string Message
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the times that the event occurred.
        /// </summary>
        public DateTime TimeStamp
        {
            get;
            private set;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Dispose of (recycle) the object.
        /// </summary>
        public void Dispose()
        {
            // Recycle this object instead of destroying it.
            objectPool.RecycleObject(this);
        }

        #endregion

        #region IRecyclable

        /// <summary>
        /// Method that gets called when an object is recycled.
        /// </summary>
        public void RecycleDenit()
        {
            //
        }

        /// <summary>
        /// Method that gets called when an old object becomes reused.
        /// </summary>
        public void RecycleInit()
        {
            //
        }

        #endregion
    }
}