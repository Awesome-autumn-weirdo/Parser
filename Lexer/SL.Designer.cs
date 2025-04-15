namespace Lexer
{
    partial class SL
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
            this.webBrowser7 = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // webBrowser7
            // 
            this.webBrowser7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser7.Location = new System.Drawing.Point(0, 0);
            this.webBrowser7.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser7.Name = "webBrowser7";
            this.webBrowser7.Size = new System.Drawing.Size(800, 450);
            this.webBrowser7.TabIndex = 0;
            // 
            // SL
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.webBrowser7);
            this.Name = "SL";
            this.Text = "SL";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser webBrowser7;
    }
}