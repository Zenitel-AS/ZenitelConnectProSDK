namespace ConnectPro.Models
{
    public abstract class DeviceSettings
    {
        public int DeviceId { get; set; }
        public virtual Device Device { get; set; }
    }
}
