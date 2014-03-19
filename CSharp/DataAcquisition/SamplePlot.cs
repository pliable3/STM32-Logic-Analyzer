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
    /// Class defining methods for turning an array of sampled bytes into separate arrays of SampleSignal objects (one
    /// for each input channel).
    /// </summary>
    public class SamplePlot
    {
        private int samplesPerByte;
        private int sampleShift;
        private SampleSignal[] currentChannelSignal;

        #region Constructors

        /// <summary>
        /// Creates and initalizes a SamplePlot object
        /// </summary>
        /// <param name="Samples">Raw sampled data from the input device</param>
        /// <param name="Channels">The number of channels being sampled (when 4 or fewer, sample data is 'stacked')</param>
        /// <param name="StackedSamples">'true' if more than one sample is stacked in each byte</param>
        public SamplePlot(byte[] Samples, int Channels, bool StackedSamples)
        {
            if (StackedSamples)
            {
                // When the number of channels is 4 or fewer, samples are 'stacked'. So here
                // we define the number of samples per byte and shift needed to get to the next
                // sample.
                switch (Channels)
                {
                    case 1:
                        samplesPerByte = 8;
                        sampleShift = 1;
                        break;
                    case 2:
                        samplesPerByte = 4;
                        sampleShift = 2;
                        break;
                    case 3:
                    case 4:
                        samplesPerByte = 2;
                        sampleShift = 4;
                        break;
                    default:
                        if (Channels < 1 || Channels > 8)
                            throw new Exception("Channels must be in the range 1 - 8");
                        samplesPerByte = 1;
                        sampleShift = 0;
                        break;
                }
            }
            else
            {
                samplesPerByte = 1;
                sampleShift = 0;
            }

            this.Channels = Channels;
            this.SampleSignals = new List<SampleSignal>[Channels];
            currentChannelSignal = new SampleSignal[Channels];

            for (int c = 0; c < Channels; c++)
            {
                // Initialize each channel low, 0 duration.
                currentChannelSignal[c] = new SampleSignal(SampleSignal.State.Low, 0);
                this.SampleSignals[c] = new List<SampleSignal>(2048);
            }

            // Build the sample arrays...
            buildSampleSignals(Samples);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the resulting signal plots for each channel.
        /// </summary>
        public List<SampleSignal>[] SampleSignals
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the number of channels being sampled (when 4 or fewer, sample data is 'stacked')
        /// </summary>
        public int Channels
        {
            get;
            internal set;
        }

        /// <summary>
        /// 'true' if more than one sample is stacked in each byte
        /// </summary>
        public bool StackedSamples
        {
            get;
            internal set;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Add a signal to the Channel's list.
        /// </summary>
        /// <param name="channel"></param>
        private void addSignal(int channel)
        {
            SampleSignal ss = currentChannelSignal[channel];

            if (ss.Duration > 0)
            {
                SampleSignals[channel].Add(ss);

                // Create a new current signal on this channel with the opposite flavor.
                // Duration = 1.
                currentChannelSignal[channel] = new SampleSignal(ss.SampleState == SampleSignal.State.High ? SampleSignal.State.Low : SampleSignal.State.High, 1);
            }
        }

        /// <summary>
        /// Process one sample of Channels-wide bits.
        /// </summary>
        /// <param name="sample"></param>
        private void processSample(byte sample)
        {
            bool stateChanged;

            for (int c = 0; c < Channels; c++)
            {
                // Check if the state for this channel changed.
                if ((sample & (1 << c)) != 0)
                    stateChanged = (currentChannelSignal[c].SampleState == SampleSignal.State.Low);
                else
                    stateChanged = (currentChannelSignal[c].SampleState == SampleSignal.State.High);

                // If the state (high/low) changed from the last sample, then add a new signal.
                // Otherwise, just increase the duration of the previous signal.
                if (stateChanged)
                    addSignal(c);
                else
                    currentChannelSignal[c].Duration++;
            }
        }

        /// <summary>
        /// Build the signale arrays from the raw sample data.
        /// </summary>
        /// <param name="Samples">Raw sampled data from the input device</param>
        private void buildSampleSignals(byte[] Samples)
        {
            byte shiftedSampleByte;

            foreach (byte sampleByte in Samples)
            {
                shiftedSampleByte = sampleByte;
                for (int s = 0; s < samplesPerByte; s++)
                {
                    processSample(shiftedSampleByte);
                    shiftedSampleByte >>= sampleShift;
                }
            }

            // Finish up.
            for (int c = 0; c < Channels; c++)
                addSignal(c);
        }

        #endregion
    }
}
