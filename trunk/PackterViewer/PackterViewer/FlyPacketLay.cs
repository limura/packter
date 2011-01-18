/*
 * packter sender ���瑗����f�[�^��s���̃f�[�^��ۊǂ��邽�߂�class
 *
 * srcAddress, srcPort
 * dstAddress, dstPort �Ƃ������O��4�� float �̒l
 * (���ꂼ�� 0.0 ���� 1.0 �܂ł̒l�ł��邱�Ƃ����҂���Ă���)�ƁA
 * �p�P�b�g�摜�ԍ�(packetImageNumber) �������Ă���B
 * 
 * packter sender���瑗��ꂽ�����������炵����͂���@�\������class���󂯎����Ă���
 *
 * �g���Ƃ��āA���Ԃ�k���悤�ɁA�o�^���ꂽ GameTime ���L�^���Ă����@�\�����������B
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
    class FlyPacketLay : FlyPacket
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

        public FlyPacketLay() {
            gameTime = new GameTime();
        }

        public FlyPacketLay(float sourceAddress, float sourcePort, float destinationAddress, float destinationPort
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
            if ((srcAddress = IPv4Tools.IPString2float(words[0])) < 0)
                return false;
            if ((dstAddress = IPv4Tools.IPString2float(words[1])) < 0)
                return false;
            if ((srcPort = String2float(words[2])) < 0)
                return false;
            if ((dstPort = String2float(words[3])) < 0)
                return false;
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
            float srcY = this.SrcPort;
            float dstX = this.DstAddress;
            float dstY = this.DstPort;
            byte imageNumber = this.PacketImageNumber;
            string fileName = this.PacketImageString;
            GameTime nowGameTime = this.CreatedGameTime;

            Vector3 startPoint = new Vector3((srcX - 0.5f) * 100.0f * 2 - defaultScale / 4.0f,
                ((srcY - 0.5f) * 100.0f * 2) - defaultScale / 4.0f, senderPosition.Z);
            Vector3 endPoint = new Vector3((dstX - 0.5f) * 100.0f * 2 - defaultScale / 4.0f,
                ((dstY - 0.5f) * 100.0f * 2) - defaultScale / 4.0f, receiverPosition.Z);

            return new PacketBoardLay(model, texture, startPoint, endPoint, nowGameTime, flyMillisecond, this.OriginalString);
        }
    }
}
