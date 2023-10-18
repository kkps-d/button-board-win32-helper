using CoreAudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Win32Helper.WinAudio;

namespace Win32Helper
{
    internal class ServerFunctions
    {
        #region WinAudio
        public static void GetOutputDevices(NetworkStream stream, int messageNum, string payload)
        {
            string json = GetDeviceListAsJson(WinAudio.WinAudio.OutputDevices);

            Console.WriteLine("[GetOutputDevices] Returning '{0}'", json);

            WriteToStream(stream, $"return_getOutputDevices,{messageNum},{json}");
        }

        public static void ReceiveDeviceListUpdates(NetworkStream stream, int messageNum, string payload)
        {
            bool receiveUpdates = bool.Parse(payload);

            try
            {
                if (receiveUpdates)
                {
                    WinAudio.WinAudio.RegisterDeviceChangedCallback((_) =>
                    {
                        string json = GetDeviceListAsJson(WinAudio.WinAudio.OutputDevices);

                        Console.WriteLine("[ReceiveDeviceListUpdates] Sending device list update", receiveUpdates);

                        WriteToStream(stream, $"update_deviceList,{json}");
                    });
                }
                else
                {
                    WinAudio.WinAudio.UnregisterDeviceChangedCallback();
                }
            }
            catch (Exception _)
            {
                receiveUpdates = !receiveUpdates;
            }

            Console.WriteLine("[ReceiveDeviceListUpdates] Returning '{0}'", receiveUpdates);

            WriteToStream(stream, $"return_receiveDeviceListUpdates,{messageNum},{receiveUpdates}");
        }
        
        private static string GetDeviceListAsJson(List<WinAudio.Device> devices)
        {
            string deviceListString = "";

            foreach (WinAudio.Device device in devices)
            {
                string? deviceId = device == null ? null : device.Id;
                string? friendlyName = device == null ? null : device.FriendlyName;
                string? volumePercent = device == null ? null : device.VolumePercent.ToString();
                string? muted = device == null ? null : device.Muted ? "true" : "false";
                string? selected = device == null ? null : device.Selected ? "true" : "false";

                deviceListString += $"{{" +
                    $"\"deviceId\": \"{deviceId}\"," +
                    $"\"friendlyName\": \"{friendlyName}\"," +
                    $"\"volumePercent\": {volumePercent}," +
                    $"\"muted\": {muted}," +
                    $"\"selected\": {selected}" +
                $"}},";
            }

            // Remove final comma
            deviceListString = deviceListString.Substring(0, deviceListString.Length - 1);

            string json = $"[{deviceListString}]";

            return json;
        }
        #endregion

        #region WinAudioDevice
        private static CancellationTokenSource devicePeakValueToken = new CancellationTokenSource();
        private static async Task SendPeakValue(NetworkStream stream, CancellationToken ct, WinAudio.Device device, int delayMs)
        {
            string deviceId = device.Id;

            ct.ThrowIfCancellationRequested();
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                string message = $"update_devicePeakValue,{deviceId},{device.PeakValue.ToString()}";
                bool failed = WriteToStream(stream, message);
                if (failed)
                {
                    Console.WriteLine("[SendPeakValue] Failed to write to stream. Aborting task");
                    break;
                }
                await Task.Delay(delayMs);
            }
        }

        public static void ReceiveDevicePeakValueUpdates(NetworkStream stream, int messageNum, string payload)
        {
            string[] splitPayload = payload.Split(',');
            string deviceId = splitPayload[0];
            bool receiveUpdates = bool.Parse(splitPayload[1]);

            try
            {
                if (receiveUpdates)
                {
                    WinAudio.Device? device = WinAudio.WinAudio.OutputDevices.Find(device => device.Id == deviceId);
                    if (device == null)
                    {
                        Console.WriteLine("[ReceiveDevicePeakValueUpdates] Device with ID '{0}' not found", payload);
                        throw new Exception("Device not found");
                    }

                    devicePeakValueToken.Cancel();
                    devicePeakValueToken = new CancellationTokenSource();
                    _ = SendPeakValue(stream, devicePeakValueToken.Token, device, 10);
                }
                else
                {
                    devicePeakValueToken.Cancel();
                }
            }
            catch (Exception _)
            {
                devicePeakValueToken.Cancel();
                receiveUpdates = !receiveUpdates;
            }

            Console.WriteLine("[ReceiveDevicePeakValueUpdates] Returning '{0}'", receiveUpdates);

            WriteToStream(stream, $"return_receiveDevicePeakValueUpdates,{messageNum},{deviceId},{receiveUpdates}");
        }

