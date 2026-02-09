using System;
using System.Collections.Generic;
using WampSharp.V2.Core.Contracts;
using System.Threading;

namespace Wamp.Client
{
    public partial class WampClient
    {
        /***********************************************************************************************************************/
        /********************                         Call Forwarding - GET                                  *******************/
        /***********************************************************************************************************************/

        /// <summary>
        /// Retrieves call forwarding rules from the Zenitel Connect Platform.
        /// The returned list may be filtered by specifying the filtering parameters.
        /// A filtering parameter not being used is specified as an empty string.
        /// </summary>
        /// <param name="dirno">If provided, only return call forwarding rules owned by this directory number.</param>
        /// <param name="fwd_type">If provided, only return call forwarding rules of this type (unconditional, on_busy, on_timeout).</param>
        /// <returns>A list of call forwarding rule elements.</returns>
        /***********************************************************************************************************************/
        public List<wamp_call_forwarding_element> requestCallForwarding(string dirno, string fwd_type)
        /***********************************************************************************************************************/
        {
            object res = GET_call_forwarding(dirno, fwd_type);

            if (res == null)
            {
                OnChildLogString?.Invoke(this, "requestCallForwarding: no result (not connected or server returned null).");
                return new List<wamp_call_forwarding_element>();
            }

            string json_str = res.ToString();
            OnChildLogString?.Invoke(this, json_str);

            var list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<wamp_call_forwarding_element>>(json_str);
            return list ?? new List<wamp_call_forwarding_element>();
        }

        /***********************************************************************************************************************/
        private object GET_call_forwarding(string dirno, string fwd_type)
        /***********************************************************************************************************************/
        {
            try
            {
                var argumentsKeywords = new Dictionary<string, object>();

                if (!string.IsNullOrEmpty(dirno))
                    argumentsKeywords["dirno"] = dirno;

                if (!string.IsNullOrEmpty(fwd_type))
                    argumentsKeywords["fwd_type"] = fwd_type;

                var rpcCallback = new RPCCallback();

                _wampRealmProxy.RpcCatalog.Invoke(
                    rpcCallback,
                    new CallOptions(),
                    GetCallForwarding,
                    new object[] { argumentsKeywords },
                    argumentsKeywords
                );

                bool cont = true;
                int loopCount = 0;
                const int sleepTime = 10;

                while (cont)
                {
                    Thread.Sleep(sleepTime);
                    if (rpcCallback.RespRecv)
                        cont = false;
                    else if (++loopCount > 30)
                        cont = false;
                }

                if (!rpcCallback.RespRecv)
                {
                    OnChildLogString?.Invoke(this, "GET_call_forwarding: No response from WAMP.");
                    return null;
                }

                if (!rpcCallback.CompletedSuccessfully)
                {
                    OnChildLogString?.Invoke(this, "GET_call_forwarding failed: " + rpcCallback.CompletionText);
                    return null;
                }

                return rpcCallback;
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception in GET_call_forwarding: " + ex.ToString());
                return null;
            }
        }


        /***********************************************************************************************************************/
        /********************                        Call Forwarding - POST                                  *******************/
        /***********************************************************************************************************************/

