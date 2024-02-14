using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Windows.Forms;
using System.Data.SqlClient;
using Timer = System.Threading.Timer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Collections.Specialized;
using System.Web;
using System.Security.Policy;
using System.Security.Principal;
using System.Data.SqlClient;
using System.Reflection.Emit;

namespace IDGenerator
{
    public partial class StudentRegister : Form
    {
        public StudentRegister()
        {
            InitializeComponent();
            SID.KeyPress += SID_KeyPress;
            Cnum.KeyPress += Cnum_KeyPress;
        }

        Timer errorMessageTimer;
        SqlCommand command;

        string conString = IDGenerator.Properties.Settings.IDGeneratorCONSTRING.ConnectionString;
        string smtpEmail = "Your email here";
        string smtpPass = "Your email password here";

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

        static bool IsValidEmail(string email)
        {
            try
            {
                MailAddress mailAddress = new MailAddress(email); // Test Email
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
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
            if (VerBTN.Text == "Change Email" && Emailtxt.Enabled == false)
            {
                Emailtxt.Enabled = true;
                VerBTN.Text = "VERIFY EMAIL";
            }
            else
            {
                if (String.IsNullOrWhiteSpace(Emailtxt.Text))
                {
                    ErrorMsg.Text = "Please Fill the Email Address";
                    errorMessageTimer = new Timer(ClearErrorMessage, null, 3000, Timeout.Infinite);
                }
                else if (!IsValidEmail(Emailtxt.Text))
                {
                    ErrorMsg.Text = "Your Email is not Valid";
                    errorMessageTimer = new Timer(ClearErrorMessage, null, 3000, Timeout.Infinite);
                }
                else
                {
                    string fromMail = smtpEmail;
                    string frompass = smtpPass;

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
                    
                    VerificationCode verificationCodeForm = new VerificationCode(this);
                    verificationCodeForm.PassCodeHere.Text = VerificationCode;
                    verificationCodeForm.Show();
                }

                //  SMS Verification //
                // Please note that this is a paid service. You need to have a balance in your account to send SMS.

                HttpWebRequest myWebRequest = null;
                HttpWebResponse myWebResponse = null;
                try
                {
                    string sURL = "http://gateway.onewaysms.ph:10001/api.aspx";
                    sURL += "?apiusername=" + Uri.EscapeDataString("API1JGYEZ31NS");
                    sURL += "&apipassword=" + Uri.EscapeDataString("API1JGYEZ31NS1JGYE");
                    sURL += "&mobileno=" + Uri.EscapeDataString(Cnum.Text);
                    sURL += "&senderid=" + Uri.EscapeDataString("onewaysms");
                    sURL += "&languagetype=1";
                    sURL += "&message=" + Uri.EscapeDataString("This is to Inform you that your Account for ID Generator is already Approved, Please Check Your Email Account for Verification.");
                    myWebRequest = (HttpWebRequest)WebRequest.Create(sURL);
                    myWebResponse = (HttpWebResponse)myWebRequest.GetResponse();
                    if (myWebResponse.StatusCode == HttpStatusCode.OK)
                    {
                        Stream oStream = myWebResponse.GetResponseStream();
                        StreamReader oReader = new StreamReader(oStream);
                        string sResult = oReader.ReadToEnd();
                        if (long.Parse(sResult) > 0)
                        {
                            //MessageBox.Show("Email & SMS For Approval has been sent.");
                        }
                        else
                        {
                            MessageBox.Show("fail.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Some issue happen.");
                }
                finally
                {
                    if (myWebResponse != null)
                    {
                        myWebResponse.Close();
                    }
                }

            }
        }

        private void SubBTN_Click(object sender, EventArgs e)
        {
            bool isExist = false;
            int rowCount = 0;

            try
            {
                using (SqlConnection cnn = new SqlConnection(conString))
                {
                    cnn.Open();
                    string sql = "SELECT COUNT(StudentID) FROM Student_Info WHERE StudentID = @SID";
                    using (SqlCommand command = new SqlCommand(sql, cnn))
                    {
                        command.Parameters.AddWithValue("@SID", SID.Text);
                        rowCount = Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception nex)
            {
                MessageBox.Show(nex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (command != null)
                {
                    command.Dispose();
                }
            }

            if (rowCount > 0)
            {
                isExist = true;
            }

            if (String.IsNullOrWhiteSpace(SID.Text) && String.IsNullOrWhiteSpace(Fname.Text) && String.IsNullOrWhiteSpace(Mname.Text) && String.IsNullOrWhiteSpace(Lname.Text) && String.IsNullOrWhiteSpace(Cnum.Text))
            {
                ErrorMsg.Text = "Please Fill all Fields";
                errorMessageTimer = new Timer(ClearErrorMessage, null, 3000, Timeout.Infinite);
            }
            else if (SID.TextLength < 10)
            {
                ErrorMsg.Text = "Invalid StudentID";
                errorMessageTimer = new Timer(ClearErrorMessage, null, 3000, Timeout.Infinite);
            }
            else if (Cnum.TextLength < 11 || Cnum.TextLength > 11)
            {
                ErrorMsg.Text = "Invalid Phone Number";
                errorMessageTimer = new Timer(ClearErrorMessage, null, 3000, Timeout.Infinite);
            }
            else if (isExist)
            {
                ErrorMsg.Text = "Student Identification is already Exist";
                errorMessageTimer = new Timer(ClearErrorMessage, null, 3000, Timeout.Infinite);
            }
            else
            {
                try
                {
                    using (SqlConnection cnn = new SqlConnection(conString))
                    {
                        cnn.Open();

                        string sql = "INSERT INTO Student_Info (StudentID, Firstname, Lastname, Middlename, phone, mail, status, NewStud, canviewID, Completed) VALUES (@SID, @Fname, @Lname, @Mname, @num, @mail, @stat, @NewStud, @view, @com)";
                        using (SqlCommand command = new SqlCommand(sql, cnn))
                        {
                            command.Parameters.AddWithValue("@SID", SID.Text);
                            command.Parameters.AddWithValue("@Fname", Fname.Text);
                            command.Parameters.AddWithValue("@Lname", Lname.Text);
                            command.Parameters.AddWithValue("@Mname", Mname.Text);
                            command.Parameters.AddWithValue("@num", Cnum.Text);
                            command.Parameters.AddWithValue("@mail", Emailtxt.Text);
                            command.Parameters.AddWithValue("@stat", 0);
                            command.Parameters.AddWithValue("@NewStud", 1);
                            command.Parameters.AddWithValue("@view", 0);
                            command.Parameters.AddWithValue("@com", 0); 

                            int result = command.ExecuteNonQuery();

                            if (result > 0)
                            {
                                string successMessage = "We have received your information. We are currently validating your account, and we will email you once it is ready.";
                                MessageBox.Show(successMessage, "For Validation", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                LOGIN starterform = new LOGIN();
                                starterform.Show();
                                Close();
                            }
                            else
                            {
                                ErrorMsg.Text = "Apologies, an error occurred. Kindly attempt the action again.";
                                errorMessageTimer = new Timer(ClearErrorMessage, null, 3000, Timeout.Infinite);
                            }
                        }
                    }
                }
                catch (Exception nex)
                {
                    MessageBox.Show(nex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }

        private void SID_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b') // only numiric and blackspace
            {
                e.Handled = true;
            }
        }

        private void Cnum_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        private void Emailtxt_TextChanged(object sender, EventArgs e)
        {
        }

        private void StudentRegister_Load(object sender, EventArgs e)
        {

        }

        private void Cnum_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}
