using System;
using System.Windows.Threading;
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
using System.Linq;
using System.Runtime.InteropServices;

namespace WpfApplication1
{
    public partial class MainWindow
    {
        public static TwitchBot bot = new TwitchBot();
        String dudebotdirectory = Path.GetTempPath() + "dudebotdirectory.txt";
        String dudebotupdateinfo = Path.GetTempPath() + "dudebotupdateinfo.txt";

        // TODO : Backup JSON file!

        public int[] numberArray;
        public String[] typeArray;
        public String[] songArray;
        public String[] requesterArray;
        public String[] responses;
        public String[] commands;
        public String[] commandLevels;
        public String[] Timedresponses;
        public String[] Timedcommands;
        public String[] Timedtoggle;
        public String[] SFXcommands;
        public String[] SFXresponses;
        public String[] imagecommands;
        public String[] imageresponses;
        public String[] eventusers;
        public String[] eventmessages;
        public BindingList<Commands> commandArray = new BindingList<Commands>();
        public BindingList<Timers> timerArray = new BindingList<Timers>();
        public BindingList<SFX> SFXArray = new BindingList<SFX>();
        public BindingList<Quotes> QuotesArray = new BindingList<Quotes>();
        public BindingList<Events> EventsArray = new BindingList<Events>();
        public BindingList<Images> ImagesArray = new BindingList<Images>();
        public BindingList<Currency> CurrencyArray = new BindingList<Currency>();
        public BindingList<Ranks> RankArray = new BindingList<Ranks>();
        public ArrayList currencyuser = new ArrayList();
        public ArrayList currencyamount = new ArrayList();
        public ArrayList currencytime = new ArrayList();
        public ArrayList currencysubcredits = new ArrayList();
        public ArrayList ranktitle = new ArrayList();
        public ArrayList rankcost = new ArrayList();
        public ArrayList quotes = new ArrayList();
        public Boolean isEmpty = false;
        public int quoteID;
        Boolean writeAndReset = false;

        Process proc = null;
        Process chat = null;
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

