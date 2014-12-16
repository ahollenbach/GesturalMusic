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

        // For rate limiting
        private DateTime lastNotePlayed;
        private static TimeSpan rateLimit = new TimeSpan(0, 0, 0, 0, 30);

        // For hand closed state (playing state)
        HandState handStateLast;
        int lastSemitone;

        public Instrument(UdpWriter osc, string name)
        {
            this.name = name;
            this.osc = osc;

            lastNotePlayed = DateTime.Now;
            handStateLast = HandState.Unknown;
            lastSemitone = -1;
        }

        public void PlayNote(int pitch, int velocity = 127, int duration = 500, int midiChannel = 1)
        {
            if (lastNotePlayed + rateLimit <= DateTime.Now)
            {
                Console.WriteLine("Playing: " + this.name + " " + pitch + " " + velocity + " " + duration + " " + midiChannel);
                OscElement elem = new OscElement("/" + this.name, pitch, velocity, duration, midiChannel);
                this.osc.Send(elem);

                lastNotePlayed = DateTime.Now;
            }
        }
        public bool CheckAndPlayNote(Body body)
        {
            double rightThreshold = 0.2;
            double leftThreshold = 0.1;

            // We're trying to play a MIDI instrument
            if (body.HandRightState == HandState.Closed)
            {
                //////////////////////////////////////////////////////////////
                // Semitone
                //////////////////////////////////////////////////////////////

                double leftMax = body.Joints[JointType.ShoulderLeft].Position.X - MainWindow.armLength + MainWindow.armLength * rightThreshold;
                double rightMax = body.Joints[JointType.ShoulderLeft].Position.X - MainWindow.armLength * leftThreshold;

                // Base our measurements off the wrist location
                double pos = body.Joints[JointType.WristLeft].Position.X;

                double percentage = (pos - leftMax) / (rightMax - leftMax);

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
                if (body.HandRightState == handStateLast && semitone == lastSemitone)
                {
                    // for now, only play notes if the previous state was not this one.
                    return false;
                }
                handStateLast = body.HandRightState;
                lastSemitone = semitone;

                int pitch = octave + semitone;

                // Send a note
                this.PlayNote(pitch);

                return true;
            }
            handStateLast = body.HandRightState;
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
                // Console.WriteLine("Left hand not open or closed.");
                throw new Exception();
            }

            for (int i = semitoneRanges.Count-1; i >= 0; i--)
            {
                if (percentage > semitoneRanges[i].Item1)
                {
                    return semitoneRanges[i].Item2;
                }
            }
            return semitoneRanges[0].Item2;
        }
    }
}
