using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConnectPro.Models
{
    /// <summary>
    /// Represents a call log entry, storing details about a call event.
    /// </summary>
    public class CallLog
    {
        #region Properties

        /// <summary>
        /// Gets or sets the unique identifier for the call log entry.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of when the call log entry was created.
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Gets or sets the name of the device involved in the call.
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the location where the call took place.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets the directory number of the caller.
        /// </summary>
        public string FromDirno { get; set; }

        /// <summary>
        /// Gets or sets the directory number of the recipient.
        /// </summary>
        public string ToDirno { get; set; }

        /// <summary>
        /// Gets or sets the directory number of the user who answered the call.
        /// </summary>
        public string AnsweredByDirno { get; set; }

        /// <summary>
        /// Gets or sets the type of call (e.g., normal, queued, group call).
        /// </summary>
        public string CallType { get; set; }

        /// <summary>
        /// Gets or sets the reason for the call event (e.g., success, failure, rejected).
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Gets or sets the current state of the call (e.g., in progress, ended).
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the sender object associated with the call log.
        /// This property is ignored during serialization and is not mapped to the database.
        /// </summary>
        [JsonIgnore]
        [NotMapped]
        public object sender { get; set; }

        #endregion

        #region Equality Comparison

        /// <summary>
        /// Determines whether the specified object is equal to the current <see cref="CallLog"/> instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><c>true</c> if the objects are equal; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (obj is CallLog other)
            {
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
            return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for the current <see cref="CallLog"/> instance.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17; // Prime number for hash initialization

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

        #endregion
    }
}
