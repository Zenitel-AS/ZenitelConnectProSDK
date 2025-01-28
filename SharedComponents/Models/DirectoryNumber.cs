using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Wamp.Client;

namespace ConnectPro.Models
{
    [Serializable]
    public class DirectoryNumber
    {
        /// <summary>
        /// Property that serves as the primary key for the Device entity in the Entity Framework. 
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Dirno { get; set; }
        public string DisplayName { get; set; }

        public DirectoryNumber() { }
        /// <summary>
        /// Constructor method that initializes a Directory instance by setting its 
        /// properties to the properties of the `sdk_directory_number_element` parameter.
        /// </summary>
        /// <param name="sdk_directory_number_element"></param>
        public DirectoryNumber(WampClient.wamp_directory_number_element sdk_directory_number_element)
        {
            this.Dirno = sdk_directory_number_element.dirno;
            this.DisplayName = sdk_directory_number_element.displayname;
        }
        /// <summary>
        /// Takes the values from the WampClient.wamp_directory_number_element and applyies them tho the Directory object
        /// </summary>
        /// <param name="sdk_directory_number_element"></param>
        public void SetValuesFromSDK(WampClient.wamp_directory_number_element sdk_directory_number_element)
        {
            this.Dirno = sdk_directory_number_element.dirno;
            this.DisplayName = sdk_directory_number_element.displayname;
        }
        /// <summary>
        /// Creates a new instance of the <see cref="Directory"/> class from a <see cref="WampClient.wamp_directory_number_element"/> object.
        /// </summary>
        /// <param name="sdk_directory_number_element">The <see cref="WampClient.wamp_directory_number_element"/> object to create the <see cref="Directory"/> from.</param>
        /// <returns>A new instance of the <see cref="Directory"/> class with its properties populated from the <paramref name="sdk_directory_number_element"/> object.</returns>
        public static DirectoryNumber NewDeviceFromSdkElement(WampClient.wamp_directory_number_element sdk_directory_number_element)
        {
            return new DirectoryNumber()
            {
                Dirno = sdk_directory_number_element.dirno,
                DisplayName = sdk_directory_number_element.displayname
            };
        }
    }
}
