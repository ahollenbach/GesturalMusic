using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ventuz.OSC;

namespace GesturalMusic
{ 
    class LooperOSC
    {
        private string name;
        private UdpWriter oscLoop;
        private int port = 22345;
        private string host = "129.21.212.195";


        public LooperOSC(UdpWriter osc, String name)
        {
            //this.name = 
            
            oscLoop = new UdpWriter(host, port);

        }

        public void record()
        {
            OscElement o = new OscElement("Looper/0/State", "Record");
            oscLoop.Send(o);
        }
        public void overdub()
        {
            OscElement o = new OscElement("Looper/0/State", "Overdub");
            oscLoop.Send(o);
        }


        public void play()
        {
            OscElement o = new OscElement("Looper/0/State", "Play");
            oscLoop.Send(o);
        }

        public void stop()
        {
            OscElement o = new OscElement("Looper/0/State", "Stop");
            oscLoop.Send(o);
        }

        public void LooperControl(Body body)
        {
            if (body.HandRightState == HandState.Closed && body.HandLeftState == HandState.Closed && body.Joints[JointType.HandTipRight].Position.Y > body.Joints[JointType.Head].Position.Y && body.Joints[JointType.HandTipLeft].Position.Y > body.Joints[JointType.Head].Position.Y)
            {
                record(); 
            }
        }
    }
}