        /// <summary>
        /// Adds or updates call forwarding rules on the Zenitel Connect Platform.
        /// Input is an array of call forwarding rule objects. Key fields are "dirno" and "fwd_type".
        /// If "dirno" and "fwd_type" do not match an existing entry, the rule is added.
        /// Otherwise, the existing entry is updated with the provided fields.
        /// </summary>
        /// <param name="rules">The list of call forwarding rules to add or update.</param>
        /// <returns>A wamp_response indicating the outcome of the operation.</returns>
        /***********************************************************************************************************************/
        public wamp_response PostCallForwardingRules(List<wamp_call_forwarding_element> rules)
        /***********************************************************************************************************************/
        {
            wamp_response wampResp = new wamp_response();

            try
            {
                string jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(rules);
                object bodyObj = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonBody);

                Dictionary<string, object> argumentsKeywords = new Dictionary<string, object>();

                RPCCallback rpcCallback = new RPCCallback();

                _wampRealmProxy.RpcCatalog.Invoke(rpcCallback,
                                                  new CallOptions(),
                                                  PostCallForwarding,
                                                  new object[] { bodyObj });

                // Wait time limited for a reply from the WAMP-protocol
                bool cont = true;
                int loopCount = 0;
                int sleepTime = 10;

                while (cont)
                {
                    Thread.Sleep(sleepTime);
                    if (rpcCallback.RespRecv)
                    {
                        cont = false;
                    }
                    else
                    {
                        loopCount++;
                        if (loopCount > 30) cont = false;
                    }
                }

                if (rpcCallback.RespRecv)
                {
                    if (rpcCallback.CompletedSuccessfully)
                    {
                        wampResp.WampResponse = ResponseType.WampRequestSucceeded;
                        wampResp.CompletionText = "PostCallForwardingRules successfully completed.";
                    }
                    else
                    {
                        wampResp.WampResponse = ResponseType.WampRequestFailed;
                        wampResp.CompletionText = "PostCallForwardingRules failed: " + rpcCallback.CompletionText;
                    }
                }
                else
                {
                    wampResp.WampResponse = ResponseType.WampNoResponce;
                    wampResp.CompletionText = "PostCallForwardingRules. No response from WAMP.";
                }
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception in PostCallForwardingRules: " + ex.ToString());
            }

            return wampResp;
        }


        /***********************************************************************************************************************/
        /********************                       Call Forwarding - DELETE                                 *******************/
        /***********************************************************************************************************************/

        /// <summary>
        /// Deletes call forwarding rules on the Zenitel Connect Platform for the specified directory number and forwarding type.
        /// </summary>
        /// <param name="dirno">Directory number owning the forwarding rules. Use "alldirno" to match all.</param>
        /// <param name="fwd_type">The forwarding type to delete (unconditional, on_busy, on_timeout). Use "all" to match all.</param>
        /// <returns>A wamp_response indicating the outcome of the operation.</returns>
        /***********************************************************************************************************************/
        public wamp_response DeleteCallForwardingRules(string dirno, string fwd_type)
        /***********************************************************************************************************************/
        {
            wamp_response wampResp = new wamp_response();

            try
            {
                Dictionary<string, object> argumentsKeywords = new Dictionary<string, object>();

                argumentsKeywords["dirno"] = dirno;
                argumentsKeywords["fwd_type"] = fwd_type;

                RPCCallback rpcCallback = new RPCCallback();

                if (_wampRealmProxy != null)
                {
                    _wampRealmProxy.RpcCatalog.Invoke(rpcCallback,
                                                      new CallOptions(),
                                                      DeleteCallForwarding,
                                                      new object[] { argumentsKeywords });

                    // Wait time limited for a reply from the WAMP-protocol
                    bool cont = true;
                    int loopCount = 0;
                    int sleepTime = 10;

                    while (cont)
                    {
                        Thread.Sleep(sleepTime);
                        if (rpcCallback.RespRecv)
                        {
                            cont = false;
                        }
                        else
                        {
                            loopCount++;
                            if (loopCount > 30) cont = false;
                        }
                    }

                    if (rpcCallback.RespRecv)
                    {
                        if (rpcCallback.CompletedSuccessfully)
                        {
                            wampResp.WampResponse = ResponseType.WampRequestSucceeded;
                            wampResp.CompletionText = "DeleteCallForwardingRules successfully completed.";
                        }
                        else
                        {
                            wampResp.WampResponse = ResponseType.WampRequestFailed;
                            wampResp.CompletionText = "DeleteCallForwardingRules failed: " + rpcCallback.CompletionText;
                        }
                    }
                    else
                    {
                        wampResp.WampResponse = ResponseType.WampNoResponce;
                        wampResp.CompletionText = "DeleteCallForwardingRules. No response from WAMP.";
                    }
                }
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception in DeleteCallForwardingRules: " + ex.ToString());
            }

            return wampResp;
        }
    }
}
