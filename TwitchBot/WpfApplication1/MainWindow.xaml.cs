using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using DesktopWPFAppLowLevelKeyboardHook;
using System.Net;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Navigation;
using System.Runtime.InteropServices;
using System.Reflection;

namespace WpfApplication1
{

    public partial class MainWindow : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public static TwitchBot bot = new TwitchBot();
        String dudebotdirectory = Path.GetTempPath() + "dudebotdirectory.txt";
        String dudebotupdateinfo = Path.GetTempPath() + "dudebotupdateinfo.txt";
        
        private void OnPropertyChanged(string propertyName = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler == null) return;
            handler(this, new PropertyChangedEventArgs(propertyName));
        }

        // TODO : Backup JSON file!

        public Boolean isEmpty = false;
        
        Process imageWindow = null;
        [DllImport("User32")]
        private static extern int SetForegroundWindow(IntPtr hwnd);
        [DllImportAttribute("User32.DLL")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private LowLevelKeyboardListener _listener;
        
        void _listener_OnKeyPressed(object sender, KeyPressedArgs e)
        {
            if (e.KeyPressed.ToString().Equals("F7"))
            {
                try
                {
                    bot.requestSystem.nextSongAuto(bot.channel, false);
                }
                catch (Exception)
                {
                }
            }
        }

        public void addcommand(Object sender, RoutedEventArgs e)
        {

        }

        public void addcurrency(Object sender, RoutedEventArgs e)
        {

        }

        public void addevent(Object sender, RoutedEventArgs e)
        {

        }

        public void addImage(Object sender, RoutedEventArgs e)
        {

        }

        public void addquote(Object sender, RoutedEventArgs e)
        {

        }

        public void addrank(Object sender, RoutedEventArgs e)
        {

        }

        public void addSFX(Object sender, RoutedEventArgs e)
        {

        }

        public void addtimer(Object sender, RoutedEventArgs e)
        {

        }

        public void removecommand(Object sender, RoutedEventArgs e)
        {

        }

        public void removecurrency(Object sender, RoutedEventArgs e)
        {

        }

        public void removeevent(Object sender, RoutedEventArgs e)
        {

        }

        public void removeImage(Object sender, RoutedEventArgs e)
        {

        }

        public void removequote(Object sender, RoutedEventArgs e)
        {

        }

        public void removerank(Object sender, RoutedEventArgs e)
        {

        }

        public void removeSFX(Object sender, RoutedEventArgs e)
        {

        }

        public void removetimer(Object sender, RoutedEventArgs e)
        {

        }

        public void writeToConfig(Object sender, RoutedEventArgs e)
        {
            Utils.saveData(bot);
            bot = Utils.loadData();
        }
        
        public void killBot(Object sender, RoutedEventArgs e)
        {
            try
            {
                bot.client.SendMessage(bot.endMessage);
                bot.clearUpTempData();
                Utils.saveData(bot);
                bot.client.Disconnect();
            }
            catch (Exception e1)
            {
                Utils.errorReport(e1);
                Debug.WriteLine(e1.ToString());
            }
        }

        public void openBot(Object sender, RoutedEventArgs e)
        {
            try
            {
                bot.botDisconnect();
            }
            catch
            {
            }
            bot = Utils.loadData(); // Sets up and connects bot object
            if (bot == null)
            {
                MessageBox.Show("Missing or corrupt 'userData.json' file. If you are updating from DudeBot 2 to DudeBot 3," +
                    " please run 'DudeBotConfigUpdater.exe' in the bin folder, this will create an updated configuration file. To restore DudeBot and fix the configuration file, press 'reset dudebot'.", "Notice");
                return;
            }
            if (bot.streamer != null && bot.streamer != "")
            {
                dynamic activeX = browser.GetType().InvokeMember("ActiveXInstance",
                    BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                    null, browser, new object[] { });
                activeX.Silent = true;
                browser.LoadCompleted += (s, e1) =>
                {     
                    mshtml.IHTMLDocument2 doc = browser.Document as mshtml.IHTMLDocument2;
                    //doc.parentWindow.execScript("document.body.style.zoom=0.9");
                    //doc.parentWindow.execScript("document.body.style.transformrigin=​\"scale(0.9, 0)\";"); // TODO : FIX!
                };
                browser.Navigate("http://www.twitch.tv/" + bot.streamer + "/chat");
                
            }
            bot.botStartUpAsync();
            OnPropertyChanged();
            DataContext = bot;
        }
        

        public MainWindow()
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeComponent();
            Show();

            String fileName = System.IO.Path.GetTempPath() + "backupGUI.txt";
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
                    System.IO.StreamReader reader = new System.IO.StreamReader(fileName);
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith("BackgroundColor="))
                        {
                            colorChange.SelectedIndex = Int32.Parse(getFollowingText(line).Trim());
                        }
                        if (line.StartsWith("fontColor="))
                        {
                            textColorChange.SelectedIndex = Int32.Parse(getFollowingText(line).Trim());
                        }
                        if (line.StartsWith("textBlockColor="))
                        {
                            textblockcolor.SelectedIndex = Int32.Parse(getFollowingText(line).Trim());
                        }
                        if (line.StartsWith("buttonColor="))
                        {
                            buttonColor.SelectedIndex = Int32.Parse(getFollowingText(line).Trim());
                        }
                        if (line.StartsWith("font="))
                        {
                            fontStyle.SelectedIndex = Int32.Parse(getFollowingText(line).Trim());
                        }
                    }
                    reader.Close();
                }

                if (!File.Exists(dudebotdirectory))
                {
                    File.Create(dudebotdirectory).Close();
                }

                if (!File.Exists(dudebotupdateinfo))
                {
                    File.Create(dudebotupdateinfo).Close();
                }
                if (songduration.IsChecked != true)
                {
                    songdurationlimit.IsEnabled = false;
                }
                System.IO.StreamWriter writer = new System.IO.StreamWriter(dudebotdirectory);
                String location = System.Reflection.Assembly.GetExecutingAssembly().Location.ToString();
                writer.Write(location.Substring(0, location.IndexOf(@"DudeBot\DudeBot.exe")));
                writer.Close();
                Utils.copyFile(Path.Combine(@"bin", "dudebotupdater.exe"), Path.Combine(Path.GetDirectoryName(Path.GetTempPath() + @"\dudebot"), "dudebotupdater.exe"));

            }
            catch (Exception)
            {
            }

        }

        public static IEnumerable<T> FindLogicalChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                foreach (object rawChild in LogicalTreeHelper.GetChildren(depObj))
                {
                    if (rawChild is DependencyObject)
                    {
                        DependencyObject child = (DependencyObject)rawChild;
                        if (child is T)
                        {
                            yield return (T)child;
                        }

                        foreach (T childOfChild in FindLogicalChildren<T>(child))
                        {
                            yield return childOfChild;
                        }
                    }
                }
            }
        }

        public void colorChange_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            String color = cb.SelectedValue.ToString();
            int num = cb.SelectedIndex;
            color = color.Substring(color.IndexOf(":") + 1).Trim();
            SolidColorBrush brush = (SolidColorBrush)new BrushConverter().ConvertFromString(color);
            tabholder.Background = brush;
            this.Background = brush;
            foreach (TabItem t in FindLogicalChildren<TabItem>(this))
            {
                t.Background = brush;
            }
            foreach (TextBlock tb in FindLogicalChildren<TextBlock>(this))
            {
                if (tb.Uid == null)
                {
                    tb.Background = brush;
                }
            }
            writeGUI();
        }

        public void gamble_changed(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if ((bool)cb.IsChecked)
            {
                maxGamble.IsEnabled = true;
                gambleCoolDown.IsEnabled = true;
            }
            else
            {
                maxGamble.IsEnabled = false;
                gambleCoolDown.IsEnabled = false;
            }
        }

        public void minigame_changed(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if ((bool)cb.IsChecked)
            {
                minigametimer.IsEnabled = true;
            }
            else
            {
                minigametimer.IsEnabled = false;
            }
        }

        public void adventure_changed(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if ((bool)cb.IsChecked)
            {
                adventurecooldowntime.IsEnabled = true;
                adventurejointime.IsEnabled = true;
                adventureminreward.IsEnabled = true;
                adventuremaxreward.IsEnabled = true;
            }
            else
            {
                adventurecooldowntime.IsEnabled = false;
                adventurejointime.IsEnabled = false;
                adventureminreward.IsEnabled = false;
                adventuremaxreward.IsEnabled = false;
            }
        }

        public void currency_changed(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if ((bool)cb.IsChecked)
            {
                currencyName.IsEnabled = true;
                currencyCommand.IsEnabled = true;
                currencyPerMinute.IsEnabled = true;
                maxGamble.IsEnabled = true;
                gambleToggle.IsEnabled = true;
                gambleCoolDown.IsEnabled = true;
                vipsongcost.IsEnabled = true;
                vipsongtoggle.IsEnabled = true;
                vipRedeemCoolDownMinutes.IsEnabled = true;
            }
            else
            {
                currencyName.IsEnabled = false;
                currencyCommand.IsEnabled = false;
                currencyPerMinute.IsEnabled = false;
                gambleToggle.IsEnabled = false;
                maxGamble.IsEnabled = false;
                gambleCoolDown.IsEnabled = false;
                vipsongcost.IsEnabled = false;
                vipsongtoggle.IsEnabled = false;
                vipRedeemCoolDownMinutes.IsEnabled = false;
            }
        }

        public void vipsongcheck(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if ((bool)cb.IsChecked)
            {
                vipsongcost.IsEnabled = true;
                vipRedeemCoolDownMinutes.IsEnabled = true;
            }
            else
            {
                vipsongcost.IsEnabled = false;
                vipRedeemCoolDownMinutes.IsEnabled = false;
            }
        }
        
        public void songduration_changed(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if ((bool)cb.IsChecked)
            {
                songdurationlimit.IsEnabled = true;
            }
            else
            {
                songdurationlimit.IsEnabled = false;
            }
        }

        public void textColorChange_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            String color = cb.SelectedValue.ToString();
            color = color.Substring(color.IndexOf(":") + 1).Trim();
            SolidColorBrush brush = (SolidColorBrush)new BrushConverter().ConvertFromString(color);
            tabholder.Foreground = brush;
            this.Foreground = brush;
            foreach (DataGrid t in FindLogicalChildren<DataGrid>(this))
            {
                // t.Foreground = brush;
            }
            foreach (MahApps.Metro.Controls.NumericUpDown t in FindLogicalChildren<MahApps.Metro.Controls.NumericUpDown>(this))
            {
                t.Foreground = brush;
            }
            foreach (ComboBox t in FindLogicalChildren<ComboBox>(this))
            {
                t.Foreground = brush;
            }
            foreach (TabItem t in FindLogicalChildren<TabItem>(this))
            {
                t.Foreground = brush;
            }
            foreach (TextBox tb in FindLogicalChildren<TextBox>(this))
            {
                tb.Foreground = brush;
            }
            foreach (TextBlock tb in FindLogicalChildren<TextBlock>(this))
            {
                if (!tb.Name.Equals("tb_a") && !tb.Name.Equals("tb_b") && !tb.Name.Equals("tb_c") && !tb.Name.Equals("tb_d") && !tb.Name.Equals("tb_e") && !tb.Name.Equals("tb_f"))
                {
                    tb.Foreground = brush;
                }
            }
            foreach (ScrollViewer tb in FindLogicalChildren<ScrollViewer>(this))
            {
                Foreground = brush;
            }
            foreach (ListBox tb in FindLogicalChildren<ListBox>(this))
            {
                tb.Foreground = brush;
            }
            writeGUI();
        }

        public void textblockcolor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            String color = cb.SelectedValue.ToString();
            color = color.Substring(color.IndexOf(":") + 1).Trim();
            SolidColorBrush brush = (SolidColorBrush)new BrushConverter().ConvertFromString(color);
            foreach (DataGrid t in FindLogicalChildren<DataGrid>(this))
            {
                //t.Background = brush;
            }
            foreach (DataGrid t in FindLogicalChildren<DataGrid>(this))
            {
                t.Background = brush;
            }
            foreach (MahApps.Metro.Controls.NumericUpDown t in FindLogicalChildren<MahApps.Metro.Controls.NumericUpDown>(this))
            {
                t.Background = brush;
            }
            foreach (ComboBox t in FindLogicalChildren<ComboBox>(this))
            {
                t.Background = brush;
            }
            foreach (CheckBox t in FindLogicalChildren<CheckBox>(this))
            {
                t.Background = brush;
            }
            foreach (TextBox tb in FindLogicalChildren<TextBox>(this))
            {
                tb.Background = brush;
            }
            foreach (ScrollViewer tb in FindLogicalChildren<ScrollViewer>(this))
            {
                tb.Background = brush;
            }
            foreach (ListBox tb in FindLogicalChildren<ListBox>(this))
            {
                tb.Background = brush;
            }
            writeGUI();
        }

        private void fontStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            String txt = cb.SelectedValue.ToString();
            txt = txt.Substring(txt.IndexOf(":") + 1).Trim();
            foreach (TextBox tb in FindLogicalChildren<TextBox>(this))
            {
                tb.FontFamily = new FontFamily(txt);
            }
            foreach (ScrollViewer tb in FindLogicalChildren<ScrollViewer>(this))
            {
                tb.FontFamily = new FontFamily(txt);
            }
            foreach (ListBox tb in FindLogicalChildren<ListBox>(this))
            {
                tb.FontFamily = new FontFamily(txt);
            }
            writeGUI();
        }

        private void buttonColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            String color = cb.SelectedValue.ToString();
            color = color.Substring(color.IndexOf(":") + 1).Trim();
            SolidColorBrush brush = (SolidColorBrush)new BrushConverter().ConvertFromString(color);
            foreach (Button tb in FindLogicalChildren<Button>(this))
            {
                if (!tb.Name.Equals("kill") && !tb.Name.Equals("open") && !tb.Name.Equals("flyoutbutton") && !tb.Name.Equals("b_a") && !tb.Name.Equals("b_b") && !tb.Name.Equals("b_c") && !tb.Name.Equals("b_d") && !tb.Name.Equals("b_e") && !tb.Name.Equals("b_f") && !tb.Name.Equals("websitebutton") && !tb.Name.Equals("discordbutton"))
                {
                    tb.Background = brush;
                }
            }
            writeGUI();
        }

        public void writeGUI()
        {
            String fileName = System.IO.Path.GetTempPath() + "backupGUI.txt";
            if (!File.Exists(fileName))
            {
                FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate);
            }
            try
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter(fileName);
                writer.Write("BackgroundColor=" + colorChange.SelectedIndex + "\r");
                writer.Write("fontColor=" + textColorChange.SelectedIndex + "\r");
                writer.Write("textBlockColor=" + textblockcolor.SelectedIndex + "\r");
                writer.Write("buttonColor=" + buttonColor.SelectedIndex + "\r");
                writer.Write("font=" + fontStyle.SelectedIndex + "\r");
                writer.Close();
            }
            catch (Exception)
            {
            }
        }
        
        public void getReady()
        {
            refresh.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        private async void loaded(Object sender, RoutedEventArgs e)
        {
            if (!checkPrereqs())
            {
                preset();
                await this.ShowMessageAsync("Welcome to DudeBot!", "This is your first time using DudeBot. Please Enter your Streamer Name, Bot Name, Bot Oauth, then scroll to the bottom and press 'APPLY'.");
                customization.Focus();
            }
            else
            {
                try
                {
                    getReady();
                    _listener = new LowLevelKeyboardListener();
                    _listener.OnKeyPressed += _listener_OnKeyPressed;
                    _listener.HookKeyboard();

                    if (!checkinfo())
                    {
                        if (checkUpdate() == 1)
                        {
                            this.MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Accented;
                            var msgbox_settings = new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" };
                            MessageDialogResult messageBoxResult = await this.ShowMessageAsync("Update Available!", "An update was found, would you like to update?", MessageDialogStyle.AffirmativeAndNegative, msgbox_settings);
                            if (messageBoxResult == MessageDialogResult.Affirmative)
                            {
                                Thread.Sleep(1000);
                                try
                                {
                                    System.Diagnostics.Process.Start(System.IO.Path.GetTempPath() + @"\dudebotupdater.exe");
                                }
                                catch (Exception)
                                {
                                }
                                Application.Current.Shutdown();
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
                if (openImageWindowOnStart.IsChecked == true)
                {
                    openImages.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
            }
        }

        public void openImageWindow(Object sender, RoutedEventArgs e)
        {
            if (imageWindow != null)
            {
                try
                {
                    imageWindow.Kill();
                    imageWindow = null;
                }
                catch (Exception)
                {
                }
            }
            try
            {
                imageWindow = new Process();
                imageWindow.StartInfo.FileName = "bin\\images.exe";
                imageWindow.Start();
            }
            catch (Exception)
            {
            }
        }

        public Boolean checkinfo()
        {
            if (new FileInfo(dudebotupdateinfo).Length == 0)
            {
                return false;
            }
            else
            {
                using (StreamReader sr = new StreamReader(dudebotupdateinfo))
                {
                    String line = sr.ReadToEnd();
                    if (!line.Equals("fail"))
                    {
                        this.ShowMessageAsync("DudeBot Updated: " + line, "UPDATED");
                    }
                }
            }
            System.IO.File.WriteAllText(dudebotupdateinfo, string.Empty);
            return true;
        }

        private class MyWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri uri)
            {
                WebRequest w = base.GetWebRequest(uri);
                w.Timeout = 3000;
                return w;
            }
        }

        private int checkUpdate()
        {
            String version = Utils.version;
            using (MyWebClient client = new MyWebClient())
            {
                try
                {
                    string s = client.DownloadString("http://dudebot.webs.com/");
                    if (s.Contains("Newest Release: (" + version))
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }
                catch (Exception)
                {
                    return -1;
                }
            }
        }

        private async void updatebot(Object sender, RoutedEventArgs e)
        {
            if (checkUpdate() == 1)
            {
                this.MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Accented;
                var msgbox_settings = new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" };
                MessageDialogResult messageBoxResult = await this.ShowMessageAsync("Update Available!", "An update was found, would you like to update?", MessageDialogStyle.AffirmativeAndNegative, msgbox_settings);
                if (messageBoxResult == MessageDialogResult.Affirmative)
                {
                    try
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.FileName = System.IO.Path.GetTempPath() + @"\dudebotupdater.exe";
                        startInfo.CreateNoWindow = false;
                        startInfo.WindowStyle = ProcessWindowStyle.Normal;
                        startInfo.UseShellExecute = false;
                        Process.Start(startInfo);
                    }
                    catch (Exception)
                    {
                    }
                    Application.Current.Shutdown();
                }
            }
            else if (checkUpdate() == 0)
            {
                await this.ShowMessageAsync("", "DudeBot is up to date!");
            }
            else
            {
                await this.ShowMessageAsync("", "Could not reach server, please check again later.");
            }
        }

        private Boolean checkPrereqs()
        {
            openBot(null, null);
            if (bot == null || bot.oauth == null || bot.oauth == "" || bot.streamer == "" || bot.streamer == null)
            {
                return false;
            }
            return true;
        }

        private void preset() // TODO : May need to update this??
        {
            refresh.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            numberSongsSongListDisplays.Value = 8;
            maxNumberSongsPerUser.Value = 1;
            maxSongListLength.Value = 100;
            followToRequestCheckbox.IsChecked = true;
            displayIfUserIsHereCheckbox.IsChecked = true;
            songlistOneLineCheckbox.IsChecked = true;
            requestOnCheckbox.IsChecked = true;
            whisperCheckbox.IsChecked = true;
            quotesOn.IsChecked = true;
            direquests.IsChecked = true;
            ylrequests.IsChecked = true;
            requestLevel.SelectedIndex = 0;
            requestCommandNames.Text = "!request,!song,!play";
            playFavLevel.SelectedIndex = 0;
            playFavCommandNames.Text = "!requestfav,!songfav,!playfav";
            triggerRequestsLevel.SelectedIndex = 2;
            triggerRequestsCommandNames.Text = "!requests";
            songListLevel.SelectedIndex = 0;
            songListCommandNames.Text = "!songlist,!list,!playlist";
            getTotalSongsLevel.SelectedIndex = 0;
            getTotalSongsInListCommandNames.Text = "!queue,!length,!total";
            getViewerCountLevel.SelectedIndex = 0;
            getViewerCountCommandNames.Text = "!viewers";
            addTopLevel.SelectedIndex = 2;
            addTopCommandNames.Text = "!addtop";
            addVipLevel.SelectedIndex = 2;
            addVipCommandNames.Text = "!addvip";
            editLevel.SelectedIndex = 2;
            editCommandNames.Text = "!edit,!change";
            nextLevel.SelectedIndex = 2;
            nextCommandNames.Text = "!next,!skip";
            clearLevel.SelectedIndex = 2;
            clearCommandNames.Text = "!clear";
            displayCurrentLevel.SelectedIndex = 0;
            displayCurrentCommandName.Text = "!current,!playing,!currentsong";
            displayNextLevel.SelectedIndex = 0;
            displayNextSongCommandName.Text = "!nextsong";
            randomNextSongLevel.SelectedIndex = 2;
            randomNextSongCommandName.Text = "!randomnext,!nextrandom,!randomsong!songrandom";
            editReqSongLevel.SelectedIndex = 0;
            editReqSongCommandName.Text = "!editsong";
            removeReqSongLevel.SelectedIndex = 0;
            removeReqSongCommand.Text = "!removesong,!wrongsong";
            addDonationLevel.SelectedIndex = 2;
            addDonationCommand.Text = "!adddonator";
            bannedKeywords.Text = "";
            songPositionLevel.SelectedIndex = 0;
            songPositionName.Text = "!mysong,!position,!songposition";
            refreshtimer.Value = 20;
            sfxCoolDownTextBox.Value = 60;
            minigametimer.Value = 60;
            startupMessage.Text = "is here, let's party.";
            minigameEndMessage.Text = "Time has run out for the current game, guesses cannot be added now, $user!";
            subOnlyRequests.Text = "Only subs can request right now, $user!";
            songduration.IsChecked = false;
            songdurationlimit.Value = 10;
            adventurejointime.Value = 120;
            adventurecooldowntime.Value = 20;
            adventureminreward.Value = 25;
            adventuremaxreward.Value = 75;
            giveawaycommandname.Text = "!raffle";
            currencyToggle.IsChecked = false;
            currencyName.Text = "points";
            currencyCommand.Text = "!points";
            currencyPerMinute.Value = 2;
            maxGamble.Value = 10000;
            gambleCoolDown.Value = 5;
            vipsongcost.Value = 500;
            vipsongtoggle.IsEnabled = true;
            gambleToggle.IsEnabled = true;
            adventureToggle.IsEnabled = true;
            endMessage.Text = "BYE FAM";
            vipRedeemCoolDownMinutes.Value = 60;
            autoShoutoutOnHost.IsChecked = true;
            quotesModOnly.IsChecked = true;
            imageCoolDown.Value = 60;
            imageDisplayTime.Value = 3;
            openImageWindowOnStart.IsChecked = false;
            sfxOverallCoolDown.Value = 0;
            ImagesOverallCoolDown.Value = 0;
            followerMessage.Text = "$user just followed the stream! Thank you!";
            subMessage.Text = "$user just subscribed to the stream! Thank you!";
            rankunitdropbox.SelectedIndex = 0;
            subCreditRedeemCost.Value = 1;
            creditsPerSub.Value = 1;
        }

        public void readSongsIntoUI()
        {
            // TODO
        }
        
        public String getLevel(String str)
        {
            return str[(str.IndexOf("=") + 1)].ToString();
        }

        public string getCommandNames(string str)
        {
            return str.Substring(str.LastIndexOf("=") + 1).Trim();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private String getFollowingText(String line)
        {
            try
            {
                String line2 = line.Substring(line.IndexOf('=') + 1);
                return line2;
            }
            catch (Exception)
            {
                return "";
            }
        }
        
        public void refresh_currency(Object sender, RoutedEventArgs e)
        {
            // TODO : read from currency???
        }

        public async void clearlist(Object sender, RoutedEventArgs e) // TODO : Json bind
        {
           
        }

        public void Button_Click(Object sender, RoutedEventArgs e)
        {
           
        }
        
        public void sortByUserName(Object sender, RoutedEventArgs e)
        {
           
        }

        public void sortByRankTitle(Object sender, RoutedEventArgs e)
        {

        }

        public void sortByAmount(Object sender, RoutedEventArgs e)
        {
            
        }

        public void sortByMinutes(Object sender, RoutedEventArgs e)
        {
            
        }

        public void sortBySubCredits(Object sender, RoutedEventArgs e)
        {
            
        }

        private void moveup(Object sender, RoutedEventArgs e)
        {
           
        }

        public void moveupSong(String song, String requester)
        {
          
        }

        private async void deleteSong(Object sender, RoutedEventArgs e)
        {
           
        }

        private void DeleteFromList(String song, String requester)
        {
          
        }

        private void TabControl_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                _listener.UnHookKeyboard();
            }
            catch (Exception)
            {
            }
            try
            {
                imageWindow.Kill();
            }
            catch (Exception)
            {
            }
            if (bot != null)
            { 
                killBot(null, null);
            }
            Process.GetCurrentProcess().CloseMainWindow();
        }
        
        private void tb_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
           
        }

        private void tb_MouseLeftButtonDown2(object sender, MouseButtonEventArgs e)
        {
           
        }

        private void tb_MouseLeftButtonDown9(object sender, MouseButtonEventArgs e)
        {
           
        }

        private void tb_MouseLeftButtonDown10(object sender, MouseButtonEventArgs e)
        {
           
        }

        private void tb_MouseLeftButtonDown4(object sender, MouseButtonEventArgs e)
        {
          
        }

        private void tb_MouseLeftButtonDown8(object sender, MouseButtonEventArgs e)
        {
          
        }

        private void tb_MouseLeftButtonDown7(object sender, MouseButtonEventArgs e)
        {
         
        }

        private void tb_MouseLeftButtonDown3(object sender, MouseButtonEventArgs e)
        {
           
        }

        private void tb_MouseLeftButtonDown5(object sender, MouseButtonEventArgs e)
        {
            editClickedSongBySong(sender, e);
            if (e.ClickCount == 2)
            {
                var tb = sender as TextBlock;
                String input = tb.Text;
                input = input.Replace(" ", "+");
                input = input.Substring(input.IndexOf("\t") + 1);
                input = input.Substring(3);
                if (input.Equals("songs+in+the+list!"))
                {
                    return;
                }
                String link = "https://www.youtube.com/results?search_query=" + input;
                var backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += (s, e1) =>
                {
                    Thread.Sleep(250);
                };
                System.Diagnostics.Process.Start(link);
            }
        }

        private void tb_MouseLeftButtonDown6(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var tb = sender as TextBlock;
                String input = tb.Text;
                if (input.Equals("") || input == null)
                {
                    return;
                }
                String link = "https://www.twitch.tv/" + input;
                var backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += (s, e1) =>
                {
                    Thread.Sleep(250);
                };
                System.Diagnostics.Process.Start(link);
            }
        }

        public void editClickedSongBySong(object sender, MouseButtonEventArgs e)
        {
          
        }

        
        public void uploadSFX(object sender, System.EventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".mp3";
            dlg.Filter = "MP3 Files (*.mp3)|*.mp3";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                editResponseSFX.Text = filename;
            }
        }
        
        public void uploadImage(object sender, System.EventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                editResponseImage.Text = filename;
            }
        }
        
        public ArrayList LoadArrayList()
        {
            // TODO : ???? OR DELETE?
            return new ArrayList();
        }

        public void addsong(object sender, RoutedEventArgs e)
        {
            // TODO
        }

        public void editsong(object sender, RoutedEventArgs e)
        {
            // TODO
        }
        
        public String formatCommand(String str)
        {
            if (!str.StartsWith("!"))
            {
                str = "!" + str;
            }
            if (str.Equals("!"))
            {
                str = "";
            }
            return str;
        }

        public async void resetbot(Object sender, RoutedEventArgs e)
        {
            this.MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Accented;
            var msgbox_settings = new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" };
            MessageDialogResult messageBoxResult = await this.ShowMessageAsync("Reset Confirmation", "Are you sure you want to reset DudeBot? All user data will be deleted, but this will fix any problems with DudeBot.", MessageDialogStyle.AffirmativeAndNegative, msgbox_settings);
            if (messageBoxResult == MessageDialogResult.Affirmative)
            {
                preset();
                streamerName.Text = "";
                botName.Text = "";
                oauth.Text = "";
                googleSheetID.Text = "";


                // TODO : Refactor JSON to new starter file!



                // Restart Bot
                await this.ShowMessageAsync("No Previous Data Found!", "This is your first time using DudeBot. Please Enter your Streamer Name, Bot Name, and Bot Oauth, then press 'APPLY' and restart DudeBot!");
                customization.Focus();
            }
        }

        private void flyout(object sender, RoutedEventArgs e)
        {
            supportflyout.IsOpen = true;
        }

        private void nightlikethis(object sender, RoutedEventArgs e)
        {
            String link = "https://www.facebook.com/nightlikethisofficial/";
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += (s, e1) =>
            {
                Thread.Sleep(250);
            };
            System.Diagnostics.Process.Start(link);
        }

        private void nightlikethistwitter(object sender, RoutedEventArgs e)
        {
            String link = "https://twitter.com/NLTBandOfficial/";
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += (s, e1) =>
            {
                Thread.Sleep(250);
            };
            System.Diagnostics.Process.Start(link);
        }

        private void nightlikethisitunes(object sender, RoutedEventArgs e)
        {
            String link = "https://itunes.apple.com/us/album/state-of-mind-ep/id1245858545";
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += (s, e1) =>
            {
                Thread.Sleep(250);
            };
            System.Diagnostics.Process.Start(link);
        }

        private void nightlikethisgooglemusic(object sender, RoutedEventArgs e)
        {
            String link = "https://play.google.com/store/music/album?id=Bhdahoyg7ltamdyiqrt5r3wo5fa";
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += (s, e1) =>
            {
                Thread.Sleep(250);
            };
            System.Diagnostics.Process.Start(link);
        }

        private void nightlikethisspotify(object sender, RoutedEventArgs e)
        {
            String link = "https://open.spotify.com/album/1mleQbNkIbQ8kOG4yGmgBN?";
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += (s, e1) =>
            {
                Thread.Sleep(250);
            };
            System.Diagnostics.Process.Start(link);
        }

        private void donateviapaypal(object sender, RoutedEventArgs e)
        {
            String link = "https://twitch.streamlabs.com/edude15000#/";
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += (s, e1) =>
            {
                Thread.Sleep(250);
            };
            System.Diagnostics.Process.Start(link);
        }

        private void websitebutton_Click(object sender, RoutedEventArgs e)
        {
            String link = "http://dudebot.webs.com/";
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += (s, e1) =>
            {
                Thread.Sleep(250);
            };
            System.Diagnostics.Process.Start(link);
        }

        private void discordbutton_Click(object sender, RoutedEventArgs e)
        {
            String link = "https://discord.gg/NFehx5h";
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += (s, e1) =>
            {
                Thread.Sleep(250);
            };
            System.Diagnostics.Process.Start(link);
        }

       
    }
}