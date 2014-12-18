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
        public static PartitionType currentPartitionType;

        // Store whether the partitions are set or not
        public static bool[] isPartitionSet = new bool[] {false, false, false, false};

        public static string val3 = "void";

        // Store which instruments the partitions are set with, if set.
        public static string[] partitionInstrSetName = new string[] { val3, val3, val3, val3 };

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
            isPartitionSet = new bool[] { false, false, false, false };
            partitionInstrSetName = new string[] { val3, val3, val3, val3 };
        }               
    }
}
