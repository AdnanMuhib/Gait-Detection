﻿using Emgu.CV;
using Emgu.CV.BgSegm;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV.XImgproc;
using System.Windows.Forms;
using System.Drawing.Imaging;
using Emgu.CV.Util;
using System.Drawing.Drawing2D;

namespace GaitRecognition
{
    public partial class GaitRecognition : Form
    {
        // input and output directories for batch Processing
        String inputFolder = @"C:\Users\Antivirus\Desktop\APS Products Images\";
        String outputFolder = @"D:\UNIVERSITY DOCUMENTS\FYP\Human Activity Recognition\Test Outputs\";

        Image<Bgr, byte> BgrImg; // Color Image

        Image<Gray, byte> img; // any input image

        Image<Gray, byte> bgImage; // Background Image from the video or custom input
        Image<Gray, byte> previousFrame; // previous frame for Opticalflow
        Image<Gray, byte> nextFrame; // next frame for Opticalflow
        
        
        int frameIndex;

        VideoCapture _capture; // to read video files

        // frame of the person 
        PersonFrame frm;

        bool saveResults = true;

        public GaitRecognition()
        {
            InitializeComponent();
            this.KeyPreview = true;
            frameIndex = 0;
            //pictureViewBox.SizeMode = PictureBoxSizeMode.Zoom;
            string filename = "20.bmp";
            //bgImage = new Image<Gray, byte>(inputFolder + "b.bmp").Resize(400,400,Inter.Cubic);
            //img = new Image<Gray, byte>(inputFolder + filename).Resize(400, 400, Inter.Cubic);
            //BgrImg = new Image<Bgr, byte>(inputFolder + filename).Resize(400, 400, Inter.Cubic);

            //removebackground(filename);
            /*
            MLP mlp = new MLP();
            mlp.LoadTrainData(@"C:\Users\Antivirus\Desktop\of\Test_Shuffle_With_Col_Labels.csv");
            mlp.Train();
            mlp.LoadTestData(@"C:\Users\Antivirus\Desktop\of\Test_Shuffle_With_Col_Labels.csv");
            mlp.Predict();*/
            // batchProcessor();
        }


        // When Process SubMenu From the Tools Menu is clicked
        private void processToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            batchProcessor();
        }

