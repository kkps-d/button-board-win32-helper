// Device
const deviceSelect = document.getElementById("device-select");
const activateDeviceBtn = document.getElementById("activate-device");
const deviceVolRange = document.getElementById("device-vol-range");
const muteDeviceBtn = deviceVolRange.querySelector("button");
const deviceVolInput = deviceVolRange.querySelector("input");
const devicePeakMeterBar = deviceVolRange.querySelector(".peak-meter-bar");

// Terminal
const terminalDiv = document.getElementById("terminal");
const terminalHeight = 5;
let lines = new Array(terminalHeight).fill("~");
renderTerminal();
function addLineToTerminal(line) {
  lines.shift();
  lines.push(line);
  renderTerminal();
}
function renderTerminal() {
  terminalDiv.innerHTML = lines.join("<br/>");
}

// Rows
const rowsDiv = document.getElementById("rows");
