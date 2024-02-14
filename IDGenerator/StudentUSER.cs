using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Windows.Forms;
using System.Data.SqlClient;
using Timer = System.Threading.Timer;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace IDGenerator
{
    public partial class StudentUSER : Form
    {
        public StudentUSER()
        {
            InitializeComponent();
        }

        Timer errorMessageTimer;
        List<string> advisers = new List<string>();
        List<string> years = new List<string>();
        List<string> sections = new List<string>();
        List<string> Schoolmo = new List<string>();
        List<string> principalmo = new List<string>();


        private string imagelocation = String.Empty;
        private string SignaturePath = String.Empty;

        public void ReadOnly()
        {
            txtmail.ReadOnly = true;
        }

        private void ClearErrorMessage(object state)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ClearErrorMessage(null)));
                return;
            }

            Error.Text = "";
        }

        static string GenerateVerificationCode()
        {
            Random random = new Random();
            int code = random.Next(100000, 999999); // Generates a random 6-digit code

            return code.ToString();
        }

        string code = GenerateVerificationCode();

        private void StudentUSER_Load(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection("Data Source=LAPTOP-7CJ5L5U7\\SQLEXPRESS;Initial Catalog=IDGeneratorProject;Integrated Security=True;Encrypt=False;TrustServerCertificate=True"))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM School_Info;", conn);
                SqlDataReader dataReader = cmd.ExecuteReader();

                while (dataReader.Read())
                {
                    advisers.Add(Convert.ToString(dataReader["adviser"]));
                    years.Add(Convert.ToString(dataReader["year"]) + " Year");
                    sections.Add(Convert.ToString(dataReader["Section"]));
                    Schoolmo.Add(Convert.ToString(dataReader["School"]));
                    principalmo.Add(Convert.ToString(dataReader["principal"]));
                }
                cbyear.DataSource = years;
                cbsection.DataSource = sections;
                txtprincipal.Text = principalmo[0];
                txtschool.Text = Schoolmo[0];
                conn.Close();
            }

            using (SqlConnection conn = new SqlConnection(@"Data Source=LAPTOP-7CJ5L5U7\SQLEXPRESS;Initial Catalog=IDGeneratorProject;Integrated Security=True;Encrypt=False;TrustServerCertificate=True"))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand("SELECT * FROM Student_Info WHERE StudentID=@SID", conn))
                {
                    cmd.Parameters.AddWithValue("@SID", UserIDlabel.Text);

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {

                            if (Convert.ToInt32(dr["canviewID"]) == 1)
                            {
                                viewIDBTN.Enabled = true;
                            }
                            else
                            {
                                viewIDBTN.Enabled = false;
                            }

                            if (Convert.ToInt32(dr["Completed"]) == 1)
                            {
                                Cover.Visible = true;
                                covermsg.Text = "Information has been Proccess, You can now View your ID";
                                ResetID.Enabled = true;
                            }
                            else
                            {
                                if (!String.IsNullOrEmpty(Convert.ToString(dr["StudentID"])))
                                {
                                    txtID.ReadOnly = true;
                                    txtID.Text = Convert.ToString(dr["StudentID"]);
                                }

                                if (!String.IsNullOrEmpty(Convert.ToString(dr["Firstname"])))
                                {
                                    txtfname.ReadOnly = true;
                                    txtfname.Text = Convert.ToString(dr["Firstname"]);
                                }

                                if (!String.IsNullOrEmpty(Convert.ToString(dr["Middlename"])))
                                {
                                    txtmname.ReadOnly = true;
                                    txtmname.Text = Convert.ToString(dr["Middlename"]);
                                }

                                if (!String.IsNullOrEmpty(Convert.ToString(dr["Lastname"])))
                                {
                                    txtLname.ReadOnly = true;
                                    txtLname.Text = Convert.ToString(dr["Lastname"]);
                                }

                                if (!String.IsNullOrEmpty(Convert.ToString(dr["phone"])))
                                {
                                    txtnum.ReadOnly = true;
                                    txtnum.Text = Convert.ToString(dr["phone"]);
                                }

                                if (!String.IsNullOrEmpty(Convert.ToString(dr["mail"])))
                                {
                                    txtmail.ReadOnly = true;
                                    txtmail.Text = Convert.ToString(dr["mail"]);
                                }
                            }
                        }
                        else
                        {
                            Cover.Visible = true;
                            covermsg.Text = "We did not find any data to display";
                        }
                    }
                }
                conn.Close();
            }
        }

        private void dtbday_ValueChanged(object sender, EventArgs e)
        {
            DateTime birthdate = dtbday.Value;
            DateTime currentDate = DateTime.Now;

            int age = currentDate.Year - birthdate.Year;

            // Adjust age if the birthdate hasn't occurred yet this year
            if (currentDate.Month < birthdate.Month || (currentDate.Month == birthdate.Month && currentDate.Day < birthdate.Day))
            {
                age--;
            }

            txtage.Text = age.ToString();
        }

        private void cbyear_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedItemIndex = cbyear.SelectedIndex;

            if (selectedItemIndex != -1)
            {
                cbadviser.Text = advisers[selectedItemIndex];
            }
        }

        private void cbsection_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            SignaturePath = path.Text;
            if (String.IsNullOrWhiteSpace(txtage.Text) || String.IsNullOrWhiteSpace(txtgn.Text) || String.IsNullOrWhiteSpace(txtgp.Text) || String.IsNullOrWhiteSpace(txtadd.Text) || String.IsNullOrWhiteSpace(cbyear.Text) || String.IsNullOrWhiteSpace(cbsection.Text)) {
                Error.Text = "Please Input all Fields";
                errorMessageTimer = new Timer(ClearErrorMessage, null, 3000, Timeout.Infinite);
            }
            else if (!RBM.Checked && !RBF.Checked)
            {
                Error.Text = "Gender is Required";
                errorMessageTimer = new Timer(ClearErrorMessage, null, 3000, Timeout.Infinite);
            }
            //else if (String.IsNullOrEmpty(SignaturePath))
            //{
            //    Error.Text = "Please Take a picture";
            //    errorMessageTimer = new Timer(ClearErrorMessage, null, 3000, Timeout.Infinite);
            //}
            else if (!Accept.Checked)
            {
                Error.Text = "Please Check the Checkbox to Confirm that all the information is Correct";
                errorMessageTimer = new Timer(ClearErrorMessage, null, 3000, Timeout.Infinite);
            } else
            {
                using (SqlConnection conn = new SqlConnection(@"Data Source=LAPTOP-7CJ5L5U7\SQLEXPRESS;Initial Catalog=IDGeneratorProject;Integrated Security=True;Encrypt=False;TrustServerCertificate=True"))
                {
                    conn.Open();

                    // Example: Update the 'ColumnName' column with 'NewValue' where the 'ConditionColumn' has a specific value
                    string updateQuery = "UPDATE Student_Info SET Firstname = @FN, Middlename = @MN, Lastname = @LN, phone = @num, mail= @mail, gender = @gen, birthdate = @bday, age = @age, year = @year, section = @sec, adviser = @adv, address = @add, guardianname = @gn, guardianphone = @gnp, principalname = @Priname, schoolname = @School, Completed = @com, canviewID = @viewID, signature = @sigpath WHERE StudentID = @SID";

                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                    {
                        int gen = 0;
                        if (RBM.Checked)
                        {
                            gen = 1;
                        }
                        else if (RBF.Checked)
                        {
                            gen = 2;
                        }

                        string bdate = dtbday.Value.ToString("yyyy-MM-dd HH:mm:ss");

                        cmd.Parameters.AddWithValue("@FN", txtfname.Text);
                        cmd.Parameters.AddWithValue("@MN", txtmname.Text);
                        cmd.Parameters.AddWithValue("@LN", txtLname.Text);
                        cmd.Parameters.AddWithValue("@num", txtnum.Text);
                        cmd.Parameters.AddWithValue("@mail", txtmail.Text);
                        cmd.Parameters.AddWithValue("@gen", gen);
                        cmd.Parameters.AddWithValue("@bday", bdate);
                        cmd.Parameters.AddWithValue("@age", txtage.Text);
                        cmd.Parameters.AddWithValue("@year", cbyear.Text);
                        cmd.Parameters.AddWithValue("@sec", cbsection.Text);
                        cmd.Parameters.AddWithValue("@adv", cbadviser.Text);
                        cmd.Parameters.AddWithValue("@add", txtadd.Text);
                        cmd.Parameters.AddWithValue("@gn", txtgn.Text);
                        cmd.Parameters.AddWithValue("@gnp", txtgp.Text);
                        cmd.Parameters.AddWithValue("@Priname", txtprincipal.Text);
                        cmd.Parameters.AddWithValue("@School", txtschool.Text);
                        cmd.Parameters.AddWithValue("@com", 1);
                        cmd.Parameters.AddWithValue("@viewID", 1);
                        cmd.Parameters.AddWithValue("@sigpath", SignaturePath);
                        cmd.Parameters.AddWithValue("@SID", UserIDlabel.Text);

                        // Execute the update command
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            if (MessageBox.Show("We Recieve your Information you can now View your ID", "Upload Successful", MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.OK)
                            {
                                viewIDBTN.Enabled = true;
                                ResetID.Enabled = true;
                                Cover.Visible = true;
                                covermsg.Text = "Information Has been Recieve you can Check your ID status in View ID Tab";
                            }
                        }
                        else
                        {
                            Error.Text = "An Error Occured Please Try Again!";
                            errorMessageTimer = new Timer(ClearErrorMessage, null, 3000, Timeout.Infinite);
                        }
                    }

                    conn.Close();
                }
            }
        }

        // Signature
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "PNG File|*.png| JPG file|*.jpg";

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    imagelocation = dialog.FileName;
                    pictureBox1.ImageLocation = imagelocation;
                }
                else
                {
                    // User canceled the dialog or no file was selected
                    return; // or handle accordingly
                }

                if (string.IsNullOrEmpty(imagelocation))
                {
                    // No image selected, handle accordingly
                    return;
                }

                // Extract the filename from the full path
                string filename = Path.GetFileName(imagelocation);

                // Create the destination folder path using the txtID value
                string destinationFolder = Path.Combine(Application.StartupPath, "Signatures", txtID.Text);

                // Create the destination folder if it doesn't exist
                if (!Directory.Exists(destinationFolder))
                {
                    Directory.CreateDirectory(destinationFolder);
                }

                // Combine the destination folder path with the filename
                string destinationPath = Path.Combine(destinationFolder, filename);

                // Generate a new filename (you can replace this with your logic)
                string newFilename = txtID.Text + "_" + txtage.Text + Path.GetExtension(filename);

                // Combine the destination folder path with the new filename
                string newDestinationPath = Path.Combine(destinationFolder, newFilename);

                // Check if the file already exists
                if (File.Exists(destinationPath))
                {
                    // Delete the existing file
                    File.Delete(destinationPath);
                }

                // Copy the image to the destination folder with the new filename
                File.Copy(imagelocation, newDestinationPath, true);
                imagename.Text = newFilename;
                SignaturePath = newDestinationPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void viewIDBTN_Click(object sender, EventArgs e)
        {
            StudentViewID View = new StudentViewID();
            View.UIDreciever.Text = UserIDlabel.Text;
            View.ShowDialog();
        }

        private void ResetID_Click(object sender, EventArgs e)
        {
            int Completed = 0, Reset = 0;

            using (SqlConnection conn = new SqlConnection(@"Data Source=LAPTOP-7CJ5L5U7\SQLEXPRESS;Initial Catalog=IDGeneratorProject;Integrated Security=True;Encrypt=False;TrustServerCertificate=True"))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT Completed, ReqReset FROM Student_Info;", conn);
                SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                if (dataReader.Read())
                {
                    //prevent value to be null
                    Completed = Convert.IsDBNull(dataReader["Completed"]) ? 0 : Convert.ToInt32(dataReader["Completed"]);
                    Reset = Convert.IsDBNull(dataReader["ReqReset"]) ? 0 : Convert.ToInt32(dataReader["ReqReset"]);
                }
            }

            if (Reset != 1)
            {
                if (Completed == 1)
                {
                    DialogResult result = MessageBox.Show("Would you like to submit a request to reset your identification information?", "Identity Reset Request", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        using (SqlConnection conn = new SqlConnection(@"Data Source=LAPTOP-7CJ5L5U7\SQLEXPRESS;Initial Catalog=IDGeneratorProject;Integrated Security=True;Encrypt=False;TrustServerCertificate=True"))
                        {
                            conn.Open();
                            using (SqlCommand cmdup = new SqlCommand("UPDATE Student_Info SET ReqReset = 1 WHERE StudentID = @SID", conn))
                            {
                                cmdup.Parameters.AddWithValue("@SID", UserIDlabel.Text);
                                int rowsAffected = cmdup.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Your request has been successfully submitted! You will receive an email notification once your request has been approved.", "Identity Reset Request", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else
                                {
                                    MessageBox.Show("Something went wrong!", "Identity Reset Request Failed!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                DialogResult result = MessageBox.Show("Are you sure you want to cancel your request to reset your identification information?", "Identity Reset Request Cancellation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    using (SqlConnection conn = new SqlConnection(@"Data Source=LAPTOP-7CJ5L5U7\SQLEXPRESS;Initial Catalog=IDGeneratorProject;Integrated Security=True;Encrypt=False;TrustServerCertificate=True"))
                    {
                        conn.Open();
                        using (SqlCommand cmdup = new SqlCommand("UPDATE Student_Info SET ReqReset = 0 WHERE StudentID = @SID", conn))
                        {
                            cmdup.Parameters.AddWithValue("@SID", UserIDlabel.Text);
                            int rowsAffected = cmdup.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("You've successfully canceled your request.", "Identity Reset Request Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("Something went wrong!", "Identity Reset Request Failed!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            StudentAccountSetting Acc = new StudentAccountSetting();
            Acc.USERID.Text = UserIDlabel.Text;
            Acc.ShowDialog();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(@"Data Source=LAPTOP-7CJ5L5U7\SQLEXPRESS;Initial Catalog=IDGeneratorProject;Integrated Security=True;Encrypt=False;TrustServerCertificate=True"))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("UPDATE Accounts SET status=0 WHERE AID = @SID", con))
                {
                    cmd.Parameters.AddWithValue("@SID", UserIDlabel.Text);
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
        
        private void button3_Click(object sender, EventArgs e)
        {
            StudentCapture Cap = new StudentCapture();
            Cap.id.Text = txtID.Text;
            Cap.uname.Text = greetings_uname.Text;
            Cap.Show();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void datelogin_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