        public static void ReceiveDeviceVolumeUpdates(NetworkStream stream, int messageNum, string payload)
        {
            string[] splitPayload = payload.Split(',');
            string deviceId = splitPayload[0];
            bool receiveUpdates = bool.Parse(splitPayload[1]);

            try
            {
                WinAudio.Device? device = WinAudio.WinAudio.OutputDevices.Find(device => device.Id == deviceId);
                if (receiveUpdates)
                {
                    if (device == null)
                    {
                        Console.WriteLine("[ReceiveDeviceVolumeUpdates] Device with ID '{0}' not found", payload);
                        throw new Exception("Device not found");
                    }

                    device.RegisterVolumeChangedCallback((bool muted, int volumePercent) =>
                    {
                        WriteToStream(stream, $"update_deviceVolume,{device.Id},{muted},{volumePercent}");
                    });
                }
                else
                {
                    if (device == null)
                    {
                        Console.WriteLine("[ReceiveDeviceVolumeUpdates] Device with ID '{0}' not found", payload);
                        throw new Exception("Device not found");
                    }
                    device?.UnregisterVolumeChangedCallback();
                }
            }
            catch (Exception _)
            {
                receiveUpdates = !receiveUpdates;
            }

            Console.WriteLine("[ReceiveDeviceVolumeUpdates] Returning '{0}'", receiveUpdates);

            WriteToStream(stream, $"return_receiveDeviceVolumeUpdates,{messageNum},{receiveUpdates}");
        }

        public static void ReceiveAudioSessionUpdates(NetworkStream stream, int messageNum, string payload)
        {
            string[] splitPayload = payload.Split(',');
            string deviceId = splitPayload[0];
            bool receiveUpdates = bool.Parse(splitPayload[1]);

            try
            {
                WinAudio.Device? device = WinAudio.WinAudio.OutputDevices.Find(device => device.Id == deviceId);

                if (device == null)
                {
                    Console.WriteLine("[ReceiveAudioSessionUpdates] Device with ID '{0}' not found", payload);
                    throw new Exception("Device not found");
                }

                if (receiveUpdates)
                {
                    device.RegisterSessionCreatedCallback((List<Session> sessions) =>
                    {
                        string json = GetSessionListAsJson(sessions);
                        WriteToStream(stream, $"update_audioSessions,{deviceId},{json}");
                    });
                }
                else
                {
                    device.UnregisterSessionCreatedCallback();
                }
            }
            catch (Exception _)
            {
                receiveUpdates = !receiveUpdates;
            }

            WriteToStream(stream, $"return_receiveAudioSessionUpdates,{messageNum},{deviceId},{receiveUpdates}");
        }

        public static void GetAudioSessions(NetworkStream stream, int messageNum, string payload)
        {
            WinAudio.Device? device = WinAudio.WinAudio.OutputDevices.Find(device => device.Id == payload);

            if (device == null)
            {
                // Return payload to be null if device is not found
                Console.WriteLine("[GetAudioSessions] Device with ID '{0}' not found", payload);
                WriteToStream(stream, $"return_getAudioSessions,{messageNum},null");
                return;
            };

            string json = GetSessionListAsJson(device.AudioSessions);

            Console.WriteLine("[GetAudioSessions] Returning '{0}'", json);

            WriteToStream(stream, $"return_getAudioSessions,{messageNum},{json}");
        }

        private static string GetSessionListAsJson(List<WinAudio.Session> sessions)
        {
            string sessionListString = "";

            foreach (WinAudio.Session session in sessions)
            {
                string sessionId = session.Id;
                string friendlyName = session.FriendlyName;
                string iconPath = session.IconPath.Replace("\\", "\\\\");
                string volumePercent = session.VolumePercent.ToString();
                string muted = session.Muted ? "true" : "false";

                sessionListString += $"{{" +
                    $"\"sessionId\": \"{sessionId}\"," +
                    $"\"friendlyName\": \"{friendlyName}\"," +
                    $"\"iconPath\": \"{iconPath}\"," +
                    $"\"volumePercent\": {volumePercent}," +
                    $"\"muted\": {muted}" +
                $"}},";
            }

            // Remove final comma
            sessionListString = sessionListString.Substring(0, sessionListString.Length - 1);

            string json = $"[{sessionListString}]";

            return json;
        }

        public static void SetActiveDevice(NetworkStream stream, int messageNum, string payload)
        {
            WinAudio.Device? device = WinAudio.WinAudio.OutputDevices.Find(device => device.Id == payload);
            WinAudio.Device defaultDevice = WinAudio.WinAudio.DefaultOutputDevice;

            if (device == null)
            {
                // Return payload to be null if device is not found
                Console.WriteLine("[SetActiveDevice] Device with ID '{0}' not found", payload);
                WriteToStream(stream, $"return_setActiveDevice,{messageNum},{defaultDevice.Id}");
                return;
            };

            if (defaultDevice.Id == device.Id)
            {
                Console.WriteLine("[SetActiveDevice] Device '{0}' is already default!", device.FriendlyName);
            }
            else
            {
                device.Selected = true;
            }

            WriteToStream(stream, $"return_setActiveDevice,{messageNum},{payload}");
        }

