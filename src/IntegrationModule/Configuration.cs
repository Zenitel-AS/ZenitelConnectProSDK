using System;
using System.IO;

namespace ConnectPro
{
    /// <summary>
    /// Represents the application configuration, extending the base configuration with logging capabilities.
    /// </summary>
    public class Configuration : Models.Configuration
    {
        #region Properties

        /// <summary>
        /// Gets or sets the file path for error logging.
        /// </summary>
        public string ErrorLogFilePath { get; set; } = "";

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Configuration"/> class.
        /// </summary>
        public Configuration()
        {
            ErrorLogFilePath = GetLogPath();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Generates and retrieves the path for the error log file.
        /// </summary>
        /// <returns>The full file path of the error log.</returns>
        private string GetLogPath()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            var folderName = "Zenitel Connect Pro";
            var fileName = "Errors.log";
            var folderPath = Path.Combine(path, folderName);

            // Ensure the directory exists
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            return Path.Combine(folderPath, fileName);
        }

        /// <summary>
        /// Creates and returns a default configuration instance.
        /// </summary>
        /// <returns>A default <see cref="Configuration"/> instance with predefined values.</returns>
        public static Configuration GetDefaultConfiguration()
        {
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

        #endregion
    }
}
