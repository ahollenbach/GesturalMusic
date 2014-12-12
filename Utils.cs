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
        /// <summary>
        /// Clamps the value between the min and max
        /// </summary>
        /// <param name="min">The min to clamp to.</param>
        /// <param name="max">The max to clamp to.</param>
        /// <param name="value">The value to clamp.</param>
        /// <returns>The value clamped. If the value is less than the minimum, it will return the minimum.
        /// If the value is greater than the maximum, it will return the maximum.</returns>
        public static float Clamp(float min, float max, float value)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        /// <summary>
        /// Clamps the value between the min and max
        /// </summary>
        /// <param name="min">The min to clamp to.</param>
        /// <param name="max">The max to clamp to.</param>
        /// <param name="value">The value to clamp.</param>
        /// <returns>The value clamped. If the value is less than the minimum, it will return the minimum.
        /// If the value is greater than the maximum, it will return the maximum.</returns>
        public static float Clamp(int min, int max, int value)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public static float LengthFloat(Joint p1, Joint p2)
        {
            return (float)Math.Sqrt(
                Math.Pow(p1.Position.X - p2.Position.X, 2) +
                Math.Pow(p1.Position.Y - p2.Position.Y, 2) +
                Math.Pow(p1.Position.Z - p2.Position.Z, 2));
        }

        public static float LengthFloat(DepthSpacePoint p1, DepthSpacePoint p2)
        {
            return (float)Math.Sqrt(
                Math.Pow(p1.X - p2.X, 2) +
                Math.Pow(p1.Y - p2.Y, 2));
        }

        public static double Length(Joint p1, Joint p2)
        {
            return Math.Sqrt(
                Math.Pow(p1.Position.X - p2.Position.X, 2) +
                Math.Pow(p1.Position.Y - p2.Position.Y, 2) +
                Math.Pow(p1.Position.Z - p2.Position.Z, 2));
        }

        public static double Length(DepthSpacePoint p1, DepthSpacePoint p2)
        {
            return Math.Sqrt(
                Math.Pow(p1.X - p2.X, 2) +
                Math.Pow(p1.Y - p2.Y, 2));
        }
    }
}
