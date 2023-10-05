# button-board-win32-helper
A helper program for win32 functionality that communicates with the button-board server. This program communicates with the main process through Windows sockets.
The port can be configured by editing the `helperPort` property in `buttonboard-config.json`.

# API
Below are the messages and payloads used to communicate with the helper program.  
JSON Types are annotated using brackets `{ }`, while text messages have no annotations.

## `WinAudio`

### To helper program

`getDefaultOutputDevice`
- **Payload:** None

`getOutputDevices`
- **Payload:** None

### From helper program
`return_getDefaultOutputDevice`
- **Payload:** `{ deviceId, friendlyName }`

`return_getOutputDevices`
- **Payload:** `{ devices: [{deviceId, friendlyName}] }`

## `WinAudioDevice`

### To helper program

`getDevice`
- **Payload:** `deviceId`

`receivePeakValueUpdates`
- **Payload:** `true | false`

`receiveVolumeUpdates`
- **Payload:** `true | false`

`getAudioSessions`
- **Payload:** `{ deviceId, audioSessions: [{ sessionId, friendlyName, iconPath, volumePercent, muted, peakValue }] }`

### From helper program

`return_getDevice`
- **Payload:** `{ deviceId, friendlyName, volumePercent, muted, peakValue, selected }`

`return_receivePeakValueUpdates`
- **Payload:** `deviceId,true | false`
- Returns the new state

`peakValueUpdate`
- **Payload:** `deviceId,newPeakValue`
- Note: This is intentionally not a JSON string to save some overhead

`return_receiveVolumeUpdates`
- **Payload:** `deviceId,true | false`
- Returns the new state

`volumeUpdate`
- **Payload:** `deviceId,newVolume`
- Note: This is intentionally not a JSON string to save some overhead

`
