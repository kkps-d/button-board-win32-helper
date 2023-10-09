using CoreAudio;
using CoreAudio.Interfaces;
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

        internal delegate void volumeChangedCallback(bool muted, int volumePercent);
        private volumeChangedCallback? volumeChanged = null;

        internal void RegisterVolumeChangedCallback(volumeChangedCallback callback)
        {
            // Remove the previous callback if it exists
            UnregisterVolumeChangedCallback();

            volumeChanged = callback;
            device!.AudioEndpointVolume!.OnVolumeNotification += VolumeChangedCallbackAdapter;
        }

        internal void UnregisterVolumeChangedCallback()
        {
            if (volumeChanged == null) return;

            device!.AudioEndpointVolume!.OnVolumeNotification -= VolumeChangedCallbackAdapter;
            volumeChanged = null;
        }


        internal delegate void sessionCreatedCallback(List<Session> sessions);
        private sessionCreatedCallback? sessionCreated = null;

        private void VolumeChangedCallbackAdapter(AudioVolumeNotificationData data)
        {
            volumeChanged!.Invoke(data.Muted, (int)(data.MasterVolume * 100));
        }

        internal void RegisterSessionCreatedCallback(sessionCreatedCallback callback)
        {
            UnregisterSessionCreatedCallback();

            sessionCreated = callback;
            device!.AudioSessionManager2!.OnSessionCreated += SessionCreatedCallbackAdapter;
        }

        internal void UnregisterSessionCreatedCallback()
        {
            if (sessionCreated == null) return;

            device!.AudioSessionManager2!.OnSessionCreated -= SessionCreatedCallbackAdapter;
            sessionCreated = null;
        }

        private void SessionCreatedCallbackAdapter(object sender, IAudioSessionControl2 newSession)
        {
            sessionCreated!.Invoke(AudioSessions);
        }
    }
}
