﻿using System;
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
using System.Windows.Controls;

namespace GesturalMusic
{
    /// <summary>
    /// Interaction logic for FloorWindow.xaml
    /// </summary>
    public partial class FloorWindow : Window
    {
        private Floor floor;
        private ModelVisual3D model;
        private ModelVisual3D text;

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
            text = new ModelVisual3D();

            this.UpdateFloorModel(-1);
            floorViewport.Children.Add(model);
            floorViewport.Children.Add(text);

            // Hide alignment cube
            AlignmentCube.Transform = new ScaleTransform3D(0, 0, 0);

            trackingMouse = false;

            this.SetupHardcodedCamera();
            
            // Hardcode proof of concept
            double textHeight = 0.05;
            floorViewport.Children.Add(CreateTextLabel3D("INSTR 0",Brushes.AntiqueWhite, true, textHeight, new Point3D(0.5, 0.01, -2 - textHeight), new Vector3D(-1,0,0), new Vector3D(0,0,1)));
            floorViewport.Children.Add(CreateTextLabel3D("INSTR 1",Brushes.AntiqueWhite, true, textHeight, new Point3D(-0.5, 0.01, -2 - textHeight), new Vector3D(-1,0,0), new Vector3D(0,0,1)));
            floorViewport.Children.Add(CreateTextLabel3D("INSTR 2",Brushes.AntiqueWhite, true, textHeight, new Point3D(0.5, 0.01, -3 - textHeight), new Vector3D(-1,0,0), new Vector3D(0,0,1)));
            floorViewport.Children.Add(CreateTextLabel3D("INSTR 3",Brushes.AntiqueWhite, true, textHeight, new Point3D(-0.5, 0.01, -3 - textHeight), new Vector3D(-1,0,0), new Vector3D(0,0,1)));
        }

        public void Draw(int curQuadrant, String[] instrNames)
        {
            this.UpdateFloorModel(curQuadrant);
        }

        /// <summary>
        /// Creates a ModelVisual3D containing a text label.
        /// Source: http://ericsink.com/wpf3d/4_Text.html
        /// </summary>
        /// <param name="text">The string</param>
        /// <param name="textColor">The color of the text.</param>
        /// <param name="bDoubleSided">Visible from both sides?</param>
        /// <param name="height">Height of the characters</param>
        /// <param name="center">The center of the label</param>
        /// <param name="over">Horizontal direction of the label</param>
        /// <param name="up">Vertical direction of the label</param>
        /// <returns>Suitable for adding to your Viewport3D</returns>
        public static ModelVisual3D CreateTextLabel3D(
            string text,
            Brush textColor,
            bool bDoubleSided,
            double height,
            Point3D center,
            Vector3D over,
            Vector3D up)
        {
            // First we need a textblock containing the text of our label
            TextBlock tb = new TextBlock(new System.Windows.Documents.Run(text));
            tb.Foreground = textColor;
            tb.FontFamily = new FontFamily("Arial");

            // Now use that TextBlock as the brush for a material
            DiffuseMaterial mat = new DiffuseMaterial();
            mat.Brush = new VisualBrush(tb);

            // We just assume the characters are square
            double width = text.Length * height;

            // Since the parameter coming in was the center of the label,
            // we need to find the four corners
            // p0 is the lower left corner
            // p1 is the upper left
            // p2 is the lower right
            // p3 is the upper right
            Point3D p0 = center - width / 2 * over - height / 2 * up;
            Point3D p1 = p0 + up * 1 * height;
            Point3D p2 = p0 + over * width;
            Point3D p3 = p0 + up * 1 * height + over * width;

            // Now build the geometry for the sign.  It's just a
            // rectangle made of two triangles, on each side.

            MeshGeometry3D mg = new MeshGeometry3D();
            mg.Positions = new Point3DCollection();
            mg.Positions.Add(p0);    // 0
            mg.Positions.Add(p1);    // 1
            mg.Positions.Add(p2);    // 2
            mg.Positions.Add(p3);    // 3

            if (bDoubleSided)
            {
                mg.Positions.Add(p0);    // 4
                mg.Positions.Add(p1);    // 5
                mg.Positions.Add(p2);    // 6
                mg.Positions.Add(p3);    // 7
            }

            mg.TriangleIndices.Add(0);
            mg.TriangleIndices.Add(3);
            mg.TriangleIndices.Add(1);
            mg.TriangleIndices.Add(0);
            mg.TriangleIndices.Add(2);
            mg.TriangleIndices.Add(3);

            if (bDoubleSided)
            {
                mg.TriangleIndices.Add(4);
                mg.TriangleIndices.Add(5);
                mg.TriangleIndices.Add(7);
                mg.TriangleIndices.Add(4);
                mg.TriangleIndices.Add(7);
                mg.TriangleIndices.Add(6);
            }

            // These texture coordinates basically stretch the
            // TextBox brush to cover the full side of the label.

            mg.TextureCoordinates.Add(new Point(0, 1));
            mg.TextureCoordinates.Add(new Point(0, 0));
            mg.TextureCoordinates.Add(new Point(1, 1));
            mg.TextureCoordinates.Add(new Point(1, 0));

            if (bDoubleSided)
            {
                mg.TextureCoordinates.Add(new Point(1, 1));
                mg.TextureCoordinates.Add(new Point(1, 0));
                mg.TextureCoordinates.Add(new Point(0, 1));
                mg.TextureCoordinates.Add(new Point(0, 0));
            }

            // And that's all.  Return the result.

            ModelVisual3D mv3d = new ModelVisual3D();
            mv3d.Content = new GeometryModel3D(mg, mat); ;
            return mv3d;
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

        private void SetupHardcodedCamera()
        {
            this.FloorCamera.Position = new Point3D(-1.7, 1.5, 0.5);
            this.FloorCamera.LookDirection = new Vector3D(0.36, -0.5, -1.2);
        }
    }
}