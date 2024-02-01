using System;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Threading;
using Timer = System.Threading.Timer;

namespace IDGenerator
{
    public partial class LOGIN : Form
    {
        SqlConnection con;
        SqlCommand cmd;
        SqlDataReader dr;
        Timer errorMessageTimer;
        public LOGIN()
        {
            InitializeComponent();
            con = new SqlConnection("Data Source=RJ45;Initial Catalog=IDGeneratorProject;Integrated Security=True");
        }

        // Methods go here ----------------------------------------------------------------
        private void UpdateLoginStatus(string username, int newStatus)
        {
            using (SqlConnection con = new SqlConnection(@"Data Source=RJ45;Initial Catalog=IDGeneratorProject;Integrated Security=True"))
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand("UPDATE Accounts SET status=@newStatus, last_access=GETDATE() WHERE username=@username", con))
                {
                    cmd.Parameters.AddWithValue("@newStatus", newStatus);
                    cmd.Parameters.AddWithValue("@username", username);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void ClearErrorMessage(object state)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ClearErrorMessage(null)));
                return;
            }

            ErrorMsg.Text = "";
            UnameError.Text = "";
            PwordError.Text = "";
        }




        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                Pword.UseSystemPasswordChar = false;
            } else
            {
                Pword.UseSystemPasswordChar = true;
            }
        }

        private void CloseBTN_MouseEnter(object sender, EventArgs e)
        {
            CloseBTN.Image = Properties.Resources.Close_Hover;
        }

        private void CloseBTN_MouseLeave(object sender, EventArgs e)
        {
            CloseBTN.Image = Properties.Resources.Close_Stanby;
        }

        private void CloseBTN_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ResetPassword_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ResetPassword RPF = new ResetPassword();
            RPF.Show();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string user = Uname.Text;
            string pass = Pword.Text;

            if (String.IsNullOrWhiteSpace(user)){
                UnameError.Text = "Username is Required";
                errorMessageTimer = new Timer(ClearErrorMessage, null, 3000, Timeout.Infinite);
            } else if (String.IsNullOrWhiteSpace(pass))
            {
                PwordError.Text = "Password is Required";
                errorMessageTimer = new Timer(ClearErrorMessage, null, 3000, Timeout.Infinite);
            } else
            {
                using (SqlConnection conn = new SqlConnection(@"Data Source=RJ45;Initial Catalog=IDGeneratorProject;Integrated Security=True"))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM Accounts WHERE username=@username OR password=@password", conn))
                    {
                        // Use parameters to prevent SQL injection
                        cmd.Parameters.AddWithValue("@username", user);
                        cmd.Parameters.AddWithValue("@password", pass);

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                String username = Convert.ToString(dr["username"]);
                                String password = Convert.ToString(dr["password"]);

                                if (username != user)
                                {
                                    ErrorMsg.Text = "Username is Invalid";
                                    errorMessageTimer = new Timer(ClearErrorMessage, null, 3000, Timeout.Infinite);
                                }
                                else if (password != pass)
                                {
                                    ErrorMsg.Text = "Password is Invalid";
                                    errorMessageTimer = new Timer(ClearErrorMessage, null, 3000, Timeout.Infinite);
                                }
                                else
                                {
                                    int role = Convert.ToInt32(dr["role"]);
                                    int status = Convert.ToInt32(dr["status"]);

                                    if (status == 0)
                                    {
                                        // update status to 1
                                        UpdateLoginStatus(user, 0);

                                        if (role == 1)
                                        {
                                            // Authentication successful
                                            AdminUSER admin = new AdminUSER();
                                            admin.Show();
                                            this.Hide();
                                        }
                                        else
                                        {
                                            StudentUSER student = new StudentUSER();
                                            student.Show();
                                            this.Hide();
                                        }
                                    }
                                    else
                                    {
                                        ErrorMsg.Text = "Your Account is Currently Login from another session.";
                                        errorMessageTimer = new Timer(ClearErrorMessage, null, 3000, Timeout.Infinite); // clear after 3 seconds
                                    }
                                }
                            }
                            else
                            {
                                ErrorMsg.Text = "Invalid Login. Please check username and password";
                                errorMessageTimer = new Timer(ClearErrorMessage, null, 3000, Timeout.Infinite);
                                Uname.Text = "";
                                Pword.Text = "";
                            }
                        }
                    }
                    con.Close();
                }
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            StudentRegister Regform = new StudentRegister();
            Regform.Show();
            this.Hide();
        }
    }
}
