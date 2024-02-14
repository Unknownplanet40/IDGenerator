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
    public partial class AdminNewRegisters : Form
    {
        public AdminNewRegisters()
        {
            InitializeComponent();
        }
        private string UID = String.Empty;
        private string FNAME = String.Empty;
        private string MAIL = String.Empty;
        private string YEAR = DateTime.Now.Year.ToString();
        string conString = IDGenerator.Properties.Settings.IDGeneratorCONSTRING.ConnectionString;
        string smtpEmail = "Your email here";
        string smtpPass = "Your email password here"; // This is not recommended. Use a secure way to store your email password like App passwords or OAuth2.0 tokens.

        static string GenerateRandomPassword(int length)
        {
            const string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            StringBuilder password = new StringBuilder();

            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                int index = random.Next(allowedChars.Length);
                password.Append(allowedChars[index]);
            }

            return password.ToString();
        }

        string PASS = GenerateRandomPassword(8);

        private void AdminNewRegisters_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'iDGeneratorProjectDataSet.Student_Info' table. You can move, or remove it, as needed.
            this.student_InfoTableAdapter.NewStudent(this.iDGeneratorProjectDataSet.Student_Info);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Please Select user first!", "No User Selected", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            } else
            {
                if (MessageBox.Show("Do you want to approve this student to create an account?", "Account Creation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    using (SqlConnection conn = new SqlConnection(conString))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("SELECT * FROM Student_Info WHERE StudentID = " + textBox1.Text, conn);
                        SqlDataReader dataReader = cmd.ExecuteReader();

                        while (dataReader.Read())
                        {
                            UID = Convert.ToString(dataReader["StudentID"]);
                            FNAME = Convert.ToString(dataReader["Firstname"]);
                            MAIL = Convert.ToString(dataReader["mail"]);
                        }
                        conn.Close();
                    }

                    string NFNAME = FNAME.Replace(" ", "");
                    string uname = NFNAME + YEAR;

                    using (SqlConnection cnn = new SqlConnection(conString))
                    {
                        cnn.Open();

                        // Insert into Accounts table
                        string insertSql = "INSERT INTO Accounts (AID, username, password, role, status, Last_access) VALUES (@AID, @Uname, @Pword, @role, @stat, @last)";
                        using (SqlCommand command = new SqlCommand(insertSql, cnn))
                        {
                            command.Parameters.AddWithValue("@AID", UID);
                            command.Parameters.AddWithValue("@Uname", uname);
                            command.Parameters.AddWithValue("@Pword", PASS);
                            command.Parameters.AddWithValue("@role", 0);
                            command.Parameters.AddWithValue("@stat", 0);
                            command.Parameters.AddWithValue("@last", DateTime.Now);

                            int result = command.ExecuteNonQuery();

                            if (result > 0)
                            {
                                // Sending verification email
                                string successMessage = "Account has been created! Account details are now being sent to the user.";
                                if (MessageBox.Show(successMessage, "Account Created", MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.OK)
                                {
                                    string fromMail = smtpEmail;
                                    string frompass = smtpPass;

                                    MailMessage message = new MailMessage();
                                    message.From = new MailAddress(fromMail);
                                    message.Subject = "Verification Code";
                                    message.To.Add(new MailAddress(MAIL));
                                    message.Body = "<html><body><p>Dear " + FNAME + "<br>" +
                                    "<p>Thank you for registering. Your account is now ready. Please log in to our application:</p>" +
                                    "<p><strong>Username:</strong> " + uname + "<br>" +
                                    "<strong>Password:</strong> " + PASS + "</p>" +
                                    "<p>If you did not register for our service, please disregard this email.</p>" +
                                    "<p>Best regards,<br>Administrator</p>" +
                                    "</body></html>";
                                    message.IsBodyHtml = true;

                                    using (var smtpClient = new SmtpClient("smtp.gmail.com"))
                                    {
                                        smtpClient.Port = 587;
                                        smtpClient.Credentials = new NetworkCredential(fromMail, frompass);
                                        smtpClient.EnableSsl = true;
                                        smtpClient.Send(message);
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("Something went wrong!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        using (SqlCommand cmd = new SqlCommand("UPDATE Student_Info SET NewStud = @New WHERE StudentID = @UserID", cnn))
                        {
                            cmd.Parameters.AddWithValue("@New", 0);
                            cmd.Parameters.AddWithValue("@UserID", UID);
                            cmd.ExecuteNonQuery();
                            this.student_InfoTableAdapter.NewStudent(this.iDGeneratorProjectDataSet.Student_Info);
                        }
                    }
                }
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int indexRow = e.RowIndex;
            if (indexRow >= 0 && indexRow < dataGridView1.Rows.Count)
            {
                DataGridViewRow row = dataGridView1.Rows[indexRow];
                textBox1.Text = row.Cells[0].Value.ToString();
            }
        }

        private void newStudentToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                this.student_InfoTableAdapter.NewStudent(this.iDGeneratorProjectDataSet.Student_Info);
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }

        }
    }
}
