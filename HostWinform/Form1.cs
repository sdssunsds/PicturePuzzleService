using GW.Function.ExcelFunction;
using PicturePuzzleService;
using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing.Imaging;
using System.Drawing;
using ICSharpCode.SharpZipLib.Zip;
using ExcelCheckLib;
using System.Net;

namespace HostWinform
{
    /// <summary>
    /// 宿主程序
    /// </summary>
    public partial class Form1 : Form
    {
        private const int WM_QUERYENDSESSION = 0x0011;//系统发送的关机命令
        private static string servicePath = Properties.Settings.Default.ServicePath;
        private int isClose = 0;
        private ServiceHost host = null;
        private Bitmap puzzleMap = null;
        private Graphics puzzleG = null;
        private ExcelModel excel = null;

        public Form1()
        {
            InitializeComponent();
            PicService.UploadPath = Properties.Settings.Default.UploadPath;
            PicService.PuzzlePath = Properties.Settings.Default.PuzzlePath;
            PicService.ZipPath = Properties.Settings.Default.ZipPath;
            PicService.PuzzleExeConfig = Properties.Settings.Default.PuzzleEXEConfig;
            PicService.HeightFunc = new Func<int[]>(() =>
            {
                int[] height = new int[8];
                try
                {
                    for (int i = 0; i < height.Length; i++)
                    {
                        height[i] = int.Parse(excel[1][0][i].Value);
                    }
                }
                catch (Exception e)
                {
                    using (StreamWriter sw = new StreamWriter(Properties.Settings.Default.PuzzlePath + "Log.txt", true))
                    {
                        sw.WriteLine("PicService.HeightFunc: " + e.Message);
                    }
                }
                return height;
            });
            PicService.DataFunc = new Func<string, inputrect_4[]>((string train) =>
            {
                string path = Application.StartupPath + @"\data\";
                List<inputrect_4> data = new List<inputrect_4>();
                if (File.Exists(path + train + ".xls"))
                {
                    path += train + ".xls";
                    excel = new ExcelModel(path);
                }
                else if (File.Exists(path + train + ".xlsx"))
                {
                    path += train + ".xlsx";
                    excel = new ExcelModel(path);
                }
                else if (Directory.Exists(path + train))
                {
                    string[] files = Directory.GetFiles(path + train);
                    foreach (string file in files)
                    {
                        if (file.Contains(".xls"))
                        {
                            path = file;
                            excel = new ExcelModel(file);
                            break;
                        }
                    }
                }
                else
                {
                    return data.ToArray();
                }
                try
                {
                    for (int i = 2; i < excel[0].RowCount; i++)
                    {
                        if (string.IsNullOrEmpty(excel[0][i][0].Value))
                        {
                            break;
                        }
                        inputrect_4 model = new inputrect_4(
                            int.Parse(excel[0][i][2].Value), excel[0][i][3].Value,
                            excel[0][i][4].Value, excel[0][i][6].Value,
                            "6" + excel[0][i][2].Value + excel[0][i][3].Value + excel[0][i][4].Value.Substring(1) + excel[0][i][6].Value + excel[0][i][25].Value + excel[0][i][26].Value + excel[0][i][27].Value,
                            int.Parse(excel[0][i][19].Value), int.Parse(excel[0][i][20].Value),
                            int.Parse(excel[0][i][21].Value), int.Parse(excel[0][i][22].Value)
                            );
                        data.Add(model);
                    }
                }
                catch (Exception e)
                {
                    using (StreamWriter sw = new StreamWriter(Properties.Settings.Default.PuzzlePath + "Log.txt", true))
                    {
                        sw.WriteLine("Error: " + train + "\r\n" + "Excel: " + path + "\r\n" + e.Message);
                    }
                }
                return data.ToArray();
            });
        }

