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

    // facing the Kinect
    enum Partition
    {
        Left,
        Right,
        Front,
        Back
    }
}
