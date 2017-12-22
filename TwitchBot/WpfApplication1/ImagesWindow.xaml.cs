using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Images
{
    public partial class MainWindow : Window
    {
        int displaySeconds = 3;
        public MainWindow()
        {
            InitializeComponent();
        }
        
        public void startUp(int displaySeconds)
        {
            this.displaySeconds = displaySeconds;
            Show();
        }

        public async void displayImage(String filePath)
        {
            Dispatcher.Invoke(new Action(async () => {
                Uri imageUri = new Uri(filePath);
                BitmapImage imageBitmap = new BitmapImage(imageUri);
                image.Source = imageBitmap;
                await Task.Delay(displaySeconds * 1000);
                image.Source = null;

            }), DispatcherPriority.ContextIdle);
        }
    }
}
