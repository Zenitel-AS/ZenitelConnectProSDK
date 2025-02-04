using ConnectPro.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace ConnectPro.Models
{
    /// <summary>
    /// Represents the association between a device and a camera.
    /// </summary>
    public class DeviceCamera
    {
        #region Properties

        /// <summary>
        /// Gets or sets the unique identifier for the device-camera association.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the associated device.
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the associated camera.
        /// </summary>
        public int CameraId { get; set; }

        /// <summary>
        /// Gets or sets the device associated with this relationship.
        /// </summary>
        public virtual Device Device { get; set; }

        /// <summary>
        /// Gets or sets the camera associated with this relationship.
        /// </summary>
        public virtual ICamera Camera { get; set; }

        #endregion
    }
}
