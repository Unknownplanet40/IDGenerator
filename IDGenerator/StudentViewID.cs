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
    public partial class StudentViewID : Form
    {
        public StudentViewID()
        {
            InitializeComponent();
        }

        string conString = IDGenerator.Properties.Settings.IDGeneratorCONSTRING.ConnectionString;
        string smtpEmail = "Your email here";
        string smtpPass = "Your email password here";

        private void StudentViewID_Load(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(conString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM Student_Info WHERE StudentID = " + UIDreciever.Text, conn);
                SqlDataReader dataReader = cmd.ExecuteReader();

                while (dataReader.Read())
                {
                    String year = Convert.ToString(dataReader["year"]);
                    char y = year[0];
                    String section = Convert.ToString(dataReader["section"]);
                    String currentyear = String.Empty;

                    string dateAsString = Convert.ToString(dataReader["birthdate"]);
                    string formattedDate = String.Empty;
                    string gen = String.Empty;

                    if (DateTime.TryParse(dateAsString, out DateTime birthdate))
                    {
                        formattedDate = birthdate.ToString("MMM d, yyyy");
                    } else
                    {
                        formattedDate = dateAsString;
                    }

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

                    int gender = Convert.ToInt32(dataReader["gender"]);

                    if (gender == 1)
                    {
                        gen = "Male";
                    } else if (gender == 2)
                    {
                        gen = "Female";
                    } else
                    {
                        gen = "Unknown";
                    }

                    txtschool.Text = Convert.ToString(dataReader["schoolname"]);
                    txtschoolback.Text = Convert.ToString(dataReader["schoolname"]);
                    txtname.Text = Convert.ToString(dataReader["Firstname"]) + " " + Convert.ToString(dataReader["Middlename"])[0] + ". " + Convert.ToString(dataReader["Lastname"]);
                    txtsid.Text = Convert.ToString(dataReader["StudentID"]);
                    txtmail.Text = Convert.ToString(dataReader["mail"]);
                    txtphone.Text = Convert.ToString(dataReader["phone"]);
                    pbsig.ImageLocation = Convert.ToString(dataReader["signature"]);
                    txtYS.Text = currentyear + " Section " + section;
                    txtgn.Text = Convert.ToString(dataReader["guardianname"]);
                    txtgnum.Text = Convert.ToString(dataReader["guardianphone"]);
                    txtads.Text = Convert.ToString(dataReader["adviser"]);
                    txtprince.Text = Convert.ToString(dataReader["principalname"]);
                    address.Text = Convert.ToString(dataReader["address"]);
                    bdate.Text = formattedDate;
                    txtage.Text = Convert.ToString(dataReader["age"]);
                    txtgender.Text = gen;

                    int stat = Convert.ToInt32(dataReader["status"]);

                    if (stat == 1)
                    {
                        statlbl.Text = "Approved (Ready to Claim)";
                    } else if (stat == 2)
                    {
                        statlbl.Text = "Disapproved (Coordinate with Admin)";
                    }
                    else
                    {
                        statlbl.Text = "Pending";
                    }


                }
                conn.Close();
            }
        }
    }
}
