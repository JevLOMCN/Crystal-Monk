namespace AutoPatcherAdmin
{
    partial class StartMain
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
            this.ButtonClient = new System.Windows.Forms.Button();
            this.ButtonServer = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ButtonClient
            // 
            this.ButtonClient.Location = new System.Drawing.Point(84, 47);
            this.ButtonClient.Name = "ButtonClient";
            this.ButtonClient.Size = new System.Drawing.Size(92, 56);
            this.ButtonClient.TabIndex = 0;
            this.ButtonClient.Text = "客户端打包";
            this.ButtonClient.UseVisualStyleBackColor = true;
            this.ButtonClient.Click += new System.EventHandler(this.ButtonClient_Click);
            // 
            // ButtonServer
            // 
            this.ButtonServer.Location = new System.Drawing.Point(84, 151);
            this.ButtonServer.Name = "ButtonServer";
            this.ButtonServer.Size = new System.Drawing.Size(92, 56);
            this.ButtonServer.TabIndex = 1;
            this.ButtonServer.Text = "服务器打包";
            this.ButtonServer.UseVisualStyleBackColor = true;
            this.ButtonServer.Click += new System.EventHandler(this.ButtonServer_Click);
            // 
            // StartMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.ButtonServer);
            this.Controls.Add(this.ButtonClient);
            this.Name = "StartMain";
            this.Text = "StartMain";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button ButtonClient;
        private System.Windows.Forms.Button ButtonServer;
    }
}