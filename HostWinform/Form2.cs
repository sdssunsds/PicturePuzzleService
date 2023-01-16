using System;
using System.Drawing;
using System.Windows.Forms;

namespace HostWinform
{
    public partial class Form2 : Form
    {
        public Image Image { get; set; }

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            pictureBox1.Image = Image;
        }
    }
}
