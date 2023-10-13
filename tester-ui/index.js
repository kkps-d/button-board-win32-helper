const path = require("node:path");
const express = require("express");
const { createServer } = require("node:http");
const app = express();
const server = createServer(app);
app.use(express.static("public"));
app.use(
  "/icons",
  express.static(
    path.join(
      __dirname,
      "..",
      "Win32Helper",
      "bin",
      "Debug",
      "net6.0-windows10.0.17763.0",
      "resources"
    )
  )
);

const { Server } = require("socket.io");
const io = new Server(server);

const WEB_PORT = 3001;
const HELPER_PORT = 3124;

const net = require("net");
var connection = net.createConnection(HELPER_PORT, "0.0.0.0", () =>
  console.log("Win32 Helper connected")
);

server.listen(WEB_PORT, () => {
  console.log("Web server listening on http://localhost:" + WEB_PORT);
});

const int32Max = 2147483647;
let messageNum = 0;
function getMessageNum() {
  let num = messageNum++;
  if (messageNum > int32Max) messageNum = 0;
  return num;
}

io.on("connection", (socket) => {
  console.log("Web UI connected");

  socket.off("winAudio", socketIoHandlerFunction);
  socket.on("winAudio", socketIoHandlerFunction);

  connection.removeListener("data", helperDataHandlerFunction);
  connection.on("data", helperDataHandlerFunction);
});

function socketIoHandlerFunction(message, payload, callback) {
  console.log(
    `Message for 'winAudio': '${message}', ${
      payload || "no payload"
    }, '${callback}'`
  );

  sendMessageToHelperAndWait(callback, message, payload);
}

// What goes in?
//  The callback, message and payload
// What comes out?
//  The results of that message
// How do you do it?
//  In caller function
// 1. Get the message num
// 2. Build the string with the message num
// 3. Send the message to the helper
// 4. Track in a dict with key being message number and value being the callback for the socket.io ack
// 5. Set a timeout to call the related callback with value null and delete the key from the dict
//  In connection.on
// 1. If results are returned, process and call the callback in dict with those values
// 2. Clear the timeout too
const functionsWaiting = {};
function sendMessageToHelperAndWait(callback, message, payload) {
  let msgNum = getMessageNum();
  connection.write(`${message},${msgNum}${payload ? `,${payload}` : ""};`);
  let timeout = setTimeout(() => {
    functionsWaiting[msgNum].callback(null);
    delete functionsWaiting[msgNum];
  }, 2000);
  functionsWaiting[msgNum] = { callback, timeout };
}

// Helper data handler does two things:
// - Handles return_ data using the explanation above, send just the payload to web ui via callback
// - Sends update_ data by removing the message number and sending via io.emit
function helperDataHandlerFunction(data) {
  // Split message and filter out empty messages
  // Empty messages are usually artifacts of splitting single messages
  // message; = is split into => ['message', '']
  let messages = data
    .toString()
    .split(";")
    .filter((message) => message.length > 0);

  for (let message of messages) {
    let splitMessage = message.split(",");
    let messageType = splitMessage[0];

    if (messageType.startsWith("return_")) {
      let msgNum = Number.parseInt(splitMessage[1]);
      let payload = splitMessage.slice(2).join(",");
      functionsWaiting[msgNum].callback(payload);
      clearTimeout(functionsWaiting[msgNum].timeout);
    } else if (messageType.startsWith("update_")) {
      io.emit("winAudio", message);
    } else {
      console.log("potentially corrupted data");
      console.log(data.toString());
    }
  }
}
