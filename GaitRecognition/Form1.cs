using Emgu.CV;
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
using Accord;
using AForge;
using Accord.MachineLearning;
using Accord.Statistics.Filters;
using Accord.Imaging.Filters;
using Accord.Imaging;
using System.Drawing.Imaging;
using Emgu.CV.Util;

namespace GaitRecognition
{
    public partial class Form1 : Form
    {
        // input and output directories for batch Processing
        String inputFolder = @"D:\UNIVERSITY DOCUMENTS\FYP\Human Activity Recognition\KTH Dataset\Gait Pics\Nasir\Nasir3\";
        String outputFolder = @"D:\UNIVERSITY DOCUMENTS\FYP\Human Activity Recognition\Test Outputs\";
        Image<Bgr, byte> BgrImg;
        Image<Gray, byte> img; // any input image
        Image<Gray, byte> bgImage; // Background Image from the video
        int frameIndex;
        VideoCapture _capture; // to read video files
        private Accord.Imaging.Filters.FiltersSequence filter = new Accord.Imaging.Filters.FiltersSequence(
                Grayscale.CommonAlgorithms.BT709,
                new NiblackThreshold(),
                new Invert()
            );
        HoughLineTransformation lineTransform;// = new HoughLineTransformation();

        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true;
            frameIndex = 0;
            //pictureViewBox.SizeMode = PictureBoxSizeMode.Zoom;
            lineTransform = new HoughLineTransformation();
            lineTransform.MinLineIntensity = 10;
            img = new Image<Gray, byte>(outputFolder + "thinned__out_10.bmp");

            // Using Hough Transformation External Class
            /*
            HoughTransformation ht = new HoughTransformation();
            Bitmap transformedImage = ht.Transformation(img.ToBitmap());
            pictureViewBox.Image = new Image<Gray, byte>(transformedImage);*/
            //Image<Gray, byte> binary = img.ThresholdBinary(new Gray(50), new Gray(255));

