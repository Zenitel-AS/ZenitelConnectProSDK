using System;
using System.Collections.Generic;
using System.Text;

namespace ConnectPro.Models
{
    public class ExceptionLog
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Sender { get; set; }
        public string Message { get; set; }
        public DateTime DateTime { get; set; }
    }
}
