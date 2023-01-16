using System;
using System.IO;
using System.Windows.Forms;

namespace ExcelCheckLib
{
    public partial class SelectForm : Form
    {
        private string dataPath = "";
        public string Path { get; private set; }

        public SelectForm()
        {
            InitializeComponent();
        }

        private void SelectForm_Load(object sender, EventArgs e)
        {
            dataPath = Application.StartupPath + @"\data\";
            if (!Directory.Exists(dataPath))
            {
                this.Close();
                return;
            }
            DirectoryInfo directory = new DirectoryInfo(dataPath);
            DirectoryInfo[] directories = directory.GetDirectories();
            FileInfo[] files = directory.GetFiles();
            if (directories != null)
            {
                foreach (DirectoryInfo item in directories)
                {
                    listBox1.Items.Add(item.Name);
                }
            }
            if (files != null)
            {
                foreach (FileInfo item in files)
                {
                    try
                    {
                        string name = item.Name;
                        name = name.Substring(0, name.LastIndexOf('.'));
                        listBox1.Items.Add(name);
                    }
                    catch (Exception) { }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listBox1_MouseDoubleClick(null, null);
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listBox1.SelectedIndex < 0)
            {
                return;
            }
            string name = listBox1.SelectedItem?.ToString();
            Path = dataPath + name;
            if (File.Exists(Path + ".xls"))
            {
                Path += ".xls";
            }
            else if (File.Exists(Path + ".xlsx"))
            {
                Path += ".xlsx";
            }
            else
            {
                string[] files = Directory.GetFiles(Path);
                foreach (string item in files)
                {
                    if (item.LastIndexOf(".xls") > 0 || item.LastIndexOf(".xlsx") > 0)
                    {
                        Path = item;
                        break;
                    }
                }
            }
            this.Close();
        }
    }
}
