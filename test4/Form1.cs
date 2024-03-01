using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Management;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net.NetworkInformation;
using Microsoft.Win32;

namespace test4
{
    public partial class Form1 : Form
    {
        private Timer timer = new Timer();
        private Timer odpoctovyTimer = new Timer();
        private int odpoctoveSekundy = 6;
        int odpocitavac = 7;
        bool motiv = Properties.Settings.Default.motiv;

        public Form1()
        {
            InitializeComponent();

            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Trace.Listeners.Add(new TextWriterTraceListener("protokoly.log"));
            Trace.AutoFlush = true;

            //Načtení uloženého intervalu a nastavení timeru
            int intervalInSeconds = Properties.Settings.Default.TimerInterval;
            timer.Interval = intervalInSeconds * 1000;
            odpocitavac = intervalInSeconds;
            odpoctoveSekundy = intervalInSeconds;

            timer.Tick += Timer_Tick;
            timer.Start();

            this.timer1.Interval = 1000;
            this.timer1.Tick += new EventHandler(timerMessage_Tick);

            //odpočítávač
            odpoctovyTimer.Interval = 1000;
            odpoctovyTimer.Tick += OdpoctovyTimer_Tick;
            odpoctovyTimer.Start();

            this.Resize += new EventHandler(Form1_Resize);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            AktualizujSíťováSpojení();
            odpoctoveSekundy = odpocitavac;
        }

        private void OdpoctovyTimer_Tick(object sender, EventArgs e)
        {
            //aktualizace textboxu
            odpoctoveSekundy--;
            string sekundy = odpoctoveSekundy.ToString();
            textBox1.Text = $"Refresh za {sekundy} s";
            if (odpoctoveSekundy <= 0)
            {
                odpoctoveSekundy = 7;
            }
            CenterRichTextBox();
            design();
        }
        private void timerMessage_Tick(object sender, EventArgs e)
        {
            //skryje zprávu a zastaví Timer
            label1.Text = "";
            timer1.Stop();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AktualizujSíťováSpojení();
            AdjustFormSizeToContent();
            CenterRichTextBox();

            design();

            textBox1.BorderStyle = BorderStyle.None;
            textBox1.BackColor = Color.White; 
            textBox1.ReadOnly = true;
            textBox1.Cursor = Cursors.Default;
            textBox1.Multiline = true;
            textBox1.TextAlign = HorizontalAlignment.Left;

            textBox1.BackColor = ColorTranslator.FromHtml("#f0f0f0");
            textBox1.ForeColor = Color.Black;

            //velikost okna
            int numberOfLines = richTextBox1.Lines.Length;
            int lineHeight = TextRenderer.MeasureText("Test", richTextBox1.Font).Height;
            int newHeight = numberOfLines * lineHeight + 100;
            this.Height = newHeight + 70;
            this.Width = 650;

            if (motiv)
            {
                //tmavý motiv
                panel1.BackColor = ColorTranslator.FromHtml("#151515");
                richTextBox1.BackColor = ColorTranslator.FromHtml("#151515");
                richTextBox1.ForeColor = Color.White;
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
                richTextBox1.BackColor = ColorTranslator.FromHtml("#f0f0f0");
                richTextBox1.ForeColor = Color.Black;
                label1.BackColor = ColorTranslator.FromHtml("#f0f0f0");
                label1.ForeColor = Color.Black;
                label2.BackColor = ColorTranslator.FromHtml("#f0f0f0");
                label2.ForeColor = Color.Black;
                textBox1.BackColor = ColorTranslator.FromHtml("#f0f0f0");
                textBox1.ForeColor = Color.Black;
            }
        }

