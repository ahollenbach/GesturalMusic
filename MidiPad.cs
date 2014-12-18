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
        private string name;

        private AbletonSliderController pitch;
        private AbletonSliderController velocity;
        private AbletonSwitchController noteOn;

        private DateTime lastNotePlayed;
        private static TimeSpan rateLimit = new TimeSpan(0, 0, 0, 0, 10);

        // Velocity tracker
        private double lWristVelocity;
        private double rWristVelocity;
        private Joint lWristLocationLast;
        private Joint rWristLocationLast;
        private DateTime lastFrame;

        public MidiPad(string name) : base(name)
        {
            
            this.name = name;

            pitch = new AbletonSliderController(MainWindow.osc, this.name + "/pitch", 0, 127, false);
            velocity = new AbletonSliderController(MainWindow.osc, this.name + "/velocity", 0, 127, false);
            noteOn = new AbletonSwitchController(MainWindow.osc, this.name + "/noteOn");

            lastNotePlayed = DateTime.Now;

            lWristVelocity = 0f;
            rWristVelocity = 0f;
        }

        public void PlayNote(double pitchVal, double velocityVal, double octaveVal) {
            Console.WriteLine("sending " + pitchVal);
            // rate limit as to note overwhelm Ableton
            if (lastNotePlayed + rateLimit <= DateTime.Now)
            {
                pitch.Send((float)pitchVal);
                velocity.Send((float)velocityVal);
                noteOn.SwitchOn(true);

                lastNotePlayed = DateTime.Now;
            }
        }
        new public void StopNote()
        {
            noteOn.SwitchOff();
        }

        new public bool CheckAndPlayNote(Body body)
        {
            double threshold = 0.85 * MainWindow.armLength;         // Don't even look for a hit unless more than threshold out
            double xThreshold = 0.25 * MainWindow.armLength;         // Don't even look for a hit unless more than threshold out in X

            // Set locations if first time
            if (lWristLocationLast == null || rWristLocationLast == null)
            {
                lWristLocationLast = body.Joints[JointType.WristLeft];
                rWristLocationLast = body.Joints[JointType.WristRight];
                lastFrame = DateTime.Now;

                return false;
            }

            // Set velocities
            TimeSpan dt = DateTime.Now - lastFrame;
            lastFrame = DateTime.Now;

            // left hand
            Joint lWrist  = body.Joints[JointType.WristLeft];
            // TODO switch back to length
            double dLWristPos = (lWrist.Position.X < lWristLocationLast.Position.X ? -1 : 1) * (double)Utils.LengthFloat(lWrist, lWristLocationLast);
            double newLWristVelocity = dLWristPos/dt.TotalMilliseconds;

            // If change in direction, and we're at least halfway extended
            if (newLWristVelocity > 0 && lWristVelocity < 0 && (double)Utils.LengthFloat(body.Joints[JointType.WristLeft], body.Joints[JointType.ShoulderLeft]) > threshold && Math.Abs(lWrist.Position.X - body.Joints[JointType.ShoulderLeft].Position.X) > xThreshold)
            {
                // Calculate which pad we're hitting
                //  5  O  2 
                //  4--|--1
                //  3 /\  0
                double pad = 0f;
                if (lWrist.Position.Y > body.Joints[JointType.ShoulderLeft].Position.Y)
                {
                    pad = 2;
                }
                else if (lWrist.Position.Y > body.Joints[JointType.SpineBase].Position.Y + 0.4 * (double)Utils.LengthFloat(body.Joints[JointType.SpineMid], body.Joints[JointType.SpineBase]))
                {
                    pad = 1;
                }

                this.PlayNote(pad+36, 0.5f, 0f);
                return true;
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
                double pad = 3f;
                if (rWrist.Position.Y > body.Joints[JointType.ShoulderRight].Position.Y)
                {
                    pad = 5;
                }
                else if (rWrist.Position.Y > body.Joints[JointType.SpineMid].Position.Y)
                {
                    pad = 4;
                }

                this.PlayNote(pad, 0.5f, 0f);
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
