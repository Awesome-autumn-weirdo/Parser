namespace Lexer
{
    partial class IK
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
            this.richTextBoxCode = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // richTextBoxCode
            // 
            this.richTextBoxCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxCode.Location = new System.Drawing.Point(0, 0);
            this.richTextBoxCode.Name = "richTextBoxCode";
            this.richTextBoxCode.Size = new System.Drawing.Size(800, 450);
            this.richTextBoxCode.TabIndex = 0;
            this.richTextBoxCode.Text = "";
            // 
            // IK
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.richTextBoxCode);
            this.Name = "IK";
            this.Text = "IK";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBoxCode;
    }
}