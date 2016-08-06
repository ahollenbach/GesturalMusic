using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace GesturalMusic
{
    /// <summary>
    /// A basic struct to store our floor points
    /// </summary>
    public struct Floor
    {
        public Point3D backLeft, frontLeft, frontRight, backRight, centroid;

        public Floor(Point3D backLeft, Point3D frontLeft, Point3D frontRight, Point3D backRight, Point3D centroid)
        {
            this.backLeft = backLeft;
            this.backRight = backRight;
            this.frontLeft = frontLeft;
            this.frontRight = frontRight;
            this.centroid = centroid;
        }

        public void SetFloorPoint(int index, Point3D p)
        {
            //   3       2
            //    q3 | q2
            // ------------
            //    q1 | q0
            //   1       0
            //      KIN
            switch (index)
            {
                case 0:
                    frontRight = p;
                    break;
                case 1:
                    frontLeft = p;
                    break;
                case 2:
                    backRight = p;
                    break;
                case 3:
                    backLeft = p;
                    break;
            }
        }

        public void SetCentroid(Point3D p)
        {
            this.centroid = p;
        }
    }
}
