#region File Description
//-----------------------------------------------------------------------------
// MainForm.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.Windows.Forms;
using Packter_viewer2;
using Packter_viewer;
using PackterViewer;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
#endregion

namespace WinFormsGraphicsDevice
{
    // System.Drawing and the XNA Framework both define Color types.
    // To avoid conflicts, we define shortcut names for them both.
    using GdiColor = System.Drawing.Color;
    using XnaColor = Microsoft.Xna.Framework.Graphics.Color;
    
    /// <summary>
    /// Custom form provides the main user interface for the program.
    /// In this sample we used the designer to add a splitter pane to the form,
    /// which contains a SpriteFontControl and a SpinningTriangleControl.
    /// </summary>
    public partial class MainForm : Form
    {
        ConfigReader configReader = new ConfigReader();
        ContentBuilder contentBuilder;
        ContentManager contentManager;
        string webBrowserTargetText;
        string caractorImageFile = null;
        System.Windows.Forms.Timer webBrowserVisibleChangeTimer = new System.Windows.Forms.Timer();
        System.Windows.Forms.Timer intervalTimer = new System.Windows.Forms.Timer();
        System.Windows.Forms.Timer bgmTimer = new System.Windows.Forms.Timer();
        System.Uri imgUri;
        System.Media.SoundPlayer bgmSoundPlayer = null;

        bool webBrowserDocumentCompleted = false;

        public MainForm(string[] args)
        {
            InitializeComponent();

            configReader.ParseArgs(args);
            webBrowserVisibleChangeTimer.Interval = configReader.MsgBoxCloseTimeSec;

            contentBuilder = new ContentBuilder();
            contentManager = new ContentManager(packterDisplayControl.Services, contentBuilder.OutputDirectory);
            packterDisplayControl.RegisterData(contentBuilder, contentManager, configReader.DefaultScale, configReader);
            if (configReader.TickKeyInputAcceptMicrosecond != null)
            {
                packterDisplayControl.OverrideKeyInputAcceptGapTimeMicrosecond(configReader.TickKeyInputAcceptMicrosecond.GetValueOrDefault());
            }

            intervalTimer.Tick += new System.EventHandler(intervalTimer_Tick);
            intervalTimer.Interval = 100;
            intervalTimer.Start();

            webBrowserVisibleChangeTimer.Tick += new System.EventHandler(webBrowserVisibleChangeTimer_Tick);
            bgmTimer.Tick += new System.EventHandler(bgmTimer_Tick);

            webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser_DocumentCompleted);
            imgUri = new System.Uri(System.IO.Directory.GetCurrentDirectory() /* + "../../../../Content/" */);
            webBrowserTargetText
                = "<html><body><div style=\"valign=top\"><img height=100% src=\"" + imgUri.ToString() + "teacher.png\">攻撃を検知しました。</body></html>";
            webBrowser.Navigate("about:blank");
        }

        void bgmTimer_Tick(object sender, System.EventArgs e)
        {
            if (bgmSoundPlayer != null)
            {
                bgmSoundPlayer.Stop();
            }
            bgmSoundPlayer = null;
            bgmTimer.Stop();
        }

