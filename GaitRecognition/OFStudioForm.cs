using DirectShowLib;
using Emgu.CV;
using Emgu.CV.Ocl;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GaitRecognition
{
    public partial class OFStudioForm : Form
    {
        VideoCapture _capture;
        bool isPlaying = false;
        public String videoPath;

        Image<Gray, byte> grayImage;
        Image<Bgr, byte> BgrImage;
        Image<Gray, byte> prevFrame;
        Image<Gray, byte> nextFrame;

        public OFStudioForm()
        {
            videoPath = "";
            InitializeComponent();
            // make a list of connected cameras to the computer
            List<String> cameras = new List<string>();


            DsDevice[] _SystemCameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            // if there is any camera
            if (_SystemCameras.Length > 0)
            {
                foreach (DsDevice d in _SystemCameras) {
                    cameras.Add(d.Name); // add the camera to the list
                }
            }
            else { // otherwise show error message
                MessageBox.Show(" 0 Cameras Detected");
            }
            
            // show the list of cameras in the drop down list
            comboBoxCameraList.DataSource = cameras;

        }

        Mat _frame = new Mat();
            
        private void imageFrameCaptured(object sender, EventArgs e)
        {

            // Handle Every Frame if Video is Playing
            if (radioButtonVideo.Checked) {
                try
                {
                    if (_capture != null)
                    {

                        BgrImage = _capture.QueryFrame().ToImage<Bgr, byte>().Resize(300, 300, Emgu.CV.CvEnum.Inter.Area);
                        pictureViewBox.Image = BgrImage;
                        //CvInvoke.Imshow("Frame ", BgrImage);
                        //_frame.Dispose();
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    _capture.Pause();

                    // detach the callback function
                    _capture.ImageGrabbed -= imageFrameCaptured;
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
                        _capture.Retrieve(_frame, 0);
                        pictureViewBox.Image = _frame.ToImage<Bgr, byte>().Resize(300,300,Emgu.CV.CvEnum.Inter.Cubic);
                        //CvInvoke.Imshow("Frame ", BgrImage);
                        //_frame.Dispose();
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    _capture.Pause();
                    // detach the callback function
                    _capture.ImageGrabbed -= imageFrameCaptured;
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
                    _capture.ImageGrabbed += imageFrameCaptured;
                    _capture.Start();
                    isPlaying = true;
                    btnPlayPause.BackgroundImage = Properties.Resources.pause36;
                }
                else if(!isPlaying && _capture != null) { // if camera is already initialized but paused

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
                    }
                    //_capture.ImageGrabbed += imageFrameCaptured;
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
            }
        }

        private void btnBrowseVideo_Click(object sender, EventArgs e)
        {
            videoPath = GaitRecognition.openFileDialogue();
            textBoxVideoPath.Text = videoPath;
            btnPlayPause.BackgroundImage = Properties.Resources.play36;
            isPlaying = false;
        }
    }
}
