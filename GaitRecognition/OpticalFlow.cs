using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Features2D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV.Util;

namespace GaitRecognition
{
    class OpticalFlow
    {

        int class_label;
        String fileName;
        //public static StreamWriter fs;
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

        List<FeatureVectorOpticalFlow> top_left_lines;
        List<FeatureVectorOpticalFlow> top_middle_lines;
        List<FeatureVectorOpticalFlow> top_right_lines;

        List<FeatureVectorOpticalFlow> middle_left_lines;
        List<FeatureVectorOpticalFlow> middle_middle_lines;
        List<FeatureVectorOpticalFlow> middle_right_lines;

        List<FeatureVectorOpticalFlow> bottom_left_lines;
        List<FeatureVectorOpticalFlow> bottom_middle_lines;
        List<FeatureVectorOpticalFlow> bottom_right_lines;

        FeatureVectorOpticalFlow top_left_line;
        FeatureVectorOpticalFlow top_middle_line;
        FeatureVectorOpticalFlow top_right_line;

        FeatureVectorOpticalFlow middle_left_line;
        FeatureVectorOpticalFlow middle_middle_line;
        FeatureVectorOpticalFlow middle_right_line;

        FeatureVectorOpticalFlow bottom_left_line;
        FeatureVectorOpticalFlow bottom_middle_line;
        FeatureVectorOpticalFlow bottom_right_line;

        List<FeatureVectorOpticalFlow> all_lines;
        // Constructor
        public OpticalFlow(String filename, int label) {
            //fs = new StreamWriter("C:\\Users\\Antivirus\\Desktop\\of\\FeaturesFile.csv", append: true);
            /*fs.WriteLine("velx_r1," + "vely_r1," + "degrees_r1," + "distance_r1,"
                 + "velx_r2," + "vely_r2," + "degrees_r2," + "distance_r2,"
                 + "velx_r3," + "vely_r3," + "degrees_r3," + "distance_r3,"
                 + "velx_r4," + "vely_r4," + "degrees_r4," + "distance_r4,"
                 + "velx_r5," + "vely_r5," + "degrees_r5," + "distance_r5,"
                 + "velx_r6," + "vely_r6," + "degrees_r6," + "distance_r6,"
                 + "velx_r7," + "vely_r7," + "degrees_r7," + "distance_r7,"
                 + "velx_r8," + "vely_r8," + "degrees_r8," + "distance_r8,"
                 + "velx_r9," + "vely_r9," + "degrees_r9," + "distance_r9,"
                 + "activity");
            fs.Close();*/
            class_label = label;
            fileName = filename;
            top_left = new Rectangle();
            top_middle = new Rectangle();
            top_right = new Rectangle();

            middle_left = new Rectangle();
            middle_middle = new Rectangle();
            middle_right = new Rectangle();

            bottom_left = new Rectangle();
            bottom_middle = new Rectangle();
            bottom_right = new Rectangle();

            top_left_lines = new List<FeatureVectorOpticalFlow>();
            top_middle_lines = new List<FeatureVectorOpticalFlow>();
            top_right_lines = new List<FeatureVectorOpticalFlow>();

            middle_left_lines = new List<FeatureVectorOpticalFlow>();
            middle_middle_lines = new List<FeatureVectorOpticalFlow>();
            middle_right_lines = new List<FeatureVectorOpticalFlow>();

            bottom_left_lines = new List<FeatureVectorOpticalFlow>();
            bottom_middle_lines = new List<FeatureVectorOpticalFlow>();
            bottom_right_lines = new List<FeatureVectorOpticalFlow>();

            top_left_line = new FeatureVectorOpticalFlow();
            top_middle_line = new FeatureVectorOpticalFlow();
            top_right_line = new FeatureVectorOpticalFlow();

            middle_left_line = new FeatureVectorOpticalFlow();
            middle_middle_line = new FeatureVectorOpticalFlow();
            middle_right_line = new FeatureVectorOpticalFlow();

            bottom_left_line = new FeatureVectorOpticalFlow();
            bottom_middle_line = new FeatureVectorOpticalFlow();
            bottom_right_line = new FeatureVectorOpticalFlow();

            all_lines = new List<FeatureVectorOpticalFlow>();
        }

