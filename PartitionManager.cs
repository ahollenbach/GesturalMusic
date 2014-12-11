using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GesturalMusic
{
    enum PartitionType
    {
        Single,
        DoubleLeftRight,
        DoubleFrontBack,
        Quad
    }

    static class PartitionManager
    {
        static PartitionType currentPartitionType;

        static bool singleSet;

        static bool doubleLeftSet, doubleRightSet;

        static bool doubleFrontSet, doubleBackSet;

        static bool quad0Set, quad1Set, quad2Set, quad3Set;

        /// <summary>
        /// Get the current partition in which the first tracked body resides in
        /// according to the partition type set
        /// </summary>
        /// <param name="spineMidPos"></param>
        /// <returns></returns>
        public static int GetPartition(CameraSpacePoint spineMidPos)
        {
            if(currentPartitionType == PartitionType.Single)
            {
                return 0;
            }
            else if(currentPartitionType == PartitionType.DoubleLeftRight)
            {
                if (spineMidPos.X > 0)
                {
                    return 1;
                }
                return 0;
            }
            else if (currentPartitionType == PartitionType.DoubleFrontBack)
            {
                if (spineMidPos.Z > KinectStageArea.GetCenterZ())
                {
                    return 1;
                }
                return 0;
            }
            else
            {
                // set quad partition
                //   3   |  2
                //  -----------
                //   1   |  0
                //    kinect
                int partition = 0;
                if (spineMidPos.X > 0)
                {
                    partition = 1;
                }
                if (spineMidPos.Z > KinectStageArea.GetCenterZ())
                {
                    partition += 2; // add 2 to make it the back partition
                }
                return partition;
            }
        }

        /// <summary>
        /// Set the type of partitions for the current object
        /// </summary>
        /// <param name="type"></param>
        public static void SetPartitionType(PartitionType type)
        {
            currentPartitionType = type;
        }


       /* /// <summary>
        /// Get the type of current partition denoted by an integer
        /// </summary>
        /// <returns></returns>
        public static int getCurrentPartitionType()
        {
            // Default value is for Single partition (i.e. No partition)
            if (currentPartitionType == PartitionType.DoubleLeftRight)      return 2;
            else if (currentPartitionType == PartitionType.DoubleFrontBack) return 3;
            else if (currentPartitionType == PartitionType.Quad)            return 4;
            else                                                            return 1;
        }*/

        /// <summary>
        /// Get a boolean value indicating whether the current partition has been set or not
        /// </summary>
        /// <returns></returns>
        public static bool getDoubleLeftSet() { return doubleLeftSet; }
        public static bool getDoubleRightSet() { return doubleRightSet;  }
        public static bool getDoubleFrontSet() { return doubleFrontSet; }
        public static bool getDoubleBackSet() { return doubleBackSet; }
        public static bool getQuad0Set() { return quad0Set; }
        public static bool getQuad1Set() { return quad1Set; }
        public static bool getQuad2Set() { return quad2Set; }
        public static bool getQuad3Set() { return quad3Set; }

    }
}
