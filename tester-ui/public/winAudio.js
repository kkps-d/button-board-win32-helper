function onReceiveDeviceList(payloadUnprocessed) {
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

  socket.emit(
    "winAudio",
    "receiveDevicePeakValueUpdates",
    `${activeDeviceId},true`,
    (payloadUnprocessed) => {
      if (payloadUnprocessed == null) {
        console.log(payloadUnprocessed);
        addLineToTerminal("Error calling receiveDevicePeakValueUpdates");
        return;
      }

      addLineToTerminal("receiveDevicePeakValueUpdates: " + payloadUnprocessed);
    }
  );

  socket.emit(
    "winAudio",
    "receiveDeviceVolumeUpdates",
    `${activeDeviceId},true`,
    (payloadUnprocessed) => {
      if (payloadUnprocessed == null) {
        console.log(payloadUnprocessed);
        addLineToTerminal("Error calling receiveDeviceVolumeUpdates");
        return;
      }

      addLineToTerminal("receiveDeviceVolumeUpdates: " + payloadUnprocessed);
    }
  );

  socket.emit(
    "winAudio",
    "receiveAudioSessionUpdates",
    `${activeDeviceId},true`,
    (payloadUnprocessed) => {
      if (payloadUnprocessed == null) {
        console.log(payloadUnprocessed);
        addLineToTerminal("Error calling receiveAudioSessionUpdates");
        return;
      }

      addLineToTerminal("receiveAudioSessionUpdates: " + payloadUnprocessed);
    }
  );
}

socket.on("connect", () => {
  addLineToTerminal("Connected to web server");

  socket.emit("winAudio", "getOutputDevices", "", (payloadUnprocessed) => {
    if (payloadUnprocessed == null) {
      console.log(payloadUnprocessed);
      addLineToTerminal("Error calling getOutputDevices");
      return;
    }

    onReceiveDeviceList(payloadUnprocessed);
  });
});

socket.on("winAudio", (message) => {
  const [event, payloadUnprocessed] = splitMessage(message);

  switch (event) {
    case "update_deviceList":
      onReceiveDeviceList(payloadUnprocessed);
      addLineToTerminal("updated devices");
      break;

    case "update_devicePeakValue":
      const peakValue = treatPayloadAsCsv(payloadUnprocessed)[1];
      devicePeakMeterBar.style.width = `${peakValue * 100}%`;
      break;

    case "update_deviceVolume":
      const volume = treatPayloadAsCsv(payloadUnprocessed)[2];
      deviceVolInput.value = Number.parseInt(volume);
      deviceVolIndicator.innerHTML = volume;
      break;

    case "update_audioSessions":
      let [deviceId, ...theRest] = treatPayloadAsCsv(payloadUnprocessed);
      // If deviceId is same as selected device on UI, update the sessions display
      if (deviceId == deviceSelect.value) {
        addLineToTerminal(
          "updated sessions for device " +
            deviceSelect.options[deviceSelect.selectedIndex].innerHTML
        );
      }

      let sessions = treatPayloadAsJson(theRest.join(","));
      createSessionElements(sessions);
      break;

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

function treatPayloadAsCsv(payload) {
  return payload.split(",");
}

function treatPayloadAsJson(payload) {
  return JSON.parse(payload);
}
