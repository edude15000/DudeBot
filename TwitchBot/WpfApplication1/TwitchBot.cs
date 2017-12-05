using System;
using System.Collections.Generic;
using System.IO;
using TwitchLib;
using TwitchLib.Models.Client;
using TwitchLib.Events.Client;
using System.Threading;
using Newtonsoft.Json.Linq;
using WpfApplication1;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TwitchLib.Services;
using TwitchLib.Events.Services.FollowerService;
using TwitchLib.Events.PubSub;

public class TwitchBot
{
    [JsonIgnore]
    public TwitchClient client;
    [JsonIgnore]
    ConnectionCredentials credentials;
    [JsonIgnore]
    public YoutubeHandler youtube;
    [JsonIgnore]
    public GoogleHandler google;
    [JsonIgnore]
    private static TwitchAPI api;
    [JsonIgnore]
    private static FollowerService service;

    public String oauth { get; set; }
    public String streamer { get; set; }
    public String channel { get; set; }
    public String botName { get; set; }
    public String followersTextFile { get; set; }
    public String subTextFile { get; set; }
    public String followerMessage { get; set; }
    public String subMessage { get; set; }
    public String subOnlyRequests { get; set; }
    public String minigameEndMessage { get; set; }
    public String giveawaycommandname { get; set; }
    public String spreadsheetId { get; set; }
    public String botColor { get; set; }
    public String endMessage { get; set; }
    public String startupMessage { get; set; }
    public int counter1, counter2, counter3, counter4, counter5;
    public int minigameTimer { get; set; }
    public int timerTotal { get; set; }
    [JsonIgnore]
    public long gameStartTime { get; set; }
    public Boolean minigameTriggered { get; set; } = false;
    public Boolean timeFinished { get; set; } = false;
    public Boolean minigameOn { get; set; }
    public Boolean adventureToggle { get; set; }
    public Boolean startAdventure { get; set; } = false;
    public Boolean waitForAdventureCoolDown { get; set; } = false;
    public Boolean raffleInProgress { get; set; } = false;
    public Boolean autoShoutoutOnHost { get; set; }
    public TextAdventure textAdventure { get; set; }
    public Currency currency { get; set; }
    public Image image { get; set; }
    public SoundEffect soundEffect { get; set; }
    public Quote quote { get; set; }
    public RequestSystem requestSystem { get; set; }
    public List<Command> sfxCommandList { get; set; } = new List<Command>();
    public List<Command> userCommandList { get; set; } = new List<Command>();
    public List<Command> timerCommandList { get; set; } = new List<Command>();
    public List<Command> botCommandList { get; set; } = new List<Command>();
    public List<Command> imageCommandList { get; set; } = new List<Command>();
    public List<Command> commandList { get; set; } = new List<Command>();
    public List<BotUser> users { get; set; } = new List<BotUser>();
    [JsonIgnore]
    public List<Double> gameGuess { get; set; } = new List<Double>();
    public List<String> raffleUsers { get; set; } = new List<String>();
    [JsonIgnore]
    public List<String> gameUser { get; set; } = new List<String>();
    public List<String> extraCommandNames { get; set; } = new List<String>();
    public Dictionary<String, String> events { get; set; } = new Dictionary<String, String>();
    public Command getViewerComm { get; set; }
    
    public event PropertyChangedEventHandler PropertyChanged;
    public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public virtual bool Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
    {
        if (object.Equals(storage, value))
            return false;
        storage = value;
        OnPropertyChanged(string.Empty);
        return true;
    }
    
    public void botStartUpAsync()
    { // Starts bot up, calls necessary threads and methods
        try
        {
            if (channel.Contains("#"))
            {
                channel = channel.Replace("#", "");
            }
            credentials = new ConnectionCredentials(botName, oauth);
            client = new TwitchClient(credentials, channel);
            api = new TwitchAPI(Utils.twitchClientID);
            service = new FollowerService(api);
            service.OnNewFollowersDetected += onNewFollower;
            service.StartService();
            TwitchPubSub pubsub = new TwitchPubSub();
            pubsub.OnBitsReceived += onPubSubBitsReceived;
            pubsub.Connect();
            client.OnJoinedChannel += onJoinedChannel;
            client.OnMessageReceived += onMessageReceived;
            client.OnChatCommandReceived += onChatCommandReceived;
            client.OnWhisperReceived += onWhisperReceived;
            client.OnNewSubscriber += onNewSubscriber;
            client.OnUserJoined += onUserJoined;
            client.OnReSubscriber += onReSubscriber;
            client.OverrideBeingHostedCheck = true;
            client.OnBeingHosted += onBeingHosted;
            client.Connect();
            setClasses();
            Console.WriteLine("DudeBot Version: " + Utils.version + " Release Date: " + Utils.releaseDate);
            Utils.writeVersion();
            textAdventure.startAdventuring(new List<String>(), (int)textAdventure.adventureStartTime * 1000);
            resetAllCommands();
            threads();
        }
        catch (Exception e1)
        {
            Console.WriteLine(e1.ToString());
            Utils.errorReport(e1);
        }
    }

    private void onBeingHosted(object sender, OnBeingHostedArgs e) // TODO : Add raid message for more than x people
    {
        if (autoShoutoutOnHost && !e.IsAutoHosted)
        {
            if (e.Viewers > 4)
            {
                client.SendMessage("Thanks for the " + e.Viewers + " viewer host! "
                    + (userVariables("$shoutout", "#" + streamer, streamer, e.HostedByChannel, "!shoutout " + e.HostedByChannel, true)));
            }
            else
            {
                client.SendMessage("Thanks for the host! "
                    + (userVariables("$shoutout", "#" + streamer, streamer, e.HostedByChannel, "!shoutout " + e.HostedByChannel, true)));
            }
        }
    }

    private void onReSubscriber(object sender, OnReSubscriberArgs e)
    {
        client.SendMessage(e.ReSubscriber.DisplayName + " just resubscribed for " + e.ReSubscriber.Months + " months! Thank you!"); // TODO : Create custom message
        foreach (BotUser botUser in users)
        {
            if (botUser.username.Equals(e.ReSubscriber.DisplayName, StringComparison.InvariantCultureIgnoreCase))
            {
                botUser.subCredits += currency.creditsPerSub;
                botUser.sub = true;
                break;
            }
        }
    }

    private void onPubSubBitsReceived(object sender, OnBitsReceivedArgs e)
    {
        client.SendMessage("Just received " + e.BitsUsed + " bits from " + e.Username + ". That brings their total to " + e.TotalBitsUsed + " bits!"); // TODO : TEST!
    }

    private void onNewFollower(object sender, OnNewFollowersDetectedArgs e) // TODO : TEST!
    {
        if (followerMessage.Contains("$user"))
        {
            client.SendMessage(followerMessage.Replace("$user", String.Join(", ", e.NewFollowers)));
        }
        else
        {
            client.SendMessage(followerMessage);
        }
    }

    public async void checkAtBeginningAsync()
    {
        var allSubscriptions = await api.Channels.v5.GetAllSubscribersAsync(channel);
        var channelFollowers = await api.Channels.v5.GetChannelFollowersAsync(channel);
        foreach (BotUser user in users)
        {
            Boolean follows = false, isSubbed = false;
            if (user.follower || user.sub)
            {
                foreach (var u in allSubscriptions)
                {
                    if (u.User.Name.Equals(user.username, StringComparison.InvariantCultureIgnoreCase))
                    {
                        follows = true;
                    }
                }
            }
            if (user.sub)
            {
                foreach (var u in channelFollowers.Follows)
                {
                    if (u.User.Name.Equals(user.username, StringComparison.InvariantCultureIgnoreCase))
                    {
                        isSubbed = true;
                    }
                }
            }
            user.follower = follows;
            user.sub = isSubbed;
        }
    }
    
    private void onMessageReceived(object sender, OnMessageReceivedArgs e)
    {
        // TODO ?
    }
    
    public void botDisconnect()
    {
        client.Disconnect();
        client.LeaveChannel(channel);
    }

    private void onJoinedChannel(object sender, OnJoinedChannelArgs e)
    {
        client.SendMessage(startupMessage);
    }

    private void onWhisperReceived(object sender, OnWhisperReceivedArgs e)
    {
        // TODO ?
    }

    private void onNewSubscriber(object sender, OnNewSubscriberArgs e)
    {
        if (subMessage.Contains("$user"))
        {
            client.SendMessage(subMessage.Replace("$user", e.Subscriber.DisplayName));
        }
        else
        {
            client.SendMessage(streamer + " : " + subMessage);
        }
        foreach (BotUser botUser in users)
        {
            if (botUser.username.Equals(e.Subscriber.DisplayName, StringComparison.InvariantCultureIgnoreCase))
            {
                botUser.subCredits += currency.creditsPerSub;
                botUser.sub = true; 
                break;
            }
        }
    }
    
    public void setClasses()
    { // Resets classes within bot using bot as passed variables to request system and
      // quote, resets google
        requestSystem.bot = this;
        quote.bot = this;
        currency.users = users;
        google = new GoogleHandler(spreadsheetId);
        youtube = new YoutubeHandler();
        textAdventure.setUpText();
        requestSystem.songList = Utils.loadSongs();
        requestSystem.formattedTotalTime = requestSystem.formatTotalTime();
    }

    public void resetAllCommands()
    { // Sets all command types for quicker type checking, sets all command names
        List<Command> commands = new List<Command>(); // Removes any null commands that come up due to early bugs
        foreach (Command command in commandList)
        {
            if (command != null)
            {
                commands.Add(command);
            }
        }
        commandList = commands;
        botCommandList = getCommands("bot");
        sfxCommandList = getCommands("sfx");
        userCommandList = getCommands("user");
        timerCommandList = getCommands("timer");
        imageCommandList = getCommands("image");
        extraCommandNames = setExtraCommandNames();
    }

