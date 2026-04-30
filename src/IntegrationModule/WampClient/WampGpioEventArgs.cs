using System;

namespace Wamp.Client
{
    /// <summary>
    /// Provides the device directory number and GPIO payload associated with a WAMP GPIO event.
    /// </summary>
    public sealed class WampGpioEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the directory number of the device that produced the GPIO event.
        /// </summary>
        public string Dirno { get; private set; }

        /// <summary>
        /// Gets the raw GPIO element payload received from WAMP.
        /// </summary>
        public WampClient.wamp_device_gpio_element Element { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WampGpioEventArgs"/> class.
        /// </summary>
        /// <param name="dirno">The directory number that identifies the source device.</param>
        /// <param name="element">The GPIO payload associated with the event.</param>
        public WampGpioEventArgs(string dirno, WampClient.wamp_device_gpio_element element)
        {
            Dirno = dirno;
            Element = element;
        }
    }
}
