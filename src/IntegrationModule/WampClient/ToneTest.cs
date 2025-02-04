using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WampSharp.V2.Core.Contracts;

namespace Wamp.Client
{
    public partial class WampClient
    {
        /***********************************************************************************************************************/
        /********************                                   Tone Test                                    *******************/
        /***********************************************************************************************************************/

        /// <summary>
        /// This method sends a tone test request to the connected Zenitel Connect Platform. It will return the outcome of the request
        /// execution in the wamp_response class.
        /// </summary>
        /// 
        /// <param name="dirNo">This parameter specifies the directory number of the device on witch to start the tone test.</param>
        /// <param name="tone_group">Tone group number</param>
        /// 
        /// <returns name="wamp_response">This class contains the result of the request execution and a describing text in case the
        /// request fails.
        /// </returns>
        /***********************************************************************************************************************/
        public wamp_response ToneTest(string dirNo, string tone_group)
        /***********************************************************************************************************************/
        {
            wamp_response wampResp = new wamp_response();

            try
            {
                Dictionary<string, object> argumentsKeywords = new Dictionary<string, object>();

                argumentsKeywords["dirno"] = dirNo;
                argumentsKeywords["tone_group"] = tone_group;

                RPCCallback rpcCallback = new RPCCallback();

                _wampRealmProxy.RpcCatalog.Invoke(rpcCallback,
                                                  new CallOptions(),
                                                  PostWampToneTest,
                                                  new object[] { argumentsKeywords });

                // Wait time limited for a reply from the WAMP-protocol
                bool cont = true;
                int loopCount = 0;
                int sleepTime = 10;

                while (cont)
                {
                    //Wait for RPC response
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
                        wampResp.CompletionText = "ToneTest sucessfully completed.";
                    }
                    else
                    {
                        wampResp.WampResponse = ResponseType.WampRequestFailed;
                        wampResp.CompletionText = "ToneTest failed: " + rpcCallback.CompletionText;
                    }
                }
                else
                {
                    wampResp.WampResponse = ResponseType.WampNoResponce;
                    wampResp.CompletionText = "ToneTest. No response from WAMP.";
                }
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception in ToneTest: " + ex.ToString());
            }

            return wampResp;
        }
    }
}
