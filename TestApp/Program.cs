using System;
using System.Linq;
using System.Text;
using System.Threading;
using ConnectPro;
using Wamp.Client;

namespace TestApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Zenitel Connect Pro - Console Test App");

            var defaultConfig = Configuration.GetDefaultConfiguration();

            Console.Write($"Server [{defaultConfig.ServerAddr}]: ");
            var input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input)) defaultConfig.ServerAddr = input.Trim();

            Console.Write($"Port [{defaultConfig.Port}]: ");
            input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input)) defaultConfig.Port = input.Trim();

            Console.Write($"Realm [{defaultConfig.Realm}]: ");
            input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input)) defaultConfig.Realm = input.Trim();

            Console.Write($"User [{defaultConfig.UserName}]: ");
            input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input)) defaultConfig.UserName = input.Trim();

            Console.Write("Password: ");
            var pwd = ReadPassword();
            if (!string.IsNullOrEmpty(pwd)) defaultConfig.Password = pwd;

            var core = new Core();
            core.Configuration = defaultConfig;

            // Start core components (creates WAMP, REST, handlers)
            core.Start();

            // Subscribe to GPIO events
            core.Events.OnGpioEvent += (s, e) =>
            {
                if (e != null)
                {
                    Console.WriteLine($"[GPIO] Dirno={e.Dirno}, Id={e.Element.id}, State={e.Element.state}");
                }
            };

            // Device list change
            core.Events.OnDeviceListChange += (s, e) =>
            {
                Console.WriteLine("Device list changed:");
                try
                {
                    var list = core.Collection.RegisteredDevices;
                    if (list != null)
                    {
                        foreach (var d in list)
                        {
                            Console.WriteLine($" - dirno={d.dirno}, ip={d.device_ip}");
                        }
                    }
                }
                catch { }
            };

            // Child log entries from WAMP
            core.Events.OnChildLogEntry += (s, msg) => Console.WriteLine("[WAMP] " + msg);

            // Open connection (WAMP + REST auth)
            core.ConnectionHandler.OpenConnection();

            Console.WriteLine("Started. Press 'q' then Enter to quit.");

            // Wait until user quits
            while (true)
            {
                var line = Console.ReadLine();
                if (string.Equals(line.Trim(), "q", StringComparison.OrdinalIgnoreCase))
                    break;
                Thread.Sleep(100);
            }

            core.Dispose();
        }

        private static string ReadPassword()
        {
            var sb = new StringBuilder();
            ConsoleKeyInfo key;
            while ((key = Console.ReadKey(true)).Key != ConsoleKey.Enter)
            {
                if (key.Key == ConsoleKey.Backspace)
                {
                    if (sb.Length > 0)
                    {
                        sb.Length--;
                        Console.Write("\b \b");
                    }
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    sb.Append(key.KeyChar);
                    Console.Write("*");
                }
            }
            Console.WriteLine();
            return sb.ToString();
        }
    }
}
