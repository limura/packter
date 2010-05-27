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
#endregion

using Packter_viewer2;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

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

        public MainForm()
        {
            InitializeComponent();

            vertexColor1.SelectedIndex = 1;
            vertexColor2.SelectedIndex = 2;
            vertexColor3.SelectedIndex = 4;

            contentBuilder = new ContentBuilder();
            contentManager = new ContentManager(packterDisplayControl.Services, contentBuilder.OutputDirectory);
            packterDisplayControl.RegisterContentBuilder(contentBuilder, contentManager);

            webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser_DocumentCompleted);
            webBrowser.Navigate("http://www.google.co.jp");
        }

        string webBrowserTargetText = "dummy";
        void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (webBrowser.DocumentText != webBrowserTargetText)
            {
                System.Uri imgUri = new System.Uri(System.IO.Directory.GetCurrentDirectory() + "../../../../Content/teacher.png");
                webBrowser.DocumentText
                    = webBrowserTargetText
                    = "<html><body bgcolor=lightgreen><img height=100% src=\"" + imgUri.ToString() + "\">çUåÇÇåüímÇµÇ‹ÇµÇΩÅB</body></html>";
            }
        }


        /// <summary>
        /// Event handler updates the spinning triangle control when
        /// one of the three vertex color combo boxes is altered.
        /// </summary>
        void vertexColor_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            // Which vertex was changed?
            int vertexIndex;

            if (sender == vertexColor1)
                vertexIndex = 0;
            else if (sender == vertexColor2)
                vertexIndex = 1;
            else if (sender == vertexColor3)
                vertexIndex = 2;
            else
                return;

            // Which color was selected?
            ComboBox combo = (ComboBox)sender;

            string colorName = combo.SelectedItem.ToString();

            GdiColor gdiColor = GdiColor.FromName(colorName);

            XnaColor xnaColor = new XnaColor(gdiColor.R, gdiColor.G, gdiColor.B);

            // Update the spinning triangle control with the new color.
            spinningTriangleControl.Vertices[vertexIndex].Color = xnaColor;
        }

#if false
        private void packterDisplayControl_KeyDown(object sender, KeyEventArgs e)
        {
            packterDisplayControl.KeyDown(e.KeyCode);
        }

        private void packterDisplayControl_KeyUp(object sender, KeyEventArgs e)
        {
            packterDisplayControl.KeyUp(e.KeyCode);
        }
#endif
    }
}
