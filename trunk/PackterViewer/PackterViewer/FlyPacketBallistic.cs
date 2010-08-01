/*
 * packter sender から送られるデータ一行分のデータを保管するためのclass
 *
 * srcAddress, srcPort
 * dstAddress, dstPort という名前の4つの float の値
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

namespace Packter_viewer
{
    class FlyPacketBallistic : FlyPacket
    {
        float srcAddress = 0;
        float dstAddress = 0;
        float srcPort = 0;
        float dstPort = 0;
        byte packetImageNumber = 0;
        string packetImageString = null;
        string packetDescription = null;

        GameTime gameTime = null;
        string originalString = "undefined";
        string originalResult = "undefined";

        public FlyPacketBallistic() {
            gameTime = new GameTime();
        }

        public FlyPacketBallistic(float sourceAddress, float sourcePort, float destinationAddress, float destinationPort
            , byte imageNumber, string description, GameTime createdGameTime)
        {
            srcAddress = sourceAddress;
            srcPort = sourcePort;
            dstAddress = destinationPort;
            dstPort = destinationPort;
            packetImageNumber = imageNumber;
            packetDescription = description;
            gameTime = createdGameTime;
            originalString = String.Format("{0}, {1}, {2}, {3}, {4}, {5}"
                , sourceAddress, sourcePort, destinationAddress, destinationPort, description, imageNumber);
        }
        
        public float SrcAddress
        {
            get { return srcAddress; }
        }
        
        public float DstAddress
        {
            get { return dstAddress; }
        }
        
        public float SrcPort
        {
            get { return srcPort; }
        }
        
        public float DstPort
        {
            get { return dstPort; }
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
            set { gameTime = value; }
        }

        float IPString2float(string str)
        {
            try
            {
                if (str == null || str.Length <= 0)
                    return -1;
                float f = 0;
                if (str.IndexOf('.') >= 0 && str.IndexOf('.') == str.LastIndexOf('.') && float.TryParse(str, out f) == true && f <= 1 && f >= 0)
                {
                    return f;
                }
                IPAddress addr = IPAddress.Parse(str);
                if (addr == null)
                {
                    return -1;
                }
                byte[] ba = addr.GetAddressBytes();
                if (ba.Length < 4)
                    return -1;

                f = 0;
                float fmax = 0;
                foreach (byte b in ba)
                {
                    f = f * 256 + b;
                    fmax = fmax * 256 + 255;
                }
                return f / fmax;
            }
            catch { return -1; }
        }
        float String2float(string str)
        {
            try
            {
                if (str == null || str.Length <= 0)
                    return -1;
                float f;
                if (float.TryParse(str, out f) == false)
                    return -1;
                if (f >= 1 && f <= 65536 && str.IndexOf('.') < 0)
                    return f / 65536.0f;
                if (f >= 0 && f <= 1)
                    return f;
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
            if (words.Length < 6)
                return false;
            if ((srcAddress = IPString2float(words[0])) < 0)
                return false;
            if ((dstAddress = IPString2float(words[1])) < 0)
                return false;
            if ((srcPort = String2float(words[2])) < 0)
                return false;
            if ((dstPort = String2float(words[3])) < 0)
                return false;
            System.Diagnostics.Debug.WriteLine(srcAddress.ToString() + ", " + srcPort.ToString() + " => " + dstAddress.ToString() + ", " + dstPort.ToString());
            packetImageString = words[4];
            if (byte.TryParse(words[4], out packetImageNumber) == false)
                packetImageNumber = 0;
            packetDescription = words[5];
            originalString = line;
            
            return true;
#if false // good old bunary format reader.
            int size = buffer.Length;
            // srcAddress
            if (size - start - 4 < 0)
                return false;
            srcAddress = (UInt32)((buffer[start] << 24) + (buffer[start + 1] << 16) + (buffer[start + 2] << 8) + buffer[start + 3]);
            start += 4;

            // dstAddress
            if (size - start - 4 < 0)
                return false;
            dstAddress = (UInt32)((buffer[start] << 24) + (buffer[start + 1] << 16) + (buffer[start + 2] << 8) + buffer[start + 3]);
            start += 4;

            // srcPort
            if (size - start - 2 < 0)
                return false;
            srcPort = (UInt16)((buffer[start] << 8) + buffer[start + 1]);
            start += 2;

            // dstPort
            if (size - start - 2 < 0)
                return false;
            dstPort = (UInt16)((buffer[start] << 8) + buffer[start + 1]);
            start += 2;

            // packetImageNumber
            if (size - start - 1 < 0)
                return false;
            packetImageNumber = buffer[start];
            start += 1;

            // dummy maybe '\n'
            start += 1;
            
            return false;
#endif
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
            float srcX = this.SrcAddress;
            float srcZ = this.SrcPort;
            float dstX = this.DstAddress;
            float dstZ = this.DstPort;
            byte imageNumber = this.PacketImageNumber;
            string fileName = this.PacketImageString;
            GameTime nowGameTime = this.CreatedGameTime;

            Vector3 startPoint = new Vector3((srcX - 0.5f) * 100.0f * 2 - defaultScale / 4.0f
                    , 200.0f
                , ((srcZ - 0.5f) * 100.0f * 2) - defaultScale / 4.0f);
            Vector3 endPoint = new Vector3((dstX - 0.5f) * 100.0f * 2 - defaultScale / 4.0f
                    , 200.0f
                , ((dstZ - 0.5f) * 100.0f * 2) - defaultScale / 4.0f);

            return new PacketBoardBallistic(model, texture, startPoint, endPoint, nowGameTime, flyMillisecond, this.OriginalString);
        }
    }
}
