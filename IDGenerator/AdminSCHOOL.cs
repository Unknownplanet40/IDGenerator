using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Windows.Forms;
using System.Data.SqlClient;
using Timer = System.Threading.Timer;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Xml;

namespace IDGenerator
{
    public partial class AdminSCHOOL : Form
    {
        public AdminSCHOOL()
        {
            InitializeComponent();
        }

        private void AdminSCHOOL_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'iDGeneratorProjectDataSet.School_Info' table. You can move, or remove it, as needed.
            this.school_InfoTableAdapter.Fill(this.iDGeneratorProjectDataSet.School_Info);
            tableupdate();
            tableid();
        }

        private void tableupdate()
        {
            using (SqlConnection conn = new SqlConnection(@"Data Source=LAPTOP-7CJ5L5U7\SQLEXPRESS;Initial Catalog=IDGeneratorProject;Integrated Security=True;Encrypt=False;TrustServerCertificate=True"))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT School, principal FROM School_Info WHERE ID = 1", conn);
                SqlDataReader dataReader = cmd.ExecuteReader();

                while (dataReader.Read())
                {

                    textBox1.Text = Convert.ToString(dataReader["School"]);
                    textBox2.Text = Convert.ToString(dataReader["principal"]);
                }
                conn.Close();
            }
        }

        private void tableid()
        {
            using (SqlConnection conn = new SqlConnection(@"Data Source=LAPTOP-7CJ5L5U7\SQLEXPRESS;Initial Catalog=IDGeneratorProject;Integrated Security=True;Encrypt=False;TrustServerCertificate=True"))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT TOP 1 ID FROM School_Info ORDER BY ID DESC", conn);
                SqlDataReader dataReader = cmd.ExecuteReader();

                while (dataReader.Read())
                {

                    textBox3.Text = Convert.ToString(dataReader["ID"]);
                }
                conn.Close();
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int indexRow = e.RowIndex;
            if (indexRow >= 0 && indexRow < dataGridView1.Rows.Count)
            {
                DataGridViewRow row = dataGridView1.Rows[indexRow];
                textBox3.Text = row.Cells[0].Value.ToString();
                textBox4.Text = row.Cells[1].Value.ToString();
                textBox5.Text = row.Cells[2].Value.ToString();
                textBox6.Text = row.Cells[3].Value.ToString();
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(textBox1.Text) && String.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Please Enter A value", "Empty input", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            } else
            {
                using (SqlConnection conn = new SqlConnection(@"Data Source=LAPTOP-7CJ5L5U7\SQLEXPRESS;Initial Catalog=IDGeneratorProject;Integrated Security=True;Encrypt=False;TrustServerCertificate=True"))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("UPDATE School_Info SET School = @sch, principal = @prin WHERE ID = 1", conn))
                    {
                        cmd.Parameters.AddWithValue("@sch", textBox1.Text);
                        cmd.Parameters.AddWithValue("@prin", textBox2.Text);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Updated Successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            tableupdate();
                        }
                        else
                        {
                            MessageBox.Show("Something went wrong!", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(textBox4.Text) && String.IsNullOrWhiteSpace(textBox5.Text) && String.IsNullOrWhiteSpace(textBox6.Text))
            {
                MessageBox.Show("Please Enter A value", "Empty input", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(@"Data Source=LAPTOP-7CJ5L5U7\SQLEXPRESS;Initial Catalog=IDGeneratorProject;Integrated Security=True;Encrypt=False;TrustServerCertificate=True"))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("UPDATE School_Info SET adviser = @ad, year = @y, section = @sec WHERE ID = @DI", conn))
                    {
                        cmd.Parameters.AddWithValue("@ad", textBox4.Text);
                        cmd.Parameters.AddWithValue("@y", textBox5.Text);
                        cmd.Parameters.AddWithValue("@sec", textBox6.Text);
                        cmd.Parameters.AddWithValue("@DI", textBox3.Text);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Updated Successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.school_InfoTableAdapter.Fill(this.iDGeneratorProjectDataSet.School_Info);
                        }
                        else
                        {
                            MessageBox.Show("Something went wrong!", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(textBox4.Text) && String.IsNullOrWhiteSpace(textBox5.Text) && String.IsNullOrWhiteSpace(textBox6.Text))
            {
                MessageBox.Show("Please Enter A value", "Empty input", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(@"Data Source=LAPTOP-7CJ5L5U7\SQLEXPRESS;Initial Catalog=IDGeneratorProject;Integrated Security=True;Encrypt=False;TrustServerCertificate=True"))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO School_Info (ID,adviser,year,section,School,principal) VALUES (@DI,@ad,@y,@sec,NULL,NULL)", conn))
                    {

                        int addid = int.Parse(textBox3.Text);
                        addid += 1;

                        cmd.Parameters.AddWithValue("@ad", textBox4.Text);
                        cmd.Parameters.AddWithValue("@y", textBox5.Text);
                        cmd.Parameters.AddWithValue("@sec", textBox6.Text);
                        cmd.Parameters.AddWithValue("@DI", addid);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Inserted Successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.school_InfoTableAdapter.Fill(this.iDGeneratorProjectDataSet.School_Info);
                        }
                        else
                        {
                            MessageBox.Show("Something went wrong!", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }
    }
}
