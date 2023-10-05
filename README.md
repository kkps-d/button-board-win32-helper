# button-board-win32-helper
A helper program for win32 functionality that communicates with the button-board server. This program communicates with the main process through Windows sockets.  
The port can be configured by editing the `helperPort` property in `buttonboard-config.json`.

# Dev TODO
- [ ] Create notify and callbacks in CoreAudio for:
  - [ ] Audio device changes
  - [ ] Audio device mute status updates
  - [ ] Audio session updates
  - [ ] Audio session volume updates
  - [ ] Audio session mute status updates
- [ ] Implement loops to poll for:
  - [ ] Audio device peak meters
  - [ ] Audio session peak meters
- [ ] Add to API documentation:
  - [ ] Audio device add/remove update
  - [ ] Audio device mute status update
  - [ ] Audio session mute status update
- [ ] Change to API documentation:
  - [ ] Add `receiveAudioSessionUpdates` to `WinAudioDevice`. We may not be interested in getting updates for the device that is not active
- [ ] Add VS project to Git
- [ ] Try to get Github Actions working to:
  - [ ] Build the executable
  - [ ] Do releases

# API
- Below are the messages and payloads used to communicate with the helper program.  
- JSON Types are annotated using brackets `{ }`, while text messages have no annotations.  
  - The justification for using just plaintext is to reduce overhead when serializing/deserializing and transmitting messages that are usually spammed, like volume changes or peak meter updates.  
- Messages with no payloads have no code blocks under them

## `WinAudio`

### To helper program

- `getDefaultOutputDevice`
- `getOutputDevices`

### From helper program
- `return_getDefaultOutputDevice`  
```
payload = { deviceId: string, friendlyName: string }
```

- `return_getOutputDevices`
```
payload = { 
  devices: [{ deviceId: string, friendlyName: string }]
}
```

## `WinAudioDevice`

### To helper program

- `getDevice`
```
payload = deviceId: string
```

- `receiveDevicePeakValueUpdates`
```
payload = deviceId: string, value: boolean
```

- `receiveDeviceVolumeUpdates`
```
payload = deviceId: string, value: boolean
```

- `getAudioSessions`
```
payload = deviceId: string
```

- `setActiveDevice`
```
payload = deviceId: string
```

- `setDeviceVolume`
```
payload = deviceId: string, volumePercent: int (0 - 100), confirmVolumeChange: boolean
- 'confirmVolumeChange' supresses 'return_setDeviceVolume' if false.
- This method also does not fire 'update_deviceVolume'.
```

- `setDeviceMute`
```
payload = deviceId: string, value: boolean
```

### From helper program

- `return_getDevice`
```
payload = {
  deviceId: string,
  friendlyName: string,
  volumePercent: int (0 - 100),
  muted: boolean,
  selected: boolean
}
```

- `return_receiveDevicePeakValueUpdates`
```
payload = deviceId: string, newState: boolean
```

- `update_devicePeakValue`
```
payload = deviceId: string, newPeakValue: float (0 - 1)
```

- `return_receiveDeviceVolumeUpdates`
```
payload = deviceId: string, newState: boolean
```

- `update_deviceVolume`
```
payload = deviceId: string, newVolumePercent: int (0 - 100)
```

- `return_getAudioSessions`
```
payload = {
  deviceId: string,
  audioSessions: [{
    sessionId: string,
    friendlyName: string,
    iconPath: string | null,
    volumePercent: int (0 - 100),
    muted: boolean
  }]
}
```

- `update_audioSessions`
```
payload = {
  deviceId: string,
  audioSessions: [{
    sessionId: string,
    friendlyName: string,
    iconPath: string | null,
    volumePercent: int (0 - 100),
    muted: boolean
  }]
}
- Identical to 'return_getAudioSessions'. Should they be handled the same?
```

- `return_setActiveDevice`
```
payload = deviceId: string, newState: boolean
```

- `return_setDeviceVolume`
```
payload = deviceId: string, newVolumePercent: int (0 - 100)
- Identical to 'update_deviceVolume'. Should they be handled the same?
- Supressed if 'setDeviceVolume' is called with 'confirmValueChange = false'
```

- `return_setDeviceMute`
```
payload = deviceId: string, newState: boolean
```

## `WinAudioSession`

### To helper program

- `receiveSessionPeakValueUpdates`
```
payload = sessionId: string, value: boolean
```

- `receiveSessionVolumeUpdates`
```
payload = sessionId: string, value: boolean
```

- `setSessionVolume`
```
payload = sessionId: string, volumePercent: int (0 - 100), confirmVolumeChange: boolean
- 'confirmVolumeChange' supresses 'return_setSessionVolume' if false.
- This method also does not fire 'update_sessionVolume'.
```

- `setSessionMute`
```
payload = sessionId: string, value: boolean
```

### From helper program

- `return_receiveSessionPeakValueUpdates`
```
payload = sessionId: string, newState: boolean
```

- `update_sessionPeakValue`
```
payload = sessionId: string, newPeakValue: float (0 - 1)
```

- `return_receiveSessionVolumeUpdates`
```
payload = sessionId: string, newState: boolean
```

- `update_sessionVolume`
```
payload = sessionId: string, newVolume: int (0 - 100)
```

- `return_setSessionVolume`
```
payload = sessionId: string, newVolume: int (0 - 100)
- Identical to `update_sessionPeakValue`. Should they be handled the same?
```

- `return_setSessionMute`
```
payload = sessionId: string, newState: boolean
```
