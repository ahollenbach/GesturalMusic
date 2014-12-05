using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ventuz.OSC;

namespace GesturalMusic
{
    /// <summary>
    /// AbletonSwitchController is for setting switches in Ableton (i.e. play/pause). This is for values 
    /// that can be turned on and off.
    /// </summary>
    class AbletonSwitchController : AbletonController
    {
        /// <summary>
        /// Creates a new Ableton switch controller.
        /// </summary>
        /// <param name="osc">The OSC UDP writer</param>
        /// <param name="name">The name of the controller</param>
        public AbletonSwitchController(UdpWriter osc, string name)
        {
            // Every controller should set these two values
            this.controlName = "/" + name;
            this.osc = osc;
        }

        /// <summary>
        /// Sends a signal with the given value.
        /// </summary>
        /// <param name="value">The value to send. Either 0 or 1.</param>
        public override void Send(float value)
        {
            Console.WriteLine(this.controlName + ": " + value);
            OscElement elem = new OscElement(controlName, value);
            osc.Send(elem);
        }

        /// <summary>
        /// Sends a switch on (1) signal to Ableton.
        /// </summary>
        public void SwitchOn()
        {
            Send(1.0f);
        }

        /// <summary>
        /// Sends a switch on (1) signal to Ableton.
        /// </summary>
        public void SwitchOn(bool tru)
        {
            Random r = new Random();
            Send((float) (r.NextDouble() + 0.000001));
        }

        /// <summary>
        /// Sends a switch off (0) signal to Ableton.
        /// </summary>
        public void SwitchOff()
        {
            Send(0.0f);
        }
    }
}
