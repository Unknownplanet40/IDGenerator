using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV.Structure;
using Emgu.CV;
using System.IO;

namespace IDGenerator
{
    public partial class StudentCapture : Form
    {
        bool _streaming;
        Capture _capture;
        public StudentCapture()
        {
            InitializeComponent();
            _streaming = false;
            _capture = new Capture();
        }

        private void streaming(object sender, System.EventArgs e)
        {
            var img = _capture.QueryFrame().ToImage<Bgr, Byte>();
            var bmp = img.Bitmap;
            pictureBox1.Image = bmp;
        }

        private void StudentCapture_Load(object sender, EventArgs e)
        {
            Application.Idle += streaming;
            _streaming = !_streaming;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = pictureBox1.Image;
            pictureBox1.Visible = false;
            pictureBox2.Visible = true;


        }

        private void button3_Click(object sender, EventArgs e)
        {
            pictureBox1.Visible = true;
            pictureBox2.Visible = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Idle -= streaming;
            StudentUSER Stu = new StudentUSER();
            Stu.UserIDlabel.Text = id.Text;
            Stu.greetings_uname.Text = uname.Text;
            Stu.pictureBox1.Image = pictureBox2.Image;
            if (pictureBox2.Image != null)
            {
                // Specify the folder where you want to save the images based on ID
                string destinationFolder = Path.Combine(Application.StartupPath, "Profile", id.Text);

                // Ensure the destination folder exists, create it if not
                if (!Directory.Exists(destinationFolder))
                {
                    Directory.CreateDirectory(destinationFolder);
                }

                // Specify the file path within the destination folder
                string filePath = Path.Combine(destinationFolder, id.Text + "_Image");

                if (File.Exists(filePath))
                {
                    // If it exists, delete the old image
                    File.Delete(filePath);
                }

                // Save the image to the specified location
                pictureBox2.Image.Save(filePath);

                // Update the label with the image path
                Stu.path.Text = filePath;
            }
            Stu.Show();
            this.Close();
        }
    }
}
