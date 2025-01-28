using System;
using System.IO;

namespace ConnectPro
{
    public class Configuration : Models.Configuration
    {
        public string ErrorLogFilePath { get; set; } = "";

        public Configuration()
        {
            ErrorLogFilePath = GetLogPath();
        }

        private string GetLogPath()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            var folderName = "Zenitel Connect Pro";
            var fileName = "Errors.log";
            var folderPath = System.IO.Path.Combine(path, folderName);

            string dbDirectory = System.IO.Path.GetDirectoryName(folderPath);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            return System.IO.Path.Combine(folderPath, fileName);
        }

        public static Configuration GetDefaultConfiguration() {
            return new Configuration()
            {
                ServerAddr = "192.168.1.5",
                Port = "8086",
                UserName = "",
                Password = "",
                MachineName = Environment.MachineName,
                Realm = "zenitel"
            };
        }
    }
}
