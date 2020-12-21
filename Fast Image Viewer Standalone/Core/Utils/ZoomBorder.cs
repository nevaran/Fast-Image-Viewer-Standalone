using System.Linq;
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

        public double ZoomSensitivity
        {
            get { return (double)GetValue(ZoomSensitivityProperty); }
            set { SetValue(ZoomSensitivityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ZoomSensitivity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ZoomSensitivityProperty =
            DependencyProperty.Register("ZoomSensitivity", typeof(double), typeof(ZoomBorder), new PropertyMetadata(0.3));

        private readonly DebounceDispatcher ddClamp = new DebounceDispatcher();

        private static TranslateTransform GetTranslateTransform(UIElement element)
        {
            return (TranslateTransform)((TransformGroup)element.RenderTransform).Children.First(tr => tr is TranslateTransform);
        }

        private static ScaleTransform GetScaleTransform(UIElement element)
        {
            return (ScaleTransform)((TransformGroup)element.RenderTransform).Children.First(tr => tr is ScaleTransform);
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

                this.SizeChanged += ZoomBorder_SizeChanged;
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

                double zoom = e.Delta > 0 ? ZoomSensitivity : -ZoomSensitivity;
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

                    OnClamp();
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

        private void Child_MouseMove(object sender, MouseEventArgs e)
        {
            if (child != null)
            {
                if (child.IsMouseCaptured)
                {
                    var tt = GetTranslateTransform(child);
                    Vector v = start - e.GetPosition(this);

                    tt.X = origin.X - v.X;
                    tt.Y = origin.Y - v.Y;

                    OnClamp();
                }
            }
        }

        private void ZoomBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ddClamp.Debounce(100, OnClamp);
        }
        #endregion

        private void OnClamp()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var st = GetScaleTransform(child);
                var tt = GetTranslateTransform(child);

                Rect r = new Rect(child.RenderSize);
                ClampPan(ref tt, r, st.ScaleX);
            });
        }

        private static void ClampPan(ref TranslateTransform tt, Rect r, double scale)
        {
            double leftLimit = r.Width * 0.5;
            double rightLimit = -(r.Width * scale - (r.Width * 0.5));
            double topLimit = r.Height * 0.5;
            double botLimit = -(r.Height * scale - (r.Height * 0.5));

            if (tt.X > leftLimit)//left
                tt.X = leftLimit;
            if (tt.X < rightLimit)//right
                tt.X = rightLimit;

            if (tt.Y > topLimit)//top
                tt.Y = topLimit;
            if (tt.Y < botLimit)//bottom
                tt.Y = botLimit;

            //System.Diagnostics.Debug.WriteLine($"{leftLimit:F1} | {rightLimit:F1} | {topLimit:F1} | {botLimit:F1}\n{tt.X:F1} | {tt.Y:F1}\nX {scaleX:F1} Y {scaleY:F1} :{System.DateTime.Now}");
        }

        /*void Child_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Reset();
        }*/
    }
}