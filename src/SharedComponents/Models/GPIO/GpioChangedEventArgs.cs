using System;

namespace ConnectPro.Models
{
    /// <summary>
    /// Provides details about a GPIO point update raised by a device.
    /// </summary>
    public sealed class GpioChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the directory number of the device whose GPIO changed.
        /// </summary>
        public string Dirno { get; }

        /// <summary>
        /// Gets the GPIO point that changed.
        /// </summary>
        public GpioPoint Point { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GpioChangedEventArgs"/> class.
        /// </summary>
        /// <param name="dirno">The directory number of the source device.</param>
        /// <param name="point">The GPIO point that changed.</param>
        public GpioChangedEventArgs(string dirno, GpioPoint point)
        {
            Dirno = dirno;
            Point = point;
        }
    }

}
