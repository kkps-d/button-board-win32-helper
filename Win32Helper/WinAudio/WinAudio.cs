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

        internal delegate void defaultDeviceChangedCallback(DefaultDeviceChangedEventArgs args);
        private static defaultDeviceChangedCallback? defaultDeviceChanged = null;

        internal static void RegisterDeviceChangedCallback(defaultDeviceChangedCallback callback)
        {
            UnregisterDeviceChangedCallback();

            defaultDeviceChanged = callback;
            notificationClient.DefaultDeviceChanged += DeviceChangedCallbackAdapter;
        }

        internal static void UnregisterDeviceChangedCallback()
        {
            if (defaultDeviceChanged == null) return;

            notificationClient.DefaultDeviceChanged -= DeviceChangedCallbackAdapter;
            defaultDeviceChanged = null;
        }

        private static void DeviceChangedCallbackAdapter(object? sender, DefaultDeviceChangedEventArgs args)
        {
            defaultDeviceChanged?.Invoke(args);
        }
    }
}
