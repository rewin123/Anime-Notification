using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;

namespace AnimeNotification
{
    public partial class Form1 : Form
    {
        Link[] current;
        bool alarm = true;
        List<string> notifyNames = new List<string>();

        int baseContextItems = 3;
        public Form1()
        {
            InitializeComponent();

            Preference.Init();
            timer1.Interval = Preference.UpdateInterval;
            
            current = GetLinks();
            listBox1.Items.AddRange(current);

            UpdateContextStrip();

            timer1.Start();

            KeyDown += Form1_KeyDown;
            listBox1.KeyDown += Form1_KeyDown;

            notifyIcon1.MouseDown += NotifyIcon1_MouseDown;
            listBox1.MouseDoubleClick += ListBox1_MouseDoubleClick;

            contextMenuStrip1.MouseDown += ContextMenuStrip1_MouseDown;
            contextMenuStrip1.ItemClicked += ContextMenuStrip1_ItemClicked;

            label1.Text = "Последнее обновление " + DateTime.Now.ToLongTimeString();
        }

        private void ContextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            int index = contextMenuStrip1.Items.IndexOf(e.ClickedItem);
            if(index >= baseContextItems)
            {
                System.Diagnostics.Process.Start(current[4 - index + baseContextItems].http);
            }
        }

        void UpdateContextStrip()
        {
            while (contextMenuStrip1.Items.Count > baseContextItems)
            {
                contextMenuStrip1.Items.RemoveAt(baseContextItems);
            }

            for (int i = 0; i < Math.Min(5, current.Length); i++)
            {
                contextMenuStrip1.Items.Add(current[4 - i].name);
            }
        }

        private void ContextMenuStrip1_MouseDown(object sender, MouseEventArgs e)
        {
            
        }

        private void ListBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            System.Diagnostics.Process.Start(current[listBox1.SelectedIndex].http);
        }

        private void NotifyIcon1_MouseDown(object sender, MouseEventArgs e)
        {
            if(alarmTimer.Enabled)
            {
                notifyIcon1.ShowBalloonTip(30000, "Новое аниме вышло", ListToText(notifyNames), ToolTipIcon.Info);
                notifyNames.Clear();
            }
            alarmTimer.Stop();
            notifyIcon1.Icon = Resource1.icon;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Space)
            {
                Test();
            }
        }
        
        string ListToText(List<string> strs)
        {
            string text = "";
            for (int i = 0; i < strs.Count; i++)
            {
                text += strs[i] + "\n";
            }
            return text;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            Link[] newnames = GetLinks();
            List<Link> nomatched = new List<Link>();
            for(int i = 0;i < newnames.Length;i++)
            {
                bool ok = false;
                for(int j = 0;j < current.Length;j++)
                {
                    if(current[j].name == newnames[i].name)
                    {
                        ok = true;
                        break;
                    }
                }
                if(!ok)
                {
                    nomatched.Add(newnames[i]);
                    alarmTimer.Start();
                    notifyNames.Add(newnames[i].name);
                }
            }

            //оповещение
            string text = "";
            for(int i = 0;i < nomatched.Count;i++)
            {
                text += nomatched[i] + "\n";
            }
            if (text != "")
            {
                notifyIcon1.ShowBalloonTip(1000, "Новое аниме вышло", text, ToolTipIcon.Info);
            }

            listBox1.Items.Clear();
            listBox1.Items.AddRange(newnames);
            current = newnames;

            if(text != "")
              UpdateContextStrip();

            label1.Text = "Последнее обновление " + DateTime.Now.ToLongTimeString();

            

            GC.Collect();
        }

        void Test()
        {
            string text = "";
            for (int i = 0; i < 5; i++)
            {
                text += current[i] + "\n";
                notifyNames.Add(current[i].name);
            }

            while (contextMenuStrip1.Items.Count > 2)
            {
                contextMenuStrip1.Items.RemoveAt(2);
            }

            for (int i = 0; i < Math.Min(5, current.Length); i++)
            {
                contextMenuStrip1.Items.Add(current[i].name);
            }

            notifyIcon1.ShowBalloonTip(30000, "Тестовое окно", text, ToolTipIcon.Info);
            alarmTimer.Start();
        }

        private void alarmTimer_Tick(object sender, EventArgs e)
        {
            if(alarm)
            {
                notifyIcon1.Icon = Resource1.newspaper_icon_11;
            }
            else
            {
                notifyIcon1.Icon = Resource1.icon;
            }
            alarm = !alarm;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Normal;
            ShowInTaskbar = true;
        }

        Link[] GetLinks()
        {
            List<Link> links = new List<Link>();
            XmlReader reader = XmlReader.Create("http://online.anidub.com/rss.xml");
            reader.ReadToFollowing("title"); //пропускаем название сайта

            while (reader.ReadToFollowing("title")) //пока не закончится список последних вышедших аниме
            {
                reader.Read();
                Link l = new Link(reader.Value, "");
                reader.ReadToFollowing("link");
                reader.Read();
                l.http = reader.Value;
                links.Add(l);
            }
            return links.ToArray();
        }

        ~Form1()
        {
            notifyIcon1.Visible = false;
            Preference.Save();
        }

        private void настройкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings settings = new Settings();
            settings.ShowDialog();
            timer1.Interval = Preference.UpdateInterval;
        }
    }

    class Link
    {
        public string name;
        public string http;

        public Link(string name, string http)
        {
            this.name = name;
            this.http = http;
        }

        public override string ToString()
        {
            return name;
        }

        public static bool operator==(Link one, Link two)
        {
            return one.name == two.name;
        }

        public static bool operator !=(Link one, Link two)
        {
            return one.name != two.name;
        }

        

        public override int GetHashCode()
        {
            return 0;
        }
    }

    class Preference
    {
        static string prefPath = "Settings.txt";
        public static int UpdateInterval;

        static int defaultUpdateInterval = 300000;
        public static void Init()
        {
            if(File.Exists(prefPath))
            {
                string[] data = File.ReadAllLines(prefPath);
                UpdateInterval = int.Parse(data[0]);
                if(UpdateInterval < defaultUpdateInterval)
                {
                    UpdateInterval = defaultUpdateInterval;
                }
            }
            else
            {
                UpdateInterval = defaultUpdateInterval;
            }
        }

        public static void Save()
        {
            File.WriteAllLines(prefPath, new string[] { UpdateInterval.ToString() });
        }
    }
}
