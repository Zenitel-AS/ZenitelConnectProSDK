namespace ConnectPro.Models
{
    public class Configuration
    {
        
        public int ID { get; set; }
        public string ControllerName { get; set; } = "";
        public string ServerAddr { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Port { get; set; }
        public string Realm { get; set; }
        public string OperatorDirNo { get; set; } = "";
        public bool DisplayConfigurationInSmartClient { get; set; } = true;
        public bool EnablePopupWindow { get; set; } = true;

        public string MachineName { get; set; } = "";
        public Operator Operator { get; set; }

        private (bool, int) ConfigurationHasEmptyValues()
        {
            if (this.ServerAddr == null) return (true, 0);
            if (this.UserName == null) return (true, 1);
            if (this.Password == null) return (true, 2);
            if (this.Port == null) return (true, 3);
            if (this.Realm == null) return (true, 4);
            if (this.OperatorDirNo == null) return (true, 5);
            return (false, 0);
        }
        public void FillEmptyValuesWithDefaults()
        {
            (bool, int) EmptyValues = ConfigurationHasEmptyValues();
            while (EmptyValues != (false, 0))
            {
                switch (EmptyValues.Item2)
                {
                    case (0):
                        this.ServerAddr = "169.254.1.5";
                        break;
                    case (1):
                        this.UserName = "";
                        break;
                    case (2):
                        this.Password = "";
                        break;
                    case (3):
                        this.Port = "8086";
                        break;
                    case (4):
                        this.Realm = "zenitel";
                        break;
                    case (5):
                        this.OperatorDirNo = "";
                        break;
                }
                EmptyValues = ConfigurationHasEmptyValues();
            }
        }
    }
}
