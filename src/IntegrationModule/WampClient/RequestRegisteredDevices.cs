﻿using System;
using System.Collections.Generic;
using WampSharp.Core.Serialization;
using WampSharp.V2.Client;
using WampSharp.V2.PubSub;
using WampSharp.V2.Core.Contracts;

namespace Wamp.Client
{

    public partial class WampClient
    {
        /***********************************************************************************************************************/
        /********************                     Request all registered devices                             *******************/
        /***********************************************************************************************************************/

        /// <summary>This method will request all registered devices.</summary>
        /// <returns>
        /// The method returns the list of all registered devices. Each element contains the device directory number and the current
        /// connection state reachable / not reachable.
        /// </returns>

        /***********************************************************************************************************************/
        public List<wamp_device_registration_element> requestRegisteredDevices()
        /***********************************************************************************************************************/
        {
            object res = GetSystemDevicesRegistered();
            string json_str = res.ToString();
            OnChildLogString?.Invoke(this, json_str);

            List<wamp_device_registration_element> registeredDevices = Newtonsoft.Json.JsonConvert.DeserializeObject<List<wamp_device_registration_element>>(json_str);
            return registeredDevices;
        }

        /// <summary>
        /// This method requests a list of IP interfaces available on the Zenitel Connect Platform 
        /// </summary>
        /// <returns>The method returns a list of IP interface Ports. Each element contains the MAC-Addres, status and name</returns>
        /***********************************************************************************************************************/
        public List<wamp_interface_list> requestInterfaceList()
        /***********************************************************************************************************************/
        {
            object res = GetInterfaceList();
            string json_str = res.ToString();
            OnChildLogString?.Invoke(this, json_str);

            List<wamp_interface_list> interfaceList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<wamp_interface_list>>(json_str);
            return interfaceList;
        }

        /// <summary>
        /// This method requests a list of calls registered at the Zenitel Connect Platform. The returned list may be filtered (reduced)
        /// by specifying the filtering parameters. A filtering parameter not being used is specified as an empty string. 
        /// </summary>
        /// <param name="dirNo">Only return calls having this directory number as member.</param>
        /// <param name="callId">Only return the call having this call identification.</param>
        /// <param name="state">Only return calls being in the specified state.</param>
        /// <returns>The method returns a list of calls according to the filtering specified via the parameters.</returns>
        /***********************************************************************************************************************/
        public List<wamp_call_element> requestCallList(string dirNo, string callId, string state)
        /***********************************************************************************************************************/
        {
            object res = GET_calls(dirNo, callId, state);
            string json_str = res.ToString();
            OnChildLogString?.Invoke(this, json_str);

            List<wamp_call_element> callList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<wamp_call_element>>(json_str);
            return callList;
        }

        /// <summary>
        /// This method requests a list of queued calls registered at the Zenitel Connect Platform. The returned list may be filtered (reduced)
        /// by specifying the filtering parameter. A filtering parameter not being used is specified as an empty string. 
        /// </summary>
        /// <param name="queueDirNo">Only return call queue with this directory number.</param>
        /// <returns>The method returns a list of call queues according to the filtering specified via the parameters.</returns>
        /***********************************************************************************************************************/
        public List<wamp_call_leg_element> requestQueuedCalls(string queueDirNo)
        /***********************************************************************************************************************/
        {
            object res = GET_calls_queued(queueDirNo);
            string json_str = res.ToString();
            OnChildLogString?.Invoke(this, json_str);

            List<wamp_call_leg_element> callQueuedList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<wamp_call_leg_element>>(json_str);
            return callQueuedList;
        }

        /// <summary>
        /// This methods requests the Call Legs from the Zenitel Connect Platform. The returned list may be filtered by adding the following
        /// parameters as filters
        /// </summary>
        /// <param name="fromDirNo"></param>
        /// <param name="toDirNo"></param>
        /// <param name="dirNo"></param>
        /// <param name="legId"></param>
        /// <param name="callId"></param>
        /// <param name="State"></param>
        /// <param name="legRole"></param>
        /// <returns></returns>
        /***********************************************************************************************************************/
        public List<wamp_call_leg_element> requestCallLegs(string fromDirNo, string toDirNo, string dirNo, string legId,
                                                                 string callId, string State, string legRole)
        /***********************************************************************************************************************/
        {
            object res = GET_call_queue_legs(fromDirNo, toDirNo, dirNo, legId, callId, State, legRole);
            string json_str = res.ToString();
            OnChildLogString?.Invoke(this, json_str);

            List<WampClient.wamp_call_leg_element> callQueueLegList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<wamp_call_leg_element>>(json_str);
            return callQueueLegList;
        }



