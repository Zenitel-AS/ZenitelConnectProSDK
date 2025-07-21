using System;
using Newtonsoft.Json;
using ConnectPro.Models;

namespace ConnectPro.DTO
{
    /// <summary>
    /// Data Transfer Object for the CallLog model.
    /// </summary>
    public class CallLogDto
    {
        #region Properties

        /// <summary>
        /// Gets or sets the unique identifier for the call log entry.
        /// </summary>
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
        /// This property is ignored during serialization.
        /// </summary>
        [JsonIgnore]
        public object sender { get; set; }

        #endregion

        #region Model Conversion

        /// <summary>
        /// Converts this DTO into a CallLog model object.
        /// </summary>
        public CallLog ToModel()
        {
            return new CallLog
            {
                Id = this.Id,
                Time = this.Time,
                DeviceName = this.DeviceName,
                Location = this.Location,
                FromDirno = this.FromDirno,
                ToDirno = this.ToDirno,
                AnsweredByDirno = this.AnsweredByDirno,
                CallType = this.CallType,
                Reason = this.Reason,
                State = this.State,
                sender = this.sender
            };
        }

        /// <summary>
        /// Creates a CallLogDto from a CallLog model object.
        /// </summary>
        public static CallLogDto FromModel(CallLog log)
        {
            if (log == null) return null;

            return new CallLogDto
            {
                Id = log.Id,
                Time = log.Time,
                DeviceName = log.DeviceName,
                Location = log.Location,
                FromDirno = log.FromDirno,
                ToDirno = log.ToDirno,
                AnsweredByDirno = log.AnsweredByDirno,
                CallType = log.CallType,
                Reason = log.Reason,
                State = log.State,
                sender = log.sender
            };
        }

        #endregion
    }
}
