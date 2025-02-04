using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConnectPro.Models
{
    /// <summary>
    /// Represents a prerecorded message that can be associated with a device in the system.
    /// </summary>
    public class PrerecordedMessage
    {
        /// <summary>
        /// Gets or sets the unique identifier for the prerecorded message.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the prerecorded message.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the directory number (Dirno) associated with this message.
        /// </summary>
        public string Dirno { get; set; }

        /// <summary>
        /// Gets or sets the group number associated with this message.
        /// <para><b>Note:</b> This property is <c>NotMapped</c> and will not be stored in the database.</para>
        /// </summary>
        [NotMapped]
        public string GroupNo { get; set; }
    }
}
