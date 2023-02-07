#define puzzleC  // 使用C++拼图

using ICSharpCode.SharpZipLib.Zip;
using GW.XML;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PicturePuzzleService
{
    public class PicService : IService
    {
        public bool complete1 = false;
        public bool complete2 = false;
        public bool complete3 = false;

        private static object lock_logXml = new object();
        private static object lock_errorXml = new object();
        private static object lock_frontXml = new object();
        private static object lock_backXml = new object();
        private static object lock_lidarXml = new object();
        private static object lock_rgvXml = new object();
        private static object lock_depthXml = new object();
        private static object lock_serviceXml = new object();
        private static object lock_lightXml = new object();
        private static DateTime saveTime = DateTime.Now;
        private static XmlModel logXml = null;
        private static XmlModel error = null;
        private static XmlModel frontLog = null;
        private static XmlModel backLog = null;
        private static XmlModel lidarLog = null;
        private static XmlModel rgvLog = null;
        private static XmlModel depthLog = null;
        private static XmlModel serviceLog = null;
        private static XmlModel lightLog = null;
#if puzzleC
        private static bool locationInitComplete = false;
        private static bool puzzleInitComplete = false;
        private static IntPtr obj = IntPtr.Zero;
        private static IntPtr objLocation = IntPtr.Zero;
        public static Func<int[]> HeightFunc;
        public static Func<string, inputrect_4[]> DataFunc;
#endif
        private static Dictionary<string, List<int>> savePicArray = null;
        private static Dictionary<string, List<string>> savePicArray2 = null;
        private static Dictionary<int, Dictionary<int, byte[]>> picDataRom = null;
        private static Dictionary<string, Dictionary<int, byte[]>> picDataRom2 = null;
        private static Dictionary<string, Dictionary<int, byte[]>> picDataRom3 = null;
        private static Dictionary<string, Dictionary<RobotName, Dictionary<int, byte[]>>> picDataRoms = null;

        /// <summary>
        /// 每个待拼接图片的默认宽度
        /// </summary>
        public static int DefaultWidth { get; set; } = 4096;
        /// <summary>
        /// 每个待拼接图片的默认高度
        /// </summary>
        public static int DefaultHeight { get; set; } = 2260;
        /// <summary>
        /// 每次拼图的拼接数目（0代表压缩拼图方式）
        /// </summary>
        public static int PuzzleCount { get; set; } = 0;
        /// <summary>
        /// 上传路径
        /// </summary>
        public static string UploadPath { get; set; } = @"D:\PuzzleService\Upload\";
        /// <summary>
        /// 拼图路径
        /// </summary>
        public static string PuzzlePath { get; set; } = @"D:\PuzzleService\Puzzle\";
        /// <summary>
        /// 压缩后路径
        /// </summary>
        public static string ZipPath { get; set; } = @"D:\PuzzleService\Zip\{0}.zip";
        /// <summary>
        /// 附加拼图程序配置文件位置
        /// </summary>
        public static string PuzzleExeConfig { get; set; }
        /// <summary>
        /// cutimg.engine文件路径
        /// </summary>
        public static string EnginePath { get; set; }
        /// <summary>
        /// 备份路径
        /// </summary>
        public static string BakPath { get; set; }
        public static string BackBakPath
        {
            get
            {
                return BakPath + "Back\\";
            }
        }
        public static string FrontBakPath
        {
            get
            {
                return BakPath + "Front\\";
            }
        }

        public PicService()
        {
            if (savePicArray == null)
            {
                savePicArray = new Dictionary<string, List<int>>();
            }
            if (savePicArray2 == null)
            {
                savePicArray2 = new Dictionary<string, List<string>>();
            }
            if (picDataRom == null)
            {
                picDataRom = new Dictionary<int, Dictionary<int, byte[]>>();
            }
            if (picDataRom2 == null)
            {
                picDataRom2 = new Dictionary<string, Dictionary<int, byte[]>>();
            }
            if (picDataRom3 == null)
            {
                picDataRom3 = new Dictionary<string, Dictionary<int, byte[]>>();
            }
            if (picDataRoms == null)
            {
                picDataRoms = new Dictionary<string, Dictionary<RobotName, Dictionary<int, byte[]>>>();
            }
        }

        public static void SaveLog()
        {
            Task.Run(() =>
            {
                do
                {
                    Thread.Sleep(600000);
                    string logPath = Application.StartupPath + "\\Log\\" + DateTime.Now.ToString("yyyy-MM-dd") + "\\";

                    lock (lock_logXml)
                    {
                        logXml?.Save(logPath + "log.xml");
                    }
                    lock (lock_errorXml)
                    {
                        error?.Save(logPath + "error.xml");
                    }
                    lock (lock_frontXml)
                    {
                        frontLog?.Save(logPath + "frontLog.xml");
                    }
                    lock (lock_backXml)
                    {
                        backLog?.Save(logPath + "backLog.xml");
                    }
                    lock (lock_lidarXml)
                    {
                        lidarLog?.Save(logPath + "lidarLog.xml");
                    }
                    lock (lock_rgvXml)
                    {
                        rgvLog?.Save(logPath + "rgvLog.xml");
                    }
                    lock (lock_depthXml)
                    {
                        depthLog?.Save(logPath + "depthLog.xml");
                    }
                    lock (lock_serviceXml)
                    {
                        serviceLog?.Save(logPath + "serviceLog.xml");
                    }
                    lock (lock_lightXml)
                    {
                        lightLog?.Save(logPath + "lightLog.xml");
                    }
                } while (true);
            });
        }

        public static void SaveLog(string log, int type)
        {
            if (saveTime.Day != DateTime.Now.Day || saveTime.Month != DateTime.Now.Month || saveTime.Year != DateTime.Now.Year)
            {
                saveTime = DateTime.Now;
                lock (lock_logXml)
                {
                    logXml = null;
                }
                lock (lock_errorXml)
                {
                    error = null;
                }
                lock (lock_frontXml)
                {
                    frontLog = null;
                }
                lock (lock_backXml)
                {
                    backLog = null;
                }
                lock (lock_lidarXml)
                {
                    lidarLog = null;
                }
                lock (lock_rgvXml)
                {
                    rgvLog = null;
                }
                lock (lock_depthXml)
                {
                    depthLog = null;
                }
                lock (lock_serviceXml)
                {
                    serviceLog = null;
                }
                lock (lock_lightXml)
                {
                    lightLog = null;
                }
            }
            string logPath = Application.StartupPath + "\\Log\\" + DateTime.Now.ToString("yyyy-MM-dd") + "\\";
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
            string time = DateTime.Now.ToString("HH:mm:ss");
            if (type != 4 && type != 5 && type != 6)
            {
                lock (lock_logXml)
                {
                    if (logXml == null)
                    {
                        logXml = new XmlModel(logPath + "log.xml", true);
                    }
                    InsertLog(logXml, time, log);
                }
            }
            if (type < 0)
            {
                lock (lock_errorXml)
                {
                    if (error == null)
                    {
                        error = new XmlModel(logPath + "error.xml", true);
                    }
                    InsertLog(error, time, log);
                }
            }
            else if (type == 1)
            {
                lock (lock_frontXml)
                {
                    if (frontLog == null)
                    {
                        frontLog = new XmlModel(logPath + "frontLog.xml", true);
                    }
                    InsertLog(frontLog, time, log);
                }
            }
            else if (type == 2)
            {
                lock (lock_backXml)
                {
                    if (backLog == null)
                    {
                        backLog = new XmlModel(logPath + "backLog.xml", true);
                    }
                    InsertLog(backLog, time, log);
                }
            }
            else if (type == 3)
            {
                lock (lock_lidarXml)
                {
                    if (lidarLog == null)
                    {
                        lidarLog = new XmlModel(logPath + "lidarLog.xml", true);
                    }
                    InsertLog(lidarLog, time, log);
                }
            }
            else if (type == 4)
            {
                lock (lock_rgvXml)
                {
                    if (rgvLog == null)
                    {
                        rgvLog = new XmlModel(logPath + "rgvLog.xml", true);
                    }
                    InsertLog(rgvLog, time, log);
                }
            }
            else if (type == 5)
            {
                lock (lock_depthXml)
                {
                    if (depthLog == null)
                    {
                        depthLog = new XmlModel(logPath + "depthLog.xml", true);
                    }
                    InsertLog(depthLog, time, log);
                }
            }
            else if (type == 6)
            {
                lock (lock_serviceXml)
                {
                    if (serviceLog == null)
                    {
                        serviceLog = new XmlModel(logPath + "serviceLog.xml", true);
                    }
                    InsertLog(serviceLog, time, log);
                }
            }
            else if (type == 7)
            {
                lock (lock_lightXml)
                {
                    if (lightLog == null)
                    {
                        lightLog = new XmlModel(logPath + "lightLog.xml", true);
                    }
                    InsertLog(lightLog, time, log);
                }
            }
        }

        private static void InsertLog(XmlModel xml, string time, string log)
        {
            XmlModel xm = xml["log"][xml["log"].BrotherCount];
            xm["time"] = time;
            xm["text"] = log;
        }

        public void AddLog(string log, int type = 6)
        {
            Task.Run(() => { SaveLog(log, type); });
        }

        /// <summary>
        /// 获取定位
        /// </summary>
        /// <param name="id">唯一标识</param>
        /// <param name="picName1">图片名称</param>
        /// <param name="picName2">图片名称</param>
        /// <param name="picName3">图片名称</param>
        public int GetLocation(string id, string picName1, string picName2, string picName3, string robotID, int state)
        {
            AddLog("获取定位信息");
            string dir = UploadPath + id + "_" + robotID;
            string path1 = dir + @"\Location\" + picName1;
            string path2 = dir + @"\Location\" + picName2;
            string path3 = dir + @"\Location\" + picName3;
            if (objLocation == IntPtr.Zero)
            {
                objLocation = CheckLocationCdoublePlus.LocationFactory();
                locationInitComplete = CheckLocationCdoublePlus.CallOnInit(objLocation, Application.StartupPath + "\\axis_s_640.engine") == 0;
            }
            if (locationInitComplete)
            {
                int loc = (int)CheckLocationCdoublePlus.Callgetdis(objLocation, path1, path2, state);
                AddLog("得到定位修正：" + loc);
                return loc;
            }
            return 0;
        }

        /// <summary>
        /// 分段上传
        /// </summary>
        /// <param name="picIndex">图片索引</param>
        /// <param name="dataIndex">图片数据索引</param>
        /// <param name="dataLength">图片数据数量</param>
        /// <param name="id">唯一标识</param>
        /// <param name="imgData">图片数据</param>
        public void UploadImage(int picIndex, int dataIndex, int dataLength, string id, byte[] imgData, string robotID)
        {
            /*
             * 分段传输的接受逻辑可以通过委托SavePictureFunction实现二次开发
             * 该委托返回false时，不再执行下面的接收逻辑
             */
            if (PictureDelegete.SavePictureFunction?.Invoke(picIndex, dataIndex, dataLength, id, imgData) ?? true)
            {
                if (!savePicArray.ContainsKey(id))
                {
                    savePicArray.Add(id, new List<int>());
                }

                if (!savePicArray[id].Contains(picIndex))
                {
                    savePicArray[id].Add(picIndex);
                }

                if (!picDataRom.ContainsKey(picIndex))
                {
                    picDataRom.Add(picIndex, new Dictionary<int, byte[]>());
                }

                if (picDataRom[picIndex].ContainsKey(dataIndex))
                {
                    picDataRom[picIndex][dataIndex] = imgData;
                }
                else
                {
                    picDataRom[picIndex].Add(dataIndex, imgData);
                }

                if (picDataRom[picIndex].Count == dataLength)
                {
                    try
                    {
                        if (picIndex == 10000)
                        {
                            string[] files = Directory.GetFiles(BakPath, "*.jpg");
                            foreach (string item in files)
                            {
                                File.Delete(item);
                            }
                        }
                        string dir = UploadPath + id + "_" + robotID;
                        List<byte> bufferRom = new List<byte>();
                        for (int i = 0; i < dataLength; i++)
                        {
                            bufferRom.AddRange(picDataRom[picIndex][i]);
                        }
                        string path = dir + @"\" + picIndex + ".jpg";
                    save:
                        try
                        {
                            CreateImageFromBytes(path, bufferRom.ToArray());
                        }
                        catch (Exception e)
                        {
                            AddLog("保存线阵图片Error：" + e.Message);
                            goto save;
                        }
                        AddLog("保存线阵图片: " + path);
                        try
                        {
                            if (File.Exists(BakPath + picIndex + ".jpg"))
                            {
                                File.Delete(BakPath + picIndex + ".jpg");
                            }
                            File.Copy(path, BakPath + picIndex + ".jpg");
                        }
                        catch (Exception e)
                        {
                            AddLog("备份线阵图片Error：" + e.Message);
                        }
                        picDataRom[picIndex].Clear();
                        picDataRom.Remove(picIndex);
                        GC.Collect();
                        savePicArray[id].Sort();
                    }
                    catch (Exception e)
                    {
                        AddLog("执行保存线阵图片Error：" + e.Message);
                    }

#if !puzzleC
                    if (!string.IsNullOrEmpty(PuzzleExeConfig))
                    {
                        using (StreamWriter sw = new StreamWriter(PuzzleExeConfig))
                        {
                            string[] vs = id.Split('_');
                            sw.WriteLine(vs[2] + "-" + vs[3] + "-" + vs[4] + " " + vs[5] + ":" + vs[6]);
                            sw.WriteLine(dir);
                            sw.WriteLine(vs[0] + "_" + vs[1]);
                        }
                    } 
#endif
                }
            }
        }

        /// <summary>
        /// 分段上传
        /// </summary>
        /// <param name="picIndex">图片索引</param>
        /// <param name="dataIndex">图片数据索引</param>
        /// <param name="dataLength">图片数据数量</param>
        /// <param name="id">唯一标识</param>
        /// <param name="imgData">图片数据</param>
        public void UploadImage2(string picIndex, int dataIndex, int dataLength, string id, byte[] imgData, string robotID)
        {
            if (!savePicArray2.ContainsKey(id))
            {
                savePicArray2.Add(id, new List<string>());
            }

            if (!savePicArray2[id].Contains(picIndex))
            {
                savePicArray2[id].Add(picIndex);
            }

            if (!picDataRom2.ContainsKey(picIndex))
            {
                picDataRom2.Add(picIndex, new Dictionary<int, byte[]>());
            }

            if (picDataRom2[picIndex].ContainsKey(dataIndex))
            {
                picDataRom2[picIndex][dataIndex] = imgData;
            }
            else
            {
                picDataRom2[picIndex].Add(dataIndex, imgData);
            }

            if (picDataRom2[picIndex].Count == dataLength)
            {
                try
                {
                    string dir = UploadPath + id + "_" + robotID;
                    List<byte> bufferRom = new List<byte>();
                    for (int i = 0; i < dataLength; i++)
                    {
                        bufferRom.AddRange(picDataRom2[picIndex][i]);
                    }
                    string path = dir + @"\" + picIndex + ".jpg";
                save:
                    try
                    {
                        CreateImageFromBytes(path, bufferRom.ToArray());
                    }
                    catch (Exception e)
                    {
                        AddLog("保存线阵图片Error：" + e.Message);
                        goto save;
                    }
                    AddLog("保存线阵图片: " + path);
                    try
                    {
                        if (File.Exists(BakPath + picIndex + ".jpg"))
                        {
                            File.Delete(BakPath + picIndex + ".jpg");
                        }
                        File.Copy(path, BakPath + picIndex + ".jpg");
                    }
                    catch (Exception e)
                    {
                        AddLog("备份线阵图片Error：" + e.Message);
                    }
                    picDataRom2[picIndex].Clear();
                    picDataRom2.Remove(picIndex);
                    GC.Collect();
                    savePicArray2[id].Sort();
                }
                catch (Exception e)
                {
                    AddLog("执行保存线阵图片Error：" + e.Message);
                }
            }
        }

        /// <summary>
        /// 分段上传
        /// </summary>
        /// <param name="picName">图片名称</param>
        /// <param name="dataIndex">图片数据索引</param>
        /// <param name="dataLength">图片数据数量</param>
        /// <param name="id">唯一标识</param>
        /// <param name="imgData">图片数据</param>
        public void UploadImage3(string picName, int dataIndex, int dataLength, string id, byte[] imgData, string robotID)
        {
            if (!picDataRom3.ContainsKey(picName))
            {
                picDataRom3.Add(picName, new Dictionary<int, byte[]>());
            }

            if (picDataRom3[picName].ContainsKey(dataIndex))
            {
                picDataRom3[picName][dataIndex] = imgData;
            }
            else
            {
                picDataRom3[picName].Add(dataIndex, imgData);
            }

            if (picDataRom3[picName].Count == dataLength)
            {
                try
                {
                    string dir = UploadPath + id + "_" + robotID;
                    List<byte> bufferRom = new List<byte>();
                    for (int i = 0; i < dataLength; i++)
                    {
                        bufferRom.AddRange(picDataRom3[picName][i]);
                    }
                    string path = dir + @"\Location\" + picName;
                save:
                    try
                    {
                        CreateImageFromBytes(path, bufferRom.ToArray());
                    }
                    catch (Exception e)
                    {
                        AddLog("保存定位图片Error：" + e.Message);
                        goto save;
                    }
                    AddLog("保存定位图片: " + path);
                    try
                    {
                        if (File.Exists(BakPath + picName))
                        {
                            File.Delete(BakPath + picName);
                        }
                        File.Copy(path, BakPath + picName);
                    }
                    catch (Exception e)
                    {
                        AddLog("备份定位图片Error：" + e.Message);
                    }
                    picDataRom3[picName].Clear();
                    picDataRom3.Remove(picName);
                    GC.Collect();
                }
                catch (Exception e)
                {
                    AddLog("执行保存定位图片Error：" + e.Message);
                }
            }
        }

        /// <summary>
        /// 完成传输
        /// </summary>
        /// <param name="id">唯一标识</param>
        public void UploadComplete(string id, string robotID, int number)
        {
            Process[] processes = Process.GetProcessesByName("PuzzleConsole");
            if (processes != null)
            {
                foreach (Process item in processes)
                {
                    item.Kill();
                }
            }
            Process.Start(Application.StartupPath + "\\PuzzleConsole.exe", id + " " + robotID + " " + number);
            if (savePicArray.ContainsKey(id))
            {
#if !puzzleC
                string[] ids = id.Split('_');
                if (PictureDelegete.PuzzlePictureFunction?.Invoke(id, savePicArray[id].ToArray()) ?? true)
                {
                    if (PuzzleCount == 0)
                    {
                        Directory.CreateDirectory(ZipPath.Substring(0, ZipPath.LastIndexOf(@"\")));
                        Directory.CreateDirectory(PuzzlePath + id);
                        int remainder = savePicArray[id].Count % 8;
                        int length = savePicArray[id].Count / 8;
                        int index = 0;
                        for (int i = 0; i < 8; i++)
                        {
                            if (i == 0)
                            {
                                length += remainder;
                            }
                            else
                            {
                                length += savePicArray[id].Count / 8;
                            }

                #region 压缩拼图
                            int currentHeight = 0;
                            int height = 2260 * (i == 0 ? length : savePicArray[id].Count / 8);
                            using (Bitmap tableChartImage = new Bitmap(2048, height / 2))
                            {
                                using (Graphics graph = Graphics.FromImage(tableChartImage))
                                {
                                    for (int j = index; j < length; j++)
                                    {
                                        using (Image img = Image.FromFile(UploadPath + id + @"\" + savePicArray[id][j] + ".jpg"))
                                        {
                                            using (Bitmap tmp = new Bitmap(img, 2048, 1130))
                                            {
                                                graph.DrawImage(tmp, 0, currentHeight, tmp.Width, tmp.Height);
                                                currentHeight += 1130;
                                            }
                                        }
                                    }
                                }
                                CompressImg(PuzzlePath + id + @"\" + ids[0] + "_" + ids[1] + "_" + (i + 1) + "_" + robotID + ".jpg", tableChartImage, 100);
                            }
                #endregion
                            index = length;
                        }
                    }
                    else
                    {
                        int count = PuzzleCount;
                        int length = savePicArray[id].Count / count + (savePicArray[id].Count % count > 0 ? 1 : 0);
                        for (int i = 0; i < length; i++)
                        {
                            int drawY = 0;
                            Bitmap bitmap = new Bitmap(DefaultWidth, DefaultHeight * count);
                            Graphics g = Graphics.FromImage(bitmap);
                            for (int j = 0; j < count; j++)
                            {
                                int index = i * count + j;
                                if (index < savePicArray[id].Count)
                                {
                                    Image image = Image.FromFile(UploadPath + id + @"\" + savePicArray[id][index] + ".jpg");
                                    g.DrawImage(image, 0, drawY, DefaultWidth, DefaultHeight);
                                    image.Dispose();
                                    drawY += DefaultHeight;
                                }
                            }
                            CreateImageFromBytes(PuzzlePath + id + @"\" + ids[0] + "_" + ids[1] + "_" + (i + 1) + "_" + robotID + ".jpg", ImageToBytes(bitmap));
                        }
                    }
#endif
            }
            savePicArray.Remove(id);
            savePicArray2.Remove(id);
            picDataRom.Clear();
            picDataRom2.Clear();
        }

        /// <summary>
        /// 分段上传
        /// </summary>
        /// <param name="parsIndex">部件总编号</param>
        /// <param name="robot">机械臂</param>
        /// <param name="dataIndex">图片数据索引</param>
        /// <param name="dataLength">图片数据数量</param>
        /// <param name="id">唯一标识</param>
        /// <param name="imgData">图片数据</param>
        /// <param name="robotID">机器人编号</param>
        /// <returns></returns>
        public string UploadPictrue(string parsIndex, int robot, int dataIndex, int dataLength, string id, byte[] imgData, string robotID)
        {
            RobotName robotName = (RobotName)robot;
            if (!picDataRoms.ContainsKey(parsIndex))
            {
                picDataRoms.Add(parsIndex, new Dictionary<RobotName, Dictionary<int, byte[]>>());
            }

            if (!picDataRoms[parsIndex].ContainsKey(robotName))
            {
                picDataRoms[parsIndex].Add(robotName, new Dictionary<int, byte[]>());
            }

            if (!picDataRoms[parsIndex][robotName].ContainsKey(dataIndex))
            {
                picDataRoms[parsIndex][robotName].Add(dataIndex, null);
            }
            picDataRoms[parsIndex][robotName][dataIndex] = imgData;

            try
            {
                if (picDataRoms[parsIndex][robotName].Count == dataLength)
                {
                    string dir = UploadPath + id + "_" + robotID + @"\" + robotName.ToString();
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    List<byte> bufferRom = new List<byte>();
                    for (int i = 0; i < dataLength; i++)
                    {
                        bufferRom.AddRange(picDataRoms[parsIndex][robotName][i]);
                    }
                    string path = dir + @"\" + parsIndex + ".jpg";
                save:
                    try
                    {
                        CreateImageFromBytes(path, bufferRom.ToArray());
                    }
                    catch (Exception e)
                    {
                        AddLog("保存面阵图片Error：" + e.Message);
                        goto save;
                    }
                    AddLog("保存面阵图片（" + robotName.ToString() + "）: " + path);
                    try
                    {
                        switch (robotName)
                        {
                            case RobotName.Front:
                                if (File.Exists(FrontBakPath + parsIndex + ".jpg"))
                                {
                                    File.Delete(FrontBakPath + parsIndex + ".jpg");
                                }
                                File.Copy(path, FrontBakPath + parsIndex + ".jpg");
                                break;
                            case RobotName.Back:
                                if (File.Exists(BackBakPath + parsIndex + ".jpg"))
                                {
                                    File.Delete(BackBakPath + parsIndex + ".jpg");
                                }
                                File.Copy(path, BackBakPath + parsIndex + ".jpg");
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        AddLog("备份面阵图片Error：" + e.Message);
                    }
                    picDataRoms[parsIndex][robotName].Clear();
                    picDataRoms[parsIndex].Remove(robotName);
                    if (picDataRoms[parsIndex].Count == 0)
                    {
                        picDataRoms.Remove(parsIndex);
                    }
                    GC.Collect();
                    return path;
                }
            }
            catch (Exception e)
            {
                AddLog("执行保存面阵图片Error：" + e.Message);
            }
            return "";
        }

        /// <summary>
        /// byte数组转图片
        /// </summary>
        public static Image BytesToImage(byte[] buffer)
        {
            MemoryStream ms = new MemoryStream(buffer);
            Image image = Image.FromStream(ms);
            return image;
        }

        /// <summary>
        /// 拼切图
        /// </summary>
        public void Complete(string id, string robotID, int number)
        {
            /*
             * 拼接逻辑的二次开发可以通过委托PuzzlePictureFunction来实现
             * 该委托返回false时，不再执行下面的拼图逻辑
             * 下面的拼图逻辑存在上限，最多拼几张图片
             */
            complete1 = complete2 = complete3 = false;
#if puzzleC
            Task.Run(() =>
            {
                Console.WriteLine("开始拼切图");
                try
                {
                    if (obj == IntPtr.Zero)
                    {
                        obj = PuzzleCdoublePlus.ExportObjectFactory();
                        Console.WriteLine(EnginePath);
                        puzzleInitComplete = PuzzleCdoublePlus.CallOnInit(obj, EnginePath) == 0;
                    }
                    if (puzzleInitComplete)
                    {
                        string[] ids = id.Split('_');
                        config_info config = new config_info(UploadPath + id + "_" + robotID, ids[0] + "_" + ids[1], ids[2] + "_" + ids[3] + "_" + ids[4] + "_" + ids[5] + "_" + ids[6], PuzzlePath);
                        //inputrect_4[] input = DataFunc?.Invoke(ids[0] + "_" + ids[1]);
                        //int[] height = HeightFunc?.Invoke();
                        int s = PuzzleCdoublePlus.Callcutimg(obj, config,
                            //input, input.Length, height, height.Length,
                            new inputrect_4[0], 0, new int[0], 0,
                            number);
                        Console.WriteLine("完成拼切图" + id + "_" + robotID + "\r\n" + s);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    complete1 = true;
                }
            });
            Task.Run(() =>
            {
                try
                {
                    string[] front = Directory.GetFiles(FrontBakPath);
                    string[] back = Directory.GetFiles(BackBakPath);
                    foreach (string item in front)
                    {
                        File.Delete(item);
                    }
                    foreach (string item in back)
                    {
                        File.Delete(item);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    complete2 = true;
                }
            });
#endif
            ZipImage(id, robotID);
            complete3 = true;
        }

        /// <summary>
        /// byte数组直接保存为图片文件
        /// </summary>
        public static void CreateImageFromBytes(string fileName, byte[] buffer)
        {
            FileInfo info = new FileInfo(fileName);
            Directory.CreateDirectory(info.Directory.FullName);
            File.WriteAllBytes(fileName, buffer);
        }

        /// <summary>
        /// 图片转byte数组
        /// </summary>
        public static byte[] ImageToBytes(Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Jpeg);
                byte[] buffer = new byte[ms.Length];
                ms.Seek(0, SeekOrigin.Begin);
                ms.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        /// <summary>
        /// 图片压缩
        /// </summary>
        public static void ZipImage(string id, string robotID)
        {
            CreateZipFile(PuzzlePath + id, string.Format(ZipPath, id + "_" + robotID));
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

        private static void CreateZipFile(string filesPath, string zipFilePath)
        {
            if (!Directory.Exists(filesPath))
            {
                Console.WriteLine("Cannot find directory '{0}'", filesPath);
                return;
            }

            try
            {
                string[] filenames = Directory.GetFiles(filesPath);
                using (ZipOutputStream s = new ZipOutputStream(File.Create(zipFilePath)))
                {

                    s.SetLevel(9); // 压缩级别 0-9
                    //s.Password = "123"; //Zip压缩文件密码
                    byte[] buffer = new byte[4096]; //缓冲区大小
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

        public void Upload3DData(string parsIndex, int robot, string data, string id, string robotID)
        {
            
        }

        public void UploadParameter(float[] kc, float[] kk, string robotID)
        {
            
        }
    }

    public enum RobotName
    {
        Front = 0, Back
    }
}
