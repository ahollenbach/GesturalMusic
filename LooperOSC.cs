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
        private bool isRecording = false;
        private bool isOverdubbing = false;
        private bool isPlaying = false;
        private bool isStopped = false;

        private static UdpWriter oscLoop;
        private double barrier = 0.2;

        public LooperOSC()
        {
            Console.WriteLine("Starting looper...");
            ResetOsc();
        }

        public static void ResetOsc()
        {
            oscLoop = new UdpWriter(MainWindow.oscHost, MainWindow.looperOscPort);
        }

        public void record()
        {
            if (!isRecording)
            {
                OscElement o = new OscElement("/Looper/0/State", "Record");
                oscLoop.Send(o);
                isRecording = true;
                isOverdubbing = false;
                isPlaying = false;
                isStopped = false;
            }
            else
                return;
        }

        public void overdub()
        {
            if (!isOverdubbing)
            {
                OscElement o = new OscElement("/Looper/0/State", "Overdub");
                oscLoop.Send(o);
                isRecording = false;
                isOverdubbing = true;
                isPlaying = false;
                isStopped = false;
            }
            else
                return;
        }


        public void play()
        {
            if (!isPlaying)
            {
                OscElement o = new OscElement("/Looper/0/State", "Play");
                oscLoop.Send(o);
                isRecording = false;
                isOverdubbing = false;
                isPlaying = true;
                isStopped = false;
            }
        }

        public void stop()
        {
            if (!isStopped)
            {
                OscElement o = new OscElement("/Looper/0/State", "Stop");
                oscLoop.Send(o);
                isRecording = false;
                isOverdubbing = false;
                isPlaying = false;
                isStopped = true;
            }
        }

        /// <summary>
        /// Determines if looper data should be sent, given the body skeleton.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public bool SendLooperData(Body body)
        {
            try
            {
                if ((body.HandRightState == HandState.Lasso && body.HandLeftState == HandState.Open) &&
                    (body.Joints[JointType.WristRight].Position.Y < body.Joints[JointType.SpineShoulder].Position.Y))
                {
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
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }
    }
}
