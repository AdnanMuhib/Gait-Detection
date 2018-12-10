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
        private int _CameraIndex;
        private bool _captureInProgress = false;
        int CameraDevice = 0;
        bool playpauseToggle = false;
        public OFStudioForm()
        {
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


            _capture = new VideoCapture();
            _capture.ImageGrabbed += imageFrameCaptured;

        }

        private void imageFrameCaptured(object sender, EventArgs e)
        {
            if (_capture != null) {
                Mat _frame = new Mat();
                _capture.Retrieve(_frame,0);
                pictureViewBox.Image = _frame;
                _frame.Dispose();
            }
            
            //MessageBox.Show("Camera Detected");
            //_capture.ImageGrabbed -= imageFrameCaptured;
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
            if (playpauseToggle) // if playing already then pause
            {
                btnPlayPause.BackgroundImage = Properties.Resources.play36;
                playpauseToggle = false;
                
            }
            else { // if not playing then play
                btnPlayPause.BackgroundImage = Properties.Resources.pause36;
                playpauseToggle = true;                
                _capture.Start();
            }
            
        }
    }
}
