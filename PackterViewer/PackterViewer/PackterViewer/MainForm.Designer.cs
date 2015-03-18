namespace WinFormsGraphicsDevice
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.basePanel = new System.Windows.Forms.Panel();
            this.webBrowser = new System.Windows.Forms.WebBrowser();
            this.packterDisplayControl = new PackterViewer.PackterDisplayControl();
            this.basePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // basePanel
            // 
            this.basePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.basePanel.Controls.Add(this.webBrowser);
            this.basePanel.Controls.Add(this.packterDisplayControl);
            this.basePanel.Location = new System.Drawing.Point(0, 0);
            this.basePanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.basePanel.Name = "basePanel";
            this.basePanel.Size = new System.Drawing.Size(841, 605);
            this.basePanel.TabIndex = 0;
            // 
            // webBrowser
            // 
            this.webBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser.Location = new System.Drawing.Point(0, 406);
            this.webBrowser.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser.Name = "webBrowser";
            this.webBrowser.ScrollBarsEnabled = false;
            this.webBrowser.Size = new System.Drawing.Size(839, 196);
            this.webBrowser.TabIndex = 5;
            this.webBrowser.Visible = false;
            // 
            // packterDisplayControl
            // 
            this.packterDisplayControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.packterDisplayControl.Location = new System.Drawing.Point(0, 0);
            this.packterDisplayControl.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.packterDisplayControl.Name = "packterDisplayControl";
            this.packterDisplayControl.Size = new System.Drawing.Size(841, 605);
            this.packterDisplayControl.StatusDraw = false;
            this.packterDisplayControl.TabIndex = 6;
            this.packterDisplayControl.Text = "packterDisplayControl1";
            this.packterDisplayControl.KeyUp += new System.Windows.Forms.KeyEventHandler(this.packterDisplayControl_KeyUp);
            this.packterDisplayControl.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.packterDisplayControl_KeyPress);
            this.packterDisplayControl.KeyDown += new System.Windows.Forms.KeyEventHandler(this.packterDisplayControl_KeyDown);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(839, 602);
            this.Controls.Add(this.basePanel);
            this.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.Name = "MainForm";
            this.Text = "PackterViewer (http://www.packter.net)";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.basePanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel basePanel;
        private System.Windows.Forms.WebBrowser webBrowser;
        private PackterViewer.PackterDisplayControl packterDisplayControl;
    }
}

