using ServiceStack.Common.Web;
using ServiceStack.ServiceClient.Web;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// <param name="flatMessageData">required MType key as channel name</param>
        /// <returns></returns>
        public ApiResult ApiEnqueue(Dictionary<string, object> flatMessageData)
        {
            if (flatMessageData == null || !flatMessageData.ContainsKey("MType"))
                throw new Exception("required MType key as channel name");

            System.Net.HttpStatusCode returnCode;
            try
            {
                HttpResult result = client.Put<HttpResult>("/tmq/q", flatMessageData);
                returnCode = result.StatusCode;
            }
            catch (WebServiceException webEx)
            {
                returnCode = (System.Net.HttpStatusCode)webEx.StatusCode;
            }
            if (returnCode == System.Net.HttpStatusCode.OK)
                return ApiResult.OK;

            if (returnCode == System.Net.HttpStatusCode.InternalServerError)
                return ApiResult.ServiceError;

            if (returnCode == System.Net.HttpStatusCode.NotAcceptable)
                return ApiResult.MessageRejected;

            return ApiResult.ServiceError;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="flatMessageData">required MType key as channel name</param>
        /// <returns></returns>
        public bool Enqueue(Dictionary<string, object> flatMessageData)
        {
            return ApiEnqueue(flatMessageData) == ApiResult.OK;
        }
    }
}