        // Calculate Sections, Assign Lines for Each Section and Draw on Image
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
            foreach (FeatureVectorOpticalFlow fv in all_lines) {
                

                // first row sections
                if (fv.line.P1.X < section_width
                    && fv.line.P2.X < section_width
                    && fv.line.P1.Y < section_height
                    && fv.line.P2.Y < section_height)
                {
                    top_left_lines.Add(fv);
                }
                else if (fv.line.P1.X < 2 * section_width
                    && fv.line.P2.X < 2 * section_width
                    && fv.line.P1.Y < section_height && fv.line.P2.Y < section_height)
                {
                    top_middle_lines.Add(fv);
                }
                else if (fv.line.P1.X < w && fv.line.P2.X < w
                    && fv.line.P1.Y < section_height
                    && fv.line.P2.Y < section_height) {
                    top_right_lines.Add(fv);
                }

                // middle row sections
                else if (fv.line.P1.X < section_width
                    && fv.line.P2.X < section_width
                    && fv.line.P1.Y < 2* section_height
                    && fv.line.P2.Y < 2* section_height)
                {
                    middle_left_lines.Add(fv);
                }
                else if (fv.line.P1.X < 2 * section_width
                    && fv.line.P2.X < 2 * section_width
                    && fv.line.P1.Y < 2* section_height
                    && fv.line.P2.Y < 2* section_height)
                {
                    middle_middle_lines.Add(fv);
                }
                else if (fv.line.P1.X < w && fv.line.P2.X < w
                    && fv.line.P1.Y < 2* section_height
                    && fv.line.P2.Y < 2 * section_height)
                {
                    middle_right_lines.Add(fv);
                }

                // third row sections
                else if (fv.line.P1.X < section_width
                    && fv.line.P2.X < section_width
                    && fv.line.P1.Y < h && fv.line.P2.Y < h)
                {
                    bottom_left_lines.Add(fv);
                }
                else if (fv.line.P1.X < 2 * section_width
                    && fv.line.P2.X < 2 * section_width
                    && fv.line.P1.Y < h && fv.line.P2.Y < h)
                {
                    bottom_middle_lines.Add(fv);
                }
                else if (fv.line.P1.X < w
                    && fv.line.P2.X < w
                    && fv.line.P1.Y < h
                    && fv.line.P2.Y < h)
                {
                    bottom_right_lines.Add(fv);
                }
            }

            // get line with maximum Length from each section list of lines
            // And Draw the line on the image

            if (top_left_lines.Count > 0) {
                top_left_line = getBigLine(top_left_lines);
                CvInvoke.ArrowedLine(img, top_left_line.line.P1, top_left_line.line.P2, new Bgr(Color.White).MCvScalar, 1);
            }

            if (top_middle_lines.Count > 0)
            {
                top_middle_line = getBigLine(top_middle_lines);
                CvInvoke.ArrowedLine(img, top_middle_line.line.P1, top_middle_line.line.P2, new Bgr(Color.White).MCvScalar, 1);
            }

            if (top_right_lines.Count > 0)
            {
                top_right_line = getBigLine(top_right_lines);
                CvInvoke.ArrowedLine(img, top_right_line.line.P1, top_right_line.line.P2, new Bgr(Color.White).MCvScalar, 1);
            }


            if (middle_left_lines.Count > 0)
            {
                middle_left_line = getBigLine(middle_left_lines);
                CvInvoke.ArrowedLine(img, middle_left_line.line.P1, middle_left_line.line.P2, new Bgr(Color.White).MCvScalar, 1);
            }

            if (middle_middle_lines.Count > 0)
            {
                middle_middle_line = getBigLine(middle_middle_lines);
                CvInvoke.ArrowedLine(img, middle_middle_line.line.P1, middle_middle_line.line.P2, new Bgr(Color.White).MCvScalar, 1);
            }

            if (middle_right_lines.Count > 0)
            {
                middle_right_line = getBigLine(middle_right_lines);
                CvInvoke.ArrowedLine(img, middle_right_line.line.P1, middle_right_line.line.P2, new Bgr(Color.White).MCvScalar, 1);
            }


