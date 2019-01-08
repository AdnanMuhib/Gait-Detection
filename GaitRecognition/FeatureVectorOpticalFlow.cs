using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaitRecognition
{
    class FeatureVectorOpticalFlow
    {
        double velx;
        double vely;
        double distance;
        double degrees;
        LineSegment2D line;

        public FeatureVectorOpticalFlow() {
            velx = 0;
            vely = 0;
            distance = 0;
            degrees = 0;
        }

        public FeatureVectorOpticalFlow(double pvelx, double pvely, double dist, double deg)
        {
            velx = pvelx;
            vely = pvely;
            distance = dist;
            degrees = deg;
        }
    }
}
