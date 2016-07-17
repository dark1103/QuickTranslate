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
using GlobalKeyboardHook;

namespace QuickTranslate
{
    public partial class Form1 : Form
    {
        public static Form1 instance;
        Yandex.Language fromLanguage = Yandex.Language.en;
        Yandex.Language toLanguage = Yandex.Language.ru;
        public void UpdateLanguages(Yandex.Language fromLanguage, Yandex.Language toLanguage)
        {
            this.fromLanguage = fromLanguage;
            this.toLanguage = toLanguage;
        }
        public void SetHotKey(Keys key)
        {
            hookManager.HookedKeys.Clear();
            hookManager.HookedKeys.Add(key);
        }
        YandexDictionary yandexDictionary;
        YandexTranslate yandexTranslate;
        HookManager hookManager = new HookManager();
        public Form1()
        {
            InitializeComponent();
            instance = this;
            yandexDictionary = new YandexDictionary("dict.1.1.20160522T131420Z.b10d6ce777738629.422522620ec229c0f864d8c04c07d714f65acfc7", "v1");
            yandexTranslate = new YandexTranslate("trnsl.1.1.20160303T171835Z.a633112472d45a8d.8017a9dc7885242b2a7445c2cc4104683baff4ac", "v1.5");

            this.FormBorderStyle = FormBorderStyle.None;
            this.AllowTransparency = true;
            this.BackColor = Color.AliceBlue;//цвет фона  
            this.TransparencyKey = this.BackColor;

            hookManager.HookedKeys.Add((Keys)Enum.Parse(typeof(Keys), Properties.Settings.Default.HotKey));
            hookManager.KeyDown += keydown_handler;
        }

        private readonly static KeyEventHandler keydown_handler = Global_KeyDown;
        string word = "";
        const int clearTimerDelay = 5;
        int clearTimer;
        private void translateUpdate_Timer_Tick(object sender, EventArgs e)
        {
            Translate();
            if (Properties.Settings.Default.AutoClear)
            {
                if (clearTimer-- <= 0)
                {
                    clearTimer = clearTimerDelay;
                    Clear();
                }
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                Application.Exit();
                break;
                case Keys.Enter:
                Translate();
                break;
            }
        }
        private static void Global_KeyDown(object sender, KeyEventArgs e)
        {
            if (!instance.Visible)
            {
                instance.Show();
            }
            else if(instance.Focused || instance.input_Textbox.Focused)
            {
                instance.Hide();
            }
            else
            {
                instance.WindowState = FormWindowState.Minimized;
                instance.WindowState = FormWindowState.Normal;
                instance.Focus();
            }
        }

        private async void Translate()
        {
            if (word != input_Textbox.Text)
            {
                translations_ListBox.Items.Clear();
                if (!string.IsNullOrWhiteSpace(input_Textbox.Text))
                {
                    word = input_Textbox.Text;
                    var response = await yandexDictionary.TranslateAsyns(fromLanguage, toLanguage, input_Textbox.Text);
                    if (response.Translations.Count > 0)
                    {
                        translations_ListBox.Items.AddRange(response.Translations.ToArray());
                    }
                    else
                    {
                        string translation = await yandexTranslate.TranslateAsyns(fromLanguage, toLanguage, input_Textbox.Text);
                        translations_ListBox.Items.Add(translation);
                    }
                }
                translations_ListBox.Height = translations_ListBox.Items.Count * 13 + 10;
         
                translations_ListBox.Visible = translations_ListBox.Items.Count > 0;
            }
        }
        private void Clear()
        {
            word = "";
            input_Textbox.Text = "";
            translations_ListBox.Items.Clear();
            translations_ListBox.Hide();
        }

        private void input_Textbox_TextChanged(object sender, EventArgs e)
        {
            TextBox box = (TextBox)sender;

            box.Width = box.Text.Length * 5;
            translations_ListBox.Width = box.Text.Length * 5;
            clearTimer = clearTimerDelay;
            if(box.Text.Length == 0)
            {
                this.Opacity = Properties.Settings.Default.Opacity/100d;
                translations_ListBox.Visible = false;
            }
            else
            {
                this.Opacity = 1;
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDownPosition = MousePosition;
            mouseDownPosition.Offset(-Location.X, -Location.Y);
            move_Timer.Enabled = true;
            if (e.Button == MouseButtons.Right) new SettingsForm().Show();
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            move_Timer.Enabled = false;
        }
        Point mouseDownPosition;
        private void move_Timer_Tick(object sender, EventArgs e)
        {
            Point point = MousePosition;
            point.Offset(-mouseDownPosition.X,-mouseDownPosition.Y);
            Location = point;
        }

        private void input_Textbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '`') e.Handled = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.WindowPosition != new Point(-1, -1))
            {
                this.Location = Properties.Settings.Default.WindowPosition;
            }
            this.TopMost = Properties.Settings.Default.TopMost;
            this.Opacity = Properties.Settings.Default.Opacity / 100d;
            this.ShowInTaskbar = Properties.Settings.Default.ShowInTaskbar;


            fromLanguage = (Yandex.Language)Enum.Parse(typeof(Yandex.Language), Properties.Settings.Default.FromLanguage, true);
            toLanguage = (Yandex.Language)Enum.Parse(typeof(Yandex.Language), Properties.Settings.Default.ToLanguage, true);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.WindowPosition = this.Location;
            Properties.Settings.Default.Save();
            //Clipboard.SetText(string.Join(Environment.NewLine, Enum.GetNames(typeof(Keys))));
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void настройкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new SettingsForm().Show();
        }

        private void tray_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                this.Show();
            }
        }
        
        private void Form1_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                translateUpdate_Timer.Start();
            }
            else
            {
                if(Properties.Settings.Default.AutoClear) Clear();
                move_Timer.Stop();
                translateUpdate_Timer.Stop();
            }
        }
    }
}