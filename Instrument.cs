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
            // We're trying to play a MIDI instrument
            if (body.HandRightState == HandState.Closed)
            {
                float armLength = Utils.Length(body.Joints[JointType.ShoulderLeft], body.Joints[JointType.ElbowLeft]) +
                                  Utils.Length(body.Joints[JointType.ElbowLeft], body.Joints[JointType.WristLeft]);

                float min = body.Joints[JointType.ShoulderLeft].Position.X - armLength;
                float max = body.Joints[JointType.ShoulderLeft].Position.X;
                // Base our measurements off the wrist location
                float pos = body.Joints[JointType.WristLeft].Position.X;

                // Clamp the pitch to only be 0.8 of the full extension of the arm (helps with lower and upper octaves)
                float pitch = 1 - Utils.Clamp(0f, 1f, 1 - (pos - min) / (max - min));

                // baseOctave is the lowest octave - so, if in C, we want our lowest note to be C3 (0-based)
                // userOctave can add to baseOctave, so our octaves available start at C3 (lower), C4 (slightly down), and C5 (above shoulder)
                float baseOctave = 4;
                float userOctave = 0;
                if (body.Joints[JointType.WristLeft].Position.Y > body.Joints[JointType.SpineShoulder].Position.Y)
                {
                    userOctave = 2;
                }
                else if (body.Joints[JointType.WristLeft].Position.Y > body.Joints[JointType.SpineMid].Position.Y)
                {
                    userOctave = 1;
                }

                // Scale octave to 12 semitones per octave
                float octave = (userOctave + baseOctave) * 12;

                // Send a note
                this.PlayNote(pitch, 0.5f, octave, body.HandLeftState == HandState.Open ? "white" : "black");
            }
        }
    }
}
