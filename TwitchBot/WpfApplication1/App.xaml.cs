using System;
using System.IO;
using System.Windows;

namespace WpfApplication1
{
    public partial class App : Application
    {
        public static MainWindow guiWindow = new MainWindow();
        public static Images.MainWindow imagesWindow = new Images.MainWindow();
        public static CustomsForgeLogin.MainWindow customsForgeWindow;
        
        static void Main(String[] args)
        {
            guiWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            guiWindow.InitializeComponent();
            guiWindow.Show();
            if (!File.Exists(guiWindow.dudebotdirectory))
            {
                File.Create(guiWindow.dudebotdirectory).Close();
            }
            if (!File.Exists(guiWindow.dudebotupdateinfo))
            {
                File.Create(guiWindow.dudebotupdateinfo).Close();
            }
            if (guiWindow.songduration.IsChecked != true)
            {
                guiWindow.songdurationlimit.IsEnabled = false;
            }
            StreamWriter writer = new StreamWriter(guiWindow.dudebotdirectory);
            String location = System.Reflection.Assembly.GetExecutingAssembly().Location.ToString();
            writer.Write(location.Substring(0, location.IndexOf(@"DudeBot\DudeBot.exe")));
            writer.Close();
            Utils.copyFile(Path.Combine(@"bin", "dudebotupdater.exe"), Path.Combine(Path.GetDirectoryName(Path.GetTempPath() + @"\dudebot"), "dudebotupdater.exe"));
        }
    }
}
