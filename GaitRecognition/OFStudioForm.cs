using DirectShowLib;
using Emgu.CV;
using Emgu.CV.Ocl;
using Emgu.CV.Structure;
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace GaitRecognition
{
    public partial class OFStudioForm : Form
    {
        VideoCapture _capture;
        bool isPlaying = false;
        public String videoPath;
        int frameSkip;
        int frameCounter;
        int activityLabel;
        //Image<Gray, byte> grayImage;
        //Image<Bgr, byte> BgrImage;
        Image<Gray, byte> prevFrame;
        Image<Gray, byte> nextFrame;
        OpticalFlow _opticalflow;
        String filename;
        MLP mlp;
        public OFStudioForm()
        {

            InitializeComponent();
            // make a list of connected cameras to the computer
            List<String> cameras = new List<string>();
            videoPath = "";
            frameSkip = 0;
            frameCounter = 0;
            activityLabel = (int)ActivityClass.pointing;
            DsDevice[] _SystemCameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            // if there is any camera
            if (_SystemCameras.Length > 0)
            {
                foreach (DsDevice d in _SystemCameras) {
                    cameras.Add(d.Name); // add the camera to the list
                }
            }
            else { // otherwise show error message
                //MessageBox.Show(" 0 Cameras Detected");
            }

            // show the list of cameras in the drop down list
            comboBoxCameraList.DataSource = cameras;
           
           mlp = new MLP();
           mlp.LoadTrainedModel("ann_mlp_model.xml");
           //mlp.LoadTrainData(@"C:\Users\Antivirus\Desktop\of\train.csv");
           //mlp.Train();
           //mlp.SaveModel("ann_mlp_model.xml");
           //MessageBox.Show("Training Completed");
           //mlp.LoadTestData(@"C:\Users\Antivirus\Desktop\of\test.csv");
           //mlp.Predict();
            //MessageBox.Show("Prediction Completed");
        }

        Mat _frame = new Mat();

        private void imageFrameCaptured(object sender, EventArgs e)
        {
            frameCounter = frameCounter + 1;

            // Handle Every Frame if Video is Playing

            if (radioButtonVideo.Checked) {
                try
                {
                    if (_capture != null)
                    {
                        if (frameCounter == 1) {
                            prevFrame = _capture.QueryFrame().ToImage<Gray, byte>().Resize(200, 200, Emgu.CV.CvEnum.Inter.Area);
                        }
                        else {// if (frameCounter % frameSkip == 0) { // use only the frames after skipped frames
                              //BgrImage = _capture.QueryFrame().ToImage<Bgr, byte>().Resize(400, 400, Emgu.CV.CvEnum.Inter.Area);
                              //pictureViewBox.Image = BgrImage;
                            for (int i = 0; i < frameSkip; i++) {
                                _capture.Grab(); // skip the number of frames
                            }
                            _opticalflow = new OpticalFlow(filename, (int)ActivityClass.walking);
                            nextFrame = _capture.QueryFrame().ToImage<Gray, byte>().Resize(200, 200, Emgu.CV.CvEnum.Inter.Area);
                            Image<Hsv, byte> outputImg = _opticalflow.CalculateOpticalFlow(prevFrame, nextFrame, frameCounter);
                            var sample = _opticalflow.GetFeatureMatrix();
                            int prediction = mlp.Inference(sample);
                            if (prediction == -1) {
                                labelPrediction.Text = "Static";
                            }
                            else {
                                labelPrediction.Text = Enum.GetName(typeof(ActivityClass), prediction);
                            }
                            opticalViewBox.Image = outputImg;
                            outputImg.Dispose();
                            //_opticalflow.PyrLkOpticalFlow(prevFrame, nextFrame);
                            prevFrame.Dispose();
                            pictureViewBox.Image = nextFrame;
                            prevFrame = nextFrame.Clone();
                            nextFrame.Dispose();
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                    _capture.Pause();
                    // detach the callback function
                    _capture.ImageGrabbed -= imageFrameCaptured;

                    _capture.Dispose();
                    _capture = null;

                    isPlaying = false;
                    // change the button icon to play
                    btnPlayPause.BackgroundImage = Properties.Resources.play36;
                }
            }

            // Handle Every Frame if Camera is On
            if (radioButtonCamera.Checked)
            {
                try
                {
                    if (_capture != null)
                    {

                        if (frameCounter == 1)
                        {
                            _capture.Retrieve(_frame, 0);
                            prevFrame = _frame.ToImage<Gray, byte>().Resize(400, 400, Emgu.CV.CvEnum.Inter.Cubic);
                        }
                        else {// if (frameCounter % frameSkip == 0) { // use only the frames after skipped frames
                            _capture.Retrieve(_frame, 0);
                            nextFrame = _frame.ToImage<Gray, byte>().Resize(400, 400, Emgu.CV.CvEnum.Inter.Cubic);
                            _opticalflow = new OpticalFlow("", (int)ActivityClass.walking);
                            opticalViewBox.Image = _opticalflow.CalculateOpticalFlow(prevFrame, nextFrame, frameCounter);
                            pictureViewBox.Image = nextFrame;
                            prevFrame.Dispose();
                            prevFrame = nextFrame.Clone();
                            nextFrame.Dispose();
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    _capture.Pause();
                    // detach the callback function
                    _capture.ImageGrabbed -= imageFrameCaptured;

                    _capture.Dispose();
                    _capture = null;
                    isPlaying = false;
                    // change the button icon to play
                    btnPlayPause.BackgroundImage = Properties.Resources.play36;
                }
            }


        }

        private void radioButtonCamera_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonCamera.Checked) {
                textBoxVideoPath.Enabled = false;
                btnBrowseVideo.Enabled = false;
                comboBoxCameraList.Enabled = true;
            }
        }

        private void radioButtonVideo_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonVideo.Checked) {
                comboBoxCameraList.Enabled = false;
                textBoxVideoPath.Enabled = true;
                btnBrowseVideo.Enabled = true;
            }
        }

        private void btnPlayPause_Click(object sender, EventArgs e)
        {
            if (radioButtonCamera.Checked) // if using Live Camera
            {
                // check if camera is on
                if (isPlaying && _capture.IsOpened)
                {
                    _capture.Pause();
                    // detach the callback function
                    _capture.ImageGrabbed -= imageFrameCaptured;
                    isPlaying = false;
                    // change the button icon to play
                    btnPlayPause.BackgroundImage = Properties.Resources.play36;
                }
                else if (!isPlaying && _capture == null) // check if camera is not or it's first time for camera
                {
                    _capture = new VideoCapture();
                    //_capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.PosFrames, frameSkip);
                    _capture.ImageGrabbed += imageFrameCaptured;
                    _capture.Start();
                    isPlaying = true;
                    btnPlayPause.BackgroundImage = Properties.Resources.pause36;
                }
                else if (!isPlaying && _capture != null) { // if camera is already initialized but paused

                    _capture.ImageGrabbed += imageFrameCaptured;
                    _capture.Start();
                    isPlaying = true;
                    btnPlayPause.BackgroundImage = Properties.Resources.pause36;
                }

            } // end if using live camera
            // if using video input
            else
            {
                // check if video is already playing then pause
                if (isPlaying)
                {
                    _capture.Pause();
                    Application.Idle -= imageFrameCaptured;
                    btnPlayPause.BackgroundImage = Properties.Resources.play36;
                    isPlaying = false;
                }
                // if video path is selected and video not playing
                else if (!isPlaying && videoPath != "")
                {
                    if (_capture == null) { // if playing first time
                        _capture = new VideoCapture(videoPath);
                        //_capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.PosFrames, frameSkip);
                    }
                    Application.Idle += imageFrameCaptured;
                    btnPlayPause.BackgroundImage = Properties.Resources.pause36;
                    isPlaying = true;
                }
            } // end else using video 
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (_capture != null) { // if camera is open or video is playing then 
                _capture.ImageGrabbed -= imageFrameCaptured;
                _capture.Pause();
                _capture.Stop();
                _capture.Dispose();
                btnPlayPause.BackgroundImage = Properties.Resources.play36;
                isPlaying = false;
                pictureViewBox.Image = null;
                _capture = null;
            }
        }

        private void btnBrowseVideo_Click(object sender, EventArgs e)
        {
            videoPath = GaitRecognition.openFileDialogue();
            if (videoPath == null || videoPath == "")
                return;
            FileInfo fi = new FileInfo(videoPath);
            filename = fi.Name.Split('.')[0];
            textBoxVideoPath.Text = videoPath;
            btnPlayPause.BackgroundImage = Properties.Resources.play36;
            isPlaying = false;
            frameCounter = 0;
        }

        private void numericFrameSkip_ValueChanged(object sender, EventArgs e)
        {
            frameSkip = (int)numericFrameSkip.Value;
            //_capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.PosFrames, frameSkip);
        }

        private void selectVideosFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    string[] files = Directory.GetFiles(fbd.SelectedPath);
                    //String location = Path.GetDirectoryName(files[0]);
                    //String outputFolder = location + "\\Soundless";
                    //bool folderExists = Directory.Exists(outputFolder);
                    //if (!folderExists)
                    //   Directory.CreateDirectory(outputFolder);
                    batchProcess(files);
                }
            }
        }

        public void batchProcess(String[] files) {

            foreach (string filePath in files)
            {

                if (filePath.EndsWith(".avi"))
                {
                    if (_capture != null)
                    {
                        Application.Idle -= imageFrameCaptured;
                        _capture.Stop();
                        _capture.Dispose();
                        _capture = null;
                    }

                    filename = Path.GetFileName(filePath).Split('.')[0];
                    _capture = new VideoCapture(filePath);
                    int TotalFrames = Convert.ToInt32(_capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameCount));

                    //ThreadStart ofThread = new ThreadStart(()=>OpticalFlowThread(filePath,TotalFrames));
                    //Thread childThread = new Thread(ofThread);
                    //childThread.Start();

                    ThreadStart framesThread = new ThreadStart(() => SaveVideoFramesThread(filePath, TotalFrames));
                    Thread childThread2 = new Thread(framesThread);
                    childThread2.Start();

                    //SaveVideoFramesThread(filePath, TotalFrames);
                    OpticalFlowThread(filePath, TotalFrames);
                    //childThread.Abort();
                    //childThread2.Abort();
                    /*for (int i = 0; i < TotalFrames; i++) {
                        try
                        {
                            if (_capture != null)
                            {
                                if (i == 0)
                                {
                                    prevFrame = _capture.QueryFrame().ToImage<Gray, byte>().Resize(400, 400, Emgu.CV.CvEnum.Inter.Area);
                                }
                                else
                                {
                                    for (int j = 0; j < frameSkip; j++)
                                    {
                                        _capture.Grab(); // skip the number of frames
                                        i++;
                                    }
                                    _opticalflow = new OpticalFlow(filename, activityLabel);
                                    nextFrame = _capture.QueryFrame().ToImage<Gray, byte>().Resize(400, 400, Emgu.CV.CvEnum.Inter.Area);
                                    Image<Hsv, byte> outputImg = _opticalflow.CalculateOpticalFlow(prevFrame, nextFrame, frameCounter);
                                    opticalViewBox.Image = outputImg;
                                    outputImg.Dispose();
                                    //_opticalflow.PyrLkOpticalFlow(prevFrame, nextFrame);
                                    prevFrame.Dispose();
                                    pictureViewBox.Image = nextFrame;
                                    prevFrame = nextFrame.Clone();
                                    nextFrame.Dispose();
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);

                            _capture.Pause();
                            _capture.Stop();
                            _capture.Dispose();
                            _capture = null;

                            isPlaying = false;
                            // change the button icon to play
                            btnPlayPause.BackgroundImage = Properties.Resources.play36;
                        }
                    }*/

                    //Application.Idle += imageFrameCaptured;
                    //readWriteVideo(filePath, fileName, outputFolder);
                }
                Console.WriteLine("Features Extracted: " + filename);
            }
            MessageBox.Show("Videos Processed: " + files.Length.ToString(), "Message");
        }
        public void OpticalFlowThread(String filePath,int TotalFrames) {
            //try
            //{
                Console.WriteLine("Child thread of Optical Flow starts");
                String fileName = Path.GetFileName(filePath).Split('.')[0];
                VideoCapture capture = new VideoCapture(filePath);
                for (int i = 0; i < TotalFrames - 1; i++)
                {
                    try
                    {
                        if (capture != null)
                        {
                            if (i == 0)
                            {
                                prevFrame = capture.QueryFrame().ToImage<Gray, byte>().Resize(200, 200, Emgu.CV.CvEnum.Inter.Area);
                            }
                            else
                            {
                                for (int j = 0; j < frameSkip; j++)
                                {
                                    capture.Grab(); // skip the number of frames
                                    i++;
                                }
                                _opticalflow = new OpticalFlow(filename, activityLabel);
                                nextFrame = capture.QueryFrame().ToImage<Gray, byte>().Resize(200, 200, Emgu.CV.CvEnum.Inter.Area);
                                _opticalflow.CalculateOpticalFlow(prevFrame, nextFrame, frameCounter);
                                //opticalViewBox.Image = outputImg;
                                //outputImg.Dispose();
                                //_opticalflow.PyrLkOpticalFlow(prevFrame, nextFrame);
                                prevFrame.Dispose();
                                //pictureViewBox.Image = nextFrame;
                                prevFrame = nextFrame.Clone();
                                nextFrame.Dispose();
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);

                        capture.Pause();
                        capture.Stop();
                        capture.Dispose();
                        capture = null;

                        isPlaying = false;
                        // change the button icon to play
                        btnPlayPause.BackgroundImage = Properties.Resources.play36;
                    }
                }
                Console.WriteLine("Child thread of Optical Flow Ends");
            //}
            //catch (ThreadAbortException e)
            //{
            //    Console.WriteLine("Thread Abort Exception");
            //}
            //finally
            //{
            //    Console.WriteLine("Couldn't catch the Thread Exception");
            //}
        }
        public void SaveVideoFramesThread(String filePath,int TotalFrames)
        {
            Console.WriteLine("Child thread of Frames Starts");
            try
            {
                String fileName = Path.GetFileName(filePath).Split('.')[0];
                VideoCapture video = new VideoCapture(filePath);
                String location = Path.GetDirectoryName(filePath);
                String outputFolder = location + "\\frames";
                bool folderExists = Directory.Exists(outputFolder);
                if (!folderExists)
                    Directory.CreateDirectory(outputFolder);
                try
                {
                    for (int i = 0; i < TotalFrames - 1; i++)
                    {
                        Image<Gray, byte> frame = video.QueryFrame().ToImage<Gray, byte>();
                        CvInvoke.Imwrite(outputFolder + "\\" + fileName + " " + i + ".png", frame);
                        frame.Dispose();
                    }
                    video.Stop();
                    video.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("End of Video Exception" + ex.Message);
                }
               
            }
            catch (ThreadAbortException e)
            {
                Console.WriteLine("Thread Abort Exception");
            }
            finally
            {
                Console.WriteLine("Child thread of Frames ends");
            }
        }
    }
}
