using GW.XML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LogWin
{
    public partial class Form1 : Form
    {
        private int barMax = 0;
        private float barIndex = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.AutoGenerateColumns = false;
            Bind();
            label1.BringToFront();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Column2.Visible = checkBox1.Checked;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            checkBox3.Enabled = checkBox4.Enabled = checkBox5.Enabled = checkBox6.Enabled = checkBox7.Enabled =
            Column8.Visible = Column3.Visible = Column4.Visible = Column5.Visible = Column6.Visible = Column7.Visible =
            checkBox3.Checked = checkBox4.Checked = checkBox5.Checked = checkBox6.Checked = checkBox7.Checked = checkBox2.Checked;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            Column4.Visible = checkBox3.Checked;
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            Column5.Visible = checkBox4.Checked;
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            Column6.Visible = checkBox5.Checked;
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            Column7.Visible = checkBox6.Checked;
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            Column8.Visible = checkBox7.Checked;
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            Column9.Visible = checkBox8.Checked;
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            Bind();
        }

        private void dataGridView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (dataGridView1.SelectedCells != null && dataGridView1.SelectedCells.Count > 0)
            {
                DataGridViewCell cell = dataGridView1.SelectedCells[0];
                if (cell.ColumnIndex > 0)
                {
                    new Form2()
                    {
                        DateTime = dateTimePicker1.Value.ToString("yyyy-MM-dd"),
                        LogName = dataGridView1.Columns[cell.ColumnIndex].DataPropertyName
                    }.Show(this);
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (barIndex > 0 && barIndex < barMax)
            {
                progressBar1.Value = (int)barIndex;
                label1.Text = (barIndex / barMax * 100).ToString("f2") + "%";
            }
        }

        private void Bind()
        {
            this.Enabled = false;
            dataGridView1.DataSource = null;
            label1.Visible = progressBar1.Visible = timer1.Enabled = true;
            string date = dateTimePicker1.Value.ToString("yyyy-MM-dd");
            string path = Application.StartupPath + "\\Log\\" + date;
            if (Directory.Exists(path))
            {
                Task.Run(() =>
                {
                    List<LogModel> list = new List<LogModel>();
                    string[] fileNames = Directory.GetFiles(path);
                    barMax = fileNames.Length * 1000;
                    Invoke(new Action(() => { progressBar1.Maximum = barMax; }));
                    PropertyInfo[] properties = typeof(LogModel).GetProperties();
                    foreach (string name in fileNames)
                    {
                        if (name.Contains(".xml"))
                        {
                            int type = GetType(path, name);
                            XmlModel xml = new XmlModel(name, true);
                            float range = 1000f / xml["log"].BrotherCount;
                            for (int i = 0; i < xml["log"].BrotherCount; i++)
                            {
                                barIndex += range;
                                XmlModel xm = xml["log"][i];
                                string time = xm["time"];
                                string log = xm["text"];
                                log = log.Replace("&quot;", "\"");
                                string[] vs = time.Split(':');
                                int tv = int.Parse(vs[0]) * 3600 + int.Parse(vs[1]) * 60 + int.Parse(vs[2]);
                                List<LogModel> logs = list.FindAll(l => l.time == time);
                                if (logs != null && logs.Count > 0)
                                {
                                    bool isAdd = false;
                                    foreach (LogModel item in logs)
                                    {
                                        object o = properties[type].GetValue(item, null);
                                        if (o == null || string.IsNullOrEmpty(o.ToString()))
                                        {
                                            properties[type].SetValue(item, log);
                                            isAdd = true;
                                            break;
                                        }
                                    }
                                    if (!isAdd)
                                    {
                                        LogModel _log = new LogModel();
                                        _log.timeValue = tv;
                                        _log.time = time;
                                        properties[type].SetValue(_log, log);
                                        list.Add(_log);
                                    }
                                }
                                else
                                {
                                    LogModel _log = new LogModel();
                                    _log.timeValue = tv;
                                    _log.time = time;
                                    properties[type].SetValue(_log, log);
                                    int index = list.FindIndex(l => l.timeValue > tv);
                                    if (index < 0)
                                    {
                                        list.Add(_log);
                                    }
                                    else
                                    {
                                        list.Insert(index, _log);
                                    }
                                }
                            }
                        }
                    }

                    List<LogModel> bindList = new List<LogModel>();
                    foreach (LogModel item in list)
                    {
                        int index = bindList.FindLastIndex(l => l.time == item.time);
                        if (index < 0 || index == bindList.Count - 1)
                        {
                            bindList.Add(item);
                        }
                        else
                        {
                            bindList.Insert(index + 1, item);
                        }
                    }
                    list = null;

                    Invoke(new Action(() =>
                    {
                        dataGridView1.DataSource = bindList;
                        this.Enabled = true;
                        label1.Visible = progressBar1.Visible = timer1.Enabled = false;
                    }));
                });
            }
            else
            {
                this.Enabled = true;
                label1.Visible = progressBar1.Visible = timer1.Enabled = false;
            }
        }

        private int GetType(string path, string name)
        {
            name = name.Replace(path + "\\", "").Replace(".xml", "");
            switch (name)
            {
                case "error":
                    return 10;
                case "frontLog":
                    return 4;
                case "backLog":
                    return 5;
                case "lidarLog":
                    return 6;
                case "rgvLog":
                    return 8;
                case "depthLog":
                    return 9;
                case "serviceLog":
                    return 2;
                case "lightLog":
                    return 7;
                default:
                    return 3;
            }
        }

        private class LogModel
        {
            public int timeValue { get; set; }
            public string time { get; set; }
            public string service { get; set; }
            public string log { get; set; }
            public string front { get; set; }
            public string back { get; set; }
            public string lidar { get; set; }
            public string light { get; set; }
            public string rgv { get; set; }
            public string depth { get; set; }
            public string error { get; set; }
        }
    }
}
