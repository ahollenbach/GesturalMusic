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

    class PartitionManager
    {
        public static Partition GetPartition()
        {
            return Partition.Left;
        }
    }

    // facing the Kinect
    enum Partition
    {
        Left,
        Right,
        Front,
        Back,
        FrontLeft,
        FrontRight,
        BackLeft,
        BackRight
    }
}
