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
            this.packterDisplayControl = new Packter_viewer2.PackterDisplayControl();
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
            this.basePanel.Margin = new System.Windows.Forms.Padding(2);
            this.basePanel.Name = "basePanel";
            this.basePanel.Size = new System.Drawing.Size(631, 484);
            this.basePanel.TabIndex = 0;
            // 
            // webBrowser
            // 
            this.webBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser.Location = new System.Drawing.Point(11, 325);
            this.webBrowser.Margin = new System.Windows.Forms.Padding(2);
            this.webBrowser.MinimumSize = new System.Drawing.Size(15, 16);
            this.webBrowser.Name = "webBrowser";
            this.webBrowser.ScrollBarsEnabled = false;
            this.webBrowser.Size = new System.Drawing.Size(607, 150);
            this.webBrowser.TabIndex = 5;
            this.webBrowser.Visible = false;
            // 
            // packterDisplayControl
            // 
            this.packterDisplayControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.packterDisplayControl.Location = new System.Drawing.Point(0, 0);
            this.packterDisplayControl.Margin = new System.Windows.Forms.Padding(2);
            this.packterDisplayControl.Name = "packterDisplayControl";
            this.packterDisplayControl.Size = new System.Drawing.Size(631, 484);
            this.packterDisplayControl.TabIndex = 6;
            this.packterDisplayControl.Text = "packterDisplayControl1";
            this.packterDisplayControl.KeyUp += new System.Windows.Forms.KeyEventHandler(this.packterDisplayControl_KeyUp);
            this.packterDisplayControl.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.packterDisplayControl_KeyPress);
            this.packterDisplayControl.KeyDown += new System.Windows.Forms.KeyEventHandler(this.packterDisplayControl_KeyDown);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(629, 482);
            this.Controls.Add(this.basePanel);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "MainForm";
            this.Text = "Packter_viewer";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.basePanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel basePanel;
        private System.Windows.Forms.WebBrowser webBrowser;
        private Packter_viewer2.PackterDisplayControl packterDisplayControl;
    }
}

