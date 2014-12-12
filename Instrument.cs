using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ventuz.OSC;

namespace GesturalMusic
{
    class Instrument
    {
        private string name;
        private UdpWriter osc;

        public Instrument(UdpWriter osc, string name)
        {
            this.name = name;
            this.osc = osc;
        }

        public void PlayNote(int pitch, int velocity = 50, int duration = 300, int midiChannel = 1)
        {
            OscElement elem = new OscElement("/" + this.name, pitch, velocity, duration, midiChannel);
            this.osc.Send(elem);
        }
        public void CheckAndPlayNote(Body body)
        {
            double minThreshold = 0.1;
            double maxThreshold = 0.2;

            // We're trying to play a MIDI instrument
            if (body.HandRightState == HandState.Closed)
            {
                double armLength = Utils.Length(body.Joints[JointType.ShoulderLeft], body.Joints[JointType.ElbowLeft]) +
                                  Utils.Length(body.Joints[JointType.ElbowLeft], body.Joints[JointType.WristLeft]);

                double min = body.Joints[JointType.ShoulderLeft].Position.X - armLength;
                double max = body.Joints[JointType.ShoulderLeft].Position.X;

                // Base our measurements off the wrist location
                double pos = body.Joints[JointType.WristLeft].Position.X;

                // Clamp the pitch to only be 0.8 of the full extension of the arm (helps with lower and upper octaves)
                int semitone = 1 - (int)Utils.Clamp(0f, 1f, (float)(1 - (pos - min) / (max - min)));

                // baseOctave is the lowest octave - so, if in C, we want our lowest note to be C3 (0-based)
                // userOctave can add to baseOctave, so our octaves available start at C3 (lower), C4 (slightly down), and C5 (above shoulder)
                int baseOctave = 4;
                int userOctave = 0;
                if (body.Joints[JointType.WristLeft].Position.Y > body.Joints[JointType.SpineShoulder].Position.Y)
                {
                    userOctave = 2;
                }
                else if (body.Joints[JointType.WristLeft].Position.Y > body.Joints[JointType.SpineMid].Position.Y)
                {
                    userOctave = 1;
                }
                else if (body.Joints[JointType.WristLeft].Position.Y > body.Joints[JointType.SpineBase].Position.Y)
                {
                    userOctave = 0;
                }
                else
                {
                    return;
                }

                // Scale octave to 12 semitones per octave
                int octave = (userOctave + baseOctave) * 12;

                int pitch = octave + semitone;

                // Send a note
                this.PlayNote(pitch,2,3,"df");
            }
        }
    }
}
