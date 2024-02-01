namespace IDGenerator
{
    partial class PanelPlayground
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.greetings_uname = new System.Windows.Forms.Label();
            this.datelogin = new System.Windows.Forms.Label();
            this.panel5 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panel5);
            this.panel1.Controls.Add(this.datelogin);
            this.panel1.Controls.Add(this.greetings_uname);
            this.panel1.Controls.Add(this.panel4);
            this.panel1.Controls.Add(this.panel3);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(800, 490);
            this.panel1.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(794, 30);
            this.panel2.TabIndex = 0;
            // 
            // panel3
            // 
            this.panel3.Location = new System.Drawing.Point(3, 39);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(30, 448);
            this.panel3.TabIndex = 1;
            // 
            // panel4
            // 
            this.panel4.Location = new System.Drawing.Point(767, 39);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(30, 448);
            this.panel4.TabIndex = 2;
            // 
            // greetings_uname
            // 
            this.greetings_uname.AutoSize = true;
            this.greetings_uname.Font = new System.Drawing.Font("MS Reference Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.greetings_uname.Location = new System.Drawing.Point(39, 39);
            this.greetings_uname.Name = "greetings_uname";
            this.greetings_uname.Size = new System.Drawing.Size(138, 19);
            this.greetings_uname.TabIndex = 3;
            this.greetings_uname.Text = "Hi [Username]!";
            // 
            // datelogin
            // 
            this.datelogin.AutoSize = true;
            this.datelogin.Font = new System.Drawing.Font("MS Reference Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.datelogin.Location = new System.Drawing.Point(40, 58);
            this.datelogin.Name = "datelogin";
            this.datelogin.Size = new System.Drawing.Size(66, 15);
            this.datelogin.TabIndex = 4;
            this.datelogin.Text = "Last login:";
            // 
            // panel5
            // 
            this.panel5.Location = new System.Drawing.Point(39, 76);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(722, 127);
            this.panel5.TabIndex = 5;
            // 
            // PanelPlayground
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1263, 606);
            this.Controls.Add(this.panel1);
            this.Name = "PanelPlayground";
            this.Text = "PanelPlayground";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label greetings_uname;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Label datelogin;
    }
}