Gestural Music
=============

The project allows a single user to create music through gestures using the Kinect v2 and Ableton Live. The system supports MIDI note generation, looping, partitioning the physical space for control of up to four separate instruments at once, and more, allowing for a user to build their own virtual band.

⚠️ _This project is no longer maintained. You can reach out to the author for more information, to which I will do my best to reply._

## 1. Getting Started

This project requires the following:

* Windows 8 or 10
* [Kinect for Windows SDK 2.0](http://www.microsoft.com/en-us/kinectforwindows/develop/)
* [Visual Studio](http://www.visualstudio.com/)
* [Ableton Live](https://www.ableton.com/en/live/new-in-9/)
* [Max for Live](https://www.ableton.com/en/live/max-for-live/)
* Kinect v2

Unfortunately, the project is sensitive to the requirements of the Kinect SDK and may not run on all devices. Tested using Visual Studio 2013 and 2015.

## 2. Installation

1. Clone the repository to your local machine
2. Copy [Other/LooperOSC/midifile.class](Other/LooperOSC/midifile.class) to ```<Max_Directory>/Max/Cycling '74/java/classes```
  1. Alternatively, you can copy it to another directory and add the line ```max.dynamic.class.dir <path_to_class>``` to ```<Max_Directory>/Max/Cycling '74/java/max.java.config.txt```
3. Copy [Other/LooperOSC/OSC-route.mxe](Other/LooperOSC/OSC-route.mxe) to ```<Max_Directory>/Max\Cycling '74\max-externals```
4. You also need to install Emgu CV. You may have to copy some of the dll's to your build/bin directory in order for calibration to work.

## 3. Running

The [test project](Other/test Project/test.als) should be configured and ready to go, so start the Kinect app and the Ableton file, step back and enjoy!

### Controls
The 3D projection controls can be a bit confusing at first. The goal is align the camera such that the projection screen is displaying a non-distorted representation of the stage area. The camera is used to estimate the projector's pose.

The WASD controls are used to move the camera location along the X and Y planes, Q and E raise and lower the camera, and the middle mouse click can be used to adjust the look direction.

## 4. Video

- Version 2.0 demonstration: https://www.youtube.com/watch?v=5LAG0SH2lwg
- Version 1.0 demonstration: https://www.youtube.com/watch?v=NU2ZUjyDzuI


*For more information, please see the Documentation folder.*