        private void getUserList() // TODO : read from method instead of file!
        {
            if (proc == null)
            {
                userlist.Text = "NOT CONNECTED";
                return;
            }
            try
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(System.IO.Path.GetTempPath() + "currentusers.txt");
                String users = "", line = "";
                int count = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    users += line + "\r";
                    count++;
                }
                userlistheader.Text = "User List (" + count + ")";
                userlist.Text = users;
            }
            catch (Exception)
            {

            }
        }

        public async void openChat(Object sender, RoutedEventArgs e)
        {

            if (!streamerName.Text.Equals(""))
            {
                String link = "https://www.twitch.tv/" + streamerName.Text + "/chat";
                var backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += (s, e1) =>
                {
                    Thread.Sleep(250);
                };
                chat = new Process();
                chat.StartInfo.FileName = link;
                chat.Start();
            }
            else
            {
                await this.ShowMessageAsync("Not Connected", "Please enter a streamer name in the configuration tab before opening Twitch chat!");
            }
        }

        public void killBot(Object sender, RoutedEventArgs e)
        {
            // TODO send bye message
            try
            {
                bot.clearUpTempData();
                Utils.saveData(bot);
                bot.botDisconnect();
            }
            catch (Exception e1)
            {
                Utils.errorReport(e1);
                Debug.WriteLine(e1.ToString());
            }
        }

        public void openBot(Object sender, RoutedEventArgs e)
        {
            bot = Utils.loadData(); // Sets up and connects bot object
            MessageBox.Show(Directory.GetCurrentDirectory());
            if (bot == null)
            {
                MessageBox.Show("Missing or corrupt 'userData.json' file. If you are updating from DudeBot 2 to DudeBot 3," +
                    " please run 'DudeBotConfigUpdater.exe' in the bin folder, this will create an updated configuration file. To restore DudeBot and fix the configuration file, press 'reset dudebot'.", "Notice");
                return;
            }
            bot.botStartUp();
        }

        public MainWindow()
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeComponent();
            this.Show();

            String fileName = System.IO.Path.GetTempPath() + "backupGUI.txt";
            try
            {
                if (!File.Exists(fileName))
                {
                    File.Create(fileName).Close();
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

                if (!File.Exists(System.IO.Path.GetTempPath() + "dudebotkeyboardcontroller.txt"))
                {
                    File.Create(System.IO.Path.GetTempPath() + "dudebotkeyboardcontroller.txt").Close();
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
                revloReward.IsEnabled = true;
                amountResult.IsEnabled = true;
                minigametimer.IsEnabled = true;
            }
            else
            {
                revloReward.IsEnabled = false;
                amountResult.IsEnabled = false;
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

        public void amountResult_changed(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if ((bool)cb.IsChecked)
            {
                revloReward.IsEnabled = false;
            }
            else
            {
                revloReward.IsEnabled = true;
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

        void timer_Tick(object sender, EventArgs e)
        {
            refresh.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            if (checkPrereqs())
            {
                try
                {
                    getUserList(); // TODO : needed???
                }
                catch (Exception)
                {
                }
            }
        }

        void proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                console.Text = console.Text + "\n" + e.Data;
                console.ScrollToEnd();
            }));
        }

        public void getReady()
        {
            readConfig();
            readTextFile();
            refresh.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        private async void loaded(Object sender, RoutedEventArgs e)
        {
            if (!checkPrereqs())
            {
                preset();
                await this.ShowMessageAsync("Welcome to DudeBot!", "This is your first time using DudeBot. Please Enter your Streamer Name, Bot Name, Bot Oauth, then scroll to the bottom and press 'APPLY'.");
                writeAndReset = true;
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
                DispatcherTimer timer;
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(5000);
                timer.Tick += new EventHandler(timer_Tick);
                timer.Start();

                openBot(null, null);

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
            // TODO : check streamer and oauth in json
            return false;
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
            revloReward.Value = 75;
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

        private void readConfig() // TODO : Json bind
        {
            string line;
            try
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(configFile);
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains("channel="))
                    {
                        String temp = getFollowingText(line);
                        streamerName.Text = temp.Substring(1);
                    }
                    else if (line.Contains("oauth="))
                    {
                        oauth.Text = getFollowingText(line);
                    }
                    else if (line.Contains("googleSheet="))
                    {
                        googleSheetID.Text = getFollowingText(line);
                    }
                    else if (line.Contains("botName="))
                    {
                        botName.Text = getFollowingText(line);
                    }
                    else if (line.Contains("regulars="))
                    {
                        regulars.Text = getFollowingText(line);
                    }
                    else if (line.Contains("favSongs="))
                    {
                        favSongList.Text = getFollowingText(line);
                    }
                    else if (line.Contains("numOfSongsToDisplay="))
                    {
                        numberSongsSongListDisplays.Value = Double.Parse(getFollowingText(line));
                    }
                    else if (line.Contains("numOfSongsInQueuePerUser="))
                    {
                        maxNumberSongsPerUser.Value = Double.Parse(getFollowingText(line));
                    }
                    else if (line.Contains("maxSonglistLength="))
                    {
                        maxSongListLength.Value = Double.Parse(getFollowingText(line));
                    }
                    else if (line.Contains("mustFollowToRequest="))
                    {
                        if (line.Contains("true"))
                        {
                            followToRequestCheckbox.IsChecked = true;
                        }
                        else if (line.Contains("false"))
                        {
                            followToRequestCheckbox.IsChecked = false;
                        }
                    }
                    else if (line.Contains("displayIfUserIsHere="))
                    {
                        if (line.Contains("true"))
                        {
                            displayIfUserIsHereCheckbox.IsChecked = true;
                        }
                        else if (line.Contains("false"))
                        {
                            displayIfUserIsHereCheckbox.IsChecked = false;
                        }

                    }
                    else if (line.Contains("displaySonglistOneLine="))
                    {
                        if (line.Contains("true"))
                        {
                            songlistOneLineCheckbox.IsChecked = true;
                        }
                        else if (line.Contains("false"))
                        {
                            songlistOneLineCheckbox.IsChecked = false;
                        }

                    }
                    else if (line.Contains("requestsOn="))
                    {
                        if (line.Contains("true"))
                        {
                            requestOnCheckbox.IsChecked = true;
                        }
                        else if (line.Contains("false"))
                        {
                            requestOnCheckbox.IsChecked = false;
                        }
                    }
                    else if (line.Contains("requestCommands="))
                    {
                        requestLevel.SelectedIndex = Int32.Parse(getLevel(line));
                        requestCommandNames.Text = getCommandNames(line);
                    }
                    else if (line.Contains("favSongCommand="))
                    {
                        playFavLevel.SelectedIndex = Int32.Parse(getLevel(line));
                        playFavCommandNames.Text = getCommandNames(line);
                    }
                    else if (line.Contains("requestsTrigger="))
                    {
                        triggerRequestsLevel.SelectedIndex = Int32.Parse(getLevel(line));
                        triggerRequestsCommandNames.Text = getCommandNames(line);
                    }
                    else if (line.Contains("songlistCommands="))
                    {
                        songListLevel.SelectedIndex = Int32.Parse(getLevel(line));
                        songListCommandNames.Text = getCommandNames(line);
                    }
                    else if (line.Contains("getTotalSongsCommands="))
                    {
                        getTotalSongsLevel.SelectedIndex = Int32.Parse(getLevel(line));
                        getTotalSongsInListCommandNames.Text = getCommandNames(line);
                    }
                    else if (line.Contains("getViewerCountCommands="))
                    {
                        getViewerCountLevel.SelectedIndex = Int32.Parse(getLevel(line));
                        getViewerCountCommandNames.Text = getCommandNames(line);
                    }
                    else if (line.Contains("addtopCommands="))
                    {
                        addTopLevel.SelectedIndex = Int32.Parse(getLevel(line));
                        addTopCommandNames.Text = getCommandNames(line);
                    }
                    else if (line.Contains("addvipCommands="))
                    {
                        addVipLevel.SelectedIndex = Int32.Parse(getLevel(line));
                        addVipCommandNames.Text = getCommandNames(line);
                    }
                    else if (line.Contains("editCommands="))
                    {
                        editLevel.SelectedIndex = Int32.Parse(getLevel(line));
                        editCommandNames.Text = getCommandNames(line);
                    }
                    else if (line.Contains("nextCommands="))
                    {
                        nextLevel.SelectedIndex = Int32.Parse(getLevel(line));
                        nextCommandNames.Text = getCommandNames(line);
                    }
                    else if (line.Contains("clearCommands="))
                    {
                        clearLevel.SelectedIndex = Int32.Parse(getLevel(line));
                        clearCommandNames.Text = getCommandNames(line);
                    }
                    else if (line.Contains("getCurrentCommands="))
                    {
                        displayCurrentLevel.SelectedIndex = Int32.Parse(getLevel(line));
                        displayCurrentCommandName.Text = getCommandNames(line);
                    }
                    else if (line.Contains("getNextSongCommands="))
                    {
                        displayNextLevel.SelectedIndex = Int32.Parse(getLevel(line));
                        displayNextSongCommandName.Text = getCommandNames(line);
                    }
                    else if (line.Contains("randomNextSong="))
                    {
                        randomNextSongLevel.SelectedIndex = Int32.Parse(getLevel(line));
                        randomNextSongCommandName.Text = getCommandNames(line);
                    }
                    else if (line.Contains("editMySong="))
                    {
                        editReqSongLevel.SelectedIndex = Int32.Parse(getLevel(line));
                        editReqSongCommandName.Text = getCommandNames(line);
                    }
                    else if (line.Contains("removeMySong="))
                    {
                        removeReqSongLevel.SelectedIndex = Int32.Parse(getLevel(line));
                        removeReqSongCommand.Text = getCommandNames(line);
                    }
                    else if (line.Contains("adddonatorCommands="))
                    {
                        addDonationLevel.SelectedIndex = Int32.Parse(getLevel(line));
                        addDonationCommand.Text = getCommandNames(line);
                    }
                    else if (line.Contains("bannedKeywords="))
                    {
                        bannedKeywords.Text = getFollowingText(line);
                    }
                    else if (line.Contains("mySongPosition="))
                    {
                        songPositionLevel.SelectedIndex = Int32.Parse(getLevel(line));
                        songPositionName.Text = getCommandNames(line);
                    }
                    else if (line.Contains("timer="))
                    {
                        refreshtimer.Value = Double.Parse(getFollowingText(line));
                    }
                    else if (line.Contains("whispersOn="))
                    {
                        if (line.Contains("true"))
                        {
                            whisperCheckbox.IsChecked = true;
                        }
                        else if (line.Contains("false"))
                        {
                            whisperCheckbox.IsChecked = false;
                        }
                    }
                    else if (line.Contains("quotesOn="))
                    {
                        if (line.Contains("true"))
                        {
                            quotesOn.IsChecked = true;
                        }
                        else if (line.Contains("false"))
                        {
                            quotesOn.IsChecked = false;
                        }
                    }
                    else if (line.Contains("sfxTimer="))
                    {
                        sfxCoolDownTextBox.Value = Double.Parse(getFollowingText(line));
                    }
                    else if (line.Contains("revloReward="))
                    {
                        revloReward.Value = Double.Parse(getFollowingText(line));
                    }
                    else if (line.Contains("amountResult="))
                    {
                        if (line.Contains("true"))
                        {
                            amountResult.IsChecked = true;
                        }
                        else if (line.Contains("false"))
                        {
                            amountResult.IsChecked = false;
                        }
                    }
                    else if (line.Contains("minigameTimer="))
                    {
                        minigametimer.Value = Double.Parse(getFollowingText(line));
                    }
                    else if (line.Contains("startupMessage="))
                    {
                        startupMessage.Text = getFollowingText(line);
                    }
                    else if (line.Contains("minigameEndMessage="))
                    {
                        minigameEndMessage.Text = getFollowingText(line);
                    }
                    else if (line.Contains("subOnlyRequests="))
                    {
                        subOnlyRequests.Text = getFollowingText(line);
                    }
                    else if (line.Contains("directInputRequests="))
                    {
                        if (line.Contains("true"))
                        {
                            direquests.IsChecked = true;
                        }
                        else if (line.Contains("false"))
                        {
                            direquests.IsChecked = false;
                        }
                    }
                    else if (line.Contains("youtubeLinkRequests="))
                    {
                        if (line.Contains("true"))
                        {
                            ylrequests.IsChecked = true;
                        }
                        else if (line.Contains("false"))
                        {
                            ylrequests.IsChecked = false;
                        }
                    }
                    else if (line.Contains("maxSongLimitOn="))
                    {
                        if (line.Contains("true"))
                        {
                            songduration.IsChecked = true;
                        }
                        else if (line.Contains("false"))
                        {
                            songduration.IsChecked = false;
                        }
                    }
                    else if (line.Contains("maxSongDuration="))
                    {
                        songdurationlimit.Value = Double.Parse(getFollowingText(line));
                    }
                    else if (line.Contains("adventureUserJoinTime="))
                    {
                        adventurejointime.Value = Double.Parse(getFollowingText(line));
                    }
                    else if (line.Contains("adventureCoolDownTime="))
                    {
                        adventurecooldowntime.Value = Double.Parse(getFollowingText(line));
                    }
                    else if (line.Contains("adventureMinReward="))
                    {
                        adventureminreward.Value = Double.Parse(getFollowingText(line));
                    }
                    else if (line.Contains("adventureMaxReward="))
                    {
                        adventuremaxreward.Value = Double.Parse(getFollowingText(line));
                    }
                    else if (line.Contains("giveawaycommandname="))
                    {
                        giveawaycommandname.Text = getFollowingText(line);
                    }
                    else if (line.Contains("currencyName="))
                    {
                        currencyName.Text = getFollowingText(line);
                    }
                    else if (line.Contains("currencyPerMinute="))
                    {
                        currencyPerMinute.Value = Double.Parse(getFollowingText(line));
                    }
                    else if (line.Contains("maxGamble="))
                    {
                        maxGamble.Value = Double.Parse(getFollowingText(line));
                    }
                    else if (line.Contains("currencyCoolDownMinutes="))
                    {
                        gambleCoolDown.Value = Double.Parse(getFollowingText(line));
                    }
                    else if (line.Contains("currencyToggle="))
                    {
                        if (line.Contains("true"))
                        {
                            currencyToggle.IsChecked = true;
                        }
                        else if (line.Contains("false"))
                        {
                            currencyToggle.IsChecked = false;
                        }
                    }
                    else if (line.Contains("currencyCommandName="))
                    {
                        currencyCommand.Text = getFollowingText(line);
                    }
                    else if (line.Contains("vipSongCost="))
                    {
                        vipsongcost.Value = Double.Parse(getFollowingText(line));
                    }
                    else if (line.Contains("vipSongToggle="))
                    {
                        if (line.Contains("true"))
                        {
                            vipsongtoggle.IsChecked = true;
                        }
                        else if (line.Contains("false"))
                        {
                            vipsongtoggle.IsChecked = false;
                        }
                    }
                    else if (line.Contains("gambleToggle="))
                    {
                        if (line.Contains("true"))
                        {
                            gambleToggle.IsChecked = true;
                        }
                        else if (line.Contains("false"))
                        {
                            gambleToggle.IsChecked = false;
                        }
                    }
                    else if (line.Contains("adventureToggle="))
                    {
                        if (line.Contains("true"))
                        {
                            adventureToggle.IsChecked = true;
                        }
                        else if (line.Contains("false"))
                        {
                            adventureToggle.IsChecked = false;
                        }
                    }
                    else if (line.Contains("minigameOn="))
                    {
                        if (line.Contains("true"))
                        {
                            minigameToggle.IsChecked = true;
                        }
                        else if (line.Contains("false"))
                        {
                            minigameToggle.IsChecked = false;
                        }
                    }
                    else if (line.Contains("endMessage="))
                    {
                        endMessage.Text = getFollowingText(line);
                    }
                    else if (line.Contains("vipRedeemCoolDownMinutes="))
                    {
                        vipRedeemCoolDownMinutes.Value = Double.Parse(getFollowingText(line));
                    }
                    else if (line.Contains("autoShoutoutOnHost="))
                    {
                        if (line.Contains("true"))
                        {
                            autoShoutoutOnHost.IsChecked = true;
                        }
                        else if (line.Contains("false"))
                        {
                            autoShoutoutOnHost.IsChecked = false;
                        }
                    }
                    else if (line.Contains("quotesModOnly="))
                    {
                        if (line.Contains("true"))
                        {
                            quotesModOnly.IsChecked = true;
                        }
                        else if (line.Contains("false"))
                        {
                            quotesModOnly.IsChecked = false;
                        }
                    }
                    else if (line.Contains("imageDisplayTimeSeconds="))
                    {
                        imageDisplayTime.Value = Double.Parse(getFollowingText(line));
                    }
                    else if (line.Contains("imageCoolDown="))
                    {
                        imageCoolDown.Value = Double.Parse(getFollowingText(line));
                    }
                    else if (line.Contains("openImageWindowOnStart="))
                    {
                        if (line.Contains("true"))
                        {
                            openImageWindowOnStart.IsChecked = true;
                        }
                        else if (line.Contains("false"))
                        {
                            openImageWindowOnStart.IsChecked = false;
                        }
                    }
                    else if (line.Contains("sfxOverallCoolDown="))
                    {
                        sfxOverallCoolDown.Value = Double.Parse(getFollowingText(line));
                    }
                    else if (line.Contains("imagesOverallCoolDown="))
                    {
                        ImagesOverallCoolDown.Value = Double.Parse(getFollowingText(line));
                    }
                    else if (line.Contains("followersTextFile="))
                    {
                        followersTextFile.Text = (getFollowingText(line));
                    }
                    else if (line.Contains("subTextFile="))
                    {
                        subTextFile.Text = (getFollowingText(line));
                    }
                    else if (line.Contains("followerMessage="))
                    {
                        followerMessage.Text = (getFollowingText(line));
                    }
                    else if (line.Contains("subMessage="))
                    {
                        subMessage.Text = (getFollowingText(line));
                    }
                    else if (line.Contains("rankupUnitCost="))
                    {
                        rankunitdropbox.SelectedIndex = Int32.Parse(getFollowingText(line));
                    }
                    else if (line.Contains("subCreditRedeemCost="))
                    {
                        subCreditRedeemCost.Value = Int32.Parse(getFollowingText(line));
                    }
                    else if (line.Contains("creditsPerSub="))
                    {
                        creditsPerSub.Value = Int32.Parse(getFollowingText(line));
                    }

                }
            }
            catch
            {
            }
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

        private void readTextFile() // TODO : convert to json file!
        {
            try
            {
                string line;
                int counter = 0;
                System.IO.StreamReader reader = new System.IO.StreamReader(Utils.songlistfile);
                while ((line = reader.ReadLine()) != null)
                {
                    counter++;
                }
                reader.Close();
                if (counter != 0)
                {
                    songArray = new String[counter];
                    numberArray = new int[counter];
                    typeArray = new String[counter];
                    requesterArray = new String[counter];
                    String line2;
                    int counter2 = 0;
                    try
                    {
                        System.IO.StreamReader file2 = new System.IO.StreamReader(Utils.songlistfile);
                        while ((line2 = file2.ReadLine()) != null)
                        {
                            String[] parts = line2.Split('\t');
                            string requester = parts[parts.Length - 1];
                            if (line2.StartsWith("VIP\t"))
                            {
                                String song = line2.Substring(line2.IndexOf('\t') + 1, line2.LastIndexOf('\t') - 3);
                                typeArray[counter2] = "VIP";
                                songArray[counter2] = song;
                                numberArray[counter2] = counter2 + 1;
                                requesterArray[counter2] = requester;
                            }
                            else if (line2.StartsWith("$$$\t"))
                            {
                                String song = line2.Substring(line2.IndexOf('\t') + 1, line2.LastIndexOf('\t') - 3);
                                typeArray[counter2] = "$$$";
                                songArray[counter2] = song;
                                numberArray[counter2] = counter2 + 1;
                                requesterArray[counter2] = requester;
                            }
                            else
                            {
                                String song = line2.Substring(0, line2.IndexOf('\t'));
                                typeArray[counter2] = "REG";
                                songArray[counter2] = song;
                                numberArray[counter2] = counter2 + 1;
                                requesterArray[counter2] = requester;
                            }
                            counter2++;
                        }
                        isEmpty = false;
                        file2.Close();
                    }
                    catch (Exception)
                    {
                    }
                }
                else
                {
                    isEmpty = true;
                }
            }
            catch
            {
            }
        }

        public void refresh_currency(Object sender, RoutedEventArgs e)
        {
            // TODO : read from currency???
        }

        public async void clearlist(Object sender, RoutedEventArgs e) // TODO : Json bind
        {
            this.MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Accented;
            var msgbox_settings = new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" };
            MessageDialogResult messageBoxResult = await this.ShowMessageAsync("", "Are you sure you want to clear the song list?", MessageDialogStyle.AffirmativeAndNegative, msgbox_settings);
            if (messageBoxResult == MessageDialogResult.Affirmative)
            {
                try
                {
                    System.IO.StreamWriter writer = new System.IO.StreamWriter(Utils.songlistfile); // Refactor to json
                    writer.Write("");
                    writer.Close();
                    System.IO.StreamWriter writer2 = new System.IO.StreamWriter(Utils.currentSongFile); // keep
                    writer2.Write("Song list is empty");
                    writer2.Close();
                    System.IO.StreamWriter writer3 = new System.IO.StreamWriter(Utils.currentRequesterFile); // keep
                    writer3.Write("");
                    writer3.Close();
                    refresh.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    songArray = new String[1000];
                    typeArray = new String[1000];
                    requesterArray = new String[1000];
                    return;
                }
                catch
                {

                }
            }
        }

        public void Button_Click(Object sender, RoutedEventArgs e)
        {
            try
            {
                readTextFile();
                if (!isEmpty)
                {
                    SongInfo[] songinfoArray = new SongInfo[songArray.Length];
                    for (int i = 0; i < songArray.Length; i++)
                    {
                        TextBlock tb = new TextBlock();
                        tb.Text = " #" + numberArray[i] + "\t" + typeArray[i] + "\t" + songArray[i];
                        tb.Uid = i.ToString();
                        tb.MouseLeftButtonDown += new MouseButtonEventHandler(tb_MouseLeftButtonDown5);

                        TextBlock tb2 = new TextBlock();
                        String user = requesterArray[i].Replace("(", "");
                        user = user.Replace(")", "");
                        tb2.Uid = i.ToString();
                        tb2.Text = user;
                        tb2.MouseLeftButtonDown += new MouseButtonEventHandler(tb_MouseLeftButtonDown6);

                        Button tb3 = new Button();
                        tb3.MouseLeftButtonDown += new MouseButtonEventHandler(deleteSong);
                        tb3.Uid = i.ToString();

                        Button tb4 = new Button();
                        if (i != 0)
                        {
                            tb4.MouseLeftButtonDown += new MouseButtonEventHandler(moveup);
                        }
                        tb4.Uid = i.ToString();
                        songinfoArray[i] = new SongInfo(tb, tb2, tb4, tb3);

                    }
                    songdatagrid.ItemsSource = songinfoArray;
                }
                else
                {
                    TextBlock tb = new TextBlock();
                    tb.Text = "No songs in the list!";
                    SongInfo[] songinfoArray = new SongInfo[1];
                    songinfoArray[0] = new SongInfo(tb, null, null, null);
                    songdatagrid.ItemsSource = songinfoArray;
                }
            }
            catch (Exception)
            {
            }

        }

        public class SongInfo // TODO : Json bind
        {
            public TextBlock songInfo { get; set; }
            public TextBlock songReq { get; set; }
            public Button moveUp { get; set; }
            public Button delete { get; set; }

            public SongInfo(TextBlock songInfo, TextBlock songReq, Button moveUp, Button delete)
            {
                this.songInfo = songInfo;
                this.songReq = songReq;
                this.moveUp = moveUp;
                this.delete = delete;
            }
        }

        public class Currency // TODO : Json bind
        {
            public TextBlock currencyUser { get; set; }
            public TextBlock currencyAmount { get; set; }
            public TextBlock currencyTime { get; set; }
            public TextBlock currencySubCredits { get; set; }

            public Currency(TextBlock currencyUser, TextBlock currencyAmount, TextBlock currencyTime, TextBlock currencySubCredits)
            {
                this.currencyUser = currencyUser;
                this.currencyAmount = currencyAmount;
                this.currencyTime = currencyTime;
                this.currencySubCredits = currencySubCredits;
            }
        }

        public class Ranks // TODO : Json bind
        {
            public TextBlock rankTitle { get; set; }
            public TextBlock rankCost { get; set; }

            public Ranks(TextBlock rankTitle, TextBlock rankCost)
            {
                this.rankTitle = rankTitle;
                this.rankCost = rankCost;
            }
        }

        public class Commands // TODO : Json bind
        {
            public TextBlock commandName { get; set; }
            public TextBlock commandResponse { get; set; }
            public TextBlock commandLevel { get; set; }

            public Commands(TextBlock commandName, TextBlock commandResponse, TextBlock commandLevel)
            {
                this.commandName = commandName;
                this.commandResponse = commandResponse;
                this.commandLevel = commandLevel;
            }
        }

        public class Timers // TODO : Json bind
        {
            public TextBlock timerName { get; set; }
            public TextBlock timerResponse { get; set; }
            public TextBlock timerToggle { get; set; }

            public Timers(TextBlock timerName, TextBlock timerResponse, TextBlock timerToggle)
            {
                this.timerName = timerName;
                this.timerResponse = timerResponse;
                this.timerToggle = timerToggle;
            }
        }

        public class Quotes // TODO : Json bind
        {
            public TextBlock quoteInfo { get; set; }

            public Quotes(TextBlock quoteInfo)
            {
                this.quoteInfo = quoteInfo;
            }
        }

        public class SFX // TODO : Json bind
        {
            public TextBlock sfxCommand { get; set; }
            public TextBlock sfxResponse { get; set; }

            public SFX(TextBlock sfxCommand, TextBlock sfxResponse)
            {
                this.sfxCommand = sfxCommand;
                this.sfxResponse = sfxResponse;
            }
        }

        public class Images // TODO : Json bind
        {
            public TextBlock imageCommand { get; set; }
            public TextBlock imageResponse { get; set; }

            public Images(TextBlock imageCommand, TextBlock imageResponse)
            {
                this.imageCommand = imageCommand;
                this.imageResponse = imageResponse;
            }
        }

        public class Events // TODO : Json bind
        {
            public TextBlock eventUser { get; set; }
            public TextBlock eventMessage { get; set; }

            public Events(TextBlock eventUser, TextBlock eventMessage)
            {
                this.eventUser = eventUser;
                this.eventMessage = eventMessage;
            }
        }

        public void sortByUserName(Object sender, RoutedEventArgs e)
        {
            try
            {
                var currencySorted = new BindingList<Currency>(CurrencyArray.OrderBy(x => x.currencyUser.Text).ToList());
                for (int i = 0; i < currencySorted.Count; i++)
                {
                    CurrencyArray[i] = currencySorted[i];
                }
                // TODO : add to currency ???
            }
            catch (Exception)
            {
            }
        }

        public void sortByAmount(Object sender, RoutedEventArgs e)
        {
            try
            {
                var currencySorted = new BindingList<Currency>(CurrencyArray.OrderBy(x => Int32.Parse(x.currencyAmount.Text)).ToList());
                for (int i = 0; i < currencySorted.Count; i++)
                {
                    CurrencyArray[i] = currencySorted[i];
                }
                // TODO : add to currency ???
            }
            catch (Exception)
            {
            }
        }

        public void sortByMinutes(Object sender, RoutedEventArgs e)
        {
            try
            {
                var currencySorted = new BindingList<Currency>(CurrencyArray.OrderBy(x => Int32.Parse(x.currencyTime.Text)).ToList());
                for (int i = 0; i < currencySorted.Count; i++)
                {
                    CurrencyArray[i] = currencySorted[i];
                }
                // TODO : add to currency ???
            }
            catch (Exception)
            {
            }
        }

        public void sortBySubCredits(Object sender, RoutedEventArgs e)
        {
            try
            {
                var currencySorted = new BindingList<Currency>(CurrencyArray.OrderBy(x => Int32.Parse(x.currencySubCredits.Text)).ToList());
                for (int i = 0; i < currencySorted.Count; i++)
                {
                    CurrencyArray[i] = currencySorted[i];
                }
                // TODO : add to currency ???
            }
            catch (Exception)
            {
            }
        }

        private void moveup(Object sender, RoutedEventArgs e)
        {
            Button a = sender as Button;
            try
            {
                if (a.Uid != "0")
                {
                    String song = songArray[Int32.Parse(a.Uid)];
                    String requester = requesterArray[Int32.Parse(a.Uid)];
                    if (song.Contains("\t"))
                    {
                        song = song.Replace("\t", "");
                    }
                    if (requester.Contains("\t"))
                    {
                        requester = requester.Replace("\t", "");
                    }
                    moveupSong(song, requester);
                    readTextFile();
                }
            }
            catch (Exception)
            {
            }
        }

        public async void moveupSong(String song, String requester)
        {
            try
            {
                System.IO.StreamReader br = new System.IO.StreamReader(Utils.songlistfile); // TODO : Json bind
                String check = br.ReadLine();
                br.Close();
                if (check.Contains(song + "\t" + requester))
                {
                    await this.ShowMessageAsync("NOTICE", "Can't move first song up any higher than first position! (The first request user must have removed their song before last refresh!)");
                    refresh.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    return;
                }

                String line;
                int counter = 0;
                String currentsong = "";
                String uppersong;
                int currentlevel = 0;
                int upperlevel = 0;
                string writeToUpper = "";
                string writeToCurrent = "";
                System.IO.StreamReader reader = new System.IO.StreamReader(Utils.songlistfile); // TODO : Json bind
                while ((line = reader.ReadLine()) != null)
                {
                    counter++;
                    if (line.Contains(song + "\t" + requester))
                    {
                        currentsong = line;
                        if (currentsong.StartsWith("VIP\t"))
                        {
                            currentlevel = 1;
                        }
                        else if (currentsong.StartsWith("$$$\t"))
                        {
                            currentlevel = 2;
                        }
                        else
                        {
                            currentlevel = 0;
                        }

                        break;
                    }
                }
                reader.Close();
                System.IO.StreamReader reader2 = new System.IO.StreamReader(Utils.songlistfile); // TODO : Json bind
                for (int i = 1; i < counter; i++)
                {
                    line = reader2.ReadLine();
                }
                uppersong = line;
                if (uppersong.StartsWith("VIP\t"))
                {
                    upperlevel = 1;
                }
                else if (uppersong.StartsWith("$$$\t"))
                {
                    upperlevel = 2;
                }
                else
                {
                    upperlevel = 0;
                }
                reader2.Close();
                String upperSongEdit1 = "";
                String currentSongEdit1 = "";
                if (upperlevel != 0)
                {
                    writeToCurrent = uppersong.Substring(0, 4);
                    upperSongEdit1 = uppersong.Substring(4);
                }
                if (currentlevel != 0)
                {
                    writeToUpper = currentsong.Substring(0, 4);
                    currentSongEdit1 = currentsong.Substring(4);
                }
                System.IO.StreamReader reader3 = new System.IO.StreamReader(Utils.songlistfile); // TODO : Json bind
                System.IO.StreamWriter writer = new System.IO.StreamWriter(Utils.templistfile); // TODO : Json bind
                while ((line = reader3.ReadLine()) != null)
                {
                    if (line.Equals(uppersong))
                    {
                        break;
                    }
                    else
                    {
                        writer.Write(line + "\r");
                    }
                }
                reader3.ReadLine();
                if (currentSongEdit1.Equals(""))
                {
                    if (currentlevel == 0)
                    {
                        if (upperlevel == 1)
                        {
                            writer.Write("VIP\t" + currentsong + "\r");
                        }
                        else if (upperlevel == 2)
                        {
                            writer.Write("$$$\t" + currentsong + "\r");
                        }
                        else
                        {
                            writer.Write(currentsong + "\r");
                        }
                    }
                    else
                    {
                        writer.Write(currentsong + "\r");
                    }
                }
                else
                {
                    writer.Write(writeToCurrent + currentSongEdit1 + "\r");
                }
                if (upperSongEdit1.Equals(""))
                {
                    writer.Write(uppersong + "\r");
                }
                else
                {
                    writer.Write(writeToUpper + upperSongEdit1 + "\r");
                }
                while ((line = reader3.ReadLine()) != null)
                {
                    writer.Write(line + "\r");
                }
                reader3.Close();
                writer.Close();
                System.IO.StreamWriter writer2 = new System.IO.StreamWriter(Utils.songlistfile); // TODO : Json bind
                writer2.Write("");
                writer2.Close();
                Utils.copyFile(Utils.templistfile, Utils.songlistfile); // TODO : Json bind
                System.IO.StreamWriter writer3 = new System.IO.StreamWriter(Utils.templistfile); // TODO : Json bind
                writer3.Write("");
                writer3.Close();
                refresh.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
            catch
            {
            }
        }

        private async void deleteSong(Object sender, RoutedEventArgs e)
        {
            try
            {
                Button a = sender as Button;
                String song = songArray[Int32.Parse(a.Uid)];
                if (song.Contains("\t"))
                {
                    song = song.Replace("\t", "");
                }
                this.MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Accented;
                var msgbox_settings = new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" };
                MessageDialogResult messageBoxResult = await this.ShowMessageAsync("Delete Confirmation", "Are you sure you want to delete " + song + "?", MessageDialogStyle.AffirmativeAndNegative, msgbox_settings);
                if (messageBoxResult == MessageDialogResult.Affirmative)
                {
                    DeleteFromList(song, requesterArray[Int32.Parse(a.Uid)]);
                }
                readTextFile();
            }
            catch (Exception)
            {
            }
        }

        private void DeleteFromList(String song, String requester)
        {
            try
            {
                if (song.Contains("\t"))
                {
                    song = song.Replace("\t", "");
                }
                song = song.Trim();
                String line;
                System.IO.StreamReader reader = new System.IO.StreamReader(Utils.songlistfile);
                System.IO.StreamWriter writer = new System.IO.StreamWriter(Utils.templistfile);
                while ((line = reader.ReadLine()) != null)
                {
                    if (!(line.Contains(song + "\t" + requester)))
                    {
                        writer.Write(line + "\r");
                    }
                    else
                    {
                        break;
                    }
                }
                while ((line = reader.ReadLine()) != null)
                {
                    writer.Write(line + "\r");
                }
                reader.Close();
                writer.Close();

                System.IO.StreamWriter writer2 = new System.IO.StreamWriter(Utils.songlistfile);
                writer2.Write("");
                writer2.Close();
                Utils.copyFile(Utils.templistfile, Utils.songlistfile);
                System.IO.StreamWriter writer3 = new System.IO.StreamWriter(Utils.templistfile);
                writer3.Write("");
                writer3.Close();
                refresh.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
            catch (Exception)
            {

            }
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
            killBot(null, null);
        }
        
        private void tb_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var line = sender as TextBlock;
            editCommand.Text = commandArray[Int32.Parse(line.Uid)].commandName.Text;
            editResponse.Text = commandArray[Int32.Parse(line.Uid)].commandResponse.Text;
            String level = commandArray[Int32.Parse(line.Uid)].commandLevel.Text;
            if (level.Equals("Everyone"))
            {
                editCommandLevel.SelectedIndex = 0;
            }
            else if (level.Equals("Subs, Mods, Streamer"))
            {
                editCommandLevel.SelectedIndex = 1;
            }
            else if (level.Equals("Mods, Streamer"))
            {
                editCommandLevel.SelectedIndex = 2;
            }
            else if (level.Equals("Streamer"))
            {
                editCommandLevel.SelectedIndex = 3;
            }
        }

        private void tb_MouseLeftButtonDown2(object sender, MouseButtonEventArgs e)
        {
            var line = sender as TextBlock;
            editCommandTimed.Text = timerArray[Int32.Parse(line.Uid)].timerName.Text;
            editResponseTimed.Text = timerArray[Int32.Parse(line.Uid)].timerResponse.Text;
            String checkeda = timerArray[Int32.Parse(line.Uid)].timerToggle.Text;
            if (checkeda == "ON")
            {
                editToggleTimed.IsChecked = true;
            }
        }

        private void tb_MouseLeftButtonDown9(object sender, MouseButtonEventArgs e)
        {
            var line = sender as TextBlock;
            editCurrencyUserName.Text = CurrencyArray[Int32.Parse(line.Uid)].currencyUser.Text;
            editCurrencyAmount.Text = CurrencyArray[Int32.Parse(line.Uid)].currencyAmount.Text;
            editCurrencyTime.Text = CurrencyArray[Int32.Parse(line.Uid)].currencyTime.Text;
            editCurrencySubCredits.Text = CurrencyArray[Int32.Parse(line.Uid)].currencySubCredits.Text;
        }

        private void tb_MouseLeftButtonDown10(object sender, MouseButtonEventArgs e)
        {
            var line = sender as TextBlock;
            editRankTitle.Text = RankArray[Int32.Parse(line.Uid)].rankTitle.Text;
            editRankCost.Text = RankArray[Int32.Parse(line.Uid)].rankCost.Text;
        }

        private void tb_MouseLeftButtonDown4(object sender, MouseButtonEventArgs e)
        {
            var line = sender as TextBlock;
            editCommandSFX.Text = SFXArray[Int32.Parse(line.Uid)].sfxCommand.Text;
            editResponseSFX.Text = SFXArray[Int32.Parse(line.Uid)].sfxResponse.Text;
        }

        private void tb_MouseLeftButtonDown8(object sender, MouseButtonEventArgs e)
        {
            var line = sender as TextBlock;
            editCommandImage.Text = ImagesArray[Int32.Parse(line.Uid)].imageCommand.Text;
            editResponseImage.Text = ImagesArray[Int32.Parse(line.Uid)].imageResponse.Text;
        }

        private void tb_MouseLeftButtonDown7(object sender, MouseButtonEventArgs e)
        {
            var line = sender as TextBlock;
            editEventUser.Text = EventsArray[Int32.Parse(line.Uid)].eventUser.Text;
            editEventMessage.Text = EventsArray[Int32.Parse(line.Uid)].eventMessage.Text;
        }

        private void tb_MouseLeftButtonDown3(object sender, MouseButtonEventArgs e)
        {
            var line = sender as TextBlock;
            int id = Int32.Parse(line.Uid);
            quoteEdit.Text = QuotesArray[id].quoteInfo.Text;
            quoteID = id;
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
            var tb = sender as TextBlock;
            if (tb.Text.Equals("No songs in the list!"))
            {
                return;
            }
            for (int i = 0; i < songArray.Length; i++)
            {
                if (tb.Text.EndsWith("\t" + songArray[i]))
                {
                    if (songArray[i].EndsWith("\t"))
                    {
                        editSong.Text = songArray[i].Replace("\t", "");
                    }
                    else
                    {
                        editSong.Text = songArray[i];
                    }
                    editRequester.Text = requesterArray[i].Replace(")", "").Replace("(", "");
                    songplace.Value = numberArray[i];
                    break;
                }
            }
        }

        public void uploadRevlobotCSV(object sender, System.EventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".csv";
            dlg.Filter = "CSV Files (*.csv)|*.csv";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                revlobotCSVPath.Text = filename;
            }
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

        public void uploadFollowers(object sender, System.EventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text Files (*.txt)|*.txt";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                followersTextFile.Text = filename;
            }
        }

        public void uploadSub(object sender, System.EventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text Files (*.txt)|*.txt";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                subTextFile.Text = filename;
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
            System.IO.StreamReader reader = new System.IO.StreamReader(Utils.songlistfile);
            ArrayList list = new ArrayList();
            string a;
            while ((a = reader.ReadLine()) != null)
            {
                list.Add(a);
            }
            reader.Close();
            return list;
        }

        public async void addsong(object sender, RoutedEventArgs e)
        {
            String uName = "";
            if (string.IsNullOrWhiteSpace(editRequester.Text))
            {
                uName = streamerName.Text;
            }
            else
            {
                uName = editRequester.Text;
            }
            if (!string.IsNullOrWhiteSpace(editSong.Text))
            {
                ArrayList text = LoadArrayList();
                try
                {
                    if (text.Count == 0)
                    {
                        text.Insert(0, editSong.Text + "\t(" + uName + ")");
                    }
                    else
                    {
                        String temp = (String)text[(Int32)songplace.Value - 1];
                        if (temp != null)
                        {
                            if (temp.StartsWith("$$$\t"))
                            {
                                text.Insert((Int32)songplace.Value - 1, "$$$\t" + editSong.Text + "\t(" + uName + ")");
                            }
                            else if (temp.StartsWith("VIP\t"))
                            {
                                text.Insert((Int32)songplace.Value - 1, "VIP\t" + editSong.Text + "\t(" + uName + ")");
                            }
                            else
                            {
                                text.Insert((Int32)songplace.Value - 1, editSong.Text + "\t(" + uName + ")");
                            }
                        }
                        else
                        {
                            return;
                        }
                    }

                }
                catch (ArgumentOutOfRangeException)
                {
                    text.Add(editSong.Text + "\t(" + uName + ")");
                }
                catch (Exception)
                {
                }
                File.WriteAllLines(Utils.songlistfile, text.Cast<String>());
                refresh.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                editSong.Text = "";
                editRequester.Text = "";
                songplace.Value = 1;
            }
            else
            {
                await this.ShowMessageAsync("Warning", "Please enter the song name!");
            }
        }

        public async void editsong(object sender, RoutedEventArgs e)
        {
            String uName = "";
            if (string.IsNullOrWhiteSpace(editRequester.Text))
            {
                uName = streamerName.Text;
            }
            else
            {
                uName = editRequester.Text;
            }
            if (!string.IsNullOrWhiteSpace(editSong.Text))
            {
                try
                {
                    string[] text = File.ReadAllLines(Utils.songlistfile);
                    if (text[0] == "")
                    {
                        await this.ShowMessageAsync("Warning", "Place # " + songplace.Value + " does not exist!");
                        return;
                    }
                    String temp = text[(Int32)songplace.Value - 1];
                    if (temp != null)
                    {
                        if (temp.StartsWith("$$$\t"))
                        {
                            text[(Int32)songplace.Value - 1] = "$$$\t" + editSong.Text + "\t(" + uName + ")";
                        }
                        else if (temp.StartsWith("VIP\t"))
                        {
                            text[(Int32)songplace.Value - 1] = "VIP\t" + editSong.Text + "\t(" + uName + ")";
                        }
                        else
                        {
                            text[(Int32)songplace.Value - 1] = editSong.Text + "\t(" + uName + ")";
                        }
                    }
                    else
                    {
                        await this.ShowMessageAsync("Warning", "Place # " + songplace.Value + " does not exist!");
                        return;
                    }
                    File.WriteAllLines(Utils.songlistfile, text);
                    refresh.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    editSong.Text = "";
                    editRequester.Text = "";
                    songplace.Value = 1;
                }
                catch (Exception)
                {
                    await this.ShowMessageAsync("Warning", "Place # " + songplace.Value + " does not exist!");
                    return;
                }
            }
            else
            {
                await this.ShowMessageAsync("Warning", "Please enter the song name!");
            }
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
                // Set variable presets (config.txt)
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

        public async void runCSVImport(object sender, System.EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(formatCommand(revlobotCSVPath.Text)))
            {
                await this.ShowMessageAsync("Incorrect Path!", "Revlobot CSV Path is incorrect. Please reupload it and try again.");
                return;
            }
            String user = null, points = null;
            TextBlock tb3 = new TextBlock();
            tb3.Text = "0";
            TextBlock tb4 = new TextBlock();
            tb4.Text = "0";
            try
            {
                using (var fs = File.OpenRead(revlobotCSVPath.Text))
                using (var reader = new StreamReader(fs))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');
                        if (!values[0].Equals("Username"))
                        {
                            user = values[0];
                            points = values[2];
                            for (int i = 0; i < CurrencyArray.Count; i++)
                            {
                                var item = CurrencyArray[i].currencyUser as TextBlock;
                                if (item.Text.ToLower().StartsWith(user.ToLower()))
                                {
                                    var item2 = CurrencyArray[i].currencyTime as TextBlock;
                                    tb3.Text = item2.Text;
                                    var item3 = CurrencyArray[i].currencySubCredits as TextBlock;
                                    tb4.Text = item3.Text;
                                    if (importFromRevloCheckBox.IsChecked == true)
                                    {
                                        user = null;
                                    }
                                    else
                                    {
                                        CurrencyArray.Remove(CurrencyArray[i]);
                                    }
                                }
                            }
                            if (user == null)
                            {
                                break;
                            }
                            TextBlock tb = new TextBlock();
                            tb.Text = user;
                            TextBlock tb2 = new TextBlock();
                            tb2.Text = points;
                            CurrencyArray.Add(new Currency(tb, tb2, tb3, tb4));
                        }
                    }
                    reader.Close();
                }
                // TODO : add to currency! ???
                await this.ShowMessageAsync("Successfully Imported", "Revlobot points have been successfully imported.");
            }
            catch (Exception f)
            {
                MessageBox.Show(f.StackTrace);
                await this.ShowMessageAsync("Importing Failed", "Failed to import points, please try again later.");
            }
        }
    }
}