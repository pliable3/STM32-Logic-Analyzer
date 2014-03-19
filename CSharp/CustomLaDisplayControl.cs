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

//#define ShowDashedTransitionLine

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using LogicAnalyzer.DataAcquisition;

namespace LogicAnalyzer
{
    /// <summary>
    /// Class defining methods to display graphically display the output of the Logic Analyzer.
    /// </summary>
    public partial class CustomLaDisplayControl : UserControl
    {
        // Constants
        private const int GridLineThickness = 1;
        private const int TransitionLineThickness = 1;
        private const int PlotOffset = 25;
        private const int PlotHeight = 50;
        private const int HighStateYValue = 5;
        private const int LowStateYValue = 45;

        /// <summary>
        /// The list of sample signals to plot.
        /// </summary>
        private List<SampleSignal>[] Signals;

        // If 'ShowDashedTransitionLine' is defined, a white dashed-line will be shown whenever
        // the user hovers the cursor over a transition line in the grid. The MouseMove and Paint
        // messages occur so quickly that sometimes there are several dashed-lines or black areas
        // where the line used to be. Revisit this.
#if ShowDashedTransitionLine
        private int mouseOverX = -1;
        private Pen dashedPen;
#endif

        private int totalSampleTicks;
        private Pen gridPen;
        private Font gridFont;
        private Brush gridBrush;

        private int SamplingRate;
        private int TicksPerGridLine;
        private int MicrosPerGridLine;

        public int LeftSampleTick = 0;
        public int PixelsPerSampleTick = 16; // Zoom value (16 is 1:1)

        #region Constructors

        /// <summary>
        /// Creates and initializes a CustomLaDisplayControl (UserControl) object.
        /// </summary>
        public CustomLaDisplayControl()
        {
            InitializeComponent();

            // Create our pens and fonts only once.
            gridPen = new Pen(Brushes.Yellow, GridLineThickness);
            gridFont = new Font("Calibri", 12);
            gridBrush = new SolidBrush(Color.GreenYellow);

#if ShowDashedTransitionLine
            dashedPen = new Pen(Brushes.White, GridLineThickness);
            dashedPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
#endif
            Clear();
        }

