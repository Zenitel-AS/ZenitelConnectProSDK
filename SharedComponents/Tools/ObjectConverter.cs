using ConnectPro.Models;
using System.Collections.Generic;
using Wamp.Client;

namespace ConnectPro.Tools
{
    public class ObjectConverter
    {
        public static List<Device> ConvertSdkDeviceElementList(List<WampClient.wamp_device_registration_element> list)
        {
            if (list == null)
                return new List<Device>();

            List<Device> convertedList = new List<Device>();
            foreach (WampClient.wamp_device_registration_element sdk_device_element in list)
            {
                convertedList.Add(new Device(sdk_device_element));
            }
            return convertedList;
        }

        public static List<Group> ConvertSdkGroupElementList(List<WampClient.wamp_group_element> list)
        {
            if (list == null)
                return new List<Group>();

            List<Group> convertedList = new List<Group>();
            foreach (WampClient.wamp_group_element sdk_group_element in list)
            {
                convertedList.Add(new Group(sdk_group_element));
            }
            return convertedList;
        }

        public static List<AudioMessage> ConvertSdkAudioMessageElementList(List<WampClient.wamp_audio_messages_element> list)
        {
            if (list == null)
                return new List<AudioMessage>();

            List<AudioMessage> convertedList = new List<AudioMessage>();
            foreach (WampClient.wamp_audio_messages_element sdk_audio_message_element in list)
            {
                convertedList.Add(new AudioMessage(sdk_audio_message_element));
            }
            return convertedList;
        }

        public static AudioMessageWrapper ConvertSdkAudioMessageWrapper(WampClient.AudioMessageWrapper sdkAudioMessageWrapper)
        {
            return new AudioMessageWrapper()
            {
                AudioMessages = ConvertSdkAudioMessageElementList(sdkAudioMessageWrapper.AudioMessages),
                AvailableSpace = sdkAudioMessageWrapper.AvailableSpace,
                UsedSpace = sdkAudioMessageWrapper.UsedSpace
            };
        }
    }
}
