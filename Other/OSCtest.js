// Requires OSC Receiver (https://www.npmjs.org/package/osc-receiver)

var OscReceiver = require('osc-receiver'), receiver = new OscReceiver();
console.log("OSC receiver started...");
receiver.bind(8000);

receiver.on('/test', function(a) {
	console.log(a);
});