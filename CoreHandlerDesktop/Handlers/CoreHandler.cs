using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreHandlerDesktop
{
    public static class CoreHandler
    {
        private static ConnectPro.Core _core;

        public static bool IsConnected = false;


        public static ConnectPro.Core Core
        {
            get
            {
                if (_core == null)
                {
                    _core = new ConnectPro.Core()
                    {
                        Configuration = new ConnectPro.Configuration()
                        {
                            ServerAddr = "10.8.32.10",
                            UserName = "int",
                            Password = "int12345",
                            Realm = "",
                            Port = "8086",
                            MachineName = "TestMachine",
                            ControllerName = "TestCore",
                            DisplayConfigurationInSmartClient = false,
                            EnablePopupWindow = false,
                            ErrorLogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ErrorLog.txt"),
                            Operator = null,
                            OperatorDirNo = "200"
                        }
                    };
                    _core.Start();
                }
                return _core;
            }
            set { _core = value; }
        }

        public static void Dispose()
        {
            if (_core != null)
            {
                _core.Dispose();
                _core = null;
            }
        }
    }
}
