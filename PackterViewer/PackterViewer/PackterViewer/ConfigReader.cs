using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace PackterViewer
{
    class ConfigReader
    {
        private static volatile ConfigReader instance;
        private static object syncObject = new Object();

        private ConfigReader() { }

        public static ConfigReader Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncObject)
                    {
                        if (instance == null)
                            instance = new ConfigReader();
                    }
                }
                return instance;
            }
        }
        
        float defaultScale = 8.0f;
        string sofTalkPath = "softalk\\softalkw.exe";
        string packterMSGString1 = "<html lang=\"ja\"><body>";
        string packterMSGString2 = "</body></html>";
        Nullable<int> tickKeyInputAcceptMicrosecond = null;
        int msgBoxCloseTimeSec = 1000 * 10; // 10秒
        string loadPacketTarget = "ball";
        string senderBoard = null;
        string receiverBoard = null;
        string gatewayBoard = null;
        float senderBoardScale = 1.0f;
        float receiverBoardScale = 1.0f;
        float gatewayBoardScale = 1.0f;
        string senderBoardTextureFile = null;
        string receiverBoardTextureFile = null;
        string gatewayBoardTextureFile = null;
        Dictionary<string, string> htmlConvertList = new Dictionary<string, string>();
        string htmlConvertToCurrentDirTarget = null;
        bool skydomeEnabled = false;
        string skydomeTexture = null;
        Microsoft.Xna.Framework.Color bgColor = Microsoft.Xna.Framework.Color.CornflowerBlue;
        string htmlConvertToCurrentWidthTarget = null;
        string htmlConvertToCurrentHeightTarget = null;
        int maxSENum = 10;
        string xACTFileForAudioEngine = null;
        string xACTFileForWaveBank = null;
        string xACTFileForSoundBank = null;
        string ballisticMapTextureImage = null;
        string ballisticMapMeshFile = null;
        float ballisticMapMeshScale = 1.0f;
        bool gatewayBoardEnabled = false;
        double xAxisStart = 0.0;
        double xAxisEnd = 1.0;
        double yAxisStart = 0.0;
        double yAxisEnd = 1.0;
        ushort bindPort = 11300;

        public enum ProgramMode
        {
            SenderReceiver,
            Ballistic,
        };
        ProgramMode programMode = ProgramMode.SenderReceiver;

        public float DefaultScale
        {
            get { return defaultScale; }
            set { defaultScale = value; }
        }
        public string SofTalkPath
        {
            get { return sofTalkPath; }
            set { if (value != null && System.IO.File.Exists(value)) sofTalkPath = value; }
        }
        public string PackterMSGString1
        {
            get { return packterMSGString1; }
        }
        public string PackterMSGString2
        {
            get { return packterMSGString2; }
        }
        public Nullable<int> TickKeyInputAcceptMicrosecond
        {
            get { return tickKeyInputAcceptMicrosecond; }
        }
        public int MsgBoxCloseTimeSec
        {
            get { return msgBoxCloseTimeSec; }
        }
        public string LoadPacketTarget
        {
            get { return loadPacketTarget; }
        }
        public string SenderBoardFile
        {
            get { return senderBoard; }
        }
        public string ReceiverBoardFile
        {
            get { return receiverBoard; }
        }
        public string GatewayBoardFile
        {
            get { return gatewayBoard; }
        }
        public float SenderBoardScale
        {
            get { return senderBoardScale; }
        }
        public float ReceiverBoardScale
        {
            get { return receiverBoardScale; }
        }
        public float GatewayBoardScale
        {
            get { return gatewayBoardScale; }
        }
        public string SenderBoardTextureFile
        {
            get { return senderBoardTextureFile; }
        }
        public string ReceiverBoardTextureFile
        {
            get { return receiverBoardTextureFile; }
        }
        public string GatewayBoardTextureFile
        {
            get { return gatewayBoardTextureFile; }
        }
        public Dictionary<string, string> HtmlConvertList
        {
            get { return htmlConvertList; }
        }
        public string HtmlConvertToCurrentDirTarget
        {
            get { return htmlConvertToCurrentDirTarget; }
        }
        public bool SkydomeEnabled
        {
            get { return skydomeEnabled; }
        }
        public string SkydomeTexture
        {
            get { return skydomeTexture; }
        }
        public Microsoft.Xna.Framework.Color BGColor
        {
            get { return bgColor; }
        }
        public string HtmlConvertToCurrentWidthTarget
        {
            get { return htmlConvertToCurrentWidthTarget; }
        }
        public string HtmlConvertToCurrentHeightTarget
        {
            get { return htmlConvertToCurrentHeightTarget; }
        }
        public int MaxSENum
        {
            get { return maxSENum; }
        }
        public bool XACTEnabled
        {
            get { return xACTFileForAudioEngine != null && xACTFileForSoundBank != null && xACTFileForWaveBank != null; }
        }
        public string XACTFileForAudioEngine
        {
            get { return xACTFileForAudioEngine; }
        }
        public string XACTFileForWaveBank
        {
            get { return xACTFileForWaveBank; }
        }
        public string XACTFileForSoundBank
        {
            get { return xACTFileForSoundBank; }
        }
        public ProgramMode Mode
        {
            get { return programMode; }
        }
        public string BallisticMapTextureImage
        {
            get { return ballisticMapTextureImage; }
        }
        public string BallisticMapMeshFile
        {
            get { return ballisticMapMeshFile; }
        }
        public float BallisticMapMeshScale
        {
            get { return ballisticMapMeshScale; }
        }
        public bool GatewayBoardEnabled
        {
            get { return gatewayBoardEnabled; }
        }
        public double XAxisStart
        {
            get { return xAxisStart; }
        }
        public double XAxisEnd
        {
            get { return xAxisEnd; }
        }
        public double YAxisStart
        {
            get { return yAxisStart; }
        }
        public double YAxisEnd
        {
            get { return yAxisEnd; }
        }
        public ushort BindPort
        {
            get { return bindPort; }
        }

        bool ConvertToFloat(string str, out float val)
        {
            val = 0.0f;
            try
            {
                val = float.Parse(str);
            }
            catch
            {
                return false;
            }
            return true;
        }

        bool ConvertIPv4ToDouble(string str, out double val)
        {
            val = IPv4Tools.IPString2Double(str);
            if (val < 0.0)
            {
                return false;
            }
            return true;
        }

        public void ParseArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                float f;
                string key = args[i].ToLower();
                if (args.Length > i + 1)
                {
                    string value = args[i + 1];
                    if (key == "/size" && float.TryParse(value, out f))
                    {
                        if (f > 0)
                            defaultScale = f;
                        i++; continue;
                    }

                    if (key == "/softalk" && System.IO.File.Exists(value))
                    {
                        sofTalkPath = value;
                        i++; continue;
                    }

                    if (key == "/loadpackettarget")
                    {
                        SetLoadPacketTarget(value);
                        i++; continue;
                    }

                    if (key == "/senderboardfile" && System.IO.File.Exists(value))
                    {
                        senderBoard = value;
                        i++; continue;
                    }

                    if (key == "/receiverboardfile" && System.IO.File.Exists(value))
                    {
                        receiverBoard = value;
                        i++; continue;
                    }

                    if (key == "/gatewayboardfile" && System.IO.File.Exists(value))
                    {
                        gatewayBoard = value;
                        i++; continue;
                    }

                    float tmpFloat = 0.0f;
                    double tmpDouble = 0.0;
                    if (key == "/senderboardscale" && ConvertToFloat(value, out tmpFloat))
                    {
                        senderBoardScale = tmpFloat;
                        i++; continue;
                    }
                    if (key == "/receiverboardscale" && ConvertToFloat(value, out tmpFloat))
                    {
                        receiverBoardScale = tmpFloat;
                        i++; continue;
                    }
                    if (key == "/gatewayboardscale" && ConvertToFloat(value, out tmpFloat))
                    {
                        gatewayBoardScale = tmpFloat;
                        i++; continue;
                    }

                    if (key == "/senderboardtexturefile" && System.IO.File.Exists(value))
                    {
                        senderBoardTextureFile = value;
                        i++; continue;
                    }

                    if (key == "/receiverboardtexturefile" && System.IO.File.Exists(value))
                    {
                        receiverBoardTextureFile = value;
                        i++; continue;
                    }

                    if (key == "/gatewayboardtexturefile" && System.IO.File.Exists(value))
                    {
                        gatewayBoardTextureFile = value;
                        i++; continue;
                    }

                    if (key == "/htmlconverttocurrentdirtarget" && !string.IsNullOrEmpty(value))
                    {
                        htmlConvertToCurrentDirTarget = value;
                        i++; continue;
                    }

                    if (key == "/skydometexture" && System.IO.File.Exists(value))
                    {
                        skydomeEnabled = true;
                        skydomeTexture = value;
                        i++; continue;
                    }
                    if (key == "/bgcolor")
                    {
                        bgColor = PhaseColor(value, bgColor);
                        i++; continue;
                    }

                    if (key == "/htmlconverttocurrentwidthtarget" && !string.IsNullOrEmpty(value))
                    {
                        htmlConvertToCurrentWidthTarget = value;
                        i++; continue;
                    }

                    if (key == "/htmlconverttocurrentheighttarget" && !string.IsNullOrEmpty(value))
                    {
                        htmlConvertToCurrentHeightTarget = value;
                        i++; continue;
                    }

                    if (key == "/maxsenum" && !string.IsNullOrEmpty(value))
                    {
                        maxSENum = int.Parse(value);
                        i++; continue;
                    }

                    if (key == "/ballisticmaptextureimage" && !string.IsNullOrEmpty(value) && System.IO.File.Exists(value))
                    {
                        ballisticMapTextureImage = value;
                        i++; continue;
                    }

                    if (key == "/ballisticmapmeshfile" && !string.IsNullOrEmpty(value) && System.IO.File.Exists(value))
                    {
                        ballisticMapMeshFile = value;
                        i++; continue;
                    }

                    if (key == "/ballisticmapmeshscale" && !string.IsNullOrEmpty(value) && ConvertToFloat(value, out tmpFloat))
                    {
                        ballisticMapMeshScale = tmpFloat;
                        i++; continue;
                    }

                    if (key == "/xaxisstart" && !string.IsNullOrEmpty(value) && ConvertIPv4ToDouble(value, out tmpDouble))
                    {
                        xAxisStart = tmpDouble;
                        continue;
                    }
                    if (key == "/xaxisend" && !string.IsNullOrEmpty(value) && ConvertIPv4ToDouble(value, out tmpDouble))
                    {
                        xAxisEnd = tmpDouble;
                        continue;
                    }
                    if (key == "/yaxisstart" && !string.IsNullOrEmpty(value) && ConvertIPv4ToDouble(value, out tmpDouble))
                    {
                        yAxisStart = tmpDouble;
                        continue;
                    }
                    if (key == "/yaxisend" && !string.IsNullOrEmpty(value) && ConvertIPv4ToDouble(value, out tmpDouble))
                    {
                        yAxisEnd = tmpDouble;
                        continue;
                    }
                    if (key == "/bindport" && !string.IsNullOrEmpty(value) && Convert.ToUInt16(value) > 0 && Convert.ToUInt16(value) <= 65535)
                    {
                        bindPort = Convert.ToUInt16(value);
                        continue;
                    }

                }

                if (key == "/enableskydome")
                {
                    skydomeEnabled = true;
                    continue;
                }

                if (key == "/ballisticmode")
                {
                    programMode = ProgramMode.Ballistic;
                    continue;
                }

                if (key == "/senderreceivermode")
                {
                    programMode = ProgramMode.SenderReceiver;
                    continue;
                }

                if (key == "/gatewayboardenable")
                {
                    gatewayBoardEnabled = true;
                    continue;
                }

                if (System.IO.File.Exists(args[i]))
                {
                    ReadFromFile(args[i]);
                    continue;
                }
            }
        }
        
        public bool ReadFromFile(string fileName)
        {
            try
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(fileName, System.Text.Encoding.GetEncoding("Shift_JIS"));
                string text;
                while ((text = reader.ReadLine()) != null)
                {
                    if (text.IndexOf('#') == 0)
                        continue;
                    string[] list = text.Split(new char[] { '=' }, 2);
                    if (list.Length == 2)
                    {
                        string key = list[0];
                        string value = list[1];
                        key = key.Trim();
                        key = key.ToLower();
                        float tmpFloat = 0.0f;
                        double tmpDouble = 0.0;
                        value = value.Trim();
                        switch (key)
                        {
                            case "size":
                                {
                                    float f;
                                    if (float.TryParse(value, out f))
                                    {
                                        if (f > 0)
                                            defaultScale = f;
                                    }
                                }
                                break;
                            case "softalk":
                                if (System.IO.File.Exists(value))
                                {
                                    sofTalkPath = value;
                                }
                                break;
                            case "packtermsgformat1":
                                packterMSGString1 = value;
                                break;
                            case "packtermsgformat2":
                                packterMSGString2 = value;
                                break;
                            case "tickkeyinputacceptmicrosecond":
                                try
                                {
                                    tickKeyInputAcceptMicrosecond = int.Parse(value);
                                }
                                catch
                                {
                                    tickKeyInputAcceptMicrosecond = null;
                                }
                                break;
                            case "msgboxclosetimesec":
                                try
                                {
                                    int sec = int.Parse(value);
                                    if (sec > 0)
                                    {
                                        msgBoxCloseTimeSec = sec * 1000;
                                    }
                                }
                                catch { }
                                break;
                            case "loadpackettarget":
                                SetLoadPacketTarget(value);
                                break;
                            case "senderboardfile":
                                if (System.IO.File.Exists(value))
                                {
                                    senderBoard = value;
                                }
                                break;
                            case "receiverboardfile":
                                if (System.IO.File.Exists(value))
                                {
                                    receiverBoard = value;
                                }
                                break;
                            case "gatewayboardfile":
                                if (System.IO.File.Exists(value))
                                {
                                    gatewayBoard = value;
                                }
                                break;
                            case "senderboardscale":
                                if (ConvertToFloat(value, out tmpFloat))
                                {
                                    senderBoardScale = tmpFloat;
                                }
                                break;
                            case "receiverboardscale":
                                if (ConvertToFloat(value, out tmpFloat))
                                {
                                    receiverBoardScale = tmpFloat;
                                }
                                break;
                            case "gatewayboardscale":
                                if (ConvertToFloat(value, out tmpFloat))
                                {
                                    gatewayBoardScale = tmpFloat;
                                }
                                break;
                            case "senderboardtexturefile":
                                if (System.IO.File.Exists(value))
                                {
                                    senderBoardTextureFile = value;
                                }
                                break;
                            case "receiverboardtexturefile":
                                if (System.IO.File.Exists(value))
                                {
                                    receiverBoardTextureFile = value;
                                }
                                break;
                            case "gatewayboardtexturefile":
                                if (System.IO.File.Exists(value))
                                {
                                    gatewayBoardTextureFile = value;
                                }
                                break;
                            case "htmlconverttarget":
                                if(!string.IsNullOrEmpty(value)){
                                    if (value.IndexOf(' ') > 0)
                                    {
                                        string[] keyValue = value.Split(new char[] { ' ' }, 2);
                                        if (keyValue.Length == 2)
                                        {
                                            htmlConvertList.Add(keyValue[0], keyValue[1]);
                                        }
                                    }
                                }
                                break;
                            case "htmlconverttocurrentdirtarget":
                                if (!string.IsNullOrEmpty(value))
                                {
                                    htmlConvertToCurrentDirTarget = value;
                                }
                                break;

                            case "enableskydome":
                                if(!string.IsNullOrEmpty(value)){
                                    if (value.ToLower() == "true")
                                    {
                                        skydomeEnabled = true;
                                    }
                                }
                                break;
                            case "skydometexture":
                                if (!string.IsNullOrEmpty(value))
                                {
                                    if (System.IO.File.Exists(value))
                                    {
                                        skydomeEnabled = true;
                                        skydomeTexture = value;
                                    }
                                }
                                break;
                            case "bgcolor":
                                if (!string.IsNullOrEmpty(value))
                                {
                                    bgColor = PhaseColor(value, bgColor);
                                }
                                break;

                            case "htmlconverttocurrentwidthtarget":
                                if (!string.IsNullOrEmpty(value))
                                {
                                    htmlConvertToCurrentWidthTarget = value;
                                }
                                break;

                            case "htmlconverttocurrentheighttarget":
                                if (!string.IsNullOrEmpty(value))
                                {
                                    htmlConvertToCurrentHeightTarget = value;
                                }
                                break;
                                
                            case "maxsenum":
                                if (!string.IsNullOrEmpty(value))
                                {
                                    int num = int.Parse(value);
                                    if (num > 0)
                                    {
                                        maxSENum = num;
                                    }
                                }
                                break;
                            case "xactfileforaudioengine":
                                if (!string.IsNullOrEmpty(value) && System.IO.File.Exists(value))
                                {
                                    xACTFileForAudioEngine = value;
                                }
                                break;
                            case "xactfileforwavebank":
                                if (!string.IsNullOrEmpty(value) && System.IO.File.Exists(value))
                                {
                                    xACTFileForWaveBank = value;
                                }
                                break;
                            case "xactfileforsoundbank":
                                if (!string.IsNullOrEmpty(value) && System.IO.File.Exists(value))
                                {
                                    xACTFileForSoundBank = value;
                                }
                                break;
                            case "programmode":
                                switch(value.ToLower()){
                                    case "ballistic":
                                        programMode = ProgramMode.Ballistic;
                                        break;
                                    case "senderreceiver":
                                        programMode = ProgramMode.SenderReceiver;
                                        break;
                                    default:
                                        break;
                                }
                                break;

                            case "ballisticmaptextureimage":
                                if (!string.IsNullOrEmpty(value) && System.IO.File.Exists(value))
                                {
                                    ballisticMapTextureImage = value;
                                }
                                break;

                            case "ballisticmapmeshfile":
                                if (!string.IsNullOrEmpty(value) && System.IO.File.Exists(value))
                                {
                                    ballisticMapMeshFile = value;
                                }
                                break;

                            case "ballisticmapmeshscale":
                                if (!string.IsNullOrEmpty(value) && ConvertToFloat(value, out tmpFloat))
                                {
                                    ballisticMapMeshScale = tmpFloat;
                                }
                                break;

                            case "gatewayboardenable":
                                if (!string.IsNullOrEmpty(value))
                                {
                                    if (value.ToLower() == "true")
                                    {
                                        gatewayBoardEnabled = true;
                                    }
                                    else
                                    {
                                        gatewayBoardEnabled = false;
                                    }
                                }
                                break;

                            case "bindport":
                                if (!string.IsNullOrEmpty(value) && Convert.ToUInt16(value) > 0 && Convert.ToUInt16(value) <= 65535)
                                {
                                    bindPort = Convert.ToUInt16(value);
                                }
                                break;

                            case "xaxisstart":
                                if (!string.IsNullOrEmpty(value) && ConvertIPv4ToDouble(value, out tmpDouble))
                                {
                                    xAxisStart = tmpDouble;
                                }
                                break;

                            case "xaxisend":
                                if (!string.IsNullOrEmpty(value) && ConvertIPv4ToDouble(value, out tmpDouble))
                                {
                                    xAxisEnd = tmpDouble;
                                }
                                break;

                            case "yaxisstart":
                                if (!string.IsNullOrEmpty(value) && ConvertIPv4ToDouble(value, out tmpDouble))
                                {
                                    yAxisStart = tmpDouble;
                                }
                                break;

                            case "yaxisend":
                                if (!string.IsNullOrEmpty(value) && ConvertIPv4ToDouble(value, out tmpDouble))
                                {
                                    yAxisEnd = tmpDouble;
                                }
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        void SetLoadPacketTarget(string target)
        {
            string t = target.ToLower();
            switch (t)
            {
                case "ball":
                case "board":
                    loadPacketTarget = t;
                    break;
                default:
                    break;
            }
        }

        Microsoft.Xna.Framework.Color PhaseColor(string value, Microsoft.Xna.Framework.Color currentColor)
        {
            try
            {
                byte r = Convert.ToByte(value.Substring(0, 2), 16);
                byte g = Convert.ToByte(value.Substring(2, 2), 16);
                byte b = Convert.ToByte(value.Substring(4, 2), 16);
                return new Microsoft.Xna.Framework.Color(r, g, b);
            }
            catch
            {
                return currentColor;
            }
        }

        public double RevisionXAxis(double value)
        {
            // srcAddress, dstAddres, srcPort, dstPort を ConfigReader.Instance.XAxsisStart 等で補正します。
            double xWidth = ConfigReader.Instance.XAxisEnd - ConfigReader.Instance.XAxisStart;
            if (double.IsNaN(0.0 / xWidth))
            {
                xWidth = 1.0;
            }

            return (value - ConfigReader.Instance.XAxisStart) / xWidth;
        }
        public double RevisionYAxis(double value)
        {
            double yWidth = ConfigReader.Instance.YAxisEnd - ConfigReader.Instance.YAxisStart;
            if (double.IsNaN(0.0 / yWidth))
            {
                yWidth = 1.0;
            }
            return (value - ConfigReader.Instance.YAxisStart) / yWidth;
        }
    }
}
