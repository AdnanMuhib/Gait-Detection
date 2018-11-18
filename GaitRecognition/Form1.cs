using Emgu.CV;
using Emgu.CV.BgSegm;
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
            //removebackground();
            batchProcessor();
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
                removebackground("bg_" + imgName);
            }
            return;
        }

        // Background Subtraction From the Given Background and Input Image
        public void removebackground(string filepath = null) {
            //bgImage = new Image<Gray, byte>(outputFolder + "Frame_102.bmp");
            //img = new Image<Gray, byte>(outputFolder + "Frame_27.bmp");
            //Image<Bgr, byte> bgImage1 = new Image<Bgr, byte>(outputFolder + "Frame_102.bmp");
            //Image<Bgr, byte>  img1 = new Image<Bgr, byte>(outputFolder + "Frame_27.bmp");
            try
            {
               // bgImage = bgImage.SmoothBlur(2, 2, true);
                //img = img.SmoothBlur(2, 2, true);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
            

            Image<Gray, byte> output = new Image<Gray, byte>(bgImage.Width, bgImage.Height);

            BackgroundSubtractorMOG2 bgsubtractor = new BackgroundSubtractorMOG2(varThreshold:80,shadowDetection:false);
            bgsubtractor.Apply(bgImage, output);
            bgsubtractor.Apply(img, output);
            pictureViewBox.Image = output;
            img.Dispose();
            if (filepath != null) {
                CvInvoke.Imwrite(outputFolder + filepath, output);
            }
            output.Dispose();
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
                            CvInvoke.Imwrite(outputFolder + "Frame_" + frameIndex + ".bmp", img);
                            removebackground();
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
