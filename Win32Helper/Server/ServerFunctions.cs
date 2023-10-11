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
        public static void GetOutputDevices(NetworkStream stream, int messageNum, string payload)
        {
            List<WinAudio.Device> devices = WinAudio.WinAudio.OutputDevices;

            string deviceListJson = "";

            foreach (WinAudio.Device device in devices)
            {
                string? deviceId = device == null ? null : device.Id;
                string? friendlyName = device == null ? null : device.FriendlyName;
                string? volumePercent = device == null ? null : device.VolumePercent.ToString();
                string? muted = device == null ? null : device.Muted ? "true" : "false";
                string? selected = device == null ? null : device.Selected ? "true" : "false";

                deviceListJson += $"{{" +
                    $"\"deviceId\": \"{deviceId}\"," +
                    $"\"friendlyName\": \"{friendlyName}\"," +
                    $"\"volumePercent\": \"{volumePercent}\"," +
                    $"\"muted\": {muted}," +
                    $"\"selected\": {selected}" +
                $"}},";
            }

            // Remove final comma
            deviceListJson = deviceListJson.Substring(0, deviceListJson.Length - 1);

            string json = $"[{deviceListJson}]";

            Console.WriteLine("[GetOutputDevices] Returning '{0}'", json);

            WriteToStream(stream, $"return_getOutputDevices,{messageNum},{json}");
        }

        private static async void WriteToStream(NetworkStream stream, string data)
        {
            if (stream == null) return;
            if (!stream.CanWrite) return;

            byte[] buffer = Encoding.ASCII.GetBytes(data);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }
    }
}
