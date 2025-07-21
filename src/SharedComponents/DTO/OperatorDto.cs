using ConnectPro.Models;

namespace ConnectPro.DTO
{
    /// <summary>
    /// Data Transfer Object for the Operator model.
    /// </summary>
    public class OperatorDto
    {
        #region Properties

        /// <summary>
        /// Gets or sets the machine name associated with the operator.
        /// </summary>
        public string MachineName { get; set; }

        /// <summary>
        /// Gets or sets the directory number assigned to the operator.
        /// </summary>
        public string DirectoryNumber { get; set; }

        #endregion

        #region Model Conversion

        /// <summary>
        /// Converts this DTO into an Operator model object.
        /// </summary>
        public Operator ToModel()
        {
            return new Operator
            {
                MachineName = this.MachineName,
                DirectoryNumber = this.DirectoryNumber
            };
        }

        /// <summary>
        /// Creates an OperatorDto from an Operator model object.
        /// </summary>
        public static OperatorDto FromModel(Operator op)
        {
            if (op == null) return null;

            return new OperatorDto
            {
                MachineName = op.MachineName,
                DirectoryNumber = op.DirectoryNumber
            };
        }

        #endregion
    }
}
