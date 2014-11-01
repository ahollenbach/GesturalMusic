using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GesturalMusic
{
    class KinectStageArea
    {
        // All measurements relative to facing the Kinect.
        public static class FRONT_RIGHT 
        {
            public static double X = 1.02;
            public static double Z = 1.84;
        }

        public static class FRONT_LEFT 
        {
            public static double X = -1.02;
            public static double Z = 1.84;
        }

        public static class BACK_LEFT 
        {
            public static double X = 1.76;
            public static double Z = 3.77;
        }

        public static class BACK_RIGHT 
        {
            public static double X = -1.76;
            public static double Z = 3.77;
        }

        public static double PADDING = 0.14;

        public static double GetCenterZ()
        {
            return FRONT_LEFT.Z + (BACK_LEFT.Z - FRONT_LEFT.Z) / 2;
        }
    }
}