    public void syncFileTimerThread()
    {
        try
        {
            Thread.Sleep(1000);
        }
        catch (Exception e1)
        {
            Console.WriteLine(e1.ToString());
            Utils.errorReport(e1);
        }
        int count = 0;
        double start_time = Environment.TickCount;
        int i = 0;
        while (true)
        {
            try
            {
                if ((Environment.TickCount - start_time) >= (timerTotal * 60000))
                { // Prints timed
                  // commands when
                  // needed
                    start_time = Environment.TickCount;
                    if (timerCommandList[i].output.Equals("$songlist"))
                    {
                        try
                        {
                            requestSystem.songlistTimer(channel);
                        }
                        catch (Exception e)
                        {
                            Utils.errorReport(e);
                            Console.WriteLine(e.ToString());
                        }
                    }
                    else
                    {

                        client.SendMessage(timerCommandList[i].output);
                    }
                    i++;
                    if (i >= count)
                    {
                        i = 0;
                    }
                }
                Thread.Sleep(1000);
            }
            catch (Exception e)
            {
                Utils.errorReport(e);
                Console.WriteLine(e.ToString());
            }
        }
    }

    public void bonusAllThread()
    {
        while (currency.toggle)
        {
            try
            {
                if (!currency.toggle)
                {
                    break;
                }
                Thread.Sleep(60000);
                currency.bonusall(Utils.getAllViewers(streamer), true, 0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Utils.errorReport(e);
            }
        }
    }
    
    public void saveThread()
    {
        while (true)
        {
            try
            {
                Thread.Sleep(10000);
                try
                {
                    Utils.saveData(this);
                }
                catch (Exception e)
                {
                    Utils.errorReport(e);
                    Console.WriteLine(e.ToString());
                }
            }
            catch (Exception)
            {
            }
        }
    }

    public void adventureThread(String sender)
    {
        textAdventure.start(sender);
        if (textAdventure.enoughPlayers)
        {
            List<String> winners = textAdventure.selectWinners();
            String winnerString = "";
            if (winners.Count > 0)
            {
                for (int i = 0; i < winners.Count; i++)
                {
                    winnerString += winners[i] + ", ";
                }
                winnerString = winnerString.Substring(0, winnerString.Trim().Length - 1);
            }
            String winMessage = textAdventure.winningMessage(winners);
            if (winMessage.Contains("$users"))
            {
                winMessage = winMessage.Replace("$users", winnerString);
            }
            if (winMessage.Contains("$user"))
            {
                winMessage = winMessage.Replace("$user", winnerString);
            }
            client.SendMessage(winMessage);
            int adventurePoints = textAdventure.adventurePointsMax;
            if (textAdventure.adventurePointsMax != textAdventure.adventurePointsMin)
            {
                adventurePoints = new Random().Next(
                        textAdventure.adventurePointsMax - textAdventure.adventurePointsMin)
                        + textAdventure.adventurePointsMin;
            }
            if (currency.toggle)
            {
                foreach (String str in winners)
                {
                    client.SendMessage(currency.bonus(str, adventurePoints));
                }
            }
        }
        else
        {
            client.SendMessage("Not enough people joined the adventure, at least 3 people are required to start an adventure. Try again later!");
        }
        startAdventure = false;
        textAdventure.adventureStartTime = Environment.TickCount;
        waitForAdventureCoolDown = true;
    }

    public void sfxThread(String message, String sender)
    {
        soundEffect.sfxCOMMANDS(message, channel, sender, sfxCommandList);
    }

    public void imageThread(String message, String sender)
    {
        image.imageCOMMANDS(message, channel, sender, imageCommandList);
    }

    public void rouletteThread(String sender)
    {
        client.SendMessage("/me places the revolver to " + sender + "'s head...");
        try
        {
            Thread.Sleep(3000);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            Utils.errorReport(e);
        }
        if ((new Random().Next(7) + 1) == 6)
        {
            client.SendMessage("/timeout " + sender + " 1");
            client.SendMessage("The gun fires and " + sender + " lies dead in chat.");
        }
        else
        {
            client.SendMessage("The trigger is pulled, and the revolver clicks. "
                    + sender + " has lived to survive roulette!");
        }
    }

    public void threads()
    {
        new Thread(new ThreadStart(syncFileTimerThread)).Start();
        if (currency.toggle)
        {
            new Thread(new ThreadStart(bonusAllThread)).Start();
        }
        new Thread(new ThreadStart(saveThread)).Start();
    }

    public void clearUpTempData()
    { // Resets all bot startup data
        gameStartTime = 0;
        waitForAdventureCoolDown = false;
        minigameTriggered = false;
        startAdventure = false;
        textAdventure.allowUserAdds = true;
        textAdventure.enoughPlayers = false;
        textAdventure.startTimerInMS = 0;
        image.imageStartTime = (long)0;
        image.userCoolDowns.Clear();
        soundEffect.SFXstartTime = 0;
        soundEffect.userCoolDowns.Clear();
        requestSystem.favSongsPlayedThisStream.Clear();
        requestSystem.doNotWriteToHistory = true;
        gameGuess.Clear();
        foreach (BotUser botUser in users)
        {
            botUser.gaveSpot = false;
            botUser.vipCoolDown = 0;
            botUser.gambleCoolDown = 0;
        }
        // TODO: reset all the good stuff as needed
    }

    public List<Command> getCommands(String type)
    { // Gets a list of all 'type' commands
        List<Command> list = new List<Command>();
        foreach (Command c in commandList)
        {
            if (c.commandType.Equals(type))
            {
                list.Add(c);
            }
        }
        return list;
    }

    // Sets all commands for !addcom, !editcom, !removecom
    public List<String> setExtraCommandNames()
    {
        String[] result = { "!givespot", "!regnext", "!nextreg", "!regularnext", "!nextregular", "!quote", "!addquote",
                "!quotes", "!minigame", "!startgame", "!guess", "!endgame", "!sfx", "!images", "!totalrequests",
                "!toprequester", "!setcounter1", "!setcounter2", "!setcounter3", "!setcounter4", "!setcounter5",
                "!addcom", "!botcolor", "!colorbot", "!adventure", "!bonus", "!bonusall", giveawaycommandname,
                "!gamble", "!currency", currency.currencyCommand, "!vipsong", "!vipsongon", "!vipsongoff", "!info",
                "!rank", "!join", "!leaderboards", "!removesong", "!promote", "!rankup", "!editquote", "!changequote",
                "!removequote", "!deletequote", "!subcredits", "!givecredits", "!subsong" };
        return new List<String>(result);
        // Add TO AS NEEDED
    }

    // Increments or decrements user's request amount
    public void addUserRequestAmount(String sender, Boolean op)
    {
        foreach (BotUser botUser in users)
        {
            if (botUser.username.Equals(sender, StringComparison.InvariantCultureIgnoreCase))
            {
                if (op)
                {
                    botUser.numRequests += 1;
                }
                else
                {
                    botUser.numRequests -= 1;
                    if (botUser.numRequests < 0)
                    {
                        botUser.numRequests = 0;
                    }
                }
                break;
            }
        }
        Utils.saveData(this);
    }


    private void onChatCommandReceived(object s, OnChatCommandReceivedArgs e)
    {
        String sender = e.Command.ChatMessage.Username;
        String message = e.Command.ChatMessage.Message;
        if (!message.StartsWith("!"))
        {
            return;
        }
        if (sender.Equals(botName, StringComparison.InvariantCultureIgnoreCase))
        {
            return;
        }
        String temp = message.ToLower();
        if (minigameTriggered && !timeFinished)
        {
            if (gameStartTime + minigameTimer < Environment.TickCount)
            {
                timeFinished = true;
            }
        }

        // SFX
        for (int i = 0; i < sfxCommandList.Count; i++)
        {
            if (temp.StartsWith(sfxCommandList[i].input[0]))
            {
                var t = new Thread(() => sfxThread(message, sender));
                t.Start();
            }
        }

        // Images
        for (int i = 0; i < imageCommandList.Count; i++)
        {
            if (temp.StartsWith(imageCommandList[i].input[0]))
            {
                var t = new Thread(() => imageThread(message, sender));
                t.Start();
            }
        }
        // USER COMMANDS
        for (int i = 0; i < userCommandList.Count; i++)
        {
            if (temp.StartsWith(userCommandList[i].input[0] + " ")
                    || temp.Equals(userCommandList[i].input[0], StringComparison.InvariantCultureIgnoreCase))
            {

                userCOMMANDS(message, channel, sender);
                return;
            }
        }
        // USER TIMER COMMANDS
        for (int i = 0; i < timerCommandList.Count; i++)
        {
            if (temp.StartsWith(timerCommandList[i].input[0] + " ")
                    || temp.Equals(timerCommandList[i].input[0], StringComparison.InvariantCultureIgnoreCase))
            {

                userCOMMANDS(message, channel, sender);
                return;
            }
        }
        // SFX COMMAND
        if (temp.Equals("!sfx"))
        {
            if (sfxCommandList.Count == 0)
            {
                return;
            }
            String line = "";
            for (int i = 0; i < sfxCommandList.Count; i++)
            {
                line = line + sfxCommandList[i].input[0] + ", ";
            }
            line = line.Substring(0, line.Length - 2).Trim();
            client.SendMessage("Sound Effects: " + line);
            return;
        }
        // Images COMMAND
        if (temp.Equals("!images"))
        {
            if (imageCommandList.Count == 0)
            {
                return;
            }
            String line = "";
            for (int i = 0; i < imageCommandList.Count; i++)
            {
                line = line + imageCommandList[i].input[0] + ", ";
            }
            line = line.Substring(0, line.Length - 2).Trim();
            client.SendMessage("Images: " + line);
            return;
        }
        // REQUEST
        for (int i = 0; i < requestSystem.requestComm.input.Length; i++)
        {
            if (temp.StartsWith(requestSystem.requestComm.input[i] + " ")
                    || temp.Equals(requestSystem.requestComm.input[i], StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    requestSystem.requestCOMMAND(message, channel, sender);
                }
                catch (Exception e1)
                {
                    Console.WriteLine(e1.ToString());
                    Utils.errorReport(e1);
                }
                return;
            }
        }
        // SKIP TO NEXT SONG
        for (int i = 0; i < requestSystem.nextComm.input.Length; i++)
        {
            if (temp.Equals(requestSystem.nextComm.input[i], StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    requestSystem.nextCOMMAND(message, channel, sender);
                }
                catch (Exception e1)
                {
                    Console.WriteLine(e1.ToString());
                    Utils.errorReport(e1);
                }
                return;
            }
        }
        // CLEAR
        for (int i = 0; i < requestSystem.clearComm.input.Length; i++)
        {
            if (temp.Equals(requestSystem.clearComm.input[i], StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    requestSystem.clearCOMMAND(message, channel, sender);
                }
                catch (IOException e1)
                {
                    Console.WriteLine(e1.ToString());
                    Utils.errorReport(e1);
                }
                return;
            }
        }
        // EDIT COMMAND
        for (int i = 0; i < requestSystem.editComm.input.Length; i++)
        {
            if (temp.StartsWith(requestSystem.editComm.input[i] + " "))
            {
                try
                {
                    requestSystem.editCOMMAND(message, channel, sender);
                }
                catch (Exception e1)
                {
                    Console.WriteLine(e1.ToString());
                    Utils.errorReport(e1);
                }
                return;
            }
        }
        // Add VIP COMMAND
        for (int i = 0; i < requestSystem.addvipComm.input.Length; i++)
        {
            if (temp.StartsWith(requestSystem.addvipComm.input[i] + " "))
            {
                try
                {
                    requestSystem.addvipCOMMAND(message, channel, sender);
                }
                catch (Exception e1)
                {
                    Console.WriteLine(e1.ToString());
                    Utils.errorReport(e1);
                }
                return;
            }
        }
        // Add TOP COMMAND
        for (int i = 0; i < requestSystem.addtopComm.input.Length; i++)
        {
            if (temp.StartsWith(requestSystem.addtopComm.input[i] + " "))
            {
                try
                {
                    requestSystem.addtopCOMMAND(message, channel, sender);
                }
                catch (Exception e1)
                {
                    Console.WriteLine(e1.ToString());
                    Utils.errorReport(e1);
                }
                return;
            }
        }
        // getTotalSongs COMMAND
        for (int i = 0; i < requestSystem.getTotalComm.input.Length; i++)
        {
            if (temp.Equals(requestSystem.getTotalComm.input[i], StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    requestSystem.getTotalSongCOMMAND(message, channel, sender);
                }
                catch (Exception e1)
                {
                    Console.WriteLine(e1.ToString());
                    Utils.errorReport(e1);
                }
                return;
            }
        }
        // SONG LIST
        for (int i = 0; i < requestSystem.songlistComm.input.Length; i++)
        {
            if (temp.Equals(requestSystem.songlistComm.input[i]))
            {
                try
                {
                    requestSystem.songlistCOMMAND(message, channel, sender);
                }
                catch (Exception e1)
                {
                    Console.WriteLine(e1.ToString());
                    Utils.errorReport(e1);
                }
                return;
            }
        }
        // CURRENT SONG
        for (int i = 0; i < requestSystem.getCurrentComm.input.Length; i++)
        {
            if (message.Equals(requestSystem.getCurrentComm.input[i], StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    requestSystem.getCurrentSongCOMMAND(message, channel, sender);
                }
                catch (Exception e1)
                {
                    Console.WriteLine(e1.ToString());
                    Utils.errorReport(e1);
                }
                return;
            }
        }
        // TRIGGER REQUESTS
        for (int i = 0; i < requestSystem.triggerRequestsComm.input.Length; i++)
        {
            if (temp.StartsWith(requestSystem.triggerRequestsComm.input[i] + " "))
            {
                try
                {
                    requestSystem.triggerRequestsCOMMAND(message, channel, sender);
                }
                catch (IOException e1)
                {
                    Utils.errorReport(e1);
                    Console.WriteLine(e1.ToString());
                }
                return;
            }
        }
        // GET NEXT SONG
        for (int i = 0; i < requestSystem.getNextComm.input.Length; i++)
        {
            if (message.Equals(requestSystem.getNextComm.input[i], StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    requestSystem.getNextSongCOMMAND(message, channel, sender);
                }
                catch (Exception e1)
                {
                    Console.WriteLine(e1.ToString());
                    Utils.errorReport(e1);
                }
                return;
            }
        }
        // Currency Commands
        if (message.Equals("!currency on", StringComparison.InvariantCultureIgnoreCase))
        {
            if (sender.Equals(Utils.botMaker, StringComparison.InvariantCultureIgnoreCase) || sender.Equals(streamer))
            {
                if (!currency.toggle)
                {
                    try
                    {
                        triggerCurrency(true, channel);
                    }
                    catch (IOException e1)
                    {
                        Utils.errorReport(e1);
                        Console.WriteLine(e1.ToString());
                    }
                    new Thread(new ThreadStart(bonusAllThread)).Start();
                    client.SendMessage("The currency system has been turned on, " + sender + "!");
                }
                else
                {
                    client.SendMessage("The currency system is already on, " + sender + "!");
                }
            }
            return;
        }
        if (message.Equals("!currency off", StringComparison.InvariantCultureIgnoreCase))
        {
            if (sender.Equals(Utils.botMaker, StringComparison.InvariantCultureIgnoreCase) || sender.Equals(streamer))
            {
                try
                {
                    triggerCurrency(false, channel);
                }
                catch (IOException e1)
                {
                    Utils.errorReport(e1);
                    Console.WriteLine(e1.ToString());
                }
            }
            return;
        }
        if (message.Equals(currency.currencyCommand, StringComparison.InvariantCultureIgnoreCase))
        {
            if (currency.toggle)
            {
                client.SendMessage(currency.getCurrency(sender));
            }
            return;
        }
        if (message.Equals("!rank", StringComparison.InvariantCultureIgnoreCase))
        {
            if (currency.toggle)
            {
                client.SendMessage(currency.getRank(sender, streamer, botName));
                return;
            }
        }
        if (message.Trim().StartsWith("!bonus") && !message.Trim().StartsWith("!bonusall"))
        {
            if (currency.toggle)
            {
                if (sender.Equals(Utils.botMaker, StringComparison.InvariantCultureIgnoreCase)
                    || sender.Equals(streamer, StringComparison.InvariantCultureIgnoreCase)
                        || Utils.checkIfUserIsOP(sender, channel, streamer, users))
                {
                    if (message.Equals("!bonus", StringComparison.InvariantCultureIgnoreCase))
                    {
                        client.SendMessage("Type in the format '!bonus user amount'");
                    }
                    String[] tempArray = Utils.getFollowingText(message).Split(' ');
                    String neg = "";
                    if (tempArray[1].StartsWith("-"))
                    {
                        neg = "-";
                        tempArray[1] = tempArray[1].Substring(1);
                    }
                    if (Utils.isInteger(tempArray[1]))
                    {
                        if (tempArray[0].GetType() == typeof(String))
                        {
                            if (tempArray.Length == 2)
                            {
                                if (tempArray[0].StartsWith("@"))
                                {
                                    tempArray[0] = tempArray[0].Replace("@", "");
                                }
                                if (neg.Equals("-"))
                                {

                                    client.SendMessage(currency.bonus(tempArray[0], -1 * Int32.Parse(tempArray[1])));
                                }
                                else
                                {

                                    client.SendMessage(currency.bonus(tempArray[0], Int32.Parse(tempArray[1])));
                                }
                                return;
                            }
                        }
                    }
                    client.SendMessage("Type in the format '!bonus user amount'");
                }
                return;
            }
        }
        if (message.StartsWith("!bonusall "))
        {
            if (currency.toggle)
            {
                if (sender.Equals(Utils.botMaker, StringComparison.InvariantCultureIgnoreCase) || sender.Equals(streamer, StringComparison.InvariantCultureIgnoreCase)
                        || Utils.checkIfUserIsOP(sender, channel, streamer, users))
                {
                    String temp2 = Utils.getFollowingText(message).Trim();
                    String neg = "";
                    if (temp2.StartsWith("-"))
                    {
                        neg = "-";
                        temp2 = temp2.Substring(1);
                    }
                    if (Utils.isInteger(temp2))
                    {
                        if (neg.Equals("-"))
                        {
                            client.SendMessage(currency.bonusall(
                                    Utils.getAllViewers(streamer), false, -1 * Int32.Parse(temp2)));
                        }
                        else
                        {
                            client.SendMessage(currency.bonusall(Utils.getAllViewers(streamer), false, Int32.Parse(temp2)));
                        }
                    }
                    else
                    {
                        client.SendMessage("Please type in the format '!bonusall amount', "
                                + sender + "!");
                    }
                }
                return;
            }
        }
        if (message.Trim().Equals("!gamble on", StringComparison.InvariantCultureIgnoreCase))
        {
            if (sender.Equals(Utils.botMaker) || sender.Equals(streamer)
                    || Utils.checkIfUserIsOP(sender, channel, streamer, users))
            {
                try
                {
                    triggerGamble(true, channel);
                }
                catch (IOException e1)
                {
                    Utils.errorReport(e1);
                    Console.WriteLine(e1.ToString());
                }
            }
            return;
        }
        if (message.Trim().Equals("!gamble off", StringComparison.InvariantCultureIgnoreCase))
        {
            if (sender.Equals(Utils.botMaker) || sender.Equals(streamer)
                    || Utils.checkIfUserIsOP(sender, channel, streamer, users))
            {
                try
                {
                    triggerGamble(false, channel);
                }
                catch (IOException e1)
                {
                    Utils.errorReport(e1);
                    Console.WriteLine(e1.ToString());
                }
            }
            return;
        }
        if (message.Trim().StartsWith("!gamble ") || message.Trim().Equals("!gamble"))
        {
            if (currency.toggle)
            {
                if (message.Equals("!gamble", StringComparison.InvariantCultureIgnoreCase))
                {
                    client.SendMessage("To gamble points, type '!gamble <amount>'. You may gamble every "
                            + currency.gambleCoolDownMinutes
                            + " minutes. There is a  2% chance of tripling your gamble and 38% of doubling it, "
                            + sender + "!");
                    return;
                }
                String temp2 = Utils.getFollowingText(message).Trim();
                if (Utils.isInteger(temp2))
                {
                    client.SendMessage(currency.gamble(sender, Int32.Parse(temp2)));
                }
                else
                {
                    client.SendMessage("Please type in the format '!gamble amount', " + sender
                            + "!");
                }
                return;
            }
        }
        if (message.Equals("!vipsongoff", StringComparison.InvariantCultureIgnoreCase))
        {
            try
            {
                requestSystem.triggerVIPs(false, channel);
            }
            catch (IOException e1)
            {
                Utils.errorReport(e1);
                Console.WriteLine(e.ToString());
            }
            return;
        }
        if (message.Equals("!vipsongon", StringComparison.InvariantCultureIgnoreCase))
        {
            try
            {
                requestSystem.triggerVIPs(true, channel);
            }
            catch (IOException e1)
            {
                Utils.errorReport(e1);
                Console.WriteLine(e1.ToString());
            }
            return;
        }
        if (message.ToLower().StartsWith("!vipsong") && !message.ToLower().Equals("!vipsong", StringComparison.InvariantCultureIgnoreCase))
        {
            if (currency.toggle)
            {
                if (currency.vipSongToggle)
                {
                    try
                    {
                        if (currency.vipsong(sender) == 1)
                        {
                            requestSystem.addVip(channel, Utils.getFollowingText(message), sender);

                            client.SendMessage(sender + " cashed in " + currency.vipSongCost
                                    + " " + currency.currencyName + " for a VIP song! '"
                                    + Utils.getFollowingText(message)
                                    + "' has been added as a VIP song to the song list!");
                        }
                        else if (currency.vipsong(sender) == 0)
                        {

                            client.SendMessage("You need " + currency.vipSongCost + " "
                                    + currency.currencyName + " to buy a VIP song, " + sender + "!");
                        }
                        else
                        {

                            client.SendMessage("You may redeem a VIP song once every "
                                    + currency.vipRedeemCoolDownMinutes + " minutes, " + sender + "!");
                        }
                    }
                    catch (Exception e1)
                    {
                        Utils.errorReport(e1);
                        Console.WriteLine(e1.ToString());
                    }
                }
                else
                {
                    client.SendMessage("VIP Songs are currently turned off, " + sender + "!");
                }
                return;
            }
        }
        if (message.Equals("!info", StringComparison.InvariantCultureIgnoreCase))
        {
            if (currency.toggle)
            {
                String result = "You can check your amount of " + currency.currencyName + " by typing '"
                        + currency.currencyCommand + "'. ";
                if (currency.currencyPerMinute > 0)
                {
                    result += "You gain " + currency.currencyPerMinute + " " + currency.currencyName
                            + " per minute while in the stream. ";
                }
                if (currency.gambleToggle)
                {
                    result += "You can gamble by typing '!gamble <amount>'. ";
                }
                if (currency.vipSongToggle)
                {
                    result += "You can cash in " + currency.vipSongCost + " " + currency.currencyName
                            + " for a VIP song by typing '!vipsong <song>'. ";
                }
                result += "You can check the leaderboards by typing '!leaderboards'. ";
                if (currency.rankupUnitCost == 1)
                {
                    result += "You can check how much it costs to rank up by typing '!nextrank' and purchase the next rank by typing '!rankup'. ";
                }

                client.SendMessage(result);
            }
            return;
        }
        if (message.Equals("!leaderboards", StringComparison.InvariantCultureIgnoreCase))
        {
            if (currency.toggle)
            {
                String temp2 = currency.getLeaderBoards(streamer, botName);
                if (temp2.StartsWith("bad "))
                {
                    Exception e1 = new Exception(temp2);
                    Console.WriteLine(e1.ToString());
                    Utils.errorReport(e1);
                }
                client.SendMessage(temp2);
            }
            return;
        }

        if (message.Equals("!nextrank", StringComparison.InvariantCultureIgnoreCase))
        {
            if (currency.toggle)
            {
                if (currency.ranks.Count > 0)
                {
                    String temp2 = currency.nextRank(sender);
                    client.SendMessage(temp2);
                }
                else
                {
                    client.SendMessage("There are no ranks in this stream.");
                }
            }
            return;
        }

        if (message.Equals("!rankup", StringComparison.InvariantCultureIgnoreCase))
        {
            if (currency.rankupUnitCost == 1)
            {
                if (currency.toggle)
                {
                    if (currency.ranks.Count > 0)
                    {
                        String temp2 = currency.rankup(sender);
                        client.SendMessage(temp2);
                    }
                    else
                    {
                        client.SendMessage("There are no ranks in this stream.");
                    }
                }
            }
            return;
        }

        // Bot Info
        botInfoCOMMAND(message, channel, sender);
        // List commands
        if (temp.Equals("!commands", StringComparison.InvariantCultureIgnoreCase))
        {
            listCommands(message, channel, sender);
            return;
        }
        // Edit Requester Song
        for (int i = 0; i < requestSystem.editSongComm.input.Length; i++)
        {
            if (temp.StartsWith(requestSystem.editSongComm.input[i] + " "))
            {
                try
                {
                    requestSystem.editMySongCOMMAND(message, channel, sender);
                }
                catch (IOException e1)
                {
                    Utils.errorReport(e1);
                    Console.WriteLine(e1.ToString());
                }
                return;
            }
        }
        // Remove Requester Song
        for (int i = 0; i < requestSystem.removeSongComm.input.Length; i++)
        {
            if (temp.Equals(requestSystem.removeSongComm.input[i], StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    requestSystem.removeMySong(message, channel, sender);
                }
                catch (IOException e1)
                {
                    Utils.errorReport(e1);
                    Console.WriteLine(e1.ToString());
                }
                return;
            }
        }
        // Add Donator Song
        for (int i = 0; i < requestSystem.adddonatorComm.input.Length; i++)
        {
            if (temp.StartsWith(requestSystem.adddonatorComm.input[i] + " "))
            {
                try
                {
                    requestSystem.addDonatorCOMMAND(message, channel, sender);
                }
                catch (Exception e1)
                {
                    Utils.errorReport(e1);
                    Console.WriteLine(e1.ToString());
                }
                return;
            }
        }
        // CHECK SONG POSITION
        for (int i = 0; i < requestSystem.songPositionComm.input.Length; i++)
        {
            if (temp.Equals(requestSystem.songPositionComm.input[i], StringComparison.InvariantCultureIgnoreCase))
            {
                requestSystem.checkSongPositionCOMMAND(message, channel, sender);
                return;
            }
        }
        // RANDOM NEXT SONG
        for (int i = 0; i < requestSystem.randomComm.input.Length; i++)
        {
            if (message.Equals(requestSystem.randomComm.input[i], StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    requestSystem.randomizerCommand(message, channel, sender);
                }
                catch (Exception e1)
                {
                    Console.WriteLine(e1.ToString());
                    Utils.errorReport(e1);
                }
                return;
            }
        }
        // Random favorite song
        for (int i = 0; i < requestSystem.favSongComm.input.Length; i++)
        {
            if (message.Equals(requestSystem.favSongComm.input[i], StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    requestSystem.chooseRandomFavorite(message, channel, sender);
                }
                catch (Exception e1)
                {
                    Utils.errorReport(e1);
                    Console.WriteLine(e1.ToString());
                }
                return;
            }
        }
        // Next Regular Command
        if (message.ToLower().Equals("!nextreg", StringComparison.InvariantCultureIgnoreCase)
                || message.ToLower().Equals("!nextregular", StringComparison.InvariantCultureIgnoreCase)
                || message.ToLower().Equals("!regnext", StringComparison.InvariantCultureIgnoreCase)
                || message.ToLower().Equals("!regularnext", StringComparison.InvariantCultureIgnoreCase))
        {
            if (sender.Equals(streamer) || Utils.checkIfUserIsOP(sender, channel, streamer, users))
            {
                try
                {
                    requestSystem.nextRegular(message, channel, sender);
                }
                catch (Exception e1)
                {
                    Utils.errorReport(e1);
                    Console.WriteLine(e1.ToString());
                }
                return;
            }
        }
        // Give Song Command
        if (message.ToLower().StartsWith("!givespot") || message.ToLower().StartsWith("!givesong"))
        {
            try
            {
                requestSystem.giveSpot(message, channel, sender);
            }
            catch (IOException e1)
            {
                Utils.errorReport(e1);
                Console.WriteLine(e1.ToString());
            }
            return;
        }
        // Viewers COMMAND
        for (int i = 0; i < getViewerComm.input.Length; i++)
        {
            if (temp.Equals(getViewerComm.input[i]))
            {
                viewerCOMMAND(message, channel, sender);
                return;
            }
        }
        // UndoNext COMMAND
        if (message.Equals("!undonext", StringComparison.InvariantCultureIgnoreCase) || message.Equals("!undoskip", StringComparison.InvariantCultureIgnoreCase))
        {
            if (sender.Equals(Utils.botMaker) || sender.Equals(streamer)
                    || Utils.checkIfUserIsOP(sender, channel, streamer, users))
            {
                if (requestSystem.lastSong != null)
                {
                    try
                    {
                        requestSystem.doNotWriteToHistory = true;
                        requestSystem.addtopCOMMAND(requestSystem.addtopComm.input[0] + " " + requestSystem.lastSong,
                                channel, sender);
                    }
                    catch (Exception e1)
                    {
                        Utils.errorReport(e1);
                        Console.WriteLine(e1.ToString());
                    }
                    requestSystem.lastSong = null;
                }
                else
                {
                    client.SendMessage("Either this command has just been called or there doesn't exist a previous song, "
                            + sender);
                }
            }
        }

        // Quotes System
        if (temp.Equals("!quotes on") || temp.Equals("!quotes off"))
        {
            try
            {
                quote.triggerQuotes(message, channel, sender);
            }
            catch (IOException e1)
            {
                Console.WriteLine(e1.ToString());
                Utils.errorReport(e1);
            }
            return;
        }
        if (quote.quotesOn == true)
        {
            try
            {
                quote.quotesSystem(message, channel, sender, streamer, users);
            }
            catch (Exception e1)
            {
                Utils.errorReport(e1);
                Console.WriteLine(e1.ToString());
            }
        }
        // Total Requests System
        if (message.ToLower().Equals("!totalrequests", StringComparison.InvariantCultureIgnoreCase))
        {
            int num = requestSystem.getNumRequests(sender);
            if (num < 1)
            {
                client.SendMessage("You have yet to request a song in this stream, " + sender + "!");
            }
            else if (num == 1)
            {
                client.SendMessage("You have requested " + num + " song in " + streamer
                        + "'s stream, " + sender + "!");
            }
            else
            {
                client.SendMessage("You have requested " + num + " songs in " + streamer
                        + "'s stream, " + sender + "!");
            }
            return;
        }
        if (message.ToLower().Equals("!toprequester", StringComparison.InvariantCultureIgnoreCase))
        {
            int max = 0;
            String user = "";
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].numRequests > max && !users[i].username.Equals("revlobot", StringComparison.InvariantCultureIgnoreCase)
                        && !users[i].username.Equals(streamer, StringComparison.InvariantCultureIgnoreCase))
                {
                    max = users[i].numRequests;
                    user = users[i].username;
                }
            }
            if (max == 1)
            {
                client.SendMessage("The top requester in " + streamer + "'s stream is " + user
                        + " with " + max + " request!");
            }
            else if (max > 1)
            {
                client.SendMessage("The top requester in " + streamer + "'s stream is " + user
                        + " with " + max + " requests!");
            }
            else
            {
                client.SendMessage("Could not find any requesters");
            }
            return;
        }
        // Reset counters
        if (message.ToLower().StartsWith("!setcounter"))
        {
            if (sender.Equals(streamer) || sender.Equals(Utils.botMaker))
            {
                String num = Utils.getFollowingText(message).Trim();
                if (Utils.isInteger(num))
                {
                    if (message.ToLower().StartsWith("!setcounter1"))
                    {
                        setCounter(message, channel, sender, 1, Int32.Parse(num));
                        return;
                    }
                    else if (message.ToLower().StartsWith("!setcounter2"))
                    {
                        setCounter(message, channel, sender, 2, Int32.Parse(num));
                        return;
                    }
                    else if (message.ToLower().StartsWith("!setcounter3"))
                    {
                        setCounter(message, channel, sender, 3, Int32.Parse(num));
                        return;
                    }
                    else if (message.ToLower().StartsWith("!setcounter4"))
                    {
                        setCounter(message, channel, sender, 4, Int32.Parse(num));
                        return;
                    }
                    else if (message.ToLower().StartsWith("!setcounter5"))
                    {
                        setCounter(message, channel, sender, 5, Int32.Parse(num));
                        return;
                    }
                }
            }
        }
        // Raffle
        if (message.Equals("!startraffle", StringComparison.InvariantCultureIgnoreCase))
        {
            if (sender.Equals(Utils.botMaker, StringComparison.InvariantCultureIgnoreCase) || sender.Equals(streamer, StringComparison.InvariantCultureIgnoreCase)
                    || Utils.checkIfUserIsOP(sender, channel, streamer, users))
            {
                if (raffleInProgress)
                {
                    client.SendMessage("A raffle is already in progress!");
                }
                else
                {
                    raffleUsers.Clear();
                    raffleInProgress = true;
                    client.SendMessage("A raffle has just started! Type '" + giveawaycommandname
                            + "' to join the raffle!");
                }
            }
            return;
        }
        if (message.ToLower().StartsWith("!endraffle"))
        {
            if (raffleInProgress)
            {
                if (raffleUsers.Count > 0)
                {
                    if (message.Equals("!endraffle", StringComparison.InvariantCultureIgnoreCase))
                    {
                        client.SendMessage("The winner of the raffle is: "
                                + raffleUsers[new Random().Next(raffleUsers.Count)]);
                    }
                    else
                    {
                        if (Utils.isInteger(Utils.getFollowingText(message)))
                        {
                            for (int i = 0; i < Int32.Parse(Utils.getFollowingText(message)); i++)
                            {
                                if (raffleUsers.Count != 0)
                                {
                                    String win = raffleUsers[new Random().Next(raffleUsers.Count)];
                                    client.SendMessage("The winner of the raffle is: " + win);
                                    raffleUsers.Remove(win);
                                }
                            }
                        }
                        else
                        {
                            client.SendMessage("Please type in the form of '!endraffle' to choose one winner or '!endraffle <number>' to choose multiple winners, "
                                    + sender + "!");
                            return;
                        }
                    }
                }
                else
                {
                    client.SendMessage("No one joined the raffle!");
                }
            }
            else
            {
                client.SendMessage("There is no raffle in progress!");
                return;
            }
            raffleUsers.Clear();
            raffleInProgress = false;
            return;
        }
        if (message.Equals(giveawaycommandname, StringComparison.InvariantCultureIgnoreCase))
        {
            if (raffleInProgress)
            {
                foreach (String str in raffleUsers)
                {
                    if (str.Equals(sender))
                    {
                        return;
                    }
                }
                raffleUsers.Add(sender);
            }
            return;
        }
        // Minigame
        if (message.StartsWith("!minigame") || message.StartsWith("!startgame"))
        {
            if (sender.Equals(streamer) || Utils.checkIfUserIsOP(sender, channel, streamer, users)
                    || sender.Equals(Utils.botMaker))
            {
                if (message.Equals("!minigame on"))
                {
                    try
                    {
                        triggerMiniGame(true, channel);
                    }
                    catch (IOException e1)
                    {
                        Utils.errorReport(e1);
                        Console.WriteLine(e1.ToString());
                    }
                    return;
                }
                if (message.Equals("!minigame off"))
                {
                    try
                    {
                        triggerMiniGame(false, channel);
                    }
                    catch (IOException e1)
                    {
                        Utils.errorReport(e1);
                        Console.WriteLine(e1.ToString());
                    }
                    return;
                }
                if (message.StartsWith("!startgame") && minigameOn)
                {
                    if (minigameTriggered)
                    {
                        client.SendMessage("The guessing game is currently aleady in progress!");
                        return;
                    }
                    else
                    {
                        gameGuess.Clear();
                        gameUser.Clear();
                        client.SendMessage("The guessing game has started! You may only enter once! You have "
                                + (minigameTimer / 1000) + " seconds to enter a guess by typing '!guess amount'");
                        minigameTriggered = true;
                        gameStartTime = Environment.TickCount;
                        timeFinished = false;
                        return;
                    }
                }
            }
        }
        if (message.Trim().Contains("!guess ") && !message.Trim().Equals("!guess", StringComparison.InvariantCultureIgnoreCase) && minigameOn)
        {
            if (minigameTriggered)
            {
                if (timeFinished)
                {
                    if (minigameEndMessage.Contains("$user"))
                    {
                        client.SendMessage(minigameEndMessage.Replace("$user", sender));
                    }
                    else
                    {
                        client.SendMessage(minigameEndMessage);
                    }
                    return;
                }
                for (int i = 0; i < gameUser.Count; i++)
                {
                    if (gameUser[i].Equals(sender))
                    {
                        client.SendMessage("You already guessed, " + sender + "!");
                        return;
                    }
                }
                String temp2 = Utils.getFollowingText(message).Trim();
                if (Utils.isDouble(temp2))
                {
                    gameGuess.Add(Double.Parse(temp2));
                    gameUser.Add(sender);
                    return;
                }
                else
                {
                    client.SendMessage("Please enter a guess in the form of '!guess amount'");
                    return;
                }
            }
            else
            {
                client.SendMessage("There is no current game in progress, " + sender + "!");
                return;
            }
        }

        if (message.Trim().StartsWith("!endgame"))
        {
            if (sender.Equals(streamer) || Utils.checkIfUserIsOP(sender, channel, streamer, users)
                    || sender.Equals(Utils.botMaker))
            {
                if (message.Trim().Equals("!endgame", StringComparison.InvariantCultureIgnoreCase) || message.Trim().Equals("!cancelgame", StringComparison.InvariantCultureIgnoreCase))
                {
                    client.SendMessage("Current guessing game has been canceled!");
                    timeFinished = false;
                    minigameTriggered = false;
                    gameGuess.Clear();
                    gameUser.Clear();
                    return;
                }
                if (message.Trim().Contains("!endgame ") && !message.Equals("!endgame", StringComparison.InvariantCultureIgnoreCase) && minigameOn)
                {
                    String temp2 = Utils.getFollowingText(message).Trim();
                    if (gameGuess.Count == 0)
                    {
                        client.SendMessage("No one guessed, therefore no one wins! :( ");
                        timeFinished = false;
                        minigameTriggered = false;
                        gameGuess.Clear();
                        gameUser.Clear();
                        return;
                    }
                    if (Utils.isDouble(temp2))
                    {
                        String winners = "";
                        double winner = Utils.closest(Double.Parse(temp2), gameGuess);
                        int j = 0;
                        while (!gameGuess[j].Equals(winner))
                        {
                            j++;
                        }
                        double win = gameGuess[j];
                        for (int i = 0; i < gameGuess.Count; i++)
                        {
                            if (gameGuess[i].Equals(win))
                            {
                                winners += gameUser[i] + ", ";
                            }
                        }
                        winners = winners.Substring(0, winners.Length - 2).Trim();
                        if (winners.Contains(","))
                        {
                            client.SendMessage("The winners are " + winners + " with guesses of "
                                    + gameGuess[j] + "!");
                        }
                        else
                        {
                            client.SendMessage("The winner is " + winners + " with a guess of "
                                    + gameGuess[j] + "!");
                        }
                        if (currency.toggle)
                        {
                            String[] winnersArray = winners.Split(',');
                            foreach (String str in winnersArray)
                            {
                                client.SendMessage(currency.bonus(str.Trim(), (int)Math.Round(Double.Parse(temp2))));
                            }
                        }
                        timeFinished = false;
                        minigameTriggered = false;
                        gameGuess.Clear();
                        gameUser.Clear();
                    }
                    else
                    {

                        client.SendMessage("Please enter a winning amount in the form of '!endgame amount'");
                    }
                }
            }
            return;
        }
        // Text Adventure
        if (message.Equals("!adventure on", StringComparison.InvariantCultureIgnoreCase))
        {
            if (Utils.checkIfUserIsOP(sender, channel, streamer, users) || sender.Equals(Utils.botMaker)
                    || sender.Equals(streamer))
            {
                if (!adventureToggle)
                {
                    adventureToggle = true;
                    client.SendMessage("Adventure system turned on!");
                }
                else
                {
                    client.SendMessage("Adventure system is already on!");
                }
                return;
            }
        }
        if (message.Equals("!adventure off", StringComparison.InvariantCultureIgnoreCase))
        {
            if (Utils.checkIfUserIsOP(sender, channel, streamer, users) || sender.Equals(Utils.botMaker)
                    || sender.Equals(streamer))
            {
                if (adventureToggle)
                {
                    adventureToggle = false;
                    client.SendMessage("Adventure system turned off!");
                }
                else
                {
                    client.SendMessage("Adventure system is already off!");
                }
                return;
            }
        }
        if (message.Equals("!adventure", StringComparison.InvariantCultureIgnoreCase) || message.Equals("!join", StringComparison.InvariantCultureIgnoreCase))
        {
            if (adventureToggle)
            {
                if (waitForAdventureCoolDown)
                {
                    if (textAdventure.adventureStartTime + (textAdventure.adventureCoolDown * 60000) <= Environment.TickCount)
                    {
                        waitForAdventureCoolDown = false;
                    }
                    else
                    {
                        double timeLeft = Math.Abs(Math.Ceiling((Double)((Environment.TickCount
                                - (textAdventure.adventureStartTime + (textAdventure.adventureCoolDown * 60000)))
                                / 60000)));
                        if (timeLeft == 0.0)
                        {
                            timeLeft = 1.0;
                        }
                        if (timeLeft == 1.0)
                        {
                            client.SendMessage("We must prepare ourselves before another adventure. Please try again in "
                            + timeLeft + " minute, " + sender + "!");
                        }
                        else
                        {
                            client.SendMessage("We must prepare ourselves before another adventure. Please try again in "
                            + timeLeft + " minutes, " + sender + "!");
                        }
                        return;
                    }
                }
                if (!waitForAdventureCoolDown)
                {
                    if (!startAdventure)
                    {
                        startAdventure = true;
                        client.SendMessage(textAdventure.getText().Replace("$user", sender)
                            + " Type '!adventure' or '!join' to join!");
                        var t = new Thread(() => adventureThread(sender));
                        t.Start();
                        return;
                    }
                    if (startAdventure)
                    {
                        try
                        {
                            if (textAdventure.addUser(sender) == 0)
                            {

                                client.SendMessage("Please try again in a little bit, " + sender
                                        + "!");
                                startAdventure = false;
                            }
                        }
                        catch (Exception e1)
                        {
                            Utils.errorReport(e1);
                            Console.WriteLine(e1.ToString());
                        }
                    }
                    return;
                }
            }
        }
        // Change Bot Color
        if (message.Trim().StartsWith("!botcolor ") || message.Trim().StartsWith("!colorbot "))
        {
            if (Utils.checkIfUserIsOP(sender, channel, streamer, users) || sender.Equals(Utils.botMaker)
                    || sender.Equals(streamer))
            {
                String[] colors = { "Blue", "BlueViolet", "CadetBlue", "Chocolate", "Coral", "DodgerBlue", "Firebrick",
                    "GoldenRod", "Green", "HotPink", "OrangeRed", "Red", "SeaGreen", "SpringGreen", "YellowGreen" };
                String color = null;
                for (int i = 0; i < colors.Length; i++)
                {
                    if (colors[i].Equals(Utils.getFollowingText(message).Trim(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        color = Utils.getFollowingText(message).Trim();
                        break;
                    }
                }
                if (color == null)
                {
                    client.SendMessage("Invalid color, colors available: Blue, BlueViolet, CadetBlue, Chocolate, Coral, DodgerBlue, Firebrick,GoldenRod, Green, HotPink, OrangeRed, Red, SeaGreen, SpringGreen, YellowGreen");
                    return;
                }
                client.SendMessage("/color " + Utils.getFollowingText(message).Trim());
                botColor = color;
            }
        }
        // Others
        if (message.Trim().Equals("!rocksmithunite", StringComparison.InvariantCultureIgnoreCase))
        {
            client.SendMessage("Make sure to checkout RocksmithUnite at https://www.twitch.tv/team/rocksmithunite");
            return;
        }

        // Remove Songs
        if (message.Trim().ToLower().StartsWith("!removesong ")
                || message.Trim().ToLower().StartsWith("!deletesong "))
        {
            requestSystem.removeSongCOMMAND(sender, channel, streamer, users, message, temp);
        }

        if (message.Trim().ToLower().StartsWith("!editcom !")
                || message.Trim().ToLower().StartsWith("!updatecom !"))
        {
            if (sender.Equals(streamer) || Utils.checkIfUserIsOP(sender, channel, streamer, users)
                    || sender.Equals(Utils.botMaker))
            {
                String comm, response;
                try
                {
                    comm = Utils.getFollowingText(message).Substring(0, Utils.getFollowingText(message).IndexOf(" "));
                    response = Utils.getFollowingText(message)
                            .Substring(Utils.getFollowingText(message).IndexOf(" ") + 1);
                }
                catch
                {
                    client.SendMessage("To edit a command, it must be in the form '!editcom !command response'");
                    return;
                }
                foreach (Command command in commandList)
                {
                    if (comm.Equals(command.input[0], StringComparison.InvariantCultureIgnoreCase))
                    {
                        command.output = response;
                        resetAllCommands();
                        client.SendMessage(comm + " has been edited, " + sender + "!");
                        return;
                    }
                }
                client.SendMessage(comm + " does not exist, " + sender + "!");
                return;
            }
        }

        if (message.Trim().ToLower().StartsWith("!removecom !")
                || message.Trim().ToLower().StartsWith("!deletecom !"))
        {
            if (sender.Equals(streamer) || Utils.checkIfUserIsOP(sender, channel, streamer, users)
                    || sender.Equals(Utils.botMaker))
            {
                String comm;
                try
                {
                    comm = Utils.getFollowingText(message);
                }
                catch
                {
                    client.SendMessage("To remove a command, it must be in the form '!removecom !command'");
                    return;
                }
                foreach (Command command in commandList)
                {
                    if (comm.Equals(command.input[0], StringComparison.InvariantCultureIgnoreCase))
                    {
                        commandList.Remove(command);
                        resetAllCommands();
                        client.SendMessage(comm + " has been removed, " + sender + "!");
                        return;
                    }
                }
                client.SendMessage(comm + " does not exist, " + sender + "!");
                return;
            }
        }

        if (message.Trim().ToLower().StartsWith("!addcom !"))
        {
            if (sender.Equals(streamer) || Utils.checkIfUserIsOP(sender, channel, streamer, users)
                    || sender.Equals(Utils.botMaker))
            {
                String comm, response;
                try
                {
                    comm = Utils.getFollowingText(message).Substring(0, Utils.getFollowingText(message).IndexOf(" "));
                    response = Utils.getFollowingText(message)
                            .Substring(Utils.getFollowingText(message).IndexOf(" ") + 1);
                }
                catch
                {
                    client.SendMessage("To Add a command, it must be in the form '!addcom !command response'");
                    return;
                }
                foreach (Command command in commandList)
                {
                    if (comm.Equals(command.input[0], StringComparison.InvariantCultureIgnoreCase))
                    {
                        client.SendMessage(comm + " already exists, " + sender + "!");
                        return;
                    }
                }
                String[] str = { comm };
                commandList.Add(new Command(str, 0, response, "user", true));
                client.SendMessage(comm + " has been added, " + sender + "!");
                resetAllCommands();
                return;
            }
        }

        if (message.Equals("!promote", StringComparison.InvariantCultureIgnoreCase))
        {
            if (sender.Equals(streamer) || Utils.checkIfUserIsOP(sender, channel, streamer, users)
                    || sender.Equals(Utils.botMaker))
            {
                client.SendMessage("To promote a user's song, type '!promote @user', " + sender + "!");
                return;
            }
        }
        if (message.Trim().ToLower().StartsWith("!promote "))
        {
            requestSystem.promoteSongCommand(sender, channel, streamer, users, message);
        }

        if (message.Trim().ToLower().Equals("!subcredits", StringComparison.InvariantCultureIgnoreCase))
        {
            client.SendMessage(currency.getSubCredits(sender));
            return;
        }

        if (message.Trim().ToLower().StartsWith("!givecredits"))
        {
            if (sender.Equals(streamer) || Utils.checkIfUserIsOP(sender, channel, streamer, users))
            {
                if (message.Equals("!givecredits", StringComparison.InvariantCultureIgnoreCase))
                {
                    client.SendMessage("Type in the format '!givecredits user amount'");
                }
                String[] tempArray = Utils.getFollowingText(message).Split(' ');
                String neg = "";
                if (tempArray[1].StartsWith("-"))
                {
                    neg = "-";
                    tempArray[1] = tempArray[1].Substring(1);
                }
                if (Utils.isInteger(tempArray[1]))
                {
                    if (tempArray[0].GetType() == typeof(String))
                    {
                        if (tempArray.Length == 2)
                        {
                            if (tempArray[0].StartsWith("@"))
                            {
                                tempArray[0] = tempArray[0].Replace("@", "");
                            }
                            if (neg.Equals("-"))
                            {
                                client.SendMessage(currency.bonusSubCredits(tempArray[0], -1 * Int32.Parse(tempArray[1])));
                            }
                            else
                            {
                                client.SendMessage(currency.bonusSubCredits(tempArray[0], Int32.Parse(tempArray[1])));
                            }
                            return;
                        }
                    }
                }

                client.SendMessage("Type in the format '!givecredits user amount'");
            }
        }

        if (message.Trim().ToLower().StartsWith("!subsong")
                && !message.ToLower().Equals("!subsong", StringComparison.InvariantCultureIgnoreCase))
        {
            try
            {
                if (currency.redeemSubCredits(sender))
                {
                    requestSystem.addDonator(channel, Utils.getFollowingText(message), sender);
                    client.SendMessage(sender + " cashed in " + currency.subCreditRedeemCost
                            + " " + " sub credits for a sub song! '" + Utils.getFollowingText(message)
                            + "' has been added as a $$$ song to the song list!");
                }
                else
                {
                    client.SendMessage("You need " + currency.subCreditRedeemCost + " "
                            + " sub credits to buy a $$$ song, " + sender + "!");
                }
            }
            catch (Exception e1)
            {
                Utils.errorReport(e1);
                Console.WriteLine(e1.ToString());
            }
        }

    }

    public void read() // is this needed?
    {
        try
        {
            MainWindow.bot = Utils.loadData();
        }
        catch (IOException e1)
        {
            Utils.errorReport(e1);
            Console.WriteLine(e1.ToString());
        }
    }

    public void setCounter(String message, String channel, String sender, int counter, int value)
    {
        client.SendMessage("Counter" + counter + " has been set to " + value + "!");
        if (counter == 1)
        {
            counter1 = value;
        }
        else if (counter == 2)
        {
            counter2 = value;
        }
        else if (counter == 3)
        {
            counter3 = value;
        }
        else if (counter == 4)
        {
            counter4 = value;
        }
        else if (counter == 5)
        {
            counter5 = value;
        }
    }

    public String getRandomUser(String channel)
    {
        Random rand = new Random();
        int index = rand.Next(Utils.getAllViewers(streamer).Count);
        return Utils.getAllViewers(streamer)[index].ToString();
    }

    public void botInfoCOMMAND(String message, String channel, String sender)
    {
        if (message.Equals("!edudebot", StringComparison.InvariantCultureIgnoreCase) || message.Equals("!bot", StringComparison.InvariantCultureIgnoreCase)
                || message.Equals("!edude15000", StringComparison.InvariantCultureIgnoreCase) || message.Equals("!dudebot", StringComparison.InvariantCultureIgnoreCase))
        {
            client.SendMessage("Dudebot is a free bot programmed by Edude15000 using PircBot. "
                    + "If you would like to use Dudebot, the download link is here: http://dudebot.webs.com/ Make sure to join the DudeBot Discord also: https://discord.gg/NFehx5h");
        }
        if (message.Equals("!version", StringComparison.InvariantCultureIgnoreCase) || message.Equals("!botversion", StringComparison.InvariantCultureIgnoreCase)
                || message.Equals("!dudebotversion", StringComparison.InvariantCultureIgnoreCase))
        {
            client.SendMessage("Dudebot " + Utils.version + " (" + Utils.releaseDate + ")");
        }
        if (message.Equals("!refresh", StringComparison.InvariantCultureIgnoreCase))
        {
            read();
        }
    }

    public void listCommands(String message, String channel, String sender)
    {
        String temp = message.ToLower();
        String line = "!givespot, !totalrequests, !toprequester, !subcredits";
        if (sfxCommandList.Count > 0)
        {
            line += "!sfx, ";
        }
        if (imageCommandList.Count > 0)
        {
            line += "!images, ";
        }
        if (currency.toggle)
        {
            line += "!rank, !info, !gamble, " + currency.currencyCommand + ", ";
        }
        if (temp.Equals("!commands", StringComparison.InvariantCultureIgnoreCase))
        {
            foreach (Command command in botCommandList)
            {
                for (int i = 0; i < command.input.Length; i++)
                {
                    if (command.level == 0)
                    {
                        line += command.input[i] + ", ";
                    }
                }
            }
            client.SendMessage("DudeBot Commands: " + line.Substring(0, line.Length - 2));
            line = "";
            for (int i = 0; i < userCommandList.Count; i++)
            {
                if (!userCommandList[i].output.Equals("$shoutout"))
                {
                    line += userCommandList[i].input[0] + ", ";
                }
            }
            for (int i = 0; i < timerCommandList.Count; i++)
            {
                if (!timerCommandList[i].output.Contains("$shoutout")
                        && !timerCommandList[i].output.Equals("$shoutout"))
                {
                    line += timerCommandList[i].input[0] + ", ";
                }
            }
            if (line.Length >= 2)
            {
                client.SendMessage("Other Commands: " + line.Substring(0, line.Length - 2));
            }
            read();
        }
    }

    public Boolean checkUserLevelCustomCommands(String sender, int level, String channel)
    {
        int holder = 0;
        if (sender.Equals(Utils.botMaker, StringComparison.InvariantCultureIgnoreCase))
        {
            return true;
        }
        if (level == 0)
        {
            return true;
        }
        if (sender.Equals(streamer))
        {
            return true;
        }
        foreach (BotUser botUser in users)
        {
            if (botUser.username.Equals(sender, StringComparison.InvariantCultureIgnoreCase) && botUser.sub)
            {
                holder = 1;
                break;
            }
        }
        if (level <= holder)
        {
            return true;
        }
        if (Utils.checkIfUserIsOP(sender, channel, streamer, users))
        {
            holder = 2;
        }
        if (level <= holder)
        {
            return true;
        }
        return false;
    }

    public Boolean checkUserLevel(String sender, Command command, String channel)
    {
        int holder = 0;
        if (sender.Equals(Utils.botMaker, StringComparison.InvariantCultureIgnoreCase))
        {
            return true;
        }
        if (command.level == 0)
        {
            return true;
        }
        if (sender.Equals(streamer, StringComparison.InvariantCultureIgnoreCase))
        {
            return true;
        }
        foreach (BotUser botUser in users)
        {
            if (botUser.username.Equals(sender, StringComparison.InvariantCultureIgnoreCase) && botUser.sub)
            {
                holder = 1;
                break;
            }
        }
        if (command.level <= holder)
        {
            return true;
        }
        if (Utils.checkIfUserIsOP(sender, channel, streamer, users))
        {
            holder = 2;
        }
        if (command.level <= holder)
        {
            return true;
        }
        return false;
    }

    public List<String> getHostList() // TODO : TEST!
    {
        List<String> result = new List<String>();
        try
        {
            dynamic a = new JObject(Utils.callURL("https://api.twitch.tv/kraken/oauth2/authorize?channels/" + streamer));
            String id = a["_id"];
            a = new JObject(Utils.callURL("http://tmi.twitch.tv/hosts?include_logins=1&target=" + id));
            JArray info = a["hosts"];
            for (int i = 0; i < info.Count; i++)
            {
                a = info[i];
                result.Add(a["host_login"]);
            }
        }
        catch (Exception e1)
        {
            Utils.errorReport(e1);
            Console.WriteLine(e1.ToString());
        }
        return result;
    }

    public void triggerCurrency(Boolean trigger, String channel)
    {
        if (trigger)
        {
            currency.toggle = true;
            client.SendMessage("Currency system turned on!");
        }
        else
        {
            currency.toggle = false;
            client.SendMessage("Currency system turned off!");
        }
    }

    public void triggerMiniGame(Boolean trigger, String channel)
    {
        if (trigger)
        {
            minigameOn = true;
            client.SendMessage("Minigame turned on!");
        }
        else
        {
            minigameOn = false;
            client.SendMessage("Minigame turned off!");
        }
    }

    public void triggerGamble(Boolean trigger, String channel)
    {
        if (trigger)
        {
            currency.gambleToggle = true;
            client.SendMessage("Gambling has been turned on!");
        }
        else
        {
            currency.gambleToggle = false;
            client.SendMessage("Gambling has been turned off!");
        }
    }

    public void viewerCOMMAND(String message, String channel, String sender)
    {
        if (checkUserLevel(sender, getViewerComm, channel))
        {
            for (int i = 0; i < getViewerComm.input.Length; i++)
            {
                String temp = message.ToLower();
                if (temp.Equals(getViewerComm.input[i]))
                {
                    client.SendMessage("Current viewer count: "
                            + Utils.getNumberOfUsers(channel, streamer));
                }
            }
        }
    }

    public void userCOMMANDS(String message, String channel, String sender)
    {
        for (int i = 0; i < userCommandList.Count; i++)
        {
            String temp = message.ToLower();
            if (temp.StartsWith(userCommandList[i].input[0])
                    && checkUserLevelCustomCommands(sender, userCommandList[i].level, channel))
            {
                client.SendMessage(userVariables(userCommandList[i].output, channel, sender,
                        Utils.getFollowingText(message), message, false));
            }
        }
        for (int i = 0; i < timerCommandList.Count; i++)
        {
            String temp = message.ToLower();
            if (temp.Equals(timerCommandList[i].input[0]))
            {
                client.SendMessage(userVariables(timerCommandList[i].output, channel, sender,
                        Utils.getFollowingText(message), message, false));
            }
        }
    }

    public String userVariables(String response, String channel, String sender, String followingText, String message, Boolean ev)
    {
        if (followingText.StartsWith("@"))
        {
            followingText = followingText.Substring(1);
        }
        if (response.Contains("$viewers"))
        {
            response = response.Replace("$viewers", Utils.getNumberOfUsers(channel, streamer));
        }
        if (response.Contains("$user"))
        {
            response = response.Replace("$user", sender);
        }
        if (response.Contains("$input"))
        {
            if ((message != followingText) || ev)
            {
                response = response.Replace("$input", followingText);
            }
            else
            {
                return "";
            }
        }
        if (response.Contains("$length"))
        {
            try
            {
                response = response.Replace("$length", requestSystem.getNumberOfSongs());
            }
            catch (IOException e)
            {
                Utils.errorReport(e);
                Console.WriteLine(e.ToString());
            }
        }
        if (response.Contains("$randomuser"))
        {
            getRandomUser(channel);
        }
        if (response.Contains("$currentsong"))
        {
            try
            {
                String line = requestSystem.getCurrentSongTitle(channel);
                if (line.Contains("\t"))
                {
                    line = line.Substring(0, line.IndexOf("\t"));
                }
                response = response.Replace("$currentsong", line);
            }
            catch (Exception e1)
            {
                Utils.errorReport(e1);
                Console.WriteLine(e1.ToString());
            }
        }
        if (response.Contains("$currentrequester"))
        {
            try
            {
                String line = requestSystem.getCurrentSongTitle(channel);
                if (line.Contains("\t"))
                {
                    line = line.Substring(line.LastIndexOf("\t") + 2, line.Length - 1);
                }
                response = response.Replace("$currentrequester", line);
            }
            catch (Exception e1)
            {
                Utils.errorReport(e1);
                Console.WriteLine(e1.ToString());
            }
        }
        if (response.Contains("$randomuser"))
        {
            response = response.Replace("$randomuser", getRandomUser(channel));
        }
        if (response.Contains("$randomnumber3"))
        {
            if (message.Contains("coffee") && sender.Equals("hardrockangelart", StringComparison.InvariantCultureIgnoreCase))
            {
                response = response.Replace("$randomnumber3", "1000");
            }
            else if (message.Contains("guitar") || message.Contains("bass"))
            {
                response = response.Replace("$randomnumber3", (Utils.getRandomNumber(25) + 75).ToString());
            }
            else if (message.Contains("frosk") && sender.Equals("foopjohnson", StringComparison.InvariantCultureIgnoreCase)
                  || message.Contains("foop") && sender.Equals("ninja_frosk", StringComparison.InvariantCultureIgnoreCase))
            {
                response = "0% Love, 100% Froskin' the Johnson!";
            }
            else if (message.Contains("pizza") && sender.Equals("nipplejesus", StringComparison.InvariantCultureIgnoreCase))
            {
                response = "110% PIZZA GOODNESS!";
            }
            else if (message.Contains("pizza") && sender.Equals("nipplejesus", StringComparison.InvariantCultureIgnoreCase))
            {
                response = "110% PIZZA GOODNESS!";
            }
            else if (message.Contains("azeven") && sender.Equals("SirBrutalify", StringComparison.InvariantCultureIgnoreCase))
            {
                response = "100% LOVE BABY!";
            }
            else if (message.Contains("SirBrutalify") && sender.Equals("azeven", StringComparison.InvariantCultureIgnoreCase))
            {
                response = "100% LOVE BABY!";
            }
            else
            {
                response = response.Replace("$randomnumber3", Utils.getRandomNumber(100).ToString());
            }
        }
        if (response.Contains("$randomnumber2"))

        {
            response = response.Replace("$randomnumber2", Utils.getRandomNumber(10).ToString());
        }
        if (response.Contains("$8ball"))
        {
            if ((message != followingText) || ev)
            {
                response = response.Replace("$8ball", "Magic 8-ball says..." + ballResponse(sender, streamer, message));
            }
            else
            {
                return "";
            }
        }
        if (response.Contains("$streamer"))
        {
            response = response.Replace("$streamer", streamer);
        }
        if (response.Contains("$shoutout"))
        {
            if ((message != followingText) || ev)
            {
                if (sender.Equals(Utils.botMaker) || sender.Equals(streamer)
                        || Utils.checkIfUserIsOP(sender, channel, streamer, users))
                {
                    String info = Utils.callURL("https://api.twitch.tv/kraken/channels/" + followingText);
                    try
                    {
                        String game = ((dynamic)new JObject(info))["game"];

                        if (game.Equals("null", StringComparison.InvariantCultureIgnoreCase))
                        {
                            response = response.Replace("$shoutout",
                                    "Make sure to follow " + followingText + " at twitch.tv/" + followingText);
                        }
                        else
                        {
                            response = response.Replace("$shoutout", "Make sure to follow " + followingText
                                    + " at twitch.tv/" + followingText + " ! They were last streaming " + game + "!");
                        }
                    }
                    catch (Exception)
                    {
                        response = response.Replace("$shoutout",
                                "Make sure to follow " + followingText + " at twitch.tv/" + followingText);
                    }
                }
                else
                {
                    response = "";
                }
            }
            else
            {
                response = "";
            }
        }
        if (response.Contains("$start")) // TODO : TEST!
        {
            String info = Utils.callURL("https://api.twitch.tv/kraken/channels/" + streamer);
            try
            {
                dynamic a = new JObject(info);
                String created = a["created_at"];
                DateTime d = Convert.ToDateTime(created);
                String str = d.ToString("MM/dd/yyyy");
                response = response.Replace("$start", str);
            }
            catch (Exception)
            {
                response = "";
            }
        }
        if (response.Contains("$uptime")) // TODO : TEST!
        {
            String info = Utils.callURL("https://api.twitch.tv/kraken/streams/" + streamer);
            try
            {
                dynamic a = new JObject(info);
                dynamic s = a["stream"];
                String name = s["created_at"];
                DateTime d = Convert.ToDateTime(name);
                String str = d.ToString("MM/dd/yyyy");
                long millisFromEpoch = (long)(d - new DateTime(1970, 1, 1)).TotalMilliseconds;
                response = response.Replace("$uptime",
                                            Utils.timeConversion((int)((Environment.TickCount - millisFromEpoch) / 1000)));
            }
            catch (Exception)
            {
                response = "Stream is not currently online!";
            }
        }
        if (response.Contains("$following"))
        {
            if (streamer.Equals(sender))
            {
                response = "You can't follow yourself! Kappa";
            }
            else
            {
                String info = Utils
                        .callURL("https://api.twitch.tv/kraken/users/" + sender + "/follows/channels/" + streamer);
                try
                {
                    dynamic a = new JObject(info);
                    String created = a["created_at"];
                    DateTime d = Convert.ToDateTime(created);
                    long millisFromEpoch = (long)(d - new DateTime(1970, 1, 1)).TotalMilliseconds;
                    long diff = ((Environment.TickCount - millisFromEpoch) / 1000);
                    response = response.Replace("$following", Utils.timeConversionYears(diff));
                }
                catch (Exception)
                {
                    response = "";
                }
            }
        }
        if (response.Contains("$counter1"))
        {
            response = response.Replace("$counter1", (counter1 + 1).ToString());
            updateValue("counter1", counter1);
        }
        if (response.Contains("$counter2"))
        {
            response = response.Replace("$counter2", (counter2 + 1).ToString());
            updateValue("counter2", counter2);
        }
        if (response.Contains("$counter3"))
        {
            response = response.Replace("$counter3", (counter3 + 1).ToString());
            updateValue("counter3", counter3);
        }
        if (response.Contains("$counter4"))
        {
            response = response.Replace("$counter4", (counter4 + 1).ToString());
            updateValue("counter4", counter4);
        }
        if (response.Contains("$counter5"))
        {
            response = response.Replace("$counter5", (counter5 + 1).ToString());
            updateValue("counter5", counter5);
        }
        if (response.Contains("$roulette"))
        {
            response = "";
            var t = new Thread(() => rouletteThread(sender));
            t.Start();
        }
        return response;
    }

    public void updateValue(String counter, int val)
    {
        if (counter == "counter1")
        {
            counter1 = val + 1;
        }
        else if (counter == "counter2")
        {
            counter2 = val + 1;
        }
        else if (counter == "counter3")
        {
            counter3 = val + 1;
        }
        else if (counter == "counter4")
        {
            counter4 = val + 1;
        }
        else if (counter == "counter5")
        {
            counter5 = val + 1;
        }
    }

    public String ballResponse(String sender, String streamer, String message)
    {
        message = message.ToLower();
        if (message.Contains("edude15000") || message.Contains("edude"))
        {
            return "Edude15000 is too sexy for this question, " + sender;
        }
        String line = "";
        Random rand = new Random();
        int index = rand.Next(21) + 1;
        if (index == 1)
        {
            line = "It is certain, " + sender;
        }
        else if (index == 2)
        {
            line = "It is decidedly so, " + sender;
        }
        else if (index == 3)
        {
            line = "Without a doubt, " + sender;
        }
        else if (index == 4)
        {
            line = "Yes, definitely, " + sender;
        }
        else if (index == 5)
        {
            line = "You may rely on it, " + sender;
        }
        else if (index == 6)
        {
            line = "As I see it, yes, " + sender;
        }
        else if (index == 7)
        {
            line = "Most likely, " + sender;
        }
        else if (index == 8)
        {
            line = "Outlook good, " + sender;
        }
        else if (index == 9)
        {
            line = "Yes, " + sender;
        }
        else if (index == 10)
        {
            line = "Signs point to yes, " + sender;
        }
        else if (index == 11)
        {
            line = "Reply hazy try again, " + sender;
        }
        else if (index == 12)
        {
            line = "Ask again later, " + sender;
        }
        else if (index == 13)
        {
            line = "Better not tell you now, " + sender;
        }
        else if (index == 14)
        {
            line = streamer + " doesn't think so, " + sender;
        }
        else if (index == 15)
        {
            line = streamer + " definitely thinks so, " + sender;
        }
        else if (index == 16)
        {
            line = "Don't count on it, " + sender;
        }
        else if (index == 17)
        {
            line = "My reply is no, " + sender;
        }
        else if (index == 18)
        {
            line = "My sources say no, " + sender;
        }
        else if (index == 19)
        {
            line = "Outlook not so good, " + sender;
        }
        else if (index == 20)
        {
            line = "Very doubtful, " + sender;
        }
        return line;
    }

    public Boolean checkIfUserIsHere(String user, String channel)
    {
        foreach (String userName in Utils.getAllViewers(streamer))
        {
            if (user.Equals("(" + userName + ")")) // TODO
            {
                return true;
            }
        }
        return false;
    }

    private void onUserJoined(object s, OnUserJoinedArgs e) // TODO
    {
        String login = e.Username;
        if (!containsUser(users, login))
        {
            BotUser u = new BotUser(login, 0, false, false, false, 0, 0, null, 0, 0, 0);
            users.Add(u);
        }
        if (events.ContainsKey(login))
        {
            client.SendMessage(userVariables(events[login], channel, login, events[login], events[login], true));
        }
    }

    public static Boolean containsUser(List<BotUser> list, String user)
    {
        foreach (BotUser b in list)
        {
            if (b.username.Equals(user, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    public BotUser getBotUser(String username)
    {
        foreach (BotUser botUser in users)
        {
            if (botUser.username.Equals(username, StringComparison.InvariantCultureIgnoreCase))
            {
                return botUser;
            }
        }
        return null;
    }

}