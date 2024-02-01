using System;
using System.Net;
using System.Net.Mail;
using System.Windows.Forms;
using System.Threading;
using Timer = System.Threading.Timer;

namespace IDGenerator
{
    public partial class StudentRegister : Form
    {
        Timer errorMessageTimer;
        public StudentRegister()
        {
            InitializeComponent();
        }

        private void ClearErrorMessage(object state)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ClearErrorMessage(null)));
                return;
            }

            ErrorMsg.Text = "";
        }

        public void EnableSubBTN()
        {
            SubBTN.Enabled = true;
        }

        static string GenerateVerificationCode()
        {
            Random random = new Random();
            int code = random.Next(100000, 999999); // Generates a random 6-digit code

            return code.ToString();
        }

        string VerificationCode = GenerateVerificationCode();

        private void Backtomainmenu_Click(object sender, EventArgs e)
        {
            LOGIN starterform = new LOGIN();
            starterform.Show();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(Emailtxt.Text))
            {
                ErrorMsg.Text = "Please Fill the Email Address";
                errorMessageTimer = new Timer(ClearErrorMessage, null, 3000, Timeout.Infinite);
            } else
            {
                string fromMail = "Ryanjamesc4@gmail.com";
                string frompass = "pxfsgnddzvmenzus";

                MailMessage message = new MailMessage();
                message.From = new MailAddress(fromMail);
                message.Subject = "Verification Code";
                message.To.Add(new MailAddress(Emailtxt.Text));
                message.Body = "<html><body><p>Dear " + Fname.Text + "<br>" +
                "<p>Thank you for registering. To complete the registration process, please copy and paste the following verification code on our Application</p>" +
                "<p><strong>Verification Code:</strong> " + VerificationCode +
                "<br>" +
                "<p>Please note that this verification code are valid for a limited time.</p>" +
                "<br>" +
                "<p>If you did not Register for our service, please disregard this email.</p>" +
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

                // Assuming you are creating the VerificationCode form from StudentRegister form
                VerificationCode verificationCodeForm = new VerificationCode(this);
                verificationCodeForm.PassCodeHere.Text = VerificationCode;
                verificationCodeForm.Show();
            }
        }
    }
}
