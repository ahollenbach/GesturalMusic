using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Emgu.CV;
using Microsoft.Kinect;
using Emgu.CV.Structure;
using System.Runtime.InteropServices;
using LightBuzz.Vitruvius;

namespace GesturalMusic
{
    /// <summary>
    /// Interaction logic for FloorWindow.xaml
    /// </summary>
    public partial class FloorWindow : Window
    {
        private Floor floor;
        private ModelVisual3D model = new ModelVisual3D();
        private Material highlightMaterial;
        private Material normalMaterial;

        private Boolean trackingMouse;
        private Boolean trackingRightMouse;
        private int cornerSelected;
        private Point startMousePoint;
        ColorFrameReader colorFrameReader;
        bool calibrated = false;
        bool calibrating = false;
        GeometryModel3D alignmentCube;
        KinectSensor kinectSensor;
        private System.Windows.Media.Imaging.BitmapImage bitmap;

        public FloorWindow()
        {
            this.InitializeComponent();

            this.highlightMaterial = GetSurfaceMaterial(Colors.Red);
            this.normalMaterial = GetSurfaceMaterial(Colors.Green);

            // Draw first time
            // Depths are negative or otherwise x has to be negative
            this.floor = new Floor(new Point3D(-2, 0, -4), new Point3D(-1.25, 0, -2), new Point3D(1.25, 0, -2), new Point3D(2, 0, -4), new Point3D(0, 0, -3));
            model = new ModelVisual3D();
            
            this.UpdateFloorModel(-1);
            floorViewport.Children.Add(model);

            // Hide alignment cube
            AlignmentCube.Transform = new ScaleTransform3D(0, 0, 0);

            trackingMouse = false;

            //tmp();
        }

        public void Draw(int curQuadrant, String[] instrNames)
        {
            if(!this.calibrating)
            {
                this.UpdateFloorModel(curQuadrant);
            } else
            {
                model.Content = new Model3DGroup(); // Messy
            }


            //    // Draw center marker
            //    dc.DrawRectangle(FlatColors.WHITE, null, new Rect(floor.centroid.X - 10, floor.centroid.Y - 1, 20, 2));
            //    dc.DrawRectangle(FlatColors.WHITE, null, new Rect(floor.centroid.X - 3, floor.centroid.Y - 4, 6, 8));

            //    // And the instrument to each partition
            //    if (curQuadrant != -1)
            //    {
            //        dc.DrawText(TextBuilder(instrNames[0], flowDirection: FlowDirection.RightToLeft), new Point(floor.points[2].X - 10, floor.points[2].Y - 20));
            //        dc.DrawText(TextBuilder(instrNames[1], flowDirection: FlowDirection.LeftToRight), new Point(floor.points[1].X + 10, floor.points[1].Y - 20));
            //        dc.DrawText(TextBuilder(instrNames[2], flowDirection: FlowDirection.RightToLeft), new Point(floor.points[3].X - 10, floor.points[3].Y + 2));
            //        dc.DrawText(TextBuilder(instrNames[3], flowDirection: FlowDirection.LeftToRight), new Point(floor.points[0].X + 10, floor.points[0].Y + 2));
            //    }

            //    this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
            //}

        }

        public Material GetSurfaceMaterial(Color color)
        {
            return new DiffuseMaterial(new SolidColorBrush(color));
        }

        public void UpdateFloorModel(int currentPartition)
        {
            Model3DGroup floorModel = new Model3DGroup();

            Point3D backMidpoint = GetMidpoint(this.floor.backLeft, this.floor.backRight);
            Point3D frontMidpoint = GetMidpoint(this.floor.frontLeft, this.floor.frontRight);
            Point3D leftMidpoint = GetMidpoint(this.floor.backLeft, this.floor.frontLeft);
            Point3D rightMidpoint = GetMidpoint(this.floor.backRight, this.floor.frontRight);

            Material material = normalMaterial;

            // 0
            material = currentPartition == 0 ? highlightMaterial : normalMaterial;
            AddQuad(floorModel, material, this.floor.frontRight, frontMidpoint, this.floor.centroid, rightMidpoint);

            // 1
            material = currentPartition == 1 ? highlightMaterial : normalMaterial;
            AddQuad(floorModel, material, this.floor.frontLeft, leftMidpoint, this.floor.centroid, frontMidpoint);

            // 2
            material = currentPartition == 2 ? highlightMaterial : normalMaterial;
            AddQuad(floorModel, material, this.floor.centroid, backMidpoint, this.floor.backRight, rightMidpoint);

            // 3
            material = currentPartition == 3 ? highlightMaterial : normalMaterial;
            AddQuad(floorModel, material, leftMidpoint, this.floor.backLeft, backMidpoint, this.floor.centroid);

            model.Content = floorModel;
        }

