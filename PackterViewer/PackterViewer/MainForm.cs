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
        ContentBuilder contentBuilder;
        ContentManager contentManager;
        string webBrowserTargetText;
        System.Windows.Forms.Timer webBrowserVisibleChangeTimer = new System.Windows.Forms.Timer();
        System.Windows.Forms.Timer intervalTimer = new System.Windows.Forms.Timer();
        System.Uri imgUri;
        System.Media.SoundPlayer bgmSoundPlayer = null;

        public MainForm()
        {
            InitializeComponent();

            contentBuilder = new ContentBuilder();
            contentManager = new ContentManager(packterDisplayControl.Services, contentBuilder.OutputDirectory);
            packterDisplayControl.RegisterContentBuilder(contentBuilder, contentManager);

            intervalTimer.Tick += new System.EventHandler(intervalTimer_Tick);
            intervalTimer.Interval = 100;
            intervalTimer.Start();

            webBrowserVisibleChangeTimer.Interval = 30 * 1000; // 30ïb
            webBrowserVisibleChangeTimer.Tick += new System.EventHandler(webBrowserVisibleChangeTimer_Tick);

            webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser_DocumentCompleted);
            imgUri = new System.Uri(System.IO.Directory.GetCurrentDirectory() + "../../../../Content/");
            webBrowserTargetText
                = "<html><body><div style=\"valign=top\"><img height=100% src=\"" + imgUri.ToString() + "teacher.png\">çUåÇÇåüímÇµÇ‹ÇµÇΩÅB</body></html>";
            webBrowser.Navigate("http://www.google.co.jp");
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
                        if (msg.ContainsKey("PACTERHTML")) // plain HTML
                        {
                            List<string> stringList = msg["PACTERHTML"];
                            if (stringList.Count > 0)
                            {
                                targetHtml = stringList[stringList.Count - 1];
                            }
                        }
                        string msgString = null;
                        if (msg.ContainsKey("PACTERMSG")) // âÊëúî‘çÜÇ∆ï∂éöóÒÇÃìz
                        {
                            List<string> stringList = msg["PACTERMSG"];
                            // formatàƒ
                            // imgî‘çÜ, ï∂éöóÒ
                            if (stringList.Count > 0)
                            {
                                msgString = stringList[stringList.Count - 1];
                            }
                        }
                        if (!string.IsNullOrEmpty(msgString))
                        {
                            int pos = msgString.IndexOf(',');
                            string numString = msgString.Substring(0, pos);
                            string htmlString = msgString.Substring(pos + 1);
                            int num = int.Parse(numString);
                            if (num >= 1 && num <= 10 && !string.IsNullOrEmpty(htmlString))
                            {
                                targetHtml = "<html lang=\"ja\"><body><img height=100% src=\""
                                    + imgUri.ToString() + numString + ".png\">" + htmlString + "</body></html>";
                            }
                        }
                        if (!string.IsNullOrEmpty(targetHtml))
                        {
                            LoadHtml(targetHtml);
                        }

                        if (msg.ContainsKey("PACTERSOUND")) // sound
                        {
                            List<string> targetList = msg["PACTERSOUND"];
                            if(targetList.Count > 0){
                                if (bgmSoundPlayer != null)
                                {
                                    bgmSoundPlayer.Stop();
                                }
                                try
                                {
                                    bgmSoundPlayer = new System.Media.SoundPlayer(targetList[targetList.Count - 1]);
                                    if(bgmSoundPlayer != null)
                                        bgmSoundPlayer.PlayLooping();
                                }
                                catch
                                {
                                    bgmSoundPlayer = null;
                                }
                            }
                        }
                    }
                }
            }
        }

        void webBrowserVisibleChangeTimer_Tick(object sender, System.EventArgs e)
        {
            webBrowser.Visible = false;
            webBrowserVisibleChangeTimer.Stop();
        }

        void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (!string.IsNullOrEmpty(webBrowserTargetText) && webBrowser.DocumentText != webBrowserTargetText)
            {
                webBrowser.DocumentText = webBrowserTargetText;
                webBrowserTargetText = null;
            }
        }

        void MessageWindowUpdateHandler(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                UnloadHtml();
            }
            else
            {
                LoadHtml(text);
            }
        }

        private void UnloadHtml()
        {
            webBrowserTargetText = "";
            webBrowser.Visible = false;
            webBrowserVisibleChangeTimer.Stop();
        }
        private void LoadHtml(string txt)
        {
            webBrowserTargetText = txt;
            webBrowserVisibleChangeTimer.Stop();
            webBrowserVisibleChangeTimer.Start();
            webBrowser.Visible = true;
            webBrowser.Navigate("http://127.0.0.1:12345/");
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
