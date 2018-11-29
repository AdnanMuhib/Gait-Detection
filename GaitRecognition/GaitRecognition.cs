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
using System.Drawing.Imaging;
using Emgu.CV.Util;

namespace GaitRecognition
{
    public partial class GaitRecognition : Form
    {
        // input and output directories for batch Processing
        String inputFolder = @"D:\UNIVERSITY DOCUMENTS\FYP\Human Activity Recognition\KTH Dataset\Gait Pics\Nasir\Nasir3\";
        String outputFolder = @"D:\UNIVERSITY DOCUMENTS\FYP\Human Activity Recognition\Test Outputs\";

        Image<Bgr, byte> BgrImg; // Color Image

        Image<Gray, byte> img; // any input image

        Image<Gray, byte> bgImage; // Background Image from the video or custom input

        int frameIndex;

        VideoCapture _capture; // to read video files
        

        bool saveResults = true;

        public GaitRecognition()
        {
            InitializeComponent();
            this.KeyPreview = true;
            frameIndex = 0;
            //pictureViewBox.SizeMode = PictureBoxSizeMode.Zoom;
            
            bgImage = new Image<Gray, byte>(inputFolder + "b.bmp").Resize(400,400,Inter.Cubic);
            img = new Image<Gray, byte>(inputFolder + "12.bmp").Resize(400, 400, Inter.Cubic);
            BgrImg = new Image<Bgr, byte>(inputFolder + "12.bmp").Resize(400, 400, Inter.Cubic);

            removebackground("12.bmp");
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
            
            List<String> files = new List<string>();
            DirectoryInfo d = new DirectoryInfo(inputFolder);

            FileInfo[] Files = d.GetFiles("*.bmp"); //Getting BMP files
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
                img = new Image<Gray, byte>(inputFolder +  imgName).Resize(400, 400, Inter.Cubic);
                BgrImg = new Image<Bgr, byte>(inputFolder + imgName).Resize(400, 400, Inter.Cubic);
                removebackground("_out_" + imgName);
            }
            this.Close();
            return;
        }

        // Background Subtraction From the Given Background and Input Image
        public void removebackground(string filepath = null) {
            Image<Gray, byte> output = new Image<Gray, byte>(bgImage.Width, bgImage.Height);
            BackgroundSubtractorMOG2 bgsubtractor = new BackgroundSubtractorMOG2(varThreshold:100,shadowDetection:false);
            bgsubtractor.Apply(bgImage, output);
            bgsubtractor.Apply(img, output);
            pictureViewBox.Image = output;
            output.Canny(100,100);

            CvInvoke.Erode(output, output, null, new System.Drawing.Point(-1, -1), 1, BorderType.Reflect, default(MCvScalar));
            CvInvoke.Dilate(output, output, null, new System.Drawing.Point(-1, -1), 1, BorderType.Reflect, default(MCvScalar));

            // Write the Silhoutte output to the file
            if (filepath != null && saveResults == true)
            {
                CvInvoke.Imwrite(outputFolder + "bg_subtract_" + filepath, output);
            }

            // Using Thinning Algorithm on Silhoutte
            Image<Gray, byte> thinOutput = new Image<Gray, byte>(output.Width, output.Height);
            XImgprocInvoke.Thinning(output, thinOutput, ThinningTypes.ZhangSuen);
            pictureViewBox.Image = thinOutput.Not().Not();

            // Write the thinned Image to the file
            if (filepath != null && saveResults == true)
            {
                CvInvoke.Imwrite(outputFolder + "thinned_" + filepath, thinOutput.Not().Not());
                
            }
            PersonFrame person = new PersonFrame();
            person.FindPoints(output);
            person.calculate_width();
            person.calculate_height();
            Point srcPt = new Point(person.top_left.X, person.top_right.Y);
            Rectangle rec = new Rectangle(srcPt, new Size((int)person.width, (int)person.height));
            CvInvoke.Rectangle(thinOutput, rec, new Rgb(Color.White).MCvScalar, 2);
            CvInvoke.Imshow("Person Fame", thinOutput.Not());
            // Applying Hough Line Transformation
            //Hough(thinOutput, filepath);
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
                        8); //gap between lines

            // drawing all hough lines on lineImg
            Mat lineImage = new Mat(ThinImage.Size, DepthType.Cv8U, 3);
            lineImage.SetTo(new MCvScalar(0));
            foreach (LineSegment2D line in lines)
            {
                if ((line.P2.X - line.P1.X) != 0)
                { // to avoid divide by zero
                    double slope = (line.P2.Y - line.P1.Y) / (line.P2.X - line.P1.X);
                    CvInvoke.Line(lineImage, line.P1, line.P2, new Bgr(Color.Green).MCvScalar, 1);
                    //Console.WriteLine("Height {0}, Slope {1}, Direction {2}", line.Length, slope, line.Direction);
                }
            }
            
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
            pictureViewBox.Image = distImg;
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
