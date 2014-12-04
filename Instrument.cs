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

        private AbletonSliderController octave;
        private AbletonSliderController pitch;
        private AbletonSliderController velocity;
        private AbletonSwitchController noteOn;

        private AbletonSliderController octaveBlack;
        private AbletonSliderController pitchBlack;
        private AbletonSliderController velocityBlack;
        private AbletonSwitchController noteOnBlack;

        private DateTime lastNotedPlayed;
        private TimeSpan lastDuration;
        private static TimeSpan rateLimit = new TimeSpan(0, 0, 0, 0, 100);

        private bool playing;

        public Instrument(UdpWriter osc, string name)
        {
            this.name = name;
            this.osc = osc;

            octave = new AbletonSliderController(osc, this.name + "/octave/white", 0, 127, false);
            pitch = new AbletonSliderController(osc, this.name + "/pitch/white", 0, 127, false);
            velocity = new AbletonSliderController(osc, this.name + "/velocity/white", 0, 127, false);
            noteOn = new AbletonSwitchController(osc, this.name + "/noteOn/white");

            octaveBlack = new AbletonSliderController(osc, this.name + "/octave/black", 0, 127, false);
            pitchBlack = new AbletonSliderController(osc, this.name + "/pitch/black", 0, 127, false);
            velocityBlack = new AbletonSliderController(osc, this.name + "/velocity/black", 0, 127, false);
            noteOnBlack = new AbletonSwitchController(osc, this.name + "/noteOn/black");

            lastNotedPlayed = DateTime.Now;
            lastDuration = new TimeSpan(0);
            playing = false;
        }

        public void PlayNote(float pitchVal, float velocityVal, float octaveVal, string color) {
            // rate limit as to note overwhelm Ableton
            if (lastNotedPlayed + lastDuration <= DateTime.Now)
            {
                Console.WriteLine(lastNotedPlayed);
                if (color == "black")
                {
                    octaveBlack.Send(octaveVal);
                    pitchBlack.Send(pitchVal);
                    velocityBlack.Send(velocityVal);
                    noteOnBlack.SwitchOn();
                }
                else
                {
                    octave.Send(octaveVal);
                    pitch.Send(pitchVal);
                    velocity.Send(velocityVal);
                    noteOn.SwitchOn();
                }
                

                lastNotedPlayed = DateTime.Now;
                lastDuration = rateLimit; // TODO: Probably not necessary
                playing = true;
            }
        }
        public void StopNote()
        {
            if (playing)
            {
                noteOn.SwitchOff();
                noteOnBlack.SwitchOff();
            }
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
                float pitch = 1 - Utils.Clamp(0f, .8f, 1 - (pos - min) / (max - min));

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
            else if (body.HandRightState == HandState.Open)
            {
                this.StopNote();
            }
        }
    }
}
