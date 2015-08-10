using ServiceStack.Common.Web;
using ServiceStack.ServiceClient.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace TaskClient.Clients
{
    public class HttpRest
    {
        JsonServiceClient client;
        public string uri
        {
            get
            {
                return client.BaseUri;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="restServiceUri">http://someDomain:82 by default</param>
        public HttpRest(string restServiceUri = "http://127.0.0.1:82/")
        {
            client = new JsonServiceClient(restServiceUri);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="flatMessageData">required MType key for routing to channel name on service side</param>
        /// <returns>rejected: if message type not well configured with channel and queue(check platform configuration)<br/> ok: message enqueued<br/> other: service error</returns>
        public ApiResult ApiEnqueue(Dictionary<string, object> flatMessageData)
        {
            if (flatMessageData == null || !flatMessageData.ContainsKey("MType"))
                throw new Exception("required MType key as channel name");

            System.Net.HttpStatusCode returnCode;
            try
            {
                HttpWebResponse result = client.Put<HttpWebResponse>("/tmq/q", flatMessageData);
                returnCode = result.StatusCode;
            }
            catch (WebServiceException webEx)
            {
                returnCode = (System.Net.HttpStatusCode)webEx.StatusCode;
            }
            if (returnCode == System.Net.HttpStatusCode.Created)
                return ApiResult.OK;

            if (returnCode == System.Net.HttpStatusCode.InternalServerError)
                return ApiResult.ServiceError;

            if (returnCode == System.Net.HttpStatusCode.NotAcceptable)
                return ApiResult.MessageRejected;

            return ApiResult.ServiceError;
        }
        /// <summary>
        /// required MType key
        /// </summary>
        /// <param name="flatMessageData">required MType key for routing to channel name on service side</param>
        /// <returns></returns>
        public bool Enqueue(Dictionary<string, object> flatMessageData)
        {
            return ApiEnqueue(flatMessageData) == ApiResult.OK;
        }
        public void Enqueue(TaskQueue.Providers.TaskMessage message)
        {
            ApiResult result = ApiEnqueue(message.GetSendEnvelope());
            if (result != ApiResult.OK)
                throw new Exception("Message rejected. Check platform configuration, perhaps message type doesn't have any channel");
        }
        public TaskQueue.RepresentedModel GetValidationInfo(string MType, string channelName = null)
        {
            Validation.ValidationResponse result = client.Post<Validation.ValidationResponse>("/tmq/v", new Validation.ValidationRequest { MType = MType, ChannelName = channelName });
            if (result == null)
                throw new Exception(string.Format("Message type '{0}' not routed to worker module {1}",
                    MType, channelName == null ? "" : string.Format("or '{1}' channel absent", channelName)));
            return TaskQueue.RepresentedModel.FromSchema(result.ModelScheme);
        }
    }
}
