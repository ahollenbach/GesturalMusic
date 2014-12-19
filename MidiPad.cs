using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ventuz.OSC;

namespace GesturalMusic
{
    class MidiPad : Instrument
    {
        public string name;

        private DateTime lastTimeLPadPlayed;
        private DateTime lastTimeRPadPlayed;
        private int lastLPadPlayed;
        private int lastRPadPlayed;

        private static TimeSpan rateLimit  = new TimeSpan(0, 0, 0, 0, 10);
        private static TimeSpan reHitLimit = new TimeSpan(0, 0, 0, 0, 200);

        // Velocity tracker
        private double lWristVelocity;
        private double rWristVelocity;
        private Joint lWristLocationLast;
        private Joint rWristLocationLast;
        private DateTime lastFrame;

        public MidiPad(string name) : base(name)
        {
            this.name = name;

            lastTimeLPadPlayed = DateTime.Now;
            lastTimeRPadPlayed = DateTime.Now;

            lWristVelocity = 0f;
            rWristVelocity = 0f;
        }

        new public void PlayNote(int pitch, int velocity = 127, int duration = 5000, int midiChannel = 1)
        {
            Console.WriteLine("Playing: " + this.name + " " + pitch + " " + velocity + " " + duration + " " + midiChannel);
            OscElement elem = new OscElement("/" + this.name, pitch, velocity, duration, midiChannel);
            MainWindow.osc.Send(elem);
        }

        new public void StopNote()
        {
            //noteOn.SwitchOff();
        }

        new public bool CheckAndPlayNote(Body body)
        {
            double threshold = 0.85 * MainWindow.armLength;         // Don't even look for a hit unless more than threshold out
            double xThreshold = 0.4 * MainWindow.armLength;         // Don't even look for a hit unless more than threshold out in X

            // Set locations if first time
            if (lWristLocationLast == null || rWristLocationLast == null)
            {
                lWristLocationLast = body.Joints[JointType.WristLeft];
                rWristLocationLast = body.Joints[JointType.WristRight];
                lastFrame = DateTime.Now;
                lastLPadPlayed = -1;
                lastRPadPlayed = -1;

                return false;
            }

            // Set velocities
            TimeSpan dt = DateTime.Now - lastFrame;
            lastFrame = DateTime.Now;



            // left hand
            Joint lWrist  = body.Joints[JointType.WristLeft];

            double dLWristPos = (lWrist.Position.X < lWristLocationLast.Position.X ? -1 : 1) * Utils.Length(lWrist, lWristLocationLast);
            double newLWristVelocity = dLWristPos/dt.TotalMilliseconds;

            // If change in direction, and we're at least halfway extended in x and at least .85 extended overall
            if (newLWristVelocity > 0 && lWristVelocity < 0 && Utils.Length(body.Joints[JointType.WristLeft], body.Joints[JointType.ShoulderLeft]) > threshold && Math.Abs(lWrist.Position.X - body.Joints[JointType.ShoulderLeft].Position.X) > xThreshold)
            {
                // Calculate which pad we're hitting
                //  5  O  2 
                //  4--|--1
                //  3 /\  0
                int pad = 0;
                if (lWrist.Position.Y > body.Joints[JointType.ShoulderLeft].Position.Y)
                {
                    pad = 2;
                }
                else if (lWrist.Position.Y > (body.Joints[JointType.SpineBase].Position.Y + 0.4 * Utils.Length(body.Joints[JointType.SpineMid], body.Joints[JointType.SpineBase])))
                {
                    pad = 1;
                }
                if (lastLPadPlayed != pad || lastTimeLPadPlayed + reHitLimit <= DateTime.Now)
                {
                    this.PlayNote(36 + pad);
                    lastTimeLPadPlayed = DateTime.Now;
                    lastLPadPlayed = pad;
                }
            }


            // right hand
            Joint rWrist = body.Joints[JointType.WristRight];
            double dRWristPos = (double)Utils.LengthFloat(rWrist, rWristLocationLast);
            double newRWristVelocity = dRWristPos / dt.TotalMilliseconds;

            // If change in direction, and we're at least halfway extended
            if (newRWristVelocity < 0 && rWristVelocity > 0 && body.Joints[JointType.WristRight].Position.X > threshold)
            {
                // Calculate which pad we're hitting
                //  5  O  2 
                //  4--|--1
                //  3 /\  0
                int pad = 3;
                if (rWrist.Position.Y > body.Joints[JointType.ShoulderRight].Position.Y)
                {
                    pad = 5;
                }
                else if (rWrist.Position.Y > body.Joints[JointType.SpineMid].Position.Y)
                {
                    pad = 4;
                }

                this.PlayNote(pad);
            }

            // Update velocities
            lWristVelocity = newLWristVelocity;
            rWristVelocity = newRWristVelocity;
            lWristLocationLast = lWrist;
            rWristLocationLast = rWrist;

            // Do we need to send a StopNote?
            return false;
        }
    }
}