        protected override void WndProc(ref Message m)//此方法用于处理Windows消息
        {
            switch (m.Msg)//获取消息值
            {
                case WM_QUERYENDSESSION:
                    m.Result = (IntPtr)isClose;//为了响应消息处理，设置返回值
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            PicService.BakPath = Application.StartupPath + "\\Bak\\";
            if (!Directory.Exists(PicService.BakPath))
            {
                Directory.CreateDirectory(PicService.BakPath);
            }
            if (!Directory.Exists(PicService.BackBakPath))
            {
                Directory.CreateDirectory(PicService.BackBakPath);
            }
            if (!Directory.Exists(PicService.FrontBakPath))
            {
                Directory.CreateDirectory(PicService.FrontBakPath);
            }
            string modeSnPath = Application.StartupPath + "\\mode_sn.txt";
            if (File.Exists(modeSnPath))
            {
                using (StreamReader sr = new StreamReader(modeSnPath))
                {
                    string txt = sr.ReadToEnd();
                    string[] modeSn = txt.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    if (modeSn.Length > 0)
                    {
                        cb_mode.Items.AddRange(modeSn[0].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                    }
                    if (modeSn.Length > 1)
                    {
                        cb_sn.Items.AddRange(modeSn[1].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                    }
                }
            }
            if (!Directory.Exists(Application.StartupPath + "\\Log"))
            {
                Directory.CreateDirectory(Application.StartupPath + "\\Log");
            }
            Task.Run(() =>
            {
                do
                {
                    if (DateTime.Now.Hour == 0)
                    {
                        DeleteFiles(Directory.GetFiles(PicService.BakPath));
                        DeleteFiles(Directory.GetFiles(PicService.BackBakPath));
                        DeleteFiles(Directory.GetFiles(PicService.FrontBakPath));
                    }
                    Thread.Sleep(3600000);
                } while (true);
            });
            // 以下为测试拼图逻辑的最大拼接数量
#if false
            string id = "__2021_08_08_06_42";
            Dictionary<string, List<int>> savePicArray = new Dictionary<string, List<int>>();
            savePicArray.Add(id, new List<int>());
            for (int i = 0; i < 241; i++)
            {
                savePicArray[id].Add(10000 + i);
            }
            int count = 15;
            int length = savePicArray[id].Count / count + (savePicArray[id].Count % count > 0 ? 1 : 0);
            for (int i = 0; i < length; i++)
            {
                Mat[] mats = new Mat[count];
                for (int j = 0; j < count; j++)
                {
                    int index = i * count + j;
                    if (index < savePicArray[id].Count)
                    {
                        mats[j] = Cv2.ImRead(@"D:\PuzzleService\Upload\" + id + @"\" + savePicArray[id][index] + ".jpg");
                    }
                }
                Mat puzzleMat = new Mat();
                Cv2.VConcat(mats, puzzleMat);
                puzzleMat.ImWrite(@"D:\PuzzleService\Puzzle\" + id + ".jpg");

                foreach (Mat item in mats)
                {
                    item.Dispose();
                }
                puzzleMat.Dispose();
            } 
#endif
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (new Form3().ShowDialog(this) == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            host?.Close();
            isClose = 1;//使消息值等于1，实现允许关机
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            button1_Click(null, null);
            PicService.SaveLog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (host == null)
            {
                host = new ServiceHost(typeof(PicService));

                //绑定
                BasicHttpBinding binding = new BasicHttpBinding();
                binding.MaxBufferSize = 2147483647;
                binding.MaxReceivedMessageSize = 2147483647;
                //终结点
                host.AddServiceEndpoint(typeof(IService), binding, servicePath);
                if (host.Description.Behaviors.Find<ServiceMetadataBehavior>() == null)
                {
                    //行为
                    ServiceMetadataBehavior behavior = new ServiceMetadataBehavior();
                    behavior.HttpGetEnabled = true;

                    //元数据地址
                    behavior.HttpGetUrl = new Uri(servicePath + "PicService");
                    host.Description.Behaviors.Add(behavior);

                    //启动
                    host.Open();
                    AddLog("服务已打开");
                    MessageBox.Show("服务已打开");
                }
            }
            else
            {
                MessageBox.Show("服务已打开");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            AddLog("开始重新拼图");
            Dictionary<string, FileInfo[]> uploads = new Dictionary<string, FileInfo[]>();
            int count = GetDict(Properties.Settings.Default.UploadPath, uploads);

            button4.Enabled = false;
            progressBar1.Value = 0;
            progressBar1.Maximum = count;

            Task.Run(() =>
            {
                foreach (KeyValuePair<string, FileInfo[]> item in uploads)
                {
                    string puzzlePath = Properties.Settings.Default.PuzzlePath + item.Key;
                    string zipPath = string.Format(Properties.Settings.Default.ZipPath, item.Key);
                    Directory.CreateDirectory(puzzlePath);

                    int _length = item.Value.Length % 8;
                    int[] lengths = new int[8];
                    for (int i = 0; i < lengths.Length; i++)
                    {
                        lengths[i] = item.Value.Length / 8;
                        if (_length > 0)
                        {
                            lengths[i]++;
                            _length--;
                        }
                    }

                    int index = 0;
                    int puzzleIndex = 0;
                    string[] vs = item.Key.Split('_');
                    string puzzleID = vs[0] + "_" + vs[1] + "_" + "{0}_" + vs[vs.Length - 1];
                    foreach (FileInfo file in item.Value)
                    {
                        if (puzzleMap == null)
                        {
                            puzzleMap = new Bitmap(2048, lengths[index] * 1130);
                            puzzleG = Graphics.FromImage(puzzleMap);
                            puzzleIndex = 0;
                        }

                        PuzzleImage(file.FullName, puzzleIndex);
                        puzzleIndex++;
                        if (puzzleIndex >= lengths[index])
                        {
                            CompressImg(puzzlePath + "\\" + string.Format(puzzleID, index + 1) + ".jpg", puzzleMap, 100);
                            puzzleG.Dispose();
                            puzzleG = null;
                            puzzleMap.Dispose();
                            puzzleMap = null;
                            index++;
                        }

                        Invoke(new Action(() =>
                        {
                            progressBar1.Value++;
                        }));
                    }

                    CreateZipFile(puzzlePath, zipPath);
                }
                MessageBox.Show("重拼图完成");
                Invoke(new Action(() =>
                {
                    button4.Enabled = true;
                }));
            });
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string path = folderBrowserDialog.SelectedPath;
                AddLog("图片导出——" + path);
                Dictionary<string, FileInfo[]> uploads = new Dictionary<string, FileInfo[]>();
                Dictionary<string, FileInfo[]> puzzles = new Dictionary<string, FileInfo[]>();
                Dictionary<string, FileInfo> zips = new Dictionary<string, FileInfo>();

                int count = GetDict(Properties.Settings.Default.UploadPath, uploads);
                count += GetDict(Properties.Settings.Default.PuzzlePath, puzzles);
                count += GetDict(Properties.Settings.Default.ZipPath.Substring(0, Properties.Settings.Default.ZipPath.LastIndexOf("\\")), zips);

                progressBar1.Value = 0;
                progressBar1.Maximum = count;
                Task.Run(() =>
                {
                    foreach (KeyValuePair<string, FileInfo[]> item in uploads)
                    {
                        string savePath = path + "\\" + item.Key;
                        string uploadPath = savePath + "\\upload\\";
                        string puzzlePath = savePath + "\\puzzle\\";
                        string zipPath = savePath + "\\zip\\";
                        Directory.CreateDirectory(savePath);
                        Directory.CreateDirectory(uploadPath);
                        Directory.CreateDirectory(puzzlePath);
                        Directory.CreateDirectory(zipPath);

                        foreach (FileInfo file in item.Value)
                        {
                            File.Copy(file.FullName, uploadPath + file.Name);
                            Invoke(new Action(() =>
                            {
                                progressBar1.Value++;
                            }));
                        }

                        if (puzzles.ContainsKey(item.Key))
                        {
                            foreach (FileInfo file in puzzles[item.Key])
                            {
                                File.Copy(file.FullName, puzzlePath + file.Name);
                                Invoke(new Action(() =>
                                {
                                    progressBar1.Value++;
                                }));
                            }
                            puzzles.Remove(item.Key);
                        }

                        if (zips.ContainsKey(item.Key))
                        {
                            File.Copy(zips[item.Key].FullName, zipPath + zips[item.Key].Name);
                            Invoke(new Action(() =>
                            {
                                progressBar1.Value++;
                            }));
                            zips.Remove(item.Key);
                        }
                    }
                    MessageBox.Show("导出完成");
                });
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            progressBar1.Value = 0;
            progressBar1.Maximum = 8;
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string path = folderBrowserDialog.SelectedPath;
                string name = path.Substring(path.LastIndexOf(@"\") + 1);
                AddLog("C++拼图——" + path);
                if (name.Split('_').Length < 8)
                {
                    MessageBox.Show("路径不合法");
                }
                else
                {
                    new PicService().UploadComplete(name.Substring(0, name.LastIndexOf("_")), name.Substring(name.LastIndexOf("_") + 1), 0);
                    button2.Enabled = true;
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            AddLog("检查Excel");
            new CheckForm().Show(this);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cb_mode.Text) || string.IsNullOrEmpty(cb_sn.Text))
            {
                MessageBox.Show("未设置车型车号"); 
                return;
            }
            AddLog("轴定位");
            string upID = DateTime.Now.ToString("yyyy_MM_dd_HH_mm") + "_00000001";
            if (!Directory.Exists(Properties.Settings.Default.UploadPath + upID))
            {
                Directory.CreateDirectory(Properties.Settings.Default.UploadPath + upID);
            }
            this.Enabled = false;
            progressBar1.Value = 0;
            progressBar1.Maximum = flowLayoutPanel2.Controls.Count;
            string mode = cb_mode.Text;
            string sn = cb_sn.Text;
            new Thread(new ThreadStart(() =>
            {
                Form4 form = new Form4();
                form.ShowDialog(this);
                foreach (Control control in flowLayoutPanel2.Controls)
                {
                    if ((control as CheckBox).Checked)
                    {
                        form.Addlog($"车型：{mode}  车号：{sn}");
                        string path = control.Tag.ToString();
                        string name = path.Substring(path.LastIndexOf('\\') + 1);
                        string part = control.Text;
                        form.Addlog($"上传图片路径：{path}");
                        form.Addlog($"上传部件编号：{part}");
                        string copy = Properties.Settings.Default.UploadPath + upID + "\\" + name;
                        File.Copy(path, copy);
                        form.Addlog($"上传图片拷贝>>{copy}");
                        string url = "http://192.168.0.102:20001/img/Upload/" + upID + "/" + name;
                        form.Addlog($"上传图片位置：{url}");
                        UploadImage(url, part, mode, sn, (string log) => { form.Addlog(log); });
                        Invoke(new Action(() => { progressBar1.Value++; }));
                        Thread.Sleep(10);
                    }
                }
                form.Close();
                MessageBox.Show("上传完成");
                Invoke(new Action(() => { this.Enabled = true; }));
            })).Start();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                this.Enabled = false;
                flowLayoutPanel2.Controls.Clear();
                string path = folderBrowserDialog.SelectedPath;
                string[] jpgs = Directory.GetFiles(path, "*.jpg");
                string[] pngs = Directory.GetFiles(path, "*.png");
                string[] bmps = Directory.GetFiles(path, "*.bmp");
                progressBar1.Value = 0;
                progressBar1.Maximum = jpgs.Length + pngs.Length + bmps.Length;
                new Thread(new ThreadStart(() =>
                {
                    foreach (string item in jpgs)
                    {
                        AddControl(item);
                        Invoke(new Action(() => { progressBar1.Value++; }));
                    }
                    foreach (string item in pngs)
                    {
                        AddControl(item);
                        Invoke(new Action(() => { progressBar1.Value++; }));
                    }
                    foreach (string item in bmps)
                    {
                        AddControl(item);
                        Invoke(new Action(() => { progressBar1.Value++; }));
                    }
                    Invoke(new Action(() => { this.Enabled = true; }));
                })).Start();
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            string dir = Properties.Settings.Default.UploadPath + "test_0000000\\Location\\";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "红外图|*.bmp";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string path = openFileDialog.FileName;
                string name1 = new FileInfo(path).Name;
                if (File.Exists(dir + name1))
                {
                    File.Delete(dir + name1);
                }
                File.Copy(path, dir + name1);
                openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "深度图|*.png";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    path = openFileDialog.FileName;
                    string name2 = new FileInfo(path).Name;
                    if (File.Exists(dir + name2))
                    {
                        File.Delete(dir + name2);
                    }
                    File.Copy(path, dir + name2);
                    int state = 0;
                    if (name1[3] != '1')
                    {
                        state = 1;
                    }
                    MessageBox.Show(new PicService().GetLocation("test", name1, name2, "", "0000000", state).ToString());
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            foreach (Control item in flowLayoutPanel2.Controls)
            {
                (item as CheckBox).Checked = checkBox1.Checked;
            }
        }

        private void Cb_MouseClick(object sender, MouseEventArgs e)
        {
            string path = (sender as Control).Tag.ToString();
            if (pictureBox1.Tag == null || pictureBox1.Tag.ToString() != path)
            {
                try
                {
                    pictureBox1.Image = Image.FromFile(path);
                    pictureBox1.Tag = path;
                }
                catch (Exception) { }
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                new Form2() { Image = pictureBox1.Image }.Show(); 
            }
        }

        private void AddControl(string imgPath)
        {
            string name = imgPath.Substring(imgPath.LastIndexOf('\\') + 1);
            string part = name.Substring(0, name.IndexOf('.'));
            Invoke(new Action(() =>
            {
                CheckBox cb = new CheckBox();
                cb.Text = part;
                cb.Tag = imgPath;
                cb.AutoSize = true;
                cb.MouseClick += Cb_MouseClick;
                flowLayoutPanel2.Controls.Add(cb);
            }));
        }

        private void AddLog(string log)
        {
            PicService.SaveLog(log, 6);
        }

        private void CompressImg(string path, Image img, int quality)
        {
            int i = 0;
        SaveImage:
            try
            {
                EncoderParameter qualityParam = new EncoderParameter(Encoder.Quality, quality);
                ImageCodecInfo jpegCodec = null;
                ImageCodecInfo[] codes = ImageCodecInfo.GetImageEncoders();
                for (int j = 0; j < codes.Length; j++)
                {
                    if (codes[j].MimeType == "image/jpeg")
                    {
                        jpegCodec = codes[j];
                        break;
                    }
                }

                EncoderParameters encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = qualityParam;
                img.Save(path, jpegCodec, encoderParams);
            }
            catch (Exception)
            {
                if (i < 10)
                {
                    Thread.Sleep(1000);
                    i++;
                    goto SaveImage;
                }
            }
        }

        private void CreateZipFile(string filesPath, string zipFilePath)
        {
            try
            {
                string[] filenames = Directory.GetFiles(filesPath);
                using (ZipOutputStream s = new ZipOutputStream(File.Create(zipFilePath)))
                {

                    s.SetLevel(9);
                    byte[] buffer = new byte[4096];
                    foreach (string file in filenames)
                    {
                        ZipEntry entry = new ZipEntry(Path.GetFileName(file));
                        entry.DateTime = DateTime.Now;
                        s.PutNextEntry(entry);
                        using (FileStream fs = File.OpenRead(file))
                        {
                            int sourceBytes;
                            do
                            {
                                sourceBytes = fs.Read(buffer, 0, buffer.Length);
                                s.Write(buffer, 0, sourceBytes);
                            } while (sourceBytes > 0);
                        }
                    }
                    s.Finish();
                    s.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception during processing {0}", ex);
            }
        }

        private void DeleteFiles(string[] files)
        {
            try
            {
                foreach (string item in files)
                {
                    if (File.Exists(item))
                    {
                        File.Delete(item);
                    }
                }
            }
            catch (Exception e)
            {
                AddLog(e.Message);
            }
        }

        private int GetDict(string path, Dictionary<string, FileInfo[]> dict)
        {
            int count = 0;
            string date = dateTimePicker1.Value.ToString("yyyy_MM_dd");
            DirectoryInfo directory = new DirectoryInfo(path);
            DirectoryInfo[] directories = directory.GetDirectories();
            foreach (DirectoryInfo item in directories)
            {
                if (item.Name.Contains(date))
                {
                    FileInfo[] files = item.GetFiles();
                    foreach (FileInfo file in files)
                    {
                        int i;
                        if (int.TryParse(file.Name.Replace(".jpg", ""), out i) && i < 10000)
                        {
                            File.Move(file.FullName, item.FullName + "\\" + (10000 + i) + ".jpg");
                        }
                    }
                    dict.Add(item.Name, item.GetFiles());
                    count += dict[item.Name].Length; 
                }
            }
            return count;
        }

        private int GetDict(string path, Dictionary<string, FileInfo> dict)
        {
            int count = 0;
            string date = dateTimePicker1.Value.ToString("yyyy_MM_dd");
            DirectoryInfo directory = new DirectoryInfo(path);
            FileInfo[] files = directory.GetFiles();
            foreach (FileInfo item in files)
            {
                if (item.Name.Contains(date))
                {
                    dict.Add(item.Name.Substring(0, item.Name.LastIndexOf(".")), item);
                    count++; 
                }
            }
            return count;
        }

        private void PuzzleImage(string imagePath, int index)
        {
            if (puzzleG != null)
            {
                Image image = Image.FromFile(imagePath);
                puzzleG.DrawImage(image, new Rectangle(0, 1130 * index, puzzleMap.Width, 1130), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
                image.Dispose();
            }
        }

        private void UploadImage(string path, string part, string mode, string sn, Action<string> addlog)
        {
            string strURL = Properties.Settings.Default.UploadDataServer + "/planMalfunctionManagement/auth/add" +
                string.Format("?abnormalPhoto={0}&uniqueNumber={1}&motorCarModel={2}&motorCarNumber={3}&componentNumber={4}",
                path, "00000001", mode, sn, part);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strURL);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded;charset=gb2312";
            // 编码格式
            byte[] payload = System.Text.Encoding.UTF8.GetBytes("");
            request.ContentLength = payload.Length;
            Stream writer;
            try
            {
                writer = request.GetRequestStream();
            }
            catch (Exception ex)
            {
                writer = null;
                addlog("连接服务器失败[HTTP上传面阵图片]: " + ex.Message);
                return;
            }
            writer.Write(payload, 0, payload.Length);
            writer.Close();
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                response = ex.Response as HttpWebResponse;
            }
            Stream s = response.GetResponseStream();
            StreamReader sRead = new StreamReader(s);
            string postContent = sRead.ReadToEnd();
            sRead.Close();
            addlog($"HTTP请求：{strURL}");
            addlog($"HTTP返回：{postContent}");
        }
    }
}
