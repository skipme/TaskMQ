using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace TaskClient.Clients
{
    public class TcpBson : IMessageClient
    {
        public const int MAX_BATCH_SIZE = 32;

        private TcpClient client;
        private NetworkStream stream;
        private TCPBsonBase.ClientCtx ctx;

        public TcpBson()
        {
            ctx = new TCPBsonBase.ClientCtx();
            client = new TcpClient("localhost", 83);
            //client.NoDelay = true;
            stream = client.GetStream();
        }
        public ApiResult ApiEnqueue(Dictionary<string, object> flatMessageData)
        {
            ctx.Put(flatMessageData);
            ctx.WriteOppositeStateIfRequired(stream);
            return ApiResult.OK;
        }
        /// <summary>
        /// Please make sure what flatMessageData count &lt;= <see cref="MAX_BATCH_SIZE" /> for preventing incomplete push for full batch collection 
        /// </summary>
        /// <param name="flatMessageData"></param>
        /// <returns></returns>
        public ApiResult ApiEnqueueBatch(ICollection<Dictionary<string, object>> flatMessageData)
        {
            for (int i = 0; i < flatMessageData.Count; i += MAX_BATCH_SIZE)
            {
                ctx.PutBatch(flatMessageData.Skip(i).Take(MAX_BATCH_SIZE).ToArray());
                ctx.WriteOppositeStateIfRequired(stream);
            }
            return ApiResult.OK;
        }
        public bool Enqueue(Dictionary<string, object> flatMessageData)
        {
            return ApiEnqueue(flatMessageData) == ApiResult.OK;
        }

        public bool Enqueue(TaskQueue.Providers.TaskMessage message)
        {
            return Enqueue(message.GetSendEnvelope());
        }
    }
}
