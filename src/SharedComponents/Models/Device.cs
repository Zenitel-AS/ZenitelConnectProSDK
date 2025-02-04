using ConnectPro.Enums;
using ConnectPro.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Wamp.Client;

namespace ConnectPro.Models
{
    /// <summary>
    /// Represents a device entity with network, authentication, and state information.
    /// </summary>
    [Serializable]
    public class Device
    {
        #region Properties

        /// <summary>
        /// Gets or sets the unique identifier for the device.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DeviceId { get; set; }

        /// <summary>
        /// Gets or sets the IP address of the device.
        /// </summary>
        public string device_ip { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the directory number assigned to the device.
        /// </summary>
        public string dirno { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the location where the device is installed.
        /// </summary>
        public string location { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name assigned to the device.
        /// </summary>
        public string name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of the device.
        /// </summary>
        public string device_type { get; set; }

        /// <summary>
        /// Gets or sets the state indicating whether the device is reachable or not.
        /// </summary>
        public DeviceState? DeviceState { get; set; } = null;

        /// <summary>
        /// Gets or sets the current call state of the device.
        /// </summary>
        public CallState? CallState { get; set; } = null;

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

        /// <summary>
        /// Gets or sets the node number of the device.
        /// </summary>
        [NotMapped]
        public byte NodeNumber { get; set; }

        /// <summary>
        /// Gets or sets the collection of cameras associated with this device.
        /// <para><b>Note:</b> This property is <c>NotMapped</c> and will not be stored in the database.</para>
        /// </summary>
        [NotMapped]
        public virtual ICollection<ICamera> Cameras { get; set; } = new List<ICamera>();

        /// <summary>
        /// Gets or sets the Entity Framework-dependent property used for camera association.
        /// </summary>
        public virtual ICollection<DeviceCamera> DeviceCameras { get; set; } = new List<DeviceCamera>();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Device"/> class.
        /// </summary>
        public Device() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Device"/> class using a WAMP device registration element.
        /// </summary>
        /// <param name="sdk_device_element">The WAMP device registration element.</param>
        public Device(WampClient.wamp_device_registration_element sdk_device_element)
        {
            device_ip = sdk_device_element.device_ip;
            dirno = sdk_device_element.dirno;
            location = sdk_device_element.location;
            name = sdk_device_element.name;
            device_type = sdk_device_element.device_type;

            if (Enum.TryParse(sdk_device_element.state, ignoreCase: true, out DeviceState deviceState))
            {
                DeviceState = deviceState;
            }

            CallState = DeviceState == Enums.DeviceState.reachable ? Enums.CallState.reachable : Enums.CallState.fault;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates the current <see cref="Device"/> instance with values from a WAMP device registration element.
        /// </summary>
        /// <param name="sdk_device_element">The WAMP device registration element containing updated values.</param>
        public void SetValuesFromSDK(WampClient.wamp_device_registration_element sdk_device_element)
        {
            device_ip = sdk_device_element.device_ip;
            dirno = sdk_device_element.dirno;
            location = sdk_device_element.location;
            name = sdk_device_element.name;
            device_type = sdk_device_element.device_type;

            if (Enum.TryParse(sdk_device_element.state, ignoreCase: true, out DeviceState deviceState))
            {
                DeviceState = deviceState;
            }

            CallState = DeviceState == Enums.DeviceState.reachable ? Enums.CallState.reachable : Enums.CallState.fault;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Device"/> class from a WAMP device registration element.
        /// </summary>
        /// <param name="sdk_device_element">The WAMP device registration element.</param>
        /// <returns>A new instance of <see cref="Device"/> with properties populated from the SDK element.</returns>
        public static Device NewDeviceFromSdkElement(WampClient.wamp_device_registration_element sdk_device_element)
        {
            Device device = new Device()
            {
                device_ip = sdk_device_element.device_ip,
                dirno = sdk_device_element.dirno,
                location = sdk_device_element.location,
                DeviceState = null,
                name = sdk_device_element.name,
                device_type = sdk_device_element.device_type,
                CallState = null
            };

            if (Enum.TryParse(sdk_device_element.state, ignoreCase: true, out DeviceState deviceState))
            {
                device.DeviceState = deviceState;
            }

            device.CallState = device.DeviceState == Enums.DeviceState.reachable ? Enums.CallState.reachable : Enums.CallState.fault;

            return device;
        }

        #endregion
    }
}
