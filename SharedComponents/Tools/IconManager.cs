using System.Collections.Generic;

namespace ConnectPro.Tools
{
    public static class IconManager
    {
        // A dictionary to map device types to their corresponding icons
        private static readonly Dictionary<string, string> DeviceIconMap = new Dictionary<string, string>
        {
            // Intercom devices
            { "CLEANROOMINTERCOMSTATIONIP-CROR", "Intercom.ico" },
            { "ECPIR-3P", "Intercom.ico" },
            { "IPHOTLINESTATION", "Intercom.ico" },
            { "IPVANDALRESISTANTSUBSTATION", "Intercom.ico" },
            { "IPHDT-DY", "Intercom.ico" },
            { "IPHDT-KDY", "Intercom.ico" },
            { "IPHDT-KY", "Intercom.ico" },
            { "IPHDT-Y", "Intercom.ico" },
            { "TCIA-2-P", "Intercom.ico" },
            { "TCIS-1", "Intercom.ico" },
            { "TCIS-2", "Intercom.ico" },
            { "TCIS-3", "Intercom.ico" },
            { "TCIS-4", "Intercom.ico" },
            { "TCIS-5", "Intercom.ico" },
            { "TCIS-6", "Intercom.ico" },
            { "TCIS-C1", "Intercom.ico" },
            { "TFIE-1", "Intercom.ico" },
            { "TFIE-2", "Intercom.ico" },
            { "TFIE-6", "Intercom.ico" },
            { "TFIX-1-V2", "Intercom.ico" },
            { "TFIX-2-V2", "Intercom.ico" },
            { "TFIX-3-V2", "Intercom.ico" },
            { "TMIS-1", "Intercom.ico" },
            { "TMIS-2", "Intercom.ico" },
            { "TMIS-4", "Intercom.ico" },
            { "VANDALRESISTANTINTERCOMSTATIONVR3G-1", "Intercom.ico" },
            { "VANDALRESISTANTINTERCOMSTATIONVR3G-1P", "Intercom.ico" },
            { "VANDALRESISTANTSUBSTATIONIPVRS-1", "Intercom.ico" },
            { "VR3G-1P", "Intercom.ico" },
            { "ZENITEL-CLIENT", "Intercom.ico" },
            { "TCIV-2+", "Intercom.ico" },
            { "TCIV-3+", "Intercom.ico" },
            { "TCIV-5+", "Intercom.ico" },
            { "TCIV-6+", "Intercom.ico" },
            { "TEIV-1+", "Intercom.ico" },
            { "TEIV-4+", "Intercom.ico" },
            { "TMIV-1+", "Intercom.ico" },
            { "ZDD-1", "Intercom.ico" },

            // Operator stations
            { "CRM-V-2", "Desk.ico" },
            { "IPDDS", "Desk.ico" },
            { "IPDM-V2", "Desk.ico" },
            { "IPDMH-V2", "Desk.ico" },
            { "IPDMHB-V2", "Desk.ico" },
            { "IPDMHB-V2_P", "Desk.ico" },
            { "ITSV-4", "Desk.ico" },
            { "ITSV-5", "Desk.ico" },

            // Speakers
            { "ELSII-10HM", "Speaker.ico" },
            { "ELSII-10LHM", "Speaker.ico" },
            { "ELSII-10PM", "Speaker.ico" },
            { "ELSII-10WM", "Speaker.ico" },
            { "ELSIR-10CM", "Speaker.ico" }
        };

        public static string GetIconForDevice(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return "Intercom.ico"; // Default for null or empty input

            // Normalize the input
            type = type.Replace(" ", "").ToUpper();

            // Try to get the icon for the given device type
            return DeviceIconMap.TryGetValue(type, out var icon) ? icon : "Intercom.ico";
        }
    }
}
