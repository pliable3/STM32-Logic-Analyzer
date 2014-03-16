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
    public partial class CustomSignalPlotControl : UserControl
    {
        private const int HighStateYValue = 5;
        private const int LowStateYValue = 45;

        private List<SampleSignal> Signal;

        // We don't want these to be 'properties' because the IDE will add them to
        // the values that can be set in the visual editor (and initialize them to zero).
        public int LeftSampleTick = 0;
        public int PixelsPerSampleTick = 1;

        public CustomSignalPlotControl()
        {
            InitializeComponent();
        }

        public void Plot(List<SampleSignal> Signal)
        {
            this.Signal = Signal;
            this.Invalidate();
        }

        private int SampleTicksToPixels(int SampleTicks)
        {
            return SampleTicks * PixelsPerSampleTick;
        }

        private int PixelsToSampleTicks(int Pixels)
        {
            return Pixels / PixelsPerSampleTick;
        }

        private void CustomSignalPlotControl_Paint(object sender, PaintEventArgs e)
        {
            if (Signal != null && Signal.Count > 0)
            {
                int elapsedTime = 0;
                int i;
                int prevX, x, y = -1;
                int clipLeftSampleTick = this.LeftSampleTick + PixelsToSampleTicks(e.ClipRectangle.Left);
                int clipRightSampleTick = this.LeftSampleTick + PixelsToSampleTicks(e.ClipRectangle.Right);

                // Find our starting point.
                for (i = 0; i < Signal.Count; i++)
                {
                    if (clipLeftSampleTick < (elapsedTime + Signal[i].Duration))
                        break;

                    elapsedTime += Signal[i].Duration;
                }

                if (i < Signal.Count)
                {
                    prevX = SampleTicksToPixels(elapsedTime - this.LeftSampleTick);

                    // Plot.
                    for (; i < Signal.Count; i++)
                    {
                        if (y >= 0)
                            e.Graphics.DrawLine(Pens.Red, prevX, LowStateYValue, prevX, HighStateYValue);

                        x = SampleTicksToPixels((elapsedTime + Signal[i].Duration) - this.LeftSampleTick);
                        y = (Signal[i].SampleState == SampleSignal.State.High ? HighStateYValue : LowStateYValue);
                        e.Graphics.DrawLine(Pens.Red, prevX, y, x, y);
                        elapsedTime += Signal[i].Duration;

                        if (clipRightSampleTick <= elapsedTime)
                            break;

                        prevX = x;
                    }
                }
            }
        }

        private void CustomSignalPlotControl_Resize(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        private void CustomSignalPlotControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (Signal != null)
            {
                int thisSampleTick = this.LeftSampleTick + PixelsToSampleTicks(e.X);
                int elapsedTime = 0;

                // If the mouse is hovered over transition point for this signal,
                // alert the parent component.

                // Check if we're on a transition point.
                for (int i = 0; i < Signal.Count; i++)
                {
                    if (thisSampleTick == elapsedTime)
                    {
                        OnMouseOverTransition(thisSampleTick, e.X);
                        break;
                    }

                    elapsedTime += Signal[i].Duration;
                }
            }
        }

        #region Events

        public event EventHandler<MouseOverTransitionEventArgs> MouseOverTransition;

        private void OnMouseOverTransition(int sampleTick , int xCoordinate)
        {
            EventHandler<MouseOverTransitionEventArgs> handler = MouseOverTransition;

            if (handler != null)
                handler(this, new MouseOverTransitionEventArgs(sampleTick, xCoordinate));
        }

        #endregion
    }
}
