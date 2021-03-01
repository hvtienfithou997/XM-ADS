
namespace JobokoService
{
    partial class Form1
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
            this.btnSearchLog = new System.Windows.Forms.Button();
            this.txtTerm = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.rtbLog = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // btnSearchLog
            // 
            this.btnSearchLog.Location = new System.Drawing.Point(330, 30);
            this.btnSearchLog.Name = "btnSearchLog";
            this.btnSearchLog.Size = new System.Drawing.Size(75, 23);
            this.btnSearchLog.TabIndex = 0;
            this.btnSearchLog.Text = "Tra log";
            this.btnSearchLog.UseVisualStyleBackColor = true;
            this.btnSearchLog.Click += new System.EventHandler(this.btnSearchLog_Click);
            // 
            // txtTerm
            // 
            this.txtTerm.Location = new System.Drawing.Point(74, 32);
            this.txtTerm.Name = "txtTerm";
            this.txtTerm.Size = new System.Drawing.Size(188, 20);
            this.txtTerm.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Từ khóa";
            // 
            // rtbLog
            // 
            this.rtbLog.Location = new System.Drawing.Point(15, 129);
            this.rtbLog.Name = "rtbLog";
            this.rtbLog.Size = new System.Drawing.Size(487, 190);
            this.rtbLog.TabIndex = 3;
            this.rtbLog.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(524, 450);
            this.Controls.Add(this.rtbLog);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtTerm);
            this.Controls.Add(this.btnSearchLog);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSearchLog;
        private System.Windows.Forms.TextBox txtTerm;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox rtbLog;
    }
}