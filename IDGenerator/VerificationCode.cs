using System;
using System.Windows.Forms;

namespace IDGenerator
{
    public partial class VerificationCode : Form
    {
        private StudentRegister studentRegister; // Field to hold the reference to StudentRegister

        public VerificationCode(StudentRegister studentRegister)
        {
            InitializeComponent();
            this.studentRegister = studentRegister;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(tb1.Text) && String.IsNullOrWhiteSpace(tb2.Text) && String.IsNullOrWhiteSpace(tb3.Text) && String.IsNullOrWhiteSpace(tb4.Text) && String.IsNullOrWhiteSpace(tb5.Text) && String.IsNullOrWhiteSpace(tb6.Text))
            {
                MessageBox.Show("Please Fill all Fields", "Empty Fields", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                string VCode = tb1.Text + tb2.Text + tb3.Text + tb4.Text + tb5.Text + tb6.Text;
                string Code = PassCodeHere.Text;
                if (Code != VCode)
                {
                    MessageBox.Show("You Inputted a Wrong Verification Code", "Verification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Verification Match!", "Verified Successfully", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    studentRegister.EnableSubBTN(); // Call the method on the existing instance
                    studentRegister.VerBTN.Enabled = false;
                    Close();
                }
            }
        }
    }
}
