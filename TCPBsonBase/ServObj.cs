using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace TCPBsonBase
{

    public class ServObj : StateObj
    {

        public void ProcessState(ServerClientCtx ctx)
        {
            switch (this.state)
            {
                case "PUT":
                    ctx.OnPut(
                        this.PAsInt("id"),
                        this.PAsInt("setid"),
                        this.PAsDict("message")
                        );
                    break;
                case "BATCH_PUT":
                    ctx.OnPutBatch(
                        this.PAsInt("id"),
                        this.PAsInt("setid"),
                        this.PAsDictCollection("messages")
                        );
                    break;
                default:
                    throw new Exception("unknown state");
                    break;
            }
        }

        public void SetPut(int id, int setId, Dictionary<string, object> message)
        {
            this.state = "PUT";
            this.stateParameters = new Dictionary<string, object>{
                { "id", id},
                { "setid", setId},
                { "message", message }
            };
        }
        public void SetPutBatch(int id, int setId, ICollection<Dictionary<string, object>> messages)
        {
            this.state = "BATCH_PUT";
            this.stateParameters = new Dictionary<string, object>{
                { "id", id},
                { "setid", setId},
                { "messages", messages }
            };
        }
        public void SetGetPutIds(int minimalId)
        {
            this.state = "PUT_GET";
            this.stateParameters = new Dictionary<string, object>{
                { "minimalId", minimalId }
            };
        }
    }
}
