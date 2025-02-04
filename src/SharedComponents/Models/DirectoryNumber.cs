using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Wamp.Client;

namespace ConnectPro.Models
{
    /// <summary>
    /// Represents a directory number entity, storing information about a device's directory number and display name.
    /// </summary>
    [Serializable]
    public class DirectoryNumber
    {
        #region Properties

        /// <summary>
        /// Gets or sets the unique identifier for the directory number.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the directory number assigned to a device.
        /// </summary>
        public string Dirno { get; set; }

        /// <summary>
        /// Gets or sets the display name associated with the directory number.
        /// </summary>
        public string DisplayName { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryNumber"/> class.
        /// </summary>
        public DirectoryNumber() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryNumber"/> class from a WAMP directory number element.
        /// </summary>
        /// <param name="sdk_directory_number_element">The WAMP directory number element.</param>
        public DirectoryNumber(WampClient.wamp_directory_number_element sdk_directory_number_element)
        {
            Dirno = sdk_directory_number_element.dirno;
            DisplayName = sdk_directory_number_element.displayname;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates the current <see cref="DirectoryNumber"/> instance with values from a WAMP directory number element.
        /// </summary>
        /// <param name="sdk_directory_number_element">The WAMP directory number element containing updated values.</param>
        public void SetValuesFromSDK(WampClient.wamp_directory_number_element sdk_directory_number_element)
        {
            Dirno = sdk_directory_number_element.dirno;
            DisplayName = sdk_directory_number_element.displayname;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DirectoryNumber"/> class from a WAMP directory number element.
        /// </summary>
        /// <param name="sdk_directory_number_element">The WAMP directory number element.</param>
        /// <returns>A new instance of <see cref="DirectoryNumber"/> with properties populated from the SDK element.</returns>
        public static DirectoryNumber NewDeviceFromSdkElement(WampClient.wamp_directory_number_element sdk_directory_number_element)
        {
            return new DirectoryNumber()
            {
                Dirno = sdk_directory_number_element.dirno,
                DisplayName = sdk_directory_number_element.displayname
            };
        }

        #endregion
    }
}
