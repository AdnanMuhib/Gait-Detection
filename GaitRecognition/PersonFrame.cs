using Emgu.CV;
using Emgu.CV.Structure;
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

        public PersonFrame() {
            top_left = new Point(0,0);
            top_right = new Point(0,0);
            bottom_left = new Point(0,0);
            bottom_right = new Point(0,0);
            width = 0;
            height = 0;
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
    }
}
