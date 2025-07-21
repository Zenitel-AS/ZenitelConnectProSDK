using ConnectPro.DTO;

namespace ConnectPro.Models
{
    /// <summary>
    /// Represents the system configuration settings for connecting to the server and managing operator settings.
    /// </summary>
    public class Configuration
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

        /// <summary>
        /// Gets or sets the password for authentication.
        /// </summary>
        public string Password { get; set; }

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

        /// <summary>
        /// Gets or sets the operator associated with this configuration.
        /// </summary>
        public Operator Operator { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Checks if any required configuration fields have empty or null values.
        /// </summary>
        /// <returns>A tuple where the first value is a boolean indicating if any empty values exist, 
        /// and the second value is an integer representing the missing field index.</returns>
        private (bool, int) ConfigurationHasEmptyValues()
        {
            if (ServerAddr == null) return (true, 0);
            if (UserName == null) return (true, 1);
            if (Password == null) return (true, 2);
            if (Port == null) return (true, 3);
            if (Realm == null) return (true, 4);
            if (OperatorDirNo == null) return (true, 5);
            return (false, 0);
        }

        /// <summary>
        /// Fills empty configuration values with default settings.
        /// </summary>
        public void FillEmptyValuesWithDefaults()
        {
            (bool, int) EmptyValues = ConfigurationHasEmptyValues();
            while (EmptyValues != (false, 0))
            {
                switch (EmptyValues.Item2)
                {
                    case 0:
                        ServerAddr = "169.254.1.5";
                        break;
                    case 1:
                        UserName = "";
                        break;
                    case 2:
                        Password = "";
                        break;
                    case 3:
                        Port = "8086";
                        break;
                    case 4:
                        Realm = "zenitel";
                        break;
                    case 5:
                        OperatorDirNo = "";
                        break;
                }
                EmptyValues = ConfigurationHasEmptyValues();
            }
        }

        #endregion

        #region DTO Conversion

        /// <summary>
        /// Converts this Configuration model object to its DTO representation.
        /// </summary>
        public ConfigurationDto ToDto()
        {
            return new ConfigurationDto
            {
                ID = this.ID,
                ControllerName = this.ControllerName,
                ServerAddr = this.ServerAddr,
                UserName = this.UserName,
                Password = this.Password,
                Port = this.Port,
                Realm = this.Realm,
                OperatorDirNo = this.OperatorDirNo,
                DisplayConfigurationInSmartClient = this.DisplayConfigurationInSmartClient,
                EnablePopupWindow = this.EnablePopupWindow,
                MachineName = this.MachineName,
                Operator = this.Operator?.ToDto()
            };
        }

        /// <summary>
        /// Creates a Configuration model instance from a ConfigurationDto.
        /// </summary>
        public static Configuration FromDto(ConfigurationDto dto)
        {
            if (dto == null) return null;

            return new Configuration
            {
                ID = dto.ID,
                ControllerName = dto.ControllerName,
                ServerAddr = dto.ServerAddr,
                UserName = dto.UserName,
                Password = dto.Password,
                Port = dto.Port,
                Realm = dto.Realm,
                OperatorDirNo = dto.OperatorDirNo,
                DisplayConfigurationInSmartClient = dto.DisplayConfigurationInSmartClient,
                EnablePopupWindow = dto.EnablePopupWindow,
                MachineName = dto.MachineName,
                Operator = Operator.FromDto(dto.Operator)
            };
        }

        #endregion


    }
}
