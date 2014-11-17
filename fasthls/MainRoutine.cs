using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ThunderAgentLib;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace fastFFmpeg
{
    class MainRoutine
    {
        [STAThread]
        static void Main(string[] args)
        {

            PY py = new PY();
            if (args.Length <= 0) return;

            if (args[0].Equals("html")) { htmlhelper(); Environment.Exit(0); }
            if (args[0].Equals("test")) { new Scanner(Directory.GetCurrentDirectory()); while (true) { Console.Write(">"); string tmp = Console.ReadLine(); if (tmp.Equals("quit")) break; }; Environment.Exit(0); }
            if (args[0].Equals("dl")) { new DownLoad(args[1]); while (true) { Console.Write(">"); string tmp = Console.ReadLine(); if (tmp.Equals("quit"))break; }; Environment.Exit(0); }

            

            FileInfo finfo = new FileInfo(args[0]);

            string filename = System.IO.Path.GetFileNameWithoutExtension(finfo.FullName);
            string fileExt = System.IO.Path.GetExtension(finfo.FullName);
            string pyname = py.GetAbbreviation(NameFormat(filename));

            if (IsDone(pyname)) { MessageBox.Show("这个影片已经转换完毕了"); Application.Exit(); Environment.Exit(0); }  //Environment.Exit(0);

            System.Diagnostics.ProcessStartInfo p = null;
            System.Diagnostics.Process proc;

            System.IO.DirectoryInfo a = System.IO.Directory.CreateDirectory(pyname);

            //file name normilaz
            filename = filename.Normalize();

            filename = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(filename)); ;

            string param = "-y -i " + filename + fileExt + " " + "-pix_fmt yuv420p -vcodec libx264 -acodec aac -strict experimental -ac 2 -b:a 96k -ar 44100 -r 29.94 -profile:v baseline -b:v 1500k -maxrate 2000k -force_key_frames 50 -s 1280*720 -map 0 -flags -global_header -f segment -segment_list " + pyname + "/" + "q.m3u8 -segment_time 10 -segment_format mpeg_ts -segment_list_type m3u8 " + pyname + "/" + "segment%05d.ts";

            p = new ProcessStartInfo("ffmpeg.exe", param);
            p.CreateNoWindow = false;
            p.UseShellExecute = false;
            p.RedirectStandardOutput = true;

            try
            {
                if (p != null)
                {

                    proc = Process.Start(p);
                    Console.WriteLine("In process");
                    proc.BeginOutputReadLine();
                    proc.WaitForExit();
                    markDone(pyname, NameFormat(filename));
                    toHTML(NameFormat(filename), pyname);
                    Console.WriteLine("Done");


                }
            }
            catch (Exception)
            {

                throw;
            }


        }
        //name format
        public static string NameFormat(String str)
        {
            string[] tmp = str.Split(new char[] { '.' });
            if (tmp.Length >= 2)
            {
                return tmp[3];
            }
            else
            {
                return str;
            }

        }
        //check isDone
        public static bool IsDone(string folderName)
        {
            String path = folderName + @"\d.txt";
            if (File.Exists(path))
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        public static void markDone(string folderName, string moviename)
        {
            FileStream marker = new FileStream(folderName + "\\d.txt", FileMode.Create);
            UTF8Encoding CODER = new UTF8Encoding(true);
            StreamWriter wtr = new StreamWriter(marker, CODER);
            wtr.Write(moviename);
            wtr.Close();

        }

        public static void toHTML(string moviename, string moviefolder)
        {
            //转换成网页
            FileStream file = new FileStream("m.html", FileMode.Append);
            UTF8Encoding utf8 = new UTF8Encoding(true);
            StreamWriter sw = new StreamWriter(file, utf8);
            sw.AutoFlush = true;

            string windowsplayer = @"http://69.30.248.194/movie/player/StrobeMediaPlayback.html?src=";
            string namesuoxie = moviefolder + @"\" + "q.m3u8";

            namesuoxie = "\"" + namesuoxie + "\"";

            //for IOS
            string m3u8para = "<a href=" + namesuoxie + ">" + moviename + @"</a>";
            sw.WriteLine(m3u8para, utf8);

            //for win
            string windowspara = windowsplayer + @"http://69.30.248.194/movie/" + moviefolder + @"/q.m3u8";
            windowspara = "\"" + windowspara + "\"";
            string windowsplay = @"&nbsp &nbsp &nbsp &nbsp &nbsp" + "<a href=" + windowspara + ">" + moviename + "（windows点这里）" + @"</a>" + @"<br/>";

            sw.WriteLine(windowsplay, utf8);

            sw.Dispose();
            file.Dispose();

        }

        public static void toHTML(string moviename, string moviefolder, string path)
        {
            /*@"http://69.30.248.194/movie/"*/
            // example of path
            FileStream file = new FileStream("index.html", FileMode.Append);
            UTF8Encoding utf8 = new UTF8Encoding(true);
            StreamWriter sw = new StreamWriter(file, utf8);
            sw.AutoFlush = true;



            string windowsplayer = @"http://69.30.248.194/movie/player/StrobeMediaPlayback.html?src=";
            string namesuoxie = moviefolder + @"\" + "q.m3u8";

            namesuoxie = "\"" + namesuoxie + "\"";

            string m3u8para = "<a href=" + namesuoxie + ">" + moviename + @"</a>";
            sw.WriteLine(m3u8para, utf8);

            string windowspara = windowsplayer + path/*@"http://69.30.248.194/movie/"*/ + moviefolder + @"/q.m3u8";
            windowspara = "\"" + windowspara + "\"";
            string windowsplay = @"&nbsp &nbsp &nbsp &nbsp &nbsp" + "<a href=" + windowspara + ">" + moviename + "（windows点这里）" + @"</a>" + @"<br/>";

            sw.WriteLine(windowsplay, utf8);

        }

        public static void htmlhelper()
        {
            string[] dir = Directory.GetDirectories(Directory.GetCurrentDirectory());

            for (int i=0; i<dir.Length ;i++)
            {

                dir[i] = pathFormat(dir[i]);
               
            }

            System.IO.StreamReader sr;

            foreach (string filepath in dir)
            {

                if (filepath.Equals("player")) { continue; }
                sr = new System.IO.StreamReader(filepath + "\\" + "d.txt", System.Text.Encoding.UTF8);
                string tmp = sr.ReadToEnd();
                sr.Dispose();
                toHTML(tmp, filepath);

            }

        }
        public static void htmlhelper(string ignorepath )
        {
            string[] dir = Directory.GetDirectories(Directory.GetCurrentDirectory());

            for (int i = 0; i < dir.Length; i++)
            {

                dir[i] = pathFormat(dir[i]);

            }

            System.IO.StreamReader sr;

            foreach (string filepath in dir)
            {

                if (filepath.Equals(ignorepath)) { continue; }
                sr = new System.IO.StreamReader(filepath + "\\" + "d.txt", System.Text.Encoding.UTF8);
                string tmp = sr.ReadToEnd();
                sr.Dispose();
                toHTML(tmp, filepath);

            }

        }

        public static string pathFormat(string inp)
        {
                  string path = inp;

                  int index = path.LastIndexOf(@"\");

             
                 string result = path.Substring(index + 1, path.Length - index - 1);
                 return result;
                 
                
        }


    }


    public class Scanner : IDisposable
    {
        //FileSystemWatcher []watchers;
        FileSystemWatcher watcher;

        static int count;

        public Scanner(string path)
        {
            count = 0;
           /* watchers = new FileSystemWatcher[] { new FileSystemWatcher(path, "*.rmvb"), new FileSystemWatcher(path, "*.mkv"), new FileSystemWatcher(path, "*.mp4") };
            for (int i=0; i<3 ; i++)
            {
                watchers[i].Created += new FileSystemEventHandler(fs_Created);//监视创建
                watchers[i].Renamed += new RenamedEventHandler(fs_Created);
                watchers[i].EnableRaisingEvents = true;//启动监视
            } */
            watcher = new FileSystemWatcher(path, "*.*");
            watcher.Created += new FileSystemEventHandler(fs_Created);//监视创建
            watcher.Renamed += new RenamedEventHandler(fs_Created);
            watcher.EnableRaisingEvents = true;//启动监视

           

        }
        void fs_Created(object sender, FileSystemEventArgs e)
        {
            string[] tmps = e.Name.Split(new char[] { '.' });
            string tmp = tmps[tmps.Length -1];

            if (tmp.ToLower().Equals("rmvb") || tmp.ToLower().Equals("mp4") || tmp.ToLower().Equals("mkv") || tmp.ToLower().Equals("wmv") || tmp.ToLower().Equals("avi") || tmp.ToLower().Equals("asf"))
            {
                Console.WriteLine("Now working on "+e.Name);
                this.go(e.Name);
                count++;
                Console.WriteLine("Sequnce Pointer.." + count);
            }
            else
            {
                tmp = null;
            }
        }

        void go(string name)
        {
            Process proc = new Process();
            
            proc.Exited += new EventHandler(jc_exit);

            proc.EnableRaisingEvents = true;
            proc.StartInfo.FileName = "fastFFmpeg.exe";
            proc.StartInfo.Arguments = name ;
           

                while (count > 1) 
                {
                    Thread.Sleep(2*1000);
                    Console.WriteLine("Waiting for recent file done...");
                }

            proc.Start();
            
            //proc.WaitForExit();
        }

        void jc_exit(object sender, EventArgs e)
        {
            count--;
            Console.WriteLine("Sequnce Pointer.."+count);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
                watcher.Dispose();
            }
            // free native resources
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }




        



    }


    public class DownLoad
    {
        #region Dll Import 需要导入的api 声明。

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, string lParam);

        const int WM_GETTEXT = 0x000D;

        const int WM_SETTEXT = 0x000C;

        const int WM_CLICK = 0x00F5;


        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr PostMessage(IntPtr hwnd, int wMsg, IntPtr wParam, string lParam);

        const int WM_KEYDOWN = 0x100;
        const int WM_KEYUP = 0x101;

         #endregion  

        public DownLoad(string link)
        {
            if (link.Equals("txt")) { this.loadFromTxt(Directory.GetCurrentDirectory()); }
            else
            {

                go(link);

            }
            
        }

        private void go(string link)
        {
            Thread.Sleep(1 * 1000);
            ThunderAgentLib.AgentClass engine = new AgentClass();
            engine.AddTask(link, "", "", "", "", 1, 0, 5);
            engine.CommitTasks2(1);

            for (int i = 0; i < 2; i++)
            {
                confirm();
            }
        }

        private void confirm()
        {


            IntPtr hWnd = FindWindow(null, "新建任务");
            IntPtr VK_RETURN = new IntPtr(13);



            if (!hWnd.Equals(IntPtr.Zero))
            {
                //MessageBox.Show("find！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Thread.Sleep(1 * 1000);
                SendMessage(hWnd, WM_KEYDOWN, VK_RETURN, "0");
            }


            Thread.Sleep(1 * 1000);
        }

        private void loadFromTxt(string s)
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = s;//@"d:DownLoads";//args[1];

            
            // Only watch text files.
            watcher.Filter = "shit.txt";

            // Add event handlers.
            //watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);

            // Begin watching.
            watcher.EnableRaisingEvents = true;


        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            Thread.Sleep(300);
            FileStream f = new FileStream(e.Name,FileMode.Open);
            string line = new StreamReader(f).ReadLine();
            go(line);
            f.Close();
            File.Delete(e.Name);
            
        }


    }


        public class PY
        {
            // Fields
            private string m_strAbbreviation;
            private string m_strFullCode;
            private string m_strFullCodeWithSpace;
            private string m_strInput;
            private static string[] pyStr;
            private static int[] pyValue;

            public PY()
            {
                //
                // TODO: 在此处添加构造函数逻辑
                //
            }
            public string GetAbbreviation(string strInput)
            {
                this.m_strInput = strInput;
                this.GetPyCode();
                return this.m_strAbbreviation;
            }


            public string GetFullCode(string strInput)
            {
                this.m_strInput = strInput;
                this.GetPyCode();
                return this.m_strFullCode;
            }


            public string GetFullCodeWithSpace(string strInput)
            {
                this.m_strInput = strInput;
                this.GetPyCode();
                return this.m_strFullCodeWithSpace;
            }


            private bool GetPyCode()
            {
                this.m_strFullCode = "";
                this.m_strFullCodeWithSpace = "";
                this.m_strAbbreviation = "";
                byte[] buffer1 = new byte[2];
                int num1 = 0;
                int num2 = 0;
                int num3 = 0;
                char[] chArray1 = this.m_strInput.ToCharArray();
                for (int num4 = 0; num4 < chArray1.Length; num4++)
                {
                    buffer1 = System.Text.Encoding.Default.GetBytes(chArray1[num4].ToString());
                    num2 = buffer1[0];
                    if (num2 <= 0x7f)
                    {
                        this.m_strFullCode = this.m_strFullCode + chArray1[num4];
                        this.m_strFullCodeWithSpace = this.m_strFullCodeWithSpace + chArray1[num4];
                        this.m_strFullCodeWithSpace = this.m_strFullCodeWithSpace + " ";
                        this.m_strAbbreviation = this.m_strAbbreviation + chArray1[num4];
                    }
                    else
                    {
                        num3 = buffer1[1];
                        num1 = ((num2 * 0x100) + num3) - 0x10000;
                        if ((num1 > 0) && (num1 < 160))
                        {
                            this.m_strFullCode = this.m_strFullCode + chArray1[num4];
                            this.m_strFullCodeWithSpace = this.m_strFullCodeWithSpace + chArray1[num4];
                            this.m_strFullCodeWithSpace = this.m_strFullCodeWithSpace + " ";
                            this.m_strAbbreviation = this.m_strAbbreviation + chArray1[num4];
                        }
                        else
                        {
                            for (int num5 = PY.pyValue.Length - 1; num5 >= 0; num5--)
                            {
                                if (PY.pyValue[num5] <= num1)
                                {
                                    this.m_strFullCode = this.m_strFullCode + PY.pyStr[num5];
                                    this.m_strFullCodeWithSpace = this.m_strFullCodeWithSpace + PY.pyStr[num5];
                                    this.m_strFullCodeWithSpace = this.m_strFullCodeWithSpace + " ";
                                    this.m_strAbbreviation = this.m_strAbbreviation + PY.pyStr[num5].Substring(0, 1);
                                    break;
                                }
                            }
                        }
                    }
                }
                return true;
            }

            static PY()
            {
                PY.pyValue = new int[] { 
           -20319, -20317, -20304, -20295, -20292, -20283, -20265, -20257, -20242, -20230, -20051, -20036, -20032, -20026, -20002, -19990, 
           -19986, -19982, -19976, -19805, -19784, -19775, -19774, -19763, -19756, -19751, -19746, -19741, -19739, -19728, -19725, -19715, 
           -19540, -19531, -19525, -19515, -19500, -19484, -19479, -19467, -19289, -19288, -19281, -19275, -19270, -19263, -19261, -19249, 
           -19243, -19242, -19238, -19235, -19227, -19224, -19218, -19212, -19038, -19023, -19018, -19006, -19003, -18996, -18977, -18961, 
           -18952, -18783, -18774, -18773, -18763, -18756, -18741, -18735, -18731, -18722, -18710, -18697, -18696, -18526, -18518, -18501, 
           -18490, -18478, -18463, -18448, -18447, -18446, -18239, -18237, -18231, -18220, -18211, -18201, -18184, -18183, -18181, -18012, 
           -17997, -17988, -17970, -17964, -17961, -17950, -17947, -17931, -17928, -17922, -17759, -17752, -17733, -17730, -17721, -17703, 
           -17701, -17697, -17692, -17683, -17676, -17496, -17487, -17482, -17468, -17454, -17433, -17427, -17417, -17202, -17185, -16983, 
           -16970, -16942, -16915, -16733, -16708, -16706, -16689, -16664, -16657, -16647, -16474, -16470, -16465, -16459, -16452, -16448, 
           -16433, -16429, -16427, -16423, -16419, -16412, -16407, -16403, -16401, -16393, -16220, -16216, -16212, -16205, -16202, -16187, 
           -16180, -16171, -16169, -16158, -16155, -15959, -15958, -15944, -15933, -15920, -15915, -15903, -15889, -15878, -15707, -15701, 
           -15681, -15667, -15661, -15659, -15652, -15640, -15631, -15625, -15454, -15448, -15436, -15435, -15419, -15416, -15408, -15394, 
           -15385, -15377, -15375, -15369, -15363, -15362, -15183, -15180, -15165, -15158, -15153, -15150, -15149, -15144, -15143, -15141, 
           -15140, -15139, -15128, -15121, -15119, -15117, -15110, -15109, -14941, -14937, -14933, -14930, -14929, -14928, -14926, -14922, 
           -14921, -14914, -14908, -14902, -14894, -14889, -14882, -14873, -14871, -14857, -14678, -14674, -14670, -14668, -14663, -14654, 
           -14645, -14630, -14594, -14429, -14407, -14399, -14384, -14379, -14368, -14355, -14353, -14345, -14170, -14159, -14151, -14149, 
           -14145, -14140, -14137, -14135, -14125, -14123, -14122, -14112, -14109, -14099, -14097, -14094, -14092, -14090, -14087, -14083, 
           -13917, -13914, -13910, -13907, -13906, -13905, -13896, -13894, -13878, -13870, -13859, -13847, -13831, -13658, -13611, -13601, 
           -13406, -13404, -13400, -13398, -13395, -13391, -13387, -13383, -13367, -13359, -13356, -13343, -13340, -13329, -13326, -13318, 
           -13147, -13138, -13120, -13107, -13096, -13095, -13091, -13076, -13068, -13063, -13060, -12888, -12875, -12871, -12860, -12858, 
           -12852, -12849, -12838, -12831, -12829, -12812, -12802, -12607, -12597, -12594, -12585, -12556, -12359, -12346, -12320, -12300, 
           -12120, -12099, -12089, -12074, -12067, -12058, -12039, -11867, -11861, -11847, -11831, -11798, -11781, -11604, -11589, -11536, 
           -11358, -11340, -11339, -11324, -11303, -11097, -11077, -11067, -11055, -11052, -11045, -11041, -11038, -11024, -11020, -11019, 
           -11018, -11014, -10838, -10832, -10815, -10800, -10790, -10780, -10764, -10587, -10544, -10533, -10519, -10331, -10329, -10328, 
           -10322, -10315, -10309, -10307, -10296, -10281, -10274, -10270, -10262, -10260, -10256, -10254
          };
                PY.pyStr = new string[] { 
            "a", "ai", "an", "ang", "ao", "ba", "bai", "ban", "bang", "bao", "bei", "ben", "beng", "bi", "bian", "biao", 
            "bie", "bin", "bing", "bo", "bu", "ca", "cai", "can", "cang", "cao", "ce", "ceng", "cha", "chai", "chan", "chang", 
            "chao", "che", "chen", "cheng", "chi", "chong", "chou", "chu", "chuai", "chuan", "chuang", "chui", "chun", "chuo", "ci", "cong", 
            "cou", "cu", "cuan", "cui", "cun", "cuo", "da", "dai", "dan", "dang", "dao", "de", "deng", "di", "dian", "diao", 
            "die", "ding", "diu", "dong", "dou", "du", "duan", "dui", "dun", "duo", "e", "en", "er", "fa", "fan", "fang", 
            "fei", "fen", "feng", "fo", "fou", "fu", "ga", "gai", "gan", "gang", "gao", "ge", "gei", "gen", "geng", "gong", 
            "gou", "gu", "gua", "guai", "guan", "guang", "gui", "gun", "guo", "ha", "hai", "han", "hang", "hao", "he", "hei", 
            "hen", "heng", "hong", "hou", "hu", "hua", "huai", "huan", "huang", "hui", "hun", "huo", "ji", "jia", "jian", "jiang", 
            "jiao", "jie", "jin", "jing", "jiong", "jiu", "ju", "juan", "jue", "jun", "ka", "kai", "kan", "kang", "kao", "ke", 
            "ken", "keng", "kong", "kou", "ku", "kua", "kuai", "kuan", "kuang", "kui", "kun", "kuo", "la", "lai", "lan", "lang", 
            "lao", "le", "lei", "leng", "li", "lia", "lian", "liang", "liao", "lie", "lin", "ling", "liu", "long", "lou", "lu", 
            "lv", "luan", "lue", "lun", "luo", "ma", "mai", "man", "mang", "mao", "me", "mei", "men", "meng", "mi", "mian", 
            "miao", "mie", "min", "ming", "miu", "mo", "mou", "mu", "na", "nai", "nan", "nang", "nao", "ne", "nei", "nen", 
            "neng", "ni", "nian", "niang", "niao", "nie", "nin", "ning", "niu", "nong", "nu", "nv", "nuan", "nue", "nuo", "o", 
            "ou", "pa", "pai", "pan", "pang", "pao", "pei", "pen", "peng", "pi", "pian", "piao", "pie", "pin", "ping", "po", 
            "pu", "qi", "qia", "qian", "qiang", "qiao", "qie", "qin", "qing", "qiong", "qiu", "qu", "quan", "que", "qun", "ran", 
            "rang", "rao", "re", "ren", "reng", "ri", "rong", "rou", "ru", "ruan", "rui", "run", "ruo", "sa", "sai", "san", 
            "sang", "sao", "se", "sen", "seng", "sha", "shai", "shan", "shang", "shao", "she", "shen", "sheng", "shi", "shou", "shu", 
            "shua", "shuai", "shuan", "shuang", "shui", "shun", "shuo", "si", "song", "sou", "su", "suan", "sui", "sun", "suo", "ta", 
            "tai", "tan", "tang", "tao", "te", "teng", "ti", "tian", "tiao", "tie", "ting", "tong", "tou", "tu", "tuan", "tui", 
            "tun", "tuo", "wa", "wai", "wan", "wang", "wei", "wen", "weng", "wo", "wu", "xi", "xia", "xian", "xiang", "xiao", 
            "xie", "xin", "xing", "xiong", "xiu", "xu", "xuan", "xue", "xun", "ya", "yan", "yang", "yao", "ye", "yi", "yin", 
            "ying", "yo", "yong", "you", "yu", "yuan", "yue", "yun", "za", "zai", "zan", "zang", "zao", "ze", "zei", "zen", 
            "zeng", "zha", "zhai", "zhan", "zhang", "zhao", "zhe", "zhen", "zheng", "zhi", "zhong", "zhou", "zhu", "zhua", "zhuai", "zhuan", 
            "zhuang", "zhui", "zhun", "zhuo", "zi", "zong", "zou", "zu", "zuan", "zui", "zun", "zuo"
           };
            }
        }







    /*
namespace test
{


    class Program
    {


       //#region Dll Import 需要导入的api 声明。



        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "SendMessage")]

        private static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, string lParam);

        const int WM_GETTEXT = 0x000D;

        const int WM_SETTEXT = 0x000C;

        const int WM_CLICK = 0x00F5;


        [DllImport("user32.dll",CharSet=CharSet.Unicode)]  
        public static extern IntPtr PostMessage(IntPtr hwnd,int wMsg,IntPtr wParam,string lParam);
        const int WM_KEYDOWN = 0x100;
        const int WM_KEYUP = 0x101;

       // #endregion  

        private void confirm()
        {


            IntPtr hWnd = FindWindow(null, "新建任务");
            IntPtr VK_RETURN = new IntPtr(13);
           
            
             
          if (!hWnd.Equals(IntPtr.Zero))
            {
            //MessageBox.Show("find！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            Thread.Sleep(1 * 1000);
            SendMessage(hWnd, WM_KEYDOWN, VK_RETURN, "0"); 
            }


          Thread.Sleep(1 * 1000);
        }



  

        static void Main(string[] args)
        {


            Thread.Sleep(10 * 1000);
            ThunderAgentLib.AgentClass engine = new AgentClass();
            engine.AddTask("http://www.baidu.com/index.html", "", "", "", "", 1, 0, 5);
            engine.CommitTasks2(1);

            for (int i = 0; i < 2; i++)
            {
               new Program().confirm();
            }
               
            
        }
    }
}


    */

    
}