            //removebackground();
            // batchProcessor();
            //Skelatanize();
            // binarization filtering sequence

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
                detectPerson();
            }
        }

        public void HoughTransform(Bitmap tempImage) {
            Bitmap image = Accord.Imaging.Image.Clone(tempImage, PixelFormat.Format24bppRgb);
            CannyEdgeDetector filter = new CannyEdgeDetector();
            filter.ApplyInPlace(image);
            pictureViewBox.Image = new Image<Bgr, byte>(image);
            // Lock the Source Image
            BitmapData sourceData = image.LockBits(ImageLockMode.ReadOnly);

            // binarize the image
            UnmanagedImage binarySource = filter.Apply(new UnmanagedImage(sourceData));

            tempImage.Dispose();
            // apply Hough line transform
            lineTransform.ProcessImage(binarySource);
            // get lines using relative intensity
            HoughLine[] lines = lineTransform.GetLinesByRelativeIntensity(0.9);
            foreach (HoughLine line in lines) {
                // draw line on the image
                Drawing.Line(sourceData, new IntPoint(), new IntPoint(),
                           Color.Red);
                line.Draw(binarySource, Color.Red);
            }
            image.UnlockBits(sourceData);
            pictureViewBox.Image = new Image<Bgr, byte>(lineTransform.ToBitmap());
            //pictureViewBox.Image = new Image<Bgr, byte>(binarySource.ToManagedImage());
        }
        //    foreach (HoughLine line in lines)
        //    {

        //        // get line's radius and theta values
        //        int r = line.Radius;
        //        double t = line.Theta;

        //        // check if line is in lower part of the image
        //        if (r < 0)
        //        {
        //            t += 180;
        //            r = -r;
        //        }

        //        // convert degrees to radians
        //        t = (t / 180) * Math.PI;

        //        // get image centers (all coordinate are measured relative
        //        // to center)
        //        int w2 = image.Width / 2;
        //        int h2 = image.Height / 2;

        //        double x0 = 0, x1 = 0, y0 = 0, y1 = 0;

        //        if (line.Theta != 0)
        //        {
        //            // none vertical line
        //            x0 = -w2; // most left point
        //            x1 = w2;  // most right point

        //            // calculate corresponding y values
        //            y0 = (-Math.Cos(t) * x0 + r) / Math.Sin(t);
        //            y1 = (-Math.Cos(t) * x1 + r) / Math.Sin(t);
        //        }
        //        else
        //        {
        //            // vertical line
        //            x0 = line.Radius;
        //            x1 = line.Radius;

        //            y0 = h2;
        //            y1 = -h2;
        //        }

        //        // draw line on the image
        //        Drawing.Line(sourceData,
        //                   new IntPoint((int)x0 + w2, h2 - (int)y0),
        //                   new IntPoint((int)x1 + w2, h2 - (int)y1),
        //                   Color.Red);
        //        }
        //    // unlock source image
        //    image.UnlockBits(sourceData);


        //}
        // Open File Selection Dialogue Box
        public String openFileDialogue() {
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
            String inputFolderPath = @"D:\UNIVERSITY DOCUMENTS\FYP\Human Activity Recognition\KTH Dataset\Gait Pics\Asim\asim1\";
            String outputFolderPath = @"D:\UNIVERSITY DOCUMENTS\FYP\Human Activity Recognition\Test Outputs\";
            List<String> files = new List<string>();
            DirectoryInfo d = new DirectoryInfo(inputFolderPath);

            FileInfo[] Files = d.GetFiles("*.bmp"); //Getting BMP files
            foreach (FileInfo file in Files) {
                if (file.Name.Equals("b.bmp"))
                {
                    bgImage = new Image<Gray, byte>(inputFolderPath + file.Name);
                }
                else {
                    files.Add(file.Name);
                }
            }

            foreach (String imgName in files) {
                img = new Image<Gray, byte>(inputFolderPath +  imgName);
                BgrImg = new Image<Bgr, byte>(inputFolderPath + imgName);
                removebackground("_out_" + imgName);
            }
            this.Close();
            return;
        }

        // Background Subtraction From the Given Background and Input Image
        public void removebackground(string filepath = null) {

            //filepath = null; // to avoid saving files in the output folder
            Image<Gray, byte> output = new Image<Gray, byte>(bgImage.Width, bgImage.Height);
            BackgroundSubtractorMOG2 bgsubtractor = new BackgroundSubtractorMOG2(varThreshold:100,shadowDetection:false);
            bgsubtractor.Apply(bgImage, output);
            bgsubtractor.Apply(img, output);
            pictureViewBox.Image = output;
            output.Canny(100,100);
            //img.Dispose();

            CvInvoke.Erode(output, output, null, new System.Drawing.Point(-1, -1), 1, BorderType.Reflect, default(MCvScalar));
            //CvInvoke.Dilate(output, output, null, new System.Drawing.Point(-1, -1), 1, BorderType.Reflect, default(MCvScalar));

            // Write the Silhoutte output to the file
            if (filepath != null)
            {
                CvInvoke.Imwrite(outputFolder + "bg_subtract_" + filepath, output);
            }

            // Using Thinning Algorithm on Silhoutte
            Image<Gray, byte> thinOutput = new Image<Gray, byte>(output.Width, output.Height);
            XImgprocInvoke.Thinning(output, thinOutput, ThinningTypes.ZhangSuen);
            pictureViewBox.Image = thinOutput.Not().Not();

            //Skelatanize(output, filepath);
            /*HoughTransformation ht = new HoughTransformation();
            Bitmap transformedImage = ht.Transformation(img.ToBitmap());
            pictureViewBox.Image = new Image<Gray, byte>(transformedImage);*/
            // Write the thinned Image to the file
            if (filepath != null)
            {
                CvInvoke.Imwrite(outputFolder + "thinned_" + filepath, thinOutput.Not().Not());
                //CvInvoke.Imwrite(outputFolder + "transformed_" + filepath, new Image<Gray, byte>(transformedImage));
            }


            //Hough(thinOutput, filepath);

            //img.Dispose();
            output.Dispose();
            thinOutput.Dispose();
        }
        
        // Applying Hough Transformation to get Straight lines
        public void Hough(Image<Gray, byte> ThinImage, String filePath = null) {

            double cannyThreshold = 0.0;
            double cannyThresholdLinking = 0.0;
            UMat cannyEdges = new UMat();
            // Detection of a person
            //this is the CPU version
            using (HOGDescriptor des = new HOGDescriptor())
            {
                des.SetSVMDetector(HOGDescriptor.GetDefaultPeopleDetector());
                MCvObjectDetection[] objects = des.DetectMultiScale(img, useMeanshiftGrouping: true);

                for (int i = 0; i < objects.Length; i++)
                {
                    ThinImage.ROI = objects[i].Rect;
                    CvInvoke.Canny(ThinImage, cannyEdges, cannyThreshold, cannyThresholdLinking);
                    LineSegment2D[] lines = CvInvoke.HoughLinesP(cannyEdges,
                        1,     //Distance resolution in pixel-related units
                        Math.PI / 120.0, //Angle resolution measured in radians.
                        10, //threshold
                        25, //min Line width
                        8); //gap between lines
                    //BgrImg.Draw(objects[i].Rect, new Bgr(Color.Green), 2);
                    Mat lineImage = new Mat(ThinImage.Size, DepthType.Cv8U, 3);
                    lineImage.SetTo(new MCvScalar(0));
                    foreach (LineSegment2D line in lines)
                    {
                        CvInvoke.Line(BgrImg, line.P1, line.P2, new Bgr(Color.Green).MCvScalar, 1);
                    }
                        
                }
            }
            pictureViewBox.Image = BgrImg;
            BgrImg.Dispose();



            //CvInvoke.Canny(ThinImage, cannyEdges, cannyThreshold, cannyThresholdLinking);
            /*
            LineSegment2D[] lines = CvInvoke.HoughLinesP(
              cannyEdges,
              1,     //Distance resolution in pixel-related units
              Math.PI / 120.0, //Angle resolution measured in radians.
              10, //threshold
              25, //min Line width
              8); //gap between lines
              */
          /*  using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(cannyEdges, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
                int count = contours.Size;
                for (int i = 0; i < count; i++)
                {
                    using (VectorOfPoint contour = contours[i])
                    using (VectorOfPoint approxContour = new VectorOfPoint())
                    {
                        CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * 0.05, true);
                        if (CvInvoke.ContourArea(approxContour, false) < ThinImage.Width) // only consider contours with area less than the width of the image
                        {
                            if (approxContour.Size == 3) //The contour has 3 vertices, it is a triangle
                            {
                                System.Drawing.Point[] pts = approxContour.ToArray();
                            }
                            else if (approxContour.Size == 4) //The contour has 4 vertices.
                            {
                                #region determine if all the angles in the contour are within [80, 100] degree
                                bool isRectangle = true;
                                System.Drawing.Point[] pts = approxContour.ToArray();
                                LineSegment2D[] edges = PointCollection.PolyLine(pts, true);

                                for (int j = 0; j < edges.Length; j++)
                                {
                                    double angle = Math.Abs(
                                       edges[(j + 1) % edges.Length].GetExteriorAngleDegree(edges[j]));
                                    if (angle < 80 || angle > 100)
                                    {
                                        isRectangle = false;
                                        break;
                                    }
                                }
                                #endregion
                            }
                        }
                    }
                }
            } */
            /*
            Mat lineImage = new Mat(ThinImage.Size, DepthType.Cv8U, 3);
            lineImage.SetTo(new MCvScalar(0));
            foreach (LineSegment2D line in lines)
                CvInvoke.Line(lineImage, line.P1, line.P2, new Bgr(Color.Green).MCvScalar, 2);
            //pictureViewBox.Image = lineImage;

            if (filePath != null) {
                CvInvoke.Imwrite(outputFolder+ "Hough__" + filePath, lineImage);
            }*/
        }
        public void detectPerson() {
            // Detection of a person
            //this is the CPU version
            using (HOGDescriptor des = new HOGDescriptor())
            {
                des.SetSVMDetector(HOGDescriptor.GetDefaultPeopleDetector());
                MCvObjectDetection[] objects = des.DetectMultiScale(img, useMeanshiftGrouping:true);

                for (int i = 0; i < objects.Length; i++)
                {
                    BgrImg.Draw(objects[i].Rect, new Bgr(Color.Green), 2);
                }
            }
            pictureViewBox.Image = BgrImg;
            BgrImg.Dispose();
        }
        // Skelton Extraction
        public void Skelatanize(Image<Gray,byte> imgOld, String fileName = null)
        {
            if (fileName == null) {
                return;
            }
            //img = new Image<Gray, byte>(outputFolder + "Frame_27.bmp");
            //Image<Gray, byte> imgOld = new Image<Gray, byte>(outputFolder + fileName);
            Image<Gray, byte> img2 = imgOld.Clone();//(new Image<Gray, byte>(imgOld.Width, imgOld.Height, new Gray(255))).Sub(imgOld);
            Image<Gray, byte> eroded = new Image<Gray, byte>(img2.Size);
            Image<Gray, byte> temp = new Image<Gray, byte>(img2.Size);
            Image<Gray, byte> skel = new Image<Gray, byte>(img2.Size);
            skel.SetValue(0);
            CvInvoke.Threshold(img2, img2, 127, 256, 0);
            var element = CvInvoke.GetStructuringElement(ElementShape.Cross, new Size(3, 3), new System.Drawing.Point(-1, -1));
            bool done = false;

            while (!done)
            {
                CvInvoke.Erode(img2, eroded, element, new System.Drawing.Point(-1, -1), 1, BorderType.Reflect, default(MCvScalar));
                CvInvoke.Dilate(eroded, temp, element, new System.Drawing.Point(-1, -1), 1, BorderType.Reflect, default(MCvScalar));
                CvInvoke.Subtract(img2, temp, temp);
                CvInvoke.BitwiseOr(skel, temp, skel);
                eroded.CopyTo(img2);
                if (CvInvoke.CountNonZero(img2) == 0) done = true;
            }
            CvInvoke.Imwrite(outputFolder + "Sk_" + fileName, skel);
            //pictureViewBox.Image = skel;
            //return skel.Bitmap;
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
                    bgImage = _capture.QueryFrame().ToImage<Gray, byte>();
                    CvInvoke.Imwrite(outputFolder + "Frame_"+ frameIndex + ".bmp", bgImage);
                }
                else
                {
                    img = new Image<Gray, byte>(bgImage.Width, bgImage.Height);
                    
                    if (_capture.Retrieve(img))
                    {
                        try
                        {
                            img = _capture.QueryFrame().ToImage<Gray, byte>();
                            BgrImg = _capture.QueryFrame().ToImage<Bgr, byte>();
                            //CvInvoke.Imwrite(outputFolder + "Frame_" + frameIndex + ".bmp", img);
                            removebackground("Frame_" + frameIndex + ".bmp");
                            //detectPerson();
                        }
                        catch (Exception ex)
                        {
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

    }
}