        private Point3D GetMidpoint(Point3D p1, Point3D p2)
        {
            return new Point3D((p1.X + p2.X) / 2, 0, (p1.Z + p2.Z) / 2);
        }

        // Pass in clockwise
        private void AddQuad(Model3DGroup group, Material material, Point3D p0, Point3D p1, Point3D p2, Point3D p3)
        {
            group.Children.Add(CreateTriangleModel(material, p0, p2, p1));
            group.Children.Add(CreateTriangleModel(material, p2, p0, p3));
        }

        private Model3DGroup CreateTriangleModel(Material material, Point3D p0, Point3D p1, Point3D p2)
        {
            var mesh = new MeshGeometry3D();
            mesh.Positions.Add(p0);
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);
            var normal = CalculateNormal(p0, p1, p2);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);

            var model = new GeometryModel3D(mesh, material);

            var group = new Model3DGroup();
            group.Children.Add(model);
            return group;
        }

        private Vector3D CalculateNormal(Point3D p0, Point3D p1, Point3D p2)
        {
            var v0 = new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            var v1 = new Vector3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            return Vector3D.CrossProduct(v0, v1);
        }

        private FormattedText TextBuilder(String text, FlowDirection flowDirection = FlowDirection.LeftToRight, int size = 18)
        {
            return new FormattedText(text, CultureInfo.GetCultureInfo("en-us"), flowDirection, new Typeface(MainWindow.FONT_FAMILY), size, FlatColors.WHITE);
        }

        private void FloorCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if      (Keyboard.IsKeyDown(Key.D1)) this.cornerSelected = 0;
            else if (Keyboard.IsKeyDown(Key.D2)) this.cornerSelected = 1;
            else if (Keyboard.IsKeyDown(Key.D3)) this.cornerSelected = 2;
            else if (Keyboard.IsKeyDown(Key.D4)) this.cornerSelected = 3;
            else if (e.MiddleButton == MouseButtonState.Released)
            {
                // Either no num key pressed or no middle button, don't track
                this.trackingMouse = false;
                return;
            }

            // track the mouse
            this.trackingMouse = true;

            this.startMousePoint = e.GetPosition(FloorCanvas);
        }
        private void FloorCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.trackingRightMouse = true;

            this.startMousePoint = e.GetPosition(FloorCanvas);
        }

        private void FloorCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.trackingMouse = false;
        }
        private void FloorCanvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.trackingRightMouse = false;
        }

        private void FloorCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!this.trackingMouse && !this.trackingRightMouse)
            {
                return;
            }

            // Defines the movement scale from normalized (-1 -> 1) pixel movement to 3D plane movement
            double movementScale = 3;

            // Get relative mouse position
            Point currentMousePoint = e.GetPosition(FloorCanvas);
            double xOffset = (currentMousePoint.X - startMousePoint.X) / FloorCanvas.ActualWidth;
            double yOffset = (currentMousePoint.Y - startMousePoint.Y) / FloorCanvas.ActualWidth;
            startMousePoint = currentMousePoint;

            Vector3D offset = new Vector3D(xOffset * movementScale, 0, yOffset * movementScale * 3);

            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                this.UpdateCameraLookDirection(offset);
            }
            else if (this.trackingMouse)
            {
                floor.UpdateFloorPointRelative(this.cornerSelected, offset);
            } else if(trackingRightMouse)
            {
                floor.UpdateCentroidRelative(offset);
            }
        }

        private void UpdateCameraLookDirection(Vector3D offset)
        {
            this.FloorCamera.LookDirection += offset;
        }

        private void UpdateCameraPosition(Vector3D offset)
        {
            this.FloorCamera.Position += offset;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            Vector3D offset = new Vector3D();

            double delta = 0.1;

            switch(e.Key)
            {
                case Key.W:
                    offset.Z = -delta;
                    break;
                case Key.S:
                    offset.Z = delta;
                    break;
                case Key.A:
                    offset.X = -delta;
                    break;
                case Key.D:
                    offset.X = delta;
                    break;
                case Key.Q:
                    offset.Y = delta;
                    break;
                case Key.E:
                    offset.Y = -delta;
                    break;
                case Key.C:
                    AlignmentCube.Transform = new ScaleTransform3D(0, 0, 0);
                    this.calibrating = false;
                    this.calibrated = true;
                    this.CloseKinect();
                    break;
            }

            this.UpdateCameraPosition(offset);

            this.PrintSceneState();
        }
        
        private void PrintSceneState()
        {
            Console.WriteLine("------------------");
            Console.WriteLine("Camera Position: " + this.FloorCamera.Position);
            Console.WriteLine("Camera Look Dir: " + this.FloorCamera.LookDirection);
            Console.WriteLine(this.floor);
            Console.WriteLine();
        }

        private void tmp()
        {
            this.FloorCamera.Position = new Point3D(-1.7, 1.5, 0.5);
            this.FloorCamera.LookDirection = new Vector3D(0.36, -0.5, -1.2);
        }

        public void Calibrate()
        {
            Console.WriteLine("Calibrating...");

            if (this.kinectSensor == null)
            {
                this.kinectSensor = KinectSensor.GetDefault();
            }

            this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();
            if (this.colorFrameReader != null)
            {
                this.colorFrameReader.FrameArrived += this.Reader_FrameArrived;
            }
        }

        private void Reader_FrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            if (this.calibrated)
            {
                this.EndCalibration();
            }
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    var bitmap = colorFrame.ToBitmap();

                    Emgu.CV.Image<Bgr, byte> imageFrame = BitmapUtil.ToOpenCVImage(bitmap);

                    //screen.Source = bitmap;

                    //var width = colorFrame.FrameDescription.Width;
                    //var height = colorFrame.FrameDescription.Height;
                    //var data = new byte[width * height * System.Windows.Media.PixelFormats.Bgra32.BitsPerPixel / 8];
                    //colorFrame.CopyConvertedFrameDataToArray(data, ColorImageFormat.Bgra);

                    //var bitmap = new System.Drawing.Bitmap(width, height);
                    //var bitmapData = bitmap.LockBits(
                    //    new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    //    System.Drawing.Imaging.ImageLockMode.WriteOnly,
                    //    bitmap.PixelFormat);
                    //Marshal.Copy(data, 0, bitmapData.Scan0, data.Length);
                    //bitmap.UnlockBits(bitmapData);

                    //Emgu.CV.Image<Bgr, byte> imageFrame = new Image<Bgr, byte> (bitmap);

                    //FrameDescription colorFrameDescription = this.kinectSensor.ColorFrameSource.FrameDescription;
                    //this.bitmap = new System.Windows.Media.Imaging.WriteableBitmap(colorFrame. colorFrameDescription.Width, colorFrameDescription.Height);

                    //this.ShowColorFrame(colorFrame);

                    //var width = colorFrame.FrameDescription.Width;
                    //var height = colorFrame.FrameDescription.Height;

                    //System.Drawing.Bitmap bmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                    //var image = BitmapUtil.ToOpenCVImage<Bgr, byte>(bmap);

                    //System.Drawing.Size patternSize = new System.Drawing.Size(6, 6);
                    //Emgu.CV.Util.VectorOfPoint corners = new Emgu.CV.Util.VectorOfPoint();

                    //bool test = CvInvoke.FindChessboardCorners(image, patternSize, corners);

                    //Console.WriteLine("Calibration Success: " + test);
                    //foreach (System.Drawing.Point cornerPoint in corners.ToArray())
                    //{
                    //    Console.WriteLine(cornerPoint.X + ", " + cornerPoint.Y);
                    //}
                    //Console.WriteLine();

                    //this.calibrated = true;
                }
            }
        }

        private void EndCalibration()
        {
            AlignmentCube.Transform = new ScaleTransform3D(0, 0, 0);
            this.calibrating = false;
            this.calibrated = true;
            this.CloseKinect();
            Console.WriteLine("Calibration complete.");
        }

        private void FloorWindow_Closing(object sender, CancelEventArgs e)
        {
            this.CloseKinect();
        }

        private void CloseKinect()
        {
            if (this.colorFrameReader != null)
            {
                this.colorFrameReader.Dispose();
                this.colorFrameReader = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }

        private void CalibrateButton_Click(object sender, RoutedEventArgs e)
        {
            this.calibrating = true;
            AlignmentCube.Transform = new ScaleTransform3D(1, 1, 1);

            this.Calibrate();
        }
    }
}