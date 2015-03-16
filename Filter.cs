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

            // .13 -> .4 ( roughly .25)
            // .25 ( .13/.25 ~= 0.5)

            // .88 / .58 / -.15   (.3, .75)
            Console.WriteLine(Math.Pow(1 + head.Y - 3 * forearmLength + yDist, .3));
            MainWindow.osc.Send(new OscElement("/" + instrumentName + "/filterX", (float) ((xDist - .5 * forearmLength) * 4)));
            MainWindow.osc.Send(new OscElement("/" + instrumentName + "/filterY", (float) Math.Pow(1 + head.Y - 3 * forearmLength + yDist, 0.3)));
            MainWindow.osc.Send(new OscElement("/" + instrumentName + "/filterZ", rShoulder.Z - rWrist.Z));
        }
    }
}
