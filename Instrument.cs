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
        public bool CheckAndPlayNote(Body body)
        {
            double minThreshold = 0.2;
            double maxThreshold = 0.1;

            Console.WriteLine(body.HandRightState);

            // We're trying to play a MIDI instrument
            if (body.HandRightState == HandState.Closed)
            {
                //////////////////////////////////////////////////////////////
                // Semitone
                //////////////////////////////////////////////////////////////

                double armLength = Utils.Length(body.Joints[JointType.ShoulderLeft], body.Joints[JointType.ElbowLeft]) +
                                  Utils.Length(body.Joints[JointType.ElbowLeft], body.Joints[JointType.WristLeft]);

                double min = body.Joints[JointType.ShoulderLeft].Position.X - armLength + armLength * minThreshold;
                double max = body.Joints[JointType.ShoulderLeft].Position.X - armLength * maxThreshold;

                // Base our measurements off the wrist location
                double pos = body.Joints[JointType.WristLeft].Position.X;

                double percentage = (pos - min) / (max - min);
                Console.WriteLine("Percentage " + percentage);

                int semitone = 0;
                try
                {
                    semitone = getSemitone(percentage, body.HandLeftState);
                    if (semitone == -1)
                    {
                        // This is the E# (the one black key that we don't want to play)
                        return false;
                    }
                }
                catch (Exception e)
                {
                    // Problem getting the note (maybe the hand state wasn't good?), so do nothing
                    return false;
                }


                //////////////////////////////////////////////////////////////
                // Octave
                //////////////////////////////////////////////////////////////

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
                    return false;
                }

                // Scale octave to 12 semitones per octave
                int octave = (userOctave + baseOctave) * 12;


                //////////////////////////////////////////////////////////////
                // Send
                //////////////////////////////////////////////////////////////

                int pitch = octave + semitone;

                // Send a note
                this.PlayNote(pitch);

                return true;
            }
            return false;
        }

        private int getSemitone(double percentage, HandState handState)
        {
            List<Tuple<double, int>> semitoneRanges;

            if (handState == HandState.Closed)
            {
                // black semitones (minimums)
                semitoneRanges = new List<Tuple<double, int>>
                {
                    Tuple.Create(0.0  ,  1),
                    Tuple.Create(1.5/7,  3),
                    Tuple.Create(2.5/7, -1),
                    Tuple.Create(3.5/7,  6),
                    Tuple.Create(4.5/7,  8),
                    Tuple.Create(5.5/7, 10)
                };
            }
            else if (handState == HandState.Open)
            {
                // white semitones (minimums)
                semitoneRanges = new List<Tuple<double, int>>
                {
                    Tuple.Create(0.0  ,  0),
                    Tuple.Create(1.0/7,  2),
                    Tuple.Create(2.0/7,  4),
                    Tuple.Create(3.0/7,  5),
                    Tuple.Create(4.0/7,  7),
                    Tuple.Create(5.0/7,  9),
                    Tuple.Create(6.0/7, 11)
                };
            }
            else
            {
                throw new Exception();
            }

            for (int i = semitoneRanges.Count; i >= 0; i--)
            {
                if (percentage > semitoneRanges[i].Item1)
                {
                    return semitoneRanges[i].Item2;
                }
            }

            throw new Exception();
        }
    }
}
