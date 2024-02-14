using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Windows.Forms;
using System.Data.SqlClient;
using Timer = System.Threading.Timer;

namespace IDGenerator
{
    public partial class ResetPassword : Form
    {
        public ResetPassword()
        {
            InitializeComponent();
        }
        private string email = String.Empty;
        private string name = String.Empty;
        private string ID = String.Empty;

        string conString = IDGenerator.Properties.Settings.IDGeneratorCONSTRING.ConnectionString;
        string smtpEmail = "Your email here";
        string smtpPass = "Your email password here";

        static string GenerateVerificationCode()
        {
            Random random = new Random();
            int code = random.Next(100000, 999999); // Generates a random 6-digit code

            return code.ToString();
        }

        string VerificationCode = GenerateVerificationCode();

        private void CloseBTN_Click(object sender, EventArgs e)
        {
            LOGIN F1 = new LOGIN();
            F1.Show();
            this.Close();
        }

        private void code_Click(object sender, EventArgs e)
        {
            ID = SID.Text;
            using (SqlConnection conn = new SqlConnection(conString))
            {
                try {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT mail, Firstname FROM Student_Info WHERE StudentID = " + ID, conn);
                SqlDataReader dataReader = cmd.ExecuteReader();

                if (dataReader.HasRows)
                {

                    panel1.Visible = false;
                    panel3.Visible = true;

                    MessageBox.Show("We have sent a verification code to your registered email address to facilitate the password reset process. Please check your inbox and enter the code on the reset page. ", "Verification Code Sent!", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    while (dataReader.Read())
                    {
                        email = Convert.ToString(dataReader["mail"]);
                        name = Convert.ToString(dataReader["Firstname"]);
                    }
                    conn.Close();


                    string fromMail = smtpEmail;
                    string frompass = smtpPass;

                    MailMessage message = new MailMessage();
                    message.From = new MailAddress(fromMail);
                    message.Subject = "Password Reset Verification Code";
                    message.To.Add(new MailAddress(email));
                    message.Body = "<html><body><p>Dear " + name + "<br>" +
                    "<p>We received a request to reset the password associated with your account. If you did not make this request, please ignore this email." +
                    "To reset your password, please use the following verification code:</p>" +
                    "<p><strong>Verification Code:</strong> " + VerificationCode +
                    "<br>" +
                    "<p>Please note that this verification code are valid for a limited time.</p>" +
                    "<br>" +
                    "<p>If you didn't made this request, please disregard this email.</p>" +
                    "<br>" +
                    "<p>Best regards,<br>Administrator</p>" +
                    "</body>" +
                    "</html>";
                    message.IsBodyHtml = true;

                    var smtpClient = new SmtpClient("smtp.gmail.com")
                    {
                        Port = 587,
                        Credentials = new NetworkCredential(fromMail, frompass),
                        EnableSsl = true,
                    };

                    smtpClient.Send(message);
                }

                else
                {
                    MessageBox.Show("No Student Found", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
            catch (System.Exception) {

                    MessageBox.Show("Please Input Your Student ID", "Empty Field", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != VerificationCode)
            {
                MessageBox.Show("You Inputted a Wrong Verification Code", "Wrong Code", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            } else
            {
                panel3.Visible = false;
                panel4.Visible = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to Reset your password?", "Reset Password", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(conString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("UPDATE Accounts SET password = @NewPassword WHERE AID = @UserID", conn))
                    {
                        cmd.Parameters.AddWithValue("@NewPassword", textBox2.Text);
                        cmd.Parameters.AddWithValue("@UserID", ID);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Your password has been reset successfully.", "Reset Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Something went wrong!", "Reset Failed!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }
    }
}
