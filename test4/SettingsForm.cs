using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace test4
{
    public partial class SettingsForm : Form
    {
        bool motiv = Properties.Settings.Default.motiv;

        private Form1 mainForm;
        public SettingsForm(Form1 mainForm)
        {
            InitializeComponent();
            this.mainForm = mainForm;

            int centerX = this.ClientSize.Width / 2;
            int centerY = this.ClientSize.Height / 2;

            label2.Location = new Point(centerX - (label2.Width / 2), centerY - (label2.Height / 2) - 110);

            label1.Location = new Point(centerX - (label1.Width / 2), centerY - (label1.Height / 2) - 60);

            textBox1.Location = new Point(centerX - (textBox1.Width / 2), centerY - (textBox1.Height / 2) - 40); 

            button3.Location = new Point(centerX - (button3.Width / 2), centerY - (button3.Height / 2) - 15); 

            button1.Location = new Point(centerX - (button1.Width / 2), centerY - (button1.Height / 2) + 30); 

            button2.Location = new Point(centerX - (button2.Width / 2), centerY - (button2.Height / 2) + 70); 

        }
        private void SettingsForm_Load(object sender, EventArgs e)
        {
            button1.Paint += new PaintEventHandler(myButton_Paint1);
            button1.MouseEnter += MyButton_MouseEnter;
            button1.MouseLeave += MyButton_MouseLeave;

            button2.Paint += new PaintEventHandler(myButton_Paint2);
            button2.MouseEnter += MyButton_MouseEnter;
            button2.MouseLeave += MyButton_MouseLeave;

            button3.Paint += new PaintEventHandler(myButton_Paint3);
            button3.MouseEnter += MyButton_MouseEnter;
            button3.MouseLeave += MyButton_MouseLeave;

            if (motiv)
            {
                //tmavý motiv
                panel1.BackColor = ColorTranslator.FromHtml("#151515");
                label1.BackColor = ColorTranslator.FromHtml("#151515");
                label1.ForeColor = Color.White;
                label2.BackColor = ColorTranslator.FromHtml("#151515");
                label2.ForeColor = Color.White;
                textBox1.BackColor = ColorTranslator.FromHtml("#151515");
                textBox1.ForeColor = Color.White;
            }
            else
            {
                //světlý motiv
                panel1.BackColor = ColorTranslator.FromHtml("#f0f0f0");
                label1.BackColor = ColorTranslator.FromHtml("#f0f0f0");
                label1.ForeColor = Color.Black;
                label2.BackColor = ColorTranslator.FromHtml("#f0f0f0");
                label2.ForeColor = Color.Black;
                textBox1.BackColor = ColorTranslator.FromHtml("#f0f0f0");
                textBox1.ForeColor = Color.Black;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            mainForm.ChangeMotiv();

            motiv = !motiv;

            if (motiv)
            {
                //tmavý motiv
                panel1.BackColor = ColorTranslator.FromHtml("#151515");
                label1.BackColor = ColorTranslator.FromHtml("#151515");
                label1.ForeColor = Color.White;
                label2.BackColor = ColorTranslator.FromHtml("#151515");
                label2.ForeColor = Color.White;
                textBox1.BackColor = ColorTranslator.FromHtml("#151515");
                textBox1.ForeColor = Color.White;
            }
            else
            {
                //světlý motiv
                panel1.BackColor = ColorTranslator.FromHtml("#f0f0f0");
                label1.BackColor = ColorTranslator.FromHtml("#f0f0f0");
                label1.ForeColor = Color.Black;
                label2.BackColor = ColorTranslator.FromHtml("#f0f0f0");
                label2.ForeColor = Color.Black;
                textBox1.BackColor = ColorTranslator.FromHtml("#f0f0f0");
                textBox1.ForeColor = Color.Black;
            }

            //uloží nastavení motivu
            Properties.Settings.Default.motiv = motiv;
            Properties.Settings.Default.Save();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            mainForm.ChangeMotiv2();


            motiv = IsDarkThemeEnabled();

            if (motiv)
            {
                //tmavý motiv
                panel1.BackColor = ColorTranslator.FromHtml("#151515");
                label1.BackColor = ColorTranslator.FromHtml("#151515");
                label1.ForeColor = Color.White;
                label2.BackColor = ColorTranslator.FromHtml("#151515");
                label2.ForeColor = Color.White;
                textBox1.BackColor = ColorTranslator.FromHtml("#151515");
                textBox1.ForeColor = Color.White;
            }
            else
            {
                //světlý motiv
                panel1.BackColor = ColorTranslator.FromHtml("#f0f0f0");
                label1.BackColor = ColorTranslator.FromHtml("#f0f0f0");
                label1.ForeColor = Color.Black;
                label2.BackColor = ColorTranslator.FromHtml("#f0f0f0");
                label2.ForeColor = Color.Black;
                textBox1.BackColor = ColorTranslator.FromHtml("#f0f0f0");
                textBox1.ForeColor = Color.Black;
            }

            Properties.Settings.Default.motiv = motiv;
            Properties.Settings.Default.Save();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            mainForm.RefreshRate(textBox1.Text);
            textBox1.Text = "";
        }

        private void myButton_Paint1(object sender, PaintEventArgs e)
        {
            var buttonRect = button1.ClientRectangle;

            e.Graphics.FillRectangle(Brushes.SkyBlue, buttonRect);

            Image image = Properties.Resources.motiv;

            int imageX = 5;
            int imageY = (buttonRect.Height - image.Height) / 2;
            Rectangle imageRect = new Rectangle(imageX, imageY, image.Width, image.Height);

            e.Graphics.DrawImage(image, imageRect);

            using (Font boldFont = new Font("Arial", 10, FontStyle.Bold))
            {
                using (StringFormat sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;

                    var textRect = new Rectangle(imageRect.Right, buttonRect.Y, buttonRect.Width - imageRect.Right - 5, buttonRect.Height);

                    e.Graphics.DrawString("Motiv", boldFont, Brushes.Black, textRect, sf);
                }
            }
        }

        private void myButton_Paint2(object sender, PaintEventArgs e)
        {
            var buttonRect = button2.ClientRectangle;

            e.Graphics.FillRectangle(Brushes.SkyBlue, buttonRect);

            using (Font boldFont = new Font("Segoe UI", 9, FontStyle.Bold))
            {
                using (StringFormat sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;

                    e.Graphics.DrawString("Systémový motiv", boldFont, Brushes.Black, buttonRect, sf);
                }
            }
        }

        private void myButton_Paint3(object sender, PaintEventArgs e)
        {
            var buttonRect = button3.ClientRectangle;

            e.Graphics.FillRectangle(Brushes.SkyBlue, buttonRect);

            using (Font boldFont = new Font("Arial", 8, FontStyle.Bold))
            {
                using (StringFormat sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;

                    e.Graphics.DrawString("Změnit", boldFont, Brushes.Black, buttonRect, sf);
                }
            }
        }

        private void MyButton_MouseEnter(object sender, EventArgs e)
        {
            Cursor = Cursors.Hand;
        }

        private void MyButton_MouseLeave(object sender, EventArgs e)
        {
            Cursor = Cursors.Default;
        }

        private void SettingsForm_Resize(object sender, EventArgs e)
        {
            int centerX = this.ClientSize.Width / 2;
            int centerY = this.ClientSize.Height / 2;

            button1.Location = new Point(centerX - (button1.Width / 2), centerY - (button1.Height / 2) - 40);

            button2.Location = new Point(centerX - (button2.Width / 2), centerY - (button2.Height / 2) - 0);

            button3.Location = new Point(centerX - (button3.Width / 2), centerY - (button3.Height / 2) + 90);

            label1.Location = new Point(centerX - (label1.Width / 2), centerY - (label1.Height / 2) + 40);

            textBox1.Location = new Point(centerX - (textBox1.Width / 2), centerY - (textBox1.Height / 2) + 60);
        }
        public static bool IsDarkThemeEnabled()
        {
            const string keyName = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
            const string valueName = "AppsUseLightTheme";

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyName))
            {
                if (key != null)
                {
                    object value = key.GetValue(valueName);
                    if (value is int intValue)
                    {
                        //0 -> tmavý motiv zap.
                        return intValue == 0;
                    }
                }
            }

            //výchozí
            return false;
        }
    }
}
