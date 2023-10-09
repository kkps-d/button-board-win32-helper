using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Win32Helper.Tests
{
    internal class TestAPI
    {
        static internal void TestApiMain()
        {
            Console.WriteLine();

            Console.WriteLine("Default device ID: {0}", WinAudio.WinAudio.DefaultOutputDevice.Id);

            List<WinAudio.Device> devices = WinAudio.WinAudio.OutputDevices;

            foreach (WinAudio.Device device in devices)
            {
                // Print device name and ID
                Console.WriteLine("\t{0}:{1}", device.FriendlyName, device.Id);

                // Print is default device?
                Console.WriteLine("\tIs default?: {0}", device.Selected);

                // Print muted status
                Console.WriteLine("\tIs muted?: {0}", device.Muted);

                // Print volume percent
                Console.WriteLine("\tVolume: {0}%", device.VolumePercent);

                // Print peak value
                Console.WriteLine("\tPeak: {0}", device.PeakValue);

                // Print information for each session
                var sessions = device.AudioSessions;

                foreach (var session in sessions)
                {
                    // Print friendly name and ID
                    Console.WriteLine("\t\t{0} : {1}", session.FriendlyName, session.Id);

                    // Print icon path
                    Console.WriteLine("\t\tIcon path: {0}", session.IconPath);

                    // Print muted
                    Console.WriteLine("\t\tIs Muted?: {0}", session.Muted);

                    // Print volume percent
                    Console.WriteLine("\t\tVolume: {0}%", session.VolumePercent);

                    // Print peak value
                    Console.WriteLine("\t\tPeak: {0}", session.PeakValue);

                    Console.WriteLine();
                }

                Console.WriteLine();
            }
        }
    }
}
