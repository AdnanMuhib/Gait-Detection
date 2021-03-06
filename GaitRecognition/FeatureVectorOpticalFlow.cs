﻿using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaitRecognition
{
    class FeatureVectorOpticalFlow
    {
        public double velx;
        public double vely;
        public double distance;
        public double degrees;
        public LineSegment2D line;

        public FeatureVectorOpticalFlow() {
            velx = 0;
            vely = 0;
            distance = 0;
            degrees = 0;
            line = new LineSegment2D(new Point(0,0), new Point(0,0));
        }

        public FeatureVectorOpticalFlow(double pvelx, double pvely, double dist, double deg, LineSegment2D pline)
        {
            velx = pvelx;
            vely = pvely;
            distance = dist;
            degrees = deg;
            line = pline;
        }
    }
    public enum ActivityClass{
        walking = 0,
        jogging = 1,
        running = 2,
        punching = 3,
        kicking = 4,
        waving = 5,
        pointing = 6
    }
}
