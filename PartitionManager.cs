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

        public static void SetPartitionType(PartitionType type)
        {
            currentPartitionType = type;
        }
    }
}
