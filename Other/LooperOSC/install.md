**Required installation for the looper to work**

1. Copy midifile.class files in ```Max/Cycling '74/java/classes```.

2. Alternatively, install it in another directory and add the line ```max.dynamic.class.dir <path_to_classes>``` to ```Max/Cycling '74/java/max.java.config.txt```: 

3. Copy ```OSC-route.mxe``` to ```Max\Cycling '74\max-externals```

These files are taken from the CNMAT repository, and is required for LooperOSC to work (adds the Route-OSC element).