using System.ComponentModel.DataAnnotations;

namespace ConnectPro.Models
{
    public class DeviceCamera
    {
        [Key]
        public int Id { get; set; }

        public int DeviceId { get; set; }
        public int CameraId { get; set; }

        public virtual Device Device { get; set; }
        public virtual Camera Camera { get; set; }
    }
}
