using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;

namespace updater
{
    public partial class MainWindow : Window
    {
        String dudebotdirectory = "";

        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            Show();
            
            if (Directory.GetCurrentDirectory().Contains("bin"))
            {
                Application.Current.Shutdown();
            }
            else
            {
                String dudebot = Path.GetTempPath() + "dudebotdirectory.txt";
                String dudebotupdateinfo = Path.GetTempPath() + "dudebotupdateinfo.txt";
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
                                        File.WriteAllText(dudebotupdateinfo, "fail");
                                        MessageBox.Show("Failed to update! Try again later!");
                                        Application.Current.Shutdown();
                                    }
                                    webClient.DownloadFile(new Uri(link), Path.GetTempPath() + "DudeBotNew.zip");
                                }
                                try
                                {
                                    // Copy to back up directory
                                    if (Directory.Exists(backupdir))
                                    {
                                        Directory.Delete(backupdir, true);
                                    }
                                    Directory.CreateDirectory(backupdir);
                                    DirectoryCopy(dudebotdirectory, backupdir, true);
                                }
                                catch (Exception e)
                                {
                                    MessageBox.Show(e.ToString());
                                    File.WriteAllText(dudebotupdateinfo, "fail");
                                    MessageBox.Show("Failed to update! Try again later!");
                                    Close();
                                }
                                try
                                {
                                    if (Directory.Exists(Path.GetTempPath() + "Dudebot"))
                                    {
                                        DeleteDirectory(Path.GetTempPath() + "Dudebot");
                                    }

                                    ZipFile.ExtractToDirectory(Path.GetTempPath() + "DudeBotNew.zip", Path.GetTempPath());
                                    
                                    CopyNewFiles();

                                    if (File.Exists(dudebotdirectory + "DudeBotNew.zip"))
                                    {

                                        File.Delete(dudebotdirectory + "DudeBotNew.zip");
                                    }

                                    File.WriteAllText(dudebotupdateinfo, "DudeBot successfully updated!");
                                }
                                catch (Exception e)
                                {
                                    Debug.WriteLine(e);
                                    File.WriteAllText(dudebotupdateinfo, "fail");
                                    MessageBox.Show("Failed to contact server! Try again later! \n");
                                }
                            }
                            else
                            {
                                File.WriteAllText(dudebotupdateinfo, "fail");
                                MessageBox.Show("Failed to update! Try again later!");
                            }
                        }
                        catch (WebException e)
                        {
                            MessageBox.Show(e.ToString());
                            File.WriteAllText(dudebotupdateinfo, "fail");
                            MessageBox.Show("Failed to update! Try again later!");
                        }
                    }
                }
                catch (Exception f)
                {
                    MessageBox.Show(f.StackTrace);
                    File.WriteAllText(dudebotupdateinfo, "fail");
                    MessageBox.Show("Failed to update! Try again later!");
                }

                Process p = new Process();
                p.StartInfo.FileName = dudebotdirectory += @"\DudeBot.exe";
                p.Start();
            }
            Application.Current.Shutdown();
        }

        public static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }

        public void CopyNewFiles()
        {
            if (File.Exists(dudebotdirectory + "DudeBot.exe"))
            {
                File.Delete(dudebotdirectory + "DudeBot.exe");
            }
            File.Copy(Path.GetTempPath() + @"DudeBot\DudeBot.exe", dudebotdirectory + "DudeBot.exe");
            if (File.Exists(dudebotdirectory + "DudeBot.exe.config"))
            {
                File.Delete(dudebotdirectory + "DudeBot.exe.config");
            }
            File.Copy(Path.GetTempPath() + @"DudeBot\DudeBot.exe.config", dudebotdirectory + "DudeBot.exe.config");
            if (File.Exists(dudebotdirectory + "README.txt"))
            {
                File.Delete(dudebotdirectory + "README.txt");
            }
            File.Copy(Path.GetTempPath() + @"DudeBot\README.txt", dudebotdirectory + "README.txt");
            if (File.Exists(dudebotdirectory + @"bin\en_US.aff"))
            {
                File.Delete(dudebotdirectory + @"bin\en_US.aff");
            }
            File.Copy(Path.GetTempPath() + @"DudeBot\bin\en_US.aff", dudebotdirectory + @"bin\en_US.aff");
            if (File.Exists(dudebotdirectory + @"bin\en_US.dic"))
            {
                File.Delete(dudebotdirectory + @"bin\en_US.dic");
            }
            File.Copy(Path.GetTempPath() + @"DudeBot\bin\en_US.dic", dudebotdirectory + @"bin\en_US.dic");
            // TODO : Add new files as needed
        }

        public class MyWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri uri)
            {
                WebRequest w = base.GetWebRequest(uri);
                w.Timeout = 30000;
                return w;
            }
        }

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
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