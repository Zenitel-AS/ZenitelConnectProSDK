using ConnectPro.Models;
using System.Collections.Generic;
using Wamp.Client;

namespace ConnectPro.Tools
{
    public class ObjectConverter
    {
        public static List<Device> ConvertSdkDeviceElementList(List<WampClient.wamp_device_registration_element> list)
        {
            List<Device> convertedList = new List<Device>();
            foreach (WampClient.wamp_device_registration_element sdk_device_element in list)
            {
                convertedList.Add(new Device(sdk_device_element));
            }
            return convertedList;
        }
    }
}
