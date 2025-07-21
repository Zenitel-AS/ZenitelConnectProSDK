

using ConnectPro.Models;

namespace ConnectPro.DTO
{
    /// <summary>
    /// Data Transfer Object for the Group model.
    /// </summary>
    public class GroupDto
    {
        #region Properties

        /// <summary>
        /// Gets or sets the unique identifier for the group.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the directory number associated with the group.
        /// </summary>
        public string Dirno { get; set; }

        /// <summary>
        /// Gets or sets the display name of the group.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the priority level of the group.
        /// </summary>
        public string Priority { get; set; }

        /// <summary>
        /// Gets or sets the list of members belonging to this group.
        /// </summary>
        public string[] Members { get; set; }

        #endregion

        #region Model Conversion

        /// <summary>
        /// Converts this DTO into a Group model object.
        /// </summary>
        public Group ToModel()
        {
            return new Models.Group
            {
                Id = this.Id,
                Dirno = this.Dirno,
                DisplayName = this.DisplayName,
                Priority = this.Priority,
                Members = this.Members
            };
        }

        /// <summary>
        /// Creates a GroupDto from a Group model object.
        /// </summary>
        public static GroupDto FromModel(Models.Group group)
        {
            if (group == null) return null;

            return new GroupDto
            {
                Id = group.Id,
                Dirno = group.Dirno,
                DisplayName = group.DisplayName,
                Priority = group.Priority,
                Members = group.Members
            };
        }

        #endregion
    }
}
