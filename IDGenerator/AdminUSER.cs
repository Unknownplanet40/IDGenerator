using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Windows.Forms;
using System.Data.SqlClient;
using Timer = System.Threading.Timer;

namespace IDGenerator
{
    public partial class AdminUSER : Form
    {
        public AdminUSER()
        {
            InitializeComponent();
        }

        string conString = IDGenerator.Properties.Settings.IDGeneratorCONSTRING.ConnectionString;

        private void viewIDBTN_Click(object sender, EventArgs e)
        {
            AdminSTATUS Stat = new AdminSTATUS();
            Stat.ShowDialog();
        }

        private void AdminUSER_Load(object sender, EventArgs e)
        {
            //using (SqlConnection cnn = new SqlConnection("Data Source=RJ45;Initial Catalog=IDGeneratorProject;Integrated Security=True"))
            //{
            //    cnn.Open();
            //    using (SqlCommand command = new SqlCommand("SELECT COUNT(StudentID) FROM Student_Info", cnn))
            //    {
            //        label3.Text = Convert.ToString(command.ExecuteScalar());
            //    }
            //}

            //using (SqlConnection cnn = new SqlConnection("Data Source=RJ45;Initial Catalog=IDGeneratorProject;Integrated Security=True"))
            //{
            //    cnn.Open();
            //    using (SqlCommand command = new SqlCommand("SELECT COUNT(AID) FROM Accounts WHERE role = 1", cnn))
            //    {
            //        label4.Text = Convert.ToString(command.ExecuteScalar());
            //    }
            //}
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AdminNewRegisters NewReg = new AdminNewRegisters();
            NewReg.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(conString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("UPDATE Accounts SET status=0 WHERE username = @SID", con))
                {
                    cmd.Parameters.AddWithValue("@SID", UID.Text);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        LOGIN Login = new LOGIN();
                        Login.Show();
                        this.Close();
                    }
                }
            }
        }

        private void ResetID_Click(object sender, EventArgs e)
        {
            AdminREQUEST Req = new AdminREQUEST();
            Req.ShowDialog();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            AdminSCHOOL Asc = new AdminSCHOOL();
            Asc.ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            AdminACCOUNTS acc = new AdminACCOUNTS();
            acc.ShowDialog();
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
