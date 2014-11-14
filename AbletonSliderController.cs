using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ventuz.OSC;

namespace GesturalMusic
{
    /// <summary>
    /// AbletonSliderController is for sending slider-based controls to Ableton (i.e. volume).
    /// </summary>
    class AbletonSliderController : AbletonController
    {
        protected float min;
        protected float max;
        protected bool contiguous;

        /// <summary>
        /// Creates a new Ableton slider controller.
        /// </summary>
        /// <param name="osc">The OSC UDP writer</param>
        /// <param name="name">The name of the controller</param>
        /// <param name="min">The minimum value to send</param>
        /// <param name="max">The maximum value to send</param>
        /// <param name="contiguous">True if values should be contiguous. False if values should be discrete.</param>
        public AbletonSliderController(UdpWriter osc, string name, float min, float max, bool contiguous)
        {
            // Every controller should set these two values
            this.controlName = "/" + name;
            this.osc = osc;

            this.min = min;
            this.max = max;
            this.contiguous = contiguous;
        }

        /// <summary>
        /// Sends a signal with the given value.
        /// </summary>
        /// <param name="value">The value to send. Should be between 0 and 1. The controller will handle scaling.</param>
        public override void Send(float value)
        {
            //value = ScaleValue(value);

            // be double sure that it's right
            //value = Clamp(value);

            // convert it back to 0 to 1 for Ableton.
            //value = Scale0To1(value);
            
            // The reason we do this scale up, clamp, scale down is because
            // we want our 0 to 1 values to correspond to integer values in Ableton.
            // It's a little goofy and probably should be fixed in the Max Patch...

            Console.WriteLine(this.controlName + ": " + value);
            OscElement elem = new OscElement(controlName, value);
            osc.Send(elem);
        }

        /// <summary>
        /// Clamps the value between the min and max
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <returns>The value clamped. If the value is less than the minimum, it will return the minimum.
        /// If the value is greater than the maximum, it will return the maximum.</returns>
        private float Clamp(float value)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        private float ScaleValue(float value)
        {
            return value * this.max + this.min;
        }

        private float Scale0To1(float value)
        {
            // If not contiguous, convert all values to discrete integer steps
            if (!contiguous)
            {
                value = (float)Math.Floor(value);
            }
            return (value - min) / max;
        }
    }
}