        public static void SetDeviceVolume(NetworkStream stream, int messageNum, string payload)
        {
            string[] splitPayload = payload.Split(',');
            string deviceId = splitPayload[0];
            bool muted = bool.Parse(splitPayload[1]);
            int newVolume = int.Parse(splitPayload[2]);
            bool confirmVolumeChanges = bool.Parse(splitPayload[3]);

            WinAudio.Device? device = WinAudio.WinAudio.OutputDevices.Find(device => device.Id == deviceId);

            if (device == null)
            {
                Console.WriteLine("[SetDeviceVolume] Device with ID '{0}' not found", deviceId);
                if (confirmVolumeChanges)
                {
                    WriteToStream(stream, $"return_setDeviceVolume,{messageNum},null");
                }
                return;
            }

            device.VolumePercent = newVolume;
            device.Muted = muted;

            if (confirmVolumeChanges)
            {
                WriteToStream(stream, $"return_setDeviceVolume,{messageNum},{payload}");
            }
        }
        #endregion

        #region WinAudioSession
        // @TODO NEXT STEPS, PEAK METERS!
        public static void ReceiveSessionPeakValueUpdates(NetworkStream stream, int messageNum, string payload)
        {
            string[] splitPayload = payload.Split(',');
            string deviceId = splitPayload[0];
            string sessionId = splitPayload[1];
            bool receiveUpdates = bool.Parse(splitPayload[2]);

            try
            {
                if (receiveUpdates)
                {
                    WinAudio.Device? device = WinAudio.WinAudio.OutputDevices.Find(device => device.Id == deviceId);
                    if (device == null)
                    {
                        Console.WriteLine("[ReceiveDevicePeakValueUpdates] Device with ID '{0}' not found", payload);
                        throw new Exception("Device not found");
                    }

                    WinAudio.Session? session = device.AudioSessions.Find(session => session.Id == sessionId);

                    if (session == null)
                    {
                        Console.WriteLine("[SetSessionVolume] Session with ID '{0}' not found on device with ID '{1}'", sessionId, deviceId);
                        throw new Exception("Session not found");
                    }

                    session.RegisterPeakValueUpdateCallback((float peakValue) => {
                        WriteToStream(stream, $"update_sessionPeakValue,{deviceId},{sessionId},{peakValue}");
                    }, 10);
                }
                else
                {
                    WinAudio.Device? device = WinAudio.WinAudio.OutputDevices.Find(device => device.Id == deviceId);
                    if (device == null)
                    {
                        Console.WriteLine("[ReceiveDevicePeakValueUpdates] Device with ID '{0}' not found", payload);
                        throw new Exception("Device not found");
                    }

                    WinAudio.Session? session = device.AudioSessions.Find(session => session.Id == sessionId);

                    if (session == null)
                    {
                        Console.WriteLine("[SetSessionVolume] Session with ID '{0}' not found on device with ID '{1}'", sessionId, deviceId);
                        throw new Exception("Session not found");
                    }

                    session.UnregisterPeakValueUpdateCallback();
                }
            }
            catch (Exception _)
            {
                receiveUpdates = !receiveUpdates;
            }

            Console.WriteLine("[ReceiveDevicePeakValueUpdates] Returning '{0}'", receiveUpdates);

            WriteToStream(stream, $"return_receiveDevicePeakValueUpdates,{messageNum},{deviceId},{receiveUpdates}");
        }

        public static void ReceiveSessionVolumeUpdates(NetworkStream stream, int messageNum, string payload)
        {
            Console.WriteLine("[ReceiveSessionVolumeUpdates] Unimplemented");
        }

        public static void SetSessionVolume(NetworkStream stream, int messageNum, string payload)
        {
            string[] splitPayload = payload.Split(',');
            string deviceId = splitPayload[0];
            string sessionId = splitPayload[1];
            bool muted = bool.Parse(splitPayload[2]);
            int newVolume = int.Parse(splitPayload[3]);
            bool confirmVolumeChanges = bool.Parse(splitPayload[4]);

            WinAudio.Device? device = WinAudio.WinAudio.OutputDevices.Find(device => device.Id == deviceId);

            if (device == null)
            {
                Console.WriteLine("[SetSessionVolume] Device with ID '{0}' not found", deviceId);
                if (confirmVolumeChanges)
                {
                    WriteToStream(stream, $"return_setSessionVolume,{messageNum},null");
                }
                return;
            }

            WinAudio.Session? session = device.AudioSessions.Find(session => session.Id == sessionId);

            if (session == null)
            {
                Console.WriteLine("[SetSessionVolume] Session with ID '{0}' not found on device with ID '{1}'", sessionId, deviceId);
                if (confirmVolumeChanges)
                {
                    WriteToStream(stream, $"return_setSessionVolume,{messageNum},null");
                }
                return;
            }

            session.VolumePercent = newVolume;
            session.Muted = muted;

            if (confirmVolumeChanges)
            {
                WriteToStream(stream, $"return_setSessionVolume,{messageNum},{payload}");
            }
        }

        #endregion

        // Returns whether there is an error or not
        private static bool WriteToStream(NetworkStream stream, string data)
        {
            if (stream == null) return true;
            if (!stream.CanWrite) return true;

            byte[] buffer = Encoding.ASCII.GetBytes(data + ";");
            try
            {
                stream.WriteAsync(buffer, 0, buffer.Length).Wait();
            }
            catch (Exception ex)
            {
                return true;
            }

            return false;
        }
    }
}

