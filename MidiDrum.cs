using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ventuz.OSC;

namespace GesturalMusic
{
    class MidiDrum : Instrument
    {
        private string name;
        private UdpWriter osc;

        private AbletonSliderController pitch;
        private AbletonSliderController velocity;
        private AbletonSwitchController noteOn;

        private DateTime lastNotedPlayed;
        private static TimeSpan rateLimit = new TimeSpan(0, 0, 0, 0, 50);

        public MidiDrum(UdpWriter osc, string name)
        {
            
            this.name = name;
            this.osc = osc;

            pitch = new AbletonSliderController(osc, this.name + "/pitch", 0, 127, false);
            velocity = new AbletonSliderController(osc, this.name + "/velocity", 0, 127, false);
            noteOn = new AbletonSwitchController(osc, this.name + "/noteOn");

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
    }
}