        /// <summary>
        /// Clear the sample array and the display.
        /// </summary>
        public void Clear()
        {
            Signals = new List<SampleSignal>[8];
            Invalidate();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Plot an array of samples.
        /// </summary>
        /// <param name="Samples">The array of samples to plot</param>
        public void Plot(SamplePlot Samples)
        {
            int i = 0;
            int sampleTicks = 0;

            Signals = new List<SampleSignal>[Samples.SampleSignals.Length];

            foreach (List<SampleSignal> sampleList in Samples.SampleSignals)
            {
                if (i == 0)
                {
                    // Find the duration of the first sample (they are all equal).
                    foreach (SampleSignal ss in sampleList)
                        sampleTicks += ss.Duration;

                    totalSampleTicks = sampleTicks;
                }
                Signals[i++] = sampleList;
            }

            // Set the scrollbar.
            // NOTE: This could overflow if the ticks are too large.
            hScrollBar1.Maximum = totalSampleTicks;
            hScrollBar1.Value = 0;
            Invalidate();
        }

        /// <summary>
        /// Calculates the scales used to plot samples whenever display size,
        /// sampling rate, or Zoom changes.
        /// </summary>
        private void CalculateScale()
        {
            if (this.SamplingRate > 0)
            {
                int tpg;

                // Start with an estimate of the ticks per grid line...
                tpg = PixelsToSampleTicks(this.Width - this.vScrollBar1.Width) / 10;

                // Get the microseconds per grid line from above.
                MicrosPerGridLine = (int)(1000000.0 * tpg / this.SamplingRate);

                // Adjust the grid lines so they are even second, milliseconds or microseconds.
                if (MicrosPerGridLine > 1000000)
                {
                    MicrosPerGridLine /= 1000000;
                    MicrosPerGridLine *= 1000000;
                }
                else if (MicrosPerGridLine > 1000)
                {
                    MicrosPerGridLine /= 1000;
                    MicrosPerGridLine *= 1000;
                }

                // Readjust the ticks per grid line to reflect the adjustment above.
                TicksPerGridLine = (int)(MicrosPerGridLine * (this.SamplingRate / 1000000.0));
            }
        }

        /// <summary>
        /// Set the sampling rate for the samples that will be plotted
        /// </summary>
        /// <param name="SamplingRate">The sampling rate</param>
        public void SetSamplingRate(int SamplingRate)
        {
            this.SamplingRate = SamplingRate;
            CalculateScale();
        }

        /// <summary>
        /// Set the zoom back to its original state.
        /// </summary>
        public void ResetZoom()
        {
            this.PixelsPerSampleTick = 16;
            CalculateScale();
        }

        /// <summary>
        /// Zoom in.
        /// </summary>
        public void ZoomIn()
        {
            if (this.PixelsPerSampleTick < 1024)
            {
                this.PixelsPerSampleTick *= 2;
                CalculateScale();
                this.Invalidate();
            }
        }

        /// <summary>
        /// Zoom out.
        /// </summary>
        public void ZoomOut()
        {
            if (this.PixelsPerSampleTick > 1)
            {
                this.PixelsPerSampleTick /= 2;
                CalculateScale();
                this.Invalidate();
            }
        }

        /// <summary>
        /// Format a number for display on the grid.
        /// </summary>
        /// <param name="v">A number to be formatted</param>
        /// <returns>A string representing the number</returns>
        private string FormatAndTrim(double v)
        {
            return v.ToString("###0.###");
        }

        /// <summary>
        /// Convert microseconds to text (us, ms, or s).
        /// </summary>
        /// <param name="Microseconds">Time in microseconds</param>
        /// <returns>Text representing the time</returns>
        private string MicrosToText(int Microseconds)
        {
            string p;

            if (Microseconds >= 1000000)
                p = string.Concat(FormatAndTrim(Microseconds / 1000000.0), " s");
            else if (Microseconds >= 1000)
                p = string.Concat(FormatAndTrim(Microseconds / 1000.0), " ms");
            else
                p = string.Concat(FormatAndTrim(Microseconds), " us");
            return p;
        }

        /// <summary>
        /// Convert Ticks (1 Tick = 1 Sample) to text
        /// </summary>
        /// <param name="SampleTicks">The time in ticks</param>
        /// <returns>The time as text</returns>
        private string TicksToText(int SampleTicks)
        {
            return MicrosToText((int)(1000000.0 * SampleTicks / this.SamplingRate));
        }

        /// <summary>
        /// Convert Ticks (1 Tick = 1 Sample) to pixels using the current zoom.
        /// </summary>
        /// <param name="SampleTicks">The time in ticks</param>
        /// <returns>The pixel representing the time</returns>
        private int SampleTicksToPixels(int SampleTicks)
        {
            return SampleTicks * PixelsPerSampleTick / 16;
        }

        /// <summary>
        /// Convert pixels to Ticks (1 Tick = 1 Sample) using the current zoom.
        /// </summary>
        /// <param name="Pixels">The display pixel</param>
        /// <returns>The time (in Ticks) represented by the pixel</returns>
        private int PixelsToSampleTicks(int Pixels)
        {
            return (int)(Pixels / (PixelsPerSampleTick / 16.0));
        }

        /// <summary>
        /// Paint the plot within the clip region.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CustomLaDisplayControl_Paint(object sender, PaintEventArgs e)
        {
            int clipLeftSampleTick = this.LeftSampleTick + PixelsToSampleTicks(e.ClipRectangle.Left);
            int clipRightSampleTick = this.LeftSampleTick + PixelsToSampleTicks(e.ClipRectangle.Right);
            int elapsedTime = 0;
            int micros = 0;
            int x;

#if ShowDashedTransitionLine
            int mx = mouseOverX;
#endif

            if (MicrosPerGridLine > 0 && TicksPerGridLine > 0)
            {
                // Draw the grid lines in yellow.
                while (elapsedTime <= clipRightSampleTick)
                {
                    if (elapsedTime >= clipLeftSampleTick)
                    {
                        x = SampleTicksToPixels(elapsedTime - this.LeftSampleTick);

                        // NOTE: this draws the entire line, but should only draw within the clip region.
                        e.Graphics.DrawLine(gridPen, x, 0, x, this.Height);

                        // Show the time associated with the grid line.
                        if (e.ClipRectangle.Top < 25)
                            e.Graphics.DrawString(TicksToText(elapsedTime), gridFont, gridBrush, x - 20, 2);
                    }
                    elapsedTime += this.TicksPerGridLine;
                    micros += MicrosPerGridLine;
                }
            }

            if (Signals != null)
            {
                // Now, plot each signal within the clip region.
                int yOffset = PlotOffset;

                foreach (List<SampleSignal> Signal in Signals)
                {
                    if (Signal != null)
                    {
                        if (e.ClipRectangle.Top <= yOffset && e.ClipRectangle.Bottom >= yOffset)
                        {
                            int et, prevX = -1, y = -1;

                            elapsedTime = 0;

                            foreach (SampleSignal ss in Signal)
                            {
                                et = elapsedTime + ss.Duration;

                                // If the current sample time is within the displayable area, plot it.
                                if (clipLeftSampleTick < et)
                                {
                                    if (prevX < 0)
                                        prevX = SampleTicksToPixels(elapsedTime - this.LeftSampleTick);

                                    // Draw a transition between low and high.
                                    if (y >= 0)
                                        e.Graphics.DrawLine(Pens.Red, prevX, yOffset + LowStateYValue, prevX, yOffset + HighStateYValue);

                                    // Draw a line between the previous X and the current X.
                                    x = SampleTicksToPixels(et - this.LeftSampleTick);
                                    y = yOffset + (ss.SampleState == SampleSignal.State.High ? HighStateYValue : LowStateYValue);
                                    e.Graphics.DrawLine(Pens.Red, prevX, y, x, y);
                                    prevX = x;
                                }

                                // If we're off the right side of the window, stop.
                                if (clipRightSampleTick < et)
                                    break;

                                elapsedTime = et;
                            }
                        }
                    }

                    // Skip down to the next signal.
                    yOffset += PlotHeight;
                }

#if ShowDashedTransitionLine
                // If there is a transition line to paint, do it now.
                if (mx >= 0 && e.ClipRectangle.Left <= mx && e.ClipRectangle.Right >= mx)
                    e.Graphics.DrawLine(dashedPen, mx, 0, mx, this.Height);
#endif
            }
        }

        /// <summary>
        /// Scrollbar event handler. This just sets the Tick of the left side of the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            this.LeftSampleTick = e.NewValue;
            this.Invalidate();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Mouse movement event handler. This is used to display the time of the cursor location (an
        /// event is broadcast to anyone listening that the cursor location has changed).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CustomLaDisplayControl_MouseMove(object sender, MouseEventArgs e)
        {
#if ShowDashedTransitionLine
            bool mouseIsOver = false;
#endif

            if (Signals != null)
            {
                // Determine which channel area we're in.
                int channel = (e.Y - PlotOffset) / PlotHeight;

                if (channel < Signals.Length && Signals[channel] != null)
                {
                    int thisSampleTick = this.LeftSampleTick + PixelsToSampleTicks(e.X);

                    // Tell anyone who's listening what is under the cursor.
                    BroadcastOnMouseOver(channel + 1, TicksToText(thisSampleTick));

#if ShowDashedTransitionLine
                    int elapsedTime = 0;
                    List<SampleSignal> Signal = Signals[channel];

                    // If the mouse is hovered over transition point for this signal,
                    // we want to show a dashed line.

                    // Check if we're on a transition point.
                    foreach (SampleSignal ss in Signal)
                    {
                        if (thisSampleTick == elapsedTime)
                        {
                            mouseOverX = e.X;
                            mouseIsOver = true;
                            this.Invalidate(new Rectangle(mouseOverX, 0, 1, this.Height));
                            break;
                        }
                        else if (thisSampleTick < elapsedTime)
                            break;

                        elapsedTime += ss.Duration;
                    }
#endif
                }
            }

#if ShowDashedTransitionLine
            if (!mouseIsOver)
            {
                // Invalidate the previous dashed-line.
                if (mouseOverX >= 0)
                    this.Invalidate(new Rectangle(mouseOverX - 1, 0, 2, this.Height));
                mouseOverX = -1;
            }
#endif
        }

        /// <summary>
        /// Window re-size event handler. This re-calculates the scale of the plot.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CustomLaDisplayControl_Resize(object sender, EventArgs e)
        {
            CalculateScale();
            this.Invalidate();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handle this event to receive a message when the mouse moves over a signal plot.
        /// This can be used to display the channel and time that the cursor is hovering over.
        /// </summary>
        public event EventHandler<LaMouseOverEventArgs> OnMouseOver;

        /// <summary>
        /// Broadcast a message signalling to listeners that mouse has moved over a new channel and/or time.
        /// </summary>
        /// <param name="Channel">The channel that the cursor is hovering over</param>
        /// <param name="Time">The time that the cursor is hovering over</param>
        protected void BroadcastOnMouseOver(int Channel, string Time)
        {
            EventHandler<LaMouseOverEventArgs> handler = OnMouseOver;

            if (handler != null)
                handler(this, LaMouseOverEventArgs.GetInstance(Channel, Time));
        }

        #endregion
    }
}