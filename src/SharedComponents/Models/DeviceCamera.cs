using ConnectPro.DTO;
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
        public virtual Camera Camera { get; set; }

        #endregion

        #region DTO Conversion

        /// <summary>
        /// Converts this DeviceCamera model object to its DTO representation.
        /// </summary>
        public DeviceCameraDto ToDto()
        {
            return new DeviceCameraDto
            {
                DeviceId = this.DeviceId,
                CameraId = this.CameraId
                // Device and Camera navigation properties are not included in DTO by default
            };
        }

        /// <summary>
        /// Creates a DeviceCamera model instance from a DeviceCameraDto.
        /// </summary>
        public static DeviceCamera FromDto(DeviceCameraDto dto)
        {
            if (dto == null) return null;

            return new DeviceCamera
            {
                DeviceId = dto.DeviceId,
                CameraId = dto.CameraId
                // Device and Camera: not mapped from DTO
            };
        }

        #endregion

    }
}
