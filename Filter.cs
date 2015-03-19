using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ventuz.OSC;

namespace GesturalMusic
{
    class Filter
    {
        private String instrumentName;

        /** 
         * Filters
         * 
         * Currently set up so that X,Y follow the wrist location relative to the body,
         * while Z tracks the full body relative to the Kinect.
         * */
        public Filter(String instrumentName)
        {
            this.instrumentName = instrumentName;
        }

        public void SendFilterData(Body body) 
        {
            CameraSpacePoint rWrist    = body.Joints[JointType.WristRight].Position;
            CameraSpacePoint rShoulder = body.Joints[JointType.ShoulderRight].Position;
            CameraSpacePoint head      = body.Joints[JointType.Head].Position;

            double forearmLength = Utils.Length(body.Joints[JointType.WristRight], body.Joints[JointType.ElbowRight]);
            double xDist = rWrist.X - rShoulder.X;
            double yDist = rWrist.Y - head.Y;

            float zLocation = Utils.Clamp(0,1,(float)((head.Z - 2) / 4.5));
            MainWindow.osc.Send(new OscElement("/" + instrumentName + "/filterX", (float) ((xDist - .5 * forearmLength) * 4)));
            MainWindow.osc.Send(new OscElement("/" + instrumentName + "/filterY", (float) Math.Pow(1 + head.Y - 3 * forearmLength + yDist, 0.3)));
            MainWindow.osc.Send(new OscElement("/" + instrumentName + "/filterZ", 1 - zLocation));
        }
    }
}
