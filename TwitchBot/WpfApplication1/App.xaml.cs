using System;
using System.IO;
using System.Windows;

namespace WpfApplication1
{
    public partial class App : Application
    {
        public static MainWindow window = new MainWindow();
        public static Images.MainWindow imagesWindow = new Images.MainWindow();

        static void Main(String[] args)
        {
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.InitializeComponent();
            window.Show();
            String fileName = Path.GetTempPath() + "backupGUI.txt";
            try
            {
                if (!File.Exists(fileName))
                {
                    File.Create(fileName).Close();
                }
                if (!File.Exists(Utils.songListFile))
                {
                    File.Create(Utils.songListFile).Close();
                }
                else
                {
                    String line;
                    StreamReader reader = new StreamReader(fileName);
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith("BackgroundColor="))
                        {
                            window.colorChange.SelectedIndex = Int32.Parse(window.getFollowingText(line).Trim());
                        }
                        if (line.StartsWith("fontColor="))
                        {
                            window.textColorChange.SelectedIndex = Int32.Parse(window.getFollowingText(line).Trim());
                        }
                        if (line.StartsWith("textBlockColor="))
                        {
                            window.textblockcolor.SelectedIndex = Int32.Parse(window.getFollowingText(line).Trim());
                        }
                        if (line.StartsWith("buttonColor="))
                        {
                            window.buttonColor.SelectedIndex = Int32.Parse(window.getFollowingText(line).Trim());
                        }
                        if (line.StartsWith("font="))
                        {
                            window.fontStyle.SelectedIndex = Int32.Parse(window.getFollowingText(line).Trim());
                        }
                    }
                    reader.Close();
                }
                if (!File.Exists(window.dudebotdirectory))
                {
                    File.Create(window.dudebotdirectory).Close();
                }
                if (!File.Exists(window.dudebotupdateinfo))
                {
                    File.Create(window.dudebotupdateinfo).Close();
                }
                if (window.songduration.IsChecked != true)
                {
                    window.songdurationlimit.IsEnabled = false;
                }
                StreamWriter writer = new StreamWriter(window.dudebotdirectory);
                String location = System.Reflection.Assembly.GetExecutingAssembly().Location.ToString();
                writer.Write(location.Substring(0, location.IndexOf(@"DudeBot\DudeBot.exe")));
                writer.Close();
                Utils.copyFile(Path.Combine(@"bin", "dudebotupdater.exe"), Path.Combine(Path.GetDirectoryName(Path.GetTempPath() + @"\dudebot"), "dudebotupdater.exe"));

            }
            catch (Exception)
            {
            }
        }
    }
}
