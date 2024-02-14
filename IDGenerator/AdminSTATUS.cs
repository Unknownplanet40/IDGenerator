using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Windows.Forms;
using System.Data.SqlClient;
using Timer = System.Threading.Timer;
using System.Xml.Linq;

namespace IDGenerator
{
    public partial class AdminSTATUS : Form
    {
        public AdminSTATUS()
        {
            InitializeComponent();
        }

        string conString = IDGenerator.Properties.Settings.IDGeneratorCONSTRING.ConnectionString;
        string smtpEmail = "Your email here";
        string smtpPass = "Your email password here";

        private void AdminSTATUS_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'iDGeneratorProjectDataSet.Student_Info' table. You can move, or remove it, as needed.
            this.student_InfoTableAdapter.Completed(this.iDGeneratorProjectDataSet.Student_Info);

            comboBox1.Items.Add("Pending");
            comboBox1.Items.Add("Approved");
            comboBox1.Items.Add("Disapproved");

            // Optionally, set the default selected index if needed
            comboBox1.SelectedIndex = -1;

        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int indexRow = e.RowIndex;
            if (indexRow >= 0 && indexRow < dataGridView1.Rows.Count)
            {
                DataGridViewRow row = dataGridView1.Rows[indexRow];
                textBox1.Text = row.Cells[0].Value.ToString();
                textBox2.Text = row.Cells[3].Value.ToString();
                if (row.Cells[4].Value != null)
                {
                    int status = Convert.ToInt32(row.Cells[4].Value);
                    int selectedIndex = status;
                    if (selectedIndex >= 0 && selectedIndex < comboBox1.Items.Count)
                    {
                        comboBox1.SelectedIndex = selectedIndex;
                    }
                    else
                    {
                        comboBox1.SelectedIndex = -1; // Reset to no selection
                    }
                }
                else
                {
                    comboBox1.SelectedIndex = -1; // Reset to no selection
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
            {
                MessageBox.Show("Please select one of the choices except 'Pending'", "Choose another", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } else if (String.IsNullOrWhiteSpace(textBox1.Text)){
                MessageBox.Show("Select data to proceed.", "No Data Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } else
            {
                using (SqlConnection conn = new SqlConnection(conString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("UPDATE Student_Info SET status = @stat WHERE StudentID = @UserID", conn))
                    {
                        cmd.Parameters.AddWithValue("@stat", comboBox1.SelectedIndex);
                        cmd.Parameters.AddWithValue("@UserID", textBox1.Text);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Updated Successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.student_InfoTableAdapter.Completed(this.iDGeneratorProjectDataSet.Student_Info);

                            string fromMail = smtpEmail;
                            string frompass = smtpPass;

                            MailMessage message = new MailMessage();
                            message.From = new MailAddress(fromMail);
                            message.Subject = "ID Status Changed";
                            message.To.Add(new MailAddress(textBox2.Text));
                            message.Body = "<html><body><p>This is To Inform You That Your ID Status Has Been Changed, Please Check Your Account For Cofirmation.</p>" +
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
                            MessageBox.Show("Something went wrong!", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void completedToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                this.student_InfoTableAdapter.Completed(this.iDGeneratorProjectDataSet.Student_Info);
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }

        }
    }
}
