using ConnectPro.Models;
using Newtonsoft.Json;

namespace ConnectPro.DTO
{
    /// <summary>
    /// Data Transfer Object for the AudioMessage model.
    /// </summary>
    public class AudioMessageDto
    {
        #region Properties

        /// <summary>
        /// Gets or sets the unique identifier for the audio message.
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the description of the audio message.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the directory number associated with the audio message.
        /// </summary>
        [JsonProperty("dirno")]
        public string Dirno { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the directory number identifier.
        /// </summary>
        [JsonProperty("dirno_id", NullValueHandling = NullValueHandling.Ignore)]
        public int DirnoId { get; set; }

        /// <summary>
        /// Gets or sets the display name of the audio message.
        /// </summary>
        [JsonProperty("displayname")]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the duration of the audio message in seconds.
        /// </summary>
        [JsonProperty("duration")]
        public int? Duration { get; set; }

        /// <summary>
        /// Gets or sets the feature type of the audio message.
        /// </summary>
        [JsonProperty("feature_type")]
        public string FeatureType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the filename of the audio message.
        /// </summary>
        [JsonProperty("filename")]
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file path where the audio message is stored.
        /// </summary>
        [JsonProperty("filepath")]
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file size of the audio message in bytes.
        /// </summary>
        [JsonProperty("filesize", NullValueHandling = NullValueHandling.Ignore)]
        public int FileSize { get; set; }

        /// <summary>
        /// Gets or sets the unique message identifier.
        /// </summary>
        [JsonProperty("message_id", NullValueHandling = NullValueHandling.Ignore)]
        public int MessageId { get; set; }

        /// <summary>
        /// Gets or sets the number of repetitions for the audio message.
        /// </summary>
        [JsonProperty("repetitions", NullValueHandling = NullValueHandling.Ignore)]
        public int Repetitions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the audio message is currently playing.
        /// </summary>
        [JsonProperty("is_playing")]
        public bool IsPlaying { get; set; }

        #endregion

        #region Model Conversion

        /// <summary>
        /// Converts this DTO to an <see cref="AudioMessage"/> model instance.
        /// </summary>
        public AudioMessage ToModel()
        {
            return new AudioMessage
            {
                Id = this.Id,
                Description = this.Description ?? string.Empty,
                Dirno = this.Dirno ?? string.Empty,
                DirnoId = this.DirnoId,
                DisplayName = this.DisplayName ?? string.Empty,
                Duration = this.Duration ?? 0,
                FeatureType = this.FeatureType ?? string.Empty,
                FileName = this.FileName ?? string.Empty,
                FilePath = this.FilePath ?? string.Empty,
                FileSize = this.FileSize,
                MessageId = this.MessageId,
                Repetitions = this.Repetitions,
                IsPlaying = this.IsPlaying
            };
        }

        /// <summary>
        /// Creates a new instance of the <see cref="AudioMessageDto"/> from an AudioMessage model instance.
        /// </summary>
        public static AudioMessageDto FromModel(AudioMessage msg)
        {
            if (msg == null) return null;

            return new AudioMessageDto
            {
                Id = msg.Id,
                Description = msg.Description ?? string.Empty,
                Dirno = msg.Dirno ?? string.Empty,
                DirnoId = msg.DirnoId,
                DisplayName = msg.DisplayName ?? string.Empty,
                Duration = msg.Duration ?? 0,
                FeatureType = msg.FeatureType ?? string.Empty,
                FileName = msg.FileName ?? string.Empty,
                FilePath = msg.FilePath ?? string.Empty,
                FileSize = msg.FileSize,
                MessageId = msg.MessageId,
                Repetitions = msg.Repetitions,
                IsPlaying = msg.IsPlaying
            };
        }

        #endregion
    }
}
