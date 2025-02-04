namespace ConnectPro.Models
{
    /// <summary>
    /// Represents the base class for device settings, providing a link to the associated device.
    /// </summary>
    public abstract class DeviceSettings
    {
        #region Properties

        /// <summary>
        /// Gets or sets the unique identifier of the associated device.
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        /// Gets or sets the device associated with these settings.
        /// </summary>
        public virtual Device Device { get; set; }

        #endregion
    }
}
