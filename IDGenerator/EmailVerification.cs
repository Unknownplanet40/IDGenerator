using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Windows.Forms;
using System.Data.SqlClient;
using Timer = System.Threading.Timer;
using System.Text;

namespace IDGenerator
{
    public partial class EmailVerification : Form
    {
        public EmailVerification()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (mail.Text != textBox1.Text)
            {
                MessageBox.Show("You Input an Invalid Code Please try again!", "Invalid Code", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } else if (String.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Please Enter a Verification First", "NO Code Recieve!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } else
            {
                StudentUSER SU = new StudentUSER();
                SU.Show();
                this.Close();
            }
        }

        private void EmailVerification_Load(object sender, EventArgs e)
        {
            
        }
    }
}
