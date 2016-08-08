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
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

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

        public FloorWindow()
        {
            this.InitializeComponent();

            this.highlightMaterial = GetSurfaceMaterial(Colors.Red);
            this.normalMaterial = GetSurfaceMaterial(Colors.Green);

            // Draw first time
            // Depths are negative or otherwise x has to be negative
            this.floor = new Floor(new Point3D(-2, 0, -4), new Point3D(-1.25, 0, -2), new Point3D(1.25, 0, -2), new Point3D(2, 0, -4), new Point3D(0, 0, -3));
            model = new ModelVisual3D();
            
            this.UpdateFloorModel(0);
            floorViewport.Children.Add(model);

            trackingMouse = false;
        }

        public void Draw(int curQuadrant, String[] instrNames)
        {
            this.UpdateFloorModel(curQuadrant);
            

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

            Vector3D offset = new Vector3D(xOffset * movementScale, 0, yOffset * movementScale);

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
            }
            
            this.UpdateCameraPosition(offset);
        }
    }
}
