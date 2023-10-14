function onReceiveDeviceList(payloadUnprocessed) {
  const devices = treatPayloadAsJson(payloadUnprocessed);
  let activeDeviceId = addDevicesToSelector(devices);
  registerAllEventsForDevice(activeDeviceId);
}

function registerAllEventsForDevice(activeDeviceId) {
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
      console.log("updated devices");
      break;

    case "update_devicePeakValue":
      const [updatedDeviceId, peakValue] =
        treatPayloadAsCsv(payloadUnprocessed);
      if (updatedDeviceId == deviceSelect.value) {
        devicePeakMeterBar.style.width = `${peakValue * 100}%`;
      }
      break;

    case "update_deviceVolume":
      const [updatedDeviceId2, muted, volume] =
        treatPayloadAsCsv(payloadUnprocessed);
      if (updatedDeviceId2 == deviceSelect.value) {
        deviceVolInput.value = Number.parseInt(volume);
        deviceVolIndicator.innerHTML = volume;
        muteDeviceBtn.innerText = muted == "True" ? "🔇" : "🔊";
      }
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

activateDeviceBtn.addEventListener("click", () => {
  socket.emit(
    "winAudio",
    "setActiveDevice",
    deviceSelect.value,
    (payloadUnprocessed) => {
      if (payloadUnprocessed == null) {
        console.log(payloadUnprocessed);
        addLineToTerminal("Error calling setActiveDevice");
        return;
      }

      addLineToTerminal("setActiveDevice: " + payloadUnprocessed);
    }
  );
});

deviceSelect.addEventListener("change", () => {
  socket.emit("winAudio", "getOutputDevices", "", (payloadUnprocessed) => {
    if (payloadUnprocessed == null) {
      console.log(payloadUnprocessed);
      addLineToTerminal("Error calling getOutputDevices");
      return;
    }

    const devices = treatPayloadAsJson(payloadUnprocessed);
    const device = devices.find(
      (device) => device.deviceId == deviceSelect.value
    );

    registerAllEventsForDevice(device.deviceId);

    deviceVolInput.value = `${device.volumePercent}`;
    deviceVolIndicator.innerText = `${device.volumePercent}`;
    console.log(device);
    muteDeviceBtn.innerText = device.muted ? "🔇" : "🔊";
    console.log(device.deviceId);
  });
});

deviceVolInput.addEventListener("input", () => {
  changeDeviceMuteOrVolume(false);
});

muteDeviceBtn.addEventListener("click", () => {
  switch (muteDeviceBtn.innerText) {
    case "🔊":
      muteDeviceBtn.innerText = "🔇";
      break;
    case "🔇":
      muteDeviceBtn.innerText = "🔊";
      break;
  }
  changeDeviceMuteOrVolume(false);
});

function changeDeviceMuteOrVolume(confirmVolumeChange = true) {
  const newVol = deviceVolInput.value;
  deviceVolIndicator.innerText = newVol;

  socket.emit(
    "winAudio",
    "setDeviceVolume",
    `${deviceSelect.value},${
      muteDeviceBtn.innerText == "🔇"
    },${newVol},${confirmVolumeChange}`,
    (payloadUnprocessed) => {
      if (confirmVolumeChange) {
        if (payloadUnprocessed == null) {
          console.log(payloadUnprocessed);
          addLineToTerminal("Error calling getOutputDevices");
          return;
        }

        addLineToTerminal("setDeviceVolume: " + payloadUnprocessed);
      }
    }
  );
}

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
