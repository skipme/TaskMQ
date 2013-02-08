using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskQueue.Providers;

namespace TaskBroker
{
    public class QueueConParams
    {
        public QueueConParams()
        {
            MessageModels = new List<QueueConnectionParameters>();
        }
        public void Add(QueueConnectionParameters mt)
        {
            MessageModels.Add(mt);
        }
        public QueueConnectionParameters GetByName(string cpName)
        {
            QueueConnectionParameters mt = (from mm in MessageModels
                                            where mm.Name.Equals(cpName, StringComparison.OrdinalIgnoreCase)
                                            select mm).FirstOrDefault();

            return mt;
        }
        public List<QueueConnectionParameters> MessageModels;
    }
}
