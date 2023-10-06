using CoreAudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Win32Helper.WinAudio
{
    internal class WinAudio
    {
        private static MMDeviceEnumerator mmDeviceEnum = new MMDeviceEnumerator(Guid.NewGuid());

        internal static WinAudioDevice DefaultOutputDevice
        {
            get => new WinAudioDevice(mmDeviceEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console));
        }

        internal static List<WinAudioDevice> OutputDevices
        {
            get
            {
                List<WinAudioDevice> devices = new List<WinAudioDevice>();

                // Get a collection of multimedia device resources
                MMDeviceCollection mmDeviceCollection = mmDeviceEnum.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

                // Create new WinAudioDevices from the collection
                foreach (MMDevice device in mmDeviceCollection)
                {
                    devices.Add(new WinAudioDevice(device));
                }

                return devices;
            }
        }
    }
}