            if (bottom_left_lines.Count > 0)
            {
                bottom_left_line = getBigLine(bottom_left_lines);
                CvInvoke.ArrowedLine(img, bottom_left_line.line.P1, bottom_left_line.line.P2, new Bgr(Color.White).MCvScalar, 1);
            }
            if (bottom_middle_lines.Count > 0)
            {
                bottom_middle_line = getBigLine(bottom_middle_lines);
                CvInvoke.ArrowedLine(img, bottom_middle_line.line.P1, bottom_middle_line.line.P2, new Bgr(Color.White).MCvScalar, 1);
            }
            if (bottom_right_lines.Count > 0)
            {
                bottom_right_line = getBigLine(bottom_right_lines);
                CvInvoke.ArrowedLine(img, bottom_right_line.line.P1, bottom_right_line.line.P2, new Bgr(Color.White).MCvScalar, 1);
            }
            WriteFeatureToCSV();
            ///CvInvoke.Imshow("3x3 Frames", img);
            //CvInvoke.Imwrite("C:\\Users\\Antivirus\\Desktop\\of\\Frames.png",img);
            //CvInvoke.Imwrite("C:\\Users\\Antivirus\\Desktop\\of\\opticalflow" + (frameNumber - 1) + "-" + (frameNumber) + ".png", img);
            return img;
        }

