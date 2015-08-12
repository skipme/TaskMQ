using System;
namespace TaskClient.Clients
{
    interface IMessageClient
    {
        ApiResult ApiEnqueue(System.Collections.Generic.Dictionary<string, object> flatMessageData);
        bool Enqueue(System.Collections.Generic.Dictionary<string, object> flatMessageData);
        bool Enqueue(TaskQueue.Providers.TaskMessage message);
    }
}
