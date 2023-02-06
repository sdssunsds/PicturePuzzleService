using GW.XML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace LogWin
{
    public partial class Form2 : Form
    {
        private string selectedValue = "";

        public string DateTime { get; set; }
        public string LogName { get; set; }

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            string path = Application.StartupPath + "\\Log\\" + DateTime;
            if (Directory.Exists(path))
            {
                string[] fileNames = Directory.GetFiles(path);
                List<string> logs = new List<string>();
                foreach (string name in fileNames)
                {
                    if (name.Contains(".xml") && name.Contains(LogName))
                    {
                        XmlModel xml = new XmlModel(name, true);
                        for (int i = 0; i < xml["log"].BrotherCount; i++)
                        {
                            XmlModel xm = xml["log"][i];
                            string time = xm["time"];
                            string log = xm["text"];
                            log = log.Replace("&quot;", "\"");
                            logs.Add(time + "\t" + log);
                        }
                        break;
                    }
                }
                listBox1.DataSource = logs;
            }
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int start = listBox1.SelectedIndex + 1;
                if (textBox1.Text != selectedValue)
                {
                    start = 0;
                    selectedValue = textBox1.Text;
                }
                for (int i = start; i < listBox1.Items.Count; i++)
                {
                    if (listBox1.Items[i] != null && listBox1.Items[i].ToString().Contains(selectedValue))
                    {
                        listBox1.SelectedIndex = i;
                        return;
                    }
                }
                MessageBox.Show("检索不到更多的匹配项");
            }
        }
    }
}
