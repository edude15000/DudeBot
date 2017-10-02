using Dropbox.Api;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace updater
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeComponent();
            this.Show();

            // TODO :::: RUN ON THREAD (SO THAT BAR WILL MOVE ASYNCRONOUSLY!
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                if (Directory.GetCurrentDirectory().Contains("bin"))
                {
                    Application.Current.Shutdown();
                }
                else
                {
                    String dudebot = System.IO.Path.GetTempPath() + "dudebotdirectory.txt";
                    String dudebotupdateinfo = System.IO.Path.GetTempPath() + "dudebotupdateinfo.txt";
                    String dudebotdirectory = "";
                    String backupdir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\backupdudebot\\";
                    using (StreamReader sr = new StreamReader(dudebot))
                    {
                        dudebotdirectory = sr.ReadToEnd();
                    }
                    try
                    {
                        using (MyWebClient client = new MyWebClient())
                        {
                            String link = "";
                            try
                            {
                                string s = client.DownloadString("http://dudebot.webs.com/");
                                if (s.Contains("https://www.dropbox.com/"))
                                {
                                    String[] info = s.Split(new string[] { "href=" }, StringSplitOptions.None);
                                    for (int i = 0; i < info.Length; i++)
                                    {
                                        if (info[i].Contains("https://www.dropbox.com/"))
                                        {
                                            link = info[i];
                                            link = link.Substring(link.IndexOf('"') + 1, link.IndexOf("?dl=1") + 5);
                                            int index = link.IndexOf('"');
                                            if (index > 0)
                                            {
                                                link = link.Substring(0, index);
                                            }
                                            link = link.Replace(" ", "");
                                        }
                                    }
                                    using (WebClient webClient = new WebClient())
                                    {
                                        if (link == null)
                                        {
                                            System.IO.File.WriteAllText(dudebotupdateinfo, "fail");
                                            MessageBox.Show("Failed to update! Try again later!");
                                            Application.Current.Shutdown();
                                        }
                                        webClient.DownloadFile(new Uri(link), dudebotdirectory + "DudeBotNew.zip");
                                    }
                                    try
                                    {
                                        // Copy to back up directory
                                        if (Directory.Exists(backupdir))
                                        {
                                            Directory.Delete(backupdir, true);
                                        }
                                        Directory.CreateDirectory(backupdir);
                                        DirectoryCopy(dudebotdirectory + "dudebot", backupdir, true);
                                    }
                                    catch (Exception e)
                                    {
                                        MessageBox.Show(e.ToString());
                                        System.IO.File.WriteAllText(dudebotupdateinfo, "fail");
                                        MessageBox.Show("Failed to update! Try again later!");
                                        this.Close();
                                    }
                                    try
                                    {
                                        EmptyFolder(new DirectoryInfo(dudebotdirectory + "DudeBot"));
                                        ZipFile.ExtractToDirectory(dudebotdirectory + "DudeBotNew.zip", dudebotdirectory);
                                        File.Delete(dudebotdirectory + "DudeBotNew.zip");
                                        System.IO.File.WriteAllText(dudebotupdateinfo, "DudeBot successfully updated!");
                                    }
                                    catch (Exception)
                                    {
                                        // COPY BACK BACKED UP FOLDER
                                        EmptyFolder(new DirectoryInfo(dudebotdirectory + "DudeBot"));
                                        Directory.CreateDirectory(dudebotdirectory + "dudebot");
                                        DirectoryCopy(backupdir, dudebotdirectory + "dudebot", true);

                                        System.IO.File.WriteAllText(dudebotupdateinfo, "fail");
                                        MessageBox.Show("Failed to contact server! Try again later! \n");
                                    }
                                }
                                else
                                {
                                    System.IO.File.WriteAllText(dudebotupdateinfo, "fail");
                                    MessageBox.Show("Failed to update! Try again later!");
                                }
                            }
                            catch (WebException e)
                            {
                                MessageBox.Show(e.ToString());
                                System.IO.File.WriteAllText(dudebotupdateinfo, "fail");
                                MessageBox.Show("Failed to update! Try again later!");
                            }
                        }
                    }
                    catch (Exception f)
                    {
                        MessageBox.Show(f.StackTrace);
                        System.IO.File.WriteAllText(dudebotupdateinfo, "fail");
                        MessageBox.Show("Failed to update! Try again later!");
                    }

                    Process p = new Process();
                    p.StartInfo.FileName = dudebotdirectory += @"\DudeBot\DudeBot.exe";
                    p.Start();
                }
                Environment.Exit(0);
            }).Start();
            
        }


        private void EmptyFolder(DirectoryInfo directoryInfo)
        {
            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo subfolder in directoryInfo.GetDirectories())
            {
                EmptyFolder(subfolder);
            }
        }

        private class MyWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri uri)
            {
                WebRequest w = base.GetWebRequest(uri);
                w.Timeout = 30000;
                return w;
            }
        }

        private static void DirectoryCopy(
       string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory does not exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }


            // Get the file contents of the directory to copy.
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                // Create the path to the new copy of the file.
                string temppath = Path.Combine(destDirName, file.Name);

                // Copy the file.
                file.CopyTo(temppath, false);
            }

            // If copySubDirs is true, copy the subdirectories.
            if (copySubDirs)
            {

                foreach (DirectoryInfo subdir in dirs)
                {
                    // Create the subdirectory.
                    string temppath = Path.Combine(destDirName, subdir.Name);

                    // Copy the subdirectories.
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}