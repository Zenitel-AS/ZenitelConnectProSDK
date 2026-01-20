using ConnectPro.DTO;
using ConnectPro.Enums;
using ConnectPro.Interfaces;
using ConnectPro.Models.GPIO;
using Newtonsoft.Json;
using SharedComponents.Models.GPIO;
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
        #region Fields

        /// <summary>
        /// The current call state of the device.
        /// </summary>
        public CallState? _callState = null;

        /// <summary>
        /// The previous call state of the device.
        /// </summary>
        public CallState? _previousCallState = null;

        #endregion

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
        public CallState? CallState
        {
            get { return _callState; }
            set
            {
                _callState = value;
                if (_previousCallState != _callState)
                {
                    _previousCallState = _callState;
                    if (_callState.HasValue)
                        OnCallStateChange?.Invoke(this, _callState.Value);
                }
            }
        }

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

        /// <summary>
        /// Runtime GPIO control capability for this device.
        ///
        /// This property is NOT persisted and is not part of the Device DTO.
        /// It represents a live, session-bound control surface backed by
        /// WAMP subscriptions and RPC calls.
        ///
        /// Responsibilities:
        ///  - Expose available GPIO inputs and outputs
        ///  - Maintain current known GPIO state
        ///  - Raise events on GPIO state changes
        ///  - Allow activation/deactivation of GPO outputs
        ///
        /// The instance is created and attached by the SDK at runtime.
        /// Consumers must treat this as read-only and should not replace it.
        /// </summary>
        [NotMapped]
        public DeviceGpio Gpio { get; internal set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Device"/> class.
        ///
        /// Note:
        /// This parameterless constructor exists for EF and DTO mapping.
        /// It does not attach runtime-only capabilities (like GPIO).
        /// </summary>
        public Device() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Device"/> class using a WAMP device registration element.
        ///
        /// Note:
        /// This constructor maps SDK data only. It does not start GPIO listening.
        /// Use the overload with <see cref="IGpioTransport"/> to attach runtime GPIO capability.
        /// </summary>
        /// <param name="sdk_device_element">The WAMP device registration element.</param>
        public Device(WampClient.wamp_device_registration_element sdk_device_element)
        {
            if (sdk_device_element == null) throw new ArgumentNullException(nameof(sdk_device_element));

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
        /// Initializes a new instance of the <see cref="Device"/> class using a WAMP device registration element
        /// and attaches runtime GPIO capability immediately.
        ///
        /// This satisfies the requirement: "DeviceGpio should start listening immediately."
        /// </summary>
        /// <param name="sdk_device_element">The WAMP device registration element.</param>
        /// <param name="gpioTransport">
        /// Runtime GPIO transport (e.g., WAMP-backed implementation). Required to start listening immediately.
        /// </param>
        public Device(WampClient.wamp_device_registration_element sdk_device_element, IGpioTransport gpioTransport)
            : this(sdk_device_element)
        {
            // Attach runtime GPIO capability (not persisted).
            // This is a live control surface: it subscribes + snapshots immediately.
            if (gpioTransport != null && !string.IsNullOrEmpty(dirno))
            {
                Gpio = new DeviceGpio(dirno, gpioTransport);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates the current <see cref="Device"/> instance with values from a WAMP device registration element.
        ///
        /// IMPORTANT:
        /// This method does not re-create or replace <see cref="Gpio"/>. GPIO is a runtime capability with
        /// event subscriptions; replacing it would break consumers and leak subscriptions.
        /// </summary>
        /// <param name="sdk_device_element">The WAMP device registration element containing updated values.</param>
        public void SetValuesFromSDK(WampClient.wamp_device_registration_element sdk_device_element)
        {
            if (sdk_device_element == null) throw new ArgumentNullException(nameof(sdk_device_element));

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

            // Do NOT touch Gpio here. It is runtime-bound and should remain stable.
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Device"/> class from a WAMP device registration element.
        ///
        /// Note:
        /// This factory returns a data-mapped device only (no runtime GPIO).
        /// Prefer <see cref="NewDeviceFromSdkElement(WampClient.wamp_device_registration_element, IGpioTransport)"/>
        /// when you need GPIO attached immediately.
        /// </summary>
        public static Device NewDeviceFromSdkElement(WampClient.wamp_device_registration_element sdk_device_element)
        {
            if (sdk_device_element == null) throw new ArgumentNullException(nameof(sdk_device_element));

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

            device.CallState = device.DeviceState == Enums.DeviceState.reachable
                ? Enums.CallState.reachable
                : Enums.CallState.fault;

            return device;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Device"/> class from a WAMP device registration element
        /// and attaches runtime GPIO capability immediately.
        /// </summary>
        public static Device NewDeviceFromSdkElement(WampClient.wamp_device_registration_element sdk_device_element, IGpioTransport gpioTransport)
        {
            // Delegate to the constructor overload so behavior stays consistent in one place.
            return new Device(sdk_device_element, gpioTransport);
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the call state of the device changes.
        /// </summary>
        [NotMapped]
        public EventHandler<CallState> OnCallStateChange { get; set; }

        #endregion

        #region DTO Conversion

        public DeviceDto ToDto()
        {
            return new DeviceDto
            {
                DeviceId = this.DeviceId,
                DeviceIp = this.device_ip,
                Dirno = this.dirno,
                Location = this.location,
                Name = this.name,
                DeviceType = this.device_type,
                DeviceState = this.DeviceState,
                CallState = this.CallState,
                Username = this.Username,
                Password = this.Password,
                ServerIP = this.ServerIP
            };
        }

        public static Device FromDto(DeviceDto dto)
        {
            if (dto == null) return null;

            // DTO mapping creates a data object only. No runtime GPIO is attached here.
            // GPIO requires an active transport + subscriptions, which are session-bound.
            return new Device
            {
                DeviceId = dto.DeviceId,
                device_ip = dto.DeviceIp,
                dirno = dto.Dirno,
                location = dto.Location,
                name = dto.Name,
                device_type = dto.DeviceType,
                DeviceState = dto.DeviceState,
                CallState = dto.CallState,
                Username = dto.Username,
                Password = dto.Password,
                ServerIP = dto.ServerIP
            };
        }

        #endregion
    }
}
