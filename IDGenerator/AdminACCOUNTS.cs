using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Windows.Forms;
using System.Data.SqlClient;
using Timer = System.Threading.Timer;

namespace IDGenerator
{
    public partial class AdminACCOUNTS : Form
    {
        public AdminACCOUNTS()
        {
            InitializeComponent();
        }

        private int ID = 0;
        string conString = IDGenerator.Properties.Settings.IDGeneratorCONSTRING.ConnectionString;

        private void AdminACCOUNTS_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'iDGeneratorProjectDataSet1.Accounts' table. You can move, or remove it, as needed.
            this.accountsTableAdapter.Fill(this.iDGeneratorProjectDataSet1.Accounts);
            // TODO: This line of code loads data into the 'iDGeneratorProjectDataSet1.Student_Info' table. You can move, or remove it, as needed.
            this.student_InfoTableAdapter.Fill(this.iDGeneratorProjectDataSet1.Student_Info);
            // TODO: This line of code loads data into the 'iDGeneratorProjectDataSet.Accounts' table. You can move, or remove it, as needed.
            this.accountsTableAdapter.Admin(this.iDGeneratorProjectDataSet.Accounts);
            getlatesID();
        }

        private void getlatesID()
        {
            using (SqlConnection con = new SqlConnection(conString))
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand("SELECT MAX(AID) AS LatestAID FROM Accounts WHERE role = 1", con))
                {
                    // Execute the query
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        ID = Convert.ToInt32(reader["LatestAID"]);
                    }

                    reader.Close();
                }
            }

        }

        private void adminToolStripButton_Click_1(object sender, EventArgs e)
        {
            try
            {
                this.accountsTableAdapter.Admin(this.iDGeneratorProjectDataSet.Accounts);
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }

        }

        private void studentsToolStripButton_Click_1(object sender, EventArgs e)
        {
            try
            {
                this.accountsTableAdapter.Students(this.iDGeneratorProjectDataSet.Accounts);
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int indexRow = e.RowIndex;
            if (indexRow >= 0 && indexRow < dataGridView1.Rows.Count)
            {
                DataGridViewRow row = dataGridView1.Rows[indexRow];
                textBox1.Text = row.Cells[0].Value.ToString();
                textBox2.Text = row.Cells[1].Value.ToString();
                textBox3.Text = row.Cells[2].Value.ToString();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            getlatesID();
            using (SqlConnection con = new SqlConnection(conString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("UPDATE Accounts SET username = @u, password = @p WHERE AID = @SID", con))
                {
                    cmd.Parameters.AddWithValue("@u", textBox2.Text);
                    cmd.Parameters.AddWithValue("@p", textBox3.Text);
                    cmd.Parameters.AddWithValue("@SID", textBox1.Text);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Update Successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.accountsTableAdapter.Admin(this.iDGeneratorProjectDataSet.Accounts);
                    } else
                    {
                        MessageBox.Show("Something went wrong", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            getlatesID();
            if (String.IsNullOrWhiteSpace(textBox1.Text))
            {
                if (String.IsNullOrWhiteSpace(textBox2.Text) && String.IsNullOrWhiteSpace(textBox3.Text))
                {
                    MessageBox.Show("Username and Password is Empty", "Empty Fields", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                } else
                {
                    using (SqlConnection con = new SqlConnection(@"Data Source=LAPTOP-7CJ5L5U7\SQLEXPRESS;Initial Catalog=IDGeneratorProject;Integrated Security=True;Encrypt=False;TrustServerCertificate=True"))
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand("INSERT INTO Accounts (AID, username, password, role, status, last_access) VALUES (@a, @u, @p, 1, 0, GETDATE())", con))
                        {
                            cmd.Parameters.AddWithValue("@a", ID + 1);
                            cmd.Parameters.AddWithValue("@u", textBox2.Text);
                            cmd.Parameters.AddWithValue("@p", textBox3.Text);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Update Successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                this.accountsTableAdapter.Admin(this.iDGeneratorProjectDataSet.Accounts);
                            }
                            else
                            {
                                MessageBox.Show("Something went wrong", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            } else
            {
                MessageBox.Show("Please Click Clear Befor Procceding!", "Selected Content", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
        }

        private void adminToolStripButton1_Click(object sender, EventArgs e)
        {
            try
            {
                this.accountsTableAdapter.Admin(this.iDGeneratorProjectDataSet.Accounts);
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }

        }

        private void studentsToolStripButton1_Click(object sender, EventArgs e)
        {
            try
            {
                this.accountsTableAdapter.Students(this.iDGeneratorProjectDataSet.Accounts);
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}
