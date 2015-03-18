/*
 * packter sender から送られるデータ一行分のデータを保管するためのclass
 *
 * srcAddress, srcPort
 * dstAddress, dstPort
 * gatewayAddress, gatewayPort という名前の6つの float の値
 * (それぞれ 0.0 から 1.0 までの値であることが期待されている)と、
 * パケット画像番号(packetImageNumber) を持っている。
 * 
 * packter senderから送られた文字列をそれらしく解析する機能もこのclassが受け持っている
 *
 * 拡張として、時間を遡れるように、登録された GameTime を記録しておく機能も持たせた。
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using PackterViewer;

namespace PackterViewer
{
    class FlyPacketWithGateway : FlyPacket
    {
        double srcAddress = 0;
        double dstAddress = 0;
        double srcPort = 0;
        double dstPort = 0;
        double gatewayAddress = 0;
        double gatewayPort = 0;
        byte packetImageNumber = 0;
        string packetImageString = null;
        string packetDescription = null;

        GameTime gameTime = null;
        string originalString = "undefined";
        string originalResult = "undefined";

        public FlyPacketWithGateway() {
            gameTime = new GameTime();
        }

        public FlyPacketWithGateway(double sourceAddress, double sourcePort, double destinationAddress, double destinationPort
            , double gwAddress, double gwPort
            , byte imageNumber, string description, GameTime createdGameTime)
        {
            srcAddress = sourceAddress;
            srcPort = sourcePort;
            dstAddress = destinationPort;
            dstPort = destinationPort;
            gatewayAddress = gwAddress;
            gatewayPort = gwPort;
            packetImageNumber = imageNumber;
            packetDescription = description;
            gameTime = createdGameTime;
            originalString = String.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}"
                , sourceAddress, sourcePort, destinationAddress, destinationPort, gatewayAddress, gatewayPort, description, imageNumber);
        }

        public double SrcAddress
        {
            get { return srcAddress; }
        }

        public double DstAddress
        {
            get { return dstAddress; }
        }

        public double SrcPort
        {
            get { return srcPort; }
        }

        public double DstPort
        {
            get { return dstPort; }
        }

        public double GatewayAddress
        {
            get { return gatewayAddress; }
        }

        public double GatewayPort
        {
            get { return gatewayPort; }
        }

        public byte PacketImageNumber
        {
            get { return packetImageNumber; }
        }

        public string PacketImageString
        {
            get { return packetImageString; }
        }

        public GameTime CreatedGameTime
        {
            get { return gameTime; }
            //set { gameTime = value; }
        }
        public GameTime StartGameTime
        {
            set { }
        }

        double String2Double(string str)
        {
            try
            {
                if (str == null || str.Length <= 0)
                    return -1;
                double d;
                if (double.TryParse(str, out d) == false)
                    return -1;
                if (d >= 1 && d <= 65536 && str.IndexOf('.') < 0)
                    return d / 65536.0;
                if (d >= 0 && d <= 1)
                    return d;
                return -1;
            }
            catch { return -1; }
        }

        public bool SetFromData(StreamReader reader)
        {
            string line = reader.ReadLine();
            if (line == null)
                return false;
            char [] delimiters = { ',', '\n' };
            string[] words = line.Split(delimiters);
            if (words.Length < 8)
                return false;
            if ((srcAddress = IPv4Tools.IPString2Double(words[0])) < 0)
                return false;
            if ((dstAddress = IPv4Tools.IPString2Double(words[1])) < 0)
                return false;
            if ((srcPort = String2Double(words[2])) < 0)
                return false;
            if ((dstPort = String2Double(words[3])) < 0)
                return false;
            if ((gatewayAddress = IPv4Tools.IPString2Double(words[4])) < 0)
                return false;
            if ((gatewayPort = String2Double(words[5])) < 0)
                return false;

            srcAddress = ConfigReader.Instance.RevisionXAxis(srcAddress);
            dstAddress = ConfigReader.Instance.RevisionXAxis(dstAddress);
            srcPort = ConfigReader.Instance.RevisionYAxis(srcPort);
            dstPort = ConfigReader.Instance.RevisionYAxis(dstPort);
            
            packetImageString = words[6];
            if (byte.TryParse(words[7], out packetImageNumber) == false)
                packetImageNumber = 0;
            packetDescription = words[7];
            originalString = line;

            return true;
        }

        public bool setResult(StreamReader reader)
        {
            String result = reader.ReadToEnd();
            originalResult = result;
            return true;
        }

        public string OriginalResult
        {
            get { return originalResult; }
        }


        public string OriginalString
        {
            get { return originalString; }
        }

        public PacketBoard CreatePacketBoard(Model model, Texture2D texture, float defaultScale, Vector3 senderPosition, Vector3 receiverPosition, float flyMillisecond)
        {
            float srcX = (float)this.SrcAddress;
            float srcY = (float)this.SrcPort;
            float dstX = (float)this.DstAddress;
            float dstY = (float)this.DstPort;
            float gwX = (float)this.GatewayAddress;
            float gwY = (float)this.GatewayPort;
            byte imageNumber = this.PacketImageNumber;
            string fileName = this.PacketImageString;
            GameTime nowGameTime = this.CreatedGameTime;

            Vector3 startPoint = new Vector3((srcX - 0.5f) * 100.0f * 2 - defaultScale / 4.0f,
                ((srcY - 0.5f) * 100.0f * 2) - defaultScale / 4.0f, senderPosition.Z);
            Vector3 gwPoint = new Vector3((gwX - 0.5f) * 100.0f * 2 - defaultScale / 4.0f,
                ((gwY - 0.5f) * 100.0f * 2) - defaultScale / 4.0f, (senderPosition.Z + receiverPosition.Z) / 2);
            Vector3 endPoint = new Vector3((dstX - 0.5f) * 100.0f * 2 - defaultScale / 4.0f,
                ((dstY - 0.5f) * 100.0f * 2) - defaultScale / 4.0f, receiverPosition.Z);

            return new PacketBoardWithGateway(model, texture, startPoint, gwPoint, endPoint, nowGameTime, flyMillisecond, this.OriginalString);
        }
    }
}
