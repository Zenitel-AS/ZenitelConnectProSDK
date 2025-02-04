using ConnectPro.Models;
using System.Collections.Generic;
using Wamp.Client;

namespace ConnectPro.Tools
{
    /// <summary>
    /// Provides utility methods for converting SDK elements into application-specific objects.
    /// </summary>
    public class ObjectConverter
    {
        #region Device Conversion

        /// <summary>
        /// Converts a list of SDK device registration elements into a list of <see cref="Device"/> objects.
        /// </summary>
        /// <param name="list">The list of SDK device registration elements.</param>
        /// <returns>A list of converted <see cref="Device"/> objects.</returns>
        public static List<Device> ConvertSdkDeviceElementList(List<WampClient.wamp_device_registration_element> list)
        {
            if (list == null)
                return new List<Device>();

            List<Device> convertedList = new List<Device>();
            foreach (WampClient.wamp_device_registration_element sdkDeviceElement in list)
            {
                convertedList.Add(new Device(sdkDeviceElement));
            }
            return convertedList;
        }

        #endregion

        #region Group Conversion

        /// <summary>
        /// Converts a list of SDK group elements into a list of <see cref="Group"/> objects.
        /// </summary>
        /// <param name="list">The list of SDK group elements.</param>
        /// <returns>A list of converted <see cref="Group"/> objects.</returns>
        public static List<Group> ConvertSdkGroupElementList(List<WampClient.wamp_group_element> list)
        {
            if (list == null)
                return new List<Group>();

            List<Group> convertedList = new List<Group>();
            foreach (WampClient.wamp_group_element sdkGroupElement in list)
            {
                convertedList.Add(new Group(sdkGroupElement));
            }
            return convertedList;
        }

        #endregion

        #region Audio Message Conversion

        /// <summary>
        /// Converts a list of SDK audio message elements into a list of <see cref="AudioMessage"/> objects.
        /// </summary>
        /// <param name="list">The list of SDK audio message elements.</param>
        /// <returns>A list of converted <see cref="AudioMessage"/> objects.</returns>
        public static List<AudioMessage> ConvertSdkAudioMessageElementList(List<WampClient.wamp_audio_messages_element> list)
        {
            if (list == null)
                return new List<AudioMessage>();

            List<AudioMessage> convertedList = new List<AudioMessage>();
            foreach (WampClient.wamp_audio_messages_element sdkAudioMessageElement in list)
            {
                convertedList.Add(new AudioMessage(sdkAudioMessageElement));
            }
            return convertedList;
        }

        /// <summary>
        /// Converts an SDK audio message wrapper into an <see cref="AudioMessageWrapper"/> object.
        /// </summary>
        /// <param name="sdkAudioMessageWrapper">The SDK audio message wrapper.</param>
        /// <returns>A converted <see cref="AudioMessageWrapper"/> object.</returns>
        public static AudioMessageWrapper ConvertSdkAudioMessageWrapper(WampClient.AudioMessageWrapper sdkAudioMessageWrapper)
        {
            return new AudioMessageWrapper()
            {
                AudioMessages = ConvertSdkAudioMessageElementList(sdkAudioMessageWrapper.AudioMessages),
                AvailableSpace = sdkAudioMessageWrapper.AvailableSpace,
                UsedSpace = sdkAudioMessageWrapper.UsedSpace
            };
        }

        #endregion
    }
}
