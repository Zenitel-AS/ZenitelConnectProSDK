using ConnectPro.Models;

namespace ConnectPro.DTO
{
    /// <summary>
    /// Data Transfer Object for the Camera model.
    /// </summary>
    public class CameraDto
    {
        #region Properties

        /// <summary>
        /// Gets or sets the unique identifier for the camera.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the Fully Qualified Identifier (FQID) of the camera.
        /// </summary>
        public string FQID { get; set; }

        /// <summary>
        /// Gets or sets the name of the camera.
        /// </summary>
        public string Name { get; set; }

        #endregion

        #region Model Conversion

        /// <summary>
        /// Converts this DTO into a Camera model object.
        /// </summary>
        public Camera ToModel()
        {
            return new Camera
            {
                Id = this.Id,
                FQID = this.FQID,
                Name = this.Name
                // AssociatedDevices and DeviceCameras are not mapped from DTO
            };
        }

        /// <summary>
        /// Creates a CameraDto from a Camera model object.
        /// </summary>
        public static CameraDto FromModel(Camera camera)
        {
            if (camera == null) return null;

            return new CameraDto
            {
                Id = camera.Id,
                FQID = camera.FQID,
                Name = camera.Name
            };
        }

        #endregion
    }
}
