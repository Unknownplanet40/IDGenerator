using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IDGenerator
{
    public partial class AdminUSER : Form
    {
        public AdminUSER()
        {
            InitializeComponent();
        }
        

        private void label1_MouseEnter(object sender, EventArgs e)
        {
            SidebarItem1.BackColor = Color.FromArgb(63, 163, 77);
        }

        private void label1_MouseLeave(object sender, EventArgs e)
        {
            SidebarItem1.BackColor = Color.Transparent;
        }

        private void Item2_MouseEnter(object sender, EventArgs e)
        {
            SidebarItem2.BackColor = Color.FromArgb(63, 163, 77);
        }

        private void Item2_MouseLeave(object sender, EventArgs e)
        {
            SidebarItem2.BackColor = Color.Transparent;
        }
    }
}
