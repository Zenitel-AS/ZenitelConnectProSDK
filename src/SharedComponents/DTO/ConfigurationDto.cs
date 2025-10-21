using ConnectPro.Models;

namespace ConnectPro.DTO
{
    /// <summary>
    /// Data Transfer Object for the Configuration model.
    /// </summary>
    public class ConfigurationDto
    {
        #region Properties

        /// <summary>
        /// Gets or sets the unique identifier for the configuration entry.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the name of the controller.
        /// </summary>
        public string ControllerName { get; set; } = "";

        /// <summary>
        /// Gets or sets the server address.
        /// </summary>
        public string ServerAddr { get; set; }

        /// <summary>
        /// Gets or sets the username for authentication.
        /// </summary>
        public string UserName { get; set; }

        ///// <summary>
        ///// Gets or sets the password for authentication.
        ///// </summary>
        //public string Password { get; set; }

        /// <summary>
        /// Gets or sets the port number for server communication.
        /// </summary>
        public string Port { get; set; }

        /// <summary>
        /// Gets or sets the authentication realm.
        /// </summary>
        public string Realm { get; set; }

        /// <summary>
        /// Gets or sets the operator directory number.
        /// </summary>
        public string OperatorDirNo { get; set; } = "";

        /// <summary>
        /// Gets or sets a value indicating whether the configuration should be displayed in the Smart Client.
        /// </summary>
        public bool DisplayConfigurationInSmartClient { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether popup windows should be enabled.
        /// </summary>
        public bool EnablePopupWindow { get; set; } = true;

        /// <summary>
        /// Gets or sets the machine name.
        /// </summary>
        public string MachineName { get; set; } = "";

        // If you want to support OperatorDto, add here:
        // public OperatorDto Operator { get; set; }

        /// <summary>
        /// Gets or sets the operator associated with this configuration.
        /// </summary>
        public OperatorDto Operator { get; set; }

        #endregion

        #region Model Conversion

        /// <summary>
        /// Converts this DTO into a Configuration model object.
        /// </summary>
        public Configuration ToModel()
        {
            return new Configuration
            {
                ID = this.ID,
                ControllerName = this.ControllerName,
                ServerAddr = this.ServerAddr,
                UserName = this.UserName,
                Port = this.Port,
                Realm = this.Realm,
                OperatorDirNo = this.OperatorDirNo,
                DisplayConfigurationInSmartClient = this.DisplayConfigurationInSmartClient,
                EnablePopupWindow = this.EnablePopupWindow,
                MachineName = this.MachineName,
                Operator = this.Operator?.ToModel()
            };
        }

        /// <summary>
        /// Creates a ConfigurationDto from a Configuration model object.
        /// </summary>
        public static ConfigurationDto FromModel(Configuration cfg)
        {
            if (cfg == null) return null;

            return new ConfigurationDto
            {
                ID = cfg.ID,
                ControllerName = cfg.ControllerName,
                ServerAddr = cfg.ServerAddr,
                UserName = cfg.UserName,
                Port = cfg.Port,
                Realm = cfg.Realm,
                OperatorDirNo = cfg.OperatorDirNo,
                DisplayConfigurationInSmartClient = cfg.DisplayConfigurationInSmartClient,
                EnablePopupWindow = cfg.EnablePopupWindow,
                MachineName = cfg.MachineName,
                Operator = cfg.Operator?.ToDto()
            };
        }

        #endregion
    }
}
