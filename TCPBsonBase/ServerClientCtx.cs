using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                selfIncrementedId = currentSetid = setid - 1;
            }
            if (PutCallback != null)
            {
                if (PutCallback(message))
                {
                    selfIncrementedId++;
                }
            }
        }
        public void OnPutBatch(int id, int setid, Collection<Dictionary<string, object>> messages)
        {
            if (setid >= 0)
            {
                selfIncrementedId = currentSetid = setid - 1;
            }
            if (PutCallback != null)
            {
                for (int i = 0; i < messages.Count; i++)
                {
                    if (PutCallback(messages[i]))
                    {
                        selfIncrementedId++;
                    }
                }
            }
        }
        public override void ProcState()
        {
            if (selfState != null)
                selfState.ProcessState(this);
        }
    }
}
