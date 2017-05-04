using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AnimeNotification
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
            trackBar1.Value = Preference.UpdateInterval / 1000 / 60;
            trackBar1_Scroll(null, null);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            intervalLabel.Text = trackBar1.Value.ToString() + " минут";
            dayLabel.Text = "< " + (30.0f * 24 * 60 / trackBar1.Value / 1024).ToString() + " мегабайт";
            monthLabel.Text = "< " + (30.0f * 24 * 60 * 30 / trackBar1.Value / 1024).ToString() + " мегабайт";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Preference.UpdateInterval = trackBar1.Value * 60 * 1000;
            Preference.Save();
            Close();
        }
    }
}
