namespace AutoPatcherAdmin
{
    partial class SMain
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
            this.TextBoxLastVersion = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.Output = new System.Windows.Forms.Label();
            this.TextBoxOutput = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.TextBoxCurrent = new System.Windows.Forms.TextBox();
            this.ProcessButton = new System.Windows.Forms.Button();
            this.LogTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // TextBoxLastVersion
            // 
            this.TextBoxLastVersion.AllowDrop = true;
            this.TextBoxLastVersion.Location = new System.Drawing.Point(108, 19);
            this.TextBoxLastVersion.Name = "TextBoxLastVersion";
            this.TextBoxLastVersion.Size = new System.Drawing.Size(546, 21);
            this.TextBoxLastVersion.TabIndex = 24;
            this.TextBoxLastVersion.DragDrop += new System.Windows.Forms.DragEventHandler(this.TextBoxLastVersion_DragDrop);
            this.TextBoxLastVersion.DragEnter += new System.Windows.Forms.DragEventHandler(this.TextBoxLastVersion_DragEnter);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(31, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 12);
            this.label3.TabIndex = 29;
            this.label3.Text = "上次版本:";
            // 
            // Output
            // 
            this.Output.AutoSize = true;
            this.Output.Location = new System.Drawing.Point(31, 73);
            this.Output.Name = "Output";
            this.Output.Size = new System.Drawing.Size(59, 12);
            this.Output.TabIndex = 28;
            this.Output.Text = "生成目录:";
            // 
            // TextBoxOutput
            // 
            this.TextBoxOutput.Location = new System.Drawing.Point(108, 70);
            this.TextBoxOutput.Name = "TextBoxOutput";
            this.TextBoxOutput.Size = new System.Drawing.Size(546, 21);
            this.TextBoxOutput.TabIndex = 27;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(31, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 26;
            this.label1.Text = "当前版本:";
            // 
            // TextBoxCurrent
            // 
            this.TextBoxCurrent.AllowDrop = true;
            this.TextBoxCurrent.Location = new System.Drawing.Point(108, 46);
            this.TextBoxCurrent.Name = "TextBoxCurrent";
            this.TextBoxCurrent.Size = new System.Drawing.Size(546, 21);
            this.TextBoxCurrent.TabIndex = 25;
            this.TextBoxCurrent.DragDrop += new System.Windows.Forms.DragEventHandler(this.TextBoxCurrent_DragDrop);
            this.TextBoxCurrent.DragEnter += new System.Windows.Forms.DragEventHandler(this.TextBoxCurrent_DragEnter);
            // 
            // ProcessButton
            // 
            this.ProcessButton.Location = new System.Drawing.Point(685, 19);
            this.ProcessButton.Name = "ProcessButton";
            this.ProcessButton.Size = new System.Drawing.Size(125, 32);
            this.ProcessButton.TabIndex = 30;
            this.ProcessButton.Text = "生成";
            this.ProcessButton.UseVisualStyleBackColor = true;
            this.ProcessButton.Click += new System.EventHandler(this.ProcessButton_Click);
            // 
            // LogTextBox
            // 
            this.LogTextBox.Location = new System.Drawing.Point(33, 120);
            this.LogTextBox.Multiline = true;
            this.LogTextBox.Name = "LogTextBox";
            this.LogTextBox.ReadOnly = true;
            this.LogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.LogTextBox.Size = new System.Drawing.Size(777, 287);
            this.LogTextBox.TabIndex = 31;
            // 
            // SMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(856, 446);
            this.Controls.Add(this.LogTextBox);
            this.Controls.Add(this.ProcessButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Output);
            this.Controls.Add(this.TextBoxOutput);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TextBoxCurrent);
            this.Controls.Add(this.TextBoxLastVersion);
            this.Name = "SMain";
            this.Text = "SMain";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TextBoxLastVersion;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label Output;
        private System.Windows.Forms.TextBox TextBoxOutput;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TextBoxCurrent;
        private System.Windows.Forms.Button ProcessButton;
        private System.Windows.Forms.TextBox LogTextBox;
    }
}