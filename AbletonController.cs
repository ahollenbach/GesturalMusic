using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ventuz.OSC;

namespace Microsoft.Samples.Kinect.BodyBasics
{
    /// <summary>
    /// AbletonController is an abstract parent class for all Ableton control classes. All classes
    /// inheriting from this one are used to send OSC signals to Ableton to control various 
    /// aspects of the music.
    /// </summary>
    abstract class AbletonController
    {
        /// <summary>
        /// The name of the element you are controlling. i.e. "volume".
        /// For more complex elements, you can subdivide as such: "loop/create" or "loop/delete"
        /// </summary>
        protected string controlName;

        /// <summary>
        /// The OSC UDP writer (sends OSC messages via the network).
        /// </summary>
        protected UdpWriter osc;

        /// <summary>
        /// Sends an OSC signal with the given value.
        /// </summary>
        /// <param name="value">The value to send.</param>
        public abstract void Send(float value);
    }
}
