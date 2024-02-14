namespace IDGenerator
{
    partial class StudentRegister
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
            this.VerBTN = new System.Windows.Forms.Button();
            this.Backtomainmenu = new System.Windows.Forms.Button();
            this.SID = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.Fname = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.Mname = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.Lname = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.Emailtxt = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.Cnum = new System.Windows.Forms.TextBox();
            this.ErrorMsg = new System.Windows.Forms.Label();
            this.SubBTN = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // VerBTN
            // 
            this.VerBTN.BackColor = System.Drawing.Color.Goldenrod;
            this.VerBTN.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.VerBTN.ForeColor = System.Drawing.Color.White;
            this.VerBTN.Location = new System.Drawing.Point(31, 530);
            this.VerBTN.Margin = new System.Windows.Forms.Padding(4);
            this.VerBTN.Name = "VerBTN";
            this.VerBTN.Size = new System.Drawing.Size(600, 78);
            this.VerBTN.TabIndex = 1;
            this.VerBTN.Text = "VERIFY EMAIL";
            this.VerBTN.UseVisualStyleBackColor = false;
            this.VerBTN.Click += new System.EventHandler(this.button1_Click);
            // 
            // Backtomainmenu
            // 
            this.Backtomainmenu.BackColor = System.Drawing.Color.Gray;
            this.Backtomainmenu.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Backtomainmenu.ForeColor = System.Drawing.Color.White;
            this.Backtomainmenu.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Backtomainmenu.Location = new System.Drawing.Point(6, 757);
            this.Backtomainmenu.Margin = new System.Windows.Forms.Padding(4);
            this.Backtomainmenu.Name = "Backtomainmenu";
            this.Backtomainmenu.Size = new System.Drawing.Size(110, 41);
            this.Backtomainmenu.TabIndex = 2;
            this.Backtomainmenu.Text = "BACK";
            this.Backtomainmenu.UseVisualStyleBackColor = false;
            this.Backtomainmenu.Click += new System.EventHandler(this.Backtomainmenu_Click);
            // 
            // SID
            // 
            this.SID.BackColor = System.Drawing.SystemColors.Window;
            this.SID.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.SID.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SID.ForeColor = System.Drawing.SystemColors.Desktop;
            this.SID.Location = new System.Drawing.Point(276, 140);
            this.SID.Margin = new System.Windows.Forms.Padding(4);
            this.SID.MaxLength = 10;
            this.SID.Name = "SID";
            this.SID.Size = new System.Drawing.Size(317, 38);
            this.SID.TabIndex = 3;
            this.SID.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.SID_KeyPress);
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("Arial Narrow", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Goldenrod;
            this.label2.Location = new System.Drawing.Point(53, 145);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(198, 28);
            this.label2.TabIndex = 4;
            this.label2.Text = "STUDENT ID :";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("Arial Narrow", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.DarkGreen;
            this.label3.Location = new System.Drawing.Point(57, 201);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(194, 35);
            this.label3.TabIndex = 9;
            this.label3.Text = "FIRST NAME :";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // Fname
            // 
            this.Fname.BackColor = System.Drawing.SystemColors.Window;
            this.Fname.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Fname.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.Fname.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Fname.ForeColor = System.Drawing.SystemColors.Desktop;
            this.Fname.Location = new System.Drawing.Point(276, 199);
            this.Fname.Margin = new System.Windows.Forms.Padding(4);
            this.Fname.Name = "Fname";
            this.Fname.Size = new System.Drawing.Size(317, 38);
            this.Fname.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Font = new System.Drawing.Font("Arial Narrow", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.DarkGreen;
            this.label4.Location = new System.Drawing.Point(24, 251);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(227, 50);
            this.label4.TabIndex = 11;
            this.label4.Text = "MIDDLE NAME :";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Mname
            // 
            this.Mname.BackColor = System.Drawing.SystemColors.Window;
            this.Mname.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Mname.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.Mname.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Mname.ForeColor = System.Drawing.SystemColors.Desktop;
            this.Mname.Location = new System.Drawing.Point(276, 257);
            this.Mname.Margin = new System.Windows.Forms.Padding(4);
            this.Mname.Name = "Mname";
            this.Mname.Size = new System.Drawing.Size(317, 38);
            this.Mname.TabIndex = 10;
            // 
            // label5
            // 
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.Font = new System.Drawing.Font("Arial Narrow", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.DarkGreen;
            this.label5.Location = new System.Drawing.Point(57, 319);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(194, 35);
            this.label5.TabIndex = 13;
            this.label5.Text = "LAST NAME :";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Lname
            // 
            this.Lname.BackColor = System.Drawing.SystemColors.Window;
            this.Lname.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Lname.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.Lname.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Lname.ForeColor = System.Drawing.SystemColors.Desktop;
            this.Lname.Location = new System.Drawing.Point(276, 317);
            this.Lname.Margin = new System.Windows.Forms.Padding(4);
            this.Lname.Name = "Lname";
            this.Lname.Size = new System.Drawing.Size(317, 38);
            this.Lname.TabIndex = 12;
            // 
            // label6
            // 
            this.label6.BackColor = System.Drawing.Color.Transparent;
            this.label6.Font = new System.Drawing.Font("Arial Narrow", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.DarkGreen;
            this.label6.Location = new System.Drawing.Point(6, 379);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(245, 39);
            this.label6.TabIndex = 15;
            this.label6.Text = "EMAIL ADDRESS :";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Emailtxt
            // 
            this.Emailtxt.BackColor = System.Drawing.SystemColors.Window;
            this.Emailtxt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Emailtxt.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Emailtxt.ForeColor = System.Drawing.SystemColors.Desktop;
            this.Emailtxt.Location = new System.Drawing.Point(276, 379);
            this.Emailtxt.Margin = new System.Windows.Forms.Padding(4);
            this.Emailtxt.Name = "Emailtxt";
            this.Emailtxt.Size = new System.Drawing.Size(317, 38);
            this.Emailtxt.TabIndex = 14;
            this.Emailtxt.TextChanged += new System.EventHandler(this.Emailtxt_TextChanged);
            // 
            // label7
            // 
            this.label7.BackColor = System.Drawing.Color.Transparent;
            this.label7.Font = new System.Drawing.Font("Arial Narrow", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.DarkGreen;
            this.label7.Location = new System.Drawing.Point(-8, 433);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(259, 49);
            this.label7.TabIndex = 17;
            this.label7.Text = "CONTACT NUMBER :";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Cnum
            // 
            this.Cnum.BackColor = System.Drawing.SystemColors.Window;
            this.Cnum.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Cnum.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Cnum.ForeColor = System.Drawing.SystemColors.Desktop;
            this.Cnum.Location = new System.Drawing.Point(276, 438);
            this.Cnum.Margin = new System.Windows.Forms.Padding(4);
            this.Cnum.Name = "Cnum";
            this.Cnum.Size = new System.Drawing.Size(317, 38);
            this.Cnum.TabIndex = 16;
            this.Cnum.TextChanged += new System.EventHandler(this.Cnum_TextChanged);
            this.Cnum.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Cnum_KeyPress);
            // 
            // ErrorMsg
            // 
            this.ErrorMsg.BackColor = System.Drawing.Color.Transparent;
            this.ErrorMsg.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ErrorMsg.ForeColor = System.Drawing.Color.Red;
            this.ErrorMsg.Location = new System.Drawing.Point(-75, 86);
            this.ErrorMsg.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.ErrorMsg.Name = "ErrorMsg";
            this.ErrorMsg.Size = new System.Drawing.Size(840, 50);
            this.ErrorMsg.TabIndex = 18;
            this.ErrorMsg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // SubBTN
            // 
            this.SubBTN.BackColor = System.Drawing.Color.DarkGreen;
            this.SubBTN.Enabled = false;
            this.SubBTN.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SubBTN.ForeColor = System.Drawing.Color.Gold;
            this.SubBTN.Location = new System.Drawing.Point(31, 628);
            this.SubBTN.Margin = new System.Windows.Forms.Padding(4);
            this.SubBTN.Name = "SubBTN";
            this.SubBTN.Size = new System.Drawing.Size(600, 83);
            this.SubBTN.TabIndex = 19;
            this.SubBTN.Text = "SUBMIT";
            this.SubBTN.UseVisualStyleBackColor = false;
            this.SubBTN.Click += new System.EventHandler(this.SubBTN_Click);
            // 
            // panel1
            // 
            this.panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(660, 74);
            this.panel1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Green;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("Arial Narrow", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Gold;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(660, 74);
            this.label1.TabIndex = 1;
            this.label1.Text = "ACCOUNT REGISTRATION";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // StudentRegister
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BackgroundImage = global::IDGenerator.Properties.Resources.bg_pic_login;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(660, 811);
            this.Controls.Add(this.SubBTN);
            this.Controls.Add(this.ErrorMsg);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.Cnum);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.Emailtxt);
            this.Controls.Add(this.Lname);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.Mname);
            this.Controls.Add(this.Fname);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.SID);
            this.Controls.Add(this.Backtomainmenu);
            this.Controls.Add(this.VerBTN);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label5);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "StudentRegister";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "StudentRegister";
            this.Load += new System.EventHandler(this.StudentRegister_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Backtomainmenu;
        private System.Windows.Forms.TextBox SID;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox Mname;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox Lname;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox Cnum;
        private System.Windows.Forms.Label ErrorMsg;
        public System.Windows.Forms.TextBox Fname;
        public System.Windows.Forms.Button SubBTN;
        public System.Windows.Forms.Button VerBTN;
        public System.Windows.Forms.TextBox Emailtxt;
    }
}