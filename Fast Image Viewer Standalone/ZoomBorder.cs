﻿using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FIVStandard.Utils
{
    public class ZoomBorder : Border
    {
        private UIElement child = null;
        private Point origin;
        private Point start;

        private TranslateTransform GetTranslateTransform(UIElement element)
        {
            return (TranslateTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is TranslateTransform);
        }

        private ScaleTransform GetScaleTransform(UIElement element)
        {
            return (ScaleTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is ScaleTransform);
        }

        public override UIElement Child
        {
            get { return base.Child; }
            set
            {
                if (value != null && value != this.Child)
                    this.Initialize(value);
                base.Child = value;
            }
        }

        public void Initialize(UIElement element)
        {
            this.child = element;
            if (child != null)
            {
                TransformGroup group = new TransformGroup();
                ScaleTransform st = new ScaleTransform();
                group.Children.Add(st);
                TranslateTransform tt = new TranslateTransform();
                group.Children.Add(tt);
                child.RenderTransform = group;
                child.RenderTransformOrigin = new Point(0.0, 0.0);
                this.MouseWheel += Child_MouseWheel;
                this.MouseLeftButtonDown += Child_MouseLeftButtonDown;
                this.MouseLeftButtonUp += Child_MouseLeftButtonUp;
                this.MouseMove += Child_MouseMove;
                //this.PreviewMouseRightButtonDown += Child_PreviewMouseRightButtonDown;
            }
        }

        public void Reset()
        {
            if (child != null)
            {
                // reset zoom
                var st = GetScaleTransform(child);
                st.ScaleX = 1.0;
                st.ScaleY = 1.0;

                // reset pan
                var tt = GetTranslateTransform(child);
                tt.X = 0.0;
                tt.Y = 0.0;
            }
        }

        #region Child Events
        private void Child_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (child != null)
            {
                var st = GetScaleTransform(child);
                var tt = GetTranslateTransform(child);

                double zoom = e.Delta > 0 ? Properties.Settings.Default.ZoomSensitivity : -Properties.Settings.Default.ZoomSensitivity;
                if (!(e.Delta > 0) && (st.ScaleX < .4 || st.ScaleY < .4))
                    return;

                Point relative = e.GetPosition(child);
                double abosuluteX;
                double abosuluteY;

                abosuluteX = relative.X * st.ScaleX + tt.X;
                abosuluteY = relative.Y * st.ScaleY + tt.Y;

                st.ScaleX += zoom;
                st.ScaleY += zoom;

                //zoom clamp max
                if (st.ScaleX > 50.0)
                    st.ScaleX = 50.0;
                if (st.ScaleY > 50.0)
                    st.ScaleY = 50.0;

                //zoom clamp min
                if (st.ScaleX < 1.0)
                    st.ScaleX = 1.0;
                if (st.ScaleY < 1.0)
                    st.ScaleY = 1.0;

                //snap panning back to center
                if (st.ScaleX == 1.0 && st.ScaleY == 1.0)
                {
                    tt.X = 0.0;
                    tt.Y = 0.0;
                }
                else
                {
                    tt.X = abosuluteX - relative.X * st.ScaleX;
                    tt.Y = abosuluteY - relative.Y * st.ScaleY;

                    //var windowBorder = new Rect(child.RenderSize);
                    //ClampPan(ref tt, ref windowBorder);
                }
            }
        }

        private void Child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (child != null)
            {
                var tt = GetTranslateTransform(child);
                start = e.GetPosition(this);
                origin = new Point(tt.X, tt.Y);
                this.Cursor = Cursors.Hand;
                child.CaptureMouse();
            }
        }

        private void Child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (child != null)
            {
                child.ReleaseMouseCapture();
                this.Cursor = Cursors.Arrow;
            }
        }

        void Child_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Reset();
        }

        private void Child_MouseMove(object sender, MouseEventArgs e)
        {
            if (child != null)
            {
                if (child.IsMouseCaptured)
                {
                    var tt = GetTranslateTransform(child);
                    Vector v = start - e.GetPosition(this);

                    var st = GetScaleTransform(child);
                    if(st.ScaleX == 1.0 && st.ScaleY == 1.0)
                    {
                        tt.X = 0.0;
                        tt.Y = 0.0;
                    }
                    else
                    {
                        tt.X = origin.X - v.X;
                        tt.Y = origin.Y - v.Y;

                        //var windowBorder = new Rect(child.RenderSize);

                        //ClampPan(ref tt, ref windowBorder);

                        //MainWindow.AppWindow.Title = $"{tt.Y.ToString("F0")} --- {windowBorder.Top.ToString("F0")} | {windowBorder.Bottom.ToString("F0")}";//DEBUG
                    }
                }
            }
        }
        #endregion

        private void ClampPan(ref TranslateTransform tt, ref Rect r)//TODO: get proper coords
        {
            if ((tt.X) > r.Right / 2)//left
                tt.X = r.Right / 2;
            if (tt.X < -r.Right)//right
                tt.X = -r.Right;

            if ((tt.Y) > r.Bottom / 2)//top
                tt.Y = r.Bottom / 2;
            if (tt.Y < -r.Bottom)//bottom
                tt.Y = -r.Bottom;
        }
    }
}