using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskBroker
{
    public class QueueMTClassificator
    {
        public QueueMTClassificator()
        {
            MessageModels = new List<MessageType>();
        }
        public MessageType GetByName(string mtName)
        {
            MessageType mt = (from mm in MessageModels
                              where mm.UniqueName.Equals(mtName, StringComparison.OrdinalIgnoreCase)
                              select mm).FirstOrDefault();

            return mt;
        }
        public List<MessageType> MessageModels;
    }
}
