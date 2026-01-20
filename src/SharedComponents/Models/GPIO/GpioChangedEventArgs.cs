using System;

namespace ConnectPro.Models
{
    public sealed class GpioChangedEventArgs : EventArgs
    {
        public string Dirno { get; }
        public GpioPoint Point { get; }

        public GpioChangedEventArgs(string dirno, GpioPoint point)
        {
            Dirno = dirno;
            Point = point;
        }
    }

}
