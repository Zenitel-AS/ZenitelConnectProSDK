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
        /// Gets or sets the directory number associated with the audio message.
        /// </summary>
        [JsonProperty("dirno")]
        public string Dirno { get; set; } = string.Empty;

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
                Dirno = this.Dirno,
                FileName = this.FileName,
                FilePath = this.FilePath,
                FileSize = this.FileSize,
                Duration = this.Duration,
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
                Dirno = msg.Dirno,
                FileName = msg.FileName,
                FilePath = msg.FilePath,
                FileSize = msg.FileSize,
                Duration = msg.Duration,
                IsPlaying = msg.IsPlaying
            };
        }

        #endregion
    }
}
