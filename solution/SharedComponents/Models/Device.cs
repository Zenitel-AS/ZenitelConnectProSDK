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
    [Serializable]
    public class Device
    {
        /// <summary>
        /// Property that serves as the primary key for the Device entity in the Entity Framework. 
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DeviceId { get; set; }

        ///// <summary>
        ///// List of Cameras assosiated with this device
        ///// </summary>
        //[NotMapped]
        //public virtual ICollection<Camera> Cameras { get; set; } = new List<Camera>();
        ///// <summary>
        ///// EntityFramework dependent property used for camera assosiation
        ///// </summary>
        //public virtual ICollection<DeviceCamera> DeviceCameras { get; set; } = new List<DeviceCamera>();
        [NotMapped]
        public byte NodeNumber { get; set; }

        /// <summary>
        /// Device IP Address
        /// </summary>
        public string device_ip { get; set; } = String.Empty;

        /// <summary>
        /// Dirno is the directory number of the device.
        /// </summary>
        public string dirno { get; set; } = String.Empty;

        /// <summary>
        /// Location is the where the device is placed.
        /// </summary>
        public string location { get; set; } = String.Empty;

        /// <summary>
        /// Name is the assigned name of the device.
        /// </summary>
        public string name { get; set; } = String.Empty;

        public string device_type { get; set; }

        /// <summary>
        /// State indicates if the device is "reachable" or "not reachable".
        /// </summary>
        public DeviceState? DeviceState { get; set; } = null;
        public CallState? CallState { get; set; } = null;

        public string Username { get; set; } = String.Empty;
        public string Password { get; set; } = String.Empty;
        public string ServerIP { get; set; } = String.Empty;



        [NotMapped]
        public virtual ICollection<ICamera> Cameras { get; set; } = new List<ICamera>();
        /// <summary>
        /// EntityFramework dependent property used for camera assosiation
        /// </summary>
        public virtual ICollection<DeviceCamera> DeviceCameras { get; set; } = new List<DeviceCamera>();


        public Device() { }
        /// <summary>
        /// Constructor method that initializes a Device instance by setting its 
        /// properties to the properties of the `sdk_device_element` parameter.
        /// </summary>
        /// <param name="sdk_device_element"></param>
        public Device(WampClient.wamp_device_registration_element sdk_device_element)
        {
            this.device_ip = sdk_device_element.device_ip;
            this.dirno = sdk_device_element.dirno;
            this.location = sdk_device_element.location;
            //this.state = sdk_device_element.state;
            this.name = sdk_device_element.name;
            this.device_type = sdk_device_element.device_type;


            if (Enum.TryParse(sdk_device_element.state, ignoreCase: true, out DeviceState callState))
            {
                this.DeviceState = callState;
            }

            this.CallState = this.DeviceState == Enums.DeviceState.reachable ? Enums.CallState.reachable : Enums.CallState.fault;
        }
        /// <summary>
        /// Takes the values from the WampClient.wamp_device_registration_element and applyies them tho the Device object
        /// </summary>
        /// <param name="sdk_device_element"></param>
        public void SetValuesFromSDK(WampClient.wamp_device_registration_element sdk_device_element)
        {
            this.device_ip = sdk_device_element.device_ip;
            this.dirno = sdk_device_element.dirno;
            this.location = sdk_device_element.location;
            //this.state = sdk_device_element.state;
            this.name = sdk_device_element.name;
            this.device_type = sdk_device_element.device_type;

            if (Enum.TryParse(sdk_device_element.state, ignoreCase: true, out DeviceState callState))
            {
                this.DeviceState = callState;
            }
            this.CallState = this.DeviceState == Enums.DeviceState.reachable ? Enums.CallState.reachable : Enums.CallState.fault;
        }
        /// <summary>
        /// Creates a new instance of the <see cref="Device"/> class from a <see cref="WampClient.wamp_device_registration_element"/> object.
        /// </summary>
        /// <param name="sdk_device_element">The <see cref="WampClient.wamp_device_registration_element"/> object to create the <see cref="Device"/> from.</param>
        /// <returns>A new instance of the <see cref="Device"/> class with its properties populated from the <paramref name="sdk_device_element"/> object.</returns>
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
            if (Enum.TryParse(sdk_device_element.state, ignoreCase: true, out DeviceState callState))
            {
                device.DeviceState = callState;
            }
            device.CallState = device.DeviceState == Enums.DeviceState.reachable ? Enums.CallState.reachable : Enums.CallState.fault;
            return device;
        }
    }
}

