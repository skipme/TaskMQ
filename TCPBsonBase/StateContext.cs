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
                throw new Exception("alignment error");

            int i2 = BitConverter.ToInt32(framecheck, 4);

            return i2;
        }
        /// <summary>
        /// for one sequented read research strategy 
        /// </summary>
        internal class incompleteReadContext
        {
            public byte[] buffer;//new byte[8192]; // replace with max_frame_sz?
            public incompleteReadContext(int maxread)
            {
                buffer = new byte[maxread];
            }
            public int cursor;
            public DateTime lastRead;
            public void Reset()
            {
                cursor = 0;
            }
            public void SetLastRead(byte[] srcBuffer, int cursor)
            {
                lastRead = DateTime.UtcNow;
                this.cursor = cursor;
                Array.Copy(srcBuffer, buffer, cursor);
            }
            public bool isIncomplete
            {
                get
                {
                    return cursor != 0;
                }
            }
        }
        private incompleteReadContext IrcMark = new incompleteReadContext(8);
        private incompleteReadContext IrcPayLoad = new incompleteReadContext(8192); // replace with max_frame_sz?

        private bool fullRead(System.IO.Stream stream, byte[] buffer, int count, incompleteReadContext irc)
        {
            int ccount = 0, ccread = count;
            if (irc.isIncomplete)
            {
                ccount = irc.cursor;
                ccread = count - irc.cursor;
                Array.Copy(irc.buffer, buffer, ccount);
            }
            while (ccount < count)
            {
                int cread = stream.Read(buffer, ccount, ccread);
                ccount += cread;
                if (cread < ccread)
                {
                    //if (cread == 0)
                    {
                        irc.SetLastRead(buffer, ccount);
                        return false;
                    }
                    //System.Threading.Thread.Sleep(10);// if we need to wait for only one client
                }
                else break;
                ccread -= cread; // if we need to wait for only one client
            }
            return true;
        }
        public void ReadSelfState(System.IO.Stream stream)
        {
            //int readok8 = stream.Read(framecheck, 0, 8);
            if (!fullRead(stream, framecheck, 8, IrcMark))
            {
                selfState = null;
                return;
            }

            int readsz = compareframeMarkers();
            byte[] bson = new byte[readsz];

            //int readokb = stream.Read(bson, 0, readsz);
            if (!fullRead(stream, bson, readsz, IrcPayLoad))
            {
                IrcMark.SetLastRead(framecheck, 8);// prevent frameheader reading from payload part
                selfState = null;
                return;
            }

            IrcMark.Reset();
            IrcPayLoad.Reset();

            selfState = StateObj.GetStateObj<TS>(bson);
        }

        public void WriteOppositeStateIfRequired(System.IO.Stream stream)
        {
            if (this.writeOpposite)
            {
                byte[] odata = oppositeState.GetBsonData();
                byte[] frame = new byte[odata.Length + 8];

                //Array.Copy(framemarker, 0, frame, 0, 4);
                //Array.Copy(BitConverter.GetBytes((Int32)odata.Length), 0, frame, 4, 4);
                //Array.Copy(odata, 0, frame, 8, odata.Length);
                //stream.Write(frame, 0, frame.Length);

                // write only the whole frame, it is necessary?
                stream.Write(framemarker, 0, 4);

                //stream.Flush();
                //System.Threading.Thread.Sleep(5000);

                stream.Write(BitConverter.GetBytes((Int32)odata.Length), 0, 4);
                stream.Write(odata, 0, odata.Length);
            }
        }
        public abstract void ProcState();
    }
}
