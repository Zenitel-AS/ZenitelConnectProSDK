using System;
using System.Collections.Generic;
using System.Text;
using static Wamp.Client.WampClient;

namespace ConnectPro.Models.AudioAnalytics
{
    public class AudioEventDetected
    {
        /// <summary> </summary>
        public string FromDirno { get; set; }

        /// <summary> </summary>
        public string FromName { get; set; }

        /// <summary> </summary>
        public string EventType { get; set; }

        /// <summary></summary>
        public string Probability { get; set; }

        public DateTime Time { get; set; }

        public string ProbabilityPercentageLabel
        {
            get
            {
                if (!String.IsNullOrEmpty(Probability) && double.TryParse(Probability, out double probability))
                {
                    return $"{Math.Round(probability * 100, 2)} %"; // Convert probability to percentage
                }
                return "0 %"; // Default value if parsing fails
            }
        }
        public double ProbabilityPercentage
        {
            get
            {
                if (!String.IsNullOrEmpty(Probability) && double.TryParse(Probability, out double probability))
                {
                    return Math.Round(probability * 100, 2); // Convert probability to percentage
                }
                return 0; // Default value if parsing fails
            }
        }

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
    }
}
