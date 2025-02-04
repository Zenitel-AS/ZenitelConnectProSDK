using System;
using static Wamp.Client.WampClient;

namespace ConnectPro.Models.AudioAnalytics
{
    /// <summary>
    /// Represents data received from an audio analytics event.
    /// </summary>
    public class DataReceived
    {
        #region Properties

        /// <summary>
        /// Gets or sets the directory number of the device that sent the data.
        /// </summary>
        public string FromDirno { get; set; }

        /// <summary>
        /// Gets or sets the name of the device that sent the data.
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// Gets or sets the status of the received audio data.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the data was received, in UTC format.
        /// </summary>
        public string UCT_time { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DataReceived"/> class 
        /// using the provided WAMP audio data receiving event.
        /// </summary>
        /// <param name="audioData">The received audio data from the WAMP event.</param>
        public DataReceived(wamp_audio_data_receiving audioData)
        {
            FromDirno = audioData.from_dirno;
            FromName = audioData.from_name;
            Status = audioData.status;
            UCT_time = audioData.UCT_time;
        }

        #endregion
    }
}
