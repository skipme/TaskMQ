using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPBsonBase
{
    public abstract class StateContext<TS, TO>
        where TS : StateObj
        where TO : StateObj
    {
        public StateContext()
        {
            marker = BitConverter.ToInt32(this.framemarker, 0);
        }

        private bool writeOpposite = false;
        public void SetWriteOpposite()
        {
            writeOpposite = true;
        }

        public TO oppositeState;
        public TS selfState;

        readonly byte[] framemarker = new byte[] { 0xfe, 0xae, 0xbe, 0xff };
        private Int32 marker;

        byte[] framecheck = new byte[8];
        private int compareframeMarkers()
        {
            int i1 = BitConverter.ToInt32(framecheck, 0);
            if (i1 != marker)
                throw new Exception("aligment error");

            int i2 = BitConverter.ToInt32(framecheck, 4);

            return i2;
        }
        private void fullRead(System.IO.Stream stream, byte[] buffer, int count)
        {
            int ccount = 0, ccread = count;
            while (ccount < count)
            {
                int cread = stream.Read(buffer, ccount, ccread);
                ccount += cread;
                if (cread < ccread)
                    System.Threading.Thread.Sleep(10);
                else break;
                ccread -= cread;
            }
        }
        public void ReadSelfState(System.IO.Stream stream)
        {
            //int readok8 = stream.Read(framecheck, 0, 8);
            fullRead(stream, framecheck, 8);

            int readsz = compareframeMarkers();
            byte[] bson = new byte[readsz];

            //int readokb = stream.Read(bson, 0, readsz);
            fullRead(stream, bson, readsz);

            selfState = StateObj.GetStateObj<TS>(bson);
        }

        public void WriteOppositeStateIfRequired(System.IO.Stream stream)
        {
            if (this.writeOpposite)
            {
                byte[] odata = oppositeState.GetBsonData();
                byte[] frame = new byte[odata.Length + 8];
                Array.Copy(framemarker, 0, frame, 0, 4);
                Array.Copy(BitConverter.GetBytes((Int32)odata.Length), 0, frame, 4, 4);
                Array.Copy(odata, 0, frame, 8, odata.Length);
                stream.Write(frame, 0, frame.Length);
                // write only the whole frame, it is necessary?
                //stream.Write(framemarker, 0, 4);
                //stream.Write(BitConverter.GetBytes((Int32)odata.Length), 0, 4);
                //stream.Write(odata, 0, odata.Length);
            }
        }
        public abstract void ProcState();
    }
}
