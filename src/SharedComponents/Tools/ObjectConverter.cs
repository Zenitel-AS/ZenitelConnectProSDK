using ConnectPro.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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

    /// <summary>
    /// Provides utility methods for reconciling two collections by key.
    /// </summary>
    public static class CollectionReconciler
    {
        /// <summary>
        /// Computes added, removed and updated members between existing and latest collections.
        /// </summary>
        /// <typeparam name="TItem">The item type.</typeparam>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <param name="existingItems">The existing collection snapshot.</param>
        /// <param name="latestItems">The latest collection snapshot.</param>
        /// <param name="keySelector">A function that returns a stable key for each item.</param>
        /// <param name="keyComparer">The key comparer.</param>
        /// <param name="hasChanged">Optional function used to detect member updates.</param>
        /// <param name="addedItems">Outputs members that only exist in latest snapshot.</param>
        /// <param name="removedItems">Outputs members that only exist in existing snapshot.</param>
        /// <returns><c>true</c> if any member update was detected for matching keys; otherwise <c>false</c>.</returns>
        public static bool DiffByKey<TItem, TKey>(
            IEnumerable<TItem> existingItems,
            IEnumerable<TItem> latestItems,
            Func<TItem, TKey> keySelector,
            IEqualityComparer<TKey> keyComparer,
            Func<TItem, TItem, bool> hasChanged,
            out List<TItem> addedItems,
            out List<TItem> removedItems)
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            var comparer = keyComparer ?? EqualityComparer<TKey>.Default;
            var existingByKey = new Dictionary<TKey, TItem>(comparer);

            if (existingItems != null)
            {
                foreach (var item in existingItems)
                {
                    if (item == null)
                        continue;

                    var key = keySelector(item);
                    if (!existingByKey.ContainsKey(key))
                        existingByKey.Add(key, item);
                }
            }

            var latestSeen = new HashSet<TKey>(comparer);
            addedItems = new List<TItem>();
            var hasUpdates = false;

            if (latestItems != null)
            {
                foreach (var item in latestItems)
                {
                    if (item == null)
                        continue;

                    var key = keySelector(item);
                    if (!latestSeen.Add(key))
                        continue;

                    TItem existing;
                    if (existingByKey.TryGetValue(key, out existing))
                    {
                        if (hasChanged != null && hasChanged(existing, item))
                            hasUpdates = true;

                        existingByKey.Remove(key);
                    }
                    else
                    {
                        addedItems.Add(item);
                    }
                }
            }

            removedItems = existingByKey.Values.ToList();
            return hasUpdates;
        }

        /// <summary>
        /// Computes added and removed members between existing and latest collections by key.
        /// </summary>
        public static void DiffByKey<TItem, TKey>(
            IEnumerable<TItem> existingItems,
            IEnumerable<TItem> latestItems,
            Func<TItem, TKey> keySelector,
            IEqualityComparer<TKey> keyComparer,
            out List<TItem> addedItems,
            out List<TItem> removedItems)
        {
            DiffByKey(existingItems, latestItems, keySelector, keyComparer, null, out addedItems, out removedItems);
        }
    }
}
