using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tests
{
    using NUnit.Framework;
    using System.IO;

    [TestFixture]
    public class Scaling
    {
        [Test]
        public void Scaling_Id()
        {
            TaskUniversum.Scale.ClientAuthKey[] kk = new TaskUniversum.Scale.ClientAuthKey[10000];
            for (int i = 0; i < kk.Length; i++)
            {
                kk[i] = new TaskUniversum.Scale.ClientAuthKey();
            }
            int[] fiveRange = new int[5];
            for (int i = 0; i < kk.Length; i++)
            {
                int idx = kk[i].GetParticipationIndex(fiveRange.Length);
                fiveRange[idx]++;
            }
            int[] tenRange = new int[10];
            for (int i = 0; i < kk.Length; i++)
            {
                int idx = kk[i].GetParticipationIndex(tenRange.Length);
                tenRange[idx]++;
            }
            int fiveCounts = kk.Length / 5;
            int tenCounts = kk.Length / 10;
            for (int i = 0; i < fiveRange.Length; i++)
            {
                Assert.Less(Math.Abs(fiveRange[i] - fiveCounts) / (double)fiveCounts, 0.1);
            }
            for (int i = 0; i < tenRange.Length; i++)
            {
                Assert.Less(Math.Abs(tenRange[i] - tenCounts) / (double)tenCounts, 0.1);
            }
        }
    }
}
