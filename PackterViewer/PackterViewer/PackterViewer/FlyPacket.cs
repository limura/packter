using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using PackterViewer;

namespace PackterViewer
{
    public interface FlyPacket
    {
        double SrcAddress
        {
            get;
        }

        double DstAddress
        {
            get;
        }

        double SrcPort
        {
            get;
        }

        double DstPort
        {
            get;
        }

        byte PacketImageNumber
        {
            get;
        }

        string PacketImageString
        {
            get;
        }

        GameTime CreatedGameTime
        {
            get;
            //set;
        }
        GameTime StartGameTime
        {
            set;
        }

        bool SetFromData(StreamReader reader);
        bool setResult(StreamReader reader);

        string OriginalResult
        {
            get;
        }

        string OriginalString
        {
            get;
        }

        PacketBoard CreatePacketBoard(Model model, Texture2D texture, float defaultScale, Vector3 senderPosition, Vector3 receiverPosition, float flyMillisecond);
    }
}
