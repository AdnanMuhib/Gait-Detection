using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaitRecognition
{
    class OpticalFlow
    {
        // dividing the complete frame into 3x3 rectangles of equal size

        
        Rectangle top_left;
        Rectangle top_middle;
        Rectangle top_right;

        Rectangle middle_left;
        Rectangle middle_middle;
        Rectangle middle_right;

        Rectangle bottom_left;
        Rectangle bottom_middle;
        Rectangle bottom_right;

        List<LineSegment2D> top_left_lines;
        List<LineSegment2D> top_middle_lines;
        List<LineSegment2D> top_right_lines;

        List<LineSegment2D> middle_left_lines;
        List<LineSegment2D> middle_middle_lines;
        List<LineSegment2D> middle_right_lines;

        List<LineSegment2D> bottom_left_lines;
        List<LineSegment2D> bottom_middle_lines;
        List<LineSegment2D> bottom_right_lines;

        LineSegment2D top_left_line;
        LineSegment2D top_middle_line;
        LineSegment2D top_right_line;

        LineSegment2D middle_left_line;
        LineSegment2D middle_middle_line;
        LineSegment2D middle_right_line;

        LineSegment2D bottom_left_line;
        LineSegment2D bottom_middle_line;
        LineSegment2D bottom_right_line;

        List<LineSegment2D> all_lines;
        public OpticalFlow() {
            top_left = new Rectangle();
            top_middle = new Rectangle();
            top_right = new Rectangle();

            middle_left = new Rectangle();
            middle_middle = new Rectangle();
            middle_right = new Rectangle();

            bottom_left = new Rectangle();
            bottom_middle = new Rectangle();
            bottom_right = new Rectangle();

            top_left_lines = new List<LineSegment2D>();
            top_middle_lines = new List<LineSegment2D>();
            top_right_lines = new List<LineSegment2D>();

            middle_left_lines = new List<LineSegment2D>();
            middle_middle_lines = new List<LineSegment2D>();
            middle_right_lines = new List<LineSegment2D>();

            bottom_left_lines = new List<LineSegment2D>();
            bottom_middle_lines = new List<LineSegment2D>();
            bottom_right_lines = new List<LineSegment2D>();

            top_left_line = new LineSegment2D(new Point(0, 0), new Point(0, 0));
            top_middle_line = new LineSegment2D(new Point(0, 0), new Point(0, 0));
            top_right_line = new LineSegment2D(new Point(0, 0), new Point(0, 0));

            middle_left_line = new LineSegment2D(new Point(0, 0), new Point(0, 0));
            middle_middle_line = new LineSegment2D(new Point(0, 0), new Point(0, 0));
            middle_right_line = new LineSegment2D(new Point(0, 0), new Point(0, 0));

            bottom_left_line = new LineSegment2D(new Point(0, 0), new Point(0, 0));
            bottom_middle_line = new LineSegment2D(new Point(0, 0), new Point(0, 0));
            bottom_right_line = new LineSegment2D(new Point(0, 0), new Point(0, 0));

            all_lines = new List<LineSegment2D>();
        }

        public  Image<Hsv, byte> CalculateSections(Image<Hsv, byte> frameImg, int frameNumber = 0) {
            // resultant Image
            Image<Hsv, byte> img = new Image<Hsv, byte>(frameImg.Width, frameImg.Height);

            int w = frameImg.Width; // total width of image
            int h = frameImg.Height; // total height of image
            int section_width = w / 3; // width for one section
            int section_height = h / 3; // height for one section

            // width and height of all rectangles will be same
            top_left.Width = top_middle.Width = top_right.Width = section_width;
            middle_left.Width = middle_middle.Width = middle_right.Width = section_width;
            bottom_left.Width = bottom_middle.Width = bottom_right.Width = section_width;

            top_left.Height = top_middle.Height = top_right.Height = section_height;
            middle_left.Height = middle_middle.Height = middle_right.Height = section_height;
            bottom_left.Height = bottom_middle.Height = bottom_right.Height = section_height;

            // only starting point of each section will be different

            top_left.Y = top_middle.Y = top_right.Y = 0; // same Y position for first row sections
            middle_left.Y = middle_middle.Y = middle_right.Y = section_height; 
            bottom_left.Y = bottom_middle.Y = bottom_right.Y = 2 * section_height;

            top_left.X = middle_left.X = bottom_left.X = 0;
            top_middle.X = middle_middle.X = bottom_middle.X = section_width;
            top_right.X = middle_right.X = bottom_right.X = 2 * section_width;

            // drawing the rectangles on the Image
            CvInvoke.Rectangle(img,top_left,new Bgr(Color.Green).MCvScalar,2);
            CvInvoke.Rectangle(img, top_middle, new Bgr(Color.Blue).MCvScalar, 2);
            CvInvoke.Rectangle(img, top_right, new Bgr(Color.Green).MCvScalar, 2);
            CvInvoke.Rectangle(img, middle_left, new Bgr(Color.Green).MCvScalar, 2);
            CvInvoke.Rectangle(img, middle_middle, new Bgr(Color.Blue).MCvScalar, 2);
            CvInvoke.Rectangle(img, middle_right, new Bgr(Color.Green).MCvScalar, 2);
            CvInvoke.Rectangle(img, bottom_left, new Bgr(Color.Green).MCvScalar, 2);
            CvInvoke.Rectangle(img, bottom_middle, new Bgr(Color.Blue).MCvScalar, 2);
            CvInvoke.Rectangle(img, bottom_right, new Bgr(Color.Green).MCvScalar, 2);

            // assigning the lines to their respective section list
            foreach (LineSegment2D line in all_lines) {
                

                // first row sections
                if (line.P1.X < section_width && line.P2.X < section_width && line.P1.Y < section_height && line.P2.Y < section_height)
                {
                    top_left_lines.Add(line);
                }
                else if (line.P1.X < 2 * section_width && line.P2.X < 2 * section_width && line.P1.Y < section_height && line.P2.Y < section_height)
                {
                    top_middle_lines.Add(line);
                }
                else if (line.P1.X < w && line.P2.X < w && line.P1.Y < section_height && line.P2.Y < section_height) {
                    top_right_lines.Add(line);
                }
                // middle row sections
                else if (line.P1.X < section_width && line.P2.X < section_width && line.P1.Y < 2* section_height && line.P2.Y < 2* section_height)
                {
                    middle_left_lines.Add(line);
                }
                else if (line.P1.X < 2 * section_width && line.P2.X < 2 * section_width && line.P1.Y < 2* section_height && line.P2.Y < 2* section_height)
                {
                    middle_middle_lines.Add(line);
                }
                else if (line.P1.X < w && line.P2.X < w && line.P1.Y < 2* section_height && line.P2.Y < 2 * section_height)
                {
                    middle_right_lines.Add(line);
                }
                // third row sections
                else if (line.P1.X < section_width && line.P2.X < section_width && line.P1.Y < h && line.P2.Y < h)
                {
                    top_left_lines.Add(line);
                }
                else if (line.P1.X < 2 * section_width && line.P2.X < 2 * section_width && line.P1.Y < h && line.P2.Y < h)
                {
                    top_middle_lines.Add(line);
                }
                else if (line.P1.X < w && line.P2.X < w && line.P1.Y < h && line.P2.Y < h)
                {
                    top_right_lines.Add(line);
                }
            }

            // get line with maximum Length from each section list of lines
            // And Draw the line on the image

            if (top_left_lines.Count > 0) {
                top_left_line = getBigLine(top_left_lines);
                CvInvoke.ArrowedLine(img, top_left_line.P1, top_left_line.P2, new Bgr(Color.White).MCvScalar, 1);
            }

            if (top_middle_lines.Count > 0)
            {
                top_middle_line = getBigLine(top_middle_lines);
                CvInvoke.ArrowedLine(img, top_middle_line.P1, top_middle_line.P2, new Bgr(Color.White).MCvScalar, 1);
            }

            if (top_right_lines.Count > 0)
            {
                top_right_line = getBigLine(top_right_lines);
                CvInvoke.ArrowedLine(img, top_right_line.P1, top_right_line.P2, new Bgr(Color.White).MCvScalar, 1);
            }


            if (middle_left_lines.Count > 0)
            {
                middle_left_line = getBigLine(middle_left_lines);
                CvInvoke.ArrowedLine(img, middle_left_line.P1, middle_left_line.P2, new Bgr(Color.White).MCvScalar, 1);
            }

            if (middle_middle_lines.Count > 0)
            {
                middle_middle_line = getBigLine(middle_middle_lines);
                CvInvoke.ArrowedLine(img, middle_middle_line.P1, middle_middle_line.P2, new Bgr(Color.White).MCvScalar, 1);
            }

            if (middle_right_lines.Count > 0)
            {
                middle_right_line = getBigLine(middle_right_lines);
                CvInvoke.ArrowedLine(img, middle_right_line.P1, middle_right_line.P2, new Bgr(Color.White).MCvScalar, 1);
            }


            if (bottom_left_lines.Count > 0)
            {
                bottom_left_line = getBigLine(bottom_left_lines);
                CvInvoke.ArrowedLine(img, bottom_left_line.P1, bottom_left_line.P2, new Bgr(Color.White).MCvScalar, 1);
            }
            if (bottom_middle_lines.Count > 0)
            {
                bottom_middle_line = getBigLine(bottom_middle_lines);
                CvInvoke.ArrowedLine(img, bottom_middle_line.P1, bottom_middle_line.P2, new Bgr(Color.White).MCvScalar, 1);
            }
            if (bottom_right_lines.Count > 0)
            {
                bottom_right_line = getBigLine(bottom_right_lines);
                CvInvoke.ArrowedLine(img, bottom_right_line.P1, bottom_right_line.P2, new Bgr(Color.White).MCvScalar, 1);
            }

            ///CvInvoke.Imshow("3x3 Frames", img);
            //CvInvoke.Imwrite("C:\\Users\\Antivirus\\Desktop\\of\\Frames.png",img);
            CvInvoke.Imwrite("C:\\Users\\Antivirus\\Desktop\\of\\opticalflow" + (frameNumber - 1) + "-" + (frameNumber) + ".png", img);
            return img;
        }

        public  Image<Hsv, byte> CalculateOpticalFlow(Image<Gray, byte> prevFrame, Image<Gray, byte> nextFrame, int frameNumber = 0) {

            Image<Hsv, byte> coloredMotion = new Image<Hsv, byte>(nextFrame.Width, nextFrame.Height);//CalculateSections(nextFrame);

            Image<Gray, float> velx = new Image<Gray, float>(new Size(prevFrame.Width, prevFrame.Height));
            Image<Gray, float> vely = new Image<Gray, float>(new Size(prevFrame.Width, prevFrame.Height));

            CvInvoke.CalcOpticalFlowFarneback(prevFrame, nextFrame, velx, vely, 0.5, 3, 60, 3, 5, 1.1, OpticalflowFarnebackFlag.Default);
            //Image<Hsv, Byte> coloredMotion = new Image<Hsv, Byte>(new Size(prevFrame.Width, prevFrame.Height));

            
            //StreamWriter fs = new StreamWriter("C:\\Users\\Antivirus\\Desktop\\of\\opticalflow" + (frameNumber -1) + "-" + (frameNumber) + ".csv");
            //fs.WriteLine("velx," + "vely," + "degrees," + "distance");

            for (int i = 0; i < coloredMotion.Width; i++)
            {
                for (int j = 0; j < coloredMotion.Height; j++)
                {
                    // Pull the relevant intensities from the velx and vely matrices
                    double velxHere = velx[j, i].Intensity;
                    double velyHere = vely[j, i].Intensity;

                    // Determine the color (i.e, the angle)
                    double degrees = Math.Atan(velyHere / velxHere) / Math.PI * 90 + 45;
                    if (velxHere < 0)
                    {
                        degrees += 90;
                    }
                    //coloredMotion.Data[j, i, 0] = (Byte)degrees;
                    //coloredMotion.Data[j, i, 1] = 255;

                    // Determine the intensity (i.e, the distance)
                    double intensity = Math.Sqrt(velxHere * velxHere + velyHere * velyHere) * 10;
                    Point p1 = new Point(i,j);
                    Point p2 = ComputerSecondPoint(p1, degrees, intensity);
                    if (p1.X == p2.X && p1.Y == p2.Y) {
                        continue;
                    }
                    if (intensity < 15) { // if distance is smaller then ignore
                        continue;
                    }
                    this.all_lines.Add(new LineSegment2D(p1, p2));
                    //CvInvoke.Line(coloredMotion, p1, p2, new Bgr(Color.White).MCvScalar,1);
                    //coloredMotion.Data[j, i, 2] = (intensity > 255) ? (byte)255 : (byte)intensity;

                    //fs.WriteLine(velxHere + "," + velyHere + "," + degrees + "," + intensity + "");
                }
            }

            // calculate the 9 sections and add each line to the list of respective section
           coloredMotion =  CalculateSections(coloredMotion, frameNumber);
            //CvInvoke.Imwrite("C:\\Users\\Antivirus\\Desktop\\of\\opticalflow" + (frameNumber - 1) + "-" + (frameNumber) + ".png", coloredMotion);
            //fs.Flush();
            //fs.Close();

            // coloredMotion is now an image that shows intensity of motion by lightness
            // and direction by color.
            //CvInvoke.Imshow("Lightness Motion", coloredMotion);
            return coloredMotion;
        }

        private  Point ComputerSecondPoint(Point p1, double theta, double distance) {
            Point p2 = new Point();
            p2.X = (int)Math.Round(p1.X + distance * Math.Cos(theta * Math.PI / 180.0));
            p2.Y = (int)Math.Round(p1.Y + distance * Math.Sin(theta * Math.PI / 180.0));
            //p2.X = (int)Math.Round(p1.X + distance * Math.Cos(theta));
            //p2.Y = (int)Math.Round(p1.Y + distance * Math.Sin(theta));
            return p2;
        }

        // get the line with maximum length
        private LineSegment2D getBigLine(List<LineSegment2D> lines)
        {
            if (lines.Count == 0)
                return new LineSegment2D();

            LineSegment2D bigLine = new LineSegment2D();
            bigLine = lines[0];

            foreach (LineSegment2D line in lines)
            {
                if (bigLine.Length < line.Length)
                {
                    bigLine = line;
                }
            }

            return bigLine;
        }
    }
    

}
