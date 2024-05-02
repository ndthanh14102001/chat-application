using System.Windows.Forms;
namespace Server
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.IP = new System.Windows.Forms.TextBox();
            this.Start = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.PORT = new System.Windows.Forms.TextBox();
            this.KQ = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(41, 21);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(21, 20);
            this.label1.TabIndex = 6;
            this.label1.Text = "IP";
            // 
            // IP
            // 
            this.IP.Location = new System.Drawing.Point(86, 21);
            this.IP.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.IP.Name = "IP";
            this.IP.Size = new System.Drawing.Size(275, 27);
            this.IP.TabIndex = 0;
            // 
            // Start
            // 
            this.Start.Location = new System.Drawing.Point(551, 19);
            this.Start.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Start.Name = "Start";
            this.Start.Size = new System.Drawing.Size(90, 28);
            this.Start.TabIndex = 2;
            this.Start.Text = "Start";
            this.Start.UseVisualStyleBackColor = true;
            this.Start.Click += new System.EventHandler(this.Start_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(398, 19);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 20);
            this.label2.TabIndex = 3;
            this.label2.Text = "Port";
            // 
            // PORT
            // 
            this.PORT.Location = new System.Drawing.Point(442, 19);
            this.PORT.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.PORT.Name = "PORT";
            this.PORT.Size = new System.Drawing.Size(46, 27);
            this.PORT.TabIndex = 1;
            this.PORT.Text = "2008";
            // 
            // KQ
            // 
            this.KQ.Location = new System.Drawing.Point(86, 81);
            this.KQ.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.KQ.Multiline = true;
            this.KQ.Name = "KQ";
            this.KQ.Size = new System.Drawing.Size(556, 120);
            this.KQ.TabIndex = 5;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(685, 263);
            this.Controls.Add(this.Start);
            this.Controls.Add(this.PORT);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.KQ);
            this.Controls.Add(this.IP);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label label1;
        private TextBox IP;
        private Button Start;
        private Label label2;
        private TextBox PORT;
        private TextBox KQ;
    }
}