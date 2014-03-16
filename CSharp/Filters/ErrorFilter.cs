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

namespace LogicAnalyzer.Filters
{
    /// <summary>
    /// Class defining methods for an error message data filter.
    /// </summary>
    public class ErrorFilter : AbstractDataFilter<byte>
    {
        // These tags signify the beginning and ending of an error message.
        internal static byte[] ErrorTagStart = System.Text.Encoding.ASCII.GetBytes("<err>");
        internal static byte[] ErrorTagStop = System.Text.Encoding.ASCII.GetBytes("</err>");

        private bool inErrorTag;
        private List<byte> errorMessage;

        #region Constructors

        /// <summary>
        /// Creates and initializes an error message filter. Error messages in a data stream are
        /// delimited by <err> and </err>.
        /// </summary>
        public ErrorFilter()
        {
            inErrorTag = false;
            errorMessage = new List<byte>();
        }

        #endregion

        #region Overridden Methods

        /// <summary>
        /// Write a value, that was previously thought to be part of a delimiter, to
        /// the filter output.
        /// </summary>
        /// <param name="Data">The data value being sent through the filter</param>
        public override void WriteTagTesterValue(byte Data)
        {
            // The data is part of a partial tag that needs to be re-processed.
            // If we're inside an error tag, add the data to the error message.
            // Otherwise, just pass the data through to the parent filter (or output).
            if (inErrorTag)
                errorMessage.Add(Data);
            else
                base.Write(Data);
        }

        /// <summary>
        /// Write a value to the error message filter. Delimiters will be discarded and
        /// the data within them is used to send an error message.
        /// </summary>
        /// <param name="Data">The data value being sent through the filter</param>
        public override void Write(byte Data)
        {
            if (inErrorTag)
            {
                // Check for the termination tag.
                if (!this.TagTester.ValueInTagCode(ErrorTagStop, Data))
                {
                    // If we're inside an error tag, add the data to the error message.
                    errorMessage.Add(Data);
                }
                else if (this.TagTester.Length == ErrorTagStart.Length)
                {
                    // Found the 'error stop tag'.

                    // Throw the </err> tag away.
                    inErrorTag = false;
                    this.TagTester.Clear();

                    // Throw an exception with the error message.
                    throw new Exception(new System.Text.ASCIIEncoding().GetString(errorMessage.ToArray()));
                }
            }
            else
            {
                // Check for the start tag.
                if (!this.TagTester.ValueInTagCode(ErrorTagStart, Data))
                {
                    // If we're not in error message mode, just pass the data through.
                    base.Write(Data);
                }
                else if (this.TagTester.Length == ErrorTagStart.Length)
                {
                    // Found the 'error start tag'.

                    // Throw the <err> tag away.
                    this.TagTester.Clear();
                    inErrorTag = true;
                }
            }
        }

        #endregion
    }
}
