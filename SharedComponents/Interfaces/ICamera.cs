using ConnectPro.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConnectPro.Interfaces
{
    public interface ICamera
    {
        int Id { get; set; }
        string FQID { get; set; }
        string Name { get; set; }
        ICollection<Device> AssosiatedDevices { get; set; }
        ICollection<DeviceCamera> DeviceCameras { get; set; }
    }
}
