namespace AutoPatcherAdmin
{
    partial class AMain
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
            this.ClientTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.OutputTextBox = new System.Windows.Forms.TextBox();
            this.ProcessButton = new System.Windows.Forms.Button();
            this.AllowCleanCheckBox = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.DowloadTextBox = new System.Windows.Forms.TextBox();
            this.LogTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // ClientTextBox
            // 
            this.ClientTextBox.Location = new System.Drawing.Point(99, 33);
            this.ClientTextBox.Name = "ClientTextBox";
            this.ClientTextBox.Size = new System.Drawing.Size(254, 21);
            this.ClientTextBox.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "客户端路径:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(29, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "生成目录:";
            // 
            // OutputTextBox
            // 
            this.OutputTextBox.Location = new System.Drawing.Point(99, 57);
            this.OutputTextBox.Name = "OutputTextBox";
            this.OutputTextBox.Size = new System.Drawing.Size(254, 21);
            this.OutputTextBox.TabIndex = 3;
            // 
            // ProcessButton
            // 
            this.ProcessButton.Location = new System.Drawing.Point(483, 12);
            this.ProcessButton.Name = "ProcessButton";
            this.ProcessButton.Size = new System.Drawing.Size(75, 66);
            this.ProcessButton.TabIndex = 9;
            this.ProcessButton.Text = "生成";
            this.ProcessButton.UseVisualStyleBackColor = true;
            this.ProcessButton.Click += new System.EventHandler(this.ProcessButton_Click);
            // 
            // AllowCleanCheckBox
            // 
            this.AllowCleanCheckBox.AutoSize = true;
            this.AllowCleanCheckBox.Location = new System.Drawing.Point(369, 12);
            this.AllowCleanCheckBox.Name = "AllowCleanCheckBox";
            this.AllowCleanCheckBox.Size = new System.Drawing.Size(108, 16);
            this.AllowCleanCheckBox.TabIndex = 22;
            this.AllowCleanCheckBox.Text = "Allow Clean Up";
            this.AllowCleanCheckBox.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(22, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 12);
            this.label3.TabIndex = 24;
            this.label3.Text = "下载服务器:";
            // 
            // DowloadTextBox
            // 
            this.DowloadTextBox.Location = new System.Drawing.Point(99, 6);
            this.DowloadTextBox.Name = "DowloadTextBox";
            this.DowloadTextBox.Size = new System.Drawing.Size(254, 21);
            this.DowloadTextBox.TabIndex = 23;
            // 
            // LogTextBox
            // 
            this.LogTextBox.Location = new System.Drawing.Point(12, 104);
            this.LogTextBox.Multiline = true;
            this.LogTextBox.Name = "LogTextBox";
            this.LogTextBox.ReadOnly = true;
            this.LogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.LogTextBox.Size = new System.Drawing.Size(536, 287);
            this.LogTextBox.TabIndex = 25;
            // 
            // AMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(560, 403);
            this.Controls.Add(this.LogTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.DowloadTextBox);
            this.Controls.Add(this.AllowCleanCheckBox);
            this.Controls.Add(this.ProcessButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.OutputTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ClientTextBox);
            this.Name = "AMain";
            this.Text = "Auto Patcher Admin";
            this.Load += new System.EventHandler(this.AMain_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox ClientTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox OutputTextBox;
        private System.Windows.Forms.Button ProcessButton;
        private System.Windows.Forms.CheckBox AllowCleanCheckBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox DowloadTextBox;
        private System.Windows.Forms.TextBox LogTextBox;
    }
}

