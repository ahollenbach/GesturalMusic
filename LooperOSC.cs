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
        private bool isRecd = false;
        private bool isOvdb = false;
        private bool isPlay = false;
        private bool isStop = false;
        //private bool isClear = false;
        //private bool isUndo = false;
        private string name;
        private static UdpWriter oscLoop;
        private double barrier = 0.2;

        public LooperOSC()
        {
            Console.WriteLine("Looper constructor");
            ResetOsc();

        }

        public static void ResetOsc() 
        {
            oscLoop = new UdpWriter(MainWindow.oscHost, MainWindow.looperOscPort);
        }

        public void record()
        {
            if (!isRecd)
            {
                Console.WriteLine("Recording");
                OscElement o = new OscElement("/Looper/0/State", "Record");
                oscLoop.Send(o);
                isRecd = true;
                isOvdb = false;
                isPlay = false;
                isStop = false;
            }
            else
                return;
            //Console.WriteLine("msg sent maybe??");
        }
       
        public void overdub()
        {
            if (!isOvdb)
            {
                OscElement o = new OscElement("/Looper/0/State", "Overdub");
                oscLoop.Send(o);
                isRecd = false;
                isOvdb = true;
                isPlay = false;
                isStop = false;
            }
            else
                return;
        }


        public void play()
        {
            if (!isPlay)
            {
                OscElement o = new OscElement("/Looper/0/State", "Play");
                oscLoop.Send(o);
                isRecd = false;
                isOvdb = false;
                isPlay = true;
                isStop = false;
            }
        }

        public void stop()
        {
            if (!isStop)
            {
                OscElement o = new OscElement("/Looper/0/State", "Stop");
                oscLoop.Send(o);
                isRecd = false;
                isOvdb = false;
                isPlay = false;
                isStop = true;
            }
        }

        public void clear()
        {
            /*
            OscElement o = new OscElement("/Looper/0/State", "Record");
            oscLoop.Send(o);
            isRecd = false;
            isOvdb = false;
            isPlay = false;
            isStop = false;
            o = new OscElement("/Looper/0/State", "Stop");
            oscLoop.Send(o);
             */
        }
        public bool LooperControl(Body body)
        {
            try
            {
                if ((body.HandRightState == HandState.Lasso && body.HandLeftState == HandState.Open) &&
                    (body.Joints[JointType.WristRight].Position.Y < body.Joints[JointType.SpineShoulder].Position.Y))
                    if ((body.Joints[JointType.AnkleRight].Position.Z > body.Joints[JointType.SpineBase].Position.Z + barrier) && 
                        (Math.Abs(body.Joints[JointType.AnkleRight].Position.Z - body.Joints[JointType.SpineBase].Position.Z) > 
                        Math.Abs(body.Joints[JointType.AnkleLeft].Position.Z - body.Joints[JointType.SpineBase].Position.Z)))
                    {
                        this.record();
                        return true;
                    }
                    else if ((body.Joints[JointType.AnkleRight].Position.Z < body.Joints[JointType.SpineBase].Position.Z + barrier) &&
                        (Math.Abs(body.Joints[JointType.AnkleRight].Position.Z - body.Joints[JointType.SpineBase].Position.Z) >
                        Math.Abs(body.Joints[JointType.AnkleLeft].Position.Z - body.Joints[JointType.SpineBase].Position.Z)))
                    {
                        this.overdub();
                        return true;
                    }
                    else if ((body.Joints[JointType.AnkleLeft].Position.Z < body.Joints[JointType.SpineBase].Position.Z + barrier) &&
                        (Math.Abs(body.Joints[JointType.AnkleRight].Position.Z - body.Joints[JointType.SpineBase].Position.Z) <
                        Math.Abs(body.Joints[JointType.AnkleLeft].Position.Z - body.Joints[JointType.SpineBase].Position.Z)))
                    {
                        this.play();
                        return true;
                    }
                    else if ((body.Joints[JointType.AnkleLeft].Position.Z > body.Joints[JointType.SpineBase].Position.Z + barrier) &&
                        (Math.Abs(body.Joints[JointType.AnkleRight].Position.Z - body.Joints[JointType.SpineBase].Position.Z) <
                        Math.Abs(body.Joints[JointType.AnkleLeft].Position.Z - body.Joints[JointType.SpineBase].Position.Z)))
                    {
                        this.stop();
                        return true;
                    }
                    else
                        return false;
                else if ((body.HandRightState == HandState.Lasso && body.HandLeftState == HandState.Open) &&
                    (body.Joints[JointType.WristRight].Position.Y > body.Joints[JointType.SpineShoulder].Position.Y))
                {
                    Console.WriteLine("Undo/Clear");

                }
                    return false;
            }
            catch (Exception e)
            {
                return false;
            }

        }

   }
 }
