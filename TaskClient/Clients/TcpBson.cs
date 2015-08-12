using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace TaskClient.Clients
{
    public class TcpBson : IMessageClient
    {
        TcpClient client;
        NetworkStream stream;
        TCPBsonBase.ClientCtx ctx;

        public TcpBson()
        {
            ctx = new TCPBsonBase.ClientCtx();
            client = new TcpClient("localhost", 83);
            stream = client.GetStream();
        }
        public ApiResult ApiEnqueue(Dictionary<string, object> flatMessageData)
        {
            ctx.Put(flatMessageData);
            ctx.WriteOppositeStateIfRequired(stream);
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
