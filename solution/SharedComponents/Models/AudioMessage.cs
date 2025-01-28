using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Wamp.Client;
using Newtonsoft.Json;
using System.ComponentModel;

namespace ConnectPro.Models
{
    public class AudioMessageWrapper
    {
        [JsonProperty("audio_messages")]
        public List<AudioMessage> AudioMessages { get; set; }

        [JsonProperty("available_space")]
        public int AvailableSpace { get; set; }

        [JsonProperty("used_space")]
        public int? UsedSpace { get; set; }
    }

    [Serializable]
    public class AudioMessage : INotifyPropertyChanged
    {
        /// <summary>
        /// Property that serves as the primary key for the Device entity in the Entity Framework. 
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [JsonProperty("dirno")]
        public string Dirno { get; set; }

        [JsonProperty("filename")]
        public string FileName { get; set; }

        [JsonProperty("filepath")]
        public string FilePath { get; set; }

        [JsonProperty("filesize")]
        public int FileSize { get; set; }

        [JsonProperty("duration")]
        public int? Duration { get; set; }

        
        private bool _isPlaying;
        [NotMapped]
        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                if (_isPlaying != value)
                {
                    _isPlaying = value;
                    OnPropertyChanged(nameof(IsPlaying));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public AudioMessage() { }
        /// <summary>
        /// Constructor method that initializes a AudioMessage instance by setting its 
        /// properties to the properties of the `sdk_audio_message_element` parameter.
        /// </summary>
        /// <param name="sdk_audio_message_element"></param>
        public AudioMessage(WampClient.wamp_audio_messages_element sdk_audio_message_element)
        {
            this.Dirno = sdk_audio_message_element.dirno;
            this.FileName = sdk_audio_message_element.filename;
            this.FilePath = sdk_audio_message_element.filepath;
            this.FileSize = sdk_audio_message_element.filesize;
            this.Duration = sdk_audio_message_element.duration;
        }
        /// <summary>
        /// Takes the values from the WampClient.wamp_audio_messages_element and applyies them tho the AudioMessage object
        /// </summary>
        /// <param name="sdk_audio_message_element"></param>
        public void SetValuesFromSDK(WampClient.wamp_audio_messages_element sdk_audio_message_element)
        {
            this.Dirno = sdk_audio_message_element.dirno;
            this.FileName = sdk_audio_message_element.filename;
            this.FilePath = sdk_audio_message_element.filepath;
            this.FileSize = sdk_audio_message_element.filesize;
            this.Duration = sdk_audio_message_element.duration;
        }
        /// <summary>
        /// Creates a new instance of the <see cref="AudioMessage"/> class from a <see cref="WampClient.wamp_audio_messages_element"/> object.
        /// </summary>
        /// <param name="sdk_audio_message_element">The <see cref="WampClient.wamp_audio_messages_element"/> object to create the <see cref="AudioMessage"/> from.</param>
        /// <returns>A new instance of the <see cref="AudioMessage"/> class with its properties populated from the <paramref name="sdk_audio_message_element"/> object.</returns>
        public static AudioMessage NewDeviceFromSdkElement(WampClient.wamp_audio_messages_element sdk_audio_message_element)
        {
            return new AudioMessage()
            {
                Dirno = sdk_audio_message_element.dirno,
                FileName = sdk_audio_message_element.filename,
                FilePath = sdk_audio_message_element.filepath,
                FileSize = sdk_audio_message_element.filesize,
                Duration = sdk_audio_message_element.duration
            };
        }
    }
}
