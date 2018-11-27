using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaitRecognition
{
    class Line
    {
        public double length;
        public Point p1;
        public Point p2;
        public double slope;

        public Line() {
            length = 0;
            p1 = new Point();
            p2 = new Point();
            slope = 0;
        }

        public LineSegment2D[] ConvertToArray(List<Line> lstLines)
        {
            return new LineSegment2D[10];
        }

        public static List<Line> ConvertToList(LineSegment2D[] arrLines) {
            List<Line> lines = new List<Line>();

            foreach (LineSegment2D line in arrLines) {
                if ((line.P2.X - line.P1.X) == 0)
                {
                    continue;
                }

                Line l = new Line();
                l.length = line.Length;
                l.p1.X = line.P1.X;
                l.p2.X = line.P2.X;
                l.p1.Y = line.P1.Y;
                l.p2.Y = line.P2.Y;
                l.slope = (line.P2.Y - line.P1.Y) / (line.P2.X - line.P1.X);
                lines.Add(l);
            }
            return lines;
        }

        public static Line MergeLinesBySlope(List<Line> lstLines) {
            if (lstLines.Count > 0) {
                Line l = new Line();
                l = lstLines[0];

                foreach (Line i in lstLines) {
                    if (i.p1.Y > l.p1.Y) {
                        l.p1.Y = i.p1.Y;
                        l.p1.X = i.p1.X;
                    }
                    if (i.p2.Y < l.p2.Y)
                    {
                        l.p2.Y = i.p2.Y;
                        l.p2.X = i.p2.X;
                    }
                }
                return l;
            }
            return null;
        }


        private static bool MergeByDistance(Line l1,Line l2, int thresh, bool p1) {
            double distance;
            if (p1)
            {
                distance = Math.Abs(l1.p1.X - l2.p1.X);
            }
            else {
                distance = Math.Abs(l1.p2.X - l2.p2.X);
            }
            

            if (distance <= thresh) {
                return true;
            }
            return false;
        }

        public static List<Line> MergeLinesByDistance(List<Line> lstLines, int thresh, bool p1 = true) {
            bool merged = false;
            try
            {

                for (int i = 0; i < lstLines.Count - 1; i++)
                {
                    if (MergeByDistance(lstLines[i], lstLines[i + 1], thresh, p1))
                    {
                        if (lstLines[i].length > lstLines[i + 1].length)
                        {
                            lstLines.Remove(lstLines[i + 1]);
                        }
                        else
                        {
                            lstLines.Remove(lstLines[i]);
                        }
                        merged = true;
                        break;
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }


            if (merged) {
                Line.MergeLinesByDistance(lstLines, thresh);
            }
            
            return lstLines;
        }
        
    }
    
}
