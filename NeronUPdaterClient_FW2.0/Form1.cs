using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;

namespace NeronUPdaterClient_FW2._0
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        string[] conf;
        int cv, sv,cfd;
        string ServerCatalog,NeronCatalog;
        const int progvers=3;
        bool uper;
        string curcat = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        FastZip zip = new FastZip();
        int errorz;
        private void FTPLOAD(string URL, string login, string password, string filename, string outpt, bool open)
        {
            WebClient FTPSERVER = new WebClient();
            FTPSERVER.Credentials = new NetworkCredential(login, password);
            try
            {
                byte[] newFileData = FTPSERVER.DownloadData("ftp://" + URL + filename);
                System.IO.File.WriteAllBytes(curcat + @"\" + outpt, newFileData);
                if (open)
                    System.Diagnostics.Process.Start(outpt);
                errorz = 0;
            }
            catch (WebException e)
            {
                errorz ++;
                if (errorz == 1) { notifyIcon1.ShowBalloonTip(3, "Проблемы с соединением", "Перебой в работе с сервером обновлений.", ToolTipIcon.Info); }
                if (errorz == 3)
                {
                 System.IO.File.AppendAllText("logs.txt", System.DateTime.Now.ToString() + " Erorr: " + e.ToString() + Environment.NewLine);
                 notifyIcon1.ShowBalloonTip(3, "Проблемы с соединением", "Не удалось восстановить работу с сервером обновлений! Обратитесь за помощью к администратору.", ToolTipIcon.Warning);
                }
            }
        }

            void FD(string FromDir)
            {
              foreach (string s1 in System.IO.Directory.GetFiles(FromDir))
              {
                 cfd++;
              }
              foreach (string s in System.IO.Directory.GetDirectories(FromDir))
              {
                FD(s);
              }
            }

        public static void DeleteDirectory(string path)
        {
            foreach (string directory in Directory.GetDirectories(path))
            {
                DeleteDirectory(directory);
            }

            try
            {
                System.Threading.Thread.Sleep(0);
                Directory.Delete(path, true);
            }
            catch (IOException)
            {
                Directory.Delete(path, true);
            }
            catch (UnauthorizedAccessException)
            {
                Directory.Delete(path, true);
            }
        }

        void CopyDir(string FromDir, string ToDir, bool bUP)
        {
            if (bUP&!System.IO.Directory.Exists(ToDir)) System.IO.File.AppendAllText(@"BackUPs\" + cv.ToString() + @"\delitesdirs.del", ToDir + Environment.NewLine);
            System.IO.Directory.CreateDirectory(ToDir);
            if (bUP) System.IO.Directory.CreateDirectory(@"BackUPs\"+cv.ToString()+@"\"+ ToDir.Remove(0,3));
            foreach (string s1 in System.IO.Directory.GetFiles(FromDir))
            {
                string s2 = ToDir + "\\" + System.IO.Path.GetFileName(s1);
                if (!System.IO.File.Exists(s2) & bUP) { System.IO.File.AppendAllText(@"BackUPs\" + cv.ToString() + @"\delitesfiles.del", s2 + Environment.NewLine); }
                if (System.IO.File.Exists(s2)&bUP) { System.IO.File.Move(s2, @"BackUPs\" + cv.ToString() + @"\" + s2.Remove(0,3)); }
                System.IO.File.Copy(s1, s2,true);
                this.progressBar1.BeginInvoke((MethodInvoker)(delegate
                {
                    progressBar1.Value++;
                }));
                
            }
            foreach (string s in System.IO.Directory.GetDirectories(FromDir))
            {
                CopyDir(s, ToDir + "\\" + System.IO.Path.GetFileName(s),bUP);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            FTPLOAD("192.168.1.39:22/", "ftadmin", "QQwerty1284", "progvers.inf", "servr/progvers.inf", false);
            FTPLOAD("192.168.1.39:22/", "ftadmin", "QQwerty1284", "version.inf", "servr/version.inf", false);
            zip.Password = "cripto3tester";
            if (System.IO.File.Exists("ClientInfo.config")) //Проверка наличия файла конфигурации.
            {
                //При наличии файла, считывыаем из него конфигурацию сервера.
                conf = System.IO.File.ReadAllLines("ClientInfo.config");
                ServerCatalog = conf[0];
                cv = Convert.ToInt32(conf[1]);
                NeronCatalog = conf[2];
            }
            else //Если файл не найден, то запрашиваем необходимую информацию у пользователя и создаем с ней файл конфигурации.
            {
                MessageBox.Show("Выполнен первый запуск программы или ее настройки были сброшены." + Environment.NewLine +
                    "Необходимо выполнить настроку клиента.");
                bool clos = false;//Переменная прерывания программы.
                do
                {

                    ChooseUPcatalog.ShowDialog();
                    if (ChooseUPcatalog.SelectedPath == "")
                    //Если пользователь не указал рабочую папку сервера, то повторяем запрос или закрываем программу.


                    {

                        var result = MessageBox.Show("Работа клиента не возможна без подключенмия к серверу."
                            + Environment.NewLine + "Выйти из программы?", "Внимание!", MessageBoxButtons.YesNo);

                        if (result == DialogResult.Yes)
                        {
                            clos = true;
                        }

                    }
                } while (ChooseUPcatalog.SelectedPath == "" & clos == false);
                if (clos == true) Close(); //Прерывание программы, при true.
                else
                {
                    ServerCatalog = ChooseUPcatalog.SelectedPath;
                    NeronCatalog = @"C:\LPU";
                    cv = 0;
                    System.IO.File.WriteAllText("ClientInfo.config", ServerCatalog + Environment.NewLine + "0" + Environment.NewLine + @"C\LPU");
                }
            }
            textBox1.Text = ServerCatalog; //Выводим путь к рабочей папке сервера на форму.
            textBox4.Text = System.IO.File.ReadAllText("servr/version.inf");
            sv = Convert.ToInt32(textBox4.Text);
            textBox2.Text = "Установлено";
            textBox3.Text = Convert.ToString(cv);
            textBox5.Text = NeronCatalog;

            //Проверка наличия обновлений для самой программы.

            if (Convert.ToInt32(System.IO.File.ReadAllText("servr/progvers.inf")) != progvers)
            {
                FTPLOAD("192.168.1.39:22/", "ftadmin", "QQwerty1284", "clientup/client.exe", "clientup/client.exe", false);
                System.IO.File.WriteAllText("samouper.bat", "timeout /t 5 " + Environment.NewLine
                + @"MOVE /Y clientup\client.exe client.exe" + Environment.NewLine + "start client.exe"
                + Environment.NewLine + "del %~s0 /q");
                uper = true;
                this.Close();
            }
            textBox6.Text = "Ожидание появления нового обновления.";
            timer1.Enabled = true;
        }

        private void аToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Авторские права.
            MessageBox.Show("Михалко Андрей Владимирович"+Environment.NewLine
                +"Тел: +79147304313" +Environment.NewLine
                +"Email: mihandr1@mail.ru", "Автор");
        }

        private void спрятатьФормуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Свернуть программу в трей.
            WindowState = FormWindowState.Minimized;
            Opacity = 0;
            notifyIcon1.Visible = true;
        }

        private void закрытьКлиентToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Штатный выход из программы.
            var result = MessageBox.Show("Вы действительно хотите закрыть Neron UPdater Client?", "Подтвердите действие", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                Close();
            }
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            //Развернуть программу из трея.
            notifyIcon1.Visible = false;
            WindowState = FormWindowState.Normal;
            Opacity = 100;
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (uper)
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "samouper.bat",  // Путь к приложению
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                System.Diagnostics.Process.Start(startInfo);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void сборкаToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

            if (cv > sv)
            {
                this.textBox6.BeginInvoke((MethodInvoker)(delegate
                {
                    textBox6.Text = "Откат обновлений.";
                }));
                notifyIcon1.ShowBalloonTip(10, "Проезводится возврат к прошлым версиям Neron!", "Начато восстановление файлов, не формируйте никакие отчеты до его окончания!", ToolTipIcon.Info);
                while (cv>sv)
                {
                    int tocv = cv - 1;
                    cfd = 0;
                    FD(@"BackUPs\" + tocv.ToString() + @"\LPU");
                    this.progressBar1.BeginInvoke((MethodInvoker)(delegate
                    {
                        progressBar1.Maximum = cfd;
                        progressBar1.Value = 0;
                    }));
                    CopyDir(@"BackUPs\" + tocv.ToString() + @"\LPU", NeronCatalog, false);
                    //Удаление добавленных в прошлом обновлении файлов.
                    if (System.IO.File.Exists(@"BackUPs\" + tocv.ToString() + @"\delitesfiles.del"))
                    {
                        string[] idel = new string[System.IO.File.ReadAllLines(@"BackUPs\" + tocv.ToString() + @"\delitesfiles.del").Length];
                        idel = System.IO.File.ReadAllLines(@"BackUPs\" + tocv.ToString() + @"\delitesfiles.del");
                        foreach (string df in idel)
                        {
                            if (System.IO.File.Exists(df)) System.IO.File.Delete(df);
                        }
                    }
                    if (System.IO.File.Exists(@"BackUPs\" + tocv.ToString() + @"\delitesdirs.del"))
                    {
                        string[] ideld = new string[System.IO.File.ReadAllLines(@"BackUPs\" + tocv.ToString() + @"\delitesdirs.del").Length];
                        ideld = System.IO.File.ReadAllLines(@"BackUPs\" + tocv.ToString() + @"\delitesdirs.del");
                        foreach (string dd in ideld)
                        {
                                if (System.IO.Directory.Exists(dd)) DeleteDirectory(dd);
                        }
                    }
                    DeleteDirectory(@"BackUPs\" + tocv.ToString());
                    cv = tocv;
                    this.textBox3.BeginInvoke((MethodInvoker)(delegate
                    {
                        textBox3.Text = Convert.ToString(cv);

                    }));
                    System.IO.File.WriteAllText("ClientInfo.config", ServerCatalog + Environment.NewLine + Convert.ToString(cv) + Environment.NewLine + @"C:\LPU");
                    this.textBox6.BeginInvoke((MethodInvoker)(delegate
                    {
                        progressBar1.Value = 0;
                    }));
                }
                this.textBox6.BeginInvoke((MethodInvoker)(delegate
                {
                    textBox6.Text = "Ожидание появления нового обновления.";
                    progressBar1.Value = 0;
                }));
                notifyIcon1.ShowBalloonTip(5, "Восстановление файлов завершено!", "Вы можеет продолжить формировать отчеты.", ToolTipIcon.Info);
            }
            else
            {
                this.textBox6.BeginInvoke((MethodInvoker)(delegate
                {
                    textBox6.Text = "Найдено обновление, загрузка.";
                }));
                notifyIcon1.ShowBalloonTip(10, "Найдено новое обновление для Neron!", "Начата загрузка обновления, не формируйте никакие отчеты до его окончания!", ToolTipIcon.Info);
                while (cv < sv)
                {
                    int tocv = cv + 1;
                    cfd = 0;
                    FTPLOAD("192.168.1.39:22/", "ftadmin", "QQwerty1284", "UpDats/"+ tocv.ToString() +"/UpData.zip", "servr/UpData.zip", false);
                    zip.ExtractZip("servr/UpData.zip", "servr", null);
                    System.IO.File.Delete("servr/UpData.zip");
                    FD("servr/LPU");
                    this.progressBar1.BeginInvoke((MethodInvoker)(delegate
                    {
                        progressBar1.Maximum = cfd;
                        progressBar1.Value = 0;
                    }));
                    CopyDir("servr/LPU", NeronCatalog, true);
                    System.IO.Directory.Delete("servr/LPU", true);
                    cv = tocv;
                    this.textBox3.BeginInvoke((MethodInvoker)(delegate
                    {
                        textBox3.Text = Convert.ToString(cv);

                    }));
                    System.IO.File.WriteAllText("ClientInfo.config", ServerCatalog + Environment.NewLine + Convert.ToString(cv) + Environment.NewLine + @"C:\LPU");
                    this.textBox6.BeginInvoke((MethodInvoker)(delegate
                    {
                        progressBar1.Value = 0;
                    }));
                }
                this.textBox6.BeginInvoke((MethodInvoker)(delegate
                {
                    textBox6.Text = "Ожидание появления нового обновления.";
                    progressBar1.Value = 0;
                }));
                notifyIcon1.ShowBalloonTip(5, "Обновление завершено!", "Вы можеет продолжить формировать отчеты.", ToolTipIcon.Info);
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
           
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            FTPLOAD("192.168.1.39:22/", "ftadmin", "QQwerty1284", "progvers.inf", "servr/progvers.inf", false);
            if (Convert.ToInt32(System.IO.File.ReadAllText("servr/progvers.inf")) != progvers)
            {
                    FTPLOAD("192.168.1.39:22/", "ftadmin", "QQwerty1284", "clientup/client.exe", "clientup/client.exe", false);
                    System.IO.File.WriteAllText("samouper.bat", "timeout /t 5 " + Environment.NewLine 
                    + @"MOVE /Y clientup\client.exe client.exe" + Environment.NewLine + "start client.exe" 
                    + Environment.NewLine + "del %~s0 /q");
                    uper = true;
                    this.Close();
            }
            FTPLOAD("192.168.1.39:22/", "ftadmin", "QQwerty1284", "version.inf", "servr/version.inf", false);
            textBox4.Text = System.IO.File.ReadAllText("servr/version.inf");
            sv = Convert.ToInt32(textBox4.Text);
            if (cv != sv & !backgroundWorker1.IsBusy & sv==0)
            {

                System.IO.File.WriteAllText("BuCl.bat", "rd / s / q"+ "\"" + curcat + @"\BackUPs" + "\"");

                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "BuCl.bat",  // Путь к приложению
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                System.Diagnostics.Process.Start(startInfo).WaitForExit();

                System.IO.Directory.CreateDirectory("BackUPs");
                cv = sv;
                textBox3.Text = cv.ToString();
                System.IO.File.WriteAllText("ClientInfo.config", ServerCatalog + Environment.NewLine + Convert.ToString(cv) + Environment.NewLine + @"C:\LPU");
                zip.CreateZip(curcat+ @"\BackUPs\BuCLIENT.zip"+System.DateTime.Now.ToShortDateString(), NeronCatalog, true, null);
            }
            if (cv != sv & !backgroundWorker1.IsBusy)
                backgroundWorker1.RunWorkerAsync();
        }
    }
}
