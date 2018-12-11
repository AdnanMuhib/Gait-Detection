using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaitRecognition
{
    class OpticalFlow
    {
        public OpticalFlow() {


        }

        public static Image<Hsv, byte> CalculateOpticalFlow(Image<Gray, byte> prevFrame, Image<Gray, byte> nextFrame, int frameNumber = 0) {

            Image<Gray, float> velx = new Image<Gray, float>(new Size(prevFrame.Width, prevFrame.Height));
            Image<Gray, float> vely = new Image<Gray, float>(new Size(prevFrame.Width, prevFrame.Height));

            CvInvoke.CalcOpticalFlowFarneback(prevFrame, nextFrame, velx, vely, 0.5, 3, 60, 3, 5, 1.1, OpticalflowFarnebackFlag.Default);
            Image<Hsv, Byte> coloredMotion = new Image<Hsv, Byte>(new Size(prevFrame.Width, prevFrame.Height));

            StreamWriter fs = new StreamWriter("C:\\Users\\Antivirus\\Desktop\\of\\opticalflow" + (frameNumber -1) + "-" + (frameNumber) + ".csv");
            fs.WriteLine("velx," + "vely," + "degrees," + "distance");

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

                    fs.WriteLine(velxHere + "," + velyHere + "," + degrees + "," + intensity + "");
                }
            }

            fs.Flush();
            fs.Close();

            // coloredMotion is now an image that shows intensity of motion by lightness
            // and direction by color.
            //CvInvoke.Imshow("Lightness Motion", coloredMotion);
            return coloredMotion;
        }
    }
    

}
