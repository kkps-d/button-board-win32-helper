socket.on("connect", () => {
  addLineToTerminal("Connected to web server");

  socket.emit("winAudio", "getOutputDevices", "", (payloadUnprocessed) => {
    if (payloadUnprocessed == null) {
      console.log(payloadUnprocessed);
      addLineToTerminal("Error calling getOutputDevices");
      return;
    }
    const devices = treatPayloadAsJson(payloadUnprocessed);
    let activeDeviceId = addDevicesToSelector(devices);
    socket.emit(
      "winAudio",
      "getAudioSessions",
      activeDeviceId,
      (payloadUnprocessed2) => {
        if (payloadUnprocessed2 == null) {
          addLineToTerminal("Error calling getAudioSessions");
          return;
        }

        let sessions = treatPayloadAsJson(payloadUnprocessed2);
        createSessionElements(sessions);
      }
    );

    socket.emit(
      "winAudio",
      "receiveDeviceListUpdates",
      "true",
      (payloadUnprocessed) => {
        if (payloadUnprocessed == null) {
          console.log(payloadUnprocessed);
          addLineToTerminal("Error calling receiveDeviceListUpdates");
          return;
        }

        addLineToTerminal("receiveDeviceListUpdates: " + payloadUnprocessed);
      }
    );
  });
});

socket.on("winAudio", (message) => {
  const [event, payloadUnprocessed] = splitMessage(message);

  switch (event) {
    case "update_deviceList":
      const devices = treatPayloadAsJson(payloadUnprocessed);
      let activeDeviceId = addDevicesToSelector(devices);
      socket.emit(
        "winAudio",
        "getAudioSessions",
        activeDeviceId,
        (payloadUnprocessed2) => {
          if (payloadUnprocessed2 == null) {
            addLineToTerminal("Error calling getAudioSessions");
            return;
          }

          let sessions = treatPayloadAsJson(payloadUnprocessed2);
          createSessionElements(sessions);
        }
      );
      break;

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
