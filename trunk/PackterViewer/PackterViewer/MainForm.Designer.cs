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
            this.vertexColor3 = new System.Windows.Forms.ComboBox();
            this.vertexColor2 = new System.Windows.Forms.ComboBox();
            this.vertexColor1 = new System.Windows.Forms.ComboBox();
            this.basePanel = new System.Windows.Forms.Panel();
            this.webBrowser = new System.Windows.Forms.WebBrowser();
            this.spinningTriangleControl = new WinFormsGraphicsDevice.SpinningTriangleControl();
            this.packterDisplayControl = new Packter_viewer2.PackterDisplayControl();
            this.basePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // vertexColor3
            // 
            this.vertexColor3.DropDownHeight = 500;
            this.vertexColor3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.vertexColor3.FormattingEnabled = true;
            this.vertexColor3.IntegralHeight = false;
            this.vertexColor3.Items.AddRange(new object[] {
            "BurlyWood",
            "Chartreuse",
            "Coral",
            "CornflowerBlue",
            "Cornsilk",
            "Firebrick",
            "Fuchsia",
            "Goldenrod",
            "Indigo",
            "Tan",
            "Teal",
            "Thistle",
            "Tomato"});
            this.vertexColor3.Location = new System.Drawing.Point(446, 12);
            this.vertexColor3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.vertexColor3.Name = "vertexColor3";
            this.vertexColor3.Size = new System.Drawing.Size(136, 23);
            this.vertexColor3.TabIndex = 3;
            this.vertexColor3.SelectedIndexChanged += new System.EventHandler(this.vertexColor_SelectedIndexChanged);
            // 
            // vertexColor2
            // 
            this.vertexColor2.DropDownHeight = 500;
            this.vertexColor2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.vertexColor2.FormattingEnabled = true;
            this.vertexColor2.IntegralHeight = false;
            this.vertexColor2.Items.AddRange(new object[] {
            "BurlyWood",
            "Chartreuse",
            "Coral",
            "CornflowerBlue",
            "Cornsilk",
            "Firebrick",
            "Fuchsia",
            "Goldenrod",
            "Indigo",
            "Tan",
            "Teal",
            "Thistle",
            "Tomato"});
            this.vertexColor2.Location = new System.Drawing.Point(239, 12);
            this.vertexColor2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.vertexColor2.Name = "vertexColor2";
            this.vertexColor2.Size = new System.Drawing.Size(136, 23);
            this.vertexColor2.TabIndex = 2;
            this.vertexColor2.SelectedIndexChanged += new System.EventHandler(this.vertexColor_SelectedIndexChanged);
            // 
            // vertexColor1
            // 
            this.vertexColor1.DropDownHeight = 500;
            this.vertexColor1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.vertexColor1.FormattingEnabled = true;
            this.vertexColor1.IntegralHeight = false;
            this.vertexColor1.Items.AddRange(new object[] {
            "BurlyWood",
            "Chartreuse",
            "Coral",
            "CornflowerBlue",
            "Cornsilk",
            "Firebrick",
            "Fuchsia",
            "Goldenrod",
            "Indigo",
            "Tan",
            "Teal",
            "Thistle",
            "Tomato"});
            this.vertexColor1.Location = new System.Drawing.Point(13, 12);
            this.vertexColor1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.vertexColor1.Name = "vertexColor1";
            this.vertexColor1.Size = new System.Drawing.Size(136, 23);
            this.vertexColor1.TabIndex = 1;
            this.vertexColor1.SelectedIndexChanged += new System.EventHandler(this.vertexColor_SelectedIndexChanged);
            // 
            // basePanel
            // 
            this.basePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.basePanel.Controls.Add(this.webBrowser);
            this.basePanel.Controls.Add(this.vertexColor3);
            this.basePanel.Controls.Add(this.vertexColor1);
            this.basePanel.Controls.Add(this.vertexColor2);
            this.basePanel.Controls.Add(this.spinningTriangleControl);
            this.basePanel.Controls.Add(this.packterDisplayControl);
            this.basePanel.Location = new System.Drawing.Point(0, 0);
            this.basePanel.Name = "basePanel";
            this.basePanel.Size = new System.Drawing.Size(841, 605);
            this.basePanel.TabIndex = 0;
            // 
            // webBrowser
            // 
            this.webBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser.Location = new System.Drawing.Point(13, 401);
            this.webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser.Name = "webBrowser";
            this.webBrowser.ScrollBarsEnabled = false;
            this.webBrowser.Size = new System.Drawing.Size(814, 190);
            this.webBrowser.TabIndex = 5;
            this.webBrowser.Visible = false;
            // 
            // spinningTriangleControl
            // 
            this.spinningTriangleControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.spinningTriangleControl.Location = new System.Drawing.Point(657, 3);
            this.spinningTriangleControl.Name = "spinningTriangleControl";
            this.spinningTriangleControl.Size = new System.Drawing.Size(170, 163);
            this.spinningTriangleControl.TabIndex = 4;
            this.spinningTriangleControl.Text = "spinningTriangleControl1";
            // 
            // packterDisplayControl
            // 
            this.packterDisplayControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.packterDisplayControl.Location = new System.Drawing.Point(0, 0);
            this.packterDisplayControl.Name = "packterDisplayControl";
            this.packterDisplayControl.Size = new System.Drawing.Size(841, 605);
            this.packterDisplayControl.TabIndex = 6;
            this.packterDisplayControl.Text = "packterDisplayControl1";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(839, 603);
            this.Controls.Add(this.basePanel);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "MainForm";
            this.Text = "Packter_viewer";
            this.basePanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox vertexColor1;
        private System.Windows.Forms.ComboBox vertexColor3;
        private System.Windows.Forms.ComboBox vertexColor2;
        private System.Windows.Forms.Panel basePanel;
        private SpinningTriangleControl spinningTriangleControl;
        private System.Windows.Forms.WebBrowser webBrowser;
        private Packter_viewer2.PackterDisplayControl packterDisplayControl;
    }
}

