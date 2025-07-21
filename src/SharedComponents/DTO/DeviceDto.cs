using ConnectPro.Enums;
using ConnectPro.Models;

namespace ConnectPro.DTO
{
    /// <summary>
    /// Data Transfer Object for the Device model.
    /// </summary>
    public class DeviceDto
    {
        #region Properties

        /// <summary>
        /// Gets or sets the unique identifier for the device.
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        /// Gets or sets the IP address of the device.
        /// </summary>
        public string DeviceIp { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the directory number assigned to the device.
        /// </summary>
        public string Dirno { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the location where the device is installed.
        /// </summary>
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name assigned to the device.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of the device.
        /// </summary>
        public string DeviceType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the state indicating whether the device is reachable or not.
        /// </summary>
        public DeviceState? DeviceState { get; set; }

        /// <summary>
        /// Gets or sets the current call state of the device.
        /// </summary>
        public CallState? CallState { get; set; }

        /// <summary>
        /// Gets or sets the username used for authentication.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password used for authentication.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the server IP address associated with the device.
        /// </summary>
        public string ServerIP { get; set; } = string.Empty;

        #endregion

        #region Model Conversion

        /// <summary>
        /// Converts this DTO to a <see cref="Device"/> model instance.
        /// </summary>
        public Device ToModel()
        {
            return new Device
            {
                DeviceId = this.DeviceId,
                device_ip = this.DeviceIp,
                dirno = this.Dirno,
                location = this.Location,
                name = this.Name,
                device_type = this.DeviceType,
                DeviceState = this.DeviceState,
                CallState = this.CallState,
                Username = this.Username,
                Password = this.Password,
                ServerIP = this.ServerIP
            };
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DeviceDto"/> from a Device model instance.
        /// </summary>
        public static DeviceDto FromModel(Device device)
        {
            if (device == null) return null;

            return new DeviceDto
            {
                DeviceId = device.DeviceId,
                DeviceIp = device.device_ip,
                Dirno = device.dirno,
                Location = device.location,
                Name = device.name,
                DeviceType = device.device_type,
                DeviceState = device.DeviceState,
                CallState = device.CallState,
                Username = device.Username,
                Password = device.Password,
                ServerIP = device.ServerIP
            };
        }

        #endregion
    }
}