        private void AktualizujSíťováSpojení()
        {
            richTextBox1.Clear();
            comboBox1.Items.Clear();

            int pocetSpojeni = ZjistiAktivniSpojeni();
            while (pocetSpojeni == 0)
            {
                Trace.WriteLine("Zjišťování aktivního síťového spojení selhalo.");
                MessageBox.Show("Patrně není připojen síťový kabel. Zkouším znovu...", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                pocetSpojeni = ZjistiAktivniSpojeni();
                System.Threading.Thread.Sleep(2000);
            }

            var objSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = True");

            //zjisti spojení a přidá to do richtextboxu
            List<string> pole = new List<string>();
            string aktivniSpojeni = $"Počet aktivních spojení: {pocetSpojeni}";
            string defaultGateway = $"\nDefault Gateway: {GetDefaultGatewayUsingIpConfig()}";

            pole.Add($"{aktivniSpojeni}{defaultGateway}");

            foreach (ManagementObject obj in objSearcher.Get())
            {
                if (obj["IPAddress"] == null || obj["IPSubnet"] == null) continue;

                string ipAdresa = ((string[])obj["IPAddress"])[0];
                string maska = ((string[])obj["IPSubnet"])[0];
                bool dhcp = (bool)obj["DHCPEnabled"];
                string oznameniIP = dhcp ? "DHCP                  povoleno" : "DHCP                  zakázán";
                oznameniIP += $"\nIP adresa            {ipAdresa}";
                oznameniIP += $"\nMaska                 {maska}";

                string mac = (string)obj["MACAddress"];
                if (mac != null)
                {
                    oznameniIP += $"\nMAC adresa       {mac}";
                }
                else
                {
                    oznameniIP += $"\nMAC adresa       nenalezeno";
                }

                pole.Add($"{oznameniIP}");

                comboBox1.Items.Add(ipAdresa);
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < pole.Count; i++)
            {
                sb.AppendLine(pole[i]);
                if (i < pole.Count - 1)
                {
                    sb.AppendLine("--------");
                }
            }

            design();

            richTextBox1.Text = sb.ToString();


        }
        static int ZjistiAktivniSpojeni()
        {
            var objSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionStatus=2");
            var objCollection = objSearcher.Get();
            return objCollection.Count;
        }

        //design buttonu
        private void myButton_Paint(object sender, PaintEventArgs e)
        {
            //Získání oblasti tlačítka
            var buttonRect = button1.ClientRectangle;

            e.Graphics.FillRectangle(Brushes.SkyBlue, buttonRect);

            //načtení obrázku
            Image image = Properties.Resources.zavrit;

            int imageX = 5;
            int imageY = (buttonRect.Height - image.Height) / 2;
            Rectangle imageRect = new Rectangle(imageX, imageY, image.Width, image.Height);

            e.Graphics.DrawImage(image, imageRect);

            using (Font boldFont = new Font("Arial", 10, FontStyle.Bold))
            {
                //Nastavení zarovnání textu na střed
                using (StringFormat sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;

                    var textRect = new Rectangle(imageRect.Right, buttonRect.Y, buttonRect.Width - imageRect.Right - 5, buttonRect.Height);

                    e.Graphics.DrawString("Zavřít", boldFont, Brushes.Black, textRect, sf);
                }
            }
        }
        private void myButton_Paint2(object sender, PaintEventArgs e)
        {

            var buttonRect = button2.ClientRectangle;

            
            e.Graphics.FillRectangle(Brushes.SkyBlue, buttonRect);

            
            Image image = Properties.Resources.copy3;

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

                    e.Graphics.DrawString("Kopírovat", boldFont, Brushes.Black, textRect, sf);
                }
            }
        }

        private void myButton_Paint4(object sender, PaintEventArgs e)
        {
            var buttonRect = button4.ClientRectangle;

            e.Graphics.FillRectangle(Brushes.SkyBlue, buttonRect);

            using (Font boldFont = new Font("Arial", 8, FontStyle.Bold))
            {
                using (StringFormat sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;

                    e.Graphics.DrawString("Ping", boldFont, Brushes.Black, buttonRect, sf);
                }
            }
        }

