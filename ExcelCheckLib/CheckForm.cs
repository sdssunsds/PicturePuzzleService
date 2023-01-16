using GW.Function.ExcelFunction;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExcelCheckLib
{
    public partial class CheckForm : Form
    {
        private string selectedPath = "";
        private ExcelModel excel = null;
        private int[] errorSelected = { 0, 0 };
        private Dictionary<int, List<int>> sheet1Error = new Dictionary<int, List<int>>();
        private Dictionary<int, List<int>> sheet2Error = new Dictionary<int, List<int>>();

        public CheckForm()
        {
            InitializeComponent();
        }

        private void CheckForm_Load(object sender, EventArgs e)
        {
            SelectForm form = new SelectForm();
            form.ShowDialog(this);
            if (string.IsNullOrEmpty(form.Path))
            {
                this.Close();
                return;
            }
            SetupExcel(form.Path);
        }

        private void ben_sel_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel文件|*.xls;*.xlsx";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                SetupExcel(openFileDialog.FileName);
            }
        }

        private void ben_check_Click(object sender, EventArgs e)
        {
            sheet1Error.Clear();
            sheet2Error.Clear();

            ExcelSheet sheet1 = excel[0];
            for (int i = 0; i < sheet1.RowCount; i++)
            {
                CheckIntValue(sheet1, i, 2, sheet1Error);
                CheckStrValue(sheet1, i, 3, sheet1Error);
                CheckStrValue(sheet1, i, 4, sheet1Error);
                CheckStrValue(sheet1, i, 6, sheet1Error);
                CheckIntValue(sheet1, i, 19, sheet1Error);
                CheckIntValue(sheet1, i, 20, sheet1Error);
                CheckIntValue(sheet1, i, 21, sheet1Error);
                CheckIntValue(sheet1, i, 22, sheet1Error);
            }
            if (excel.SheetCout > 1)
            {
                ExcelSheet sheet2 = excel[1];
                for (int i = 0; i < sheet2.RowCount; i++)
                {
                    for (int j = 0; j < sheet2[i].CellCount; j++)
                    {
                        CheckIntValue(sheet2, i, j, sheet2Error);
                    }
                }
            }
            int errorCount = 0;
            foreach (KeyValuePair<int, List<int>> item in sheet1Error)
            {
                errorCount += item.Value.Count;
                foreach (int c in item.Value)
                {
                    dataGridView1.Rows[item.Key].Cells[c].Style.BackColor = Color.Red; 
                }
            }
            foreach (KeyValuePair<int, List<int>> item in sheet2Error)
            {
                errorCount += item.Value.Count;
                foreach (int c in item.Value)
                {
                    dataGridView2.Rows[item.Key].Cells[c].Style.BackColor = Color.Red;
                }
            }
            lb_num.Text = errorCount.ToString();
        }

        private void ben_loc_Click(object sender, EventArgs e)
        {
            int selId = 0;
            int listCount = 0;
            DataGridView dgv = null;
            Dictionary<int, List<int>> sheetError = null;
            if (tabControl1.SelectedIndex == 0)
            {
                dgv = dataGridView1;
                sheetError = sheet1Error;
            }
            else
            {
                selId = 1;
                dgv = dataGridView2;
                sheetError = sheet2Error;
            }

            bool isFindCell = false;
            int row = -1, cell = -1;
            if (sheetError.Count == 0)
            {
                return;
            }
            foreach (KeyValuePair<int, List<int>> item in sheetError)
            {
                if (row < 0)
                {
                    row = item.Key;
                }
                if (cell < 0 && item.Value.Count > 0)
                {
                    cell = item.Value[0];
                }
                if (errorSelected[selId] < item.Value.Count + listCount)
                {
                    row = item.Key;
                    cell = item.Value[errorSelected[selId] - listCount];
                    errorSelected[selId]++;
                    isFindCell = true;
                    break;
                }
                listCount += item.Value.Count;
            }
            if (!isFindCell)
            {
                errorSelected[selId] = 1;
            }

            if (cell > -1)
            {
                dgv.CurrentCell = dgv.Rows[row].Cells[cell];
            }
        }

        private void btn_refrish_Click(object sender, EventArgs e)
        {
            SetupExcel(selectedPath);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (progressBar1.Maximum > progressBar1.Value)
            {
                progressBar1.Value++;
            }
            else
            {
                progressBar1.Value = 0;
            }
        }

        private string AddColumn(int i)
        {
            if (i > 25)
            {
                int first = i / 26 - 1;
                int end = i % 26;
                string s = AddColumn(first);
                s += AddColumn(end);
                return s;
            }
            switch (i)
            {
                case 1:
                    return "B";
                case 2:
                    return "C";
                case 3:
                    return "D";
                case 4:
                    return "E";
                case 5:
                    return "F";
                case 6:
                    return "G";
                case 7:
                    return "H";
                case 8:
                    return "I";
                case 9:
                    return "J";
                case 10:
                    return "K";
                case 11:
                    return "L";
                case 12:
                    return "M";
                case 13:
                    return "N";
                case 14:
                    return "O";
                case 15:
                    return "P";
                case 16:
                    return "Q";
                case 17:
                    return "R";
                case 18:
                    return "S";
                case 19:
                    return "T";
                case 20:
                    return "U";
                case 21:
                    return "V";
                case 22:
                    return "W";
                case 23:
                    return "X";
                case 24:
                    return "Y";
                case 25:
                    return "Z";
                default:
                    return "A";
            }
        }

        private void AddError(int row, int cell, Dictionary<int, List<int>> sheetError)
        {
            if (!sheetError.ContainsKey(row))
            {
                sheetError.Add(row, new List<int>());
            }
            sheetError[row].Add(cell);
        }

        private void CheckIntValue(ExcelSheet sheet, int row, int cell, Dictionary<int, List<int>> sheetError)
        {
            if (!int.TryParse(sheet[row][cell].Value, out _))
            {
                AddError(row, cell, sheetError);
            }
        }

        private void CheckStrValue(ExcelSheet sheet, int row, int cell, Dictionary<int, List<int>> sheetError)
        {
            if (string.IsNullOrEmpty(sheet[row][cell].Value))
            {
                AddError(row, cell, sheetError);
            }
        }

        private void SetupExcel(string path)
        {
            lb_num.Text = "0";
            dataGridView1.DataSource = dataGridView2.DataSource = null;
            progressBar1.Maximum = 100;
            progressBar1.Visible = timer1.Enabled = true;
            selectedPath = path;
            excel = null;
            Task.Run(() =>
            {
                try
                {
                    excel = new ExcelModel(path);
                    DataSet ds = new DataSet();
                    ds.Tables.Add(new DataTable());
                    ds.Tables.Add(new DataTable());
                    for (int i = 0; i < excel.SheetCout && i < tabControl1.TabPages.Count; i++)
                    {
                        try
                        {
                            this.Invoke(new Action(() => { tabControl1.TabPages[i].Text = excel[i].Name; }));
                            for (int j = 0; j < excel[i].RowCount; j++)
                            {
                                Thread.Sleep(10);
                                try
                                {
                                    if (excel[i][j].CellCount > ds.Tables[i].Columns.Count)
                                    {
                                        int len = excel[i][j].CellCount - ds.Tables[i].Columns.Count;
                                        int max = ds.Tables[i].Columns.Count;
                                        for (int n = 0; n < len; n++)
                                        {
                                            ds.Tables[i].Columns.Add(AddColumn(n + max));
                                        }
                                    }
                                    DataRow dr = ds.Tables[i].NewRow();
                                    for (int k = 0; k < excel[i][j].CellCount; k++)
                                    {
                                        dr[k] = excel[i][j][k].Value;
                                    }
                                    ds.Tables[i].Rows.Add(dr);
                                }
                                catch (Exception e)
                                {
                                    MessageBox.Show("Sheet[" + i + "] - Row[" + j + "]" + e.Message);
                                    return;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("Sheet[" + i + "]" + e.Message);
                            return;
                        }
                    }
                    this.Invoke(new Action(() =>
                    {
                        dataGridView1.DataSource = ds.Tables[0];
                        dataGridView2.DataSource = ds.Tables[1];
                    }));
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        this.Invoke(new Action(() => { dataGridView1.Rows[i].HeaderCell.Value = (i + 1).ToString(); }));
                    }
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        this.Invoke(new Action(() => { dataGridView2.Rows[i].HeaderCell.Value = (i + 1).ToString(); }));
                    }
                    this.Invoke(new Action(() =>
                    {
                        progressBar1.Visible = timer1.Enabled = false;
                    }));
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            });
        }
    }
}
