﻿using CoreAudio;
using CoreAudio.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Win32Helper.WinAudio
{
    internal class Device : IDisposable
    {
        private MMDevice device;
        private List<Session> sessions = new List<Session>();

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
            get => (int)Math.Round(device!.AudioEndpointVolume!.MasterVolumeLevelScalar * 100);
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
                if (sessions.Count <= 0)
                {
                    sessions = new List<Session>();

                    // Enumerate the audio sessions
                    device!.AudioSessionManager2!.RefreshSessions();

                    var audioSessionControl2Collection = device!.AudioSessionManager2!.Sessions;

                    foreach (var audioSesionControl2 in audioSessionControl2Collection!)
                    {
                        Session session = new Session(audioSesionControl2);
                        // Calls the session created callback if a session is invalidated.
                        // Easy way to use the callback as more of a session change
                        session.RegisterSessionInvalidatedCallback((Session.InvalidationReason reason) =>
                        {
                            SessionCreatedCallbackAdapter(null, null);
                        });
                        sessions.Add(session);
                    }
                }

                return sessions;
            }
        }

        internal Device(MMDevice mmDevice)
        {
            device = mmDevice;
        }

        public void Dispose()
        {
            UnregisterSessionCreatedCallback();
            UnregisterVolumeChangedCallback();
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

        private void VolumeChangedCallbackAdapter(AudioVolumeNotificationData data)
        {
            volumeChanged!.Invoke(data.Muted, (int)Math.Round(data.MasterVolume * 100));
        }

        internal delegate void sessionCreatedCallback(List<Session> sessions);
        private sessionCreatedCallback? sessionCreated = null;

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
            foreach (Session session in sessions)
            {
                session.Dispose();
            }

            sessions.Clear();

            sessionCreated!.Invoke(AudioSessions);
        }
    }
}
