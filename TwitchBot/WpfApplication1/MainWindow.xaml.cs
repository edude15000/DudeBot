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
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Navigation;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace WpfApplication1
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public static TwitchBot bot;
        public String dudebotdirectory = Path.GetTempPath() + "dudebotdirectory.txt";
        public String dudebotupdateinfo = Path.GetTempPath() + "dudebotupdateinfo.txt";
        private void OnPropertyChanged(string propertyName = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler == null) return;
            handler(this, new PropertyChangedEventArgs(propertyName));
        }

        
        // TODO : Backup JSON file!

        String selectedQuote = "";
        Process imageWindow = null;
        Process rockSnifferWindow = null;
        [DllImport("User32")]
        private static extern int SetForegroundWindow(IntPtr hwnd);
        [DllImportAttribute("User32.DLL")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private LowLevelKeyboardListener _listener;
        
        void _listener_OnKeyPressed(object sender, KeyPressedArgs e)
        {
            foreach (Command c in bot.hotkeyCommandList) {
                if (e.KeyPressed.ToString().Equals(c.input[0]))
                {
                    try
                    {
                        if (c.output.Equals("!next", StringComparison.InvariantCultureIgnoreCase))
                        {
                            bot.requestSystem.nextSongAuto(bot.channel, false);
                        }
                        else
                        {
                            bot.processMessage(bot.streamer, c.output);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        public async void addhotkey(Object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editHotKeyInput.Text)) && !string.IsNullOrWhiteSpace(editHotKeyCommand.Text))
            {
                foreach (Command c in bot.hotkeyCommandList)
                {
                    if (c.input[0].Equals(editHotKeyInput.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        c.output = editHotKeyCommand.Text;
                        bot.resetAllCommands();
                        editHotKeyInput.Text = "";
                        editHotKeyCommand.Text = "";
                        return;
                    }
                }
                String[] str = { editHotKeyInput.Text };
                bot.commandList.Add(new Command(str, 3, editHotKeyCommand.Text, "hotkey", true));
                bot.resetAllCommands();
                editHotKeyInput.Text = "";
                editHotKeyCommand.Text = "";
            }
            else
            {
                await this.ShowMessageAsync("Warning", "Please enter the hotkey and message/command to send!");
            }
        }

        public async void removehotkey(Object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editHotKeyInput.Text)) && !string.IsNullOrWhiteSpace(editHotKeyCommand.Text))
            {
                foreach (Command c in bot.userCommandList)
                {
                    if (c.input[0].Equals(editHotKeyInput.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        bot.commandList.Remove(c);
                        break;
                    }
                }
                bot.resetAllCommands();
                editHotKeyInput.Text = "";
                editHotKeyCommand.Text = "";
            }
            else
            {
                await this.ShowMessageAsync("Warning", "Please enter the hotkey and message/command to send!");
            }
        }

        private void editHotKeyInput_KeyDown(object sender, KeyEventArgs e)
        {
            editHotKeyInput.Text = "";
            editHotKeyInput.Text = e.Key.ToString();
        }

        private void editHotKeyButtonClick(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (sender as TextBlock);
            object datacontext = textBlock.DataContext;
            Command c = (Command)datacontext;
            editHotKeyInput.Text = c.input[0];
            editHotKeyCommand.Text = c.output;
        }


        public async void addcommand(Object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editCommand.Text)) && !string.IsNullOrWhiteSpace(editResponse.Text))
            {
                if (editCommandLevel.SelectedIndex < 0)
                {
                    editCommandLevel.SelectedIndex = 0;
                }
                foreach (Command c in bot.userCommandList)
                {
                    if (c.input[0].Equals(editCommand.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        c.output = editResponse.Text;
                        bot.resetAllCommands();
                        editCommand.Text = "";
                        editResponse.Text = "";
                        return;
                    }
                }
                String[] str = { editCommand.Text };
                bot.commandList.Add(new Command(str, editCommandLevel.SelectedIndex, editResponse.Text, "user", true));
                bot.resetAllCommands();
                editCommand.Text = "";
                editResponse.Text = "";
            }
            else
            {
                await this.ShowMessageAsync("Warning", "Please enter the command name and response!");
            }
        }

        public async void addcurrency(Object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editCurrencyUserName.Text)) && !string.IsNullOrWhiteSpace(editCurrencyAmount.Text) && !string.IsNullOrWhiteSpace(editCurrencyTime.Text) && !string.IsNullOrWhiteSpace(editCurrencySubCredits.Text))
            {
                foreach (BotUser b in bot.users)
                {
                    if (b.username.Equals(editCurrencyUserName.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        b.points = Int32.Parse(editCurrencyAmount.Text);
                        b.time = Int32.Parse(editCurrencyTime.Text);
                        b.subCredits = Int32.Parse(editCurrencySubCredits.Text);
                        writeToConfig(null, null);
                        editCurrencyAmount.Text = "";
                        editCurrencyTime.Text = "";
                        editCurrencySubCredits.Text = "";
                        editCurrencyUserName.Text = "";
                        return;
                    }
                }
                bot.users.Add(new BotUser(editCurrencyUserName.Text, 0, false, false, false, Int32.Parse(editCurrencyAmount.Text), Int32.Parse(editCurrencyTime.Text), null, 0, 0, Int32.Parse(editCurrencySubCredits.Text)));
                writeToConfig(null, null);
                editCurrencyAmount.Text = "";
                editCurrencyTime.Text = "";
                editCurrencySubCredits.Text = "";
                editCurrencyUserName.Text = "";
            }
            else
            {
                await this.ShowMessageAsync("Warning", "Please enter the user name, amount, and time in minutes!");
            }
        }

        public async void addevent(Object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(editEventUser.Text) && !string.IsNullOrWhiteSpace(editEventMessage.Text))
            {
                foreach (KeyValuePair<String, String> s in bot.events)
                {
                    if (s.Key.Equals(editEventUser.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        bot.events[s.Key] = editEventMessage.Text;
                        writeToConfig(null, null);
                        editEventUser.Text = "";
                        editEventMessage.Text = "";
                        return;
                    }
                }
                bot.events.Add(editEventUser.Text, editEventMessage.Text);
                writeToConfig(null, null);
                editEventUser.Text = "";
                editEventMessage.Text = "";
            }
            else
            {
                await this.ShowMessageAsync("Warning", "Please enter user and event response!");
            }
        }

        public async void addImage(Object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editCommandImage.Text)) && !string.IsNullOrWhiteSpace(editResponseImage.Text))
            {
                foreach (Command c in bot.imageCommandList)
                {
                    if (c.input[0].Equals(editCommandImage.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        c.output = editResponseImage.Text;
                        bot.resetAllCommands();
                        editCommandImage.Text = "";
                        editResponseImage.Text = "";
                        return;
                    }
                }
                String[] str = { editCommandImage.Text };
                bot.commandList.Add(new Command(str, 0, editResponseImage.Text, "image", true));
                bot.resetAllCommands();
                editCommandImage.Text = "";
                editResponseImage.Text = "";
            }
            else
            {
                await this.ShowMessageAsync("Warning", "Please enter the command name and upload an image file!");
            }
        }

        public async void addquote(Object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(quoteEdit.Text))
            {
                for (int i = 0; i < bot.quote.quotes.Count; i++)
                {
                    if (bot.quote.quotes[i].Equals(selectedQuote))
                    {
                        bot.quote.quotes[i] = quoteEdit.Text;
                        writeToConfig(null, null);
                        quoteEdit.Text = "";
                        selectedQuote = "";
                        return;
                    }
                }
            }
            else
            {
                await this.ShowMessageAsync("Warning", "Please enter the quote!");
            }
        }

        public async void addrank(Object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editRankTitle.Text)) && !string.IsNullOrWhiteSpace(editRankCost.Text))
            {
                foreach (KeyValuePair<String, Int32> s in bot.currency.ranks)
                {
                    if (s.Key.Equals(editRankTitle.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        bot.currency.ranks[s.Key] = Int32.Parse(editRankCost.Text);
                        bot.resetAllCommands();
                        editRankTitle.Text = "";
                        editRankCost.Text = "";
                        return;
                    }
                }
                bot.currency.ranks.Add(editRankTitle.Text, Int32.Parse(editRankCost.Text));
                bot.resetAllCommands();
                editRankTitle.Text = "";
                editRankCost.Text = "";
            }
            else
            {
                await this.ShowMessageAsync("Warning", "Please enter the rank title and cost!");
            }
        }

        public async void addSFX(Object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editCommandSFX.Text)) && !string.IsNullOrWhiteSpace(editResponseSFX.Text))
            {
                foreach (Command c in bot.sfxCommandList)
                {
                    if (c.input[0].Equals(editCommandSFX.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        c.output = editResponseSFX.Text;
                        c.volumeLevel = (int)volumeSetter.Value;
                        bot.resetAllCommands();
                        editCommandSFX.Text = "";
                        editResponseSFX.Text = "";
                        volumeSetter.Value = 100;
                        return;
                    }
                }
                String[] str = { editCommandSFX.Text };
                Command d = new Command(str, 0, editResponseSFX.Text, "sfx", true);
                d.volumeLevel = (int)volumeSetter.Value;
                bot.commandList.Add(d);
                bot.resetAllCommands();
                editCommandSFX.Text = "";
                editResponseSFX.Text = "";
                volumeSetter.Value = 100;
            }
            else
            {
                await this.ShowMessageAsync("Warning", "Please enter the command name and upload an image file!");
            }
        }

        public async void addtimer(Object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editCommandTimed.Text)) && !string.IsNullOrWhiteSpace(editResponseTimed.Text))
            {
                foreach (Command c in bot.timerCommandList)
                {
                    if (c.input[0].Equals(editCommandTimed.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        c.output = editResponseTimed.Text;
                        bot.resetAllCommands();
                        editCommandTimed.Text = "";
                        editResponseTimed.Text = "";
                        editToggleTimed.IsChecked = false;
                        return;
                    }
                }
                String[] str = { editCommandTimed.Text };
                bot.commandList.Add(new Command(str, 0, editResponseTimed.Text, "timer", true));
                bot.resetAllCommands();
                editCommandTimed.Text = "";
                editResponseTimed.Text = "";
                editToggleTimed.IsChecked = false;
            }
            else
            {
                await this.ShowMessageAsync("Warning", "Please enter the timed command name and response!");
            }
        }

        public async void removecommand(Object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editCommand.Text)))
            {
                foreach (Command c in bot.userCommandList)
                {
                    if (c.input[0].Equals(editCommand.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        bot.commandList.Remove(c);
                        break;
                    }
                }
                editCommand.Text = "";
                editResponse.Text = "";
                editCommandLevel.SelectedIndex = 0;
                bot.resetAllCommands();
            }
            else
            {
                await this.ShowMessageAsync("Warning", "To delete or edit a command, click on it in the box and then press the desired button!");
            }
        }

        public async void removecurrency(Object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editCurrencyUserName.Text)))
            {
                foreach (BotUser b in bot.users)
                {
                    if (b.username.Equals(editCurrencyUserName.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        bot.users.Remove(b);
                        break;
                    }
                }
                editCurrencyUserName.Text = "";
                editCurrencyAmount.Text = "";
                editCurrencyTime.Text = "";
                editCurrencySubCredits.Text = "";
                writeToConfig(null, null);
            }
            else
            {
                await this.ShowMessageAsync("Warning", "To delete or edit a curreny entry, click on it in the box and then press the desired button!");
            }
        }

        public async void removeevent(Object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(editEventUser.Text))
            {
                foreach (KeyValuePair<String, String> b in bot.events)
                {
                    if (b.Key.Equals(editEventUser.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        bot.events.Remove(b.Key);
                        break;
                    }
                }
                editEventUser.Text = "";
                editEventMessage.Text = "";
                writeToConfig(null, null);
            }
            else
            {
                await this.ShowMessageAsync("Warning", "To delete or edit an event, click on it in the box and then press the desired button!");
            }
        }

        public async void removeImage(Object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editCommandImage.Text)))
            {
                foreach (Command c in bot.imageCommandList)
                {
                    if (c.input[0].Equals(editCommandImage.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        bot.commandList.Remove(c);
                        break;
                    }
                }
                editCommandImage.Text = "";
                editResponseImage.Text = "";
                writeToConfig(null, null);
                bot.resetAllCommands();
            }
            else
            {
                await this.ShowMessageAsync("Warning", "To delete or edit an image, click on it in the box and then press the desired button!");
            }
        }

        public async void removequote(Object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(quoteEdit.Text))
            {
                foreach (String b in bot.quote.quotes)
                {
                    if (b.Equals(quoteEdit.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        bot.quote.quotes.Remove(b);
                        break;
                    }
                }
                quoteEdit.Text = "";
                writeToConfig(null, null);
            }
            else
            {
                await this.ShowMessageAsync("Warning", "To delete or edit a quote, click on it in the box and then press the desired button!");
            }
        }

        public async void removerank(Object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editRankTitle.Text)))
            {
                foreach (KeyValuePair < String, Int32 > b in bot.currency.ranks)
                {
                    if (b.Key.Equals(editCurrencyUserName.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        bot.currency.ranks.Remove(b.Key);
                        break;
                    }
                }
                editRankTitle.Text = "";
                editRankCost.Text = "";
                writeToConfig(null, null);
            }
            else
            {
                await this.ShowMessageAsync("Warning", "To delete or edit a rank entry, click on it in the box and then press the desired button!");
            }
        }

        public async void removeSFX(Object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editCommandSFX.Text)))
            {
                foreach (Command c in bot.sfxCommandList)
                {
                    if (c.input[0].Equals(editCommandSFX.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        bot.commandList.Remove(c);
                        break;
                    }
                }
                editCommandSFX.Text = "";
                editResponseSFX.Text = "";
                volumeSetter.Value = 100;
                writeToConfig(null, null);
                bot.resetAllCommands();
            }
            else
            {
                await this.ShowMessageAsync("Warning", "To delete or edit a SFX, click on it in the box and then press the desired button!");
            }
        }

        public async void removetimer(Object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editCommandTimed.Text)))
            {
                foreach (Command c in bot.timerCommandList)
                {
                    if (c.input[0].Equals(editCommandTimed.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        bot.commandList.Remove(c);
                        break;
                    }
                }
                editCommandTimed.Text = "";
                editResponseTimed.Text = "";
                editToggleTimed.IsChecked = false;
                writeToConfig(null, null);
                bot.resetAllCommands();
            }
            else
            {
                await this.ShowMessageAsync("Warning", "To delete or edit a command, click on it in the box and then press the desired button!");
            }
        }

        public void openRockSniffer(Object sender, RoutedEventArgs e)
        {
            if (rockSnifferWindow != null)
            {
                try
                {
                    rockSnifferWindow.Kill();
                    rockSnifferWindow = null;
                }
                catch (Exception)
                {
                }
            }
            try
            {
                rockSnifferWindow = new Process();
                rockSnifferWindow.StartInfo.FileName = "bin\\RockSniffer.exe";
                rockSnifferWindow.StartInfo.WorkingDirectory = "bin\\";
                rockSnifferWindow.StartInfo.RedirectStandardOutput = true;
                rockSnifferWindow.StartInfo.UseShellExecute = false;
                rockSnifferWindow.StartInfo.CreateNoWindow = true;
                rockSnifferWindow.Start();
            }
            catch (Exception e1)
            {
                Utils.errorReport(e1);
                Console.WriteLine(e1.ToString());
            }
        }

        public void writeToConfig(Object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                refreshAllDataGrids();
                Utils.saveData(bot);
            });
        }

        public void refreshAllDataGrids()
        {
            foreach (DataGrid d in FindLogicalChildren<DataGrid>(App.Current.MainWindow))
            {
                d.Items.Refresh();
            }
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
            if (openRockSnifferOnStartUp.IsChecked == true)
            {
                openRockSniffer(null, null);
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
        }
        
        public void getReady()
        {
            refresh.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
        
        private async void loaded(Object sender, RoutedEventArgs e)
        {
            bot = new TwitchBot(App.window);
            if (!checkPrereqs())
            {
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
            File.WriteAllText(dudebotupdateinfo, string.Empty);
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
                        customization.Focus();
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
                MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Accented;
                var msgbox_settings = new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" };
                MessageDialogResult messageBoxResult = await this.ShowMessageAsync("Update Available!", "An update was found, would you like to update?", MessageDialogStyle.AffirmativeAndNegative, msgbox_settings);
                if (messageBoxResult == MessageDialogResult.Affirmative)
                {
                    try
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.FileName = Path.GetTempPath() + @"\dudebotupdater.exe";
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

        public String getFollowingText(String line)
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
        
        public async void clearlist(Object sender, RoutedEventArgs e)
        {
            this.MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Accented;
            var msgbox_settings = new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" };
            MessageDialogResult messageBoxResult = await this.ShowMessageAsync("Clear List?", "Are you sure you want to clear the current request queue?", MessageDialogStyle.AffirmativeAndNegative, msgbox_settings);
            if (messageBoxResult == MessageDialogResult.Affirmative)
            {
                Thread.Sleep(1000);
                bot.requestSystem.clearCOMMAND(bot.requestSystem.clearComm.input[0], bot.channel, bot.streamer);
            }
        }
        
        public void sortByUserName(Object sender, RoutedEventArgs e)
        {
            bot.users = new ObservableCollection<BotUser>(bot.users.OrderBy(o => o.username).ToList());
            writeToConfig(null, null);
        }

        public void sortByRankTitle(Object sender, RoutedEventArgs e)
        {
            bot.users = new ObservableCollection<BotUser>(bot.users.OrderBy(o => o.rank).ToList());
            writeToConfig(null, null);
        }

        public void sortByAmount(Object sender, RoutedEventArgs e)
        {
            bot.users = new ObservableCollection<BotUser>(bot.users.OrderBy(o => o.points).ToList());
            writeToConfig(null, null);
        }

        public void sortByMinutes(Object sender, RoutedEventArgs e)
        {
            bot.users = new ObservableCollection<BotUser>(bot.users.OrderBy(o => o.time).ToList());
            writeToConfig(null, null);
        }

        public void sortBySubCredits(Object sender, RoutedEventArgs e)
        {
            bot.users = new ObservableCollection<BotUser>(bot.users.OrderBy(o => o.subCredits).ToList());
            writeToConfig(null, null);
        }

        private void moveup(Object sender, RoutedEventArgs e)
        {
            TextBlock textBlock = (sender as TextBlock);
            object datacontext = textBlock.DataContext; // TODO : FIX null object
            Song c = (Song)datacontext;
            for (int i = 0; i < bot.requestSystem.songList.Count; i++)
            {
                if (bot.requestSystem.songList[i].name.Equals(c.name, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (i == 0)
                    {
                        return;
                    }
                    Song a = bot.requestSystem.songList[i];
                    bot.requestSystem.songList.Remove(a);
                    bot.requestSystem.insertSong(a.name, a.requester, i-1);
                    break;
                }
            }
            writeToConfig(null, null);
        }

        private void deleteSong(Object sender, RoutedEventArgs e)
        {
            TextBlock textBlock = (sender as TextBlock);
            object datacontext = textBlock.DataContext; // TODO : FIX null object
            Song c = (Song)datacontext;
            foreach (Song s in bot.requestSystem.songList)
            {
                if (s.name.Equals(c.name, StringComparison.InvariantCultureIgnoreCase))
                {
                    bot.requestSystem.songList.Remove(s);
                    break;
                }
            }
            writeToConfig(null, null);
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
                bot.requestSystem.insertSong(editSong.Text, uName, (int)songplace.Value);
                editSong.Text = "";
                editRequester.Text = "";
                songplace.Value = 1;
                writeToConfig(null, null);
            }
            else
            {
                await this.ShowMessageAsync("Warning", "Please enter the song name!");
            }
        }

        public async void editsong(object sender, RoutedEventArgs e)
        {
            String uName = "";
            if (bot.requestSystem.songList.Count <= songplace.Value)
            {
                await this.ShowMessageAsync("Warning", "Place # " + songplace.Value + " does not exist!");
                return;
            }
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
                Song oldsong = bot.requestSystem.songList[(int)songplace.Value];
                Song newsong = new Song(uName, editRequester.Text, oldsong.level, bot);
                bot.requestSystem.songList[(int)songplace.Value] = newsong;
                editSong.Text = "";
                editRequester.Text = "";
                songplace.Value = 1;
                writeToConfig(null, null);
            }
            else
            {
                await this.ShowMessageAsync("Warning", "Please enter the song name!");
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
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
            try
            {
                rockSnifferWindow.Kill();
            }
            catch (Exception)
            {
            }
            if (bot != null)
            { 
                killBot(null, null);
            }
            // TODO : BACK UP JSON
            Process.GetCurrentProcess().CloseMainWindow();
        }
        
        private void editCommandButtonClick(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (sender as TextBlock);
            object datacontext = textBlock.DataContext;
            Command c = (Command)datacontext;
            editCommand.Text = c.input[0];
            editResponse.Text = c.output;
            editCommandLevel.SelectedIndex = c.level;
        }

        private void editTimedCommandButtonClick(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (sender as TextBlock);
            object datacontext = textBlock.DataContext;
            Command c = (Command)datacontext;
            editCommandTimed.Text = c.input[0];
            editResponseTimed.Text = c.output;
            editToggleTimed.IsChecked = c.toggle;
        }

        private void editCurrencyButtonClick(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (sender as TextBlock);
            object datacontext = textBlock.DataContext;
            BotUser c = (BotUser)datacontext;
            editCurrencyUserName.Text = c.username;
            editCurrencyAmount.Text = c.points.ToString();
            editCurrencyTime.Text = c.time.ToString();
            editCurrencySubCredits.Text = c.subCredits.ToString();
        }

        private void editRankButtonClick(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (sender as TextBlock);
            object datacontext = textBlock.DataContext;
            KeyValuePair<String, Int32> c = (KeyValuePair<String, Int32>)datacontext;
            editRankTitle.Text = c.Key;
            editRankCost.Text = c.Value.ToString();
        }

        private void editQuteButtonClick(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (sender as TextBlock);
            object datacontext = textBlock.DataContext;
            quoteEdit.Text = (String)datacontext;
            selectedQuote = quoteEdit.Text;
        }

        private void editSFXButtonClick(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (sender as TextBlock);
            object datacontext = textBlock.DataContext;
            Command c = (Command)datacontext;
            editCommandSFX.Text = c.input[0];
            editResponseSFX.Text = c.output;
            volumeSetter.Value = c.volumeLevel;
        }

        private void editImageButtonClick(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (sender as TextBlock);
            object datacontext = textBlock.DataContext;
            Command c = (Command)datacontext;
            editCommandImage.Text = c.input[0];
            editResponseImage.Text = c.output;
        }

        private void editEventsButtonClick(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (sender as TextBlock);
            object datacontext = textBlock.DataContext;
            KeyValuePair<String, String> c = (KeyValuePair<String, String>)datacontext;
            editEventUser.Text = c.Key;
            editEventMessage.Text = c.Value;
        }

        private void editSongButtonClick(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (sender as TextBlock);
            object datacontext = textBlock.DataContext;
            Song c = (Song)datacontext;
            int index = bot.requestSystem.songList.Count;
            for (int i = 0; i < bot.requestSystem.songList.Count; i++)
            {
                if (bot.requestSystem.songList[i].name.Equals(c.name, StringComparison.InvariantCultureIgnoreCase))
                {
                    index = i;
                }
            }
            songplace.Value = index;
            editSong.Text = c.name;
            editRequester.Text = c.requester;
            if (e.ClickCount == 2)
            {
                String link = "https://www.youtube.com/results?search_query=" + c.name;
                if (c.youtubeLink != null && c.youtubeLink != "")
                {
                    link = c.youtubeLink;
                }
                var backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += (s, e1) =>
                {
                    Thread.Sleep(250);
                };
                Process.Start(link);
            }
        }

        private void editRequesterButtonClick(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (sender as TextBlock);
            object datacontext = textBlock.DataContext;
            Song c = (Song)datacontext;
            int index = bot.requestSystem.songList.Count;
            for (int i = 0; i < bot.requestSystem.songList.Count; i++)
            {
                if (bot.requestSystem.songList[i].name.Equals(c.name, StringComparison.InvariantCultureIgnoreCase))
                {
                    index = i;
                }
            }
            songplace.Value = index;
            editSong.Text = c.name;
            editRequester.Text = c.requester;
            if (e.ClickCount == 2)
            {
                String link = "https://www.twitch.tv/" + c.requester;
                var backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += (s, e1) =>
                {
                    Thread.Sleep(250);
                };
                Process.Start(link);
            }
        }
        
        public void uploadSFX(object sender, EventArgs e)
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
        
        public void uploadImage(object sender, EventArgs e)
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
            MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Accented;
            var msgbox_settings = new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" };
            MessageDialogResult messageBoxResult = await this.ShowMessageAsync("Reset Confirmation", "Are you sure you want to reset DudeBot? All user data will be deleted, but this will fix any problems with DudeBot.", MessageDialogStyle.AffirmativeAndNegative, msgbox_settings);
            if (messageBoxResult == MessageDialogResult.Affirmative)
            {
                File.WriteAllText(Utils.userDataFile, string.Empty);
                writeToConfig(null, null);
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
            Process.Start(link);
        }

        private void nightlikethistwitter(object sender, RoutedEventArgs e)
        {
            String link = "https://twitter.com/NLTBandOfficial/";
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += (s, e1) =>
            {
                Thread.Sleep(250);
            };
            Process.Start(link);
        }

        private void nightlikethisitunes(object sender, RoutedEventArgs e)
        {
            String link = "https://itunes.apple.com/us/album/state-of-mind-ep/id1245858545";
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += (s, e1) =>
            {
                Thread.Sleep(250);
            };
            Process.Start(link);
        }

        private void nightlikethisgooglemusic(object sender, RoutedEventArgs e)
        {
            String link = "https://play.google.com/store/music/album?id=Bhdahoyg7ltamdyiqrt5r3wo5fa";
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += (s, e1) =>
            {
                Thread.Sleep(250);
            };
            Process.Start(link);
        }

        private void nightlikethisspotify(object sender, RoutedEventArgs e)
        {
            String link = "https://open.spotify.com/album/1mleQbNkIbQ8kOG4yGmgBN?";
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += (s, e1) =>
            {
                Thread.Sleep(250);
            };
           Process.Start(link);
        }

        private void donateviapaypal(object sender, RoutedEventArgs e)
        {
            String link = "https://twitch.streamlabs.com/edude15000#/";
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += (s, e1) =>
            {
                Thread.Sleep(250);
            };
            Process.Start(link);
        }

        private void websitebutton_Click(object sender, RoutedEventArgs e)
        {
            String link = "http://dudebot.webs.com/";
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += (s, e1) =>
            {
                Thread.Sleep(250);
            };
            Process.Start(link);
        }

        private void discordbutton_Click(object sender, RoutedEventArgs e)
        {
            String link = "https://discord.gg/NFehx5h";
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += (s, e1) =>
            {
                Thread.Sleep(250);
            };
            Process.Start(link);
        }

        private void clearHistory_Click(object sender, RoutedEventArgs e)
        {
            bot.requestSystem.songHistory.Clear();
            writeToConfig(null, null);
        }
    }
    [ValueConversion(typeof(String[]), typeof(string))]
    public class ListToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || ((String[])value).Length == 0)
            {
                return "";
            }
            return String.Join(", ", ((String[])value));
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || value.ToString().Equals(""))
            {
                return new String[0];
            }
            return value.ToString().Split(',');
        }
    }
    [ValueConversion(typeof(List<String>), typeof(string))]
    public class ListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || ((List<String>)value).Count == 0)
            {
                return "";
            }
            return String.Join(", ", ((List<String>)value));
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || value.ToString().Equals(""))
            {
                return new List<String>();
            }
            return new List<String>(value.ToString().Split(','));
        }
    }

}