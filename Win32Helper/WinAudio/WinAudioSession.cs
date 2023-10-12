using CoreAudio;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Text;
using System.Drawing.Imaging;

namespace Win32Helper.WinAudio
{
    internal class Session : IDisposable
    {
        internal enum InvalidationReason
        {
            SessionExpired,
            SessionDisconnected,
        }

        private AudioSessionControl2 control;
        private string FiguredOutFriendlyName = "";

        private bool _invalid = false;

        internal bool Invalid
        {
            get => _invalid;
        }

        internal string FriendlyName
        {
            get
            {
                if (String.IsNullOrEmpty(FiguredOutFriendlyName))
                {
                    FiguredOutFriendlyName = FigureOutFriendlyName();
                }

                return FiguredOutFriendlyName;
            }
        }

        internal string Id
        {
            get
            {
                return control.ProcessID.ToString();
            }
        }

        internal string IconPath
        {
            get
            {
                // Get the path to the resources folder and filename
                string folderPath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "resources" + Path.DirectorySeparatorChar;
                string imgFilename = this.Id + ".png";

                // If file exists, return file path
                if (File.Exists(folderPath + imgFilename))
                {
                    return folderPath + imgFilename;
                }

                // If not, try to extract the icon from the process and return the new icon path
                Process process = Process.GetProcessById((int)control.ProcessID);
                Icon? icon = GetIcon(GetProcessFilename(process));
                process.Dispose();

                if (icon != null)
                {
                    //Console.WriteLine("Saving to {0}", folderPath + imgFilename);

                    // Create directory if it doesn't exist
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    using (FileStream fs = new FileStream(folderPath + imgFilename, FileMode.Create))
                    {
                        icon.ToBitmap().Save(fs, ImageFormat.Png);
                    }

                    icon.Dispose();
                    return control.IconPath;
                }

                // Return empty string if icon path cannot be obtained. e.g. system sounds
                return "";
            }
        }
        internal int VolumePercent
        {
            get => (int)(control.SimpleAudioVolume!.MasterVolume * 100);
            set => control.SimpleAudioVolume!.MasterVolume = value / 100f;
        }

        internal bool Muted
        {
            get => control.SimpleAudioVolume!.Mute;
            set => control.SimpleAudioVolume!.Mute = value;
        }

        internal float PeakValue
        {
            get => control.AudioMeterInformation!.MasterPeakValue;
        }

        internal Session(AudioSessionControl2 audioSessionControl2)
        {
            control = audioSessionControl2;

            // Listen for session expire and terminated events. User should not be able to control or view these sessions
            control.OnStateChanged += SessionStateChangeCallbackAdapter;

            control.OnSessionDisconnected += SessionDisconnectCallbackAdapter;
        }

        public void Dispose()
        {
            UnregisterSessionInvalidatedCallback();
            UnregisterVolumeChangedCallback();
            control.OnStateChanged -= SessionStateChangeCallbackAdapter;
            control.OnSessionDisconnected -= SessionDisconnectCallbackAdapter;
        }

        internal delegate void sessionInvalidatedCallback(InvalidationReason reason);
        private sessionInvalidatedCallback? sessionInvalidated = null;

        internal void RegisterSessionInvalidatedCallback(sessionInvalidatedCallback callback)
        {
            UnregisterSessionInvalidatedCallback();

            sessionInvalidated = callback;
        }

        internal void UnregisterSessionInvalidatedCallback()
        {
            sessionInvalidated = null;
        }

        private void SessionStateChangeCallbackAdapter(object sender, AudioSessionState state)
        {
            if (state == AudioSessionState.AudioSessionStateExpired)
            {
                _invalid = true;
                sessionInvalidated?.Invoke(InvalidationReason.SessionExpired);
                UnregisterSessionInvalidatedCallback();
                control.OnStateChanged -= SessionStateChangeCallbackAdapter;
                control.OnSessionDisconnected -= SessionDisconnectCallbackAdapter;
            }
        }

        private void SessionDisconnectCallbackAdapter(object sender, AudioSessionDisconnectReason disconnectReason)
        {
            _invalid = true;
            sessionInvalidated?.Invoke(InvalidationReason.SessionDisconnected);
            UnregisterSessionInvalidatedCallback();
            control.OnStateChanged -= SessionStateChangeCallbackAdapter;
            control.OnSessionDisconnected -= SessionDisconnectCallbackAdapter;
        }

        internal delegate void volumeChangedCallback(bool muted, int volumePercent);
        private volumeChangedCallback? volumeChanged = null;

        internal void RegisterVolumeChangedCallback(volumeChangedCallback callback)
        {
            // Remove the previous callback if it exists
            UnregisterVolumeChangedCallback();

            volumeChanged = callback;
            control.OnSimpleVolumeChanged += VolumeChangedCallbackAdapter;
        }

        internal void UnregisterVolumeChangedCallback()
        {
            if (volumeChanged == null) return;

            control.OnSimpleVolumeChanged -= VolumeChangedCallbackAdapter;
            volumeChanged = null;
        }

        private void VolumeChangedCallbackAdapter(object sender, float newVolume, bool newMute)
        {
            volumeChanged!.Invoke(newMute, (int)(newVolume * 100));
        }

        private string FigureOutFriendlyName()
        {
            if (control.ProcessID == 0)
            {
                return "System Sounds";
            }

            // Get the session name from the window handle
            string sessionName = "";

            Process process = Process.GetProcessById((int)control.ProcessID);

            if (process != null && !string.IsNullOrEmpty(process.MainWindowTitle))
            {
                sessionName = process.MainWindowTitle;
            }

            // If the process name looks like a file path, discard it (for Rainmeter widgets)
            if (File.Exists(sessionName)) { sessionName = ""; }

            if (!string.IsNullOrEmpty(sessionName))
            {
                return sessionName;
            }

            // If string is still empty, get the name from the process description
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(GetProcessFilename(process!));
            sessionName = versionInfo!.FileDescription!;

            if (!string.IsNullOrEmpty(sessionName))
            {
                return sessionName;
            }

            // Finally, if the string is still empty, use the executable name
            sessionName = process!.ProcessName;
            return sessionName;
        }

        private static Icon? GetIcon(string path)
        {
            Icon? result = null;

            if (string.IsNullOrEmpty(path)) return null;

            try
            {
                result = Icon.ExtractAssociatedIcon(path)!;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to extract icon from path: {0}", ex.ToString());
            }

            return result;
        }

        // PInvoke to open processes and access executable information without administrator rights
        [Flags]
        private enum ProcessAccessFlags : uint
        {
            QueryLimitedInformation = 0x00001000
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool QueryFullProcessImageName(
        [In] IntPtr hProcess,
        [In] int dwFlags,
        [Out] StringBuilder lpExeName,
        ref int lpdwSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(
         ProcessAccessFlags processAccess,
         bool bInheritHandle,
         int processId);

        private static String GetProcessFilename(Process p)
        {
            int capacity = 260;
            StringBuilder builder = new StringBuilder(capacity);
            IntPtr ptr = OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, p.Id);
            if (!QueryFullProcessImageName(ptr, 0, builder, ref capacity))
            {
                return String.Empty;
            }

            return builder.ToString();
        }
    }
}