        void intervalTimer_Tick(object sender, System.EventArgs e)
        {
            if (packterDisplayControl != null)
            {
                List<Dictionary<string, List<string>>> msgList = new List<Dictionary<string, List<string>>>();
                msgList.Add(packterDisplayControl.GetQueuedMessagesV4());
                msgList.Add(packterDisplayControl.GetQueuedMessagesV6());
                foreach (Dictionary<string, List<string>> msg in msgList)
                {
                    if (msg != null)
                    {
                        string targetHtml = null;
                        if (msg.ContainsKey(PacketReader.HTMLTriggerString)) // plain HTML
                        {
                            List<string> stringList = msg[PacketReader.HTMLTriggerString];
                            if (stringList.Count > 0)
                            {
                                targetHtml = stringList[stringList.Count - 1];
                            }
                        }
                        string msgString = null;
                        if (msg.ContainsKey(PacketReader.MSGTriggerString)) // 画像番号と文字列の奴
                        {
                            List<string> stringList = msg[PacketReader.MSGTriggerString];
                            // format案
                            // img番号, 文字列
                            if (stringList.Count > 0)
                            {
                                msgString = stringList[stringList.Count - 1];
                            }
                        }
                        string imgFileName = null;
                        if (!string.IsNullOrEmpty(msgString))
                        {
                            int pos = msgString.IndexOf(',');
                            if (pos > 0)
                            {
                                string numString = msgString.Substring(0, pos);
                                imgFileName = numString;
                                string htmlString = msgString.Substring(pos + 1);
                                targetHtml = configReader.PackterMSGString1 + htmlString + configReader.PackterMSGString2;
                            }
                        }
                        if (!string.IsNullOrEmpty(targetHtml))
                        {
                            LoadHtml(imgFileName, targetHtml);
                        }

                        if (msg.ContainsKey(PacketReader.SOUNDTriggerString)) // sound
                        {
                            List<string> targetList = msg[PacketReader.SOUNDTriggerString];
                            if(targetList.Count > 0){
                                if (bgmSoundPlayer != null)
                                {
                                    bgmSoundPlayer.Stop();
                                }
                                bgmTimer.Stop();
                                try
                                {
                                    string target = targetList[targetList.Count - 1];
                                    string[] cmdList = target.Split(new char[] {','}, 2);
                                    if (cmdList != null && cmdList.Length == 2)
                                    {
                                        int loopTime = int.Parse(cmdList[0]);
                                        string file = cmdList[1].Trim();
                                        if (loopTime > 0)
                                        {
                                            bgmSoundPlayer = new System.Media.SoundPlayer(file);
                                            if (bgmSoundPlayer != null)
                                                bgmSoundPlayer.Play();
                                            bgmTimer.Interval = loopTime;
                                            bgmTimer.Start();
                                        }
                                        else if (loopTime < 0)
                                        {
                                            bgmSoundPlayer = new System.Media.SoundPlayer(file);
                                            if (bgmSoundPlayer != null)
                                                bgmSoundPlayer.PlayLooping();
                                            bgmTimer.Interval = -loopTime;
                                            bgmTimer.Start();
                                        }
                                    }
                                }
                                catch
                                {
                                    bgmSoundPlayer = null;
                                }
                            }
                        }

                        if (msg.ContainsKey(PacketReader.VOICETriggerString)) // ゆっくりボイス
                        {
                            List<string> targetList = msg[PacketReader.VOICETriggerString];
                            if (targetList.Count > 0 && !string.IsNullOrEmpty(targetList[targetList.Count - 1]))
                            {
                                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                                startInfo.FileName = configReader.SofTalkPath;
                                startInfo.Arguments = targetList[targetList.Count - 1];
                                startInfo.CreateNoWindow = true;
                                startInfo.UseShellExecute = true;
                                try
                                {
                                    System.Diagnostics.Process.Start(startInfo);
                                }
                                catch (System.ComponentModel.Win32Exception w32error)
                                {
                                    //PutLog(e.Message);
                                }
                            }
                        }
                    }
                }

                if (packterDisplayControl.StatusDraw != webBrowser.Visible)
                {
                    if (packterDisplayControl.StatusDraw)
                    {
                        OpenWebBrowser();
                    }else{
                        CloseWebBrowser();
                    }
                    webBrowserVisibleChangeTimer.Stop();
                }
            }
        }

        void webBrowserVisibleChangeTimer_Tick(object sender, System.EventArgs e)
        {
            UnloadHtml();
        }

        void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            webBrowserDocumentCompleted = true;
            //if (!string.IsNullOrEmpty(webBrowserTargetText) && webBrowser.DocumentText != webBrowserTargetText)
            //{
            //    OpenWebBrowser();
            //    webBrowser.DocumentText = webBrowserTargetText;
            //    webBrowserTargetText = null;
            //}
        }

        void MessageWindowUpdateHandler(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                UnloadHtml();
            }
            else
            {
                LoadHtml(null, text);
            }
        }

        private void OpenWebBrowser()
        {
            packterDisplayControl.Height = basePanel.Height - webBrowser.Height;
            webBrowser.Visible = true;
            packterDisplayControl.SetCaraImageTexture(caractorImageFile);
            packterDisplayControl.StatusDraw = true;
        }

        private void CloseWebBrowser()
        {
            packterDisplayControl.Height = basePanel.Height;
            webBrowser.Visible = false;
            packterDisplayControl.StatusDraw = false;
            packterDisplayControl.SetCaraImageTexture(null);
        }

        private void UnloadHtml()
        {
            CloseWebBrowser();
            webBrowserTargetText = "";
            webBrowserVisibleChangeTimer.Stop();
        }
        private void LoadHtml(string filename, string txt)
        {
            if (string.IsNullOrEmpty(txt) || webBrowserDocumentCompleted == false)
            {
                UnloadHtml();
                return;
            }
            webBrowserTargetText = null;
            webBrowser.DocumentText = txt;
            caractorImageFile = filename;
            OpenWebBrowser();
            webBrowserVisibleChangeTimer.Stop();
            webBrowserVisibleChangeTimer.Start();
            //webBrowser.Navigate("http://127.0.0.1:12345/");
        }

        private void packterDisplayControl_KeyDown(object sender, KeyEventArgs e)
        {
            packterDisplayControl.ProcessKeyDown(e.KeyCode);
        }

        private void packterDisplayControl_KeyUp(object sender, KeyEventArgs e)
        {
            packterDisplayControl.ProcessKeyUp(e.KeyCode);
        }

        private void packterDisplayControl_KeyPress(object sender, KeyPressEventArgs e)
        {
            //packterDisplayControl.ProcessKeyDown();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            packterDisplayControl.UnloadContent();
            webBrowserVisibleChangeTimer.Stop();
            intervalTimer.Stop();
            webBrowserVisibleChangeTimer.Dispose();
            intervalTimer.Dispose();
        }
    }
}
