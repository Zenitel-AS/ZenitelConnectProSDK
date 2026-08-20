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
        /// Gets or sets the description of the audio message.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the directory number associated with the audio message.
        /// </summary>
        [JsonProperty("dirno")]
        public string Dirno { get; set; }

        /// <summary>
        /// Gets or sets the directory number identifier.
        /// </summary>
        [JsonProperty("dirno_id")]
        public int? DirnoId { get; set; }

        /// <summary>
        /// Gets or sets the display name of the audio message.
        /// </summary>
        [JsonProperty("displayname")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the duration of the audio message in seconds.
        /// </summary>
        [JsonProperty("duration")]
        public int? Duration { get; set; }

        /// <summary>
        /// Gets or sets the feature type of the audio message.
        /// </summary>
        [JsonProperty("feature_type")]
        public string FeatureType { get; set; }

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
        public int? FileSize { get; set; }

        /// <summary>
        /// Gets or sets the unique message identifier.
        /// </summary>
        [JsonProperty("message_id")]
        public int? MessageId { get; set; }

        /// <summary>
        /// Gets or sets the number of repetitions for the audio message.
        /// </summary>
        [JsonProperty("repetitions")]
        public int? Repetitions { get; set; }

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
            if (sdkAudioMessageElement == null)
                return;

            Description = sdkAudioMessageElement.description ?? string.Empty;
            Dirno = sdkAudioMessageElement.dirno ?? string.Empty;
            DirnoId = sdkAudioMessageElement.dirno_id ?? 0;
            DisplayName = sdkAudioMessageElement.displayname ?? string.Empty;
            Duration = sdkAudioMessageElement.duration ?? 0;
            FeatureType = sdkAudioMessageElement.feature_type ?? string.Empty;
            FileName = sdkAudioMessageElement.filename ?? string.Empty;
            FilePath = sdkAudioMessageElement.filepath ?? string.Empty;
            FileSize = sdkAudioMessageElement.filesize ?? 0;
            MessageId = sdkAudioMessageElement.message_id ?? 0;
            Repetitions = sdkAudioMessageElement.repetitions ?? 0;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates the current <see cref="AudioMessage"/> instance with values from an SDK audio message element.
        /// </summary>
        /// <param name="sdkAudioMessageElement">The SDK audio message element containing new values.</param>
        public void SetValuesFromSDK(WampClient.wamp_audio_messages_element sdkAudioMessageElement)
        {
            Description = sdkAudioMessageElement.description ?? string.Empty;
            Dirno = sdkAudioMessageElement.dirno ?? string.Empty;
            DirnoId = sdkAudioMessageElement.dirno_id ?? 0;
            DisplayName = sdkAudioMessageElement.displayname ?? string.Empty;
            Duration = sdkAudioMessageElement.duration ?? 0;
            FeatureType = sdkAudioMessageElement.feature_type ?? string.Empty;
            FileName = sdkAudioMessageElement.filename ?? string.Empty;
            FilePath = sdkAudioMessageElement.filepath ?? string.Empty;
            FileSize = sdkAudioMessageElement.filesize ?? 0;
            MessageId = sdkAudioMessageElement.message_id ?? 0;
            Repetitions = sdkAudioMessageElement.repetitions ?? 0;
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
                Description = sdkAudioMessageElement.description ?? string.Empty,
                Dirno = sdkAudioMessageElement.dirno ?? string.Empty,
                DirnoId = sdkAudioMessageElement.dirno_id ?? 0,
                DisplayName = sdkAudioMessageElement.displayname ?? string.Empty,
                Duration = sdkAudioMessageElement.duration ?? 0,
                FeatureType = sdkAudioMessageElement.feature_type ?? string.Empty,
                FileName = sdkAudioMessageElement.filename ?? string.Empty,
                FilePath = sdkAudioMessageElement.filepath ?? string.Empty,
                FileSize = sdkAudioMessageElement.filesize ?? 0,
                MessageId = sdkAudioMessageElement.message_id ?? 0,
                Repetitions = sdkAudioMessageElement.repetitions ?? 0
            };
        }

        #endregion

        #region DTO Conversion
        /// <summary>
        /// Converts the current <see cref="AudioMessage"/> instance to an <see cref="AudioMessageDto"/> object.
        /// </summary>
        /// <returns>An <see cref="AudioMessageDto"/> that contains the data from this <see cref="AudioMessage"/> instance.</returns>
        public AudioMessageDto ToDto()
        {
            return new AudioMessageDto
            {
                Id = this.Id,
                Description = this.Description ?? string.Empty,
                Dirno = this.Dirno ?? string.Empty,
                DirnoId = this.DirnoId ?? 0,
                DisplayName = this.DisplayName ?? string.Empty,
                Duration = this.Duration ?? 0,
                FeatureType = this.FeatureType ?? string.Empty,
                FileName = this.FileName ?? string.Empty,
                FilePath = this.FilePath ?? string.Empty,
                FileSize = this.FileSize ?? 0,
                MessageId = this.MessageId ?? 0,
                Repetitions = this.Repetitions ?? 0,
                IsPlaying = this.IsPlaying
            };
        }
        /// <summary>
        /// Creates a new <see cref="AudioMessage"/> instance from the specified <see cref="AudioMessageDto"/>.
        /// </summary>
        /// <param name="dto">The data transfer object containing audio message information to convert. Can be <see langword="null"/>.</param>
        /// <returns>An <see cref="AudioMessage"/> initialized with values from <paramref name="dto"/>, or <see langword="null"/>
        /// if <paramref name="dto"/> is <see langword="null"/>.</returns>
        public static AudioMessage FromDto(AudioMessageDto dto)
        {
            if (dto == null) return null;

            return new AudioMessage
            {
                Id = dto.Id,
                Description = dto.Description ?? string.Empty,
                Dirno = dto.Dirno ?? string.Empty,
                DirnoId = dto.DirnoId ?? 0,
                DisplayName = dto.DisplayName ?? string.Empty,
                Duration = dto.Duration ?? 0,
                FeatureType = dto.FeatureType ?? string.Empty,
                FileName = dto.FileName ?? string.Empty,
                FilePath = dto.FilePath ?? string.Empty,
                FileSize = dto.FileSize ?? 0, 
                MessageId = dto.MessageId ?? 0,
                Repetitions = dto.Repetitions ?? 0,
                IsPlaying = dto.IsPlaying
            };
        }
        #endregion
    }
}
