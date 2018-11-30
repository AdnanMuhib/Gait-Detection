using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.XImgproc;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaitRecognition
{
    class PersonFrame
    {
        public Point top_left;
        public Point top_right;
        public Point bottom_left;
        public Point bottom_right;
        public double width;
        public double height;
        public Line middle_line;
        public Rectangle rec;
        public PersonFrame() {
            top_left = new Point(0,0);
            top_right = new Point(0,0);
            bottom_left = new Point(0,0);
            bottom_right = new Point(0,0);
            middle_line = new Line();
            width = 0;
            height = 0;
        }

        public PersonFrame(Rectangle rec) {
            top_left = new Point(0, 0);
            top_right = new Point(0, 0);
            bottom_left = new Point(0, 0);
            bottom_right = new Point(0, 0);
            middle_line = new Line();

            top_left.X = rec.X;
            top_left.Y = rec.Y;
            top_right.X = rec.X + rec.Width;
            top_right.Y = rec.Y;
            bottom_left.X = rec.X;
            bottom_left.Y = rec.Y + rec.Height;
            bottom_right.X = rec.X + rec.Width;
            bottom_right.Y = rec.Y + rec.Height;
            calculate_width();
            calculate_height();
            middle_line.p1.X = rec.X;
            middle_line.p1.Y = rec.Y + rec.Height / 2;
            middle_line.p2.X = rec.X + rec.Width;
            middle_line.p2.Y = rec.Y + rec.Height / 2;
            middle_line.length = rec.Width;
        }
        
        public void calculate_width()
        {
            if (top_left.X != 0 && top_right.X != 0) {
                width = Math.Abs(top_right.X - top_left.X);
            }
        }

        public void calculate_height() {
            if (top_left.Y != 0 && bottom_left.Y != 0) {
                height = Math.Abs(bottom_left.Y - top_left.Y);
            }
        }

        // detect first white pixel from the Binary Image
        private Point DetectFirstWhitePixel(Image<Gray, byte> binaryImage, double w, double h) {
            Point whitePoint = new Point();
            bool pixelDetected = false ;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (binaryImage.Data[x, y, 0] == 255) // if a white pixel is spotted
                    {
                        whitePoint.X = x;
                        whitePoint.Y = y;
                        pixelDetected = true;
                        break;
                    }
                }

                if (pixelDetected) {
                    break;
                }
            }
            return whitePoint;
        }

        // find Corner Points of person from the Binary Image
        public void FindPoints(Image<Gray, byte> thinImage) {

            double w = thinImage.Width; // Width of the Image
            double h = thinImage.Height; // Height of the Image

            Point p = new Point();
            p = DetectFirstWhitePixel(thinImage, w, h);
            // consider that first white pixel has the coordinates of all points
            top_left.X = top_right.X = bottom_left.X = bottom_right.X = p.X;
            top_left.Y = top_right.Y = bottom_left.Y = bottom_right.Y = p.Y;

            // iterating throgh every pixel of the image
            for (int y = 0; y < h; y++) {
                for (int x = 0; x < w; x++) {
                    if (thinImage.Data[x, y, 0] == 255) { // if a white pixel is spotted
                        
                        // check if it is top left point
                        if (top_left.X >= x) {
                            top_left.X = x;
                            top_left.Y = y;
                        }

                        //check if it is top right point
                        if (top_right.X <= x)
                        {
                            top_right.X = x;
                            top_right.Y = y;
                        }

                        // if it bottom left point
                        if (bottom_left.Y <= y)
                        {
                            bottom_left.X = x;
                            bottom_left.Y = y;
                        }

                        // if bottom right point
                        if (bottom_right.X <= x) {
                            bottom_right.X = x;
                            bottom_right.Y = y;
                        }
                    }
                }
            }
        }

        // find the frame using largest Conture
        public  Rectangle findBoundry(Image<Gray, byte> output)
        {
            Image<Bgr, byte> colorImage = output.Convert<Bgr, byte>();
            Image<Gray, byte> thinning = new Image<Gray, byte>(output.Width, output.Height);
            double largest_area = 0.0;
            int largest_contour_index = 0;
            rec = new Rectangle();
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(output, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
            for (int i = 0; i < contours.Size; i++) // iterate through each contour. 
            {
                double a = CvInvoke.ContourArea(contours[i], false);
                if (a > largest_area)
                {
                    largest_area = a;
                    largest_contour_index = i;                //Store the index of largest contour
                    rec = CvInvoke.BoundingRectangle(contours[i]); // Find the bounding rectangle for biggest contour
                }
            }

            top_left.X = rec.X;
            top_left.Y = rec.Y;
            top_right.X = rec.X + rec.Width;
            top_right.Y = rec.Y;
            bottom_left.X = rec.X;
            bottom_left.Y = rec.Y + rec.Height;
            bottom_right.X = rec.X + rec.Width;
            bottom_right.Y = rec.Y + rec.Height;
            calculate_width();
            calculate_height();
            middle_line.p1.X = rec.X;
            middle_line.p1.Y = rec.Y + rec.Height / 2;
            middle_line.p2.X = rec.X + rec.Width;
            middle_line.p2.Y = rec.Y + rec.Height / 2;
            middle_line.length = rec.Width;
            return rec;

            //XImgprocInvoke.Thinning(output, thinning, ThinningTypes.ZhangSuen);
            //thinning = thinning.Not().Not();
            //thin.Image = thinning;
            //hough(thinning, bounding_rect);
        }
    }
}
