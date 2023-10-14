# button-board-win32-helper
A helper program for win32 functionality that communicates with the button-board server. This program communicates with the main process through Windows sockets.  
The port can be configured by editing the `helperPort` property in `buttonboard-config.json`.

# Dev TODO
- [ ] Implement socket server to send events
- [ ] Implement loops to poll for:
  - [ ] Audio device peak meters
  - [ ] Audio session peak meters
- [ ] Implement sndvol style web interface for testing
- [ ] Try to get Github Actions working to:
  - [ ] Build the executable
  - [ ] Do releases
- [x] Add VS project to Git
- [x] Create notify and callbacks in CoreAudio for:
  - [x] Audio device changes (default device changed)
  - [x] Audio device volume & mute status updates
  - [x] Audio session creation updates
  - [x] Audio session volume & mute updates
  - [x] Audio session invalidation (deleted/disconnected/expired) updates
- [x] Add to API documentation:
  - [x] Audio device add/remove update
  - [x] Audio device mute status update
  - [x] Audio session mute status update
- [x] Change to API documentation:
  - [x] Add `receiveAudioSessionUpdates` to `WinAudioDevice`. We may not be interested in getting updates for the device that is not active

# API
- Below are the messages and payloads used to communicate with the helper program.  
- JSON Types are annotated using brackets `{ }`, while text messages have no annotations.  
  - The justification for using just plaintext is to reduce overhead when serializing/deserializing and transmitting messages that are usually spammed, like volume changes or peak meter updates.  
- All messages must be provided with a unique int as the first argument in a payload that does not exceed Int32.Max. This number is passed back unchanged in a response message, and is useful for identifying the results of a previous message. If the payload is JSON, the ID must NOT be part of the JSON text, and must be before it.
  - Messages that initially sent by the helper (e.g. updates) will not contain the message ID

## `WinAudio`

### To helper program

#### `getOutputDevices` ✅
```
payload = msgId: number
```

#### `receiveDeviceListUpdates` ✅
```
payload = msgId: number, value: boolean
```

### From helper program
#### `return_getOutputDevices` ✅
```
payload = msgId: number, [{
  deviceId: string,
  friendlyName: string,
  volumePercent: int (0 - 100),
  muted: boolean,
  selected: boolean
}]
```

#### `return_receiveDeviceListUpdates` ✅
```
payload = msgId: number, newState: boolean
```

#### `update_deviceList` ✅
```
payload = [{
  deviceId: string,
  friendlyName: string,
  volumePercent: int (0 - 100),
  muted: boolean,
  selected: boolean
}]
```

## `WinAudioDevice`

### To helper program

#### `receiveDevicePeakValueUpdates` ✅
```
payload = msgId: number, deviceId: string, value: boolean
```

#### `receiveDeviceVolumeUpdates` ✅
```
payload = msgId: number, deviceId: string, value: boolean
```

#### `receiveAudioSessionUpdates` ✅
```
payload = msgId: number, deviceId: string, value: boolean
```

#### `getAudioSessions` ✅
```
payload = msgId: number, deviceId: string
```

#### `setActiveDevice` ✅
```
payload = msgId: number, deviceId: string
```

#### `setDeviceVolume`
```
payload = msgId: number, deviceId: string, muted: boolean, volumePercent: int (0 - 100), confirmVolumeChange: boolean
- 'confirmVolumeChange' supresses 'return_setDeviceVolume' if false.
- This method also does not fire 'update_deviceVolume'.
```

### From helper program

#### `return_receiveDevicePeakValueUpdates` ✅
```
payload = msgId: number, deviceId: string, newState: boolean
```

#### `update_devicePeakValue` ✅
```
payload = deviceId: string, newPeakValue: float (0 - 1)
```

#### `return_receiveDeviceVolumeUpdates` ✅
```
payload = msgId: number, deviceId: string, newState: boolean
```

#### `update_deviceVolume` ✅
```
payload = deviceId: string, newMuted: boolean, newVolumePercent: int (0 - 100)
```

#### `return_receiveAudioSessionUpdates` ✅
```
payload = msgId: number, deviceId: string, newState: boolean
```

#### `update_audioSessions` ✅
```
payload = deviceId: string, [{
  sessionId: string,
  friendlyName: string,
  iconPath: string | null,
  volumePercent: int (0 - 100),
  muted: boolean
}]
```

#### `return_getAudioSessions` ✅
```
payload = [{
  sessionId: string,
  friendlyName: string,
  iconPath: string | null,
  volumePercent: int (0 - 100),
  muted: boolean
}] | null
```

#### `return_setActiveDevice` ✅
```
payload = msgId: number, currentActiveDeviceId: string
```

#### `return_setDeviceVolume`
```
payload = msgId: number, deviceId: string, newMuted: boolean, newVolumePercent: int (0 - 100)
- Supressed if 'setDeviceVolume' is called with 'confirmValueChange = false'
```

## `WinAudioSession`

### To helper program

#### `receiveSessionPeakValueUpdates`
```
payload = msgId: number, sessionId: string, value: boolean
```

#### `receiveSessionVolumeUpdates`
```
payload = msgId: number, sessionId: string, value: boolean
```

#### `setSessionVolume`
```
payload = msgId: number, sessionId: string, muted: boolean, volumePercent: int (0 - 100), confirmVolumeChange: boolean
- 'confirmVolumeChange' supresses 'return_setSessionVolume' if false.
- This method also does not fire 'update_sessionVolume'.
```

### From helper program

#### `return_receiveSessionPeakValueUpdates`
```
payload = msgId: number, sessionId: string, newState: boolean
```

#### `update_sessionPeakValue`
```
payload = sessionId: string, newPeakValue: float (0 - 1)
```

#### `return_receiveSessionVolumeUpdates`
```
payload = msgId: number, sessionId: string, newState: boolean
```

#### `update_sessionVolume`
```
payload = sessionId: string, newMuted: boolean, newVolume: int (0 - 100)
```

#### `return_setSessionVolume`
```
payload = msgId: number, sessionId: string, newMuted: boolean, newVolume: int (0 - 100)
```
