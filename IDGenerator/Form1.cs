using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IDGenerator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                Pword.UseSystemPasswordChar = false;
            } else
            {
                Pword.UseSystemPasswordChar = true;
            }
        }

        private void CloseBTN_MouseEnter(object sender, EventArgs e)
        {
            CloseBTN.Image = Properties.Resources.Close_Hover;
        }

        private void CloseBTN_MouseLeave(object sender, EventArgs e)
        {
            CloseBTN.Image = Properties.Resources.Close_Stanby;
        }

        private void CloseBTN_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ResetPassword_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ResetPassword RPF = new ResetPassword();
            RPF.Show();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AdminUSER admin = new AdminUSER();
            admin.Show();
            this.Hide();
        }
    }
}