        private void myButton_Paint7(object sender, PaintEventArgs e)
        {

            var buttonRect = button7.ClientRectangle;


            e.Graphics.FillRectangle(Brushes.SkyBlue, buttonRect);


            Image image = Properties.Resources.settings3;

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

                    e.Graphics.DrawString("Nastavení", boldFont, Brushes.Black, textRect, sf);
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

        private void Form1_Resize(object sender, EventArgs e)
        {
            design();

            CenterRichTextBox();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            AdjustFormSizeToContent();
        }

        private void AdjustFormSizeToContent()
        {
            //nastavení velikosti richtextboxu
            int numberOfLines = richTextBox1.Lines.Length;
            int lineHeight = TextRenderer.MeasureText("Test", richTextBox1.Font).Height;
            int newHeight = numberOfLines * lineHeight;

            richTextBox1.Height = (richTextBox1.Lines.Length * lineHeight);
            richTextBox1.Width = 350;

            CenterRichTextBox();
        }

        //zarovnání richtextbox na střed
        private void CenterRichTextBox()
        {
            int centerX = this.ClientSize.Width / 2;
            int centerY = this.ClientSize.Height / 2;

            int newX = centerX - (richTextBox1.Width / 2);
            int newY = centerY - (richTextBox1.Height / 2);

            richTextBox1.Location = new Point(newX, newY);
        }

        private void design()
        {
            richTextBox1.Left = (this.ClientSize.Width - richTextBox1.Width) / 2;
            richTextBox1.Top = (this.ClientSize.Height - richTextBox1.Height) / 2;

            button1.Top = richTextBox1.Bottom - 8;
            button1.Left = richTextBox1.Left + (richTextBox1.Width / 2) - (button1.Width / 2) + 80;

            button2.Top = richTextBox1.Bottom - 8;
            button2.Left = richTextBox1.Left + (richTextBox1.Width / 2) - (button1.Width / 2) - 50;

            button4.Top = richTextBox1.Bottom + 20;
            button4.Left = richTextBox1.Left + (richTextBox1.Width / 2) - (button1.Width / 2) + 215;

            button7.Top = richTextBox1.Bottom - 8;
            button7.Left = richTextBox1.Left + (richTextBox1.Width / 2) - (button1.Width / 2) - 180;

            label1.Top = richTextBox1.Bottom + 27;
            label1.Left = richTextBox1.Left + (richTextBox1.Width / 2) - (button1.Width / 2) - 57;

            label2.Top = richTextBox1.Bottom - 30;
            label2.Left = richTextBox1.Left + (richTextBox1.Width / 2) - (button1.Width / 2) + 190;

            comboBox1.Top = richTextBox1.Bottom - 10;
            comboBox1.Left = richTextBox1.Left + (richTextBox1.Width / 2) - (button1.Width / 2) + 190;

            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.Font = new Font("Segoe UI", 9);
            comboBox1.ForeColor = Color.Black;
            comboBox1.BackColor = Color.White;
            comboBox1.Width = 110;
            comboBox1.Height = 25;

            textBox1.Width = richTextBox1.Width;
            textBox1.Height = 15;
            textBox1.Location = new Point(richTextBox1.Location.X + 2, richTextBox1.Location.Y - textBox1.Height);

            //tohle je v aktualizujSitovaSpojeni()
            richTextBox1.Font = new Font(richTextBox1.Font.Name, 15, richTextBox1.Font.Style);

            richTextBox1.ReadOnly = true;
            richTextBox1.Multiline = true;
            richTextBox1.WordWrap = true;
            richTextBox1.BorderStyle = BorderStyle.None;
            richTextBox1.Cursor = Cursors.Default;

            button1.Paint += new PaintEventHandler(myButton_Paint);
            button1.MouseEnter += MyButton_MouseEnter;
            button1.MouseLeave += MyButton_MouseLeave;

            button2.Paint += new PaintEventHandler(myButton_Paint2);
            button2.MouseEnter += MyButton_MouseEnter;
            button2.MouseLeave += MyButton_MouseLeave;

            button4.Paint += new PaintEventHandler(myButton_Paint4);
            button4.MouseEnter += MyButton_MouseEnter;
            button4.MouseLeave += MyButton_MouseLeave;

            button7.Paint += new PaintEventHandler(myButton_Paint7);
            button7.MouseEnter += MyButton_MouseEnter;
            button7.MouseLeave += MyButton_MouseLeave;
        }

        //buttony
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(richTextBox1.Text);

            label1.Text = "Zkopírováno do schránky";
            timer1.Start();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {
                string selectedIP = comboBox1.SelectedItem.ToString();
                bool isPingSuccessful = PingGateway(selectedIP);
                MessageBox.Show(isPingSuccessful ? "Ping byl úspěšný." : "Ping selhal.", "Výsledek ping",
                    MessageBoxButtons.OK, isPingSuccessful ? MessageBoxIcon.Information : MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show("Vyberte prosím IP adresu z nabídky.", "Upozornění", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            SettingsForm settingsForm = new SettingsForm(this); // Předáváme 'this' jako referenci na hlavní formulář
            settingsForm.ShowDialog();
        }

        private bool PingGateway(string ipAddress)
        {
            using (Ping pingSender = new Ping())
            {
                PingOptions options = new PingOptions();

                options.Ttl = 128;
                options.DontFragment = true;

                //Testovací data pro ping paket.
                string data = "eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee";
                byte[] buffer = System.Text.Encoding.ASCII.GetBytes(data);

                int timeout = 1200;

                try
                {
                    PingReply reply = pingSender.Send(ipAddress, timeout, buffer, options);
                    if (reply.Status == IPStatus.Success)
                    {
                        return true; //ping byl úspěšný
                    }
                }
                catch (Exception ex)
                {
                    //Trace.WriteLine($"Ping selhal: {ex.Message}");
                }

                return false; //ping selhal
            }
        }

        public static string GetDefaultGatewayUsingIpConfig()
        {
            ProcessStartInfo psi = new ProcessStartInfo("ipconfig", "/all");
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;

            string output;
            using (Process process = Process.Start(psi))
            {
                output = process.StandardOutput.ReadToEnd();
            }

            string gateway = null;
            var lines = output.Split('\n');
            foreach (var line in lines)
            {
                if (line.Contains("Default Gateway"))
                {
                    gateway = line.Split(':').Last().Trim();
                    if (!string.IsNullOrEmpty(gateway) && gateway != "0.0.0.0")
                    {
                        break;
                    }
                }
            }

            return gateway;
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

        //---nastavení---
        public void ChangeMotiv()
        {
            motiv = !motiv;

            if (motiv)
            {
                //tmavý motiv
                panel1.BackColor = ColorTranslator.FromHtml("#151515");
                richTextBox1.BackColor = ColorTranslator.FromHtml("#151515");
                richTextBox1.ForeColor = Color.White;
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
                richTextBox1.BackColor = ColorTranslator.FromHtml("#f0f0f0");
                richTextBox1.ForeColor = Color.Black;
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

        public void ChangeMotiv2()
        {
            motiv = IsDarkThemeEnabled();

            if (motiv)
            {
                //tmavý motiv
                panel1.BackColor = ColorTranslator.FromHtml("#151515");
                richTextBox1.BackColor = ColorTranslator.FromHtml("#151515");
                richTextBox1.ForeColor = Color.White;
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
                richTextBox1.BackColor = ColorTranslator.FromHtml("#f0f0f0");
                richTextBox1.ForeColor = Color.Black;
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

        public void RefreshRate(string text)
        {
            //uloží nastavení timer refreshu
            if (int.TryParse(text, out int intervalInSeconds))
            {
                Properties.Settings.Default.TimerInterval = intervalInSeconds;
                Properties.Settings.Default.Save();

                int intervalInMilliseconds = intervalInSeconds * 1000;

                timer.Interval = intervalInMilliseconds;
                odpoctoveSekundy = intervalInSeconds;
                odpocitavac = intervalInSeconds;
                //textBox2.Text = "";

                timer.Stop();
                timer.Start();

                odpoctovyTimer.Stop();
                odpoctovyTimer.Start();
            }
            else
            {
                MessageBox.Show("Zadejte platné číslo pro interval.");
            }
        }

    }
}
