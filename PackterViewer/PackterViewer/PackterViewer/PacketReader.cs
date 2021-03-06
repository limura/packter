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

namespace PackterViewer
{
    class PacketReader : IDisposable
    {
        Socket socket = null;
        ArrayList packetQueue = new ArrayList();
        Mutex mutex;
        Thread readerThread = null;
        
        ConfigReader.ProgramMode mode = ConfigReader.ProgramMode.SenderReceiver;

        public const string BoardTriggerString = "PACKTER";
        public const string HTMLTriggerString = "PACKTERHTML";
        public const string MSGTriggerString = "PACKTERMSG";
        public const string SOUNDTriggerString = "PACKTERSOUND";
        public const string VOICETriggerString = "PACKTERVOICE";
        public const string SETriggerString = "PACKTERSE";
        public const string SKYDOMETextureTriggerString = "PACKTERSKYDOMETEXTURE";
        public const string BallisticTriggerString = "PACKTERBALLISTIC";
        public const string GatewayBoardTriggerString = "PACKTERWITHGATEWAY";

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
            //ReadData(Encoding.UTF8.GetBytes("PACKTERSOUND\n")); // この二回目の null な PACKTERSOUND 呼び出しで音が消える
            //ReadData(Encoding.UTF8.GetBytes("PACKTERMSG\npackter01.png,らんらんるー"));
            //ReadData(Encoding.UTF8.GetBytes("PACKTERMSG\nteacherr.png,<HTMLWIDTH>, <HTMLHEIGHT><font face=\"メイリオ\" size=+2>こうなってくれると嬉しいです<br>画像は全部用意する感じで。</font>"));
            //ReadData(Encoding.UTF8.GetBytes("PACKTERMSG\nteacherr.png"));
            //ReadData(Encoding.UTF8.GetBytes("PACKTERHTML\n<html><body>らんらんるー<a href=http://www.google.co.jp/>google</A></body></html>"));
            //ReadData(Encoding.UTF8.GetBytes("PACKTER\n0.0.0.0,255.255.255.255,0,65535,misairu,0"));
            //ReadData(Encoding.UTF8.GetBytes("PACKTERVOICE\n/W:ぱくたーが起動したよ？"));
            //ReadData(Encoding.UTF8.GetBytes("PACKTERVOICE\n/T:3 /W:ハロー"));
            //ReadData(Encoding.UTF8.GetBytes("PACKTERSKYDOMETEXTURE\npic01.png"));
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
        
        public ConfigReader.ProgramMode Mode {
            set { mode = value; }
            get { return mode; }
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
                case PacketReader.HTMLTriggerString: // plain html
                case PacketReader.MSGTriggerString:  // imgNumber, msgHTML
                case PacketReader.SOUNDTriggerString:  // soundFileName
                case PacketReader.VOICETriggerString: // ゆっくりボイス
                case PacketReader.SETriggerString: // SE
                case PacketReader.SKYDOMETextureTriggerString: // SkydomeTexture
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
                case PacketReader.BallisticTriggerString: // 弾道軌道
                    while (true)
                    {
                        FlyPacket packet = new FlyPacketBallistic();
                        if (packet.SetFromData(reader) == false)
                            break;
                        packetQueue.Add(packet);
                    }
                    break;
                case PacketReader.GatewayBoardTriggerString: // Gatewayつき
                    while (true)
                    {
                        FlyPacket packet = new FlyPacketWithGateway();
                        if (packet.SetFromData(reader) == false)
                            break;
                        packetQueue.Add(packet);
                    }
                    break;
                case PacketReader.BoardTriggerString: // PACKTER
                    while (true)
                    {
                        FlyPacket packet = null;
                        switch(mode){
                            case ConfigReader.ProgramMode.SenderReceiver:
                            default:
                                packet = new FlyPacketLay();
                                break;
                            case ConfigReader.ProgramMode.Ballistic:
                                packet = new FlyPacketBallistic();
                                break;
                        }
                        if (packet.SetFromData(reader) == false)
                            break;
                        packetQueue.Add(packet);
                    }
                    break;
                default:
                    // 何にも引っかからなかったら無視する
                    return;
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
