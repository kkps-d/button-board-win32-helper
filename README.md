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

`receiveDevicePeakValueUpdates`
- **Payload:** `true | false`

`receiveDeviceVolumeUpdates`
- **Payload:** `true | false`

`getAudioSessions`
- **Payload:** `deviceId`

`setActiveDevice`
- **Payload:** `deviceId`

`setDeviceVolume`
- **Payload:** `deviceId,volume,true | false`

`setDeviceMute`
- **Payload:** `deviceId,true | false`

### From helper program

`return_getDevice`
- **Payload:** `{ deviceId, friendlyName, volumePercent, muted, peakValue, selected }`

`return_receiveDevicePeakValueUpdates`
- **Payload:** `deviceId,true | false`
- Returns the new state

`update_devicePeakValue`
- **Payload:** `deviceId,newPeakValue`
- Note: This is intentionally not a JSON string to save some overhead

`return_receiveDeviceVolumeUpdates`
- **Payload:** `deviceId,true | false`
- Returns the new state

`update_deviceVolume`
- **Payload:** `deviceId,newVolume`
- Note: This is intentionally not a JSON string to save some overhead

`return_getAudioSessions`
- **Payload:** `{ deviceId, audioSessions: [{ sessionId, friendlyName, iconPath, volumePercent, muted, peakValue }] }`

`update_audioSessions`
- **Payload:** `{ deviceId, audioSessions: [{ sessionId, friendlyName, iconPath, volumePercent, muted, peakValue }] }`
- Identical to `return_getAudioSessions`. Should they be handled the same?

`return_setActiveDevice`
- **Payload:** `deviceId,true | false`
- Returns the new state

`return_setDeviceVolume`
- **Payload:** `deviceId,newVolume`
- Identical to `update_deviceVolume`. Definitely should be handled the same.

`return_setDeviceMute`
- **Payload:** `deviceId,true | false`
- Returns the new state

## `WinAudioSession`

### To helper program

`receiveSessionPeakValueUpdates`
- **Payload:** `true | false`

`receiveSessionVolumeUpdates`
- **Payload:** `true | false`

`setSessionVolume`
- **Payload:** `sessionId,volume,true | false`

`setSessionMute`
- **Payload:** `sessionId,true | false`

### From helper program

`return_receiveSessionPeakValueUpdates`
- **Payload:** `sessionId,true | false`
- Returns the new state

`update_sessionPeakValue`
- **Payload:** `sessionId,newPeakValue`
- Note: This is intentionally not a JSON string to save some overhead

`return_receiveSessionVolumeUpdates`
- **Payload:** `sessionId,true | false`
- Returns the new state

`update_sessionVolume`
- **Payload:** `sessionId,newVolume`
- Note: This is intentionally not a JSON string to save some overhead

`return_setSessionVolume`
- **Payload:** `sessionId,newVolume`
- Identical to `update_sessionPeakValue`. Definitely should be handled the same.

`return_setSessionMute`
- **Payload:** `sessionId,true | false`
- Returns the new state
