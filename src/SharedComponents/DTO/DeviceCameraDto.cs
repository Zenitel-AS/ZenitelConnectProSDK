using ConnectPro.Models;

namespace ConnectPro.DTO
{
    /// <summary>
    /// Data Transfer Object for the DeviceCamera model.
    /// </summary>
    public class DeviceCameraDto
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

        #endregion

        #region Model Conversion

        /// <summary>
        /// Converts this DTO into a DeviceCamera model object.
        /// </summary>
        public DeviceCamera ToModel()
        {
            return new DeviceCamera
            {
                DeviceId = this.DeviceId,
                CameraId = this.CameraId
            };
        }

        /// <summary>
        /// Creates a DeviceCameraDto from a DeviceCamera model object.
        /// </summary>
        public static DeviceCameraDto FromModel(DeviceCamera dc)
        {
            if (dc == null) return null;

            return new DeviceCameraDto
            {
                DeviceId = dc.DeviceId,
                CameraId = dc.CameraId
            };
        }

        #endregion
    }
}
