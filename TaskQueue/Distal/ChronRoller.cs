using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskQueue.Providers;

namespace TaskQueue.Distal
{
    interface ChronRoller
    {
        ulong TypeMark(ChronObject instance);
        ICollection<ulong> ChronTypesBitmap { get; }
        /// <summary>
        /// Записывает только те что изменились больше n раз
        /// </summary>
        void SyncProjections();
        void Changes(ChronObject a, ChronObject b);

        void PushChange(ChronObject obj);
    }
}
