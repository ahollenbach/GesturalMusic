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
                lastDuration = new TimeSpan(0,0,0,0,100);
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
