using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YandexTranslateDLL;

namespace QuickTranslate
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.HideOnStart = checkBox1.Checked;
            Properties.Settings.Default.TopMost = checkBox2.Checked;
            Properties.Settings.Default.AutoClear = checkBox3.Checked;
            Properties.Settings.Default.FromLanguage = comboBox1.Text;
            Properties.Settings.Default.ToLanguage = comboBox2.Text;
            Properties.Settings.Default.HotKey = comboBox3.Text;
            Properties.Settings.Default.ShowInTaskbar = checkBox4.Checked;
            Properties.Settings.Default.Opacity = (byte)numericUpDown1.Value;
            Form1.instance.Opacity = (byte)numericUpDown1.Value / 100d;
            Form1.instance.ShowInTaskbar = checkBox4.Checked;


            Form1.instance.SetHotKey((Keys)Enum.Parse(typeof(Keys), comboBox3.Text));
            Form1.instance.UpdateLanguages((Yandex.Language)Enum.Parse(typeof(Yandex.Language), Properties.Settings.Default.FromLanguage, true), (Yandex.Language)Enum.Parse(typeof(Yandex.Language), Properties.Settings.Default.ToLanguage, true));

            Properties.Settings.Default.Save();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            checkBox1.Checked = Properties.Settings.Default.HideOnStart;
            checkBox2.Checked = Properties.Settings.Default.TopMost;
            checkBox3.Checked = Properties.Settings.Default.AutoClear;
            checkBox4.Checked = Properties.Settings.Default.ShowInTaskbar;
            numericUpDown1.Value = Properties.Settings.Default.Opacity;
            comboBox1.Text = Properties.Settings.Default.FromLanguage;
            comboBox2.Text = Properties.Settings.Default.ToLanguage;
            comboBox3.Text = Properties.Settings.Default.HotKey;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            Form1.instance.TopMost = ((CheckBox)sender).Checked;
        }
    }
}
