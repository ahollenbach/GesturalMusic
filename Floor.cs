using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace GesturalMusic
{
    class Floor
    {
        // The corner points of our 4-sided floor shape
        public Point[] points;

        // This is a hard-set value by the user, due to projector angles sucking
        public Point centroid;

        public Floor(double x, double y, double w, double h)
        {
            points = new Point[4];
            //points[0] = new Point(x, y);
            //points[1] = new Point(x, y+h);
            //points[2] = new Point(x+w, y+h);
            //points[3] = new Point(x+w, y);


            // hardcoded for quick reset
            points[0] = new Point(0,326.25);
            points[1] = new Point(61.25,523.125);
            points[2] = new Point(796.875,500.625);
            points[3] = new Point(798.75,309.375);

            centroid = GetPolygonMidpoint();
        }

        /// <summary>
        /// Gets a set of points that represent the given quadrant. Use the figure
        /// below for reference. The points on the outside are the indices for the points
        ///   0       3
        ///    q3 | q2
        /// ------------
        ///    q1 | q0
        ///   1  KIN  2
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Point[] GetQuadrantPoints(int i)
        {
            // first we need to find ratio of front to back
            //double widthUpper = points[3].X - points[0].X;
            //double widthLower = points[2].X - points[1].X;
            //double ratio      = widthUpper / widthLower;
            double ratio = 1.1;

            // Get what we thought it would be
            Point wrongMidpoint = GetPolygonMidpoint();              //
            Point polyMidpoint = centroid;

            Point midpoint0_1 = GetMidpoint(points[0], points[1], Math.Abs(wrongMidpoint.Y - polyMidpoint.Y)); //
            Point midpoint1_2  = GetMidpoint(points[1], points[2]);
            Point midpoint2_3  = GetMidpoint(points[3], points[2], ratio); //
            Point midpoint3_0  = GetMidpoint(points[3], points[0]);
            

            Point[] ps = new Point[4];

            // i is the quadrant number
            // always go counter-clockwise...might not make a difference, but consistency!
            switch (i)
            {
                case 0:
                    ps[0] = points[2];
                    ps[1] = midpoint2_3;
                    ps[2] = polyMidpoint;
                    ps[3] = midpoint1_2;
                    break;
                case 1:
                    ps[0] = points[1];
                    ps[1] = midpoint1_2;
                    ps[2] = polyMidpoint;
                    ps[3] = midpoint0_1;
                    break;
                case 2:
                    ps[0] = points[3];
                    ps[1] = midpoint3_0;
                    ps[2] = polyMidpoint;
                    ps[3] = midpoint2_3;
                    break;
                case 3:
                    ps[0] = points[0];    // Corner
                    ps[1] = midpoint0_1;  // Midpoint of upperleft and lowerleft
                    ps[2] = polyMidpoint; // Midpoint of all 4
                    ps[3] = midpoint3_0;  // Midpoint of upperleft and upperright
                    break;
                default:
                    Console.WriteLine("ERROR");
                    break;
            }


            return ps;
        }

        /// <summary>
        /// Gets the midpoint. If you specify a ratio, make sure that p1 is the further away point
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="ratio">The ratio of the front and back widths. It might now be the difference in Y dist</param>
        /// <returns></returns>
        private Point GetMidpoint(Point p1, Point p2, double ratio = 1)
        {
            if (ratio == 1)
            {
                return new Point(centroid.X, (p1.Y + p2.Y)/2);
            }
            else
            {
                return new Point((p1.X + p2.X) / 2 - ratio/2, centroid.Y);
            }
            //return new Point((p1.X + p2.X) / 2, (p1.Y*ratio + p2.Y) / (1+ratio));
            
        }

        private Point GetPolygonMidpoint(double ratio = 1)
        {
            double xAvg = 0;
            double yAvg = 0;

            for (int i = 0; i < points.Length; i++)
            {
                xAvg += points[i].X;
                yAvg += points[i].Y * ((i == 0 || i == 3) ? ratio : 1);
            }

            return new Point(xAvg / points.Length, yAvg / (points.Length-2+2*ratio));
        }


        public void SetUpperLeft(double x, double y)
        {
            points[0] = new Point(x, y);
        }
        public void SetLowerLeft(double x, double y)
        {
            points[1] = new Point(x, y);
        }
        public void SetLowerRight(double x, double y)
        {
            points[2] = new Point(x, y);
        }
        public void SetUpperRight(double x, double y)
        {
            points[3] = new Point(x, y);
        }


        public void SetUpperLeft(Point p)
        {
            points[0] = p;
        }
        public void SetLowerLeft(Point p)
        {
            points[1] = p;
        }
        public void SetLowerRight(Point p)
        {
            points[2] = p;
        }
        public void SetUpperRight(Point p)
        {
            points[3] = p;
        }
    }
}
