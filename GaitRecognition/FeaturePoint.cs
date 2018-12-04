using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaitRecognition
{
    class FeaturePoint
    {
        public Point point;
        public string name;

        public FeaturePoint() {
            point = new Point();
            name = "random";
        }

        public FeaturePoint(Point p, String n) {
            point = new Point();
            point.X = p.X;
            point.Y = p.Y;
            name = n;
        }
    }
   
}
