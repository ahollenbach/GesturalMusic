using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GesturalMusic
{
    class Utils
    {
        public static float Clamp(float min, float max, float value)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public static float Length(Joint p1, Joint p2)
        {
            return (float)Math.Sqrt(
                Math.Pow(p1.Position.X - p2.Position.X, 2) +
                Math.Pow(p1.Position.Y - p2.Position.Y, 2) +
                Math.Pow(p1.Position.Z - p2.Position.Z, 2));
        }

        public static float Length(DepthSpacePoint p1, DepthSpacePoint p2)
        {
            return (float)Math.Sqrt(
                Math.Pow(p1.X - p2.X, 2) +
                Math.Pow(p1.Y - p2.Y, 2));
        }
    }
}
