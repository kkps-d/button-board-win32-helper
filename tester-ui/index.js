const express = require("express");
const { createServer } = require("node:http");
const app = express();
const server = createServer(app);
app.use(express.static("public"));

const { Server } = require("socket.io");
const io = new Server(server);

const WEB_PORT = 3001;
const HELPER_PORT = 3124;

server.listen(WEB_PORT, () => {
  console.log("Web server listening on http://localhost:" + WEB_PORT);
});

io.on("connection", (socket) => {
  console.log("Web UI connected");

  socket.emit("hello");

  socket.onAny((eventName, ...args) => {
    console.log(`Message from web: ${eventName}, ${args}`);
  });
});
