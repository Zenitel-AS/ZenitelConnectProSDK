using ConnectPro.DTO;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConnectPro.Models
{
    /// <summary>
    /// Represents an operator entity, storing machine-specific and directory number information.
    /// </summary>
    public class Operator
    {
        #region Properties

        /// <summary>
        /// Gets or sets the machine name associated with the operator.
        /// This serves as the primary key.
        /// </summary>
        [Key]
        public string MachineName { get; set; }

        /// <summary>
        /// Gets or sets the directory number assigned to the operator.
        /// <para><b>Note:</b> This property is <c>NotMapped</c> and will not be stored in the database.</para>
        /// </summary>
        [NotMapped]
        public string DirectoryNumber { get; set; }

        #endregion

        #region DTO Conversion

        /// <summary>
        /// Converts this Operator model object to its DTO representation.
        /// </summary>
        public OperatorDto ToDto()
        {
            return new OperatorDto
            {
                MachineName = this.MachineName,
                DirectoryNumber = this.DirectoryNumber
            };
        }

        /// <summary>
        /// Creates an Operator model instance from an OperatorDto.
        /// </summary>
        public static Operator FromDto(OperatorDto dto)
        {
            if (dto == null) return null;

            return new Operator
            {
                MachineName = dto.MachineName,
                DirectoryNumber = dto.DirectoryNumber
            };
        }

        #endregion

    }
}
