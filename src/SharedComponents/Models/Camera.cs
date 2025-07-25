﻿using ConnectPro.DTO;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConnectPro.Models
{
    /// <summary>
    /// Represents a camera entity within the system.
    /// </summary>
    public class Camera
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

        /// <summary>
        /// Gets or sets the list of devices associated with this camera.
        /// <para><b>Note:</b> This property is <c>NotMapped</c> and will not be stored in the database.</para>
        /// </summary>
        [NotMapped]
        public virtual ICollection<Device> AssociatedDevices { get; set; } = new List<Device>();

        /// <summary>
        /// Gets or sets the entity framework dependent property used for camera association.
        /// </summary>
        public virtual ICollection<DeviceCamera> DeviceCameras { get; set; } = new List<DeviceCamera>();

        #endregion

        #region DTO Conversion

        /// <summary>
        /// Converts this Camera model object to its DTO representation.
        /// </summary>
        public CameraDto ToDto()
        {
            return new CameraDto
            {
                Id = this.Id,
                FQID = this.FQID,
                Name = this.Name
            };
        }

        /// <summary>
        /// Creates a Camera model instance from a CameraDto.
        /// </summary>
        public static Camera FromDto(CameraDto dto)
        {
            if (dto == null) return null;

            return new Camera
            {
                Id = dto.Id,
                FQID = dto.FQID,
                Name = dto.Name
            };
        }

        #endregion

    }
}
