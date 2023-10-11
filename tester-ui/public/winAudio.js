socket.on("connect", () => {
  addLineToTerminal("Connected to web server");

  socket.emit("winAudio", "getOutputDevices", (payload) => {
    const devices = treatPayloadAsJson(payloadUnprocessed);
    deviceSelect.innerHTML = "";
    let activeDeviceId;
    for (let device of devices) {
      let deviceOption = document.createElement("option");
      deviceOption.innerText = device.friendlyName;
      deviceOption.value = device.deviceId;
      if (device.selected) {
        activeDeviceId = device.deviceId;
        deviceOption.selected = true;
      }
      deviceSelect.appendChild(deviceOption);
    }
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
  const firstComma = message.indexOf(",");
  const event = message.slice(0, firstComma);
  const payloadUnprocessed = message.slice(firstComma + 1);

  return [event, payloadUnprocessed];
}

function treatPayloadAsCsv(payload) {}

function treatPayloadAsJson(payload) {
  return JSON.parse(payload);
}
