using CoreAudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Win32Helper.WinAudio
{
    internal class Device
    {
        private MMDevice device;
        private AudioEndpointVolumeNotificationDelegate? volumeNotificationDelegate = null;

        internal string FriendlyName
        {
            get => device!.DeviceFriendlyName!;
        }

        internal string Id
        {
            get => device!.ID!;
        }

        internal int VolumePercent
        {
            get => (int)(device!.AudioEndpointVolume!.MasterVolumeLevelScalar * 100);
            set => device.AudioEndpointVolume!.MasterVolumeLevelScalar = value / 100f;
        }

        internal bool Muted
        {
            get => device!.AudioEndpointVolume!.Mute;
            set => device!.AudioEndpointVolume!.Mute = value;
        }

        internal float PeakValue
        {
            get => device!.AudioMeterInformation!.MasterPeakValue;
        }

        internal bool Selected
        {
            get => device!.Selected!;
            set => device.Selected = value;
        }

        internal List<Session> AudioSessions
        {
            get
            {
                List<Session> sessions = new List<Session>();

                // Enumerate the audio sessions
                device!.AudioSessionManager2!.RefreshSessions();

                var audioSessionControl2Collection = device!.AudioSessionManager2!.Sessions;

                foreach (var audioSesionControl2 in audioSessionControl2Collection!)
                {
                    sessions.Add(new Session(audioSesionControl2));
                }

                return sessions;
            }
        }

        internal Device(MMDevice mmDevice)
        {
            device = mmDevice;
        }

        public delegate void volumeChangeCallbackDelegate(AudioVolumeNotificationData data);

        public void registerVolumeChangeCallback(volumeChangeCallbackDelegate callback)
        {
            // Remove the previous delegate if it exists
            unregisterVolumeChangeCallback();

            volumeNotificationDelegate = new AudioEndpointVolumeNotificationDelegate(callback);
            device!.AudioEndpointVolume!.OnVolumeNotification += volumeNotificationDelegate;
        }

        public void unregisterVolumeChangeCallback()
        {
            if (volumeNotificationDelegate == null) return;

            device!.AudioEndpointVolume!.OnVolumeNotification -= volumeNotificationDelegate;
            volumeNotificationDelegate = null;
        }
    }
}
