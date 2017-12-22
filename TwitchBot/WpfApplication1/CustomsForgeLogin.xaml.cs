using System.Windows;

namespace CustomsForgeLogin
{
    public partial class MainWindow : Window
    {
        TwitchBot bot;

        public MainWindow(TwitchBot bot)
        {
            InitializeComponent();
            this.bot = bot;
            Show();
        }
        
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (bot != null)
            {
                if (string.IsNullOrWhiteSpace(cfUserName.Text) || string.IsNullOrWhiteSpace(cfPassword.Password))
                {
                    MessageBox.Show("Please enter a valid username and password!");
                    return;
                }
                bot.requestSystem.cfUserName = cfUserName.Text;
                bot.requestSystem.cfPassword = cfPassword.Password;
                if (await bot.requestSystem.getCookieFromCF())
                {
                    MessageBox.Show("Successfully connected with CustomsForge!");
                    Close();
                }
                else
                {
                    MessageBox.Show("The username or password you entered is invalid!");
                    bot.requestSystem.cfUserName = "";
                    bot.requestSystem.cfPassword = "";
                    return;
                }
            }
        }
    }
}
