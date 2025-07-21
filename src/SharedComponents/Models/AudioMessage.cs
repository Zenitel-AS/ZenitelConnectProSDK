using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using Newtonsoft.Json;
using Wamp.Client;
using ConnectPro.DTO;

namespace ConnectPro.Models
{
    /// <summary>
    /// Represents a wrapper for audio messages, including available and used space information.
    /// </summary>
    public class AudioMessageWrapper
    {
        #region Properties

        /// <summary>
        /// Gets or sets the list of audio messages.
        /// </summary>
        [JsonProperty("audio_messages")]
        public List<AudioMessage> AudioMessages { get; set; }

        /// <summary>
        /// Gets or sets the available storage space for audio messages.
        /// </summary>
        [JsonProperty("available_space")]
        public int AvailableSpace { get; set; }

        /// <summary>
        /// Gets or sets the used storage space for audio messages.
        /// </summary>
        [JsonProperty("used_space")]
        public int? UsedSpace { get; set; }

        #endregion
    }

    /// <summary>
    /// Represents an audio message entity with metadata and playback status.
    /// </summary>
    [Serializable]
    public class AudioMessage : INotifyPropertyChanged
    {
        #region Fields

        private bool _isPlaying;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the unique identifier for the audio message.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the directory number associated with the audio message.
        /// </summary>
        [JsonProperty("dirno")]
        public string Dirno { get; set; }

        /// <summary>
        /// Gets or sets the filename of the audio message.
        /// </summary>
        [JsonProperty("filename")]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the file path where the audio message is stored.
        /// </summary>
        [JsonProperty("filepath")]
        public string FilePath { get; set; }

        /// <summary>
        /// Gets or sets the file size of the audio message in bytes.
        /// </summary>
        [JsonProperty("filesize")]
        public int FileSize { get; set; }

        /// <summary>
        /// Gets or sets the duration of the audio message in seconds.
        /// </summary>
        [JsonProperty("duration")]
        public int? Duration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the audio message is currently playing.
        /// </summary>
        [NotMapped]
        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                if (_isPlaying != value)
                {
                    _isPlaying = value;
                    OnPropertyChanged(nameof(IsPlaying));
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Event triggered when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Invokes the <see cref="PropertyChanged"/> event when a property value changes.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioMessage"/> class.
        /// </summary>
        public AudioMessage() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioMessage"/> class from an SDK audio message element.
        /// </summary>
        /// <param name="sdkAudioMessageElement">The SDK audio message element to initialize the object from.</param>
        public AudioMessage(WampClient.wamp_audio_messages_element sdkAudioMessageElement)
        {
            Dirno = sdkAudioMessageElement.dirno;
            FileName = sdkAudioMessageElement.filename;
            FilePath = sdkAudioMessageElement.filepath;
            FileSize = sdkAudioMessageElement.filesize;
            Duration = sdkAudioMessageElement.duration;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates the current <see cref="AudioMessage"/> instance with values from an SDK audio message element.
        /// </summary>
        /// <param name="sdkAudioMessageElement">The SDK audio message element containing new values.</param>
        public void SetValuesFromSDK(WampClient.wamp_audio_messages_element sdkAudioMessageElement)
        {
            Dirno = sdkAudioMessageElement.dirno;
            FileName = sdkAudioMessageElement.filename;
            FilePath = sdkAudioMessageElement.filepath;
            FileSize = sdkAudioMessageElement.filesize;
            Duration = sdkAudioMessageElement.duration;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="AudioMessage"/> class from an SDK audio message element.
        /// </summary>
        /// <param name="sdkAudioMessageElement">The SDK audio message element to convert.</param>
        /// <returns>A new instance of <see cref="AudioMessage"/> with properties populated from the SDK element.</returns>
        public static AudioMessage NewDeviceFromSdkElement(WampClient.wamp_audio_messages_element sdkAudioMessageElement)
        {
            return new AudioMessage()
            {
                Dirno = sdkAudioMessageElement.dirno,
                FileName = sdkAudioMessageElement.filename,
                FilePath = sdkAudioMessageElement.filepath,
                FileSize = sdkAudioMessageElement.filesize,
                Duration = sdkAudioMessageElement.duration
            };
        }

        #endregion

        #region DTO Conversion
        public AudioMessageDto ToDto()
        {
            return new AudioMessageDto
            {
                Id = this.Id,
                Dirno = this.Dirno,
                FileName = this.FileName,
                FilePath = this.FilePath,
                FileSize = this.FileSize,
                Duration = this.Duration,
                IsPlaying = this.IsPlaying
            };
        }

        public static AudioMessage FromDto(AudioMessageDto dto)
        {
            if (dto == null) return null;

            return new AudioMessage
            {
                Id = dto.Id,
                Dirno = dto.Dirno,
                FileName = dto.FileName,
                FilePath = dto.FilePath,
                FileSize = dto.FileSize,
                Duration = dto.Duration,
                IsPlaying = dto.IsPlaying
            };
        }
        #endregion
    }
}
