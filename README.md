# button-board-win32-helper
A helper program for win32 functionality that communicates with the button-board server. This program communicates with the main process through Windows sockets.
The port can be configured by editing the `helperPort` property in `buttonboard-config.json`.

# API
JSON Types are annotated using brackets `{ }`, while text messages have no annotations.
## `WinAudio`
### Getters
`getDefaultOutputDevice`
- `in` - None
- `out` - `{ id, friendlyName }`
`getOutputDevices`
- `in` - None
- `out` - `{ devices: [{id, friendlyName}] }`
### Setters
None
