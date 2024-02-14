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
    public partial class AdminREQUEST : Form
    {
        public AdminREQUEST()
        {
            InitializeComponent();
        }
        private string EMAIL = String.Empty;
        private string NAME = String.Empty;

        private void AdminREQUEST_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'iDGeneratorProjectDataSet.Student_Info' table. You can move, or remove it, as needed.
            this.student_InfoTableAdapter.Reset(this.iDGeneratorProjectDataSet.Student_Info);
        }

        private void resetToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                this.student_InfoTableAdapter.Reset(this.iDGeneratorProjectDataSet.Student_Info);
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }

        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int indexRow = e.RowIndex;
            try
            {
                if (indexRow >= 0 && indexRow < dataGridView1.Rows.Count)
                {
                    DataGridViewRow row = dataGridView1.Rows[indexRow];
                    textBox1.Text = row.Cells[0].Value.ToString();
                    textBox2.Text = row.Cells[1].Value.ToString() + " " + row.Cells[2].Value.ToString();
                    string Year = row.Cells[3].Value.ToString();
                    string section = row.Cells[4].Value.ToString();
                    char NY = Year[0];
                    string Y = String.Empty;
                    switch (NY)
                    {
                        case '1':
                            Y = "1st Year";
                            break;
                        case '2':
                            Y = "2nd Year";
                            break;
                        case '3':
                            Y = "3rd Year";
                            break;
                        case '4':
                            Y = "4th Year";
                            break;
                        default:
                            Y = "Unknown";
                            break;
                    }

                    textBox3.Text = Y + " Section " + section;
                    EMAIL = row.Cells[6].Value.ToString();
                    NAME = textBox2.Text;
                }
            } catch (IndexOutOfRangeException ex)
            {
                MessageBox.Show("Nothing to Select!", "???", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Select atleast one", "Not Selected", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            } else
            {
                if (MessageBox.Show("This will Reset ID Information do you want to Continue?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    using (SqlConnection cnn = new SqlConnection("Data Source=LAPTOP-7CJ5L5U7\\SQLEXPRESS;Initial Catalog=IDGeneratorProject;Integrated Security=True;Encrypt=False;TrustServerCertificate=True"))
                    {
                        cnn.Open();
                        using (SqlCommand cmd = new SqlCommand("UPDATE Student_Info SET Firstname = @fname, Middlename = @mname, Lastname = @lname, gender = @gen, birthdate = @bdate, age = @age, year = @year, section = @sec, adviser = @ads, address = @add, phone = @pnum, mail = @mail, guardianname = @gn, guardianphone = @gnp, principalname = @pname, schoolname = @school, signature = @sig, ReqReset = @req, status = @stat, canviewID = @VID, Completed = @Com WHERE StudentID = @UserID", cnn))
                        {
                            // Set parameters that are not null
                            cmd.Parameters.AddWithValue("@fname", DBNull.Value);
                            cmd.Parameters.AddWithValue("@mname", DBNull.Value);

                            // Set parameters to DBNull.Value to update them to null in the database
                            cmd.Parameters.AddWithValue("@lname", DBNull.Value);
                            cmd.Parameters.AddWithValue("@gen", DBNull.Value);
                            cmd.Parameters.AddWithValue("@bdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@age", DBNull.Value);
                            cmd.Parameters.AddWithValue("@year", DBNull.Value);
                            cmd.Parameters.AddWithValue("@sec", DBNull.Value);
                            cmd.Parameters.AddWithValue("@ads", DBNull.Value);
                            cmd.Parameters.AddWithValue("@add", DBNull.Value);
                            cmd.Parameters.AddWithValue("@pnum", DBNull.Value);
                            cmd.Parameters.AddWithValue("@mail", DBNull.Value);
                            cmd.Parameters.AddWithValue("@gn", DBNull.Value);
                            cmd.Parameters.AddWithValue("@gnp", DBNull.Value);
                            cmd.Parameters.AddWithValue("@pname", DBNull.Value);
                            cmd.Parameters.AddWithValue("@school", DBNull.Value);
                            cmd.Parameters.AddWithValue("@sig", DBNull.Value);
                            cmd.Parameters.AddWithValue("@req", 0);
                            cmd.Parameters.AddWithValue("@stat", 0);
                            cmd.Parameters.AddWithValue("@VID", 0);
                            cmd.Parameters.AddWithValue("@Com", 0);

                            // Set parameters that are not null
                            cmd.Parameters.AddWithValue("@UserID", textBox1.Text);

                            int rowaffected = cmd.ExecuteNonQuery();

                            if(rowaffected > 0)
                            {
                                if (MessageBox.Show("Information has been successfully reset! We will notify the user shortly.", "Reset Execute Saccessfully!", MessageBoxButtons.OK, MessageBoxIcon.Hand) == DialogResult.OK){
                                    string fromMail = "Ryanjamesc4@gmail.com";
                                    string frompass = "pxfsgnddzvmenzus";

                                    MailMessage message = new MailMessage();
                                    message.From = new MailAddress(fromMail);
                                    message.Subject = "Verification Code";
                                    message.To.Add(new MailAddress(EMAIL));
                                    message.Body = "<html><body><p>Dear " + NAME + "<br>" +
                                    "<p>Your Identification Information Has Been Reset Please see the changes</p>" +
                                    "<p>Thank you!</p>" +
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
                                    this.student_InfoTableAdapter.Reset(this.iDGeneratorProjectDataSet.Student_Info);
                                    textBox1.Text = "";
                                    textBox2.Text = "";
                                    textBox3.Text = "";
                                }
                            }
                            else
                            {
                                MessageBox.Show("Something went wrong!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
        }

        private void resetToolStripButton_Click_1(object sender, EventArgs e)
        {
            try
            {
                this.student_InfoTableAdapter.Reset(this.iDGeneratorProjectDataSet.Student_Info);
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }

        }
    }
}
