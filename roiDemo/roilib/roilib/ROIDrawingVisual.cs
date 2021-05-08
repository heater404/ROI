using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace roilib
{
    public class ROIDrawingVisual : DrawingVisual
    {
        Pen pen = new Pen(Brushes.Black, 1);
        public void Draw(Point topLeft, Point bottomRight)
        {
            using (DrawingContext dc = this.RenderOpen())
            {
                dc.DrawRectangle(Brushes.LightGray, pen, new Rect(topLeft, bottomRight));
                dc.DrawEllipse(Brushes.LightGray, pen, topLeft, 5, 5);
                dc.DrawEllipse(Brushes.LightGray, pen, bottomRight, 5, 5);
            }
        }
    }
}
