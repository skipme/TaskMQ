using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskUniversum.Scale
{
    public class ClientAuthKey
    {
        static int CurrentConnectionIndex = 0;
        static Random rnd = new Random();

        private int AuthKey;
        private int CI;

        public override string ToString()
        {
            return String.Format("{0}-{1}", Convert.ToString(AuthKey, 16), CI);
        }
        public ClientAuthKey()
        {
            lock (rnd)
            {
                AuthKey = rnd.Next();
                CI = CurrentConnectionIndex;
                CurrentConnectionIndex++;
            }
        }

        public int GetParticipationIndex(int AllocatedRange)
        {
            return AuthKey % AllocatedRange;
        }
    }
}
