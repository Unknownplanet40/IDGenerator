using System;
using System.Windows.Forms;

namespace IDGenerator
{
    public partial class VerificationCode : Form
    {
        private StudentRegister studentRegister; // Field to hold the reference to StudentRegister
        private DateTime currentTime;
        private DateTime codeExpiration;

        public VerificationCode(StudentRegister studentRegister)
        {
            InitializeComponent();
            this.studentRegister = studentRegister;

            currentTime = DateTime.Now;
            codeExpiration = currentTime.AddMinutes(5);
            CodeExpire.Text = "Your code will Expire in " + codeExpiration.ToString("hh:mm tt");
           
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (String.IsNullOrWhiteSpace(tb1.Text) && String.IsNullOrWhiteSpace(tb2.Text) && String.IsNullOrWhiteSpace(tb3.Text) && String.IsNullOrWhiteSpace(tb4.Text) && String.IsNullOrWhiteSpace(tb5.Text) && String.IsNullOrWhiteSpace(tb6.Text))
            {
                MessageBox.Show("Please Fill all Fields", "Empty Fields", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string VCode = tb1.Text + tb2.Text + tb3.Text + tb4.Text + tb5.Text + tb6.Text;
            string Code = PassCodeHere.Text;
            if (Code != VCode) // code not match
            {
                MessageBox.Show("You Inputted a Wrong Verification Code", "Verification", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (DateTime.Now >= codeExpiration) // Expired code
            {
                MessageBox.Show("Your Verification Code Expired", "Time's up!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Close();
            }
            else // matched
            {
                MessageBox.Show("Verification Match!", "Verified Successfully", MessageBoxButtons.OK, MessageBoxIcon.Information);
                studentRegister.EnableSubBTN(); // Call the method on the existing instance
                studentRegister.VerBTN.Enabled = false;
                Close();
            }

        }
    }
}
