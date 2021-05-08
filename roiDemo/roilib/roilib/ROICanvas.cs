using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace roilib
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:roilib"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:roilib;assembly=roilib"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:CustomControl1/>
    ///
    /// </summary>
    public class ROICanvas : Canvas
    {
        OperateType operate = OperateType.None;
        Point lastPoint;
        private readonly ROIDrawingVisual roi;
        protected override int VisualChildrenCount => 1;
        protected override Visual GetVisualChild(int index)
        {
            return roi;
        }
        public ROICanvas()
        {
            roi = new ROIDrawingVisual();
            this.AddLogicalChild(roi);
            this.AddVisualChild(roi);
        }

        private bool HitPointTest(Point target, Point point)
        {
            double offset = 8;

            if (point.X > target.X + offset)
                return false;

            if (point.X < target.X - offset)
                return false;

            if (point.Y > target.Y + offset)
                return false;

            if (point.Y < target.Y - offset)
                return false;

            return true;
        }
        private bool HitCneterTest(DrawingVisual target, Point point)
        {
            return target.ContentBounds.Contains(point);
        }
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            operate = OperateType.None;
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            Point point = e.GetPosition(this);

            if (operate == OperateType.None)
            {
                if (HitPointTest(TopLeftP, point))
                {
                    this.Cursor = Cursors.SizeNWSE;
                    if (e.LeftButton == MouseButtonState.Pressed)
                        operate = OperateType.TopLeftDrag;
                }
                else if (HitPointTest(BottomRightP, point))
                {
                    this.Cursor = Cursors.SizeNWSE;

                    if (e.LeftButton == MouseButtonState.Pressed)
                        operate = OperateType.BottomRightDrag;
                }
                else if (HitCneterTest(roi, point))
                {
                    this.Cursor = Cursors.SizeAll;
                    if (e.LeftButton == MouseButtonState.Pressed)
                        operate = OperateType.CenterDrag;
                }
                else
                    this.Cursor = Cursors.Arrow;
            }

            switch (operate)
            {
                case OperateType.None:
                    break;

                case OperateType.TopLeftDrag:
                    TopLeftP = point;
                    break;

                case OperateType.BottomRightDrag:
                    BottomRightP = point;
                    break;

                case OperateType.CenterDrag:
                    double xOffset = (point.X - lastPoint.X);//右方向为正
                    double yOffset = (point.Y - lastPoint.Y);//下方向为正

                    if (TopLeftP.X == 0 && xOffset < 0)//不能往左 xOffset不能小于0
                        break;
                    if (TopLeftP.Y == 0 && yOffset < 0)//不能往上 yOffset不能小于0
                        break;
                    if (BottomRightP.X == this.ActualWidth && xOffset > 0)// 不能往右  xOffset不能大于0
                        break;
                    if (BottomRightP.Y == this.ActualHeight && yOffset > 0)// 不能往下 yOffset不能大于0
                        break;

                    var topLeft = CoerceTopLeftP(new Point(TopLeftP.X + xOffset, TopLeftP.Y + yOffset), this);
                    var bottomRight = CoerceBottomRightP(new Point(BottomRightP.X + xOffset, BottomRightP.Y + yOffset), this);

                    if (TopLeftP != topLeft)
                        TopLeftP = topLeft;

                    if (BottomRightP != bottomRight)
                        BottomRightP = bottomRight;
                    break;
            }

            lastPoint = point;
        }
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            Point point = e.GetPosition(this);

            if (HitCneterTest(roi, point))
            {
                double offset;
                if (e.Delta > 0)
                    offset = -1;
                else
                    offset = 1;

                this.TopLeftP = new Point(TopLeftP.X + offset, TopLeftP.Y + offset);
                this.BottomRightP = new Point(BottomRightP.X - offset, BottomRightP.Y - offset);
            }
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            lastPoint = e.GetPosition(this);
        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            operate = OperateType.None;
        }

        public Point BottomRightP
        {
            get
            {
                return ((Point)GetValue(BottomRightPProperty));
            }
            set
            {
                SetValue(BottomRightPProperty, value);
            }
        }
        // Using a DependencyProperty as the backing store for BottomRightP.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BottomRightPProperty =
            DependencyProperty.Register("BottomRightP", typeof(Point), typeof(ROICanvas),
                new FrameworkPropertyMetadata(new Point(0, 0),
                    new PropertyChangedCallback(OnBottomRightPPropertyChanged)));

        private static void OnBottomRightPPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ROICanvas canvas = (ROICanvas)d;
            Point value = ((Point)e.NewValue);

            Point coercedValue = CoerceBottomRightP(value, canvas);
            if (coercedValue != value)
            {
                canvas.BottomRightP = coercedValue;
            }

            canvas.roi.Draw(canvas.TopLeftP, canvas.BottomRightP);
        }

        private static Point CoerceBottomRightP(Point point, ROICanvas canvas)
        {
            if (point.X < canvas.TopLeftP.X)
            {
                point.X = canvas.TopLeftP.X;
            }

            if (point.Y < canvas.TopLeftP.Y)
            {
                point.Y = canvas.TopLeftP.Y;
            }

            if (point.X > canvas.ActualWidth)
                point.X = canvas.ActualWidth;
            if (point.Y > canvas.ActualHeight)
                point.Y = canvas.ActualHeight;

            return point;
        }

        public Point TopLeftP
        {
            get { return (Point)GetValue(TopLeftPProperty); }
            set
            {
                SetValue(TopLeftPProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TopLeftPProperty =
            DependencyProperty.Register("TopLeftP", typeof(Point), typeof(ROICanvas),
                new FrameworkPropertyMetadata(new Point(0, 0),
                    new PropertyChangedCallback(OnTopLeftPPropertyChanged)));

        private static void OnTopLeftPPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ROICanvas canvas = (ROICanvas)d;
            Point value = ((Point)e.NewValue);

            Point coercedValue = CoerceTopLeftP(value, canvas);
            if (coercedValue != value)
            {
                canvas.TopLeftP = coercedValue;
            }

            canvas.roi.Draw(canvas.TopLeftP, canvas.BottomRightP);
        }
        private static Point CoerceTopLeftP(Point point, ROICanvas canvas)
        {
            if (point.X > canvas.BottomRightP.X)
                point.X = canvas.BottomRightP.X;

            if (point.Y > canvas.BottomRightP.Y)
                point.Y = canvas.BottomRightP.Y;

            if (point.X < 0)
                point.X = 0;
            if (point.Y < 0)
                point.Y = 0;

            return point;
        }
    }
}

