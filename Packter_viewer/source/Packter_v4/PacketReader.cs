/*
 * packter sender からのパケット情報を読み取るためのclass
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Threading;
using System.IO;

namespace Packter_viewer
{
    class PacketReader
    {
        Socket socket = null;
        ArrayList packetQueue = new ArrayList();
        Mutex mutex;
        Thread readerThread = null;

        public PacketReader(IPEndPoint endpoint)
        {
            if(endpoint.AddressFamily == AddressFamily.InterNetworkV6)
                socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
            else
                socket = new Socket(AddressFamily.Unspecified, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(endpoint);
            mutex = new Mutex();
            readerThread = new Thread(new ThreadStart(PacketReadRoop));
            readerThread.Start();

#if false
            string dummyPacket = "PACTER\n"
                + "0.0.0.0,255.255.255.255,0,65535,0\n"
                + "255.255.255.255,0.0.0.0,0,65535,0\n"
                + "255.255.255.255,255.255.255.255,65535,0,0\n"
                + "0.0.0.0,0.0.0.0,65535,0,0\n"
                + "128.128.128.128,128.128.128.128,32768,0,1\n"
                + "64::0,128.128.128.128,32768,0,2\n"
                + "0.5,0.5,0.5,0.5,4\n"
                + ",,0.5,0.5,5\n";
            char[] clist = dummyPacket.ToCharArray();
            byte[] blist = new byte[clist.Length];
            int i = 0;
            foreach (char c in clist){
                blist[i++] = (byte)c;
            }
            ReadData(blist);
#endif
        }
        
        public void Dispose()
        {
            if (readerThread != null)
            {
                readerThread.Abort();
                readerThread = null;
            }
        }

        public FlyPacket[] GetAvailablePackets()
        {
            mutex.WaitOne();
            FlyPacket[] packets = (FlyPacket[])packetQueue.ToArray(typeof(FlyPacket));
            packetQueue.Clear();
            mutex.ReleaseMutex();
            return packets;
        }

        void ReadData(byte[] buffer)
        {
            MemoryStream ms = new MemoryStream(buffer);
            StreamReader reader = new StreamReader(ms);

            string firstLine = reader.ReadLine();
            if (firstLine != "PACTER")
                return;

            while (true)
            {
                FlyPacket packet = new FlyPacket();
                if (packet.SetFromData(reader) == false)
                    break;
                packetQueue.Add(packet);
            }
        }

        void PacketReadRoop()
        {
            byte[] buffer = new byte[65536];
            socket.ReceiveTimeout = 1000; // 一秒おきで Receive から抜けて、Thread.Aboart を受け付ける。
            while (true)
            {
                try
                {
                    if (socket.Receive(buffer) > 0)
                    {
                        mutex.WaitOne();
                        ReadData(buffer);
                        mutex.ReleaseMutex();
                    }
                }
                catch { }
            }
        }
    }
}
