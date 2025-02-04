using ConnectPro.Models;
using System.Collections.Generic;

namespace ConnectPro.Interfaces
{
    /// <summary>
    /// Represents a camera entity with associated devices and metadata.
    /// </summary>
    public interface ICamera
    {
        #region Properties

        /// <summary>
        /// Gets or sets the unique identifier for the camera.
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// Gets or sets the Fully Qualified Identifier (FQID) of the camera.
        /// </summary>
        string FQID { get; set; }

        /// <summary>
        /// Gets or sets the name of the camera.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the collection of devices associated with this camera.
        /// </summary>
        ICollection<Device> AssociatedDevices { get; set; }

        /// <summary>
        /// Gets or sets the collection of device-camera relationships for this camera.
        /// </summary>
        ICollection<DeviceCamera> DeviceCameras { get; set; }

        #endregion
    }
}
