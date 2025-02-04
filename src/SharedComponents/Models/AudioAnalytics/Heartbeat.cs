using System;
using static Wamp.Client.WampClient;

namespace ConnectPro.Models.AudioAnalytics
{
    /// <summary>
    /// Represents a heartbeat signal received from an audio detector.
    /// </summary>
    public class Heartbeat
    {
        #region Properties

        /// <summary>
        /// Gets or sets the directory number of the device that sent the heartbeat signal.
        /// </summary>
        public string FromDirno { get; set; }

        /// <summary>
        /// Gets or sets the name of the device that sent the heartbeat signal.
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the heartbeat signal was received, in UTC format.
        /// </summary>
        public string UCT_time { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Heartbeat"/> class 
        /// using the provided WAMP audio detector alive event.
        /// </summary>
        /// <param name="detectorAlive">The heartbeat signal from the WAMP event.</param>
        public Heartbeat(wamp_audio_detector_alive detectorAlive)
        {
            FromDirno = detectorAlive.from_dirno;
            FromName = detectorAlive.from_name;
            UCT_time = detectorAlive.UCT_time;
        }

        #endregion
    }
}
