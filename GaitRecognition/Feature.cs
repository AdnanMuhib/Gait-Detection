using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace GaitRecognition
{
    class Feature
    {
        public FeaturePoint HeadPoint;
        public FeaturePoint ArmsJointPoint;
        public FeaturePoint LeftArmPoint;
        public FeaturePoint RightArmPoint;
        public FeaturePoint SilhoutteCentroid;
        public FeaturePoint LeftFootPoint;
        public FeaturePoint RightFootPoint;

        public static List<FeaturePoint> features = new List<FeaturePoint>();
        //public static 
        public Feature() {
            HeadPoint = new FeaturePoint();
            ArmsJointPoint = new FeaturePoint();
            LeftArmPoint = new FeaturePoint();
            RightArmPoint = new FeaturePoint();
            SilhoutteCentroid = new FeaturePoint();
            LeftFootPoint = new FeaturePoint();
            RightFootPoint = new FeaturePoint();
        }
        // Method to Extract the Feature Points and assign them in the list
        public static List<FeaturePoint> ExtractFeaturePoints(
            Line BigCommonLine,
            Line BottomLeftLine,
            Line BottomRightLine,
            Line TopLeftLine,
            Line TopRightLine) {
            // for head point top point with smaller y value will be the head point
            if (BigCommonLine != null) {
                FeaturePoint fp;
                if (BigCommonLine.p1.Y < BigCommonLine.p2.Y)
                {
                    fp = new FeaturePoint(BigCommonLine.p1, "headpoint");
                }
                else {
                    fp = new FeaturePoint(BigCommonLine.p2, "headpoint");
                }

                features.Add(fp);
            }

            // Intersection Point of Two Arms is ArmsJointPoint
            if (TopLeftLine != null && TopRightLine != null) {
                Point p = new Point();
                p = FindIntersectionPoint(TopLeftLine, TopRightLine);
                FeaturePoint fp = new FeaturePoint(p, "armsjointpoint");
                features.Add(fp);
            }

            // for left arm point TopLeftLine with large Y Value
            if (TopLeftLine != null) {
                FeaturePoint fp;
                if (TopLeftLine.p1.Y > TopLeftLine.p2.Y)
                {
                    fp = new FeaturePoint(TopLeftLine.p1, "leftarmpoint");
                }
                else
                {
                    fp = new FeaturePoint(TopLeftLine.p2, "leftarmpoint");
                }
                features.Add(fp);
            }

            // for right arm point TopRightLine with large Y Value
            if (TopRightLine != null)
            {
                FeaturePoint fp;
                if (TopRightLine.p1.Y > TopRightLine.p2.Y)
                {
                    fp = new FeaturePoint(TopRightLine.p1, "rightarmpoint");
                }
                else
                {
                    fp = new FeaturePoint(TopRightLine.p2, "rightarmpoint");
                }
                features.Add(fp);
            }

            // left foot point with large Y value of BottomLeftLine
            if (BottomLeftLine != null)
            {
                FeaturePoint fp;
                if (BottomLeftLine.p1.Y > BottomLeftLine.p2.Y)
                {
                    fp = new FeaturePoint(BottomLeftLine.p1, "leftfootpoint");
                }
                else
                {
                    fp = new FeaturePoint(BottomLeftLine.p2, "leftfootpoint");
                }
                features.Add(fp);
            }

            // for right foot point BottomRightLine with large Y Value
            if (BottomRightLine != null)
            {
                FeaturePoint fp;
                if (BottomRightLine.p1.Y > BottomRightLine.p2.Y)
                {
                    fp = new FeaturePoint(BottomRightLine.p1, "rightfootpoint");
                }
                else
                {
                    fp = new FeaturePoint(BottomRightLine.p2, "rightfootpoint");
                }
                features.Add(fp);
            }
            // centroid Point will be the intersection point of bottom left line and bottom right line
            if (BottomLeftLine != null && BottomRightLine != null) {
                Point p = new Point();
                p = FindIntersectionPoint(BottomLeftLine, BottomRightLine);
                FeaturePoint fp = new FeaturePoint(p, "centroidpoint");
                features.Add(fp);
            }

            return features;
        }

        // Method to find the intersection point of two lines
        private static Point FindIntersectionPoint(Line l1, Line l2) {
            Point s1 = l1.p1;
            Point e1 = l1.p2;
            Point s2 = l2.p1;
            Point e2 = l2.p2;

            float a1 = e1.Y - s1.Y;
            float b1 = s1.X - e1.X;
            float c1 = a1 * s1.X + b1 * s1.Y;

            float a2 = e2.Y - s2.Y;
            float b2 = s2.X - e2.X;
            float c2 = a2 * s2.X + b2 * s2.Y;

            float delta = a1 * b2 - a2 * b1;
            //If lines are parallel, the result will be (NaN, NaN).
            return delta == 0 ? new Point(0,0)
                : new Point((int)((b2 * c1 - b1 * c2) / delta), (int)((a1 * c2 - a2 * c1) / delta));
        }

    }
}
