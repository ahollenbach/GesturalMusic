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
        private string host = "127.0.0.1";


        public LooperOSC(UdpWriter osc1)
        {
            //this.name = 
            Console.WriteLine("Looper constructor");
            oscLoop = osc1;

        }

        public void record()
        {
            Console.WriteLine("Recording"); 
            OscElement o = new OscElement("Looper/0/State", "Record");
            oscLoop.Send(o);
            Console.WriteLine("msg sent maybe??");
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

        public bool LooperControl(Body body)
        {
            try
            {
                if ((body.HandRightState == HandState.Lasso) && (body.HandLeftState == HandState.Closed))
                    if (body.Joints[JointType.AnkleRight].Position.Z > body.Joints[JointType.SpineBase].Position.Z)
                    {
                        this.record();
                        Console.WriteLine("Activated");
                        return true;
                    }
                    else if (body.Joints[JointType.AnkleRight].Position.Z < body.Joints[JointType.SpineBase].Position.Z)
                    {
                        this.overdub();
                        return true;
                    }
                    else if (body.Joints[JointType.AnkleLeft].Position.Z < body.Joints[JointType.SpineBase].Position.Z)
                    {
                        this.play();
                        return true;
                    }
                    else if (body.Joints[JointType.AnkleLeft].Position.Z > body.Joints[JointType.SpineBase].Position.Z)
                    {
                        this.stop();
                        return true;
                    }
                    else
                        return false;
                else//(undo clear gestures??)
                    return false;
            }
            catch (Exception e)
            {
                return false;
            }



        }
    }
}