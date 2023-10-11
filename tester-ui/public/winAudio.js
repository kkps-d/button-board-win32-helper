socket.on("connect", () => {
  addLineToTerminal("Connected to web server");

  socket.emit("winAudio", "getOutputDevices", "", (payloadUnprocessed) => {
    const devices = treatPayloadAsJson(payloadUnprocessed);
    addDevicesToSelector(devices);
  });
});

socket.on("winAudio", (message) => {
  const [event, payloadUnprocessed] = splitMessage(message);

  switch (event) {
    default:
      break;
  }
});

function splitMessage(message) {
  console.log(message);
  const firstComma = message.indexOf(",");
  const event = message.slice(0, firstComma);
  const payloadUnprocessed = message.slice(firstComma + 1);

  return [event, payloadUnprocessed];
}

function treatPayloadAsCsv(payload) {}

function treatPayloadAsJson(payload) {
  return JSON.parse(payload);
}
