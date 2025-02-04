using System;
using static Wamp.Client.WampClient;

namespace ConnectPro.Models.AudioAnalytics
{
    /// <summary>
    /// Represents an audio event detected by the system, including event type, probability, and source information.
    /// </summary>
    public class AudioEventDetected
    {
        #region Properties

        /// <summary>
        /// Gets or sets the directory number (Dirno) of the device that detected the audio event.
        /// </summary>
        public string FromDirno { get; set; }

        /// <summary>
        /// Gets or sets the name of the device that detected the audio event.
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// Gets or sets the type of the detected audio event (e.g., gunshot, glass break, etc.).
        /// </summary>
        public string EventType { get; set; }

        /// <summary>
        /// Gets or sets the probability of the detected event as a string.
        /// </summary>
        public string Probability { get; set; }

        /// <summary>
        /// Gets or sets the UTC time when the audio event was detected.
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Gets the probability as a percentage string with a "%" symbol.
        /// </summary>
        public string ProbabilityPercentageLabel
        {
            get
            {
                if (!string.IsNullOrEmpty(Probability) && double.TryParse(Probability, out double probability))
                {
                    return $"{Math.Round(probability * 100, 2)} %"; // Convert probability to percentage
                }
                return "0 %"; // Default value if parsing fails
            }
        }

        /// <summary>
        /// Gets the probability as a numeric percentage.
        /// </summary>
        public double ProbabilityPercentage
        {
            get
            {
                if (!string.IsNullOrEmpty(Probability) && double.TryParse(Probability, out double probability))
                {
                    return Math.Round(probability * 100, 2); // Convert probability to percentage
                }
                return 0; // Default value if parsing fails
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioEventDetected"/> class using data from a WAMP audio event.
        /// </summary>
        /// <param name="wamp_audio_event">The audio event data received from the WAMP client.</param>
        public AudioEventDetected(wamp_audio_event_detection wamp_audio_event)
        {
            FromDirno = wamp_audio_event.from_dirno;
            FromName = wamp_audio_event.from_name;
            EventType = wamp_audio_event.audio_event;
            Probability = wamp_audio_event.probability;

            if (DateTime.TryParse(wamp_audio_event.UCT_time, out DateTime time))
            {
                Time = time;
            }
        }

        #endregion
    }
}