        // Write Feature Vector to CSV Format
        public void WriteFeatureToCSV() {
           StreamWriter fs = new StreamWriter("C:\\Users\\Antivirus\\Desktop\\of\\FeaturesFile.csv", append: true);
            fs.WriteLine(""+top_left_line.velx + "," + top_left_line.vely + "," + top_left_line.degrees + "," + top_left_line.distance + ","
                + top_middle_line.velx + "," + top_middle_line.vely + "," + top_middle_line.degrees + "," + top_middle_line.distance + ","
                + top_right_line.velx + "," + top_right_line.vely + "," + top_right_line.degrees + "," + top_right_line.distance + ","
                + middle_left_line.velx + "," + middle_left_line.vely + "," + middle_left_line.degrees + "," + middle_left_line.distance + ","
                + middle_middle_line.velx + "," + middle_middle_line.vely + "," + middle_middle_line.degrees + "," + middle_middle_line.distance + ","
                + middle_right_line.velx + "," + middle_right_line.vely + "," + middle_right_line.degrees + "," + middle_right_line.distance + ","
                + bottom_left_line.velx + "," + bottom_left_line.vely + "," + bottom_left_line.degrees + "," + bottom_left_line.distance + ","
                + bottom_middle_line.velx + "," + bottom_middle_line.vely + "," + bottom_middle_line.degrees + "," + bottom_middle_line.distance + ","
                + bottom_right_line.velx + "," + bottom_right_line.vely + "," + bottom_right_line.degrees + "," + bottom_right_line.distance + ","
                + class_label);
            fs.Close();
           StreamWriter streamWriter = new StreamWriter("C:\\Users\\Antivirus\\Desktop\\of\\" + fileName + ".csv",append:true);
            /*streamWriter.WriteLine("velx_r1," + "vely_r1," + "degrees_r1," + "distance_r1,"
                + "velx_r2," + "vely_r2," + "degrees_r2," + "distance_r2,"
                + "velx_r3," + "vely_r3," + "degrees_r3," + "distance_r3,"
                + "velx_r4," + "vely_r4," + "degrees_r4," + "distance_r4,"
                + "velx_r5," + "vely_r5," + "degrees_r5," + "distance_r5,"
                + "velx_r6," + "vely_r6," + "degrees_r6," + "distance_r6,"
                + "velx_r7," + "vely_r7," + "degrees_r7," + "distance_r7,"
                + "velx_r8," + "vely_r8," + "degrees_r8," + "distance_r8,"
                + "velx_r9," + "vely_r9," + "degrees_r9," + "distance_r9,"
                + "activity");*/
            streamWriter.WriteLine(top_left_line.velx + "," + top_left_line.vely + "," + top_left_line.degrees + "," + top_left_line.distance + ","
               + top_middle_line.velx + "," + top_middle_line.vely + "," + top_middle_line.degrees + "," + top_middle_line.distance + ","
               + top_right_line.velx + "," + top_right_line.vely + "," + top_right_line.degrees + "," + top_right_line.distance + ","
               + middle_left_line.velx + "," + middle_left_line.vely + "," + middle_left_line.degrees + "," + middle_left_line.distance + ","
               + middle_middle_line.velx + "," + middle_middle_line.vely + "," + middle_middle_line.degrees + "," + middle_middle_line.distance + ","
               + middle_right_line.velx + "," + middle_right_line.vely + "," + middle_right_line.degrees + "," + middle_right_line.distance + ","
               + bottom_left_line.velx + "," + bottom_left_line.vely + "," + bottom_left_line.degrees + "," + bottom_left_line.distance + ","
               + bottom_middle_line.velx + "," + bottom_middle_line.vely + "," + bottom_middle_line.degrees + "," + bottom_middle_line.distance + ","
               + bottom_right_line.velx + "," + bottom_right_line.vely + "," + bottom_right_line.degrees + "," + bottom_right_line.distance + ","
               + class_label);
           streamWriter.Close();
        }
        // Calculate Optical Flow Using Farne back Algorithm
        public  Image<Hsv, byte> CalculateOpticalFlow(Image<Gray, byte> prevFrame, Image<Gray, byte> nextFrame, int frameNumber = 0) {

            Image<Hsv, byte> coloredMotion = new Image<Hsv, byte>(nextFrame.Width, nextFrame.Height);//CalculateSections(nextFrame);

            Image<Gray, float> velx = new Image<Gray, float>(new Size(prevFrame.Width, prevFrame.Height));
            Image<Gray, float> vely = new Image<Gray, float>(new Size(prevFrame.Width, prevFrame.Height));

            CvInvoke.CalcOpticalFlowFarneback(prevFrame, nextFrame, velx, vely, 0.5, 3, 60, 3, 5, 1.1, OpticalflowFarnebackFlag.Default);
            prevFrame.Dispose();
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
                    if (intensity < 5) { // if distance is smaller then ignore
                        continue;
                    }
                    this.all_lines.Add(new FeatureVectorOpticalFlow(Math.Round(velxHere, 2), Math.Round(velyHere, 2), Math.Round(degrees, 2), Math.Round(intensity, 2), new LineSegment2D( p1, p2)));
                    //CvInvoke.Line(coloredMotion, p1, p2, new Bgr(Color.White).MCvScalar,1);
                    //coloredMotion.Data[j, i, 2] = (intensity > 255) ? (byte)255 : (byte)intensity;
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

        // Calculate Optical Flow Using PyrLk Algorithm
        public void PyrLkOpticalFlow(Image<Gray, byte> prevFrame, Image<Gray, byte> nextFrame)
        {

            //Get the Optical flow of L-K feature
            Image<Gray, Byte> mask = prevFrame.Clone();
            GFTTDetector detector = new GFTTDetector(30, 0.01, 10, 3, false, 0.04);
            MKeyPoint[] fp1 =  detector.Detect(prevFrame, null);
            VectorOfPointF vp1 = new VectorOfPointF(fp1.Select(x => x.Point).ToArray());
            VectorOfPointF vp2 = new VectorOfPointF(vp1.Size);
            VectorOfByte vstatus = new VectorOfByte(vp1.Size);
            VectorOfFloat verr = new VectorOfFloat(vp1.Size);
            Size winsize = new Size(prevFrame.Width, prevFrame.Height);
            int maxLevel = 1; // if 0, winsize is not used
            MCvTermCriteria criteria = new MCvTermCriteria(10, 1);

                try
                {
                   CvInvoke.CalcOpticalFlowPyrLK(prevFrame, nextFrame, vp1, vp2, vstatus, verr, winsize, maxLevel, criteria);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
         }

        // Compute the Second Point for Line given first Point, Angle and Distance
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
        // get the line with maximum length
        private FeatureVectorOpticalFlow getBigLine(List<FeatureVectorOpticalFlow> lines)
        {
            if (lines.Count == 0)
                return new FeatureVectorOpticalFlow();

            FeatureVectorOpticalFlow bigLine = new FeatureVectorOpticalFlow();
            bigLine = lines[0];

            foreach (FeatureVectorOpticalFlow line in lines)
            {
                if (bigLine.line.Length < line.line.Length)
                {
                    bigLine = line;
                }
            }

            return bigLine;
        }
        public void Dispose()
        {
            all_lines.Clear();

            top_left_lines.Clear();
            top_middle_lines.Clear();
            top_right_lines.Clear();

            middle_left_lines.Clear();
            middle_middle_lines.Clear();
            middle_right_lines.Clear();

            bottom_left_lines.Clear();
            bottom_middle_lines.Clear();
            bottom_right_lines.Clear();

            System.GC.SuppressFinalize(this);
        }
    }
    

}
