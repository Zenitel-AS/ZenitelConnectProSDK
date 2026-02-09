using System.ComponentModel;
using Newtonsoft.Json;

namespace ConnectPro.Models
{
    /// <summary>
    /// Represents a call forwarding rule that can be applied to a directory number.
    /// Maps to the /api/call_forwarding API endpoint.
    /// </summary>
    public class CallForwardingRule : INotifyPropertyChanged
    {
        private string _dirno;
        private string _fwdType;
        private string _fwdTo;
        private bool _enabled;

        /// <summary>
        /// The directory number that owns this forwarding rule.
        /// </summary>
        [JsonProperty("dirno")]
        public string Dirno
        {
            get => _dirno;
            set
            {
                if (_dirno != value)
                {
                    _dirno = value;
                    OnPropertyChanged(nameof(Dirno));
                }
            }
        }

        /// <summary>
        /// The type of forwarding: "unconditional", "on_busy", or "on_timeout".
        /// </summary>
        [JsonProperty("fwd_type")]
        public string FwdType
        {
            get => _fwdType;
            set
            {
                if (_fwdType != value)
                {
                    _fwdType = value;
                    OnPropertyChanged(nameof(FwdType));
                }
            }
        }

        /// <summary>
        /// The directory number to forward calls to.
        /// </summary>
        [JsonProperty("fwd_to")]
        public string FwdTo
        {
            get => _fwdTo;
            set
            {
                if (_fwdTo != value)
                {
                    _fwdTo = value;
                    OnPropertyChanged(nameof(FwdTo));
                }
            }
        }

        /// <summary>
        /// Indicates whether the forwarding rule is enabled.
        /// </summary>
        [JsonProperty("enabled")]
        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    OnPropertyChanged(nameof(Enabled));
                }
            }
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Invokes the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
