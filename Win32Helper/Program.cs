using System.Collections.Generic;
using static Win32Helper.Server;
using Win32Helper.Tests;

namespace Win32Helper
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            ArgumentParser parser = new ArgumentParser(args);
            Console.WriteLine("button-board Win32 Helper");
            Console.WriteLine("Args: {0}", parser.PrintArguments());

            if (parser.GetValue("test-api").Equals("true"))
            {
                TestAPI.TestApiMain();
            } else if (parser.GetValue("test-events").Equals("true"))
            {
                TestEvents.TestEventsMain();
            } else if (!parser.GetValue("port").Equals(""))
            {
                int port = int.Parse(parser.GetValue("port"));
                await Server.Start(port, true);
            }
        }
    }

    internal class ArgumentParser
    {
        private Dictionary<string, string> arguments = new Dictionary<string, string>();
        private List<string> argumentsRaw = new List<string>();

        public ArgumentParser(string[] args)
        {
            foreach (string arg in args)
            {
                if (arg.StartsWith("--"))
                {
                    argumentsRaw.Add(arg);
                    string[] parts = arg.Substring(2).Split('=');
                    if (parts.Length == 2)
                    {
                        arguments[parts[0]] = parts[1];
                    }
                    else
                    {
                        arguments[parts[0]] = "true";
                    }
                }
            }
        }

        public string GetValue(string key)
        {
            return arguments.ContainsKey(key) ? arguments[key] : "";
        }

        public string PrintArguments()
        {
            return string.Join(" ", argumentsRaw);
        }
    }
}