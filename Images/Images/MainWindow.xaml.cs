using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Images
{
    public partial class MainWindow : Window
    { 
        int displaySeconds = 3;
        String dudebotimage = System.IO.Path.GetTempPath() + "dudebotimage.txt";

        public MainWindow()
        {
            InitializeComponent();
        }
        public String checkFile()
        {
            String line = "";
            try
            {
                using (StreamReader sr = new StreamReader(dudebotimage))
                {
                    line = sr.ReadToEnd();
                }
            }
            catch (Exception)
            {
            }
            return line;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            String line;
            if (!File.Exists(dudebotimage))
            {
                File.Create(dudebotimage).Close();
            }
            try
            {
                System.IO.StreamReader reader = new System.IO.StreamReader("bin/config.txt");
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("imageDisplayTimeSeconds="))
                    {
                        displaySeconds = Int32.Parse(line.Substring(line.IndexOf('=') + 1));
                    }
                }
                reader.Close();
            }
            catch (Exception f)
            {
                MessageBox.Show(f.ToString());
                try
                {
                    Thread.Sleep(1000);
                    System.IO.StreamReader reader = new System.IO.StreamReader("bin/config.txt");
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith("imageDisplayTimeSeconds="))
                        {
                            MessageBox.Show(line);
                            displaySeconds = Int32.Parse(line.Substring(line.IndexOf('=') + 1));
                        }
                    }
                    reader.Close();
                }
                catch (Exception)
                {
                }
            }
            
            DispatcherTimer timer;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(3000);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        async void timer_Tick(object sender, EventArgs e)
        {
            String play = checkFile();
            if (!play.Equals(""))
            {
                Uri imageUri = new Uri(play);
                BitmapImage imageBitmap = new BitmapImage(imageUri);
                image.Source = imageBitmap;
                System.IO.File.WriteAllText(dudebotimage, string.Empty);
                await Task.Delay(displaySeconds * 1000);
                image.Source = null;
            }
        }
    }
}
