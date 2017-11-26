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
        static String channel;
        static String oauth;
        static String startupMessage;
        static String line;
        static String botColor = "cadetblue";
        static TwitchBot bot;
        static String endMessage;
        String configFile = @"bin\config.txt";
        String versionFile = @"bin\version.txt";
        String commandsFile = @"bin\commands.txt";
        String timersFile = @"bin\timedcommands.txt";
        String quotesFile = @"bin\quotes.txt";
        String currencyFile = @"bin\currency.txt";
        String othersFile = @"bin\others.txt";
        string currentsongFile = @"bin\currentsong.txt";
        string currentrequesterFile = @"bin\currentrequester.txt";
        String tempFile = @"bin\temp.txt";
        string songsFile = @"bin\song.txt";
        string sfxFile = @"bin\sfx.txt";
        string usersFile = @"bin\users.txt";
        string eventsFile = @"bin\events.txt";
        string imagesFile = @"bin\images.txt";
        string ranksFile = @"bin\ranks.txt";
        string purchasedranksFile = @"bin\purchasedranks.txt";

        String backupcommandsfile = System.IO.Path.GetTempPath() + "backupdudebotcommands.txt";
        String backuptimersfile = System.IO.Path.GetTempPath() + "backupdudebottimers.txt";
        String backupbotfile = System.IO.Path.GetTempPath() + "backupdudebot.txt";
        String backupquotesfile = System.IO.Path.GetTempPath() + "backupdudebotquotes.txt";
        String backupothersfile = System.IO.Path.GetTempPath() + "backupothers.txt";
        String backupsfxfile = System.IO.Path.GetTempPath() + "backupsfx.txt";
        String backupusersfile = System.IO.Path.GetTempPath() + "backupusers.txt";
        String backupguifile = System.IO.Path.GetTempPath() + "backupGUI.txt";
        String dudebotdirectory = System.IO.Path.GetTempPath() + "dudebotdirectory.txt";
        String dudebotupdateinfo = System.IO.Path.GetTempPath() + "dudebotupdateinfo.txt";
        String backupeventsfile = System.IO.Path.GetTempPath() + "backupevents.txt";
        String backupcurrencyfile = System.IO.Path.GetTempPath() + "backupcurrency.txt";
        String backupimagesfile = System.IO.Path.GetTempPath() + "backupimages.txt";
        string backupranksfile = System.IO.Path.GetTempPath() + "backupranks.txt";
        string backuppurchasedranksfile = System.IO.Path.GetTempPath() + "backuppurchasedranks.txt";

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
                    System.IO.StreamWriter writer = new System.IO.StreamWriter(System.IO.Path.GetTempPath() + "dudebotkeyboardcontroller.txt");
                    writer.Write("a");
                    writer.Close();
                }
                catch (Exception)
                {
                    System.Threading.Thread.Sleep(500);
                    try
                    {
                        System.IO.StreamWriter writer = new System.IO.StreamWriter(System.IO.Path.GetTempPath() + "dudebotkeyboardcontroller.txt");
                        writer.Write("a");
                        writer.Close();
                    }
                    catch (Exception)
                    {
                        System.Threading.Thread.Sleep(500);
                        try
                        {
                            System.IO.StreamWriter writer = new System.IO.StreamWriter(System.IO.Path.GetTempPath() + "dudebotkeyboardcontroller.txt");
                            writer.Write("a");
                            writer.Close();
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
        }

        private void getUserList()
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
            } catch (Exception)
            {

            }

        }

        void syncUpdatesWithBot()
        {
            try
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter(System.IO.Path.GetTempPath() + "dudebotsyncbot.txt");
                writer.Write("a");
                writer.Close();
            }
            catch (Exception)
            {
                System.Threading.Thread.Sleep(500);
                try
                {
                    System.IO.StreamWriter writer = new System.IO.StreamWriter(System.IO.Path.GetTempPath() + "dudebotsyncbot.txt");
                    writer.Write("a");
                    writer.Close();
                }
                catch (Exception)
                {
                    System.Threading.Thread.Sleep(500);
                    try
                    {
                        System.IO.StreamWriter writer = new System.IO.StreamWriter(System.IO.Path.GetTempPath() + "dudebotsyncbot.txt");
                        writer.Write("a");
                        writer.Close();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        public void copyFile() {
            string fileName = "dudebotupdater.exe";
            string sourcePath = @"bin";
            string targetPath = System.IO.Path.GetDirectoryName(System.IO.Path.GetTempPath() + @"\dudebot");
            string sourceFile = System.IO.Path.Combine(sourcePath, fileName);
            string destFile = System.IO.Path.Combine(targetPath, fileName);
            System.IO.File.Copy(sourceFile, destFile, true);
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
            if (proc != null) { 
                proc.Kill();
                proc = null;
                console.Text = "BOT DISCONNECTED";
                open.IsEnabled = true;
                kill.IsEnabled = false;
            }
        }

        public void openBot(Object sender, RoutedEventArgs e)
        {
            /*
            if (proc == null)
            {
                proc = new Process();
                {
                    console.Text = "";
                    proc.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory() + "\\bin";
                    proc.StartInfo.FileName = "javaw";
                    proc.StartInfo.Arguments = "-jar bot.jar";
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.OutputDataReceived += proc_OutputDataReceived;
                    proc.Start();
                    proc.BeginOutputReadLine();
                    open.IsEnabled = false;
                    kill.IsEnabled = true;
                   
                }
            }
            */

            // TODO : pircbot -> c#
            bot = Utils.loadData(); // Sets up and connects bot object
            bot.botStartUp();
            bot.setVerbose(true);
            channel = bot.channel;
            oauth = bot.oauth;
            startupMessage = bot.startupMessage;
            endMessage = bot.endMessage;
            bot.connect("irc.twitch.tv", 6667, oauth);
            bot.joinChannel(channel);
            if (startupMessage != null)
            {
                bot.sendRawLine("PRIVMSG " + channel + " : /me " + startupMessage);
            }
            else
            {
                bot.sendRawLine("PRIVMSG " + channel + " : /me Hello!");
            }
            if (botColor != null)
            {
                bot.sendRawLine("PRIVMSG " + channel + " :" + "/color " + botColor);
            }
            File f = new File("song.txt");
            if (!f.exists())
            {
                PrintWriter writer = new PrintWriter("song.txt", "UTF-8");
                writer.close();
            }
            File f2 = new File("temp.txt");
            if (!f2.exists())
            {
                PrintWriter writer = new PrintWriter("temp.txt", "UTF-8");
                writer.close();
            }
            File f3 = new File("currentsong.txt");
            if (!f3.exists())
            {
                PrintWriter writer = new PrintWriter("currentsong.txt", "UTF-8");
                writer.close();
            }
            bot.sendRawLine("CAP REQ :twitch.tv/membership");

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

                if (!File.Exists(System.IO.Path.GetTempPath() + "dudebotsyncbot.txt"))
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
                copyFile();
                
            }
            catch (Exception e)
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
            syncUpdatesWithBot();
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
            syncUpdatesWithBot();
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
            syncUpdatesWithBot();
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
            syncUpdatesWithBot();
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
            syncUpdatesWithBot();
        }

        public void amountResult_changed(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if ((bool)cb.IsChecked)
            {
                revloReward.IsEnabled = false;
            } else
            {
                revloReward.IsEnabled = true;
            }
            syncUpdatesWithBot();
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
            syncUpdatesWithBot();
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
                if (!tb.Name.Equals("kill") && !tb.Name.Equals("open") && !tb.Name.Equals("flyoutbutton") && !tb.Name.Equals("b_a") && !tb.Name.Equals("b_b") && !tb.Name.Equals("b_c") && !tb.Name.Equals("b_d") && !tb.Name.Equals("b_e") && !tb.Name.Equals("b_f") && !tb.Name.Equals("websitebutton") && !tb.Name.Equals("discordbutton")) { 
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
                    fileCopy(timersFile, backuptimersfile);
                    fileCopy(commandsFile, backupcommandsfile);
                    fileCopy(quotesFile, backupquotesfile);
                    fileCopy(othersFile, backupothersfile);
                    fileCopy(sfxFile, backupsfxfile);
                    fileCopy(usersFile, backupusersfile);
                    fileCopy(eventsFile, backupeventsfile);
                    if (!File.Exists(currencyFile))
                    {
                        File.Create(currencyFile).Close();
                    }
                    if (!File.Exists(backupcurrencyfile))
                    {
                        File.Create(backupcurrencyfile).Close();
                    }
                    fileCopy(currencyFile, backupcurrencyfile);
                    if (!File.Exists(imagesFile))
                    {
                        File.Create(imagesFile).Close();
                    }
                    if (!File.Exists(backupimagesfile))
                    {
                        File.Create(backupimagesfile).Close();
                    }
                    fileCopy(imagesFile, backupimagesfile);
                    if (!File.Exists(purchasedranksFile))
                    {
                        File.Create(purchasedranksFile).Close();
                    }
                    if (!File.Exists(backuppurchasedranksfile))
                    {
                        File.Create(backuppurchasedranksfile).Close();
                    }
                    fileCopy(purchasedranksFile, backuppurchasedranksfile);
                    readFromCommands();
                    readFromQuotes();

                    getUserList();

                }
                catch (Exception)
                {
                }
            }
        }

        public void openBot()
        {
            proc = new Process();
            {
                proc.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory() + "\\bin";
                proc.StartInfo.FileName = "javaw";
                proc.StartInfo.Arguments = "-jar bot.jar";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.OutputDataReceived += proc_OutputDataReceived;
                proc.Start();
                proc.BeginOutputReadLine();
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
            readFromCommands();
            readFromTimers();
            readFromQuotes();
            readFromSFX();
            readFromEvents();
            readFromImages();
            readFromCurrency();
            readFromRanks();
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
                    fileCopy(timersFile, backuptimersfile);
                    fileCopy(commandsFile, backupcommandsfile);
                    fileCopy(quotesFile, backupquotesfile);
                    fileCopy(othersFile, backupothersfile);
                    fileCopy(sfxFile, backupsfxfile);
                    fileCopy(usersFile, backupusersfile);
                    fileCopy(eventsFile, backupeventsfile);
                    if (!File.Exists(currencyFile))
                    {
                        File.Create(currencyFile).Close();
                    }
                    if (!File.Exists(backupcurrencyfile))
                    {
                        File.Create(backupcurrencyfile).Close();
                    }
                    fileCopy(currencyFile, backupcurrencyfile);
                    if (!File.Exists(imagesFile))
                    {
                        File.Create(imagesFile).Close();
                    }
                    if (!File.Exists(backupimagesfile))
                    {
                        File.Create(backupimagesfile).Close();
                    }
                    fileCopy(imagesFile, backupimagesfile);
                    if (!File.Exists(ranksFile))
                    {
                        File.Create(ranksFile).Close();
                    }
                    if (!File.Exists(backupranksfile))
                    {
                        File.Create(backupranksfile).Close();
                    }
                    fileCopy(ranksFile, backupranksfile);
                    if (!File.Exists(purchasedranksFile))
                    {
                        File.Create(purchasedranksFile).Close();
                    }
                    if (!File.Exists(backuppurchasedranksfile))
                    {
                        File.Create(backuppurchasedranksfile).Close();
                    }
                    fileCopy(purchasedranksFile, backuppurchasedranksfile);

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
                                catch (Exception f)
                                {
                                }
                                Application.Current.Shutdown();
                            }
                        }
                    }
                }
                catch (Exception E)
                {
                }
                DispatcherTimer timer;
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(5000);
                timer.Tick += new EventHandler(timer_Tick);
                timer.Start();
                
                openBot();
                

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
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += (s, e) =>
            {
                Thread.Sleep(250);
            };
            if (new FileInfo(versionFile).Length == 0)
            {
                backgroundWorker.DoWork += (s, e) =>
                {
                    Thread.Sleep(250);
                };
                if (new FileInfo(versionFile).Length == 0)
                {
                    return 0;
                }
            }
            System.IO.StreamReader reader = new System.IO.StreamReader(versionFile);
            String version = reader.ReadLine();
            reader.Close();

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
                } catch (Exception)
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
                    catch (Exception f)
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
            string line;
            try
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(configFile);
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Equals("channel=") || line.Equals("channel=#"))
                    {
                        reader.Close();
                        try
                        {
                            System.IO.StreamReader reader2 = new System.IO.StreamReader(backupbotfile);
                            while ((line = reader2.ReadLine()) != null)
                            {
                                if (!reader2.ReadLine().Equals("channel=") || !reader2.ReadLine().Equals("channel=#"))
                                {
                                    getBackupConfig();
                                    fileCopy(backupcommandsfile, commandsFile);
                                    fileCopy(backuptimersfile, timersFile);
                                    fileCopy(backupquotesfile, quotesFile);
                                    fileCopy(backupothersfile, othersFile);
                                    fileCopy(backupsfxfile, sfxFile);
                                    fileCopy(backupusersfile, usersFile);
                                    fileCopy(backupeventsfile, eventsFile);
                                    if (!File.Exists(currencyFile))
                                    {
                                        File.Create(currencyFile).Close();
                                    }
                                    if (!File.Exists(backupcurrencyfile))
                                    {
                                        File.Create(backupcurrencyfile).Close();
                                    }
                                    fileCopy(backupcurrencyfile, currencyFile);
                                    if (!File.Exists(imagesFile))
                                    {
                                        File.Create(imagesFile).Close();
                                    }
                                    if (!File.Exists(backupimagesfile))
                                    {
                                        File.Create(backupimagesfile).Close();
                                    }
                                    fileCopy(backupimagesfile, imagesFile);
                                    if (!File.Exists(ranksFile))
                                    {
                                        File.Create(ranksFile).Close();
                                    }
                                    if (!File.Exists(backupranksfile))
                                    {
                                        File.Create(backupranksfile).Close();
                                    }
                                    fileCopy(backupranksfile, ranksFile);
                                    if (!File.Exists(purchasedranksFile))
                                    {
                                        File.Create(purchasedranksFile).Close();
                                    }
                                    if (!File.Exists(backuppurchasedranksfile))
                                    {
                                        File.Create(backuppurchasedranksfile).Close();
                                    }
                                    fileCopy(backuppurchasedranksfile, purchasedranksFile);
                                    reader2.Close();
                                    return true;
                                }
                            }
                            reader2.Close();
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                        break;
                    }
                }
                reader.Close();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private void getBackupConfig()
        {
            try
            {
                String line;
                String input = "";
                System.IO.StreamReader reader = new System.IO.StreamReader(backupbotfile);
                System.IO.StreamWriter writer = new System.IO.StreamWriter(tempFile);
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains("channel=") || line.Contains("oauth=") || line.Contains("botName=") || line.Contains("regulars=") || line.Contains("favSongs=") || line.Contains("numOfSongsToDisplay=")
                        || line.Contains("numOfSongsInQueuePerUser=") || line.Contains("maxSonglistLength=") || line.Contains("mustFollowToRequest=") || line.Contains("displayIfUserIsHere=")
                        || line.Contains("displaySonglistOneLine=") || line.Contains("requestsOn=") || line.Contains("requestCommands=") || line.Contains("favSongCommand=") || line.Contains("requestsTrigger=")
                        || line.Contains("requestCommands=") || line.Contains("songlistCommands=") || line.Contains("getTotalSongsCommands=") || line.Contains("getViewerCountCommands=")
                        || line.Contains("addtopCommands=") || line.Contains("addvipCommands=") || line.Contains("editCommands=") || line.Contains("nextCommands=") || line.Contains("clearCommands=")
                        || line.Contains("getCurrentCommands=") || line.Contains("getNextSongCommands=") || line.Contains("randomNextSong=") || line.Contains("editMySong=") || line.Contains("removeMySong=")
                        || line.Contains("adddonatorCommands=") || line.Contains("bannedKeywords=") || line.Contains("mySongPosition=") || line.Contains("timer=") || line.Contains("whispersOn=")
                        || line.Contains("quotesOn=") || line.Contains("minigameOn=") || line.Contains("googleSheet=") || line.Contains("sfxTimer=") || line.Contains("revloReward=") || line.Contains("amountResult=")
                        || line.Contains("minigameTimer=") || line.Contains("startupMessage=") || line.Contains("minigameEndMessage=") || line.Contains("subOnlyRequests=") 
                        || line.Contains("directInputRequests=") || line.Contains("youtubeLinkRequests=") || line.Contains("maxSongLimitOn=") || line.Contains("maxSongDuration=") || line.Contains("adventureUserJoinTime=")
                        || line.Contains("adventureCoolDownTime=") || line.Contains("adventureMinReward=") || line.Contains("adventureMaxReward=") || line.Contains("giveawaycommandname=")
                        || line.Contains("currencyName=") || line.Contains("currencyPerMinute=") || line.Contains("maxGamble=") || line.Contains("currencyCoolDownMinutes=") || line.Contains("currencyToggle=")
                        || line.Contains("currencyCommandName=") || line.Contains("vipSongCost") || line.Contains("vipSongToggle=") || line.Contains("gambleToggle=") || line.Contains("adventureToggle=")
                        || line.Contains("endMessage=") || line.Contains("vipRedeemCoolDownMinutes=") || line.Contains("autoShoutoutOnHost=") || line.Contains("quotesModOnly=") || line.Contains("imageDisplayTimeSeconds=")
                        || line.Contains("imageCoolDown=") || line.Contains("openImagesWindowOnStart=") || line.Contains("sfxOverallCoolDown=") || line.Contains("imagesOverallCoolDown=") 
                        || line.Contains("followersTextFile=") || line.Contains("subTextFile=") || line.Contains("followerMessage=") || line.Contains("subMessage") || line.Contains("rankupUnitCost=") || line.Contains("subCreditRedeemCost=")
                        || line.Contains("creditsPerSub="))
                    {
                        writer.Write(line + "\r");
                    }
                }
                reader.Close();
                writer.Close();
                System.IO.StreamReader reader2 = new System.IO.StreamReader(backupbotfile);
                while ((line = reader2.ReadLine()) != null)
                {
                    input += line;
                }
                reader2.Close();

                System.IO.StreamWriter writer2 = new System.IO.StreamWriter(tempFile, true);
                if (!input.Contains("channel=#"))
                {
                    writer2.Write("channel=#\r");
                }
                if (!input.Contains("oauth="))
                {
                    writer2.Write("oauth=\r");
                }
                if (!input.Contains("oauth="))
                {
                    writer2.Write("oauth=\r");
                }
                if (!input.Contains("botName="))
                {
                    writer2.Write("botName=\r");
                }
                if (!input.Contains("regulars="))
                {
                    writer2.Write("regulars=\r");
                }
                if (!input.Contains("favSongs="))
                {
                    writer2.Write("favSongs=\r");
                }
                if (!input.Contains("numOfSongsToDisplay="))
                {
                    writer2.Write("numOfSongsToDisplay=8\r");
                }
                if (!input.Contains("numOfSongsInQueuePerUser="))
                {
                    writer2.Write("numOfSongsInQueuePerUser=1\r");
                }
                if (!input.Contains("maxSonglistLength="))
                {
                    writer2.Write("maxSonglistLength=100\r");
                }
                if (!input.Contains("mustFollowToRequest="))
                {
                    writer2.Write("mustFollowToRequest=true\r");
                }
                if (!input.Contains("displayIfUserIsHere="))
                {
                    writer2.Write("displayIfUserIsHere=true\r");
                }
                if (!input.Contains("displaySonglistOneLine="))
                {
                    writer2.Write("displaySonglistOneLine=true\r");
                }
                if (!input.Contains("requestsOn="))
                {
                    writer2.Write("requestsOn=true\r");
                }
                if (!input.Contains("favSongCommand="))
                {
                    writer2.Write("favSongCommand=0=!requestfav,!songfav,!playfav\r");
                }
                if (!input.Contains("requestsTrigger="))
                {
                    writer2.Write("requestsTrigger=2=!requests\r");
                }
                if (!input.Contains("requestCommands="))
                {
                    writer2.Write("requestCommands=0=!request,!song,!play\r");
                }
                if (!input.Contains("songlistCommands="))
                {
                    writer2.Write("songlistCommands=0=!songlist,!list,!playlist\r");
                }
                if (!input.Contains("getTotalSongsCommands="))
                {
                    writer2.Write("getTotalSongsCommands=0=!queue,!length,!total\r");
                }
                if (!input.Contains("getViewerCountCommands="))
                {
                    writer2.Write("getViewerCountCommands=0=!viewers\r");
                }
                if (!input.Contains("addtopCommands="))
                {
                    writer2.Write("addtopCommands=2=!addtop\r");
                }
                if (!input.Contains("addvipCommands="))
                {
                    writer2.Write("addvipCommands=2=!addvip\r");
                }
                if (!input.Contains("editCommands="))
                {
                    writer2.Write("editCommands=2=!edit,!change\r");
                }
                if (!input.Contains("nextCommands="))
                {
                    writer2.Write("nextCommands=2=!next,!skip\r");
                }
                if (!input.Contains("clearCommands="))
                {
                    writer2.Write("clearCommands=2=!clear\r");
                }
                if (!input.Contains("getCurrentCommands="))
                {
                    writer2.Write("getCurrentCommands=0=!current,!playing,!currentsong\r");
                }
                if (!input.Contains("getNextSongCommands="))
                {
                    writer2.Write("getNextSongCommands=0=!nextsong\r");
                }
                if (!input.Contains("randomNextSong="))
                {
                    writer2.Write("randomNextSong=2=!randomnext,!nextrandom,!randomsong!songrandom\r");
                }
                if (!input.Contains("editMySong="))
                {
                    writer2.Write("editMySong=0=!editsong\r");
                }
                if (!input.Contains("removeMySong="))
                {
                    writer2.Write("removeMySong=0=!removesong,!wrongsong\r");
                }
                if (!input.Contains("adddonatorCommands"))
                {
                    writer2.Write("adddonatorCommands=2=!adddonator\r");
                }
                if (!input.Contains("bannedKeywords"))
                {
                    writer2.Write("bannedKeywords=\r");
                }
                if (!input.Contains("mySongPosition"))
                {
                    writer2.Write("mySongPosition=0=!mysong,!position,!songposition\r");
                }
                if (!input.Contains("timer="))
                {
                    writer2.Write("timer=20\r");
                }
                if (!input.Contains("whispersOn="))
                {
                    writer2.Write("whispersOn=true\r");
                }
                if (!input.Contains("quotesOn="))
                {
                    writer2.Write("quotesOn=true\r");
                }
                if (!input.Contains("minigameOn="))
                {
                    writer2.Write("minigameOn=true\r");
                }
                if (!input.Contains("googleSheet="))
                {
                    writer2.Write("googleSheet=\r");
                }
                if (!input.Contains("sfxTimer="))
                {
                    writer2.Write("sfxTimer=60\r");
                }
                if (!input.Contains("revloReward="))
                {
                    writer2.Write("revloReward=75\r");
                }
                if (!input.Contains("amountResult="))
                {
                    writer2.Write("amountResult=false\r");
                }
                if (!input.Contains("minigameTimer="))
                {
                    writer2.Write("minigameTimer=60\r");
                }
                if (!input.Contains("startupMessage="))
                {
                    writer2.Write("startupMessage=is here, let's party.\r");
                }
                if (!input.Contains("minigameEndMessage="))
                {
                    writer2.Write("minigameEndMessage=Time has run out for the current game, guesses cannot be added now!\r");
                }
                if (!input.Contains("subOnlyRequests="))
                {
                    writer2.Write("subOnlyRequests=Only subs can request right now!\r");
                }
                if (!input.Contains("directInputRequests="))
                {
                    writer2.Write("directInputRequests=true\r");
                }
                if (!input.Contains("youtubeLinkRequests="))
                {
                    writer2.Write("youtubeLinkRequests=true\r");
                }
                if (!input.Contains("maxSongLimitOn="))
                {
                    writer2.Write("maxSongLimitOn=false\r");
                }
                if (!input.Contains("maxSongDuration="))
                {
                    writer2.Write("maxSongDuration=10\r");
                }
                if (!input.Contains("adventureUserJoinTime="))
                {
                    writer2.Write("adventureUserJoinTime=120\r");
                }
                if (!input.Contains("adventureCoolDownTime="))
                {
                    writer2.Write("adventureCoolDownTime=20\r");
                }
                if (!input.Contains("adventureMinReward="))
                {
                    writer2.Write("adventureMinReward=25\r");
                }
                if (!input.Contains("adventureMaxReward="))
                {
                    writer2.Write("adventureMaxReward=75\r");
                }
                if (!input.Contains("giveawaycommandname="))
                {
                    writer2.Write("giveawaycommandname=!raffle\r");
                }
                if (!input.Contains("currencyName="))
                {
                    writer2.Write("currencyName=points\r");
                }
                if (!input.Contains("currencyPerMinute="))
                {
                    writer2.Write("currencyPerMinute=2\r");
                }
                if (!input.Contains("maxGamble="))
                {
                    writer2.Write("maxGamble=10000\r");
                }
                if (!input.Contains("currencyCoolDownMinutes="))
                {
                    writer2.Write("currencyCoolDownMinutes=5\r");
                }
                if (!input.Contains("currencyToggle="))
                {
                    writer2.Write("currencyToggle=false\r");
                }
                if (!input.Contains("currencyCommandName="))
                {
                    writer2.Write("currencyCommandName=!points\r");
                }
                if (!input.Contains("vipSongCost="))
                {
                    writer2.Write("vipSongCost=500\r");
                }
                if (!input.Contains("vipSongToggle="))
                {
                    writer2.Write("vipSongToggle=true\r");
                }
                if (!input.Contains("gambleToggle="))
                {
                    writer2.Write("gambleToggle=true\r");
                }
                if (!input.Contains("adventureToggle="))
                {
                    writer2.Write("adventureToggle=true\r");
                }
                if (!input.Contains("endMessage="))
                {
                    writer2.Write("endMessage=BYE FAM\r");
                }
                if (!input.Contains("vipRedeemCoolDownMinutes="))
                {
                    writer2.Write("vipRedeemCoolDownMinutes=60\r");
                }
                if (!input.Contains("autoShoutoutOnHost="))
                {
                    writer2.Write("autoShoutoutOnHost=true\r");
                }
                if (!input.Contains("quotesModOnly="))
                {
                    writer2.Write("quotesModOnly=true\r");
                }
                if (!input.Contains("imageDisplayTimeSeconds="))
                {
                    writer2.Write("imageDisplayTimeSeconds=3\r");
                }
                if (!input.Contains("imageCoolDown="))
                {
                    writer2.Write("imageCoolDown=60\r");
                }
                if (!input.Contains("openImageWindowOnStart="))
                {
                    writer2.Write("openImageWindowOnStart=false\r");
                }
                if (!input.Contains("sfxOverallCoolDown="))
                {
                    writer2.Write("sfxOverallCoolDown=0\r");
                }
                if (!input.Contains("imagesOverallCoolDown="))
                {
                    writer2.Write("imagesOverallCoolDown=0\r");
                }
                if (!input.Contains("followersTextFile="))
                {
                    writer2.Write("followersTextFile=\r");
                }
                if (!input.Contains("subTextFile="))
                {
                    writer2.Write("subTextFile=\r");
                }
                if (!input.Contains("followerMessage="))
                {
                    writer2.Write("followerMessage=$user just followed the stream! Thank You!\r");
                }
                if (!input.Contains("subMessage="))
                {
                    writer2.Write("subMessage=$user just subscribed to the stream! Thank You!\r");
                }
                if (!input.Contains("rankupUnitCost="))
                {
                    writer2.Write("rankupUnitCost=0\r");
                }
                if (!input.Contains("subCreditRedeemCost="))
                {
                    writer2.Write("subCreditRedeemCost=1\r");
                }
                if (!input.Contains("creditsPerSub="))
                {
                    writer2.Write("creditsPerSub=1\r");
                }
                writer2.Close();
                System.IO.StreamWriter writer3 = new System.IO.StreamWriter(configFile);
                writer3.Write("");
                writer3.Close();
                fileCopy(tempFile, configFile);
                System.IO.StreamWriter writer4 = new System.IO.StreamWriter(tempFile);
                writer4.Write("");
                writer4.Close();
            }
            catch (Exception)
            {

            }
        }

        private void preset()
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
        
        public void fileCopy(String f1, String f2)
        {
            try
            {
                File.WriteAllText(f2, String.Empty);

                using (FileStream stream = File.OpenRead(f1))
                using (FileStream writeStream = File.OpenWrite(f2))
                {
                    BinaryReader reader = new BinaryReader(stream);
                    BinaryWriter writer = new BinaryWriter(writeStream);

                    // create a buffer to hold the bytes 
                    byte[] buffer = new Byte[1024];
                    int bytesRead;

                    // while the read method returns bytes
                    // keep writing them to the output stream
                    while ((bytesRead =
                            stream.Read(buffer, 0, 1024)) > 0)
                    {
                        writeStream.Write(buffer, 0, bytesRead);
                    }
                    reader.Close();
                    writer.Close();
                }
            }
            catch (Exception e)
            {
            }
        }

        private void readConfig()
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

        private void writeToConfigAndReset(Object sender, RoutedEventArgs e)
        {
            writeToConfig(sender, e);
           
        }

        private async void writeToConfig(Object sender, RoutedEventArgs e)
        {
            string line;
            try
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(configFile);
                System.IO.StreamWriter writer = new System.IO.StreamWriter(tempFile);

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains("channel="))
                    {
                        if (streamerName.Text.StartsWith("#"))
                        {
                            streamerName.Text = streamerName.Text.Substring(1);
                        }
                        writer.Write("channel=#" + streamerName.Text.ToLower() + "\r");
                    }
                    else if (line.Contains("oauth="))
                    {
                        writer.Write("oauth=" + oauth.Text.ToLower() + "\r");
                    }
                    else if (line.Contains("googleSheet="))
                    {
                        writer.Write("googleSheet=" + googleSheetID.Text + "\r");
                    }
                    else if (line.Contains("botName="))
                    {
                        writer.Write("botName=" + botName.Text.ToLower() + "\r");
                    }
                    else if (line.Contains("regulars="))
                    {
                        writer.Write("regulars=" + regulars.Text.ToLower() + "\r");
                    }
                    else if (line.Contains("favSongs="))
                    {
                        writer.Write("favSongs=" + favSongList.Text.ToLower() + "\r");
                    }
                    else if (line.Contains("numOfSongsToDisplay="))
                    {
                        writer.Write("numOfSongsToDisplay=" + numberSongsSongListDisplays.Value + "\r");
                    }
                    else if (line.Contains("numOfSongsInQueuePerUser="))
                    {
                        writer.Write("numOfSongsInQueuePerUser=" + maxNumberSongsPerUser.Value + "\r");
                    }
                    else if (line.Contains("maxSonglistLength="))
                    {
                        writer.Write("maxSonglistLength=" + maxSongListLength.Value + "\r");
                    }
                    else if (line.Contains("mustFollowToRequest="))
                    {
                        if (followToRequestCheckbox.IsChecked == true)
                        {
                            writer.Write("mustFollowToRequest=true" + "\r");
                        }
                        else
                        {
                            writer.Write("mustFollowToRequest=false" + "\r");
                        }
                    }
                    else if (line.Contains("displayIfUserIsHere="))
                    {
                        if (displayIfUserIsHereCheckbox.IsChecked == true)
                        {
                            writer.Write("displayIfUserIsHere=true" + "\r");
                        }
                        else
                        {
                            writer.Write("displayIfUserIsHere=false" + "\r");
                        }
                    }
                    else if (line.Contains("displaySonglistOneLine="))
                    {
                        if (songlistOneLineCheckbox.IsChecked == true)
                        {
                            writer.Write("displaySonglistOneLine=true" + "\r");
                        }
                        else
                        {
                            writer.Write("displaySonglistOneLine=false" + "\r");
                        }
                    }
                    else if (line.Contains("requestsOn="))
                    {
                        if (requestOnCheckbox.IsChecked == true)
                        {
                            writer.Write("requestsOn=true" + "\r");
                        }
                        else
                        {
                            writer.Write("requestsOn=false" + "\r");
                        }
                    }
                    else if (line.Contains("requestCommands="))
                    {
                        writer.Write("requestCommands=" + requestLevel.SelectedIndex + "=" + requestCommandNames.Text + "\r");
                    }
                    else if (line.Contains("favSongCommand="))
                    {
                        writer.Write("favSongCommand=" + playFavLevel.SelectedIndex + "=" + playFavCommandNames.Text + "\r");
                    }
                    else if (line.Contains("requestsTrigger="))
                    {
                        writer.Write("requestsTrigger=" + triggerRequestsLevel.SelectedIndex + "=" + triggerRequestsCommandNames.Text + "\r");
                    }
                    else if (line.Contains("requestCommands="))
                    {
                        writer.Write("requestCommands=" + requestLevel.SelectedIndex + "=" + requestCommandNames.Text + "\r");
                    }
                    else if (line.Contains("songlistCommands="))
                    {
                        writer.Write("songlistCommands=" + songListLevel.SelectedIndex + "=" + songListCommandNames.Text + "\r");
                    }
                    else if (line.Contains("getTotalSongsCommands="))
                    {
                        writer.Write("getTotalSongsCommands=" + getTotalSongsLevel.SelectedIndex + "=" + getTotalSongsInListCommandNames.Text + "\r");
                    }
                    else if (line.Contains("getViewerCountCommands="))
                    {
                        writer.Write("getViewerCountCommands=" + getViewerCountLevel.SelectedIndex + "=" + getViewerCountCommandNames.Text + "\r");
                    }
                    else if (line.Contains("addtopCommands="))
                    {
                        writer.Write("addtopCommands=" + addTopLevel.SelectedIndex + "=" + addTopCommandNames.Text + "\r");
                    }
                    else if (line.Contains("addvipCommands="))
                    {
                        writer.Write("addvipCommands=" + addVipLevel.SelectedIndex + "=" + addVipCommandNames.Text + "\r");
                    }
                    else if (line.Contains("editCommands="))
                    {
                        writer.Write("editCommands=" + editLevel.SelectedIndex + "=" + editCommandNames.Text + "\r");
                    }
                    else if (line.Contains("nextCommands="))
                    {
                        writer.Write("nextCommands=" + nextLevel.SelectedIndex + "=" + nextCommandNames.Text + "\r");
                    }
                    else if (line.Contains("clearCommands="))
                    {
                        writer.Write("clearCommands=" + clearLevel.SelectedIndex + "=" + clearCommandNames.Text + "\r");
                    }
                    else if (line.Contains("getCurrentCommands="))
                    {
                        writer.Write("getCurrentCommands=" + displayCurrentLevel.SelectedIndex + "=" + displayCurrentCommandName.Text + "\r");
                    }
                    else if (line.Contains("getNextSongCommands="))
                    {
                        writer.Write("getNextSongCommands=" + displayNextLevel.SelectedIndex + "=" + displayNextSongCommandName.Text + "\r");
                    }
                    else if (line.Contains("randomNextSong="))
                    {
                        writer.Write("randomNextSong=" + randomNextSongLevel.SelectedIndex + "=" + randomNextSongCommandName.Text + "\r");
                    }
                    else if (line.Contains("editMySong="))
                    {
                        writer.Write("editMySong=" + editReqSongLevel.SelectedIndex + "=" + editReqSongCommandName.Text + "\r");
                    }
                    else if (line.Contains("removeMySong="))
                    {
                        writer.Write("removeMySong=" + removeReqSongLevel.SelectedIndex + "=" + removeReqSongCommand.Text + "\r");
                    }
                    else if (line.Contains("adddonatorCommands="))
                    {
                        writer.Write("adddonatorCommands=" + addDonationLevel.SelectedIndex + "=" + addDonationCommand.Text + "\r");
                    }
                    else if (line.Contains("bannedKeywords="))
                    {
                        writer.Write("bannedKeywords=" + bannedKeywords.Text + "\r");
                    }
                    else if (line.Contains("mySongPosition="))
                    {
                        writer.Write("mySongPosition=" + songPositionLevel.SelectedIndex + "=" + songPositionName.Text + "\r");
                    }
                    else if (line.Contains("timer="))
                    {
                        writer.Write("timer=" + refreshtimer.Value + "\r");
                    }
                    else if (line.Contains("whispersOn="))
                    {
                        if (whisperCheckbox.IsChecked == true)
                        {
                            writer.Write("whispersOn=true" + "\r");
                        }
                        else
                        {
                            writer.Write("whispersOn=false" + "\r");
                        }
                    }
                    else if (line.Contains("quotesOn="))
                    {
                        if (quotesOn.IsChecked == true)
                        {
                            writer.Write("quotesOn=true" + "\r");
                        }
                        else
                        {
                            writer.Write("quotesOn=false" + "\r");
                        }
                    }
                    else if (line.Contains("minigameOn="))
                    {
                        if (minigameToggle.IsChecked == true)
                        {
                            writer.Write("minigameOn=true" + "\r");
                        }
                        else
                        {
                            writer.Write("minigameOn=false" + "\r");
                        }
                    }
                    else if (line.Contains("sfxTimer="))
                    {
                        writer.Write("sfxTimer=" + sfxCoolDownTextBox.Value + "\r");
                    }
                    else if (line.Contains("revloReward="))
                    {
                        writer.Write("revloReward=" + revloReward.Value + "\r");
                    }
                    else if (line.Contains("amountResult="))
                    {
                        if (amountResult.IsChecked == true)
                        {
                            writer.Write("amountResult=true" + "\r");
                        }
                        else
                        {
                            writer.Write("amountResult=false" + "\r");
                        }
                    }
                    else if (line.Contains("minigameTimer="))
                    {
                        writer.Write("minigameTimer=" + minigametimer.Value + "\r");
                    }
                    else if (line.Contains("startupMessage="))
                    {
                        writer.Write("startupMessage=" + startupMessage.Text + "\r");
                    }
                    else if (line.Contains("minigameEndMessage="))
                    {
                        writer.Write("minigameEndMessage=" + minigameEndMessage.Text + "\r");
                    }
                    else if (line.Contains("subOnlyRequests="))
                    {
                        writer.Write("subOnlyRequests=" + subOnlyRequests.Text + "\r");
                    }
                    else if (line.Contains("directInputRequests="))
                    {
                        if (direquests.IsChecked == true)
                        {
                            writer.Write("directInputRequests=true" + "\r");
                        }
                        else
                        {
                            writer.Write("directInputRequests=false" + "\r");
                        }
                    }
                    else if (line.Contains("youtubeLinkRequests="))
                    {
                        if (ylrequests.IsChecked == true)
                        {
                            writer.Write("youtubeLinkRequests=true" + "\r");
                        }
                        else
                        {
                            writer.Write("youtubeLinkRequests=false" + "\r");
                        }
                    }
                    else if (line.Contains("maxSongLimitOn="))
                    {
                        if (songduration.IsChecked == true)
                        {
                            writer.Write("maxSongLimitOn=true" + "\r");
                        }
                        else
                        {
                            writer.Write("maxSongLimitOn=false" + "\r");
                        }
                    }
                    else if (line.Contains("maxSongDuration="))
                    {
                        writer.Write("maxSongDuration=" + songdurationlimit.Value + "\r");
                    }
                    else if (line.Contains("adventureUserJoinTime="))
                    {
                        writer.Write("adventureUserJoinTime=" + adventurejointime.Value + "\r");
                    }
                    else if (line.Contains("adventureCoolDownTime="))
                    {
                        writer.Write("adventureCoolDownTime=" + adventurecooldowntime.Value + "\r");
                    }
                    else if (line.Contains("adventureMinReward="))
                    {
                        writer.Write("adventureMinReward=" + adventureminreward.Value + "\r");
                    }
                    else if (line.Contains("adventureMaxReward="))
                    {
                        writer.Write("adventureMaxReward=" + adventuremaxreward.Value + "\r");
                    }
                    else if (line.Contains("giveawaycommandname="))
                    {
                        writer.Write("giveawaycommandname=" + giveawaycommandname.Text + "\r");
                    }
                    else if (line.Contains("currencyName="))
                    {
                        writer.Write("currencyName=" + currencyName.Text + "\r");
                    }
                    else if (line.Contains("currencyPerMinute="))
                    {
                        writer.Write("currencyPerMinute=" + currencyPerMinute.Value + "\r");
                    }
                    else if (line.Contains("maxGamble="))
                    {
                        writer.Write("maxGamble=" + maxGamble.Value + "\r");
                    }
                    else if (line.Contains("currencyCoolDownMinutes="))
                    {
                        writer.Write("currencyCoolDownMinutes=" + gambleCoolDown.Value + "\r");
                    }
                    else if (line.Contains("currencyToggle="))
                    {
                        if (currencyToggle.IsChecked == true)
                        {
                            writer.Write("currencyToggle=true" + "\r");
                        }
                        else
                        {
                            writer.Write("currencyToggle=false" + "\r");
                        }
                    }
                    else if (line.Contains("currencyCommandName="))
                    {
                        writer.Write("currencyCommandName=" + currencyCommand.Text + "\r");
                    }
                    else if (line.Contains("vipSongCost="))
                    {
                        writer.Write("vipSongCost=" + vipsongcost.Value + "\r");
                    }
                    else if (line.Contains("vipSongToggle="))
                    {
                        if (vipsongtoggle.IsChecked == true)
                        {
                            writer.Write("vipSongToggle=true" + "\r");
                        }
                        else
                        {
                            writer.Write("vipSongToggle=false" + "\r");
                        }
                    }
                    else if (line.Contains("gambleToggle="))
                    {
                        if (gambleToggle.IsChecked == true)
                        {
                            writer.Write("gambleToggle=true" + "\r");
                        }
                        else
                        {
                            writer.Write("gambleToggle=false" + "\r");
                        }
                    }
                    else if (line.Contains("adventureToggle="))
                    {
                        if (adventureToggle.IsChecked == true)
                        {
                            writer.Write("adventureToggle=true" + "\r");
                        }
                        else
                        {
                            writer.Write("adventureToggle=false" + "\r");
                        }
                    }
                    else if (line.Contains("endMessage="))
                    {
                        writer.Write("endMessage=" + endMessage.Text + "\r");
                    }
                    else if (line.Contains("vipRedeemCoolDownMinutes="))
                    {
                        writer.Write("vipRedeemCoolDownMinutes=" + vipRedeemCoolDownMinutes.Value + "\r");
                    }
                    else if (line.Contains("autoShoutoutOnHost="))
                    {
                        if (autoShoutoutOnHost.IsChecked == true)
                        {
                            writer.Write("autoShoutoutOnHost=true" + "\r");
                        }
                        else
                        {
                            writer.Write("autoShoutoutOnHost=false" + "\r");
                        }
                    }
                    else if (line.Contains("quotesModOnly="))
                    {
                        if (quotesModOnly.IsChecked == true)
                        {
                            writer.Write("quotesModOnly=true" + "\r");
                        }
                        else
                        {
                            writer.Write("quotesModOnly=false" + "\r");
                        }
                    }
                    else if (line.Contains("imageDisplayTimeSeconds="))
                    {
                        writer.Write("imageDisplayTimeSeconds=" + imageDisplayTime.Value + "\r");
                    }
                    else if (line.Contains("imageCoolDown="))
                    {
                        writer.Write("imageCoolDown=" + imageCoolDown.Value + "\r");
                    }
                    else if (line.Contains("openImageWindowOnStart="))
                    {
                        if (openImageWindowOnStart.IsChecked == true)
                        {
                            writer.Write("openImageWindowOnStart=true" + "\r");
                        }
                        else
                        {
                            writer.Write("openImageWindowOnStart=false" + "\r");
                        }
                    }
                    else if (line.Contains("sfxOverallCoolDown="))
                    {
                        writer.Write("sfxOverallCoolDown=" + sfxOverallCoolDown.Value + "\r");
                    }
                    else if (line.Contains("imagesOverallCoolDown="))
                    {
                        writer.Write("imagesOverallCoolDown=" + ImagesOverallCoolDown.Value + "\r");
                    }
                    else if (line.Contains("followersTextFile="))
                    {
                        writer.Write("followersTextFile=" + followersTextFile.Text + "\r");
                    }
                    else if (line.Contains("subTextFile="))
                    {
                        writer.Write("subTextFile=" + subTextFile.Text + "\r");
                    }
                    else if (line.Contains("followerMessage="))
                    {
                        writer.Write("followerMessage=" + followerMessage.Text + "\r");
                    }
                    else if (line.Contains("subMessage="))
                    {
                        writer.Write("subMessage=" + subMessage.Text + "\r");
                    }
                    else if (line.Contains("rankupUnitCost="))
                    {
                        writer.Write("rankupUnitCost=" + rankunitdropbox.SelectedIndex + "\r");
                    }
                    else if (line.Contains("subCreditRedeemCost="))
                    {
                        writer.Write("subCreditRedeemCost=" + subCreditRedeemCost.Value + "\r");
                    }
                    else if (line.Contains("creditsPerSub="))
                    {
                        writer.Write("creditsPerSub=" + creditsPerSub.Value + "\r");
                    }
                }
                reader.Close();
                writer.Close();

                File.WriteAllText(configFile, String.Empty);

                using (FileStream stream = File.OpenRead(tempFile))
                using (FileStream writeStream = File.OpenWrite(configFile))
                {
                    BinaryReader reader2 = new BinaryReader(stream);
                    BinaryWriter writer2 = new BinaryWriter(writeStream);

                    // create a buffer to hold the bytes 
                    byte[] buffer = new Byte[1024];
                    int bytesRead;

                    // while the read method returns bytes
                    // keep writing them to the output stream
                    while ((bytesRead =
                            stream.Read(buffer, 0, 1024)) > 0)
                    {
                        writeStream.Write(buffer, 0, bytesRead);
                    }
                    reader2.Close();
                    writer2.Close();
                }
                File.WriteAllText(tempFile, String.Empty);

                fileCopy(configFile, backupbotfile);
                fileCopy(commandsFile, backupcommandsfile);
                fileCopy(timersFile, backuptimersfile);
                fileCopy(quotesFile, backupquotesfile);
                fileCopy(othersFile, backupothersfile);
                fileCopy(sfxFile, backupsfxfile);
                fileCopy(usersFile, backupusersfile);
                fileCopy(eventsFile, backupeventsfile);
                if (!File.Exists(currencyFile))
                {
                    File.Create(currencyFile).Close();
                }
                if (!File.Exists(backupcurrencyfile))
                {
                    File.Create(backupcurrencyfile).Close();
                }
                fileCopy(currencyFile, backupcurrencyfile);
                if (!File.Exists(imagesFile))
                {
                    File.Create(imagesFile).Close();
                }
                if (!File.Exists(backupimagesfile))
                {
                    File.Create(backupimagesfile).Close();
                }
                fileCopy(imagesFile, backupimagesfile);
                if (!File.Exists(ranksFile))
                {
                    File.Create(ranksFile).Close();
                }
                if (!File.Exists(backupranksfile))
                {
                    File.Create(backupranksfile).Close();
                }
                fileCopy(ranksFile, backupranksfile);
            }
            catch (Exception)
            {
            }
            if (e.Source.ToString().Contains("Button"))
            {
                await this.ShowMessageAsync("", "Changes Applied!");
            }
            readTextFile();
            syncUpdatesWithBot();
            if (writeAndReset)
            {
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
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

        private void readTextFile()
        {
            try
            {
                string line;
                int counter = 0;
                System.IO.StreamReader reader = new System.IO.StreamReader(songsFile);
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
                        System.IO.StreamReader file2 = new System.IO.StreamReader(songsFile);
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
            readFromCurrency();
        }

        public async void clearlist(Object sender, RoutedEventArgs e)
        {
            this.MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Accented;
            var msgbox_settings = new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" };
            MessageDialogResult messageBoxResult = await this.ShowMessageAsync("", "Are you sure you want to clear the song list?", MessageDialogStyle.AffirmativeAndNegative, msgbox_settings);
            if (messageBoxResult == MessageDialogResult.Affirmative)
            {
                try
                {
                    System.IO.StreamWriter writer = new System.IO.StreamWriter(songsFile);
                    writer.Write("");
                    writer.Close();
                    System.IO.StreamWriter writer2 = new System.IO.StreamWriter(currentsongFile);
                    writer2.Write("Song list is empty");
                    writer2.Close();
                    System.IO.StreamWriter writer3 = new System.IO.StreamWriter(currentrequesterFile);
                    writer3.Write("");
                    writer3.Close();
                    refresh.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    songArray = new String[1000];
                    typeArray = new String[1000];
                    requesterArray = new String[1000];
                    syncUpdatesWithBot();
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
                readFromQuotes();
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
            } catch (Exception f)
            {
            }
          
        }

        public class SongInfo
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

        public class Currency
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

        public class Ranks
        {
            public TextBlock rankTitle { get; set; }
            public TextBlock rankCost { get; set; }

            public Ranks(TextBlock rankTitle, TextBlock rankCost)
            {
                this.rankTitle = rankTitle;
                this.rankCost = rankCost;
            }
        }

        public class Commands
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

        public class Timers
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

        public class Quotes
        {
            public TextBlock quoteInfo { get; set; }

            public Quotes(TextBlock quoteInfo)
            {
                this.quoteInfo = quoteInfo;
            }
        }

        public class SFX
        {
            public TextBlock sfxCommand { get; set; }
            public TextBlock sfxResponse { get; set; }

            public SFX(TextBlock sfxCommand, TextBlock sfxResponse)
            {
                this.sfxCommand = sfxCommand;
                this.sfxResponse = sfxResponse;
            }
        }

        public class Images
        {
            public TextBlock imageCommand { get; set; }
            public TextBlock imageResponse { get; set; }

            public Images(TextBlock imageCommand, TextBlock imageResponse)
            {
                this.imageCommand = imageCommand;
                this.imageResponse = imageResponse;
            }
        }

        public class Events
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
                writeToCurrency();
            } catch (Exception)
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
                writeToCurrency();
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
                writeToCurrency();
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
                writeToCurrency();
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
                    syncUpdatesWithBot();
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
                System.IO.StreamReader br = new System.IO.StreamReader(songsFile);
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
                System.IO.StreamReader reader = new System.IO.StreamReader(songsFile);
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
                System.IO.StreamReader reader2 = new System.IO.StreamReader(songsFile);
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
                System.IO.StreamReader reader3 = new System.IO.StreamReader(songsFile);
                System.IO.StreamWriter writer = new System.IO.StreamWriter(tempFile);
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
                System.IO.StreamWriter writer2 = new System.IO.StreamWriter(songsFile);
                writer2.Write("");
                writer2.Close();
                fileCopy(tempFile, songsFile);
                System.IO.StreamWriter writer3 = new System.IO.StreamWriter(tempFile);
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
                syncUpdatesWithBot();
                readTextFile();
            } catch (Exception)
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
                System.IO.StreamReader reader = new System.IO.StreamReader(songsFile);
                System.IO.StreamWriter writer = new System.IO.StreamWriter(tempFile);
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

                System.IO.StreamWriter writer2 = new System.IO.StreamWriter(songsFile);
                writer2.Write("");
                writer2.Close();
                fileCopy(tempFile, songsFile);
                System.IO.StreamWriter writer3 = new System.IO.StreamWriter(tempFile);
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
                fileCopy(othersFile, backupothersfile);
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
        }

        public void readFromCurrency()
        {
            String line;
            try
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(currencyFile);
                CurrencyArray.Clear();
                int count = 0;
                String user = "", amount = "", time = "", subCredits = "";
                while ((line = reader.ReadLine()) != null)
                {
                    String[] temp = line.Split('\t');
                    user = temp[0];
                    amount = temp[1];
                    if (temp[2] != null)
                    {
                        time = temp[2];
                    } else
                    {
                        time = "0";
                    }
                    if (temp.Length > 3)
                    {
                        subCredits = temp[3];
                    } else
                    {
                        subCredits = "0";
                    }
                    TextBlock tb = new TextBlock();
                    tb.Text = user;
                    tb.Uid = count.ToString();
                    TextBlock tb2 = new TextBlock();
                    tb2.Text = amount;
                    tb2.Uid = count.ToString();
                    TextBlock tb3 = new TextBlock();
                    tb3.Text = time;
                    tb3.Uid = count.ToString();
                    TextBlock tb4 = new TextBlock();
                    tb4.Text = subCredits;
                    tb4.Uid = count.ToString();
                    currencyuser.Insert(count, user);
                    currencyamount.Insert(count, amount);
                    currencytime.Insert(count, time);
                    currencysubcredits.Insert(count, subCredits);
                    
                    CurrencyArray.Add(new Currency(tb, tb2, tb3, tb4));
                    count++;
                }
                currencydatagrid.ItemsSource = CurrencyArray;
                reader.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.StackTrace);
            }
        }

        public void readFromRanks()
        {
            String line;
            try
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(ranksFile);
                RankArray.Clear();
                int count = 0;
                String title = "", cost = "";
                while ((line = reader.ReadLine()) != null)
                {
                    String[] temp = line.Split('\t');
                    title = temp[0];
                    cost = temp[1];
                    TextBlock tb = new TextBlock();
                    tb.Text = title;
                    tb.Uid = count.ToString();
                    TextBlock tb2 = new TextBlock();
                    tb2.Text = cost;
                    tb2.Uid = count.ToString();
                    ranktitle.Insert(count, title);
                    rankcost.Insert(count, cost);
                    RankArray.Add(new Ranks(tb, tb2));
                    count++;
                }
                rankgrid.ItemsSource = RankArray;
                reader.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.StackTrace);
            }
        }

        public void readFromCommands()
        {
            String line;
            try
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(commandsFile);
                commandArray.Clear();
                responses = new String[1000];
                commands = new String[1000];
                commandLevels = new String[1000];
                int count = 0;
                String response = "", level = "0";
                while ((line = reader.ReadLine()) != null)
                {
                    String commandName = line.Substring(0, line.IndexOf("\t"));
                    if (line.EndsWith("\t0") || line.EndsWith("\t1") || line.EndsWith("\t2") || line.EndsWith("\t3")) {
                        String[] temp = line.Split('\t');
                        response = temp[1];
                        level = temp[2];
                    }
                    else
                    {
                        response = line.Substring(line.IndexOf("\t") + 1);
                    }
                    TextBlock tb = new TextBlock();
                    tb.Text = commandName;
                    tb.Uid = count.ToString();
                    tb.MouseLeftButtonDown += new MouseButtonEventHandler(tb_MouseLeftButtonDown);
                    TextBlock tb2 = new TextBlock();
                    tb2.Text = response;
                    tb2.Uid = count.ToString();
                    TextBlock tb3 = new TextBlock();
                    if (level.Equals("0"))
                    {
                        tb3.Text = "Everyone";
                    }
                    else if (level.Equals("1"))
                    {
                        tb3.Text = "Subs, Mods, Streamer";
                    }
                    else if(level.Equals("2"))
                    {
                        tb3.Text = "Mods, Streamer";
                    }
                    else if(level.Equals("3"))
                    {
                        tb3.Text = "Streamer";
                    }
                    tb3.Uid = count.ToString();
                    responses[count] = response;
                    commands[count] = commandName;
                    commandLevels[count] = level;
                    tb2.MouseLeftButtonDown += new MouseButtonEventHandler(tb_MouseLeftButtonDown);
                    commandArray.Add(new Commands(tb, tb2, tb3));
                    count++;
                }
                commanddatagrid.ItemsSource = commandArray;
                reader.Close();
            }
            catch (Exception)
            {
            }
        }

        public void readFromTimers()
        {
            String line;
            try
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(timersFile);
                timerArray.Clear();
                Timedresponses = new String[1000];
                Timedcommands = new String[1000];
                Timedtoggle = new String[1000];
                int count = 0;
                String response = "", toggle = "1";
                while ((line = reader.ReadLine()) != null)
                {
                    String commandName = line.Substring(0, line.IndexOf("\t"));
                    if (line.EndsWith("\t0") || line.EndsWith("\t1") || line.EndsWith("\t2") || line.EndsWith("\t3"))
                    {
                        String[] temp = line.Split('\t');
                        response = temp[1];
                        toggle = temp[2];
                    }
                    else
                    {
                        response = line.Substring(line.IndexOf("\t") + 1);
                    }
                    TextBlock tb = new TextBlock();
                    tb.Text = commandName;
                    tb.Uid = count.ToString();
                    // tb.MouseLeftButtonDown += new MouseButtonEventHandler(tb_MouseLeftButtonDown2);
                    TextBlock tb2 = new TextBlock();
                    tb2.Text = response;
                    tb2.Uid = count.ToString();
                    TextBlock tb3 = new TextBlock();
                    tb3.Uid = count.ToString();
                    if (toggle.Equals("1"))
                    {
                        tb3.Text = "ON";
                    }
                    else
                    {
                        tb3.Text = "OFF";
                    }
                    Timedresponses[count] = response;
                    Timedcommands[count] = commandName;
                    Timedtoggle[count] = toggle;
                    // tb2.MouseLeftButtonDown += new MouseButtonEventHandler(tb_MouseLeftButtonDown2);
                    timerArray.Add(new Timers(tb, tb2, tb3));
                    count++;
                }
                timerdatagrid.ItemsSource = timerArray;
                reader.Close();
            }
            catch (Exception)
            {
            }
        }

        public void readFromSFX()
        {
            String line;
            try
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(sfxFile);
                SFXArray.Clear();
                SFXcommands = new String[1000];
                SFXresponses = new String[1000];
                int count = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    String commandName = line.Substring(0, line.IndexOf("\t"));
                    String response = line.Substring(line.IndexOf("\t") + 1);
                    TextBlock tb = new TextBlock();
                    tb.Text = commandName;
                    tb.Uid = count.ToString();
                    tb.MouseLeftButtonDown += new MouseButtonEventHandler(tb_MouseLeftButtonDown4);
                    TextBlock tb2 = new TextBlock();
                    tb2.Text = response;
                    tb2.Uid = count.ToString();
                    SFXresponses[count] = response;
                    SFXcommands[count] = commandName;
                    tb2.MouseLeftButtonDown += new MouseButtonEventHandler(tb_MouseLeftButtonDown4);
                    SFXArray.Add(new SFX(tb, tb2));
                    count++;
                }
                SFXdatagrid.ItemsSource = SFXArray;
                reader.Close();
            }
            catch (Exception)
            {
            }
        }

        public void readFromImages()
        {
            String line;
            try
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(imagesFile);
                ImagesArray.Clear();
                imagecommands = new String[1000];
                imageresponses = new String[1000];
                int count = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    String commandName = line.Substring(0, line.IndexOf("\t"));
                    String response = line.Substring(line.IndexOf("\t") + 1);
                    TextBlock tb = new TextBlock();
                    tb.Text = commandName;
                    tb.Uid = count.ToString();
                    tb.MouseLeftButtonDown += new MouseButtonEventHandler(tb_MouseLeftButtonDown8);
                    TextBlock tb2 = new TextBlock();
                    tb2.Text = response;
                    tb2.Uid = count.ToString();
                    imagecommands[count] = response;
                    imageresponses[count] = commandName;
                    tb2.MouseLeftButtonDown += new MouseButtonEventHandler(tb_MouseLeftButtonDown8);
                    ImagesArray.Add(new Images(tb, tb2));
                    count++;
                }
                imagedatagrid.ItemsSource = ImagesArray;
                reader.Close();
            }
            catch (Exception)
            {
            }
        }

        public void readFromEvents()
        {
            String line;
            try
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(eventsFile);
                EventsArray.Clear();
                eventusers = new String[1000];
                eventmessages = new String[1000];
                int count = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    String commandName = line.Substring(0, line.IndexOf("\t"));
                    String response = line.Substring(line.IndexOf("\t") + 1);
                    TextBlock tb = new TextBlock();
                    tb.Text = commandName;
                    tb.Uid = count.ToString();
                    tb.MouseLeftButtonDown += new MouseButtonEventHandler(tb_MouseLeftButtonDown7);
                    TextBlock tb2 = new TextBlock();
                    tb2.Text = response;
                    tb2.Uid = count.ToString();
                    eventmessages[count] = response;
                    eventusers[count] = commandName;
                    tb2.MouseLeftButtonDown += new MouseButtonEventHandler(tb_MouseLeftButtonDown7);
                    EventsArray.Add(new Events(tb, tb2));
                    count++;
                }
                eventsdatagrid.ItemsSource = EventsArray;
                reader.Close();
            }
            catch (Exception)
            {
            }
        }

        public void readFromQuotes()
        {
            String line;
            try
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(quotesFile);
                QuotesArray.Clear();
                quotes.Clear();
                int count = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    String quoteline = line;
                    TextBlock tb = new TextBlock();
                    tb.Text = quoteline;
                    tb.Uid = count.ToString();
                    quotes.Add(quoteline);
                    tb.MouseLeftButtonDown += new MouseButtonEventHandler(tb_MouseLeftButtonDown3);
                    QuotesArray.Add(new Quotes(tb));
                    count++;
                }
                quotesdatagrid.ItemsSource = QuotesArray;
                reader.Close();
            }
            catch (Exception)
            {
            }
        }

        private void tb_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var line = sender as TextBlock;
            editCommand.Text = commandArray[Int32.Parse(line.Uid)].commandName.Text;
            editResponse.Text = commandArray[Int32.Parse(line.Uid)].commandResponse.Text;
            String level = commandArray[Int32.Parse(line.Uid)].commandLevel.Text;
            if (level.Equals("Everyone")) {
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


        public void writeToSFX()
        {
            try
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter(sfxFile);
                for (int i = 0; i < SFXArray.Count; i++)
                {
                    var item = SFXArray[i].sfxCommand as TextBlock;
                    var item2 = SFXArray[i].sfxResponse as TextBlock;
                    writer.Write(item.Text + "\t" + item2.Text + "\r");
                }
                writer.Close();
                readFromSFX();
            }
            catch (Exception)
            {
            }
        }


        public void writeToImages()
        {
            try
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter(imagesFile);
                for (int i = 0; i < ImagesArray.Count; i++)
                {
                    var item = ImagesArray[i].imageCommand as TextBlock;
                    var item2 = ImagesArray[i].imageResponse as TextBlock;
                    writer.Write(item.Text + "\t" + item2.Text + "\r");
                }
                writer.Close();
                readFromImages();
            }
            catch (Exception)
            {
            }
        }

        public void writeToEvents()
        {
            try
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter(eventsFile);
                for (int i = 0; i < EventsArray.Count; i++)
                {
                    var item = EventsArray[i].eventUser as TextBlock;
                    var item2 = EventsArray[i].eventMessage as TextBlock;
                    writer.Write(item.Text + "\t" + item2.Text + "\r");
                }
                writer.Close();
                readFromEvents();
            }
            catch (Exception)
            {
            }
        }

        public void writeToCommands()
        {
            try
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter(commandsFile);
                for (int i = 0; i < commandArray.Count; i++)
                {
                    var item = commandArray[i].commandName as TextBlock;
                    var item2 = commandArray[i].commandResponse as TextBlock;
                    var item3 = commandArray[i].commandLevel as TextBlock;
                    String level = "0";
                    if (item3.Text.Equals("Everyone"))
                    {
                        level = "0";
                    }
                    else if (item3.Text.Equals("Subs, Mods, Streamer"))
                    {
                        level = "1";
                    }
                    else if (item3.Text.Equals("Mods, Streamer"))
                    {
                        level = "2";
                    }
                    else if (item3.Text.Equals("Streamer"))
                    {
                        level = "3";
                    }
                    writer.Write(item.Text + "\t" + item2.Text + "\t" + level + "\r");
                }
                writer.Close();
                readFromCommands();
            }
            catch (Exception)
            {
            }
        }

        public void writeToCurrency()
        {
            try
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter(currencyFile);
                for (int i = 0; i < CurrencyArray.Count; i++)
                {
                    var item = CurrencyArray[i].currencyUser as TextBlock;
                    var item2 = CurrencyArray[i].currencyAmount as TextBlock;
                    var item3 = CurrencyArray[i].currencyTime as TextBlock;
                    var item4 = CurrencyArray[i].currencySubCredits as TextBlock;
                    writer.Write(item.Text + "\t" + item2.Text + "\t" + item3.Text + "\t" + item4.Text + "\r");
                }
                writer.Close();
                readFromCurrency();
            }
            catch (Exception)
            {
            }
        }

        public void writeToRanks()
        {
            try
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter(ranksFile);
                for (int i = 0; i < RankArray.Count; i++)
                {
                    var item = RankArray[i].rankTitle as TextBlock;
                    var item2 = RankArray[i].rankCost as TextBlock;
                    writer.Write(item.Text + "\t" + item2.Text + "\r");
                }
                writer.Close();
                readFromRanks();
            }
            catch (Exception)
            {
            }
        }

        public void writeToQuotes()
        {
            try
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter(quotesFile);
                for (int i = 0; i < QuotesArray.Count; i++)
                {
                    var item = QuotesArray[i].quoteInfo as TextBlock;
                    writer.Write(item.Text + "\r");
                }
                writer.Close();
                readFromQuotes();
            }
            catch (Exception)
            {
            }
        }

        public void writeToTimers()
        {
            try
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter(timersFile);
                for (int i = 0; i < timerArray.Count; i++)
                {
                    var item = timerArray[i].timerName as TextBlock;
                    var item2 = timerArray[i].timerResponse as TextBlock;
                    var item3 = timerArray[i].timerToggle as TextBlock;
                    String toggle = "0";
                    if (item3.Text.Equals("ON"))
                    {
                        toggle = "1";
                    }
                    writer.Write(item.Text + "\t" + item2.Text + "\t" + toggle + "\r");
                }
                writer.Close();
                readFromTimers();
            }
            catch (Exception)
            {
            }
        }

        public async void removeSFX(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editCommandSFX.Text)))
            {
                for (int i = 0; i < SFXArray.Count; i++)
                {
                    var item = SFXArray[i].sfxCommand as TextBlock;
                    var item2 = SFXArray[i].sfxResponse as TextBlock;
                    if (item.Text.StartsWith(formatCommand(editCommandSFX.Text)))
                    {
                        SFXArray.Remove(SFXArray[i]);
                        break;
                    }
                }
                writeToSFX();
                editCommandSFX.Text = "";
                editResponseSFX.Text = "";
                syncUpdatesWithBot();
            }
            else
            {
                await this.ShowMessageAsync("Warning", "To delete or edit a SFX, click on it in the box and then press the desired button!");
            }
        }

        public async void removeImage(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editCommandImage.Text)))
            {
                for (int i = 0; i < ImagesArray.Count; i++)
                {
                    var item = ImagesArray[i].imageCommand as TextBlock;
                    var item2 = ImagesArray[i].imageResponse as TextBlock;
                    if (item.Text.StartsWith(formatCommand(editCommandImage.Text)))
                    {
                        ImagesArray.Remove(ImagesArray[i]);
                        break;
                    }
                }
                writeToImages();
                editCommandImage.Text = "";
                editResponseImage.Text = "";
                syncUpdatesWithBot();
            }
            else
            {
                await this.ShowMessageAsync("Warning", "To delete or edit an image, click on it in the box and then press the desired button!");
            }
        }

        public async void removeevent(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(editEventUser.Text))
            {
                for (int i = 0; i < EventsArray.Count; i++)
                {
                    var item = EventsArray[i].eventUser as TextBlock;
                    var item2 = EventsArray[i].eventMessage as TextBlock;
                    if (item.Text.StartsWith(editEventUser.Text))
                    {
                        EventsArray.Remove(EventsArray[i]);
                        break;
                    }
                }
                writeToEvents();
                editEventUser.Text = "";
                editEventMessage.Text = "";
                syncUpdatesWithBot();
            }
            else
            {
                await this.ShowMessageAsync("Warning", "To delete or edit an event, click on it in the box and then press the desired button!");
            }
        }

        public async void removecommand(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editCommand.Text)))
            {
                for (int i = 0; i < commandArray.Count; i++)
                {
                    var item = commandArray[i].commandName as TextBlock;
                    if (item.Text.StartsWith(formatCommand(editCommand.Text)))
                    {
                        commandArray.Remove(commandArray[i]);
                        break;
                    }
                }
                writeToCommands();
                editCommand.Text = "";
                editResponse.Text = "";
                editCommandLevel.SelectedIndex = 0;
                syncUpdatesWithBot();
            }
            else
            {
                await this.ShowMessageAsync("Warning", "To delete or edit a command, click on it in the box and then press the desired button!");
            }
        }

        public async void removecurrency(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editCurrencyUserName.Text)))
            {
                for (int i = 0; i < CurrencyArray.Count; i++)
                {
                    var item = CurrencyArray[i].currencyUser as TextBlock;
                    if (item.Text.StartsWith(editCurrencyUserName.Text))
                    {
                        CurrencyArray.Remove(CurrencyArray[i]);
                        break;
                    }
                }
                writeToCurrency();
                editCurrencyUserName.Text = "";
                editCurrencyAmount.Text = "";
                editCurrencyTime.Text = "";
                editCurrencySubCredits.Text = "";
            }
            else
            {
                await this.ShowMessageAsync("Warning", "To delete or edit a curreny entry, click on it in the box and then press the desired button!");
            }
        }

        public async void removerank(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editRankTitle.Text)))
            {
                for (int i = 0; i < RankArray.Count; i++)
                {
                    var item = RankArray[i].rankTitle as TextBlock;
                    if (item.Text.StartsWith(editRankTitle.Text))
                    {
                        RankArray.Remove(RankArray[i]);
                        break;
                    }
                }
                writeToRanks();
                editRankTitle.Text = "";
                editRankCost.Text = "";
            }
            else
            {
                await this.ShowMessageAsync("Warning", "To delete or edit a rank entry, click on it in the box and then press the desired button!");
            }
        }

        public async void removequote(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(quoteEdit.Text))
            {
                for (int i = 0; i < QuotesArray.Count; i++)
                {
                    var item = QuotesArray[i].quoteInfo as TextBlock;
                    if (item.Text.Equals(quoteEdit.Text))
                    {
                        QuotesArray.Remove(QuotesArray[i]);
                        break;
                    }
                }
                writeToQuotes();
                quoteEdit.Text = "";
                syncUpdatesWithBot();
            }
            else
            {
                await this.ShowMessageAsync("Warning", "To delete or edit a quote, click on it in the box and then press the desired button!");
            }
        }

        public async void removetimer(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editCommandTimed.Text)))
            {
                for (int i = 0; i < timerArray.Count; i++)
                {
                    var item = timerArray[i].timerName as TextBlock;
                    var item2 = timerArray[i].timerResponse as TextBlock;
                    if (item.Text.StartsWith(formatCommand(editCommandTimed.Text)))
                    {
                        timerArray.Remove(timerArray[i]);
                        break;
                    }
                }
                writeToTimers();
                editCommandTimed.Text = "";
                editResponseTimed.Text = "";
                editToggleTimed.IsChecked = false;
                syncUpdatesWithBot();
            }
            else
            {
                await this.ShowMessageAsync("Warning", "To delete or edit a command, click on it in the box and then press the desired button!");
            }
        }

        public async void addSFX(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editCommandSFX.Text)) && !string.IsNullOrWhiteSpace(editResponseSFX.Text))
            {
                TextBlock tb = new TextBlock();
                tb.Text = formatCommand(editCommandSFX.Text);
                TextBlock tb2 = new TextBlock();
                tb2.Text = editResponseSFX.Text;
                for (int i = 0; i < SFXArray.Count; i++)
                {
                    var item = SFXArray[i].sfxCommand as TextBlock;
                    var item2 = SFXArray[i].sfxResponse as TextBlock;
                    if (item.Text.StartsWith(formatCommand(editCommandSFX.Text)))
                    {
                        SFXArray.Remove(SFXArray[i]);
                        break;
                    }
                }
                SFXArray.Add(new SFX(tb, tb2));
                writeToSFX();
                editCommandSFX.Text = "";
                editResponseSFX.Text = "";
                syncUpdatesWithBot();
            }
            else
            {
                await this.ShowMessageAsync("Warning", "Please enter the command name and upload an sfx file!");
            }
        }

        public async void addImage(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editCommandImage.Text)) && !string.IsNullOrWhiteSpace(editResponseImage.Text))
            {
                TextBlock tb = new TextBlock();
                tb.Text = formatCommand(editCommandImage.Text);
                TextBlock tb2 = new TextBlock();
                tb2.Text = editResponseImage.Text;
                for (int i = 0; i < ImagesArray.Count; i++)
                {
                    var item = ImagesArray[i].imageCommand as TextBlock;
                    var item2 = ImagesArray[i].imageResponse as TextBlock;
                    if (item.Text.StartsWith(formatCommand(editCommandImage.Text)))
                    {
                        ImagesArray.Remove(ImagesArray[i]);
                        break;
                    }
                }
                ImagesArray.Add(new Images(tb, tb2));
                writeToImages();
                editCommandImage.Text = "";
                editResponseImage.Text = "";
                syncUpdatesWithBot();
            }
            else
            {
                await this.ShowMessageAsync("Warning", "Please enter the command name and upload an image file!");
            }
        }

        public async void addevent(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(editEventUser.Text) && !string.IsNullOrWhiteSpace(editEventMessage.Text))
            {
                TextBlock tb = new TextBlock();
                tb.Text = editEventUser.Text;
                TextBlock tb2 = new TextBlock();
                tb2.Text = editEventMessage.Text;
                for (int i = 0; i < EventsArray.Count; i++)
                {
                    var item = EventsArray[i].eventUser as TextBlock;
                    var item2 = EventsArray[i].eventMessage as TextBlock;
                    if (item.Text.StartsWith(editEventUser.Text))
                    {
                        EventsArray.Remove(EventsArray[i]);
                        break;
                    }
                }
                EventsArray.Add(new Events(tb, tb2));
                writeToEvents();
                editEventUser.Text = "";
                editEventMessage.Text = "";
                syncUpdatesWithBot();
            }
            else
            {
                await this.ShowMessageAsync("Warning", "Please enter the username and message!");
            }
        }

        public async void addcommand(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editCommand.Text)) && !string.IsNullOrWhiteSpace(editResponse.Text))
            {
                TextBlock tb = new TextBlock();
                tb.Text = formatCommand(editCommand.Text);
                TextBlock tb2 = new TextBlock();
                tb2.Text = editResponse.Text;
                TextBlock tb3 = new TextBlock();
                if (editCommandLevel.SelectedIndex.ToString().Equals("0"))
                {
                    tb3.Text = "Everyone";
                }
                else if (editCommandLevel.SelectedIndex.ToString().Equals("1"))
                {
                    tb3.Text = "Subs, Mods, Streamer";
                }
                else if (editCommandLevel.SelectedIndex.ToString().Equals("2"))
                {
                    tb3.Text = "Mods, Streamer";
                }
                else if (editCommandLevel.SelectedIndex.ToString().Equals("3"))
                {
                    tb3.Text = "Streamer";
                }
                
                for (int i = 0; i < commandArray.Count; i++)
                {
                    var item = commandArray[i].commandName as TextBlock;
                    var item2 = commandArray[i].commandResponse as TextBlock;
                    var item3 = commandArray[i].commandLevel as TextBlock;
                    if (item.Text.StartsWith(formatCommand(editCommand.Text)))
                    {
                        commandArray.Remove(commandArray[i]);
                        break;
                    }
                }
                commandArray.Add(new Commands(tb, tb2, tb3));
                writeToCommands();
                editCommand.Text = "";
                editResponse.Text = "";
                editCommandLevel.SelectedIndex = 0;
                syncUpdatesWithBot();
            }
            else
            {
                await this.ShowMessageAsync("Warning", "Please enter the command name and response!");
            }
        }

        public async void addcurrency(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editCurrencyUserName.Text)) && !string.IsNullOrWhiteSpace(editCurrencyAmount.Text) && !string.IsNullOrWhiteSpace(editCurrencyTime.Text) && !string.IsNullOrWhiteSpace(editCurrencySubCredits.Text))
            {
                TextBlock tb = new TextBlock();
                tb.Text = editCurrencyUserName.Text;
                TextBlock tb2 = new TextBlock();
                tb2.Text = editCurrencyAmount.Text;
                TextBlock tb3 = new TextBlock();
                tb3.Text = editCurrencyTime.Text;
                TextBlock tb4 = new TextBlock();
                tb4.Text = editCurrencySubCredits.Text;
                for (int i = 0; i < CurrencyArray.Count; i++)
                {
                    var item = CurrencyArray[i].currencyUser as TextBlock;
                    var item2 = CurrencyArray[i].currencyAmount as TextBlock;
                    var item3 = CurrencyArray[i].currencyTime as TextBlock;
                    var item4 = CurrencyArray[i].currencySubCredits as TextBlock;
                    if (item.Text.StartsWith(editCurrencyUserName.Text))
                    {
                        CurrencyArray.Remove(CurrencyArray[i]);
                        break;
                    }
                }
                CurrencyArray.Add(new Currency(tb, tb2, tb3, tb4));
                writeToCurrency();
                editCurrencyUserName.Text = "";
                editCurrencyAmount.Text = "";
                editCurrencyTime.Text = "";
                editCurrencySubCredits.Text = "";
            }
            else
            {
                await this.ShowMessageAsync("Warning", "Please enter the user name, amount, and time in minutes!");
            }
        }

        public async void addrank(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editRankTitle.Text)) && !string.IsNullOrWhiteSpace(editRankCost.Text))
            {
                TextBlock tb = new TextBlock();
                tb.Text = editRankTitle.Text;
                TextBlock tb2 = new TextBlock();
                tb2.Text = editRankCost.Text;
                for (int i = 0; i < RankArray.Count; i++)
                {
                    var item = RankArray[i].rankTitle as TextBlock;
                    var item2 = RankArray[i].rankCost as TextBlock;
                    if (item.Text.StartsWith(editRankTitle.Text))
                    {
                        RankArray.Remove(RankArray[i]);
                        break;
                    }
                }
                RankArray.Add(new Ranks(tb, tb2));
                writeToRanks();
                editRankTitle.Text = "";
                editRankCost.Text = "";
            }
            else
            {
                await this.ShowMessageAsync("Warning", "Please enter the rank title and cost!");
            }
        }

        public ArrayList LoadArrayList()
        {
            System.IO.StreamReader reader = new System.IO.StreamReader(songsFile);
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
                File.WriteAllLines(songsFile, text.Cast<String>());
                refresh.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                editSong.Text = "";
                editRequester.Text = "";
                songplace.Value = 1;
                syncUpdatesWithBot();
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
                    string[] text = File.ReadAllLines(songsFile);
                    if (text[0] == "")
                    {
                        await this.ShowMessageAsync("Warning", "Place # " + songplace.Value + " does not exist!");
                        return;
                    }
                    String temp = text[(Int32) songplace.Value - 1];
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
                    File.WriteAllLines(songsFile, text);
                    refresh.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    editSong.Text = "";
                    editRequester.Text = "";
                    songplace.Value = 1;
                    syncUpdatesWithBot();
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

        public async void addquote(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(quoteEdit.Text))
            {
                TextBlock tb = new TextBlock();
                tb.Text = quoteEdit.Text;
                for (int i = 0; i < QuotesArray.Count; i++)
                {
                    var item = QuotesArray[i].quoteInfo as TextBlock;
                    if (quoteID.ToString() == item.Uid)
                    {
                        QuotesArray.Remove(QuotesArray[i]);
                        break;
                    }
                }
                QuotesArray.Add(new Quotes(tb));
                writeToQuotes();
                readFromQuotes();
                quoteEdit.Text = "";
                syncUpdatesWithBot();
            }
            else
            {
                await this.ShowMessageAsync("Warning", "Please enter the quote!");
            }
        }

        public async void addtimer(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editCommandTimed.Text)) && !string.IsNullOrWhiteSpace(editResponseTimed.Text))
            {
                TextBlock tb = new TextBlock();
                tb.Text = formatCommand(editCommandTimed.Text);
                TextBlock tb2 = new TextBlock();
                tb2.Text = editResponseTimed.Text;
                TextBlock tb3 = new TextBlock();
                if (editToggleTimed.IsChecked == true)
                {
                    tb3.Text = "ON";
                }
                else
                {
                    tb3.Text = "OFF";
                }
                for (int i = 0; i < timerArray.Count; i++)
                {
                    var item = timerArray[i].timerName as TextBlock;
                    var item2 = timerArray[i].timerResponse as TextBlock;
                    if (item.Text.StartsWith(formatCommand(editCommandTimed.Text)))
                    {
                        timerArray.Remove(timerArray[i]);
                        break;
                    }
                }
                timerArray.Add(new Timers(tb, tb2, tb3));
                writeToTimers();
                editCommandTimed.Text = "";
                editResponseTimed.Text = "";
                editToggleTimed.IsChecked = false;
                syncUpdatesWithBot();
            }
            else
            {
                await this.ShowMessageAsync("Warning", "Please enter the timed command name and response!");
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
                if (File.Exists(backupcommandsfile))
                {
                    File.Delete(backupcommandsfile);
                }
                if (File.Exists(backupbotfile))
                {
                    File.Delete(backupbotfile);
                }
                if (File.Exists(backuptimersfile))
                {
                    File.Delete(backupbotfile);
                }
                if (File.Exists(backupothersfile))
                {
                    File.Delete(backupothersfile);
                }
                if (File.Exists(backupsfxfile))
                {
                    File.Delete(backupsfxfile);
                }
                if (File.Exists(backupquotesfile))
                {
                    File.Delete(backupquotesfile);
                }
                if (File.Exists(backupguifile))
                {
                    File.Delete(backupguifile);
                }
                if (File.Exists(backupusersfile))
                {
                    File.Delete(backupusersfile);
                }
                if (File.Exists(backupeventsfile))
                {
                    File.Delete(backupeventsfile);
                }
                if (File.Exists(backupcurrencyfile))
                {
                    File.Delete(backupcurrencyfile);
                }
                if (File.Exists(backupimagesfile))
                {
                    File.Delete(backupimagesfile);
                }
                if (File.Exists(backupranksfile))
                {
                    File.Delete(backupranksfile);
                }
                if (File.Exists(backuppurchasedranksfile))
                {
                    File.Delete(backuppurchasedranksfile);
                }
                System.IO.File.WriteAllText(commandsFile, string.Empty);
                System.IO.File.WriteAllText(timersFile, string.Empty);
                System.IO.File.WriteAllText(quotesFile, string.Empty);
                System.IO.File.WriteAllText(othersFile, string.Empty);
                System.IO.File.WriteAllText(usersFile, string.Empty);
                System.IO.File.WriteAllText(sfxFile, string.Empty);
                System.IO.File.WriteAllText(eventsFile, string.Empty);
                System.IO.File.WriteAllText(currencyFile, string.Empty);
                System.IO.File.WriteAllText(imagesFile, string.Empty);
                System.IO.File.WriteAllText(ranksFile, string.Empty);
                System.IO.File.WriteAllText(purchasedranksFile, string.Empty);
                // Set variable presets (config.txt)
                preset();
                streamerName.Text = "";
                botName.Text = "";
                oauth.Text = "";
                googleSheetID.Text = "";
                writeToConfig(sender, e);
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
                writeToCurrency();
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