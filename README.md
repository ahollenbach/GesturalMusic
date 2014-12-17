Gestural Music
=============

The project allows a single user to create music through gestures using the Kinect v2 and Ableton Live. The system supports MIDI note generation, looping, partitioning the physical space for control of up to four separate instruments at once, and more, allowing for a user to build their own virtual band.


1. Getting Started
------------------
This project requires the following:

* Windows 8, Windows 8.1, or Windows Embedded Standard 8
* [Kinect for Windows SDK 2.0 Public Preview](http://www.microsoft.com/en-us/kinectforwindows/develop/)
* [Visual Studio](http://www.visualstudio.com/)
* [Ableton Live](https://www.ableton.com/en/live/new-in-9/)
* [Max for Live](https://www.ableton.com/en/live/max-for-live/)

2. Installation
--------------------
1. Clone the repository to your local machine
2. Copy [Other/LooperOSC/midifile.class](Other/LooperOSC/midifile.class) to ```<Max_Directory>/Max/Cycling '74/java/classes```
  1. Alternatively, you can copy it to another directory and add the line ```max.dynamic.class.dir <path_to_class>``` to ```<Max_Directory>/Max/Cycling '74/java/max.java.config.txt```
3. Copy [Other/LooperOSC/OSC-route.mxe](Other/LooperOSC/OSC-route.mxe) to ```<Max_Directory>/Max\Cycling '74\max-externals```

3. Running
--------------------
The [test project](Other/test Project/test.als) should be configured and ready to go, so start the Kinect app and the Ableton file, step back and enjoy!
