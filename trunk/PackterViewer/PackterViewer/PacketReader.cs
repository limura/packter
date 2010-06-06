/*
 * packter sender ����̃p�P�b�g����ǂݎ�邽�߂�class
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
    class PacketReader : IDisposable
    {
        Socket socket = null;
        ArrayList packetQueue = new ArrayList();
        Mutex mutex;
        Thread readerThread = null;

        public const string BoardTriggerString = "PACKTER";
        public const string HTMLTriggerString = "PACKTERHTML";
        public const string MSGTriggerString = "PACKTERMSG";
        public const string SOUNDTriggerString = "PACKTERSOUND";
        public const string VOICETriggerString = "PACKTERVOICE";
        public const string SETriggerString = "PACKTERSE";

        Dictionary<string, List<string> > packterStringQueue = new Dictionary<string, List<string> >();

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

#if true
#if false
            string dummyPacket = "PACKTER\n"
                + "0.0.0.0,255.255.255.255,0,65535,0\n"
                + "255.255.255.255,0.0.0.0,0,65535,0\n"
                + "255.255.255.255,255.255.255.255,65535,0,0\n"
                + "0.0.0.0,0.0.0.0,65535,0,0\n"
                + "128.128.128.128,128.128.128.128,32768,0,1\n"
                + "64::0,128.128.128.128,32768,0,2\n"
                + "0.5,0.5,0.5,0.5,4\n"
                + ",,0.5,0.5,5\n";
            ReadData(Encoding.UTF8.GetBytes(dummyPacket));
#else
            //ReadData(Encoding.UTF8.GetBytes("PACKTERSOUND\n-10,tm2_door000.wav"));
            //ReadData(Encoding.UTF8.GetBytes("PACKTERSOUND\n0,packter01.wav"));
            //ReadData(Encoding.UTF8.GetBytes("PACKTERSOUND\n")); // ���̓��ڂ� null �� PACKTERSOUND �Ăяo���ŉ���������
            //ReadData(Encoding.UTF8.GetBytes("PACKTERMSG\npackter01.png,������["));
            ReadData(Encoding.UTF8.GetBytes("PACKTERMSG\nteacherr.png,<CURRENTDIR><font face=\"���C���I\" size=+2>�����Ȃ��Ă����Ɗ������ł�<br>�摜�͑S���p�ӂ��銴���ŁB</font>"));
            //ReadData(Encoding.UTF8.GetBytes("PACKTERMSG\nteacherr.png"));
            //ReadData(Encoding.UTF8.GetBytes("PACKTERHTML\n<html><body>������[<a href=http://www.google.co.jp/>google</A></body></html>"));
            //ReadData(Encoding.UTF8.GetBytes("PACKTER\n0.0.0.0,255.255.255.255,0,65535,misairu,0"));
            //ReadData(Encoding.UTF8.GetBytes("PACKTERVOICE\n/W:�ς����[���N��������H"));
            //ReadData(Encoding.UTF8.GetBytes("PACKTERVOICE\n/T:3 /W:�n���["));
#endif
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

        public Dictionary<string, List<string> > GetAvailableMessageStrings()
        {
            mutex.WaitOne();
            Dictionary<string, List<string> > result = new Dictionary<string,List<string>>(packterStringQueue);
            mutex.ReleaseMutex();
            packterStringQueue.Clear();
            return result;
        }

        void ReadData(byte[] buffer)
        {
            MemoryStream ms = new MemoryStream(buffer);
            StreamReader reader = new StreamReader(ms);

            string firstLine = reader.ReadLine();
            switch (firstLine)
            {
                case PacketReader.BoardTriggerString:
                    // ����͍��܂ł̃��m�Ȃ̂ł��̂܂ܒʂ�
                    break;
                case PacketReader.HTMLTriggerString: // plain html
                case PacketReader.MSGTriggerString:  // imgNumber, msgHTML
                case PacketReader.SOUNDTriggerString:  // soundFileName
                case PacketReader.VOICETriggerString: // �������{�C�X
                case PacketReader.SETriggerString: // SE
                    if(packterStringQueue.ContainsKey(firstLine) == false){
                        packterStringQueue[firstLine] = new List<string>();
                    }
                    string unicodeString = Encoding.UTF8.GetString(buffer);
                    string[] splitString = unicodeString.Split(new char[] {'\n'}, 2);
                    if (splitString.Length == 2)
                    {
                        packterStringQueue[firstLine].Add(splitString[1]);
                    }
                    break;
                default:
                    // ���ɂ�����������Ȃ������疳������
                    return;
            }
            if (firstLine != PacketReader.BoardTriggerString)
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
            socket.ReceiveTimeout = 1000; // ��b������ Receive ���甲���āAThread.Aboart ���󂯕t����B
            while (true)
            {
                try
                {
                    int length = 0;
                    if ((length = socket.Receive(buffer)) > 0)
                    {
                        byte[] tmp = new byte[length];
                        Array.Copy(buffer, 0, tmp, 0, length);
                        mutex.WaitOne();
                        ReadData(tmp);
                        mutex.ReleaseMutex();
                    }
                }
                catch(SocketException) { }
            }
        }
    }
}
