using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows;

namespace ConnectPro.Models
{
    public class CallLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public string DeviceName { get; set; }
        public string Location { get; set; }
        public string FromDirno { get; set; }
        public string ToDirno { get; set; }
        public string AnsweredByDirno { get; set; }
        public string CallType { get; set; }
        public string Reason { get; set; }
        public string State { get; set; }
        [JsonIgnore]
        [NotMapped]
        public object sender { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            CallLog other = (CallLog)obj;

            if (other == null)
            {
                return false;
            }

            return this.Time == other.Time &&
                   this.DeviceName == other.DeviceName &&
                   this.FromDirno == other.FromDirno &&
                   this.ToDirno == other.ToDirno &&
                   this.CallType == other.CallType &&
                   this.AnsweredByDirno == other.AnsweredByDirno &&
                   this.State == other.State &&
                   this.Reason == other.Reason &&
                   this.sender == other.sender &&
                   this.Location == other.Location;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17; // Choose a prime number as the initial hash code value

                // Combine the hash codes of the properties
                hash = hash * 23 + Time.GetHashCode();
                hash = hash * 23 + (DeviceName?.GetHashCode() ?? 0);
                hash = hash * 23 + (FromDirno?.GetHashCode() ?? 0);
                hash = hash * 23 + (ToDirno?.GetHashCode() ?? 0);
                hash = hash * 23 + (CallType?.GetHashCode() ?? 0);
                hash = hash * 23 + (AnsweredByDirno?.GetHashCode() ?? 0);
                hash = hash * 23 + (State?.GetHashCode() ?? 0);
                hash = hash * 23 + (Reason?.GetHashCode() ?? 0);
                hash = hash * 23 + (sender?.GetHashCode() ?? 0);
                hash = hash * 23 + (Location?.GetHashCode() ?? 0);

                return hash;
            }
        }


    }
}
