using ConnectPro.DTO;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Wamp.Client;

namespace ConnectPro.Models
{
    /// <summary>
    /// Represents a group entity, storing details such as directory number, priority, and members.
    /// Implements <see cref="INotifyPropertyChanged"/> to support UI binding.
    /// </summary>
    [Serializable]
    public class Group : INotifyPropertyChanged
    {
        #region Properties

        /// <summary>
        /// Gets or sets the unique identifier for the group.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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

        private bool _isBusy;

        /// <summary>
        /// Gets or sets a value indicating whether the group is currently busy.
        /// </summary>
        [NotMapped]
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged(nameof(IsBusy));
                    OnBussyStateChange?.Invoke(this, _isBusy);
                    if (!_isBusy)
                    {
                        BroadcastedMessageName = "Prerecorded Msg";
                    }
                }
            }
        }

        private string _broadcastedMessageName = "Prerecorded Msg";

        /// <summary>
        /// Gets or sets the name of the message currently being broadcasted.
        /// </summary>
        [NotMapped]
        public string BroadcastedMessageName
        {
            get => _broadcastedMessageName;
            set
            {
                if (_broadcastedMessageName != value)
                {
                    _broadcastedMessageName = value;
                    OnPropertyChanged(nameof(BroadcastedMessageName));
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the bussy state of the group changes.
        /// </summary>
        [NotMapped]
        public EventHandler<bool> OnBussyStateChange { get; set; }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies subscribers about a property value change.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Group"/> class.
        /// </summary>
        public Group() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Group"/> class from a WAMP group element.
        /// </summary>
        /// <param name="sdk_group_element">The WAMP group element to initialize the object from.</param>
        public Group(WampClient.wamp_group_element sdk_group_element)
        {
            Dirno = sdk_group_element.dirno;
            DisplayName = sdk_group_element.displayname;
            Priority = sdk_group_element.priority;
            Members = sdk_group_element.members;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates the current <see cref="Group"/> instance with values from a WAMP group element.
        /// </summary>
        /// <param name="sdk_group_element">The WAMP group element containing updated values.</param>
        public void SetValuesFromSDK(WampClient.wamp_group_element sdk_group_element)
        {
            Dirno = sdk_group_element.dirno;
            DisplayName = sdk_group_element.displayname;
            Priority = sdk_group_element.priority;
            Members = sdk_group_element.members;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Group"/> class from a WAMP group element.
        /// </summary>
        /// <param name="sdk_group_element">The WAMP group element to convert.</param>
        /// <returns>A new instance of <see cref="Group"/> with properties populated from the WAMP group element.</returns>
        public static Group NewGroupFromSdkElement(WampClient.wamp_group_element sdk_group_element)
        {
            return new Group()
            {
                Dirno = sdk_group_element.dirno,
                DisplayName = sdk_group_element.displayname,
                Priority = sdk_group_element.priority,
                Members = sdk_group_element.members
            };
        }

        #endregion

        #region DTO Conversion

        /// <summary>
        /// Converts this Group model object to its DTO representation.
        /// </summary>
        public GroupDto ToDto()
        {
            return new GroupDto
            {
                Id = this.Id,
                Dirno = this.Dirno,
                DisplayName = this.DisplayName,
                Priority = this.Priority,
                Members = this.Members
                // IsBusy and BroadcastedMessageName are not included in DTO by default
            };
        }

        /// <summary>
        /// Creates a Group model instance from a GroupDto.
        /// </summary>
        public static Group FromDto(GroupDto dto)
        {
            if (dto == null) return null;

            return new Group
            {
                Id = dto.Id,
                Dirno = dto.Dirno,
                DisplayName = dto.DisplayName,
                Priority = dto.Priority,
                Members = dto.Members
                // IsBusy and BroadcastedMessageName: not mapped from DTO
            };
        }

        #endregion

    }
}
