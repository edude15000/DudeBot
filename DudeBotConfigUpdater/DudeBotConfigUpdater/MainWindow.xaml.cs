using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace DudeBotConfigUpdater
{
    public partial class MainWindow : Window
    {
        List<String> subs = new List<String>();
        List<String> mods = new List<String>();
        Dictionary<String, String> purchasedRanks = new Dictionary<String, String>();
        Dictionary<String, String> numRequests = new Dictionary<String, String>();
        static readonly Regex trimmer = new Regex(@"\s\s+");
        String allCommands = "";

        public class Command
        {
            public String name { get; set; }
            public String level { get; set; }
            public String[] commands { get; set; }

            public Command(String name, String level, String[] commands)
            {
                this.name = name;
                this.level = level;
                this.commands = commands;
            }

        }
        public class User
        {
            public String name { get; set; }
            public String points { get; set; }
            public String hours { get; set; }
            public String subcredits { get; set; }

            public User(String name, String points, String hours, String subcredits)
            {
                this.name = name;
                this.points = points;
                this.hours = hours;
                this.subcredits = subcredits;
            }
        }

        public string getCommandNames(string str)
        {
            return str.Substring(str.LastIndexOf("=") + 1).Trim();
        }

        public String getLevel(String str)
        {
            return str[(str.IndexOf("=") + 1)].ToString();
        }

        private String getFollowingText(String line, char c)
        {
            try
            {
                String line2 = line.Substring(line.IndexOf(c) + 1);
                return line2;
            }
            catch (Exception)
            {
                return "";
            }

        }

        private String writeToString(String output, String name, String content)
        {
            if (content != null && content != "")
            {
                return output += "\"" + name + "\": " + formatType(content) + ",\r";
            }
            return output += "";
        }

        private String writeToStringNoComma(String output, String name, String content)
        {
            if (content != null && content != "")
            {
                return output += "\"" + name + "\": " + formatType(content) + "\r";
            }
            return output += "";
        }

        private String formatType(String str)
        {
            if (str == null)
            {
                return null;
            }
            if (str.Contains("\\"))
            {
                str = str.Replace("\\", "\\" + "\\");
            }
            if (str.Contains("\""))
            {
                str = str.Replace("\"", "\\" + "\"");
            }
            var isNumeric = int.TryParse(str, out var n);
            if (!str.Equals("false") && !str.Equals("true") && !isNumeric)
            {
                return "\"" + str + "\"";
            }
            return str;
        }

        private String writeArrayToString(String output, String name, String[] content)
        {
            output += "\"" + name + "\": [\r";
            for (int i = 0; i < content.Length; i++)
            {
                output += "\"" + content[i] + "\",\r";
            }
            return output += "],\r";
        }

        public MainWindow()
        {
            InitializeComponent();
            String commandsFile = "commands.txt";
            String configFile = "config.txt";
            String currencyFile = "currency.txt";
            String eventsFile = "events.txt";
            String imagesFile = "images.txt";
            String othersFile = "others.txt";
            String purchasedRanksFile = "purchasedranks.txt";
            String quotesFile = "quotes.txt";
            String ranksFile = "ranks.txt";
            String sfxFile = "sfx.txt";
            String timedCommandsFile = "timedcommands.txt";
            String usersFile = "users.txt";
            String userDataFile = "userData.json";
            if (!File.Exists(configFile))
            {
                MessageBox.Show("Missing an old configuration file!");
                Application.Current.Shutdown();
            }
            if (!File.Exists(userDataFile))
            {
                File.Create(userDataFile).Close();
            }
            else
            {
                MessageBox.Show("Configuration file has already been converted!");
                Application.Current.Shutdown();
            }
            StreamWriter writer = new StreamWriter(userDataFile);
            String line = "", output = "";
            List<String> favSongs = new List<String>();
            List<String> bannedKeywords = new List<String>();
            Dictionary<String, String> requestSystem = new Dictionary<String, String>();
            Dictionary<String, String> quotesSystem = new Dictionary<String, String>();
            Dictionary<String, String> sfxSystem = new Dictionary<String, String>();
            Dictionary<String, String> adventureSystem = new Dictionary<String, String>();
            Dictionary<String, String> currencySystem = new Dictionary<String, String>();
            Dictionary<String, String> imageSystem = new Dictionary<String, String>();
            List<Command> commands = new List<Command>();
            List<User> users = new List<User>();

            output += "{\r";
            Console.WriteLine("Parsing Config");
            StreamReader reader = new StreamReader(configFile);
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Equals(""))
                {
                    break;
                }
                if (line.Contains("channel="))
                {
                    String temp = getFollowingText(line, '=');
                    output = writeToString(output, "channel", temp);
                    output = writeToString(output, "streamer", temp.Substring(1));
                }
                else if (line.Contains("oauth="))
                {
                    output = writeToString(output, "oauth", getFollowingText(line, '='));
                }
                else if (line.Contains("googleSheet="))
                {
                    output = writeToString(output, "spreadsheetId", getFollowingText(line, '='));
                }
                else if (line.Contains("botName="))
                {
                    output = writeToString(output, "botName", getFollowingText(line, '='));
                }
                else if (line.Contains("regulars="))
                {
                    String temp = getFollowingText(line, '=');
                    subs = temp.Split(',').ToList();
                }
                else if (line.Contains("favSongs="))
                {
                    String temp = getFollowingText(line, '=');
                    favSongs = temp.Split(',').ToList();
                }
                else if (line.Contains("numOfSongsToDisplay="))
                {
                    requestSystem.Add("numOfSongsToDisplay", getFollowingText(line, '='));
                }
                else if (line.Contains("timer="))
                {
                    output = writeToString(output, "timerTotal", getFollowingText(line, '='));
                }
                else if (line.Contains("numOfSongsInQueuePerUser="))
                {
                    requestSystem.Add("numOfSongsInQueuePerUser", getFollowingText(line, '='));
                }
                else if (line.Contains("maxSonglistLength="))
                {
                    requestSystem.Add("maxSonglistLength", getFollowingText(line, '='));
                }
                else if (line.Contains("mustFollowToRequest="))
                {
                    requestSystem.Add("mustFollowToRequest", getFollowingText(line, '='));
                }
                else if (line.Contains("displayIfUserIsHere="))
                {
                    requestSystem.Add("displayIfUserIsHere", getFollowingText(line, '='));
                }
                else if (line.Contains("displaySonglistOneLine="))
                {
                    requestSystem.Add("displayOneLine", getFollowingText(line, '='));
                }
                else if (line.Contains("requestsOn="))
                {
                    requestSystem.Add("requestsTrigger", getFollowingText(line, '='));
                }
                else if (line.Contains("bannedKeywords="))
                {
                    String temp = getFollowingText(line, '=');
                    bannedKeywords = temp.Split(',').ToList();
                }
                else if (line.Contains("whispersOn="))
                {
                    requestSystem.Add("whisperToUser", getFollowingText(line, '='));
                }
                else if (line.Contains("quotesOn="))
                {
                    quotesSystem.Add("quotesOn", getFollowingText(line, '='));
                }
                else if (line.Contains("sfxTimer="))
                {
                    sfxSystem.Add("sfxTimer", getFollowingText(line, '='));
                }
                else if (line.Contains("amountResult="))
                {
                    output = writeToString(output, "amountResult", getFollowingText(line, '='));
                }
                else if (line.Contains("minigameTimer="))
                {
                    output = writeToString(output, "minigameTimer", getFollowingText(line, '='));
                }
                else if (line.Contains("startupMessage="))
                {
                    output = writeToString(output, "startupMessage", getFollowingText(line, '='));
                }
                else if (line.Contains("minigameEndMessage="))
                {
                    output = writeToString(output, "minigameEndMessage", getFollowingText(line, '='));
                }
                else if (line.Contains("subOnlyRequests="))
                {
                    requestSystem.Add("subOnlyRequests", getFollowingText(line, '='));
                }
                else if (line.Contains("directInputRequests="))
                {
                    requestSystem.Add("direquests", getFollowingText(line, '='));
                }
                else if (line.Contains("youtubeLinkRequests="))
                {
                    requestSystem.Add("ylrequests", getFollowingText(line, '='));
                }
                else if (line.Contains("maxSongLimitOn="))
                {
                    requestSystem.Add("maxSongLength", getFollowingText(line, '='));
                }
                else if (line.Contains("maxSongDuration="))
                {
                    requestSystem.Add("maxSongLengthInMinutes", getFollowingText(line, '='));
                }
                else if (line.Contains("adventureCoolDownTime="))
                {
                    adventureSystem.Add("adventureCoolDown", getFollowingText(line, '='));
                }
                else if (line.Contains("adventureMinReward="))
                {
                    adventureSystem.Add("adventurePointsMin", getFollowingText(line, '='));
                }
                else if (line.Contains("adventureMaxReward="))
                {
                    adventureSystem.Add("adventurePointsMax", getFollowingText(line, '='));
                }
                else if (line.Contains("giveawaycommandname="))
                {
                    output = writeToString(output, "giveawaycommandname", getFollowingText(line, '='));
                }
                else if (line.Contains("currencyName="))
                {
                    currencySystem.Add("currencyName", getFollowingText(line, '='));
                }
                else if (line.Contains("currencyPerMinute="))
                {
                    currencySystem.Add("currencyPerMinute", getFollowingText(line, '='));
                }
                else if (line.Contains("maxGamble="))
                {
                    currencySystem.Add("maxGamble", getFollowingText(line, '='));
                }
                else if (line.Contains("currencyCoolDownMinutes="))
                {
                    currencySystem.Add("gambleCoolDownMinutes", getFollowingText(line, '='));
                }
                else if (line.Contains("currencyToggle="))
                {
                    currencySystem.Add("toggle", getFollowingText(line, '='));
                }
                else if (line.Contains("currencyCommandName="))
                {
                    currencySystem.Add("currencyCommand", getFollowingText(line, '='));
                }
                else if (line.Contains("vipSongCost="))
                {
                    currencySystem.Add("vipSongCost", getFollowingText(line, '='));
                }
                else if (line.Contains("vipSongToggle="))
                {
                    currencySystem.Add("vipSongToggle", getFollowingText(line, '='));
                    requestSystem.Add("vipSongToggle", getFollowingText(line, '='));
                }
                else if (line.Contains("gambleToggle="))
                {
                    currencySystem.Add("gambleToggle", getFollowingText(line, '='));
                }
                else if (line.Contains("adventureToggle="))
                {
                    output = writeToString(output, "adventureToggle", getFollowingText(line, '='));
                }
                else if (line.Contains("minigameOn="))
                {
                    output = writeToString(output, "minigameOn", getFollowingText(line, '='));
                }
                else if (line.Contains("endMessage="))
                {
                    output = writeToString(output, "endMessage", getFollowingText(line, '='));
                }
                else if (line.Contains("vipRedeemCoolDownMinutes="))
                {
                    currencySystem.Add("vipRedeemCoolDownMinutes", getFollowingText(line, '='));
                }
                else if (line.Contains("autoShoutoutOnHost="))
                {
                    output = writeToString(output, "autoShoutoutOnHost", getFollowingText(line, '='));
                }
                else if (line.Contains("quotesModOnly="))
                {
                    quotesSystem.Add("quotesModOnly", getFollowingText(line, '='));
                }
                else if (line.Contains("imageDisplayTimeSeconds="))
                {
                    imageSystem.Add("imageDisplayTimeSeconds", getFollowingText(line, '='));
                }
                else if (line.Contains("imageCoolDown="))
                {
                    imageSystem.Add("imageCoolDown", getFollowingText(line, '='));
                }
                else if (line.Contains("openImageWindowOnStart="))
                {
                    imageSystem.Add("openImageWindowOnStart", getFollowingText(line, '='));
                }
                else if (line.Contains("sfxOverallCoolDown="))
                {
                    sfxSystem.Add("sfxOverallCoolDown", getFollowingText(line, '='));
                }
                else if (line.Contains("imagesOverallCoolDown="))
                {
                    imageSystem.Add("imageOverallCoolDown", getFollowingText(line, '='));
                }
                else if (line.Contains("followersTextFile="))
                {
                    output = writeToString(output, "followersTextFile", getFollowingText(line, '='));
                }
                else if (line.Contains("subTextFile="))
                {
                    output = writeToString(output, "subTextFile", getFollowingText(line, '='));
                }
                else if (line.Contains("followerMessage="))
                {
                    output = writeToString(output, "followerMessage", getFollowingText(line, '='));
                }
                else if (line.Contains("subMessage="))
                {
                    output = writeToString(output, "subMessage", getFollowingText(line, '='));
                }
                else if (line.Contains("rankupUnitCost="))
                {
                    currencySystem.Add("rankupUnitCost", getFollowingText(line, '='));
                }
                else if (line.Contains("subCreditRedeemCost="))
                {
                    currencySystem.Add("subCreditRedeemCost", getFollowingText(line, '='));
                }
                else if (line.Contains("creditsPerSub="))
                {
                    currencySystem.Add("creditsPerSub", getFollowingText(line, '='));
                }
                else if (line.Contains("requestCommands="))
                {
                    commands.Add(new Command("requestComm", getLevel(line), getCommandNames(line).Split(',').ToArray()));
                }
                else if (line.Contains("favSongCommand="))
                {
                    commands.Add(new Command("favSongComm", getLevel(line), getCommandNames(line).Split(',').ToArray()));
                }
                else if (line.Contains("requestsTrigger="))
                {
                    commands.Add(new Command("triggerRequestsComm", getLevel(line), getCommandNames(line).Split(',').ToArray()));
                }
                else if (line.Contains("songlistCommands="))
                {
                    commands.Add(new Command("songlistComm", getLevel(line), getCommandNames(line).Split(',').ToArray()));
                }
                else if (line.Contains("getTotalSongsCommands="))
                {
                    commands.Add(new Command("getTotalComm", getLevel(line), getCommandNames(line).Split(',').ToArray()));
                }
                else if (line.Contains("getViewerCountCommands="))
                {
                    commands.Add(new Command("getViewerCountCommands", getLevel(line), getCommandNames(line).Split(',').ToArray()));
                }
                else if (line.Contains("addtopCommands="))
                {
                    commands.Add(new Command("addtopComm", getLevel(line), getCommandNames(line).Split(',').ToArray()));
                }
                else if (line.Contains("addvipCommands="))
                {
                    commands.Add(new Command("addvipComm", getLevel(line), getCommandNames(line).Split(',').ToArray()));
                }
                else if (line.Contains("editCommands="))
                {
                    commands.Add(new Command("editComm", getLevel(line), getCommandNames(line).Split(',').ToArray()));
                }
                else if (line.Contains("nextCommands="))
                {
                    commands.Add(new Command("nextComm", getLevel(line), getCommandNames(line).Split(',').ToArray()));
                }
                else if (line.Contains("clearCommands="))
                {
                    commands.Add(new Command("clearComm", getLevel(line), getCommandNames(line).Split(',').ToArray()));
                }
                else if (line.Contains("getCurrentCommands="))
                {
                    commands.Add(new Command("getCurrentComm", getLevel(line), getCommandNames(line).Split(',').ToArray()));
                }
                else if (line.Contains("getNextSongCommands="))
                {
                    commands.Add(new Command("getNextComm", getLevel(line), getCommandNames(line).Split(',').ToArray()));
                }
                else if (line.Contains("randomNextSong="))
                {
                    commands.Add(new Command("randomComm", getLevel(line), getCommandNames(line).Split(',').ToArray()));
                }
                else if (line.Contains("editMySong="))
                {
                    commands.Add(new Command("editSongComm", getLevel(line), getCommandNames(line).Split(',').ToArray()));
                }
                else if (line.Contains("removeMySong="))
                {
                    commands.Add(new Command("removeSongComm", getLevel(line), getCommandNames(line).Split(',').ToArray()));
                }
                else if (line.Contains("adddonatorCommands="))
                {
                    commands.Add(new Command("adddonatorComm", getLevel(line), getCommandNames(line).Split(',').ToArray()));
                }
                else if (line.Contains("mySongPosition="))
                {
                    commands.Add(new Command("songPositionComm", getLevel(line), getCommandNames(line).Split(',').ToArray()));
                }
            }
            Console.WriteLine("Parsing Others");
            reader = new StreamReader(othersFile);
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Equals(""))
                {
                    break;
                }
                output = writeToString(output, line.Substring(0, line.IndexOf('=')), getFollowingText(line, '='));
            }
            output = writeToString(output, "gameStartTime", "0");
            output = writeToString(output, "minigameTriggered", "false");
            output = writeToString(output, "timeFinished", "false");
            output = writeToString(output, "startAdventure", "false");
            output = writeToString(output, "waitForAdventureCoolDown", "false");
            output = writeToString(output, "raffleInProgress", "false");
            output += "\"youtube\": { },\r";
            output += "\"google\": { },\r";

            Console.WriteLine("Parsing TextAdventure");
            output += "\"textAdventure\": {\r";
            foreach (KeyValuePair<string, string> entry in adventureSystem)
            {
                output = writeToString(output, entry.Key, entry.Value);
            }
            output += "\"users\": [],\r";
            output = writeToString(output, "startTimerInMS", "0");
            output = writeToString(output, "adventureStartTime", "120");
            output = writeToString(output, "allowUserAdds", "true");
            output = writeToStringNoComma(output, "enoughPlayers", "false");
            output += "},\r";

            Console.WriteLine("Parsing Currency");
            output += "\"currency\": {\r";
            foreach (KeyValuePair<string, string> entry in currencySystem)
            {
                output = writeToString(output, entry.Key, entry.Value);
            }
            output += "\"ranks\": {\r";
            reader = new StreamReader(ranksFile);
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Equals(""))
                {
                    break;
                }
                output = writeToString(output, line.Substring(0, line.IndexOf('\t')), getFollowingText(line, '\t'));
            }
            output = trimComma(output);
            output += "}\r";
            output += "},\r";

            Console.WriteLine("Parsing Image");
            output += "\"image\": {\r";
            foreach (KeyValuePair<string, string> entry in imageSystem)
            {
                output = writeToString(output, entry.Key, entry.Value);
            }
            output = writeToString(output, "imageStartTime", "0");
            output += "\"userCoolDowns\": { }\r";
            output += "},\r";

            Console.WriteLine("Parsing SoundEffects");
            output += "\"soundEffect\": {\r";
            foreach (KeyValuePair<string, string> entry in sfxSystem)
            {
                output = writeToString(output, entry.Key, entry.Value);
            }
            output = writeToString(output, "SFXstartTime", "0");
            output += "\"userCoolDowns\": { }\r";
            output += "},\r";

            Console.WriteLine("Parsing Quotes");
            output += "\"quote\": {\r";
            foreach (KeyValuePair<string, string> entry in quotesSystem)
            {
                output = writeToString(output, entry.Key, entry.Value);
            }
            output += "\"quotes\": [\r";
            reader = new StreamReader(quotesFile);
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Equals(""))
                {
                    break;
                }
                line = line.Replace("\"-", "\" -");
                line = trimmer.Replace(line, " ");
                line = "\"" + "\\" + line.Replace("\" -", "\\" + "\"" + " -") + " \"";
                output += line + ",\r";
            }
            output = trimComma(output);
            output += "]\r";
            output += "},\r";

            Console.WriteLine("Parsing RequestSystem");
            output += "\"requestSystem\": {\r";
            foreach (KeyValuePair<string, string> entry in requestSystem)
            {
                output = writeToString(output, entry.Key, entry.Value);
            }
            output = writeToString(output, "doNotWriteToHistory", "true");
            output += "\"favSongsPlayedThisStream\": [],\r";
            output += "\"favSongs\": [\r";
            if (favSongs != null && !favSongs[0].Equals(""))
            {
                foreach (String s in favSongs)
                {
                    output += "\"" + s + "\",\r";
                }
                output = trimComma(output);
            }
            output += "],";
            output += "\"bannedKeywords\": [\r";
            if (bannedKeywords != null && !bannedKeywords[0].Equals(""))
            {
                foreach (String s in bannedKeywords)
                {
                    output += "\"" + s + "\",\r";
                }
                output = trimComma(output);
            }
            output += "],";
            for (int i = 0; i < commands.Count; i++)
            {
                output += createJSONCommandWithName(commands[i], "bot");
            }
            output = trimComma(output);
            output += "},\r";

            Console.WriteLine("Parsing sfxCommandList");
            output += "\"sfxCommandList\": [\r";
            reader = new StreamReader(sfxFile);
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Equals(""))
                {
                    break;
                }
                String[] str = new String[1];
                str[0] = line.Substring(0, line.IndexOf('\t'));
                String s = createJSONCommand(new Command(getFollowingText(line, '\t'), "0", str), "sfx"); ;
                output += s;
                allCommands += s;
            }
            output = trimComma(output);
            output += "],\r";

            Console.WriteLine("Parsing userCommandList");
            output += "\"userCommandList\": [\r";
            reader = new StreamReader(commandsFile);
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Equals(""))
                {
                    break;
                }
                String[] str = line.Split('\t');
                String[] s = { str[0] };
                if (str[2] != null)
                {
                    String s2 = createJSONCommand(new Command(str[1], str[2], s), "user");
                    output += s2;
                    allCommands += s2;
                }
                else
                {
                    String s2 = createJSONCommand(new Command(str[1], "0", s), "user");
                    output += s2;
                    allCommands += s2;
                }
                
            }
            output = trimComma(output);
            output += "],\r";

            Console.WriteLine("Parsing timerCommandList");
            output += "\"timerCommandList\": [\r";
            reader = new StreamReader(timedCommandsFile);
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Equals(""))
                {
                    break;
                }
                String[] str = new String[1];
                str[0] = line.Substring(0, line.IndexOf('\t'));
                String s2 = createJSONCommand(new Command(getFollowingText(line, '\t'), "0", str), "timer");
                output += s2;
                allCommands += s2;
            }
            output = trimComma(output);
            output += "],\r";

            Console.WriteLine("Parsing botCommandList");
            output += "\"botCommandList\": [\r";
            for (int i = 0; i < commands.Count; i++)
            {
                String s2 = createJSONCommand(commands[i], "bot");
                output += s2;
                allCommands += s2;
            }
            output = trimComma(output);
            output += "],\r";

            Console.WriteLine("Parsing imageCommandList");
            output += "\"imageCommandList\": [\r";
            reader = new StreamReader(imagesFile);
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Equals(""))
                {
                    break;
                }
                String[] str = new String[1];
                str[0] = line.Substring(0, line.IndexOf('\t'));
                String s2 = createJSONCommand(new Command(getFollowingText(line, '\t'), "0", str), "image");
                if (s2 != null)
                {
                    output += s2;
                    allCommands += s2;
                }
            }
            output = trimComma(output);
            output += "],\r";

            Console.WriteLine("Parsing All Commands List");
            output += "\"commandList\": [\r";
            output += allCommands;
            output += "],\r";

            Console.WriteLine("Parsing users");
            output += "\"users\": [\r";
            reader = new StreamReader(currencyFile);
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Equals(""))
                {
                    break;
                }
                String[] str = line.Split('\t');
                users.Add(new User(str[0], str[1], str[2], str[3]));
            }
            reader = new StreamReader(usersFile);
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Equals(""))
                {
                    break;
                }
                String[] str = line.Split('\t');
                numRequests.Add(str[0], str[1]);
            }
            reader = new StreamReader(purchasedRanksFile);
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Equals(""))
                {
                    break;
                }
                String[] str = line.Split('\t');
                purchasedRanks.Add(str[0], str[1]);
            }
            for (int i = 0; i < users.Count; i++)
            {
                output += createUserJSON(users[i]);
            }
            output = trimComma(output);
            output += "],\r";

            output += "\"gameGuess\": [],\r";
            output += "\"raffleUsers\": [],\r";
            output += "\"allHosts\": [],\r";
            output += "\"gameUser\": [],\r";

            Console.WriteLine("Parsing events");
            output += "\"events\": {\r";
            reader = new StreamReader(eventsFile);
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Equals(""))
                {
                    break;
                }
                String[] str = line.Split('\t');
                output = writeToString(output, str[0], str[1]);
            }
            output = trimComma(output);
            output += "},\r";
            String[] stri = new String[1];
            stri[0] = "!viewers";
            output += createJSONCommandWithName(new Command("getViewerComm", "0", stri), "bot");
            output = trimComma(output);
            output += "}\r";
            writer.Write(output);
            writer.Close();
            Application.Current.Shutdown();

        }



        public String createUserJSON(User user)
        {
            Boolean mod = false, sub = false, follower = false;
            String rank = null, numR = "0";
            String s = "{\r";
            s = writeToString(s, "username", user.name);
            s = writeToString(s, "time", user.hours);
            s = writeToString(s, "points", user.points);
            s = writeToString(s, "subCredits", user.subcredits);
            foreach (String str in mods)
            {
                if (str.Equals(user.name))
                {
                    mod = true;
                    break;
                }
            }
            foreach (String str in subs)
            {
                if (str.Equals(user.name))
                {
                    sub = true;
                    break;
                }
            }
            foreach (KeyValuePair<string, string> entry in numRequests)
            {
                if (entry.Key.ToLower().Equals(user.name))
                {
                    numR = entry.Value;
                }
            }
            foreach (KeyValuePair<string, string> entry in purchasedRanks)
            {
                if (entry.Key.ToLower().Equals(user.name))
                {
                    rank = entry.Value;
                }
            }
            s = writeToString(s, "sub", sub.ToString().ToLower());
            s = writeToString(s, "mod", mod.ToString().ToLower());
            s = writeToString(s, "follower", follower.ToString().ToLower());
            s = writeToString(s, "numRequests", numR);
            s = writeToString(s, "gambleCoolDown", "0");
            s = writeToString(s, "vipCoolDown", "0");
            if (rank != null)
            {
                s = writeToString(s, "rank", rank);
            }
            s = writeToString(s, "gaveSpot", "false");
            s = trimComma(s);
            s += "},\r";
            return s;
        }

        public String createJSONCommandWithName(Command command, String type)
        {
            String s = "";
            s += "\"" + formatQuotes(command.name) + "\": {\r";
            s += "\"commandType\": \"" + type + "\",\r";
            s += "\"level\":" + command.level + ",\r";
            s += "\"input\": [\r";
            for (int i = 0; i < command.commands.Length; i++)
            {
                s += "\"" + command.commands[i] + "\",\r";
            }
            s = trimComma(s);
            s += "],\r";
            s += "\"toggle\": true\r";
            s += "},\r";
            return s;
        }

        public String createJSONCommand(Command command, String type)
        {
            String s = "{\r";
            s += "\"output\": \"" + formatQuotes(command.name) + "\",\r";
            s += "\"commandType\": \"" + type + "\",\r";
            s += "\"level\":" + command.level + ",\r";
            s += "\"input\": [\r";
            for (int i = 0; i < command.commands.Length; i++)
            {
                s += "\"" + command.commands[i] + "\",\r";
            }
            s = trimComma(s);
            s += "],\r";
            s += "\"toggle\": true\r";
            s += "},\r";
            return s;
        }

        public String formatQuotes(String s)
        {
            if (s.Contains("\\"))
            {
                s = s.Replace("\\", "\\" + "\\");
            }
            if (s.Contains("\""))
            {
                s = s.Replace("\"", "\\" + "\"");
            }
            return s;
        }

        public String trimComma(String output)
        {
            if (output.EndsWith(",\r"))
            {
                output = output.Substring(0, output.LastIndexOf(',')) + "\r";
                return output;
            }
            return output;
        }


    }
}
