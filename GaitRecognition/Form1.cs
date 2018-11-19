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

namespace GaitRecognition
{
    public partial class Form1 : Form
    {
        // input and output directories for batch Processing
        String inputFolder = @"D:\UNIVERSITY DOCUMENTS\FYP\Human Activity Recognition\KTH Dataset\Gait Pics\Ahmad\ahmad5\";
        String outputFolder = @"D:\UNIVERSITY DOCUMENTS\FYP\Human Activity Recognition\Test Outputs\";

        Image<Gray, byte> img; // any input image
        Image<Gray, byte> bgImage; // Background Image from the video
        int frameIndex;
        VideoCapture _capture; // to read video files



        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true;
            frameIndex = 0;
            pictureViewBox.SizeMode = PictureBoxSizeMode.Zoom;
            //img = new Image<Gray, byte>(outputFolder + "bg_18.bmp");
            //Image<Gray, byte> binary = img.ThresholdBinary(new Gray(50), new Gray(255));
            
            //removebackground();
           // batchProcessor();
            //Skelatanize();
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
                CvInvoke.Threshold(img, img, 10, 128, Emgu.CV.CvEnum.ThresholdType.Otsu);
                pictureViewBox.Image = img;
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
                    pictureViewBox.Image = img;
                }
            }
            
        }

        // Batch Processor
        public void batchProcessor() {
            String inputFolderPath = @"D:\UNIVERSITY DOCUMENTS\FYP\Human Activity Recognition\KTH Dataset\Gait Pics\Ahmad\ahmad5\";
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
                removebackground("_out_" + imgName);
            }
            this.Close();
            return;
        }

        // Background Subtraction From the Given Background and Input Image
        public void removebackground(string filepath = null) {

            filepath = null; // to avoid saving files in the output folder
            Image<Gray, byte> output = new Image<Gray, byte>(bgImage.Width, bgImage.Height);
            BackgroundSubtractorMOG2 bgsubtractor = new BackgroundSubtractorMOG2(varThreshold:100,shadowDetection:false);
            bgsubtractor.Apply(bgImage, output);
            bgsubtractor.Apply(img, output);
            pictureViewBox.Image = output;
            output.Canny(100,100);
            img.Dispose();

            CvInvoke.Erode(output, output, null, new Point(-1, -1), 1, BorderType.Reflect, default(MCvScalar));
            CvInvoke.Dilate(output, output, null, new Point(-1, -1), 1, BorderType.Reflect, default(MCvScalar));

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


            // Write the thinned Image to the file
            if (filepath != null)
            {
                CvInvoke.Imwrite(outputFolder + "thinned_" + filepath, thinOutput.Not().Not());
            }

            Image<Bgr, byte> linesImg = new Image<Bgr, byte>(thinOutput.Width, thinOutput.Height);

            // Using Hough Transform for Line Feature Extraction

            
            

            double cannyThreshold = 180.0;
            double cannyThresholdLinking = 120.0;
            UMat cannyEdges = new UMat();
            CvInvoke.Canny(thinOutput, cannyEdges, cannyThreshold, cannyThresholdLinking);
            LineSegment2D[] lines = CvInvoke.HoughLinesP(
              cannyEdges,
              3,  // Distance resolution in pixel-related units
              Math.PI / 45.0,  // Angle resolution measured in radians.
              10, // threshold
              30, // min Line width
              20); // gap between lines

            foreach (LineSegment2D line in lines)
            {
                linesImg.Draw(line, new Bgr(Color.Green), 1);
            }
            // Write the Lined Image to the file
            if (filepath != null)
            {
                CvInvoke.Imwrite(outputFolder + "lines_" + filepath, linesImg);
            }
           //pictureViewBox.Image = linesImg;
            img.Dispose();
            linesImg.Dispose();
            output.Dispose();
            thinOutput.Dispose();
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
            var element = CvInvoke.GetStructuringElement(ElementShape.Cross, new Size(3, 3), new Point(-1, -1));
            bool done = false;

            while (!done)
            {
                CvInvoke.Erode(img2, eroded, element, new Point(-1, -1), 1, BorderType.Reflect, default(MCvScalar));
                CvInvoke.Dilate(eroded, temp, element, new Point(-1, -1), 1, BorderType.Reflect, default(MCvScalar));
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