        /// <summary>
        /// This method requests a status list of General Purpose Outputs Ports.
        /// </summary>
        /// <param name="dirNo">This is the ID of the device having the General Outport Port</param>
        /// <param name="Id">This is the name of the General Purpose Outport Port</param>
        /// <returns>The method returns a list of GPO elements according to the filtering specified via the parameters.</returns>
        /***********************************************************************************************************************/
        public List<wamp_device_gpio_element> requestDevicesGPOs(string dirNo, string Id)
        /***********************************************************************************************************************/
        {
            object res = GET_devices_gpos(dirNo, Id);

            if (res != null)
            {
                string json_str = res.ToString();
                OnChildLogString?.Invoke(this, json_str);
                List<wamp_device_gpio_element> gpoElementList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<wamp_device_gpio_element>>(json_str);
                return gpoElementList;
            }
            else
            {
                List<wamp_device_gpio_element> gpoElementList = new List<wamp_device_gpio_element>();
                return gpoElementList;
            }
        }

        /// <summary>
        /// This method requests a status list of General Purpose Inputs Ports.
        /// </summary>
        /// <param name="device_id">This is the ID of the device having the General Inport Port</param>
        /// <param name="id">This is the name of the General Purpose Inport Port</param>
        /// <returns>The method returns a list of GPI elements according to the filtering specified via the parameters.</returns>
        /***********************************************************************************************************************/
        public List<wamp_device_gpio_element> requestDevicesGPIs(string device_id, string id)
        /***********************************************************************************************************************/
        {
            object res = GET_devices_gpis(device_id, id);

            if (res != null)
            {
                string json_str = res.ToString();
                OnChildLogString?.Invoke(this, json_str);
                List<wamp_device_gpio_element> gpoElementList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<wamp_device_gpio_element>>(json_str);
                return gpoElementList;
            }
            else
            {
                List<wamp_device_gpio_element> gpoElementList = new List<wamp_device_gpio_element>();
                return gpoElementList;
            }
        }

        /// <summary>
        /// This method requests the current software version of the Zenitel Connect Pro.
        /// </summary>
        /// <returns>The method returns the platform version class.</returns>
        /***********************************************************************************************************************/
        public wamp_platform_version requestPlatformVersion()
        /***********************************************************************************************************************/
        {
            object res = GET_PlatformVersion();

            if (res != null)
            {
                string json_str = res.ToString();

                if (json_str != null)
                {
                    OnChildLogString?.Invoke(this, json_str);

                    wamp_platform_version platformVersion = Newtonsoft.Json.JsonConvert.DeserializeObject<wamp_platform_version>(json_str);
                    return platformVersion;
                }
                else
                    return null;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// This method requests a list of configured device groups (group calls)
        /// </summary>
        /// <param name="dirno">Optional query parameter to filter on specific group (call)</param>
        /// <param name="verbose">If true, group members are also included in the response</param>
        /// <returns>The method returns a list of group elements according to the filtering specified via the parameters.</returns>
        /***********************************************************************************************************************/
        public List<wamp_group_element> requestGroups(string dirno, bool verbose)
        /***********************************************************************************************************************/
        {
            object res = GET_groups(dirno, verbose);

            if (res != null)
            {
                string json_str = res.ToString();
                OnChildLogString?.Invoke(this, json_str);
                List<wamp_group_element> groupElementList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<wamp_group_element>>(json_str);
                return groupElementList;
            }
            else
            {
                List<wamp_group_element> gpoElementList = new List<wamp_group_element>();
                return gpoElementList;
            }
        }

        /// <summary>
        /// This method requests a list of uploaded audio_messages
        /// </summary>
        /// <returns>The method returns a list of all audio message elements</returns>
        /***********************************************************************************************************************/
        public AudioMessageWrapper requestAudioMessages()
        /***********************************************************************************************************************/
        {
            object res = GET_audio_messages();

            if (res != null)
            {
                string json_str = res.ToString();
                OnChildLogString?.Invoke(this, json_str);
                AudioMessageWrapper audioMessageElementsList = Newtonsoft.Json.JsonConvert.DeserializeObject<AudioMessageWrapper>(json_str);
                return audioMessageElementsList;
            }
            else
            {
                AudioMessageWrapper audioMessageElementsList = new AudioMessageWrapper();
                return audioMessageElementsList;
            }
        }

        /// <summary>
        /// Get list of configured directory numbers
        /// </summary>
        /// <param name="dirno">Optional query parameter to filter on specific group (call)</param>
        /// <returns>The method returns a list of configured directory numbers</returns>
        /***********************************************************************************************************************/
        public List<wamp_directory_number_element> requestDirectories(string dirno)
        /***********************************************************************************************************************/
        {
            object res = GET_directories(dirno);

            if (res != null)
            {
                string json_str = res.ToString();
                OnChildLogString?.Invoke(this, json_str);
                List<wamp_directory_number_element> directoryNumbersElementsList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<wamp_directory_number_element>>(json_str);
                return directoryNumbersElementsList;
            }
            else
            {
                List<wamp_directory_number_element> directoryNumbersElementsList = new List<wamp_directory_number_element>();
                return directoryNumbersElementsList;
            }
        }

    }
}

