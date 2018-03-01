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
using System.Windows.Data;
using DudeBot;

namespace WpfApplication1
{
    public partial class MainWindow 
    {

        public static TwitchBot bot;
        public static Boolean displayedBrowser = false;
        public String dudebotdirectory = Path.GetTempPath() + "dudebotdirectory.txt";
        public String dudebotupdateinfo = Path.GetTempPath() + "dudebotupdateinfo.txt";
        
        String selectedQuote = "", selectedFavSong = "";
        Command selectedHotKey;
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
                            String message = bot.userVariables(c.output, bot.channel, bot.streamer,
                                    Utils.getFollowingText(c.output), c.output, false, null);
                            if (c.output.StartsWith("!"))
                            {
                                bot.processMessage(null, message, bot.streamer);
                            }
                            else
                            {
                                bot.client.SendMessage(message);
                            }
                        }
                    }
                    catch (Exception e1)
                    {
                        Console.WriteLine(e1.ToString());
                        Utils.errorReport(e1);
                    }
                }
            }
        }
        
        public async void addsongtofavorites(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Accented;
                var msgbox_settings = new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" };
                MessageDialogResult messageBoxResult = await this.ShowMessageAsync("Add to favorites?", "Would you like to add this song to your favorite songs list?", MessageDialogStyle.AffirmativeAndNegative, msgbox_settings);
                if (messageBoxResult == MessageDialogResult.Affirmative)
                {
                    Thread.Sleep(200);
                    TextBlock textBlock = (sender as TextBlock);
                    object datacontext = textBlock.DataContext;
                    String c = (String)datacontext;
                    bot.requestSystem.favSongs.Add(c.Substring(c.IndexOf('-') + 2));
                    writeToConfig(null, null);
                }
            }
        }

        public async void addhotkey(Object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editHotKeyInput.Text)) && !string.IsNullOrWhiteSpace(editHotKeyCommand.Text))
            {
                for (int i = 0; i < bot.hotkeyCommandList.Count; i++)
                {
                    if (selectedHotKey != null && (bot.hotkeyCommandList[i].input[0].Equals(selectedHotKey.input[0]) || bot.hotkeyCommandList[i].output.Equals(selectedHotKey.output)))
                    {
                        bot.commandList.Remove(bot.hotkeyCommandList[i]);
                        break;
                    }
                }
                selectedHotKey = null;
                String[] str = { editHotKeyInput.Text };
                bot.commandList.Add(new Command(str, 3, editHotKeyCommand.Text, "hotkey", true));
                bot.resetAllCommands();
                editHotKeyInput.Text = "";
                editHotKeyCommand.Text = "";
                writeToConfig(null, null);
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
                foreach (Command c in bot.hotkeyCommandList)
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
                writeToConfig(null, null);
            }
            else
            {
                await this.ShowMessageAsync("Warning", "Please enter the hotkey and message/command to send!");
            }
        }

        private void editHotKeyInput_KeyDown(object sender, KeyEventArgs e)
        {
            editHotKeyInput.Text = "";
            if (e.Key.ToString().StartsWith("F"))
            {
                editHotKeyInput.Text = e.Key.ToString();
            }
        }

        private void editHotKeyButtonClick(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (sender as TextBlock);
            object datacontext = textBlock.DataContext;
            Command c = (Command)datacontext;
            selectedHotKey = c;
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
                        c.level = editCommandLevel.SelectedIndex;
                        c.costToUse = (int)editCommandCost.Value;
                        bot.resetAllCommands();
                        editCommandCost.Value = 0;
                        editCommand.Text = "";
                        editResponse.Text = "";
                        writeToConfig(null, null);
                        return;
                    }
                }
                String[] str = { formatCommand(editCommand.Text) };
                Command d = new Command(str, editCommandLevel.SelectedIndex, editResponse.Text, "user", true);
                d.costToUse = (int)editCommandCost.Value;
                bot.commandList.Add(d);
                bot.resetAllCommands();
                editCommandCost.Value = 0;
                editCommand.Text = "";
                editResponse.Text = "";
                writeToConfig(null, null);
            }
            else
            {
                await this.ShowMessageAsync("Warning", "Please enter the command name and response!");
            }
        }

        public async void addoverride(Object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editOverrideInput.Text)) && !string.IsNullOrWhiteSpace(editOverrideOutput.Text))
            {
                if (editOverrideType.SelectedIndex < 0)
                {
                    editOverrideType.SelectedIndex = 0;
                }
                foreach (Command c in bot.overrideCommandList)
                {
                    if (c.input[0].Equals(editOverrideInput.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        c.output = editOverrideOutput.Text;
                        c.overrideType = editOverrideType.SelectedIndex;
                        bot.resetAllCommands();
                        editOverrideType.SelectedIndex = 0;
                        editOverrideInput.Text = "";
                        editOverrideOutput.Text = "";
                        writeToConfig(null, null);
                        return;
                    }
                }
                String[] str = { editOverrideInput.Text };
                Command d = new Command(str, 0, editOverrideOutput.Text, "override", true);
                d.overrideType = editOverrideType.SelectedIndex;
                bot.commandList.Add(d);
                bot.resetAllCommands();
                editOverrideType.SelectedIndex = 0;
                editOverrideInput.Text = "";
                editOverrideOutput.Text = "";
                writeToConfig(null, null);
            }
            else
            {
                await this.ShowMessageAsync("Warning", "Please enter the override input and output!");
            }
        }

        public async void addscenario(Object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(editAdventureName.Text) && !string.IsNullOrWhiteSpace(editAdventureStartMessage.Text) && 
                !string.IsNullOrWhiteSpace(editAdventureEveryoneLosesMessage.Text) && !string.IsNullOrWhiteSpace(editAdventureOneWinnerMessage.Text) &&
                !string.IsNullOrWhiteSpace(editAdventureMultipleWinnersMessage.Text))
            {
                foreach (AdventureScenario c in bot.adventureScenarioList)
                {
                    if (c.name.Equals(editAdventureName.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        c.startMessage = editAdventureStartMessage.Text;
                        c.allLosersMessage = editAdventureEveryoneLosesMessage.Text;
                        c.oneWinnerMessage = editAdventureOneWinnerMessage.Text;
                        c.multipleWinnersMessage = editAdventureMultipleWinnersMessage.Text;
                        editAdventureName.Text = "";
                        editAdventureStartMessage.Text = "";
                        editAdventureEveryoneLosesMessage.Text = "";
                        editAdventureOneWinnerMessage.Text = "";
                        editAdventureMultipleWinnersMessage.Text = "";
                        writeToConfig(null, null);
                        return;
                    }
                }
                AdventureScenario d = new AdventureScenario(editAdventureName.Text, editAdventureStartMessage.Text, editAdventureEveryoneLosesMessage.Text, 
                    editAdventureOneWinnerMessage.Text, editAdventureMultipleWinnersMessage.Text);
                bot.adventureScenarioList.Add(d);
                editAdventureName.Text = "";
                editAdventureStartMessage.Text = "";
                editAdventureEveryoneLosesMessage.Text = "";
                editAdventureOneWinnerMessage.Text = "";
                editAdventureMultipleWinnersMessage.Text = "";
                writeToConfig(null, null);
            }
            else
            {
                await this.ShowMessageAsync("Warning", "Please enter all of the scenario information!");
            }
        }

        public async void removescenario(Object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editAdventureName.Text)))
            {
                foreach (AdventureScenario c in bot.adventureScenarioList)
                {
                    if (c.name.Equals(editAdventureName.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        bot.adventureScenarioList.Remove(c);
                        writeToConfig(null, null);
                        break;
                    }
                }
                editAdventureName.Text = "";
                editAdventureStartMessage.Text = "";
                editAdventureEveryoneLosesMessage.Text = "";
                editAdventureOneWinnerMessage.Text = "";
                editAdventureMultipleWinnersMessage.Text = "";
                writeToConfig(null, null);
            }
            else
            {
                await this.ShowMessageAsync("Warning", "To delete or edit a scenario, click on it in the box and then press the desired button!");
            }
        }

        public async void addreward(Object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editRewardName.Text)))
            {
                foreach (Command c in bot.rewardCommandList)
                {
                    if (c.input[0].Equals(editRewardName.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        c.output = editRewardOutput.Text;
                        c.costToUse = (int)editRewardCost.Value;
                        bot.resetAllCommands();
                        editRewardName.Text = "";
                        editRewardOutput.Text = "";
                        writeToConfig(null, null);
                        return;
                    }
                }
                String[] str = { editRewardName.Text };
                Command c2 = new Command(str, 0, editRewardOutput.Text, "reward", true);
                c2.costToUse = (int)editRewardCost.Value;
                bot.commandList.Add(c2);
                bot.resetAllCommands();
                editRewardName.Text = "";
                editRewardOutput.Text = "";
                writeToConfig(null, null);
            }
            else
            {
                await this.ShowMessageAsync("Warning", "Please enter the reward name and response!");
            }
        }

        public async void removereward(Object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editRewardName.Text)))
            {
                foreach (Command c in bot.rewardCommandList)
                {
                    if (c.input[0].Equals(editRewardName.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        bot.commandList.Remove(c);
                        writeToConfig(null, null);
                        break;
                    }
                }
                editRewardName.Text = "";
                editRewardOutput.Text = "";
                bot.resetAllCommands();
                writeToConfig(null, null);
            }
            else
            {
                await this.ShowMessageAsync("Warning", "To delete or edit a reward, click on it in the box and then press the desired button!");
            }
        }

        private void editRewardButtonClick(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (sender as TextBlock);
            object datacontext = textBlock.DataContext;
            Command c = (Command)datacontext;
            editRewardName.Text = c.input[0];
            editRewardCost.Value = c.costToUse;
            editRewardOutput.Text = c.output;
        }

        public async void addcurrency(Object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editCurrencyUserName.Text)))
            {
                foreach (BotUser b in bot.users)
                {
                    if (b.username.Equals(editCurrencyUserName.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        b.points = (int)editCurrencyAmount.Value;
                        b.time = (int)editCurrencyTime.Value;
                        b.subCredits = (int)editCurrencySubCredits.Value;
                        editCurrencyAmount.Value = 0;
                        editCurrencyTime.Value = 0;
                        editCurrencySubCredits.Value = 0;
                        editCurrencyUserName.Text = "";
                        writeToConfig(null, null);
                        return;
                    }
                }
                bot.users.Add(new BotUser(editCurrencyUserName.Text, 0, false, false, false, (int)editCurrencyAmount.Value, (int)editCurrencyTime.Value, null, 0, 0, (int)editCurrencySubCredits.Value));
                editCurrencyAmount.Value = 0;
                editCurrencyTime.Value = 0;
                editCurrencySubCredits.Value = 0;
                editCurrencyUserName.Text = "";
                writeToConfig(null, null);
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
                foreach (JoinEvent s in bot.eventsList)
                {
                    if (s.userJoin.Equals(editEventUser.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        s.message = editEventMessage.Text;
                        editEventUser.Text = "";
                        editEventMessage.Text = "";
                        writeToConfig(null, null);
                        return;
                    }
                }
                bot.eventsList.Add(new JoinEvent(editEventUser.Text, editEventMessage.Text));
                editEventUser.Text = "";
                editEventMessage.Text = "";
                writeToConfig(null, null);
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
                        c.costToUse = (int)editImagecost.Value;
                        c.level = editImageCommandLevel.SelectedIndex;
                        bot.resetAllCommands();
                        editImagecost.Value = 0;
                        editCommandImage.Text = "";
                        editResponseImage.Text = "";
                        editImageCommandLevel.SelectedIndex = 0;
                        writeToConfig(null, null);
                        return;
                    }
                }
                String[] str = { formatCommand(editCommandImage.Text) };
                Command d = new Command(str, 0, editResponseImage.Text, "image", true);
                d.costToUse = (int)editImagecost.Value;
                d.level = editImageCommandLevel.SelectedIndex;
                bot.commandList.Add(d);
                bot.resetAllCommands();
                editImagecost.Value = 0;
                editCommandImage.Text = "";
                editResponseImage.Text = "";
                editImageCommandLevel.SelectedIndex = 0;
                writeToConfig(null, null);
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
                        quoteEdit.Text = "";
                        selectedQuote = "";
                        writeToConfig(null, null);
                        return;
                    }
                }
            }
            else
            {
                await this.ShowMessageAsync("Warning", "Please enter the quote!");
            }
        }

        public async void addfavsong(Object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(favSongEdit.Text))
            {
                for (int i = 0; i < bot.requestSystem.favSongs.Count; i++)
                {
                    if (bot.requestSystem.favSongs[i].Equals(selectedFavSong))
                    {
                        bot.requestSystem.favSongs[i] = favSongEdit.Text;
                        favSongEdit.Text = "";
                        selectedFavSong = "";
                        writeToConfig(null, null);
                        return;
                    }
                }
                bot.requestSystem.favSongs.Add(favSongEdit.Text);
                favSongEdit.Text = "";
                selectedFavSong = "";
                writeToConfig(null, null);
            }
            else
            {
                await this.ShowMessageAsync("Warning", "Please enter the song!");
            }
        }
        
        [STAThread]
        public void playSound(Object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            ((Command)fe.DataContext).playSound();
        }

        [STAThread]
        public void playImage(Object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            ((Command)fe.DataContext).playImage();
        }

        public async void removefavsong(Object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(favSongEdit.Text))
            {
                foreach (String b in bot.requestSystem.favSongs)
                {
                    if (b.Equals(favSongEdit.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        bot.requestSystem.favSongs.Remove(b);
                        break;
                    }
                }
                favSongEdit.Text = "";
                selectedFavSong = "";
                writeToConfig(null, null);
            }
            else
            {
                await this.ShowMessageAsync("Warning", "Please enter the song!");
            }
        }

        private void editFavSongButtonClick(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (sender as TextBlock);
            object datacontext = textBlock.DataContext;
            favSongEdit.Text = (String)datacontext;
            selectedFavSong = favSongEdit.Text;
        }

        public async void addrank(Object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editRankTitle.Text)))
            {
                foreach (KeyValuePair<String, Int32> s in bot.currency.ranks)
                {
                    if (s.Key.Equals(editRankTitle.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        bot.currency.ranks[s.Key] = (int)editRankCost.Value;
                        bot.resetAllCommands();
                        editRankTitle.Text = "";
                        editRankCost.Value = 0;
                        writeToConfig(null, null);
                        return;
                    }
                }
                bot.currency.ranks.Add(editRankTitle.Text, (int)editRankCost.Value);
                bot.resetAllCommands();
                editRankTitle.Text = "";
                editRankCost.Value = 0;
                writeToConfig(null, null);
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
                        c.costToUse = (int)editSFXcost.Value;
                        c.level = editSFXCommandLevel.SelectedIndex;
                        bot.resetAllCommands();
                        editImagecost.Value = 0;
                        editCommandSFX.Text = "";
                        editResponseSFX.Text = "";
                        volumeSetter.Value = 100;
                        editSFXCommandLevel.SelectedIndex = 0;
                        writeToConfig(null, null);
                        return;
                    }
                }
                String[] str = { formatCommand(editCommandSFX.Text) };
                Command d = new Command(str, 0, editResponseSFX.Text, "sfx", true);
                d.volumeLevel = (int)volumeSetter.Value;
                d.costToUse = (int)editSFXcost.Value;
                d.level = editSFXCommandLevel.SelectedIndex;
                bot.commandList.Add(d);
                bot.resetAllCommands();
                editImagecost.Value = 0;
                editCommandSFX.Text = "";
                editResponseSFX.Text = "";
                volumeSetter.Value = 100;
                editSFXCommandLevel.SelectedIndex = 0;
                writeToConfig(null, null);
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
                        if (editToggleTimed.IsChecked == true)
                        {
                            c.toggle = true;
                        }
                        else
                        {
                            c.toggle = false;
                        }
                        bot.resetAllCommands();
                        editCommandTimed.Text = "";
                        editResponseTimed.Text = "";
                        editToggleTimed.IsChecked = false;
                        writeToConfig(null, null);
                        return;
                    }
                }
                String[] str = { formatCommand(editCommandTimed.Text) };
                Boolean toggled = false;
                if (editToggleTimed.IsChecked == true)
                {
                    toggled = true;
                }
                bot.commandList.Add(new Command(str, 0, editResponseTimed.Text, "timer", toggled));
                bot.resetAllCommands();
                editCommandTimed.Text = "";
                editResponseTimed.Text = "";
                editToggleTimed.IsChecked = false;
                writeToConfig(null, null);
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
                writeToConfig(null, null);
            }
            else
            {
                await this.ShowMessageAsync("Warning", "To delete or edit a command, click on it in the box and then press the desired button!");
            }
        }

        public async void removeoverride(Object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editOverrideInput.Text)))
            {
                foreach (Command c in bot.overrideCommandList)
                {
                    if (c.input[0].Equals(editOverrideInput.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        bot.commandList.Remove(c);
                        break;
                    }
                }
                editOverrideInput.Text = "";
                editOverrideOutput.Text = "";
                editOverrideType.SelectedIndex = 0;
                bot.resetAllCommands();
                writeToConfig(null, null);
            }
            else
            {
                await this.ShowMessageAsync("Warning", "To delete or edit an override, click on it in the box and then press the desired button!");
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
                editCurrencyAmount.Value = 0;
                editCurrencyTime.Value = 0;
                editCurrencySubCredits.Value = 0;
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
                foreach (DudeBot.JoinEvent b in bot.eventsList)
                {
                    if (b.userJoin.Equals(editEventUser.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        bot.eventsList.Remove(b);
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
                editImageCommandLevel.SelectedIndex = 0;
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
                selectedQuote = "";
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
                foreach (KeyValuePair<String, Int32> b in bot.currency.ranks)
                {
                    if (b.Key.Equals(editCurrencyUserName.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        bot.currency.ranks.Remove(b.Key);
                        break;
                    }
                }
                editRankTitle.Text = "";
                editRankCost.Value = 0;
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
                editSFXCommandLevel.SelectedIndex = 0;
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

        public void customsForgeWindow(Object sender, RoutedEventArgs e)
        {
            App.customsForgeWindow = new CustomsForgeLogin.MainWindow(bot);
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

        public void saveNotes(Object sender, RoutedEventArgs e)
        {
            StreamWriter sw = new StreamWriter(Utils.notesFile);
            sw.Write(notestextbox.Text);
            sw.Close();
        }

        public void writeToConfig(Object sender, RoutedEventArgs e)
        {
            if (bot == null)
            {
                bot = new TwitchBot();
                bot.streamer = streamerName.Text;
                bot.botName = botName.Text;
                bot.oauth = oauth.Text;
                loadPresets();
                openBot(null, null);
            }
            else
            {
                if (bot.client == null)
                {
                    bot.streamer = streamerName.Text;
                    bot.botName = botName.Text;
                    bot.oauth = oauth.Text;
                    openBot(null, null);
                }
                else
                {
                    refreshAllDataGrids();
                }
            }
            Utils.saveData(bot);
        }
        
        public void refreshAllDataGrids()
        {
            try
            {
                foreach (DataGrid d in FindLogicalChildren<DataGrid>(App.Current.MainWindow))
                {
                    d.Items.Refresh();
                }
                bot.requestSystem.formattedTotalTime = bot.requestSystem.formatTotalTime();
                bot.requestSystem.songListLength = bot.requestSystem.songList.Count;
            }
            catch (Exception)
            {
            }
        }

        public void killBot(Object sender, RoutedEventArgs e)
        {
            try
            {
                bot.client.SendMessage(bot.endMessage);
                bot.clearUpTempData();
                Utils.saveData(bot);
                bot.botDisconnect();
                open.IsEnabled = true;
                kill.IsEnabled = false;
            }
            catch (Exception)
            {
            }
        }

        public async void openBot(Object sender, RoutedEventArgs e)
        {
            try
            {
                bot.client.Disconnect();
                bot.client.LeaveChannel(bot.channel);
            }
            catch
            {
            }
            if (bot == null)
            {
                bot = Utils.loadData(); // Sets up and connects bot object
            }
            if (bot == null)
            {
                customization.Focus();
                await this.ShowMessageAsync("Notice", "Missing or corrupt 'userData.json' file. If you are updating from DudeBot 2 to DudeBot 3," +
                    " please run 'DudeBotConfigUpdater.exe' in the bin folder, this will create an updated configuration file. If you are a new user, please enter your stream information and press 'apply'. " +
                    "To restore DudeBot and fix the configuration file, press 'reset dudebot'.");
                return;
            }
            bot.botStartUpAsync();
            new Thread(new ThreadStart(copyToSupporters)).Start();
            try
            {
                bot.client.Connect();
                bot.writeToEventLog("BOT CONNECTED");
                kill.IsEnabled = true;
                open.IsEnabled = false;
                if (bot.streamer != null && bot.streamer != "" && bot.client != null)
                {
                    showBrowser();
                    dashboard.Focus();
                }
            }
            catch (Exception)
            {
                customization.Focus();
                await this.ShowMessageAsync("Connection Failure", "DudeBot failed to connect to Twitch, please ensure that your streamer and bot names and oauths are correct!");
                if (bot != null)
                {
                    bot.client = null;
                }
                kill.IsEnabled = false;
                open.IsEnabled = true;
                return;
            }
            DataContext = bot;
            BindingOperations.EnableCollectionSynchronization(bot.requestSystem.songList, new object());
            if (bot.openRockSnifferOnStartUp == true)
            {
                openRockSniffer(null, null);
            }
        }

        public void showBrowser()
        {
            if (displayedBrowser)
            {
                return;
            }
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
            displayedBrowser = true;
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
                currencyPerMinuteSubs.IsEnabled = true;
                subCreditRedeemCost.IsEnabled = true;
                creditsPerSub.IsEnabled = true;
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
                currencyPerMinuteSubs.IsEnabled = false;
                subCreditRedeemCost.IsEnabled = false;
                creditsPerSub.IsEnabled = false;
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
            if (cb.SelectedValue == null)
            {
                cb.SelectedIndex = 3;
            }
            String color = cb.SelectedValue.ToString();
            int num = cb.SelectedIndex;
            color = color.Substring(color.IndexOf(":") + 1).Trim();
            SolidColorBrush brush = (SolidColorBrush)new BrushConverter().ConvertFromString(color);
            tabholder.Background = brush;
            this.Background = brush;
            foreach (ScrollViewer t in FindLogicalChildren<ScrollViewer>(this))
            {
                t.Background = brush;
            }
            foreach (StackPanel t in FindLogicalChildren<StackPanel>(this))
            {
                t.Background = brush;
            }
            foreach (Grid t in FindLogicalChildren<Grid>(this))
            {
                t.Background = brush;
            }
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
            foreach (DataGridCell tb in FindLogicalChildren<DataGridCell>(this))
            {
                tb.Background = brush;
            }
            foreach (GroupBox tb in FindLogicalChildren<GroupBox>(this))
            {
                var drawingcolor = System.Drawing.Color.FromArgb(brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B);
                var myColor = System.Windows.Forms.ControlPaint.Dark(drawingcolor);
                tb.Background = new SolidColorBrush(Color.FromArgb(myColor.A, myColor.R, myColor.G, myColor.B));
                myColor = System.Windows.Forms.ControlPaint.Light(drawingcolor);
                tb.BorderBrush = new SolidColorBrush(Color.FromArgb(myColor.A, myColor.R, myColor.G, myColor.B));
            }
        }

        public void textColorChange_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            if (cb.SelectedValue == null)
            {
                cb.SelectedIndex = 1;
            }
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
                if (!tb.Name.Equals("eventlog"))
                {
                    tb.Foreground = brush;
                }
            }
            foreach (TextBlock tb in FindLogicalChildren<TextBlock>(this))
            {
                if (!tb.Name.Equals("streamUptimeText") && !tb.Name.Equals("tb_a") && !tb.Name.Equals("tb_b") && !tb.Name.Equals("tb_c") && !tb.Name.Equals("tb_d") && !tb.Name.Equals("tb_e") && !tb.Name.Equals("tb_f"))
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
            if (cb.SelectedValue == null)
            {
                cb.SelectedIndex = 0;
            }
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
            if (cb.SelectedValue == null)
            {
                cb.SelectedIndex = 0;
            }
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
            if (cb.SelectedValue == null)
            {
                cb.SelectedIndex = 0;
            }
            String color = cb.SelectedValue.ToString();
            color = color.Substring(color.IndexOf(":") + 1).Trim();
            SolidColorBrush brush = (SolidColorBrush)new BrushConverter().ConvertFromString(color);
            foreach (Button tb in FindLogicalChildren<Button>(this))
            {
                if (!tb.Name.Equals("streamUptimeText") && !tb.Name.Equals("kill") && !tb.Name.Equals("open") && !tb.Name.Equals("flyoutbutton") && !tb.Name.Equals("b_a") && !tb.Name.Equals("b_b") && !tb.Name.Equals("b_c") && !tb.Name.Equals("b_d") && !tb.Name.Equals("b_e") && !tb.Name.Equals("b_f") && !tb.Name.Equals("websitebutton") && !tb.Name.Equals("discordbutton"))
                {
                    tb.Background = brush;
                }
            }
        }
        
        private async void loaded(Object sender, RoutedEventArgs e)
        {
            colorChange.SelectedIndex = 3;
            textColorChange.SelectedIndex = 1;
            textblockcolor.SelectedIndex = 0;
            fontStyle.SelectedIndex = 0;
            buttonColor.SelectedIndex = 0;
            if (File.Exists(Utils.readmeFile))
            {
                readmetextbox.Text = File.ReadAllText(Utils.readmeFile);
            }
            else
            {
                readmetextbox.Text = "Readme file could not be found!";
            }
            if (File.Exists(Utils.notesFile))
            {
                notestextbox.Text = File.ReadAllText(Utils.notesFile);
            }
            else
            {
                notestextbox.Text = "Notes file could not be found!";
            }
            if (!File.Exists(dudebotdirectory))
            {
                File.Create(dudebotdirectory).Close();
            }
            if (!File.Exists(dudebotupdateinfo))
            {
                File.Create(dudebotupdateinfo).Close();
            }
            StreamWriter writer = new StreamWriter(dudebotdirectory);
            String location = Assembly.GetExecutingAssembly().Location.ToString();
            writer.Write(location.Substring(0, location.IndexOf(@"DudeBot.exe")));
            writer.Close();
            copyUpdaterFile();
            if (!checkPrereqs())
            {
                customization.Focus();
                await this.ShowMessageAsync("Welcome to DudeBot!", "This is your first time using DudeBot. Please Enter your Streamer Name, Bot Name, Bot Oauth, then scroll to the bottom and press 'APPLY'.");
            }
            else
            {
                try
                {
                    _listener = new LowLevelKeyboardListener();
                    _listener.OnKeyPressed += _listener_OnKeyPressed;
                    _listener.HookKeyboard();

                    if (!checkinfo())
                    {
                        if (checkUpdate() == 1)
                        {
                            customization.Focus();
                            MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Accented;
                            var msgbox_settings = new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" };
                            MessageDialogResult messageBoxResult = await this.ShowMessageAsync("Update Available!", "An update was found, would you like to update?", MessageDialogStyle.AffirmativeAndNegative, msgbox_settings);
                            if (messageBoxResult == MessageDialogResult.Affirmative)
                            {
                                Thread.Sleep(1000);
                                try
                                {
                                    Process.Start(Path.GetTempPath() + @"\dudebotupdater.exe");
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

        public void copyUpdaterFile()
        {
            string fileName = "dudebotupdater.exe";
            string sourcePath = @"bin";
            string targetPath = Path.GetDirectoryName(Path.GetTempPath() + @"\dudebot");
            string sourceFile = Path.Combine(sourcePath, fileName);
            string destFile = Path.Combine(targetPath, fileName);
            File.Copy(sourceFile, destFile, true);
        }

        public void openImageWindow(Object sender, RoutedEventArgs e)
        {
            App.imagesWindow.startUp(bot.image.imageDisplayTimeSeconds);
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
                        customization.Focus();
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
                    if (s.Contains("Newest Release: (" + version) || s.Contains("We're currently down for maintenance"))
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
                customization.Focus();
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
                customization.Focus();
                await this.ShowMessageAsync("", "DudeBot is up to date!");
            }
            else
            {
                customization.Focus();
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
            MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Accented;
            var msgbox_settings = new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" };
            MessageDialogResult messageBoxResult = await this.ShowMessageAsync("Clear List?", "Are you sure you want to clear the current request queue?", MessageDialogStyle.AffirmativeAndNegative, msgbox_settings);
            if (messageBoxResult == MessageDialogResult.Affirmative)
            {
                Thread.Sleep(1000);
                bot.requestSystem.clearCOMMAND(bot.requestSystem.clearComm.input[0], bot.channel, bot.streamer, null);
                writeToConfig(null, null);
            }
        }
        
        public void sortByUserName(Object sender, RoutedEventArgs e)
        {
            bot.users = new List<BotUser>(bot.users.OrderBy(o => o.username).ToList());
            writeToConfig(null, null);
        }

        public void sortByRankTitle(Object sender, RoutedEventArgs e)
        {
            bot.users = new List<BotUser>(bot.users.OrderBy(o => o.rank).ToList());
            writeToConfig(null, null);
        }

        public void sortByAmount(Object sender, RoutedEventArgs e)
        {
            bot.users = new List<BotUser>(bot.users.OrderBy(o => o.points).ToList());
            writeToConfig(null, null);
        }

        public void sortByMinutes(Object sender, RoutedEventArgs e)
        {
            bot.users = new List<BotUser>(bot.users.OrderBy(o => o.time).ToList());
            writeToConfig(null, null);
        }

        public void sortBySubCredits(Object sender, RoutedEventArgs e)
        {
            bot.users = new List<BotUser>(bot.users.OrderBy(o => o.subCredits).ToList());
            writeToConfig(null, null);
        }

        private void moveup(Object sender, MouseButtonEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            Song c = ((Song)fe.DataContext);
            for (int i = 0; i < bot.requestSystem.songList.Count; i++)
            {
                if (bot.requestSystem.songList[i].name.Equals(c.name, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (i == 0)
                    {
                        return;
                    }
                    Song a = bot.requestSystem.songList[i];
                    String bottomSongLevel = a.level;
                    String topSongLevel = bot.requestSystem.songList[i - 1].level;
                    bot.requestSystem.songList.Remove(a);
                    bot.requestSystem.insertSong(a.name, a.requester, i-1, "", false);
                    bot.requestSystem.songList[i - 1].level = topSongLevel;
                    bot.requestSystem.songList[i].level = bottomSongLevel;
                    writeToConfig(null, null);
                    break;
                }
            }
        }

        private void movetotop(Object sender, MouseButtonEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            Song c = ((Song)fe.DataContext);
            try
            {
                bot.requestSystem.songList.Remove(c);
                if (bot.requestSystem.songList.Count > 1)
                {
                    c.level = bot.requestSystem.songList[0].level;
                }
                bot.requestSystem.songList.Insert(1, c);
                bot.requestSystem.setIndexesForSongs();
                writeToConfig(null, null);
            }
            catch (Exception) { }
        }

        private async void deleteSong(Object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(formatCommand(editSong.Text)))
            {
                foreach (Song s in bot.requestSystem.songList)
                {
                    if (s.name.Equals(editSong.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Accented;
                        var msgbox_settings = new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" };
                        MessageDialogResult messageBoxResult = await this.ShowMessageAsync("Remove Song", "Are you sure you want to remove this song?", MessageDialogStyle.AffirmativeAndNegative, msgbox_settings);
                        if (messageBoxResult == MessageDialogResult.Affirmative)
                        {
                            bot.requestSystem.songList.Remove(s);
                            bot.requestSystem.writeToCurrentSong(bot.channel, true);
                            break;
                        }
                    }
                }
                editSong.Text = "";
                editRequester.Text = "";
                songplace.Value = 1;
                writeToConfig(null, null);
            }
            else
            {
                await this.ShowMessageAsync("Warning", "To delete or edit a song, click on it in the box and then press the desired button!");
            }
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
                bot.requestSystem.insertSong(editSong.Text, uName, (int)songplace.Value - 1, "", false);
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
            if (bot.requestSystem.songList.Count < songplace.Value)
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
                Song oldsong = bot.requestSystem.songList[(int)songplace.Value - 1];
                Song newsong = new Song(editSong.Text, uName, oldsong.level, bot);
                newsong.index = oldsong.index;
                bot.requestSystem.songList[(int)songplace.Value - 1] = newsong;
                if (bot.requestSystem.checkCustomsForge)
                {
                    bot.requestSystem.songList[(int)songplace.Value - 1].setEntry(bot.requestSystem.getSongFromIgnition(editSong.Text, false), bot.requestSystem.search);
                }
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
                App.imagesWindow.Close();
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
            Application.Current.Shutdown();
            Environment.Exit(0);
        }

        private void editScenarioButtonClick(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (sender as TextBlock);
            object datacontext = textBlock.DataContext;
            AdventureScenario c = (AdventureScenario)datacontext;
            editAdventureStartMessage.Text = c.startMessage;
            editAdventureEveryoneLosesMessage.Text = c.allLosersMessage;
            editAdventureOneWinnerMessage.Text = c.oneWinnerMessage;
            editAdventureMultipleWinnersMessage.Text = c.multipleWinnersMessage;
            editAdventureName.Text = c.name;
        }

        private void editCommandButtonClick(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (sender as TextBlock);
            object datacontext = textBlock.DataContext;
            Command c = (Command)datacontext;
            editCommand.Text = c.input[0];
            editResponse.Text = c.output;
            editCommandLevel.SelectedIndex = c.level;
            editCommandCost.Value = c.costToUse;
        }

        private void editOverrideButtonClick(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (sender as TextBlock);
            object datacontext = textBlock.DataContext;
            Command c = (Command)datacontext;
            editOverrideType.SelectedIndex = c.overrideType;
            editOverrideInput.Text = c.input[0];
            editOverrideOutput.Text = c.output;
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
            editCurrencyAmount.Value = c.points;
            editCurrencyTime.Value = c.time;
            editCurrencySubCredits.Value = c.subCredits;
        }

        private void editRankButtonClick(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (sender as TextBlock);
            object datacontext = textBlock.DataContext;
            KeyValuePair<String, Int32> c = (KeyValuePair<String, Int32>)datacontext;
            editRankTitle.Text = c.Key;
            editRankCost.Value = c.Value;
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
            editSFXcost.Value = c.costToUse;
            editSFXCommandLevel.SelectedIndex = c.level;
        }

        private void editImageButtonClick(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (sender as TextBlock);
            object datacontext = textBlock.DataContext;
            Command c = (Command)datacontext;
            editCommandImage.Text = c.input[0];
            editResponseImage.Text = c.output;
            editImagecost.Value = c.costToUse;
            editImageCommandLevel.SelectedIndex = c.level;
        }

        private void editEventsButtonClick(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (sender as TextBlock);
            object datacontext = textBlock.DataContext;
            JoinEvent c = (JoinEvent)datacontext;
            editEventUser.Text = c.userJoin;
            editEventMessage.Text = c.message;
        }

        private void editSongButtonClick(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (sender as TextBlock);
            object datacontext = textBlock.DataContext;
            Song c = (Song)datacontext;
            songplace.Value = c.index;
            editSong.Text = c.name;
            editRequester.Text = c.requester;
            if (e.ClickCount == 2)
            {
                var backgroundWorker = new BackgroundWorker();
                if (bot.requestSystem.OpenCFLinkInsteadOfYoutube && c.customsForgeLink != null && !c.customsForgeLink.Equals(""))
                {
                    backgroundWorker.DoWork += (s, e1) =>
                    {
                        Thread.Sleep(250);
                    };
                    Process.Start(c.customsForgeLink);
                    return;
                }
                String link = "https://www.youtube.com/results?search_query=" + c.name;
                if (c.youtubeLink != null && c.youtubeLink != "")
                {
                    link = c.youtubeLink;
                }
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

        public void uploadStreamlabsCSV(object sender, EventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".csv";
            dlg.Filter = "CSV Files (*.csv)|*.csv";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                streamlabsCSVPath.Text = filename;
            }
        }

        public async void runCSVImport(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(formatCommand(streamlabsCSVPath.Text)))
            {
                await this.ShowMessageAsync("Incorrect Path!", "Streamlabs CSV Path is incorrect. Please reupload it and try again.");
                return;
            }
            String user = null, points = null, hours = null;
            try
            {
                using (var fs = File.OpenRead(streamlabsCSVPath.Text))
                using (var reader = new StreamReader(fs))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');
                        if (!values[0].Equals("Name"))
                        {
                            user = values[0];
                            points = values[1];
                            hours = values[2];
                            if (user != null && points != null && hours != null) {
                                BotUser newuser = bot.getBotUser(user);
                                if (newuser == null)
                                {
                                    bot.users.Add(new BotUser(user, 0, false, false, false, Int32.Parse(points), Int32.Parse(hours) * 60, null, 0, 0, 0));
                                }
                                else
                                {
                                    if (importFromStreamlabsCheckBox.IsChecked == false)
                                    {
                                        newuser.points = Int32.Parse(points);
                                        newuser.time = Int32.Parse(hours) * 60;
                                    }
                                }
                            }
                        }
                    }
                    reader.Close();
                }
                bot.removeAllUselessUsers();
                Utils.saveData(bot);
                await this.ShowMessageAsync("Successfully Imported", "Streamlabs points have been successfully imported.");
            }
            catch (Exception f)
            {
                MessageBox.Show(f.StackTrace);
                await this.ShowMessageAsync("Importing Failed", "Failed to import points, please try again later.");
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
            customization.Focus();
            MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Accented;
            var msgbox_settings = new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" };
            MessageDialogResult messageBoxResult = await this.ShowMessageAsync("Reset Confirmation", "Are you sure you want to reset DudeBot? All user data will be deleted, but this will fix any problems with DudeBot. This will close the bot and you will need to reopen it.", MessageDialogStyle.AffirmativeAndNegative, msgbox_settings);
            if (messageBoxResult == MessageDialogResult.Affirmative)
            {
                try
                {
                    File.Delete(Utils.backupUserDataFile);
                }
                catch (Exception) { }
                StreamWriter strm = File.CreateText(Utils.userDataFile);
                strm.Flush();
                strm.Close();
                bot = null;
                Window_Closing(null, null);
            }
        }

        public void loadPresets()
        {
            String[] str = { "F7" };
            bot.commandList.Add(new Command(str, 3, "!next", "hotkey", true));

            String[] str2 = { "pizza" };
            bot.commandList.Add(new Command(str2, 0, "EVERYONE LOVES PIZZA!", "override", true));

            String[] str3 = { "!harambe" };
            bot.commandList.Add(new Command(str3, 0, "Harambe would have loved $currentsong!", "user", true));
            String[] str4 = { "!8ball" };
            bot.commandList.Add(new Command(str4, 0, "$8ball", "user", true));
            String[] str5 = { "!hug" };
            bot.commandList.Add(new Command(str5, 0, "$user gives $input a hug!", "user", true));
            String[] str6 = { "!following" };
            bot.commandList.Add(new Command(str6, 0, "$user has been following the stream for $following", "user", true));
            String[] str7 = { "!start" };
            bot.commandList.Add(new Command(str7, 0, "$streamer joined Twitch on $start", "user", true));
            String[] str8 = { "!love" };
            bot.commandList.Add(new Command(str8, 0, "There is $randomnumber3% <3 between $user and $input!", "user", true));
            String[] str9 = { "!uptime" };
            bot.commandList.Add(new Command(str9, 0, "$streamer has been live for $uptime", "user", true));
            String[] str10 = { "!shoutout" };
            bot.commandList.Add(new Command(str10, 0, "$shoutout", "user", true));
            String[] str11 = { "!roulette" };
            bot.commandList.Add(new Command(str11, 0, "$roulette", "user", true));

            String[] str12 = { "!requestfav", "!songfav", "!playfav" };
            bot.requestSystem.favSongComm = new Command(str12, 0, "favSongComm", "bot", true);
            bot.commandList.Add(bot.requestSystem.favSongComm);

            String[] str13 = { "!requests" };
            bot.requestSystem.triggerRequestsComm = new Command(str13, 2, "triggerRequestsComm", "bot", true);
            bot.commandList.Add(bot.requestSystem.triggerRequestsComm);

            String[] str14 = { "!request", "!song", "!play", "!songrequest", "!sr" };
            bot.requestSystem.requestComm = new Command(str14, 0, "requestComm", "bot", true);
            bot.commandList.Add(bot.requestSystem.requestComm);

            String[] str15 = { "!songlist", "!list", "!playlist" };
            bot.requestSystem.songlistComm = new Command(str15, 0, "songlistComm", "bot", true);
            bot.commandList.Add(bot.requestSystem.songlistComm);

            String[] str16 = { "!queue", "!length", "!total" };
            bot.requestSystem.getTotalComm = new Command(str16, 0, "getTotalComm", "bot", true);
            bot.commandList.Add(bot.requestSystem.getTotalComm);

            String[] str17 = { "!viewers" };
            bot.getViewerComm = new Command(str17, 0, "getViewerCountCommands", "bot", true);
            bot.commandList.Add(bot.getViewerComm);

            String[] str18 = { "!addtop" };
            bot.requestSystem.addtopComm = new Command(str18, 2, "addtopComm", "bot", true);
            bot.commandList.Add(bot.requestSystem.addtopComm);

            String[] str19 = { "!addvip" };
            bot.requestSystem.addvipComm = new Command(str19, 2, "addvipComm", "bot", true);
            bot.commandList.Add(bot.requestSystem.addvipComm);

            String[] str20 = { "!adddonator" };
            bot.requestSystem.adddonatorComm = new Command(str20, 2, "adddonatorComm", "bot", true);
            bot.commandList.Add(bot.requestSystem.adddonatorComm);

            String[] str21 = { "!edit", "!change" };
            bot.requestSystem.editComm = new Command(str21, 2, "editComm", "bot", true);
            bot.commandList.Add(bot.requestSystem.editComm);

            String[] str22 = { "!next", "!skip" };
            bot.requestSystem.nextComm = new Command(str22, 2, "nextComm", "bot", true);
            bot.commandList.Add(bot.requestSystem.nextComm);

            String[] str23 = { "!clear" };
            bot.requestSystem.clearComm = new Command(str23, 2, "clearComm", "bot", true);
            bot.commandList.Add(bot.requestSystem.clearComm);

            String[] str24 = { "!current", "!playing", "!currentsong" };
            bot.requestSystem.getCurrentComm = new Command(str24, 0, "getCurrentComm", "bot", true);
            bot.commandList.Add(bot.requestSystem.getCurrentComm);

            String[] str25 = { "!nextsong" };
            bot.requestSystem.getNextComm = new Command(str25, 0, "getNextComm", "bot", true);
            bot.commandList.Add(bot.requestSystem.getNextComm);

            String[] str26 = { "!randomnext", "!nextrandom", "!randomsong", "!songrandom" };
            bot.requestSystem.randomComm = new Command(str26, 2, "randomComm", "bot", true);
            bot.commandList.Add(bot.requestSystem.randomComm);

            String[] str27 = { "!editsong" };
            bot.requestSystem.editSongComm = new Command(str27, 0, "editSongComm", "bot", true);
            bot.commandList.Add(bot.requestSystem.editSongComm);

            String[] str28 = { "!removesong", "!wrongsong" };
            bot.requestSystem.removeSongComm = new Command(str28, 0, "removeSongComm", "bot", true);
            bot.commandList.Add(bot.requestSystem.removeSongComm);

            String[] str29 = { "!mysong", "!position", "!songposition" };
            bot.requestSystem.songPositionComm = new Command(str29, 0, "songPositionComm", "bot", true);
            bot.commandList.Add(bot.requestSystem.songPositionComm);

            bot.currency = new Currency(null);
            bot.resetAllCommands();
        }

        private void flyout(object sender, RoutedEventArgs e)
        {
            customization.Focus();
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

        private async void clearHistory_Click(object sender, RoutedEventArgs e)
        {
            MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Accented;
            var msgbox_settings = new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" };
            MessageDialogResult messageBoxResult = await this.ShowMessageAsync("Clear?", "Are you sure you want to clear your history? ;)", MessageDialogStyle.AffirmativeAndNegative, msgbox_settings);
            if (messageBoxResult == MessageDialogResult.Affirmative)
            {
                Thread.Sleep(1000);
                bot.requestSystem.songHistory.Clear();
                writeToConfig(null, null);
            }
        }

        private void eventlog_TextChanged(object sender, TextChangedEventArgs e)
        {
            eventlog.ScrollToEnd();
        }

        private void reloadFollowersButton_Click(object sender, RoutedEventArgs e)
        {
            bot.checkAtBeginningAsync(true);
        }

        private void giveAllFollowersFollowPayout_Click(object sender, RoutedEventArgs e)
        {
            if (bot.autoFollowPayout == 0)
            {
                return;
            }
            foreach (BotUser user in bot.users)
            {
                if (!user.receivedFollowPayout && user.follower)
                {
                    user.points += bot.autoFollowPayout;
                    user.receivedFollowPayout = true;
                }
            }
        }

        private void sortSubsByUserName(object sender, RoutedEventArgs e)
        {
            bot.users = new List<BotUser>(bot.users.OrderBy(o => o.username).ToList());
            copyToSupporters();
            writeToConfig(null, null);
        }
        
        private void sortSubsByBits(object sender, RoutedEventArgs e)
        {
            bot.users = new List<BotUser>(bot.users.OrderBy(o => o.bitsDonated).ToList());
            copyToSupporters();
            writeToConfig(null, null);
        }

        private void sortSubsByMoney(object sender, RoutedEventArgs e)
        {
            bot.users = new List<BotUser>(bot.users.OrderBy(o => o.moneyDonated).ToList());
            copyToSupporters();
            writeToConfig(null, null);
        }

        private void sortSubsByMonths(object sender, RoutedEventArgs e)
        {
            bot.users = new List<BotUser>(bot.users.OrderBy(o => o.months).ToList());
            copyToSupporters();
            writeToConfig(null, null);
        }

        private void sortSubsBySubbed(object sender, RoutedEventArgs e)
        {
            bot.users = new List<BotUser>(bot.users.OrderBy(o => o.sub).ToList());
            copyToSupporters();
            writeToConfig(null, null);
        }

        public void copyToSupporters()
        {
            if (bot == null)
            {
                return;
            }
            while (true)
            {
                try
                {
                    try
                    {
                        bot.supporters.Clear();
                        foreach (BotUser user in bot.users)
                        {
                            if (user.sub || user.bitsDonated > 0)
                            {
                                bot.supporters.Add(user);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Utils.errorReport(e);
                        Console.WriteLine(e.ToString());
                    }
                    Thread.Sleep(10000);
                }
                catch (Exception)
                {
                }
            }
        }
        
        private async void clearFavorites_Click(object sender, RoutedEventArgs e)
        {
            MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Accented;
            var msgbox_settings = new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" };
            MessageDialogResult messageBoxResult = await this.ShowMessageAsync("Clear?", "Are you sure you want to clear your favorites?", MessageDialogStyle.AffirmativeAndNegative, msgbox_settings);
            if (messageBoxResult == MessageDialogResult.Affirmative)
            {
                Thread.Sleep(1000);
                bot.requestSystem.favSongs.Clear();
                writeToConfig(null, null);
            }
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
            return String.Join(",", ((String[])value));
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
            return String.Join(",", ((List<String>)value));
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