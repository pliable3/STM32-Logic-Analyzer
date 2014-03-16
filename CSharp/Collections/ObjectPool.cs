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
    /// A thread-safe, generic Object Pool that allows for re-use of well-used and/or
    /// resource-hungry objects. Objects that make use of an Object Pool must implement
    /// the IRecyclable interface and must have a parameterless constructor.
    /// They should also have a Dispose() method that calls the RecycleObject()
    /// method. Failing to do so will result in objects not being reused (and
    /// will actually use more resources).
    /// </summary>
    /// <typeparam name="T">The type of the objects in the object pool</typeparam>
    public class ObjectPool<T> : IDisposable where T : IRecyclable, new()
    {
        // Object references are held in a stack for re-use later.
        private Stack<T> objectCache = new Stack<T>(1024);

        // Diagnostic information to monitor how efficient the pool is.
        public int objNew = 0, objReused = 0, maxStack = 0;

        #region Methods

        /// <summary>
        /// Returns an object. If there is and object in the pool, then it will be
        /// removed and returned after being initialized by the object's RecycleInit()
        /// method. If there are no objects in the pool, then a new object will be
        /// created.
        /// </summary>
        /// <returns>An object from the pool or a newly created object.</returns>
        public T GetObject()
        {
            T obj = default(T);
            bool reused = false;

            // Lock to make sure that the pool is thread-safe.
            lock(objectCache)
            {
                if(objectCache.Count > 0)
                {
                    obj = objectCache.Pop();
                    reused = true;
                }
            }

            // If there was an object in the pool, return it.
            if (reused)
            {
                // Initialize the object as if it was newly constructed.
                obj.RecycleInit();
                objReused++;
                return obj;
            }

            objNew++;

            // Return a new "T" object if there were no more objects on the cache stack.
            return new T();
        }

        /// <summary>
        /// Adds an object (that is no longer needed) to the pool so that it can be re-used
        /// (recycled) later. Call this method from th object's Dispose() method.
        /// Note that the object's RecycleDenit() method will be called automatically.
        /// </summary>
        /// <param name="Object">The object to be added to the pool.</param>
        public void RecycleObject(T Object)
        {
            Object.RecycleDenit();

            // Lock to make sure that the pool is thread-safe.
            lock (objectCache)
            {
                 objectCache.Push(Object);

                if(objectCache.Count > maxStack)
                    maxStack = objectCache.Count;
            }
        }

        /// <summary>
        /// Dispose of the object pool & dispose of objects in the pool that
        /// support IDisposable
        /// </summary>
        public void Dispose()
        {
            lock(objectCache)
            {
                while(objectCache.Count > 0)
                {
                    using(objectCache.Pop() as IDisposable)
                    {
                    }
                }
            }
        }

        #endregion
    }
}
