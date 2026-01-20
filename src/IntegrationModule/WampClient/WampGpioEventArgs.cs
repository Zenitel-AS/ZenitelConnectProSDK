using System;

namespace Wamp.Client
{
    public sealed class WampGpioEventArgs : EventArgs
    {
        public string Dirno { get; private set; }
        public WampClient.wamp_device_gpio_element Element { get; private set; }

        public WampGpioEventArgs(string dirno, WampClient.wamp_device_gpio_element element)
        {
            Dirno = dirno;
            Element = element;
        }
    }
}
