const socket = io();

socket.onAny((target, data) => {
  // addLineToTerminal(`Received: ${target},${data}`);
});
