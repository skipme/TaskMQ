using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPBsonBase
{

    public class CliObj : StateObj
    {
        public void ProcessState(ClientCtx ctx)
        {
            switch (this.state)
            {
                case "ACK":
                    break;
                default:
                    throw new Exception("unknown state");
                    break;
            }
        }

        public void SetAck(bool status, string statusText)
        {
            this.state = "ACK";
            this.stateParameters = new Dictionary<string, object>{
                { "status", status },
                { "statusText", statusText }
            };
        }
    }
}
