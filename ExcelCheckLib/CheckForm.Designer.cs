
namespace ExcelCheckLib
{
    partial class CheckForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dataGridView2 = new System.Windows.Forms.DataGridView();
            this.ben_sel = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btn_save = new System.Windows.Forms.Button();
            this.lb_num = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.ben_loc = new System.Windows.Forms.Button();
            this.ben_check = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(800, 412);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.progressBar1);
            this.tabPage1.Controls.Add(this.dataGridView1);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(792, 383);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "无数据";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(83, 181);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(644, 23);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar1.TabIndex = 1;
            this.progressBar1.Visible = false;
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(3, 3);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.dataGridView1.RowTemplate.Height = 27;
            this.dataGridView1.Size = new System.Drawing.Size(786, 377);
            this.dataGridView1.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.dataGridView2);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(792, 383);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "无数据";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // dataGridView2
            // 
            this.dataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView2.Location = new System.Drawing.Point(3, 3);
            this.dataGridView2.Name = "dataGridView2";
            this.dataGridView2.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.dataGridView2.RowTemplate.Height = 27;
            this.dataGridView2.Size = new System.Drawing.Size(786, 377);
            this.dataGridView2.TabIndex = 1;
            // 
            // ben_sel
            // 
            this.ben_sel.Location = new System.Drawing.Point(12, 6);
            this.ben_sel.Name = "ben_sel";
            this.ben_sel.Size = new System.Drawing.Size(114, 25);
            this.ben_sel.TabIndex = 1;
            this.ben_sel.Text = "选择Excel";
            this.ben_sel.UseVisualStyleBackColor = true;
            this.ben_sel.Click += new System.EventHandler(this.ben_sel_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btn_save);
            this.panel1.Controls.Add(this.lb_num);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.ben_loc);
            this.panel1.Controls.Add(this.ben_check);
            this.panel1.Controls.Add(this.ben_sel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 412);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(800, 38);
            this.panel1.TabIndex = 2;
            // 
            // btn_save
            // 
            this.btn_save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_save.Location = new System.Drawing.Point(679, 6);
            this.btn_save.Name = "btn_save";
            this.btn_save.Size = new System.Drawing.Size(114, 25);
            this.btn_save.TabIndex = 6;
            this.btn_save.Text = "刷新Excel";
            this.btn_save.UseVisualStyleBackColor = true;
            this.btn_save.Click += new System.EventHandler(this.btn_refrish_Click);
            // 
            // lb_num
            // 
            this.lb_num.AutoSize = true;
            this.lb_num.Location = new System.Drawing.Point(370, 11);
            this.lb_num.Name = "lb_num";
            this.lb_num.Size = new System.Drawing.Size(15, 15);
            this.lb_num.TabIndex = 5;
            this.lb_num.Text = "0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(252, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(112, 15);
            this.label1.TabIndex = 4;
            this.label1.Text = "格式异常数量：";
            // 
            // ben_loc
            // 
            this.ben_loc.Location = new System.Drawing.Point(461, 6);
            this.ben_loc.Name = "ben_loc";
            this.ben_loc.Size = new System.Drawing.Size(114, 25);
            this.ben_loc.TabIndex = 3;
            this.ben_loc.Text = "定位异常位置";
            this.ben_loc.UseVisualStyleBackColor = true;
            this.ben_loc.Click += new System.EventHandler(this.ben_loc_Click);
            // 
            // ben_check
            // 
            this.ben_check.Location = new System.Drawing.Point(132, 6);
            this.ben_check.Name = "ben_check";
            this.ben_check.Size = new System.Drawing.Size(114, 25);
            this.ben_check.TabIndex = 2;
            this.ben_check.Text = "检查Excel";
            this.ben_check.UseVisualStyleBackColor = true;
            this.ben_check.Click += new System.EventHandler(this.ben_check_Click);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // CheckForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.panel1);
            this.Name = "CheckForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "检查Excel";
            this.Load += new System.EventHandler(this.CheckForm_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridView dataGridView2;
        private System.Windows.Forms.Button ben_sel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btn_save;
        private System.Windows.Forms.Label lb_num;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button ben_loc;
        private System.Windows.Forms.Button ben_check;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Timer timer1;
    }
}