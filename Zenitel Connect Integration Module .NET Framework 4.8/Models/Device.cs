using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Wamp.Client;

namespace ConnectPro.Models
{
    public class Device : WampClient.wamp_device_registration_element
    {
        /// <summary>
        /// Property that serves as the primary key for the Device entity in the Entity Framework. 
        /// </summary>
        [Key]
        public int DeviceId { get; set; }
        /// <summary>
        /// List of Cameras assosiated with this device
        /// </summary>
        [NotMapped]
        public virtual ICollection<Camera> Cameras { get; set; } = new List<Camera>();
        /// <summary>
        /// EntityFramework dependent property used for camera assosiation
        /// </summary>
        public virtual ICollection<DeviceCamera> DeviceCameras { get; set; } = new List<DeviceCamera>();
        [NotMapped]
        public byte NodeNumber { get; set; }
        public Device() { }
        /// <summary>
        /// Constructor method that initializes a Device instance by setting its 
        /// properties to the properties of the `sdk_device_element` parameter.
        /// </summary>
        /// <param name="sdk_device_element"></param>
        public Device(WampClient.wamp_device_registration_element sdk_device_element)
        {
            this.dirno = sdk_device_element.dirno;
            this.location = sdk_device_element.location;
            this.state = sdk_device_element.state;
            this.name = sdk_device_element.name;
        }
        /// <summary>
        /// Takes the values from the WampClient.wamp_device_registration_element and applyies them tho the Device object
        /// </summary>
        /// <param name="sdk_device_element"></param>
        public void SetValuesFromSDK(WampClient.wamp_device_registration_element sdk_device_element)
        {
            this.dirno = sdk_device_element.dirno;
            this.location = sdk_device_element.location;
            this.state = sdk_device_element.state;
            this.name = sdk_device_element.name;
        }
        /// <summary>
        /// Creates a new instance of the <see cref="Device"/> class from a <see cref="WampClient.wamp_device_registration_element"/> object.
        /// </summary>
        /// <param name="sdk_device_element">The <see cref="WampClient.wamp_device_registration_element"/> object to create the <see cref="Device"/> from.</param>
        /// <returns>A new instance of the <see cref="Device"/> class with its properties populated from the <paramref name="sdk_device_element"/> object.</returns>
        public static Device NewDeviceFromSdkElement(WampClient.wamp_device_registration_element sdk_device_element)
        {
            return new Device()
            {
                dirno = sdk_device_element.dirno,
                location = sdk_device_element.location,
                state = sdk_device_element.state,
                name = sdk_device_element.name
            };
        }
    }
}

