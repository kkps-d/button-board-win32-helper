// Device
const deviceSelect = document.getElementById("device-select");
const activateDeviceBtn = document.getElementById("activate-device");
const deviceVolRange = document.getElementById("device-vol-range");
const muteDeviceBtn = deviceVolRange.querySelector("button");
const deviceVolInput = deviceVolRange.querySelector("input");
const deviceVolIndicator = deviceVolRange.querySelector(".indicator");
const devicePeakMeterBar = deviceVolRange.querySelector(".peak-meter-bar");

function addDevicesToSelector(devices) {
  deviceSelect.innerHTML = "";
  let activeDeviceId;
  for (let device of devices) {
    let deviceOption = document.createElement("option");
    deviceOption.innerText = device.friendlyName;
    deviceOption.value = device.deviceId;
    if (device.selected) {
      activeDeviceId = device.deviceId;
      deviceOption.selected = true;
      deviceVolInput.value = `${device.volumePercent}`;
      deviceVolIndicator.innerText = `${device.volumePercent}`;
    }
    deviceSelect.appendChild(deviceOption);
  }
  return activeDeviceId;
}

// Terminal
const terminalDiv = document.getElementById("terminal");
const terminalHeight = 20;
let lines = new Array(terminalHeight).fill("~");
renderTerminal();
function addLineToTerminal(line = "") {
  lines.shift();
  lines.push(line);
  renderTerminal();
}
function renderTerminal() {
  terminalDiv.innerHTML = lines.join("<br/>");
}

// Rows
const rowsDiv = document.getElementById("rows");
