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
        Image<Bgr, byte> img; // any input image
        Image<Gray, byte> bgImage; // Background Image from the video

        VideoCapture _capture; // to read video files

        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true;
            //batchProcessor();
            removebackground();
            _capture = new VideoCapture(@"D:\UNIVERSITY DOCUMENTS\FYP\Human Activity Recognition\KTH Dataset\walking_person_15.avi");
            _capture.ImageGrabbed += _capture_ImageGrabbed;
        }

        private void _capture_ImageGrabbed(object sender, EventArgs e)
        {

            throw new NotImplementedException();
        }

        private void processToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        // When Menu item Open is clicked
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialogue();
        }

        // Open File Selection Dialogue Box
        public void openFileDialogue() {
            OpenFileDialog f = new OpenFileDialog();
            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                img = new Image<Bgr, byte>(f.FileName);
                pictureViewBox.Image = img;
            }
        }

        // when shortcut key of CTRL + O is pressed
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.O)
            {
                openFileDialogue();
            }
        }

        // Batch Processor
        public void batchProcessor() {
            String inputFolderPath = @"D:\UNIVERSITY DOCUMENTS\FYP\Human Activity Recognition\KTH Dataset\Gait Pics\";
            String outputFolderPath = @"D:\UNIVERSITY DOCUMENTS\FYP\Human Activity Recognition\Test Outputs\";
            List<String> files = new List<string>();
            DirectoryInfo d = new DirectoryInfo(inputFolderPath);

            FileInfo[] Files = d.GetFiles("*.bmp"); //Getting BMP files
            foreach (FileInfo file in Files) {
                files.Add(file.Name);
            }

            return;
        }

        public void removebackground(string filepath = null) {

            String inputFolderPath = @"D:\UNIVERSITY DOCUMENTS\FYP\Human Activity Recognition\KTH Dataset\Gait Pics\";

            bgImage = new Image<Gray, byte>(inputFolderPath + "b_2.bmp");
            Image<Gray, byte> imgOrignal = new Image<Gray, byte>(inputFolderPath + "9.bmp");
            Image<Gray, byte> output = new Image<Gray, byte>(bgImage.Width, bgImage.Height);

            //BackgroundSubtractorMOG2 bgsubtractor = new BackgroundSubtractorMOG2(varThreshold:80,shadowDetection:false);
            BackgroundSubtractorGSOC bgsubtractor = new BackgroundSubtractorGSOC(BackgroundSubtractorLSBP.CameraMotionCompensation.LK);
            bgsubtractor.Apply(bgImage, output);
            bgsubtractor.Apply(imgOrignal, output);
            pictureViewBox.Image = output;
        }
    }
}
