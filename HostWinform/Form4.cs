using System.Windows.Forms;

namespace HostWinform
{
    public partial class Form4 : Form
    {
        private int logCount = 0;

        public Form4()
        {
            InitializeComponent();
        }

        public void Addlog(string log)
        {
            logCount++;
            if (logCount > 500)
            {
                logCount = 0;
                textBox1.Text = "";
            }
            textBox1.Text += log + "\r\n";
            textBox1.SelectionStart = textBox1.Text.Length - 1;
            textBox1.ScrollToCaret();
        }
    }
}
