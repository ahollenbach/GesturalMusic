using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace GesturalMusic
{
    class FlatColors
    {
        public readonly static SolidColorBrush DARK_BLUEGRAY = new SolidColorBrush(Color.FromRgb(44, 62, 80));
        public readonly static SolidColorBrush WHITE = new SolidColorBrush(Color.FromRgb(236, 240, 241));
        public readonly static SolidColorBrush LIGHT_GRAY = new SolidColorBrush(Color.FromRgb(149, 165, 166));
        public readonly static SolidColorBrush LIGHT_BLUE = new SolidColorBrush(Color.FromRgb(52, 152, 219));
        public readonly static SolidColorBrush LIGHT_RED = new SolidColorBrush(Color.FromRgb(231, 76, 60));
        public readonly static SolidColorBrush LIGHT_GREEN = new SolidColorBrush(Color.FromRgb(46, 204, 113));
        public readonly static SolidColorBrush YELLOW = new SolidColorBrush(Color.FromRgb(241, 196, 15));

        /**
         * Takes in a color brush and returns a slightly translucent version of it. 
         * By default, the decrement is 0.1, but can be anything between 0 and 1.
         * A decrement of 1 will result in a completely translucent color (and is pointless)
         */
        public static SolidColorBrush Translucent(SolidColorBrush b, double decrement = 0.1) { 
            SolidColorBrush newBrush = b;
            Console.WriteLine(newBrush.Opacity);
            newBrush.Opacity -= decrement;
            return newBrush;
        }
    }
}
