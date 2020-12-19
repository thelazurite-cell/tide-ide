using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DockingLibrary
{
    internal class DockPanelSplitter : Border
    {
        private SplitOrientation _split;
        private FrameworkElement _prevControl;
        private FrameworkElement _nextControl;

        public DockPanelSplitter(FrameworkElement prevControl, FrameworkElement nextControl, SplitOrientation split)
        {
            this._prevControl = prevControl;
            this._nextControl = nextControl;
            this._split = split;
            this.Background = new SolidColorBrush(Colors.LightGray);

            if (this._split == SplitOrientation.Vertical)
            {
                this.Cursor = Cursors.SizeWE;
                this.Width = 5;
            }
            else
            {
                this.Cursor = Cursors.SizeNS;
                this.Height = 5;
            }
        }

        private Point ptStartDrag;

        private Point AdjustControls(Point delta)
        {
            //Console.WriteLine(delta);
            if (this._split == SplitOrientation.Vertical)
            {
                if (delta.X > 0 && this._nextControl != null)
                {
                    if (this._nextControl.ActualWidth - delta.X < this._nextControl.MinWidth)
                        delta.X = this._nextControl.ActualWidth - this._nextControl.MinWidth;
                }
                else if (delta.X < 0 && this._prevControl != null)
                {
                    if (this._prevControl.ActualWidth + delta.X < this._prevControl.MinWidth)
                        delta.X = this._prevControl.MinWidth - this._prevControl.ActualWidth;
                }

                if (this._prevControl!=null) this._prevControl.Width += delta.X;
                if (this._nextControl!=null) this._nextControl.Width -= delta.X;
            }
            else
            {
                if (delta.Y > 0 && this._nextControl!=null)
                {
                    if (this._nextControl.ActualHeight - delta.Y < this._nextControl.MinHeight)
                        delta.Y = this._nextControl.ActualHeight - this._nextControl.MinHeight;
                }
                else if (delta.Y < 0 && this._prevControl != null)
                {
                    if (this._prevControl.ActualHeight + delta.Y < this._prevControl.MinHeight)
                        delta.Y = this._prevControl.MinHeight - this._prevControl.ActualHeight;

                }
                if (this._prevControl != null) this._prevControl.Height += delta.Y;
                if (this._nextControl != null) this._nextControl.Height -= delta.Y;
            }

            return delta;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (!this.IsMouseCaptured)
            {
                //DockPanel parent = ((DockPanel)Parent);
                //int index = parent.Children.IndexOf(this);
                //if (index == 0 || index == parent.Children.Count - 1)
                //    return;
                //_prevControl = parent.Children[index - 1] as FrameworkElement;
                //_nextControl = parent.Children[index + 1] as FrameworkElement;


                this.ptStartDrag = e.GetPosition(this.Parent as IInputElement);
                this.CaptureMouse();
            }

            base.OnMouseDown(e);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (this.IsMouseCaptured)
            {
                var ptCurrontRelative = e.GetPosition(this);
                //if (ptCurrontRelative.X >= 0 && ptCurrontRelative.X <= Width)
                {
                    var ptCurrent = e.GetPosition(this.Parent as IInputElement);
                    var delta = new Point(ptCurrent.X - this.ptStartDrag.X, ptCurrent.Y - this.ptStartDrag.Y);

                    delta = this.AdjustControls(delta);

                    this.ptStartDrag = new Point(this.ptStartDrag.X+delta.X, this.ptStartDrag.Y+delta.Y);
                }
                
            }
            base.OnMouseMove(e);
        }
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (this.IsMouseCaptured)
            {
                var ptCurrent = e.GetPosition(this.Parent as IInputElement);
                var delta = new Point(ptCurrent.X - this.ptStartDrag.X , ptCurrent.Y - this.ptStartDrag.Y);
                this.AdjustControls(delta);
                this.ReleaseMouseCapture();
            }

            base.OnMouseUp(e);
        }
    }
}