        // When Menu item Open is clicked
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String imagePath = openFileDialogue();
            if (imagePath != "") {
                img = new Image<Gray, byte>(imagePath);
                BgrImg = new Image<Bgr, byte>(imagePath);
                //Bitmap tempImage = (Bitmap)Bitmap.FromFile(imagePath);
                //HoughTransform(tempImage);
                //removebackground();
            }
        }
         
        // Open File Selection Dialogue Box
        public static String openFileDialogue() {
            OpenFileDialog f = new OpenFileDialog();
            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return f.FileName;
            }

            return "";
        }

        // when shortcut key of CTRL + O is pressed
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.O)
            {
                String fileName= openFileDialogue();
                if (fileName != "")
                {
                    img = new Image<Gray, byte>(fileName);
                    BgrImg = new Image<Bgr, byte>(fileName);
                    pictureViewBox.Image = img;
                }
            }
            
        }

        // Batch Processor
        public void batchProcessor() {
            
            List<String> files = new List<string>();
            DirectoryInfo d = new DirectoryInfo(inputFolder);

            FileInfo[] Files = d.GetFiles("*.png"); //Getting BMP files
            foreach (FileInfo file in Files) {
                if (file.Name.Equals("b.bmp"))
                {
                    bgImage = new Image<Gray, byte>(inputFolder + file.Name).Resize(400,400,Inter.Cubic);
                }
                else {
                    files.Add(file.Name);
                }
            }

            foreach (String imgName in files) {
                img = new Image<Gray, byte>(inputFolder + imgName);//.Resize(400, 400, Inter.Cubic);
                //BgrImg = new Image<Bgr, byte>(inputFolder + imgName).Resize(400, 400, Inter.Cubic);
                //removebackground("_out_" + imgName);
                resizeImage(inputFolder, imgName, 600, 600, img.Width, img.Height);
                img.Dispose();
            }
            this.Close();
            return;
        }

        // Background Subtraction From the Given Background and Input Image
        public void removebackground(string filepath = null) {
            CvInvoke.Imshow("1- Background Image", bgImage);
            CvInvoke.Imshow("2- Forground Image", img);
            Image<Gray, byte> output = new Image<Gray, byte>(bgImage.Width, bgImage.Height);
            BackgroundSubtractorMOG2 bgsubtractor = new BackgroundSubtractorMOG2(varThreshold:100,shadowDetection:false);
            bgsubtractor.Apply(bgImage, output);
            bgsubtractor.Apply(img, output);
            pictureViewBox.Image = output;

            CvInvoke.Imshow("3- Background Subtracted", output);
            //output.Canny(100,100);

            CvInvoke.Erode(output, output, null, new System.Drawing.Point(-1, -1), 1, BorderType.Reflect, default(MCvScalar));

            CvInvoke.Imshow("4- After Applying Erode", output);
            CvInvoke.Dilate(output, output, null, new System.Drawing.Point(-1, -1), 5, BorderType.Reflect, default(MCvScalar));

            CvInvoke.Imshow("5- After Dilation", output);

            // Write the Silhoutte output to the file
            if (filepath != null && saveResults == true)
            {
                CvInvoke.Imwrite(outputFolder + "bg_subtract_" + filepath, output);
            }

            // finding the Bounding Box of the Person
            frm = new PersonFrame();
            Rectangle rec = frm.findBoundry(output);
            
            // Using Thinning Algorithm on Silhoutte
            Image<Gray, byte> thinOutput = new Image<Gray, byte>(output.Width, output.Height);
            XImgprocInvoke.Thinning(output, thinOutput, ThinningTypes.ZhangSuen);
            pictureViewBox.Image = thinOutput.Not().Not();
            CvInvoke.Imshow("6- After Thinning Zhang Suen", thinOutput);
            // Write the thinned Image to the file
            if (filepath != null && saveResults == true)
            {
                CvInvoke.Imwrite(outputFolder + "thinned_" + filepath, thinOutput.Not().Not());
                
            }
            
            // drawing bounding Box of the person
            CvInvoke.Rectangle(thinOutput, rec, new Rgb(Color.White).MCvScalar, 2);
            CvInvoke.Imshow("Person Bounding Box", thinOutput);
            // drawing the middle line of the Person
            //CvInvoke.Line(thinOutput, frm.middle_line.p1, frm.middle_line.p2, new Rgb(Color.White).MCvScalar, 2);

            // Display the Image
            //CvInvoke.Imshow("Person Fame", thinOutput);
            
            // Applying Hough Line Transformation
            Hough(thinOutput, filepath);

            img.Dispose();
            output.Dispose();
            thinOutput.Dispose();
        }
        
        // Get Conture of Person From the Picture

        // Applying Hough Transformation to get Straight lines
        public void Hough(Image<Gray, byte> ThinImage, String filePath = null) {

            double cannyThreshold = 180.0;
            double cannyThresholdLinking = 120.0;
            UMat cannyEdges = new UMat();

            CvInvoke.Canny(ThinImage, cannyEdges, cannyThreshold, cannyThresholdLinking);

            LineSegment2D[] lines = CvInvoke.HoughLinesP(cannyEdges,
                        1,     //Distance resolution in pixel-related units
                        Math.PI / 120.0, //Angle resolution measured in radians.
                        10, //threshold
                        5, //min Line width
                        20); //gap between lines

            // drawing all hough lines on lineImg
            Mat lineImage = new Mat(ThinImage.Size, DepthType.Cv8U, 3);
            lineImage.SetTo(new MCvScalar(0));

            foreach (LineSegment2D line in lines)
            {
                //CvInvoke.Line(lineImage, line.P1, line.P2, new Bgr(Color.Green).MCvScalar, 1);
                if ((line.P2.X - line.P1.X) != 0)
                { // to avoid divide by zero
                    //double slope = (line.P2.Y - line.P1.Y) / (line.P2.X - line.P1.X);
                    CvInvoke.Line(lineImage, line.P1, line.P2, new Bgr(Color.Green).MCvScalar, 1);
                }
            }
            CvInvoke.Imshow("7- Hough Lines", lineImage);
            // get a list of lines using Line class

            List<Line> lstLine = Line.ConvertToList(lines);

            // list of Lines in upper and Lower half
            List<Line> topLines = new List<Line>();
            List<Line> bottomLines = new List<Line>();
            List<Line> commonLines = new List<Line>();
            // seperating the lines in their upper and lower half

            foreach (Line line in lstLine) {
                if (line.p1.Y < frm.middle_line.p1.Y && line.p2.Y < frm.middle_line.p1.Y)
                {
                    topLines.Add(line);
                }
                else if (line.p1.Y > frm.middle_line.p1.Y && line.p2.Y > frm.middle_line.p1.Y)
                {
                    bottomLines.Add(line);
                }
                else {
                    commonLines.Add(line);
                }
            }
            // drawing lines with different colors
            Mat OneLineImage = new Mat(ThinImage.Size, DepthType.Cv8U, 3);
            OneLineImage.SetTo(new MCvScalar(0));

            // drawing top lines with red color
            foreach (Line line in topLines) {
                CvInvoke.Line(OneLineImage, line.p1, line.p2, new Rgb(Color.Red).MCvScalar, 1);
            }

            // drawing bottom lines with blue color
            foreach (Line line in bottomLines)
            {
                CvInvoke.Line(OneLineImage, line.p1, line.p2, new Rgb(Color.Blue).MCvScalar, 1);
            }

            // drawing Common Lines with yellow Color
            foreach (Line line in commonLines)
            {
                CvInvoke.Line(OneLineImage, line.p1, line.p2, new Rgb(Color.Yellow).MCvScalar, 1);
            }

            CvInvoke.PutText(OneLineImage, "Middle Line", new Point(10, 30), FontFace.HersheyPlain, 1, new Rgb(Color.White).MCvScalar, 1);
            CvInvoke.PutText(OneLineImage, "TOP Lines", new Point(10, 50), FontFace.HersheyPlain, 1, new Rgb(Color.Red).MCvScalar, 1);
            CvInvoke.PutText(OneLineImage, "Bottom Lines", new Point(10, 70), FontFace.HersheyPlain, 1, new Rgb(Color.Blue).MCvScalar, 1);
            CvInvoke.PutText(OneLineImage, "Common Lines", new Point(10, 90), FontFace.HersheyPlain, 1, new Rgb(Color.Yellow).MCvScalar, 1);
            
            // drawing the middle line of the Person
            CvInvoke.Line(OneLineImage, frm.middle_line.p1, frm.middle_line.p2, new Rgb(Color.White).MCvScalar, 1);
            CvInvoke.Imwrite(outputFolder + "Seperated Line" + filePath, OneLineImage);

            // displaying the image
            CvInvoke.Imshow("8- Top Bottom Seperated Lines", OneLineImage);

            // getting the top left right lines 
            List<Line> topLeftLines = Line.GetLeftRightLines(topLines)[0];
            List<Line> topRightLines = Line.GetLeftRightLines(topLines)[1];

            // getting the bottom left right lines 
            List<Line> bottomLeftLines = Line.GetLeftRightLines(bottomLines)[0];
            List<Line> bottomRightLines = Line.GetLeftRightLines(bottomLines)[1];

            // drawing lines with different colors
            Mat leftRightLineImage = new Mat(ThinImage.Size, DepthType.Cv8U, 3);
            leftRightLineImage.SetTo(new MCvScalar(0));

            // drawing top left lines 
            foreach (Line line in topLeftLines) {
                CvInvoke.Line(leftRightLineImage, line.p1, line.p2, new Rgb(Color.Blue).MCvScalar, 1);
            }

            // drawing top right lines 
            foreach (Line line in topRightLines)
            {
                CvInvoke.Line(leftRightLineImage, line.p1, line.p2, new Rgb(Color.Red).MCvScalar, 1);
            }
            // drawing bottom left lines 
            foreach (Line line in bottomLeftLines)
            {
                CvInvoke.Line(leftRightLineImage, line.p1, line.p2, new Rgb(Color.Blue).MCvScalar, 1);
            }

            // drawing bottom right lines 
            foreach (Line line in bottomRightLines)
            {
                CvInvoke.Line(leftRightLineImage, line.p1, line.p2, new Rgb(Color.Red).MCvScalar, 1);
            }

            CvInvoke.Imshow("9- Left Right Seperated Lines", leftRightLineImage);

            Mat leftRightSingleLineImage = new Mat(ThinImage.Size, DepthType.Cv8U, 3);
            leftRightSingleLineImage.SetTo(new MCvScalar(0));
            // Get the Big Line from top right Lines
            Line TopRightLine = Line.getBigLine(topRightLines);

            if (TopRightLine != null)
            {
                CvInvoke.Line(leftRightSingleLineImage, TopRightLine.p1, TopRightLine.p2, new Bgr(Color.Red).MCvScalar, 1);
            }

            // Get the Big Line from top left Lines
            Line TopLeftLine = Line.getBigLine(topLeftLines);

            if (TopLeftLine != null)
            {
                CvInvoke.Line(leftRightSingleLineImage, TopLeftLine.p1, TopLeftLine.p2, new Bgr(Color.Blue).MCvScalar, 1);
            }

            // Get the Big Line from bottom right Lines
            Line BottomRightLine = Line.getBigLine(bottomRightLines);

            if (BottomRightLine != null)
            {
                CvInvoke.Line(leftRightSingleLineImage, BottomRightLine.p1, BottomRightLine.p2, new Bgr(Color.Red).MCvScalar, 1);
            }

            // Get the Big Line from bottom left Lines
            Line BottomLeftLine = Line.getBigLine(bottomLeftLines);

            if (BottomLeftLine != null)
            {
                CvInvoke.Line(leftRightSingleLineImage, BottomLeftLine.p1, BottomLeftLine.p2, new Bgr(Color.Blue).MCvScalar, 1);
            }

            // Get the Big Line from Common Lines
            Line bigCommonLine = Line.getBigLine(commonLines);

            if (bigCommonLine != null) {
                CvInvoke.Line(leftRightSingleLineImage, bigCommonLine.p1, bigCommonLine.p2, new Rgb(Color.White).MCvScalar, 1);
            }
            CvInvoke.Imshow("10- Single Big Line", leftRightSingleLineImage);

            Mat FeaturePointsImage = new Mat(ThinImage.Size, DepthType.Cv8U, 3);
            FeaturePointsImage.SetTo(new MCvScalar(0));
            // Extract Feature Points from the Lines
            List<FeaturePoint> featurePoints = Feature.ExtractFeaturePoints(bigCommonLine, BottomLeftLine, BottomRightLine, TopLeftLine, TopRightLine);
            
            
            // Draw Feature Points on The Picture

            foreach (FeaturePoint fp in featurePoints) {
                CvInvoke.Circle(leftRightLineImage, fp.point, 5, new Bgr(Color.Green).MCvScalar, 2, LineType.EightConnected);
                CvInvoke.PutText(leftRightLineImage, fp.name,fp.point, FontFace.HersheyPlain, 1, new Bgr(Color.White).MCvScalar);
            }

            CvInvoke.PutText(leftRightLineImage, "Left Lines(Negative Slope)", new Point(10, 30), FontFace.HersheyPlain, 1, new Rgb(Color.Blue).MCvScalar, 1);
            CvInvoke.PutText(leftRightLineImage, "Right Lines(Positive Slope)", new Point(10, 50), FontFace.HersheyPlain, 1, new Rgb(Color.Red).MCvScalar, 1);
            CvInvoke.PutText(leftRightLineImage, "Big Common Line", new Point(10, 70), FontFace.HersheyPlain, 1, new Rgb(Color.White).MCvScalar, 1);

            if (filePath != null && saveResults == true)
            {
                CvInvoke.Imwrite(outputFolder + "Left_Right_" + filePath, leftRightLineImage);
            }
            CvInvoke.Imshow("11- Feature Points Image", leftRightLineImage);
            leftRightLineImage.Dispose();
            // drawing bounding Box of the person
            //CvInvoke.Rectangle(lineImage, frm.rec, new Rgb(Color.White).MCvScalar, 2);

            // drawing the middle line of the Person
            //CvInvoke.Line(lineImage, frm.middle_line.p1, frm.middle_line.p2, new Rgb(Color.White).MCvScalar, 2);

            // Display the Image
            //CvInvoke.Imshow("Lines Framed", lineImage);


            // writig the Hough Line Image to Output Folder
            if (filePath != null && saveResults == true)
            {
                CvInvoke.Imwrite(outputFolder + "HoughLine_" + filePath, lineImage);
            }
            
            // Post Processing  the Hough Lines
            
            // Convert the Hough Lines to a List of Line (Custom)Class
            List<Line> lstLines = Line.ConvertToList(lines);

            //********************************************************************************
            //* Approach 1 By Calculating Distance Between Lines
            //********************************************************************************


            // merge lines by eliminating the p1 distance
            /*
            List<Line> lstMergedByDistance = Line.MergeLinesByDistance(lstLines, 20);
            
            // merge lines by eliminating the p2 distance
            lstMergedByDistance = Line.MergeLinesByDistance(lstMergedByDistance, 20, p1:false);

            // drawing the lines on Image
            Mat distImg = new Mat(ThinImage.Size, DepthType.Cv8U, 3);
            distImg.SetTo(new MCvScalar(0));

            foreach (Line l in lstMergedByDistance)
            {
                if (l.length < 20) // ignore lines with shorter length
                {
                    continue;
                }
                CvInvoke.Line(distImg, l.p1, l.p2, new Bgr(Color.Green).MCvScalar, 1);
            }

            if (filePath != null && saveResults == true)
            {
                CvInvoke.Imwrite(outputFolder + "Distance_ Merged" + filePath + ".bmp", distImg);
            }
            pictureViewBox.Image = distImg;*/
            //********************************************************************************

            //********************************************************************************
            //* Approach 2 By Grouping the Lines With Same Slope
            //********************************************************************************

            // Grouping the lines on the basis of their Slope
            /*
            var groupedSlopList = lstLines
                .GroupBy(u => u.slope)
                .Select(grp => grp.ToList())
                .ToList();

            List<Line> mergedLines = new List<Line>();
            // mergin lines into one with same slope
            foreach (List<Line> lst in groupedSlopList)
            {
                Line line = Line.MergeLinesBySlope(lst);
                mergedLines.Add(line);
            }

            Mat slopImg = new Mat(ThinImage.Size, DepthType.Cv8U, 3);
            distImg.SetTo(new MCvScalar(0));
            
            // Drawing the Merged Lines on slopimg
            
            foreach (Line l in mergedLines) {
                    CvInvoke.Line(slopImg, l.p1, l.p2, new Bgr(Color.Green).MCvScalar, 1);
                    CvInvoke.PutText(slopImg, "" + l.slope.ToString(), l.p1, FontFace.HersheyPlain, 1, new Bgr(Color.White).MCvScalar, 1);
             }
            if (filePath != null && saveResults == true)
            {
                CvInvoke.Imwrite(outputFolder + "Slop_" + filePath + ".bmp", slopImg);
                slopImg.Dispose();
            }*/

            //********************************************************************************
        }

        private void resizeImage(string path, string originalFilename,
                     /* note changed names */
                     int canvasWidth, int canvasHeight,
                     /* new */
                     int originalWidth, int originalHeight)
        {
            Image image = Image.FromFile(path + originalFilename);

            System.Drawing.Image thumbnail =
                new Bitmap(canvasWidth, canvasHeight, PixelFormat.Format32bppArgb); // changed parm names
            System.Drawing.Graphics graphic =
                         System.Drawing.Graphics.FromImage(thumbnail);
            //graphic.Clear(Color.Transparent);

            graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphic.SmoothingMode = SmoothingMode.HighQuality;
            graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphic.CompositingQuality = CompositingQuality.HighQuality;

            /* ------------------ new code --------------- */

            // Figure out the ratio
            double ratioX = (double)canvasWidth / (double)originalWidth;
            double ratioY = (double)canvasHeight / (double)originalHeight;
            // use whichever multiplier is smaller
            double ratio = ratioX < ratioY ? ratioX : ratioY;

            // now we can get the new height and width
            int newHeight = Convert.ToInt32(originalHeight * ratio);
            int newWidth = Convert.ToInt32(originalWidth * ratio);

            // Now calculate the X,Y position of the upper-left corner 
            // (one of these will always be zero)
            int posX = Convert.ToInt32((canvasWidth - (originalWidth * ratio)) / 2);
            int posY = Convert.ToInt32((canvasHeight - (originalHeight * ratio)) / 2);

            graphic.Clear(Color.Black); // white padding
            graphic.DrawImage(image, posX, posY, newWidth, newHeight);

            /* ------------- end new code ---------------- */

            System.Drawing.Imaging.ImageCodecInfo[] info =
                             ImageCodecInfo.GetImageEncoders();
            EncoderParameters encoderParameters;
            encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality,
                             100L);
            thumbnail.Save(path + "/Output/" + originalFilename, info[1],
                             encoderParameters);
        }
        // Open the Video File
        private void openVideoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String videoPath  = openFileDialogue();
            if (videoPath != "")
            {
                frameIndex = 0;
                pictureViewBox.SizeMode = PictureBoxSizeMode.Zoom;
                try
                {
                    _capture = new VideoCapture(videoPath);
                    Application.Idle += _capture_ImageGrabbed;
                    
                }
                catch (Exception ex) {
                    MessageBox.Show(ex.Message);
                }
                
            }
            else {
                MessageBox.Show("No Video File Selected");
            }
        }

        // Process each Frame from the Video
        private void _capture_ImageGrabbed(object sender, EventArgs e)
        {
            if (_capture != null && _capture.Ptr != IntPtr.Zero)
            {
                if (frameIndex == 0)
                {
                    bgImage = _capture.QueryFrame().ToImage<Gray, byte>().Resize(400,400,Inter.Cubic);
                    previousFrame = _capture.QueryFrame().ToImage<Gray, byte>();
                    //CvInvoke.Imwrite(outputFolder + "Frame_"+ frameIndex + ".bmp", bgImage);
                }
                else
                {
                    img = new Image<Gray, byte>(bgImage.Width, bgImage.Height);
                    
                    if (_capture.Retrieve(img))
                    {
                        try
                        {
                            Image<Gray, byte> temp = img.Clone();
                            img = _capture.QueryFrame().ToImage<Gray, byte>().Resize(400, 400, Inter.Cubic);
                            nextFrame = _capture.QueryFrame().ToImage<Gray, byte>().Resize(400, 400, Inter.Cubic);
                            BgrImg = _capture.QueryFrame().ToImage<Bgr, byte>().Resize(400, 400, Inter.Cubic);
                            //CvInvoke.Imwrite(outputFolder + "Frame_" + frameIndex + ".bmp", img);
                            removebackground("Frame_" + frameIndex + ".bmp");
                            //OpticalFlow();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            Application.Idle -= _capture_ImageGrabbed;
                            _capture.Pause();
                            _capture.Stop();
                            _capture.Dispose();
                        }
                    }
                }
                frameIndex = frameIndex + 1;
            }
        }

        // Optical Flow Implementation
        public void OpticalFlow() {
           /* Image<Gray, byte> output = new Image<Gray, byte>(bgImage.Width, bgImage.Height);
            BackgroundSubtractorMOG2 bgsubtractor = new BackgroundSubtractorMOG2(varThreshold: 100, shadowDetection: false);
            bgsubtractor.Apply(bgImage, output);
            bgsubtractor.Apply(img, output);
            
            //output.Canny(100,100);

            CvInvoke.Erode(output, output, null, new System.Drawing.Point(-1, -1), 1, BorderType.Reflect, default(MCvScalar));
            CvInvoke.Dilate(output, output, null, new System.Drawing.Point(-1, -1), 5, BorderType.Reflect, default(MCvScalar));

            // finding the Bounding Box of the Person
            frm = new PersonFrame();
            Rectangle rec = frm.findBoundry(output);
            //output.ROI = rec;
            pictureViewBox.Image = output;*/
            // prep containers for x and y vectors
            Image<Gray, float> velx = new Image<Gray, float>(new Size(img.Width, img.Height));
            Image<Gray, float> vely = new Image<Gray, float>(new Size(img.Width, img.Height));

            ///previousFrame.ROI = rec;
            //nextFrame.ROI = rec;
            
            // use the optical flow algorithm.
            CvInvoke.CalcOpticalFlowFarneback(previousFrame, nextFrame, velx,vely, 0.5, 3, 60, 3, 5, 1.1, OpticalflowFarnebackFlag.Default);
            
            //spictureViewBox.Image = flowMatrix;
            // color each pixel
            Image<Hsv, Byte> coloredMotion = new Image<Hsv, Byte>(new Size(img.Width, img.Height));
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
                    coloredMotion.Data[j, i, 0] = (Byte)degrees;
                    coloredMotion.Data[j, i, 1] = 255;

                    // Determine the intensity (i.e, the distance)
                    double intensity = Math.Sqrt(velxHere * velxHere + velyHere * velyHere) * 10;
                    coloredMotion.Data[j, i, 2] = (intensity > 255) ? (byte)255 : (byte)intensity;
                }
            }
            // coloredMotion is now an image that shows intensity of motion by lightness
            // and direction by color.
            pictureViewBox.Image = coloredMotion;
            previousFrame.Dispose();
            previousFrame = img.Clone();
            img.Dispose();
            nextFrame.Dispose();
            coloredMotion.Dispose();

        }

        private void selectVideoFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            

            using (var fbd = new FolderBrowserDialog())
            {
                fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    string[] files = Directory.GetFiles(fbd.SelectedPath);
                    String location = Path.GetDirectoryName(files[0]);
                    String outputFolder = location + "\\Soundless";
                    bool folderExists = Directory.Exists(outputFolder);
                    if (!folderExists)
                        Directory.CreateDirectory(outputFolder);
                    foreach (string filePath in files) {
                        if (filePath.EndsWith(".MOV"))
                        {
                            String fileName = Path.GetFileName(filePath);
                            
                            readWriteVideo(filePath, fileName, outputFolder);
                        }
                    }
                    MessageBox.Show("Videos Written: " + files.Length.ToString(), "Message");
                }
            }
        }

        public void readWriteVideo(String filePath, String name, String outputFolder) {
            
            String outputVideo = outputFolder + "\\" + name.Split('.')[0] + ".avi";
            double TotalFrame;
            double Fps;
            int FrameNo = 0;
            VideoCapture capture = new VideoCapture(filePath);
            TotalFrame = capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameCount);
            Fps = capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps);
            int fourcc = Convert.ToInt32(capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FourCC));
            int Width = Convert.ToInt32(capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth));
            int Height = Convert.ToInt32(capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight));

            
            //VideoWriter writer = new VideoWriter(outputVideo, VideoWriter.Fourcc('X', '2', '6', '4'), Fps, new Size(Width, Height), true);
            VideoWriter writer = new VideoWriter(outputVideo, VideoWriter.Fourcc('M', 'P', '4', 'V'), Fps, new Size(640, 360), true);
            Mat m = new Mat();
            while (FrameNo < TotalFrame) {
                capture.Read(m);
                Image<Bgr, byte> frmImage = m.ToImage<Bgr, byte>().Resize(416,416,Inter.Cubic);
                writer.Write(frmImage.Mat);
                FrameNo++;
            }
            if (writer.IsOpened)
            {
                writer.Dispose();
            }

            Console.WriteLine("Success:  " + outputVideo);
            capture.Stop();
            capture.Dispose();
        }
    }
}
