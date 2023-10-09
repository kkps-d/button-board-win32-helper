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
        private static MMNotificationClient notificationClient = new MMNotificationClient(mmDeviceEnum); 

        internal static Device DefaultOutputDevice
        {
            get => new Device(mmDeviceEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console));
        }

        internal static List<Device> OutputDevices
        {
            get
            {
                List<Device> devices = new List<Device>();

                // Get a collection of multimedia device resources
                MMDeviceCollection mmDeviceCollection = mmDeviceEnum.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

                // Create new WinAudioDevices from the collection
                foreach (MMDevice device in mmDeviceCollection)
                {
                    devices.Add(new Device(device));
                }

                return devices;
            }
        }

        internal delegate void defaultDeviceChangeCallbackDelegate(DefaultDeviceChangedEventArgs args);
        private static defaultDeviceChangeCallbackDelegate? defaultDeviceChangeCallback = null;

        static internal void RegisterDeviceChangeCallback(defaultDeviceChangeCallbackDelegate callback)
        {
            UnregisterDeviceChangeCallback();

            defaultDeviceChangeCallback = callback;
            notificationClient.DefaultDeviceChanged += DeviceChangeCallbackAdapter;
        }

        static internal void UnregisterDeviceChangeCallback()
        {
            if (defaultDeviceChangeCallback == null) return;

            notificationClient.DefaultDeviceChanged -= DeviceChangeCallbackAdapter;
            defaultDeviceChangeCallback = null;
        }

        static void DeviceChangeCallbackAdapter(object? sender, DefaultDeviceChangedEventArgs args)
        {
            defaultDeviceChangeCallback?.Invoke(args);
        }
    }
}
