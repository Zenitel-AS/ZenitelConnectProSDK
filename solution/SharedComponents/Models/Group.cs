using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Wamp.Client;

namespace ConnectPro.Models
{
    [Serializable]
    public class Group : INotifyPropertyChanged
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public string Dirno { get; set; }
        public string DisplayName { get; set; }
        public string Priority { get; set; }
        public string[] Members { get; set; }


        //
        private bool _isBussy;
        [NotMapped]
        public bool IsBussy
        {
            get { return _isBussy; }
            set
            {
                if (_isBussy != value)
                {
                    _isBussy = value;
                    OnPropertyChanged("IsBussy");

                    if (_isBussy == false)
                    {
                        BrodcastedMessageName = "Prerecorded Msg";
                    }
                }
            }
        }

        private string _broadcastedMessageName = "Prerecorded Msg";
        [NotMapped]
        public string BrodcastedMessageName
        {
            get { return _broadcastedMessageName; }
            set
            {
                if (_broadcastedMessageName != value)
                {
                    _broadcastedMessageName = value;
                    OnPropertyChanged("BrodcastedMessageName");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Group() { }
        /// <summary>
        /// Constructor method that initializes a Group instance by setting its 
        /// properties to the properties of the `sdk_group_element` parameter.
        /// </summary>
        /// <param name="sdk_group_element"></param>
        public Group(WampClient.wamp_group_element sdk_group_element)
        {
            this.Dirno = sdk_group_element.dirno;
            this.DisplayName = sdk_group_element.displayname;
            this.Priority = sdk_group_element.priority;
            this.Members = sdk_group_element.members;
        }
        /// <summary>
        /// Takes the values from the WampClient.wamp_group_element and applyies them tho the Group object
        /// </summary>
        /// <param name="sdk_device_element"></param>
        public void SetValuesFromSDK(WampClient.wamp_group_element sdk_group_element)
        {
            this.Dirno = sdk_group_element.dirno;
            this.DisplayName = sdk_group_element.displayname;
            this.Priority = sdk_group_element.priority;
            this.Members = sdk_group_element.members;
        }
        /// <summary>
        /// Creates a new instance of the <see cref="Group"/> class from a <see cref="WampClient.wamp_group_element"/> object.
        /// </summary>
        /// <param name="sdk_group_element">The <see cref="WampClient.wamp_group_element"/> object to create the <see cref="Group"/> from.</param>
        /// <returns>A new instance of the <see cref="Group"/> class with its properties populated from the <paramref name="sdk_group_element"/> object.</returns>
        public static Group NewDeviceFromSdkElement(WampClient.wamp_group_element sdk_group_element)
        {
            return new Group()
            {
                Dirno = sdk_group_element.dirno,
                DisplayName = sdk_group_element.displayname,
                Priority = sdk_group_element.priority,
                Members = sdk_group_element.members
            };
        }
    }
}
