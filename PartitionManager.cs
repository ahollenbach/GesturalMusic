using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Samples.Kinect.BodyBasics
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

    // facing the Kinect
    enum Partition
    {
        FrontLeft,
        FrontRight,
        BackLeft,
        BackRight
    }
}
