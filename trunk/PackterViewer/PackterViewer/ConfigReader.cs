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
        string sofTalkPath = "softalk/softalkw.exe";
        string packterMSGString1 = "<html lang=\"ja\"><body>";
        string packterMSGString2 = "</body></html>";
        Nullable<int> tickKeyInputAcceptMicrosecond = null;
        int msgBoxCloseTimeSec = 1000 * 10; // 10秒
        string loadPacketTarget = "ball";

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

        public void ParseArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                float f;
                if (args.Length > i + 1 && args[i] == "/size" && float.TryParse(args[i + 1], out f))
                {
                    if (f > 0)
                        defaultScale = f;
                    i++; continue;
                }

                if (args.Length > i + 1 && args[i] == "/softalk" && System.IO.File.Exists(args[i + 1]))
                {
                    sofTalkPath = args[i + 1];
                    i++; continue;
                }

                if (args.Length > i + 1 && args[i] == "/loadPacketTarget")
                {
                    SetLoadPacketTarget(args[i + 1]);
                    i++; continue;
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
