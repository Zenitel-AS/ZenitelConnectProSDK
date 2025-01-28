using System;
using System.Collections.Generic;
using System.Text;
using static Wamp.Client.WampClient;

namespace ConnectPro.Models.AudioAnalytics
{
    public class Heartbeat
    {
        public string FromDirno { get; set; }

        /// <summary> </summary>
        public string FromName { get; set; }

        /// <summary> </summary>
        public string UCT_time { get; set; }

        public Heartbeat(wamp_audio_detector_alive detectorAlive)
        {
            FromDirno = detectorAlive.from_dirno;
            FromName = detectorAlive.from_name;
            UCT_time = detectorAlive.UCT_time;
        }
    }
}
