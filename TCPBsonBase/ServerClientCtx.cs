using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPBsonBase
{
    public delegate bool Put(Dictionary<string, object> message);
    public class ServerClientCtx : StateContext<ServObj, CliObj>
    {
        public ServerClientCtx()
            : base()
        {
            this.oppositeState = new CliObj();
        }

        public Put PutCallback;

        private int currentSetid = -1;
        private int selfIncrementedId = -1;

        public void OnPut(int id, int setid, Dictionary<string, object> message)
        {
            if (setid >= 0)
            {
                selfIncrementedId = currentSetid = setid;
            }
            if (PutCallback != null)
            {
                if (PutCallback(message))
                {
                    selfIncrementedId++;
                }
            }
        }
        public override void ProcState()
        {
            selfState.ProcessState(this);
        }
    }
}
