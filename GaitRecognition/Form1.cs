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

        Image<Bgr, byte> BgrImg; // Color Image

        Image<Gray, byte> img; // any input image

        Image<Gray, byte> bgImage; // Background Image from the video or custom input

        int frameIndex;

        VideoCapture _capture; // to read video files
        
        HoughLineTransformation lineTransform;

        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true;
            frameIndex = 0;
            //pictureViewBox.SizeMode = PictureBoxSizeMode.Zoom;
            lineTransform = new HoughLineTransformation();
            lineTransform.MinLineIntensity = 10;
            
            
            bgImage = new Image<Gray, byte>(inputFolder + "b.bmp");
            img = new Image<Gray, byte>(inputFolder + "11.bmp");
            BgrImg = new Image<Bgr, byte>(inputFolder + "11.bmp");

            removebackground();
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
                    bgImage = new Image<Gray, byte>(inputFolder + file.Name);
                }
                else {
                    files.Add(file.Name);
                }
            }

            foreach (String imgName in files) {
                img = new Image<Gray, byte>(inputFolder +  imgName);
                BgrImg = new Image<Bgr, byte>(inputFolder + imgName);
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

            CvInvoke.Erode(output, output, null, new System.Drawing.Point(-1, -1), 1, BorderType.Reflect, default(MCvScalar));
            CvInvoke.Dilate(output, output, null, new System.Drawing.Point(-1, -1), 1, BorderType.Reflect, default(MCvScalar));

            // Write the Silhoutte output to the file
            if (filepath != null)
            {
                CvInvoke.Imwrite(outputFolder + "bg_subtract_" + filepath, output);
            }

            // Using Thinning Algorithm on Silhoutte
            Image<Gray, byte> thinOutput = new Image<Gray, byte>(output.Width, output.Height);
            XImgprocInvoke.Thinning(output, thinOutput, ThinningTypes.ZhangSuen);
            pictureViewBox.Image = thinOutput.Not().Not();

            // Write the thinned Image to the file
            if (filepath != null)
            {
                CvInvoke.Imwrite(outputFolder + "thinned_" + filepath, thinOutput.Not().Not());
                
            }

            // Applying Hough Line Transformation
            Hough(thinOutput, filepath);
            img.Dispose();
            output.Dispose();
            thinOutput.Dispose();
        }
        
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

            Mat lineImage = new Mat(ThinImage.Size, DepthType.Cv8U, 3);
            lineImage.SetTo(new MCvScalar(0));
            foreach (LineSegment2D line in lines)
            {
                CvInvoke.Line(lineImage, line.P1, line.P2, new Bgr(Color.Green).MCvScalar, 2);
            }

            if (filePath != null)
            {
                CvInvoke.Imwrite(outputFolder + "HoughLine_" + filePath, lineImage);
            }

            pictureViewBox.Image = lineImage;
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
