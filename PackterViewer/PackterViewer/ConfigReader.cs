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
        float defaultScale = 8.0f;
        string sofTalkPath = "softalk\\softalkw.exe";
        string packterMSGString1 = "<html lang=\"ja\"><body>";
        string packterMSGString2 = "</body></html>";
        Nullable<int> tickKeyInputAcceptMicrosecond = null;
        int msgBoxCloseTimeSec = 1000 * 10; // 10秒
        string loadPacketTarget = "ball";
        string senderBoard = null;
        string receiverBoard = null;
        float senderBoardScale = 1.0f;
        float receiverBoardScale = 1.0f;
        string senderBoardTextureFile = null;
        string receiverBoardTextureFile = null;
        Dictionary<string, string> htmlConvertList = new Dictionary<string, string>();
        string htmlConvertToCurrentDirTarget = null;

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
        public float SenderBoardScale
        {
            get { return senderBoardScale; }
        }
        public float ReceiverBoardScale
        {
            get { return receiverBoardScale; }
        }
        public string SenderBoardTextureFile
        {
            get { return senderBoardTextureFile; }
        }
        public string ReceiverBoardTextureFile
        {
            get { return receiverBoardTextureFile; }
        }
        public Dictionary<string, string> HtmlConvertList
        {
            get { return htmlConvertList; }
        }
        public string HtmlConvertToCurrentDirTarget
        {
            get { return htmlConvertToCurrentDirTarget; }
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

                    float tmpFloat = 0.0f;
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

                    if (key == "/htmlconverttocurrentdirtarget" && !string.IsNullOrEmpty(value))
                    {
                        htmlConvertToCurrentDirTarget = value;
                        i++; continue;
                    }
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
                        //value.Trim();
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
                            case "htmlconverttarget":
                                if(!string.IsNullOrEmpty(value)){
                                    value = value.Trim();
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
                                value = value.Trim();
                                if (!string.IsNullOrEmpty(value))
                                {
                                    htmlConvertToCurrentDirTarget = value;
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
    }
}
