using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GesturalMusic
{
    /// <summary>
    /// Interaction logic for FloorWindow.xaml
    /// </summary>
    public partial class FloorWindow : Window
    {

        /// <summary>
        /// Constant for clamping Z values of camera space points from being negative
        /// </summary>
        private const float InferredZPositionClamp = 0.1f;

        /// <summary>
        /// Drawing group for body rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;

        /// <summary>
        /// Width of display (depth space)
        /// </summary>
        private int displayWidth;

        /// <summary>
        /// Height of display (depth space)
        /// </summary>
        private int displayHeight;

        private Floor floor;

        private Boolean trackingMouse;
        private Boolean trackingRightMouse;
        private int cornerSelected;

        public FloorWindow(int displayWidth, int displayHeight)
        {
            this.displayHeight = (int)Math.Round(this.Width);
            this.displayWidth = (int)Math.Round(this.Height);

            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            this.DataContext = this;

            this.InitializeComponent();

            // Initialize our floor
            floor = new Floor(100, 100, 400, 200);

            trackingMouse = false;
        }

        public void Draw(int curQuadrant, String[] instrNames)
        {
            this.displayHeight = (int)Math.Round(this.Width);
            this.displayWidth = (int)Math.Round(this.Height);

            using (DrawingContext dc = this.drawingGroup.Open())
            {
                // Draw a background to fill the space
                dc.DrawRectangle(FlatColors.MIDNIGHT_BLACK, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));

                // Get the latest floor information
                Path p = makePath(floor.points);
                //Console.WriteLine(floor.points[0] + " " + floor.points[1] + " " + floor.points[2] + " " + floor.points[3]);
                dc.DrawGeometry(FlatColors.LIGHT_GREEN, null, p.Data);

                // Draw current quadrant
                if (curQuadrant != -1)
                {
                    Path highlightedQuadrant = makePath(floor.GetQuadrantPoints(curQuadrant));
                    dc.DrawGeometry(FlatColors.LIGHT_RED, null, highlightedQuadrant.Data);
                }

                // Draw center marker
                dc.DrawRectangle(FlatColors.WHITE, null, new Rect(floor.centroid.X - 10, floor.centroid.Y - 1, 20, 2));
                dc.DrawRectangle(FlatColors.WHITE, null, new Rect(floor.centroid.X - 3, floor.centroid.Y - 4, 6, 8));

                // And the instrument to each partition
                if (curQuadrant != -1)
                {
                    dc.DrawText(new FormattedText(instrNames[0], CultureInfo.GetCultureInfo("en-us"),
                                                                    FlowDirection.RightToLeft,
                                                                    new Typeface(MainWindow.FONT_FAMILY),
                                                                    18, FlatColors.WHITE),
                                                                    new Point(floor.points[2].X - 10, floor.points[2].Y - 20));
                    dc.DrawText(new FormattedText(instrNames[1], CultureInfo.GetCultureInfo("en-us"),
                                                                    FlowDirection.LeftToRight,
                                                                    new Typeface(MainWindow.FONT_FAMILY),
                                                                    18, FlatColors.WHITE),
                                                                    new Point(floor.points[1].X + 10, floor.points[1].Y - 20));
                    dc.DrawText(new FormattedText(instrNames[2], CultureInfo.GetCultureInfo("en-us"),
                                                                    FlowDirection.RightToLeft,
                                                                    new Typeface(MainWindow.FONT_FAMILY),
                                                                    18, FlatColors.WHITE),
                                                                    new Point(floor.points[3].X - 10, floor.points[3].Y + 2));
                    dc.DrawText(new FormattedText(instrNames[3], CultureInfo.GetCultureInfo("en-us"),
                                                                    FlowDirection.LeftToRight,
                                                                    new Typeface(MainWindow.FONT_FAMILY),
                                                                    18, FlatColors.WHITE),
                                                                    new Point(floor.points[0].X + 10, floor.points[0].Y + 2));
                }

                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
            }
            
        }

        private void FloorCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            cornerSelected = 0;
            if      (Keyboard.IsKeyDown(Key.D1)) cornerSelected = 0;
            else if (Keyboard.IsKeyDown(Key.D2)) cornerSelected = 1;
            else if (Keyboard.IsKeyDown(Key.D3)) cornerSelected = 2;
            else if (Keyboard.IsKeyDown(Key.D4)) cornerSelected = 3;
            else
            {
                // no key pressed, don't track
                trackingMouse = false;
                return;
            }

            // track the mouse
            trackingMouse = true;

            // set the corner
            floor.points[cornerSelected] = e.GetPosition(FloorCanvas);
        }
        private void FloorCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            trackingRightMouse = true;
        }

        private void FloorCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            trackingMouse = false;
        }
        private void FloorCanvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            trackingRightMouse = false;
        }

        private void FloorCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (trackingMouse)
            {
                floor.points[cornerSelected] = e.GetPosition(FloorCanvas);
                //floor.SetLowerLeft(e.GetPosition(FloorCanvas));
            }
            else if (trackingRightMouse)
            {
                floor.centroid = e.GetPosition(FloorCanvas);
            }
        }


        // Thanks, http://stackoverflow.com/a/24959131
        private Path makePath(params Point[] points)
        {
            Path path = new Path()
            {
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            if (points.Length == 0)
            {
                return path;
            }

            PathSegmentCollection pathSegments = new PathSegmentCollection();

            for (int i = 1; i < points.Length; i++)
            {
                pathSegments.Add(new LineSegment(points[i], true));
            }

            path.Data = new PathGeometry() {
                Figures = new PathFigureCollection() {
                    new PathFigure() {
                        StartPoint = points[0],
                        Segments = pathSegments
                    }
                }
            };

            return path;
        }

        /// <summary>
        /// Gets the bitmap to display
        /// </summary>
        public ImageSource ImageSource
        {
            get
            {
                return this.imageSource;
            }
        }
    }
}
