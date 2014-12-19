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

            // X works fairly well
            // Y is somewhat bad
            // Z is bad
            MainWindow.osc.Send(new OscElement("/" + instrumentName + "/filterX", rWrist.X - rShoulder.X));
            MainWindow.osc.Send(new OscElement("/" + instrumentName + "/filterY", rWrist.Y));
            MainWindow.osc.Send(new OscElement("/" + instrumentName + "/filterZ", rShoulder.Z - rWrist.Z));
        }
    }
}
