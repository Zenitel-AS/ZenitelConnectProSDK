using System;
using System.Collections.Generic;
using System.Text;
using static Wamp.Client.WampClient;

namespace ConnectPro.Models.AudioAnalytics
{
    public class DataReceived
    {
        public string FromDirno { get; set; }

        /// <summary> </summary>
        public string FromName { get; set; }

        /// <summary> </summary>
        public string Status { get; set; }

        /// <summary> </summary>
        public string UCT_time { get; set; }

        public DataReceived(wamp_audio_data_receiving audioData)
        {
            FromDirno = audioData.from_dirno;
            FromName = audioData.from_name;
            Status = audioData.status;
            UCT_time = audioData.UCT_time;
        }
    }
}
