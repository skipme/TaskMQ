using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace TCPBsonBase
{
    public class ClientCtx : StateContext<CliObj, ServObj>
    {
        public ClientCtx()
            : base()
        {
            this.oppositeState = new ServObj();
        }

        int currentMessageId;
        public void PutSeries()
        {
            currentMessageId = 0;
        }
        public void Put(Dictionary<string, object> message)
        {
            this.SetWriteOpposite();
            oppositeState.SetPut(0, -1, message);
        }
        public void PutBatch(ICollection<Dictionary<string, object>> messages)
        {
            this.SetWriteOpposite();
            oppositeState.SetPutBatch(0, -1, messages);
        }
        public override void ProcState()
        {
            selfState.ProcessState(this);
        }
    }
}
