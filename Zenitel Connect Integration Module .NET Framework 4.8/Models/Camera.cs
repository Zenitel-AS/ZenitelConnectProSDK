using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectPro.Models
{
    public class Camera
    {
        /// <summary>
        /// List of Cameras assosiated with this device
        /// </summary>
        [NotMapped]
        public virtual ICollection<Device> AssosiatedDevices { get; set; } = new List<Device>();
        /// <summary>
        /// EntityFramework dependent property used for camera assosiation
        /// </summary>
        public virtual ICollection<DeviceCamera> DeviceCameras { get; set; } = new List<DeviceCamera>();
    }
}
