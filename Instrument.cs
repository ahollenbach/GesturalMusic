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
        // for different types
        public readonly static String PAD        = "Pad";
        public readonly static String INSTRUMENT = "Instrument";


        public string name;
        private Filter filter;

        // For rate limiting
        private DateTime lastNotePlayed;
        private static TimeSpan rateLimit = new TimeSpan(0, 0, 0, 0, 30);

        // For hand closed state (playing state)
        HandState handStateLast;
        int lastSemitone;

        public Instrument(string name)
        {
            this.name = name;
            filter = new Filter(this.name);

            lastNotePlayed = DateTime.Now;
            handStateLast = HandState.Unknown;
            lastSemitone = -1;
        }

        public String GetInstrumentType()
        {
            if (this.name.Contains("pad"))
            {
                return Instrument.PAD;
            }

            return Instrument.INSTRUMENT;
        }

        public void PlayNote(float pitch, int octave)
        {
            if (lastNotePlayed + rateLimit <= DateTime.Now)
            {
                Console.WriteLine("Playing: " + this.name + " " + pitch + " " + octave);
                OscElement pitchElem = new OscElement("/" + this.name + "/pitch", pitch);
                OscElement octaveElem = new OscElement("/" + this.name + "/octave", octave);
                MainWindow.osc.Send(pitchElem);
                MainWindow.osc.Send(octaveElem);

                

                lastNotePlayed = DateTime.Now;
            }
        }
        public bool CheckAndPlayNote(Body body)
        {
            // first thing, send filters
            filter.SendFilterData(body);

            double rightThreshold = 0.2;
            double leftThreshold = 0.1;

            // We're trying to play a MIDI instrument
            if (body.HandLeftState == HandState.Open)
            {
                //////////////////////////////////////////////////////////////
                // Semitone
                //////////////////////////////////////////////////////////////

                double leftMax = body.Joints[JointType.ShoulderLeft].Position.X - MainWindow.armLength + MainWindow.armLength * rightThreshold;
                double rightMax = body.Joints[JointType.ShoulderLeft].Position.X - MainWindow.armLength * leftThreshold;

                // Base our measurements off the wrist location
                double pos = body.Joints[JointType.WristLeft].Position.X;

                float percentage = (float)Utils.Clamp(0,1,(float)((pos - leftMax) / (rightMax - leftMax)));


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
                this.PlayNote(percentage, octave);

                return true;
            }
            return false;
        }
    }
}
