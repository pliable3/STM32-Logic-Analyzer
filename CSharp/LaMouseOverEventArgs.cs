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

namespace LogicAnalyzer
{
    public class LaMouseOverEventArgs : EventArgs, IRecyclable
    {
        private static ObjectPool<LaMouseOverEventArgs> objectPool = new ObjectPool<LaMouseOverEventArgs>();

        #region Constructors

        /// <summary>
        /// This constructor is not meant to be called externally. Use the static method GetInstance() instead.
        /// The constructor cannot be made 'private', however, because the ObjectPool needs to be able to
        /// instantiate LaMouseOverEventArgs objects.
        /// </summary>
        public LaMouseOverEventArgs()
        {
        }

        public static LaMouseOverEventArgs GetInstance(int Channel, string Time)
        {
            LaMouseOverEventArgs args = objectPool.GetObject();

            args.Channel = Channel;
            args.Time = Time;
            return args;
        }

        #endregion

        #region Properties

        public int Channel
        {
            get;
            internal set;
        }

        public string Time
        {
            get;
            internal set;
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            // Recycle this object instead of destroying it.
            objectPool.RecycleObject(this);
        }

        #endregion

        #region IRecyclable

        public void RecycleDenit()
        {
            //
        }

        public void RecycleInit()
        {
            //
        }

        #endregion
    }
}