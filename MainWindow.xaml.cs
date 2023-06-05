﻿namespace GesturalMusic
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    using Ventuz.OSC;
    using System.Windows.Controls;
    using System.Windows.Media.Media3D;

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public readonly static String FONT_FAMILY = "Verdana";

        /// <summary>
        /// Radius of drawn hand circles
        /// </summary>
        private const double HandSize = 30;

        /// <summary>
        /// Thickness of drawn joint lines
        /// </summary>
        private const double JointThickness = 3;

        /// <summary>
        /// Thickness of clip edge rectangles
        /// </summary>
        private const double ClipBoundsThickness = 10;

        /// <summary>
        /// Constant for clamping Z values of camera space points from being negative
        /// </summary>
        private const float InferredZPositionClamp = 0.1f;

        /// <summary>
        /// Brush used for drawing hands that are currently tracked as closed
        /// </summary>
        private readonly Brush handClosedBrush = FlatColors.Translucent(FlatColors.LIGHT_RED);

        /// <summary>
        /// Brush used for drawing hands that are currently tracked as opened
        /// </summary>
        private readonly Brush handOpenBrush = FlatColors.Translucent(FlatColors.LIGHT_GREEN);

        /// <summary>
        /// Brush used for drawing hands that are currently tracked as in lasso (pointer) position
        /// </summary>
        private readonly Brush handLassoBrush = FlatColors.Translucent(FlatColors.LIGHT_BLUE);

        /// <summary>
        /// Brush used for drawing joints that are currently tracked
        /// </summary>
        private readonly Brush trackedJointBrush = FlatColors.WHITE;

        /// <summary>
        /// Brush used for drawing joints that are currently inferred
        /// </summary>        
        private readonly Brush inferredJointBrush = FlatColors.YELLOW;

        /// <summary>
        /// Pen used for drawing bones that are currently inferred
        /// </summary>        
        private readonly Pen inferredBonePen = new Pen(FlatColors.LIGHT_GRAY, 1);

        /// <summary>
        /// Drawing group for body rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

        /// <summary>
        /// Coordinate mapper to map one type of point to another
        /// </summary>
        private CoordinateMapper coordinateMapper = null;

        /// <summary>
        /// Reader for body frames
        /// </summary>
        private BodyFrameReader bodyFrameReader = null;

        /// <summary>
        /// Array for the bodies
        /// </summary>
        private Body[] bodies = null;

        /// <summary>
        /// definition of bones
        /// </summary>
        private List<Tuple<JointType, JointType>> bones;

        /// <summary>
        /// Width of display (depth space)
        /// </summary>
        private int displayWidth;

        /// <summary>
        /// Height of display (depth space)
        /// </summary>
        private int displayHeight;

        /// <summary>
        /// The host ip address (the computer with Ableton + Max for Live on it). Default: "127.0.0.1"
        /// </summary>
        public static String oscHost = "127.0.0.1";

        /// <summary>
        /// The port to send to: default 22345
        /// </summary>
        private static int oscPort = 22345;

        /// <summary>
        /// The port to send looper information to: default 22344
        /// </summary>
        public static int looperOscPort = 2345;

        /// <summary>
        /// Current status text to display
        /// </summary>
        public static UdpWriter osc;

        Random r = new Random();
        private DateTime startTime;
        StreamWriter jointDataFile;

        private Instrument[] instruments;
        LooperOSC looper;

        public static double armLength;

        public readonly static String ADVANCED_MODE = "Advanced";
        public readonly static String DEMO_MODE = "Demo";
        public static String playingMode;

        FloorWindow floorWindow;

        /// <summary>
        /// Set the number of partitions
        /// </summary>
        /// 
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SetNumPartitions(object sender, RoutedEventArgs e)
        {
            if (onePartition.IsChecked.GetValueOrDefault())        PartitionManager.SetPartitionType(PartitionType.Single);
            else if (twoPartitionLR.IsChecked.GetValueOrDefault()) PartitionManager.SetPartitionType(PartitionType.DoubleLeftRight);
            else if (twoPartitionFB.IsChecked.GetValueOrDefault()) PartitionManager.SetPartitionType(PartitionType.DoubleFrontBack);
            else if (quadPartition.IsChecked.GetValueOrDefault())  PartitionManager.SetPartitionType(PartitionType.Quad);
        }

        /// <summary>
        /// Launch projector screen
        /// </summary>
        /// 
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void LaunchProjectorScreen(object sender, RoutedEventArgs e)
        {
            if (this.floorWindow == null || !this.floorWindow.IsVisible)
            {
                this.floorWindow = new FloorWindow();
                this.floorWindow.Show();
            }
        }

        private void SetRecipient(object sender, RoutedEventArgs e)
        {
            oscHost = RecipientIpAddress.Text;
            Console.WriteLine(oscHost);
            osc = new UdpWriter(oscHost, oscPort);
            LooperOSC.ResetOsc();
            // Save in settings
            //UserSettings.Default.RecipientIpAddress = RecipientIpAddress.Text;
            //UserSettings.Default.Save();
        }

        /// <summary>
        /// Sets the playing mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetMode(object sender, RoutedEventArgs e)
        {
            if (AdvancedMode.IsChecked.GetValueOrDefault())
            {
                playingMode = ADVANCED_MODE;

                for (int i = 1; i < PartitionRadio.Children.Count; i++)
                {
                    PartitionRadio.Children[i].IsEnabled = true;
                }
                instruments[0] = new Instrument("instr0");
            }
            else
            {
                playingMode = DEMO_MODE;

                PartitionManager.SetPartitionType(PartitionType.Single);
                onePartition.IsChecked = true;

                for (int i = 1; i < PartitionRadio.Children.Count; i++)
                {
                    PartitionRadio.Children[i].IsEnabled = false;
                }
                instruments[0] = new DemoInstrument("instr0");
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            // Get the reference time
            startTime = DateTime.Now;
            jointDataFile = new StreamWriter("jointOutput.csv", true);

            // initialize WPF
            try
            {
                RecipientIpAddress.Text = UserSettings.Default.RecipientIpAddress;
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine("IP settings not initialized!");
            }

            ///////////////////////////////////////////////////////////////////////
            // Set up OSC
            ///////////////////////////////////////////////////////////////////////
            osc = new UdpWriter(oscHost, oscPort);

            //Looper
            looper = new LooperOSC();

            // Instruments
            instruments = new Instrument[4];

            for (int i = 0; i < instruments.Length; i++)
            {
                instruments[i] = new Instrument("instr" + i);
            }


            ///////////////////////////////////////////////////////////////////////
            // Initialize Kinect
            ///////////////////////////////////////////////////////////////////////
            this.kinectSensor = KinectSensor.GetDefault();

            // get the coordinate mapper
            this.coordinateMapper = this.kinectSensor.CoordinateMapper;

            // get the depth (display) extents
            FrameDescription frameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;

            // get size of joint space
            this.displayWidth = frameDescription.Width;
            this.displayHeight = frameDescription.Height;

            // open the reader for the body frames
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            // a bone defined as a line between two joints
            this.bones = new List<Tuple<JointType, JointType>>();

            // Torso
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Head, JointType.Neck));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Neck, JointType.SpineShoulder));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.SpineMid));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineMid, JointType.SpineBase));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipLeft));

            // Right Arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderRight, JointType.ElbowRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowRight, JointType.WristRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.HandRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandRight, JointType.HandTipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.ThumbRight));

            // Left Arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderLeft, JointType.ElbowLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowLeft, JointType.WristLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.HandLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandLeft, JointType.HandTipLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.ThumbLeft));

            // Right Leg
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipRight, JointType.KneeRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeRight, JointType.AnkleRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleRight, JointType.FootRight));

            // Left Leg
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipLeft, JointType.KneeLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeLeft, JointType.AnkleLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleLeft, JointType.FootLeft));

            // open the sensor
            this.kinectSensor.Open();

            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // use the window object as the view model in this simple example
            this.DataContext = this;

            // initialize the components (controls) of the window
            this.InitializeComponent();
        }

        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

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

        /// <summary>
        /// Execute start up tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.bodyFrameReader != null)
            {
                this.bodyFrameReader.FrameArrived += this.Reader_FrameArrived;
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.bodyFrameReader != null)
            {
                // BodyFrameReader is IDisposable
                this.bodyFrameReader.Dispose();
                this.bodyFrameReader = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }

            if (jointDataFile != null)
            {
                jointDataFile.Close();
            }
        }

        /// <summary>
        /// Handles the body frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData(this.bodies);

                    Update(bodyFrame);
                }
            }
        }

        private void Update(BodyFrame bodyFrame)
        {
            // Selects the first body that is tracked and use that for our calculations
            Body body = System.Linq.Enumerable.FirstOrDefault(this.bodies, bod => bod.IsTracked);

            if (body != null)
            {
                bool played;
                bool loop;

                // Set arm length if not yet set
                //if (armLength == 0 && body.HandLeftState == HandState.Lasso && body.HandRightState == HandState.Lasso)
                //{
                //    armLength = Utils.Length(body.Joints[JointType.ShoulderLeft], body.Joints[JointType.ElbowLeft]) +
                //                Utils.Length(body.Joints[JointType.ElbowLeft], body.Joints[JointType.WristLeft]);
                //}
                
                if(armLength == 0) {
                    played = false;
                    loop = false;
                }
                else
                {
                    played = SendInstrumentData(body);
                    loop = looper.SendLooperData(body);
                }
            }

            ///////////////////////////////////////////////////////////////////////
            // Draw the Screen
            ///////////////////////////////////////////////////////////////////////
            using (DrawingContext dc = this.drawingGroup.Open())
            {
                if (body == null)
                {
                    dc.DrawRectangle(FlatColors.WHITE, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));

                    // Draw our floor
                    if (this.floorWindow != null) this.floorWindow.Draw(1, new String[4]);
                    return;
                }

                // calculate the arm length
                armLength = Utils.Length(body.Joints[JointType.ShoulderLeft], body.Joints[JointType.ElbowLeft]) +
                                Utils.Length(body.Joints[JointType.ElbowLeft], body.Joints[JointType.WristLeft]);

                dc.DrawRectangle(FlatColors.DARK_BLUEGRAY, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));

                //Vector4 floor = bodyFrame.FloorClipPlane;
                //Console.WriteLine("FloorClipPlane");
                //Console.WriteLine(floor.X + " " + floor.Y + " " + floor.Z + " " + floor.W);
                //Console.WriteLine("FloorClipPlane");

                //DepthSpacePoint depthSpacePoint = this.coordinateMapper.MapCameraPointToDepthSpace(floor);

                foreach (Body b in this.bodies)
                {
                    Pen drawPen = new Pen(FlatColors.WHITE,7);

                    if (b.IsTracked)
                    {
                        this.DrawClippedEdges(b, dc);

                        IReadOnlyDictionary<JointType, Joint> joints = b.Joints;

                        // convert the joint points to depth (display) space
                        Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();

                        foreach (JointType jointType in joints.Keys)
                        {
                            // sometimes the depth(Z) of an inferred joint may show as negative
                            // clamp down to 0.1f to prevent coordinatemapper from returning (-Infinity, -Infinity)
                            CameraSpacePoint position = joints[jointType].Position;
                            if (position.Z < 0)
                            {
                                position.Z = InferredZPositionClamp;
                            }

                            DepthSpacePoint depthSpacePoint = this.coordinateMapper.MapCameraPointToDepthSpace(position);
                            jointPoints[jointType] = new Point(depthSpacePoint.X, depthSpacePoint.Y);
                        }

                        if (MainWindow.playingMode == MainWindow.ADVANCED_MODE)
                        {
                            this.DrawCurrentPartition(b, joints, jointPoints, dc);
                            //this.InstrumentSelect(b, joints, jointPoints, dc );
                        }
                        else
                        {
                            this.displayDemoHud(b, joints, jointPoints, dc);
                        }
                        
                        this.DrawBody(joints, jointPoints, dc, drawPen);

                        this.DrawHand(b.HandLeftState, jointPoints[JointType.HandLeft], dc);
                        this.DrawHand(b.HandRightState, jointPoints[JointType.HandRight], dc);

                        // after the first, the rest are darkened a bit (to signify not tracked)
                        drawPen = new Pen(FlatColors.LIGHT_GRAY, 7);
                    }
                }

                // prevent drawing outside of our render area
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
            }

            if (this.floorWindow != null)
            {
                String[] names = new String[instruments.Length];
                for (int i = 0; i < instruments.Length; i++)
                {
                    names[i] = instruments[i].name;
                }

                this.floorWindow.Draw(PartitionManager.GetPartition(body.Joints[JointType.SpineMid].Position), names);
            }
        }

        /// <summary>
        /// Sends OSC messages if applicable
        /// </summary>
        /// <returns>The color the background should display (for user feedback)</returns>
        private bool SendInstrumentData(Body body)
        {
            // Send joint data to animators, write to a file
            if (body.HandLeftState == body.HandRightState && body.HandLeftState == HandState.Open)
            {
                //SendJointData(body, true);
            }

            CameraSpacePoint spineMidPos = body.Joints[JointType.SpineMid].Position;
            CameraSpacePoint lHandPos = body.Joints[JointType.HandLeft].Position;
            CameraSpacePoint rHandPos = body.Joints[JointType.HandRight].Position;

            // trigger start if both left and right hand are open
            bool triggerStart = body.HandLeftState == body.HandRightState && body.HandLeftState == HandState.Open;
            // trigger end if both left and right hand are closed and below the Kinect
            bool triggerEnd = body.HandLeftState == body.HandRightState && body.HandLeftState == HandState.Closed && lHandPos.Y < 0 && rHandPos.Y < 0;

            int partition = PartitionManager.GetPartition(spineMidPos);

            if (body.HandLeftState == HandState.Lasso)
            {
                // Send volume as a value between 0 and 1, only when thumbs up
                //sliders[instruments[partition] + "/volume"].Send(lHandPos.Y);
            }

            // Ask the instrument if it wants to play
            Instrument instrument = instruments[partition];
            Console.WriteLine(instrument.ToString());
            if (instrument.ToString() == "MidiPad")
            {
                MidiPad pad = (MidiPad)instruments[partition];

                return pad.CheckAndPlayNote(body);
            }
            else if (playingMode == DEMO_MODE)
            {
                DemoInstrument demoInstr = (DemoInstrument)instruments[partition];

                return demoInstr.CheckAndPlayNoteD(body);
            }
            else
            {
                return instrument.CheckAndPlayNote(body);
            }
        }

        /// <summary>
        /// Sends joint data to the animators. If the boolean writeToFile is set, it will
        /// generate a file locally of animation data.
        /// TODO: This method only writes to file and does not send over OSC
        /// </summary>
        /// <param name="body">The body of joints to send</param>
        /// <param name="writeToFile">If we should write to a file (currently ignored)</param>
        private void SendJointData(Body body, bool writeToFile)
        {
            TimeSpan elapsedTime = DateTime.Now - startTime;

            IReadOnlyDictionary<JointType,JointOrientation> jointOrientations = body.JointOrientations;
            foreach (JointType jointType in Enum.GetValues(typeof(JointType)))
            {
                Joint joint = body.Joints[jointType];
                Vector4 vec = jointOrientations[jointType].Orientation;

                jointDataFile.WriteLine(joint.JointType + "," + joint.Position.X + "," + joint.Position.Y + "," + joint.Position.Z + "," + 
                                                                vec.W + "," + vec.X + "," + vec.Y + "," + vec.Z + "," + 
                                                                elapsedTime.TotalMilliseconds);
            }
        }

        /// <summary>
        /// Change Partition Manager parameters to store set instruments for partitions
        /// of current PartitionType
        /// </summary>
        /// <param name="number"></param>
        /// <param name="instrumentName"></param>
        private void changeValuesinPartitionManager(int number, string instrumentName)
        {
            PartitionManager.isPartitionSet[number] = true;
            PartitionManager.partitionInstrSetName[number] = instrumentName;
            PartitionManager.val3 = instrumentName;
            instruments[number] = new Instrument(instrumentName);
        }

        private void displayDemoHud(Body b, IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, DrawingContext drawingContext)
        {
            // do nothing for now
        }

        /// <summary>
        /// Check the current PartitionType which is selected and if an instrument
        /// has been set for the partition the body is in that PartitionType 
        /// </summary>
        /// <param name="b"></param>
        /// <param name="joints"></param>
        /// <param name="jointPoints"></param>
        /// <param name="drawingContext"></param>
        private void decidePartitionToBeChecked(Body b, IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, DrawingContext drawingContext)
        {
            // DOUBLE LEFT RIGHT PARTITIONS
            if (PartitionManager.currentPartitionType == PartitionType.DoubleLeftRight)
            {
                int whichPartitionAmIIn = PartitionManager.GetPartition(b.Joints[JointType.SpineMid].Position);

                // Check if in LEFT partition
                if (whichPartitionAmIIn == 0)
                {
                    PartitionManager.val3 = PartitionManager.partitionInstrSetName[whichPartitionAmIIn];

                    // Check flag for LEFT partition
                    if (!PartitionManager.isPartitionSet[whichPartitionAmIIn])
                    {
                        string temp = InstrumentSelect(b, joints, jointPoints, drawingContext);

                        if (temp != "void")
                        {
                            changeValuesinPartitionManager(whichPartitionAmIIn, temp);
                        }
                    }
                }
                // Check if in RIGHT partition
                else if (whichPartitionAmIIn == 1)
                {
                    PartitionManager.val3 = PartitionManager.partitionInstrSetName[whichPartitionAmIIn];

                    // Check flag for RIGHT partition
                    if (!PartitionManager.isPartitionSet[whichPartitionAmIIn])
                    {
                        string temp = InstrumentSelect(b, joints, jointPoints, drawingContext);

                        if (temp != "void")
                        {
                            changeValuesinPartitionManager(whichPartitionAmIIn, temp);
                        }
                    }
                }
            } // END DOUBLE LEFT RIGHT PARTITIONS

            // START DOUBLE FRONT BACK PARTITIONS
            else if (PartitionManager.currentPartitionType == PartitionType.DoubleFrontBack)
            {
                int whichPartitionAmIIn = PartitionManager.GetPartition(b.Joints[JointType.SpineMid].Position);

                // Check if in FRONT partition
                if (whichPartitionAmIIn == 0)
                {
                    PartitionManager.val3 = PartitionManager.partitionInstrSetName[whichPartitionAmIIn];

                    // Check flag for FRONT partition
                    if (!PartitionManager.isPartitionSet[whichPartitionAmIIn])
                    {
                        string temp = InstrumentSelect(b, joints, jointPoints, drawingContext);

                        if (temp != "void")
                        {
                            changeValuesinPartitionManager(whichPartitionAmIIn, temp);
                        }
                    }
                }
                // Check if in BACK partition
                else if (whichPartitionAmIIn == 1)
                {
                    PartitionManager.val3 = PartitionManager.partitionInstrSetName[whichPartitionAmIIn];

                    // Check flag for BACK partition
                    if (!PartitionManager.isPartitionSet[whichPartitionAmIIn])
                    {
                        string temp = InstrumentSelect(b, joints, jointPoints, drawingContext);

                        if (temp != "void")
                        {
                            changeValuesinPartitionManager(whichPartitionAmIIn, temp);
                        }
                    }
                }
            } // END DOUBLE FRONT BACK PARTITIONS

            // START QUAD PARTITIONS
            else if (PartitionManager.currentPartitionType == PartitionType.Quad)
            {
                int whichPartitionAmIIn = PartitionManager.GetPartition(b.Joints[JointType.SpineMid].Position);

                // Check if in partition 0
                if (whichPartitionAmIIn == 0)
                {
                    PartitionManager.val3 = PartitionManager.partitionInstrSetName[whichPartitionAmIIn];

                    // Check flag for partition 0
                    if (!PartitionManager.isPartitionSet[whichPartitionAmIIn])
                    {
                        string temp = InstrumentSelect(b, joints, jointPoints, drawingContext);

                        if (temp != "void")
                        {
                            changeValuesinPartitionManager(whichPartitionAmIIn, temp);
                        }

                    }
                }
                // Check if in partition 1
                else if (whichPartitionAmIIn == 1)
                {
                    PartitionManager.val3 = PartitionManager.partitionInstrSetName[whichPartitionAmIIn];

                    // Check flag for partition 1
                    if (!PartitionManager.isPartitionSet[whichPartitionAmIIn])
                    {
                        string temp = InstrumentSelect(b, joints, jointPoints, drawingContext);

                        if (temp != "void")
                        {
                            changeValuesinPartitionManager(whichPartitionAmIIn, temp);
                        }
                    }
                }
                // Check if in partition 2
                else if (whichPartitionAmIIn == 2)
                {
                    PartitionManager.val3 = PartitionManager.partitionInstrSetName[whichPartitionAmIIn];

                    // Check flag for partition 2
                    if (!PartitionManager.isPartitionSet[whichPartitionAmIIn])
                    {
                        string temp = InstrumentSelect(b, joints, jointPoints, drawingContext);

                        if (temp != "void")
                        {
                            changeValuesinPartitionManager(whichPartitionAmIIn, temp);
                        }

                    }
                }
                // Check if in partition 3
                else if (whichPartitionAmIIn == 3)
                {
                    PartitionManager.val3 = PartitionManager.partitionInstrSetName[whichPartitionAmIIn];

                    // Check flag for partition 3
                    if (!PartitionManager.isPartitionSet[whichPartitionAmIIn])
                    {
                        string temp = InstrumentSelect(b, joints, jointPoints, drawingContext);

                        if (temp != "void")
                        {
                            changeValuesinPartitionManager(whichPartitionAmIIn, temp);
                        }
                    }
                }
            } // END QUAD PARTITIONS


            // DEFAULT
            // START SINGLE PARTITION
            else
            {
                int whichPartitionAmIIn = PartitionManager.GetPartition(b.Joints[JointType.SpineMid].Position);

                // Check partition
                if (whichPartitionAmIIn == 0)
                {
                    PartitionManager.val3 = PartitionManager.partitionInstrSetName[whichPartitionAmIIn];

                    // Check flag partition
                    if (!PartitionManager.isPartitionSet[whichPartitionAmIIn])
                    {
                        string temp = InstrumentSelect(b, joints, jointPoints, drawingContext);

                        if (temp != "void")
                        {
                            changeValuesinPartitionManager(whichPartitionAmIIn, temp);
                        }
                    }
                }
            }

            // Display "DEFAULT" or "SET INSTRUMENT_NAME" at the top right corner of
            // the screen for the current partition
            if (PartitionManager.val3 == "void")
            {
                displaySetConfirmation(drawingContext, instruments[PartitionManager.GetPartition(b.Joints[JointType.SpineMid].Position)].name);
            }
            else
            {
                displaySetConfirmation(drawingContext, PartitionManager.val3);
            }
        }

        private void DrawCurrentPartition(Body b, IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, DrawingContext drawingContext)
        {
            // display current location
            drawingContext.DrawRectangle(
                    Brushes.DarkGray,
                    null,
                    new Rect(this.displayWidth / 2 - 36, this.displayHeight-30, 72, 30));
            String curPartition = PartitionManager.GetPartition(b.Joints[JointType.SpineMid].Position).ToString();
            drawingContext.DrawText(new FormattedText(curPartition, CultureInfo.GetCultureInfo("en-us"),
                                                              FlowDirection.LeftToRight,
                                                              new Typeface(MainWindow.FONT_FAMILY),
                                                              20, System.Windows.Media.Brushes.White),
                                                              new Point(this.displayWidth / 2 - 6, this.displayHeight-28));
            this.DisplayLooperState(drawingContext, looper.GetState());
        }

        /// <summary>
        /// Set an instrument for current partition by choosing from a list of
        /// instruments
        /// </summary>
        /// <param name="joints"></param>
        /// <param name="jointPoints"></param>
        /// <param name="drawingContext"></param>
        private string InstrumentSelect(Body body, IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, DrawingContext drawingContext)
        {
            //PartitionType partVal = PartitionManager.getCurrentPartitionType();
            //PartitionManager.currentPartitionType
            // Get HandTipLeft and ShoulderLeft positions
            CameraSpacePoint htLeftc = joints[JointType.HandTipLeft].Position;
            CameraSpacePoint sLeftc = joints[JointType.ShoulderLeft].Position;

            // Create 3D Vectors for HandTipLeft, ShoulderLeft and a reference point
            Vector3D hL = new Vector3D(htLeftc.X, htLeftc.Y, htLeftc.Z);
            Vector3D sL = new Vector3D(sLeftc.X, sLeftc.Y, sLeftc.Z);
            Vector3D temp = new Vector3D(htLeftc.X, sLeftc.Y, htLeftc.Z);

            // Get the 3D Vectors representing the two lines
            Vector3D line1 = sL - hL;
            Vector3D line2 = sL - temp;

            // Get dot product of the two which gives the angle between the lines
            // double angle = Vector3D.DotProduct(line1, line2);

            // Get the angle between the two lines by using inbuilt Vector3D function
            double angle2 = Vector3D.AngleBetween(line2, line1);
            //Console.WriteLine(" angle iiiisssssssssssssssssssss theeeeeesss" + angle2);


            // If handtip is at the required angle w.r.t the left shoulder
            // AND HandState is closed
            // SELECT current option

            DepthSpacePoint sl = this.coordinateMapper.MapCameraPointToDepthSpace(sLeftc);

            float textXOffset = -15;
            float textYOffset = -20;

            float i0XOffset = -150;
            float i0YOffset = -30;

            if (sl.X + i0XOffset < 10)
            {
                i0XOffset += Math.Abs(sl.X + i0XOffset + 10);
            }

            if (htLeftc.Y > sLeftc.Y)
            {
                // Hand at approximately 25 degrees
                if (angle2 > 20 && angle2 < 30 && body.HandLeftState == HandState.Closed)
                {
                    drawingContext.DrawEllipse(Brushes.IndianRed, new Pen(Brushes.MistyRose, 1), new Point(sl.X + i0XOffset, sl.Y + i0YOffset), 20, 20);
                    if (body.HandRightState == HandState.Lasso) return "instr0";
                }
                else
                {
                    drawingContext.DrawEllipse(Brushes.White, new Pen(Brushes.White, 1), new Point(sl.X + i0XOffset, sl.Y + i0YOffset), 20, 20);
                }
                drawingContext.DrawText(new FormattedText("i0",CultureInfo.GetCultureInfo("en-us"),
                                                              FlowDirection.LeftToRight,
                                                              new Typeface(MainWindow.FONT_FAMILY),
                                                              30, System.Windows.Media.Brushes.Black),
                                                              new Point(sl.X + i0XOffset + textXOffset, sl.Y + i0YOffset + textYOffset));

                // Hand at approx 45 degrees
                if (angle2 > 40 && angle2 < 50 && body.HandLeftState == HandState.Closed)
                {
                    drawingContext.DrawEllipse(Brushes.RoyalBlue, new Pen(Brushes.RoyalBlue, 1), new Point(sl.X + i0XOffset + 20, sl.Y + i0YOffset - 40), 20, 20);
                    if (body.HandRightState == HandState.Lasso) return "instr1";
                }
                else
                {
                    drawingContext.DrawEllipse(Brushes.White, new Pen(Brushes.White, 1), new Point(sl.X + i0XOffset + 20, sl.Y + i0YOffset - 40), 20, 20);
                }
                drawingContext.DrawText(new FormattedText("i1", CultureInfo.GetCultureInfo("en-us"),
                                                              FlowDirection.LeftToRight,
                                                              new Typeface(MainWindow.FONT_FAMILY),
                                                              30, System.Windows.Media.Brushes.Black),
                                                              new Point(sl.X + i0XOffset + textXOffset + 20, sl.Y + i0YOffset + textYOffset - 40));

                // Hand at approximately 65 degrees
                if (angle2 > 60 && angle2 < 70 && body.HandLeftState == HandState.Closed)
                {
                    drawingContext.DrawEllipse(Brushes.Goldenrod, new Pen(Brushes.SandyBrown, 1), new Point(sl.X + i0XOffset + 60, sl.Y + i0YOffset - 60), 20, 20);
                    if (body.HandRightState == HandState.Lasso) return "instr2";
                }
                else
                {
                    drawingContext.DrawEllipse(Brushes.White, new Pen(Brushes.White, 1), new Point(sl.X + i0XOffset + 60, sl.Y + i0YOffset - 60), 20, 20);
                }
                drawingContext.DrawText(new FormattedText("i2", CultureInfo.GetCultureInfo("en-us"),
                                                              FlowDirection.LeftToRight,
                                                              new Typeface(MainWindow.FONT_FAMILY),
                                                              30, System.Windows.Media.Brushes.Black),
                                                              new Point(sl.X + i0XOffset + textXOffset + 60, sl.Y + i0YOffset + textYOffset - 60));
                // hAND AT APPROX 85
                if (angle2 > 80 && angle2 < 90 && body.HandLeftState == HandState.Closed)
                {
                    drawingContext.DrawEllipse(Brushes.Ivory, new Pen(Brushes.SandyBrown, 1), new Point(sl.X + i0XOffset + 100, sl.Y + i0YOffset - 70), 20, 20);
                    if (body.HandRightState == HandState.Lasso) return "instr3";
                }
                else
                {
                    drawingContext.DrawEllipse(Brushes.White, new Pen(Brushes.White, 1), new Point(sl.X + i0XOffset + 100, sl.Y + i0YOffset - 70), 20, 20);
                }
                drawingContext.DrawText(new FormattedText("i3", CultureInfo.GetCultureInfo("en-us"),
                                                              FlowDirection.LeftToRight,
                                                              new Typeface(MainWindow.FONT_FAMILY),
                                                              30, System.Windows.Media.Brushes.Black),
                                                              new Point(sl.X + i0XOffset + textXOffset + 100, sl.Y + i0YOffset + textYOffset - 70));

                
            }

            return "void";
        }

        /// <summary>
        /// Display if an instrument is set or unset for current partition
        /// in the top right of the screen
        /// </summary>
        /// <param name="flag"></param>
        /// <param name="drawingContext"></param>
        private void displaySetConfirmation(DrawingContext drawingContext, string displayThis)
        {
            Point textOrigin = new Point(430, 40);
            Point ellipseOrigin = new Point(450, 50);
            // Denotes whether current region has been set or unset
            drawingContext.DrawEllipse(FlatColors.LIGHT_BLUE, new Pen(FlatColors.LIGHT_BLUE, 1), ellipseOrigin, 40, 40);
            FormattedText writeThis = new FormattedText(displayThis, new CultureInfo("en-US"), FlowDirection.LeftToRight, new Typeface("Arial"), 15.0, Brushes.White);
            drawingContext.DrawText(writeThis, textOrigin);
        }

        private void DisplayLooperState(DrawingContext drawingContext, string displayThis)
        {
            Point textOrigin = new Point(430, 40);
            Point ellipseOrigin = new Point(450, 50);
            // Denotes whether current region has been set or 

            SolidColorBrush color = FlatColors.LIGHT_BLUE;
            if (displayThis == "RECD") color = FlatColors.LIGHT_RED;
            if (displayThis == "ODUB") color = FlatColors.LIGHT_ORANGE;
            if (displayThis == "PLAY") color = FlatColors.LIGHT_GREEN;
            if (displayThis == "STOP") color = FlatColors.LIGHT_BLUE;

            drawingContext.DrawEllipse(color, new Pen(color, 1), ellipseOrigin, 40, 40);
            FormattedText writeThis = new FormattedText(displayThis, new CultureInfo("en-US"), FlowDirection.LeftToRight, new Typeface("Arial"), 15.0, Brushes.White);
            drawingContext.DrawText(writeThis, textOrigin);
        }

        /// <summary>
        /// Draws a body
        /// </summary>
        /// <param name="joints">joints to draw</param>
        /// <param name="jointPoints">translated positions of joints to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="drawingPen">specifies color to draw a specific body</param>
        private void DrawBody(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, DrawingContext drawingContext, Pen drawingPen)
        {
            // Draw the bones
            foreach (var bone in this.bones)
            {
                this.DrawBone(joints, jointPoints, bone.Item1, bone.Item2, drawingContext, drawingPen);
            }

            // Draw the joints
            foreach (JointType jointType in joints.Keys)
            {
                Brush drawBrush = null;

                TrackingState trackingState = joints[jointType].TrackingState;

                if (trackingState == TrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;
                }
                else if (trackingState == TrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, jointPoints[jointType], JointThickness, JointThickness);
                }
            }

            // Note guideline

            //DepthSpacePoint sl = this.coordinateMapper.MapCameraPointToDepthSpace(joints[JointType.ShoulderLeft].Position);
            //DepthSpacePoint el = this.coordinateMapper.MapCameraPointToDepthSpace(joints[JointType.ElbowLeft].Position);
            //DepthSpacePoint wl = this.coordinateMapper.MapCameraPointToDepthSpace(joints[JointType.WristLeft].Position);

            //double rightThreshold = 0.2;
            //double leftThreshold = 0.1;
            //double leftMax = joints[JointType.ShoulderLeft].Position.X - MainWindow.armLength + MainWindow.armLength * rightThreshold;
            //double rightMax = joints[JointType.ShoulderLeft].Position.X - MainWindow.armLength * leftThreshold;

            //Pen guidePen = new Pen(Brushes.Wheat, 2);
            //int guideHeight = 14;
            //double[] guideLinesX = new double[] { 1, 0.91, 0.75, 0.58, 0.41, 0.24, 0.08 };
            //for (int i = 0; i < guideLinesX.Length; i++)
            //{
            //    drawingContext.DrawLine(guidePen, new Point(sl.X - armLength * guideLinesX[i], sl.Y - guideHeight / 2), new Point(sl.X - armLength * guideLinesX[i], sl.Y + guideHeight / 2));
            //}

            // Base horizontal line
            //drawingContext.DrawLine(guidePen, new Point(sl.X, sl.Y), new Point(sl.X - armLength, sl.Y));


            // Line on the hand
            //drawingContext.DrawLine(new Pen(Brushes.Chartreuse, 1), new Point(wl.X, wl.Y + 30), new Point(wl.X, wl.Y - 30));

                   
            

        }

        /// <summary>
        /// Draws one bone of a body (joint to joint)
        /// </summary>
        /// <param name="joints">joints to draw</param>
        /// <param name="jointPoints">translated positions of joints to draw</param>
        /// <param name="jointType0">first joint of bone to draw</param>
        /// <param name="jointType1">second joint of bone to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// /// <param name="drawingPen">specifies color to draw a specific bone</param>
        private void DrawBone(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, JointType jointType0, JointType jointType1, DrawingContext drawingContext, Pen drawingPen)
        {
            Joint joint0 = joints[jointType0];
            Joint joint1 = joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == TrackingState.NotTracked ||
                joint1.TrackingState == TrackingState.NotTracked)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if ((joint0.TrackingState == TrackingState.Tracked) && (joint1.TrackingState == TrackingState.Tracked))
            {
                drawPen = drawingPen;
            }

            drawingContext.DrawLine(drawPen, jointPoints[jointType0], jointPoints[jointType1]);
        }

        /// <summary>
        /// Draws a hand symbol if the hand is tracked: red circle = closed, green circle = opened; blue circle = lasso
        /// </summary>
        /// <param name="handState">state of the hand</param>
        /// <param name="handPosition">position of the hand</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawHand(HandState handState, Point handPosition, DrawingContext drawingContext)
        {
            switch (handState)
            {
                case HandState.Closed:
                    drawingContext.DrawEllipse(this.handClosedBrush, null, handPosition, HandSize, HandSize);
                    break;

                case HandState.Open:
                    drawingContext.DrawEllipse(this.handOpenBrush, null, handPosition, HandSize, HandSize);
                    break;

                case HandState.Lasso:
                    drawingContext.DrawEllipse(this.handLassoBrush, null, handPosition, HandSize, HandSize);
                    break;
            }
        }

        /// <summary>
        /// Draws indicators to show which edges are clipping body data
        /// </summary>
        /// <param name="body">body to draw clipping information for</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawClippedEdges(Body body, DrawingContext drawingContext)
        {
            FrameEdges clippedEdges = body.ClippedEdges;

            if (clippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, this.displayHeight - ClipBoundsThickness, this.displayWidth, ClipBoundsThickness));
            }

            if (clippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, this.displayWidth, ClipBoundsThickness));
            }

            if (clippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, this.displayHeight));
            }

            if (clippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(this.displayWidth - ClipBoundsThickness, 0, ClipBoundsThickness, this.displayHeight));
            }
        }
    }
}
