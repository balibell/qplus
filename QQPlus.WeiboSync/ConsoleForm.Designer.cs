namespace QQPlus.WeiboSync
{
    partial class ConsoleForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private Main main;


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
            this.ListBoxShow = new System.Windows.Forms.ListBox();
            this.FetchButton = new System.Windows.Forms.Button();
            this.ClearBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ListBoxShow
            // 
            this.ListBoxShow.FormattingEnabled = true;
            this.ListBoxShow.ItemHeight = 12;
            this.ListBoxShow.Location = new System.Drawing.Point(12, 69);
            this.ListBoxShow.Name = "ListBoxShow";
            this.ListBoxShow.Size = new System.Drawing.Size(1043, 400);
            this.ListBoxShow.TabIndex = 0;
            // 
            // FetchButton
            // 
            this.FetchButton.Location = new System.Drawing.Point(12, 37);
            this.FetchButton.Name = "FetchButton";
            this.FetchButton.Size = new System.Drawing.Size(75, 23);
            this.FetchButton.TabIndex = 1;
            this.FetchButton.Text = "获取微博";
            this.FetchButton.UseVisualStyleBackColor = true;
            this.FetchButton.Click += new System.EventHandler(this.FetchButton_Click);
            // 
            // ClearBtn
            // 
            this.ClearBtn.Location = new System.Drawing.Point(93, 37);
            this.ClearBtn.Name = "ClearBtn";
            this.ClearBtn.Size = new System.Drawing.Size(75, 23);
            this.ClearBtn.TabIndex = 2;
            this.ClearBtn.Text = "清除记录";
            this.ClearBtn.UseVisualStyleBackColor = true;
            this.ClearBtn.Click += new System.EventHandler(this.ClearBtn_Click);
            // 
            // ConsoleForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1067, 481);
            this.Controls.Add(this.ClearBtn);
            this.Controls.Add(this.FetchButton);
            this.Controls.Add(this.ListBoxShow);
            this.Name = "ConsoleForm";
            this.Text = "表单";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox ListBoxShow;
        private System.Windows.Forms.Button FetchButton;
        private System.Windows.Forms.Button ClearBtn;
    }
}