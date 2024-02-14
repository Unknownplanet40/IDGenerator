using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Windows.Forms;
using System.Data.SqlClient;
using Timer = System.Threading.Timer;

namespace IDGenerator
{
    public partial class StudentAccountSetting : Form
    {
        public StudentAccountSetting()
        {
            InitializeComponent();
        }
        private string oldPass = String.Empty;

        string conString = IDGenerator.Properties.Settings.IDGeneratorCONSTRING.ConnectionString;
        string smtpEmail = "Your email here";
        string smtpPass = "Your email password here";

        private void StudentAccountSetting_Load(object sender, EventArgs e)
        {

            try
            {
                using (SqlConnection conn = new SqlConnection(conString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT * FROM Student_Info WHERE StudentID = " + USERID.Text, conn);
                    SqlDataReader dataReader = cmd.ExecuteReader();

                    while (dataReader.Read())
                    {
                        String year = Convert.ToString(dataReader["year"]);
                        char y = year[0];

                        String currentyear = String.Empty;
                        String section = Convert.ToString(dataReader["section"]);

                        switch (y)
                        {
                            case '1':
                                currentyear = "1st Year";
                                break;
                            case '2':
                                currentyear = "1st Year";
                                break;
                            case '3':
                                currentyear = "1st Year";
                                break;
                            case '4':
                                currentyear = "1st Year";
                                break;
                            default:
                                currentyear = "Unknow";
                                break;
                        }

                        textBox3.Text = Convert.ToString(dataReader["Firstname"]) + " " + Convert.ToString(dataReader["Middlename"])[0] + ". " + Convert.ToString(dataReader["Lastname"]);
                        textBox4.Text = Convert.ToString(dataReader["mail"]);
                        textBox5.Text = Convert.ToString(dataReader["phone"]);
                        pictureBox1.ImageLocation = Convert.ToString(dataReader["signature"]);
                        textBox6.Text = currentyear + " Section " + section;
                        textBox7.Text = Convert.ToString(dataReader["guardianname"]);
                        textBox8.Text = Convert.ToString(dataReader["guardianphone"]);
                        textBox9.Text = Convert.ToString(dataReader["address"]);
                    }
                    conn.Close();
                }
            } catch (Exception ex)
            {

            }

            using (SqlConnection conn = new SqlConnection(conString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM Accounts WHERE AID = " + USERID.Text, conn);
                SqlDataReader dataReader = cmd.ExecuteReader();

                while (dataReader.Read())
                {

                    textBox1.Text = Convert.ToString(dataReader["username"]);
                    textBox2.Text = Convert.ToString(dataReader["password"]);
                    oldPass = textBox2.Text;
                }
                conn.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox2.Text != oldPass)
            {
                DialogResult result = MessageBox.Show("Are you sure you want to Reset your password?", "Reset Password", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    using (SqlConnection conn = new SqlConnection(@"Data Source=LAPTOP-7CJ5L5U7\SQLEXPRESS;Initial Catalog=IDGeneratorProject;Integrated Security=True;Encrypt=False;TrustServerCertificate=True"))
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand("UPDATE Accounts SET password = @NewPassword WHERE AID = @UserID", conn))
                        {
                            cmd.Parameters.AddWithValue("@NewPassword", textBox2.Text);
                            cmd.Parameters.AddWithValue("@UserID", USERID.Text);
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
            } else
            {
                MessageBox.Show("No Change have been made", "Nothing Change", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void USERID_Click(object sender, EventArgs e)
        {

        }
    }
}
