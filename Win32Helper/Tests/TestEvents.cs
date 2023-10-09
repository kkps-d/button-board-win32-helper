using CoreAudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Win32Helper.Tests
{
    internal class TestEvents
    {
        internal static void TestEventsMain()
        {
            Console.CursorVisible = false;

            CancellationTokenSource cts = new CancellationTokenSource();

            WinAudio.WinAudio.RegisterDeviceChangeCallback((_) =>
            {
                cts.Cancel();
                cts.Dispose();
            });

            while (true)
            {
                try
                {
                    cts = new CancellationTokenSource();
                    _ = HandleEvents(cts.Token);
                } catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        internal static Task HandleEvents(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            Console.Clear();

            WinAudio.Device device = WinAudio.WinAudio.DefaultOutputDevice;
            List<WinAudio.Session> sessions = device.AudioSessions;
            string deviceName = device.FriendlyName;
            string volume = device.VolumePercent.ToString() + "%    ";
            string isMuted = device.Muted.ToString();

            device.RegisterVolumeChangeCallback((bool newMuted, int newVolume) =>
            {
                volume = newVolume.ToString() + "%    ";
                isMuted = newMuted.ToString() + "   ";
            });

            device.RegisterSessionCreateCallback((List<WinAudio.Session> newSessions) =>
            {
                sessions = newSessions;
            });

            while (true)
            {
                ct.ThrowIfCancellationRequested();

                string outString = "Current device: " + deviceName + "\nVolume: " + volume + "\nIs muted?: " + isMuted + "\n\n";
                
                foreach (WinAudio.Session session in sessions)
                {
                    string bar = DrawMeter(30, session.PeakValue, 1f);

                    outString += session.FriendlyName + ", " + session.VolumePercent + "%, " + (session.Muted ? "Muted  " : "Unmuted") + "\n" + bar + "\n";
                }

                Console.WriteLine(outString);
                Console.SetCursorPosition(0, 0);
            }
        }

        internal static string DrawMeter(int length, float currentValue, float maxValue)
        {
            if (currentValue < 0) { currentValue = 0; }
            if (currentValue > maxValue) { currentValue = maxValue; }

            float perBlockValue = maxValue / (float)length;

            int fullBlockCount = (int)(currentValue / perBlockValue);

            string progressBar = "[" + new string('█', fullBlockCount); // █

            bool printHalfBlock = ((currentValue / perBlockValue) - fullBlockCount >= 0.5f);

            if (printHalfBlock) { progressBar += '▓'; }

            int fillerLength = length - fullBlockCount - (printHalfBlock ? 1 : 0);
            if (fillerLength < 0) { fillerLength = 0; }

            progressBar += new string(' ', fillerLength) + "]";

            return progressBar;
        }
    }
}
