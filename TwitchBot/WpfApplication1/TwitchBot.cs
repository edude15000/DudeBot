using System;
using System.Collections.Generic;
using System.IO;
using TwitchLib;
using TwitchLib.Models.Client;
using TwitchLib.Events.Client;
using System.Threading;
using WpfApplication1;
using Newtonsoft.Json;
using TwitchLib.Services;
using TwitchLib.Events.Services.FollowerService;
using TwitchLib.Events.PubSub;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TwitchLib.Interfaces;
using Cleverbot.Net;
using Newtonsoft.Json.Linq;
using System.Linq;

public class TwitchBot : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    protected bool SetField<T>(ref T field, T value, string propertyName)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
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
                "!removequote", "!deletequote", "!subcredits", "!givecredits", "!subsong", "!addfav", "!redeem", "!redeeminfo" };
        return new List<String>(result);
        // Add TO AS NEEDED
    }

    [JsonIgnore]
    public TwitchClient client;
    [JsonIgnore]
    ConnectionCredentials credentials;
    [JsonIgnore]
    public YoutubeHandler youtube = new YoutubeHandler();
    [JsonIgnore]
    public GoogleHandler google;
    [JsonIgnore]
    public GoogleHandler googleCustomSetlist;
    [JsonIgnore]
    private static TwitchAPI api;
    [JsonIgnore]
    private static FollowerService service;
    [JsonIgnore]
    private static List<String> backupUsersInChat = new List<String>();
    [JsonIgnore]
    public String Oauth;
    public String oauth
    {
        get => Oauth;
        set { SetField(ref Oauth, value, nameof(oauth)); }
    }
    [JsonIgnore]
    public String Streamer;
    public String streamer
    {
        get => Streamer;
        set { SetField(ref Streamer, value, nameof(streamer)); }
    }
    [JsonIgnore]
    public String Channel;
    public String channel
    {
        get => Channel;
        set { SetField(ref Channel, value, nameof(channel)); }
    }
    [JsonIgnore]
    public String BotName;
    public String botName
    {
        get => BotName;
        set { SetField(ref BotName, value, nameof(botName)); }
    }
    public Boolean reloadedAllFollowers { get; set; } = false;
    [JsonIgnore]
    public String FollowerMessage = "$user just followed the stream! Thank you!";
    public String followerMessage
    {
        get => FollowerMessage;
        set { SetField(ref FollowerMessage, value, nameof(followerMessage)); }
    }
    [JsonIgnore]
    public String FollowerMessageCommand = "";
    public String followerMessageCommand
    {
        get => FollowerMessageCommand;
        set { SetField(ref FollowerMessageCommand, value, nameof(followerMessageCommand)); }
    }
    [JsonIgnore]
    public String SubMessage = "$user just subscribed to the stream! Thank you!";
    public String subMessage
    {
        get => SubMessage;
        set { SetField(ref SubMessage, value, nameof(subMessage)); }
    }
    [JsonIgnore]
    public String SubMessageCommand = "";
    public String subMessageCommand
    {
        get => SubMessageCommand;
        set { SetField(ref SubMessageCommand, value, nameof(subMessageCommand)); }
    }
    [JsonIgnore]
    public String ResubMessage = "$user just resubscribed for $months months! Thank you!";
    public String resubMessage
    {
        get => ResubMessage;
        set { SetField(ref ResubMessage, value, nameof(resubMessage)); }
    }
    [JsonIgnore]
    public String ResubMessageCommand = "";
    public String resubMessageCommand
    {
        get => ResubMessageCommand;
        set { SetField(ref ResubMessageCommand, value, nameof(resubMessageCommand)); }
    }
    [JsonIgnore]
    public int RaidViewersRequired = 5;
    public int raidViewersRequired
    {
        get => RaidViewersRequired;
        set { SetField(ref RaidViewersRequired, value, nameof(raidViewersRequired)); }
    }
    [JsonIgnore]
    public String HostMessage = "Thanks for the host! $shoutout";
    public String hostMessage
    {
        get => HostMessage;
        set { SetField(ref HostMessage, value, nameof(hostMessage)); }
    }
    [JsonIgnore]
    public String HostMessageCommand = "";
    public String hostMessageCommand
    {
        get => HostMessageCommand;
        set { SetField(ref HostMessageCommand, value, nameof(hostMessageCommand)); }
    }
    [JsonIgnore]
    public String BitMessage = "Just received $bits bits from $user. That brings their total to $totalbits bits!";
    public String bitMessage
    {
        get => BitMessage;
        set { SetField(ref BitMessage, value, nameof(bitMessage)); }
    }
    [JsonIgnore]
    public String BitMessageCommand = "";
    public String bitMessageCommand
    {
        get => BitMessageCommand;
        set { SetField(ref BitMessageCommand, value, nameof(bitMessageCommand)); }
    }
    [JsonIgnore]
    public String RaidMessage = "We are getting raided! Thanks for the $viewers viewer raid! $shoutout";
    public String raidMessage
    {
        get => RaidMessage;
        set { SetField(ref RaidMessage, value, nameof(raidMessage)); }
    }
    [JsonIgnore]
    public String RaidMessageCommand = "";
    public String raidMessageCommand
    {
        get => RaidMessageCommand;
        set { SetField(ref RaidMessageCommand, value, nameof(raidMessageCommand)); }
    }
    [JsonIgnore]
    public String SubOnlyRequests = "Only sub requests are being accepted right now, $user";
    public String subOnlyRequests
    {
        get => SubOnlyRequests;
        set { SetField(ref SubOnlyRequests, value, nameof(subOnlyRequests)); }
    }
    [JsonIgnore]
    public String MinigameEndMessage = "Time has run out for the current game, guesses cannot be added now, $user!";
    public String minigameEndMessage
    {
        get => MinigameEndMessage;
        set { SetField(ref MinigameEndMessage, value, nameof(minigameEndMessage)); }
    }
    [JsonIgnore]
    public String Giveawaycommandname = "!raffle";
    public String giveawaycommandname
    {
        get => Giveawaycommandname;
        set { SetField(ref Giveawaycommandname, value, nameof(giveawaycommandname)); }
    }
    [JsonIgnore]
    public String SpreadsheetId;
    public String spreadsheetId
    {
        get => SpreadsheetId;
        set { SetField(ref SpreadsheetId, value, nameof(spreadsheetId)); }
    }
    [JsonIgnore]
    public String BotColor = "CadetBlue";
    public String botColor
    {
        get => BotColor;
        set { SetField(ref BotColor, value, nameof(botColor)); }
    }
    [JsonIgnore]
    public String EndMessage = "BYE EVERYONE";
    public String endMessage
    {
        get => EndMessage;
        set { SetField(ref EndMessage, value, nameof(endMessage)); }
    }
    [JsonIgnore]
    public String StartupMessage = "I AM ALIVE!";
    public String startupMessage
    {
        get => StartupMessage;
        set { SetField(ref StartupMessage, value, nameof(startupMessage)); }
    }
    [JsonIgnore]
    public int MinigameTimer = 90;
    public int minigameTimer
    {
        get => MinigameTimer;
        set { SetField(ref MinigameTimer, value, nameof(minigameTimer)); }
    }
    [JsonIgnore]
    public int TimerTotal = 20;
    public int timerTotal
    {
        get => TimerTotal;
        set { SetField(ref TimerTotal, value, nameof(timerTotal)); }
    }
    [JsonIgnore]
    public Boolean MinigameTriggered = false;
    public Boolean minigameTriggered
    {
        get => MinigameTriggered;
        set { SetField(ref MinigameTriggered, value, nameof(minigameTriggered)); }
    }
    [JsonIgnore]
    public int GuessingGamePayout = 0;
    public int guessingGamePayout
    {
        get => GuessingGamePayout;
        set { SetField(ref GuessingGamePayout, value, nameof(guessingGamePayout)); }
    }
    [JsonIgnore]
    public Boolean AllowLinks = true;
    public Boolean allowLinks
    {
        get => AllowLinks;
        set { SetField(ref AllowLinks, value, nameof(allowLinks)); }
    }
    [JsonIgnore]
    public String[] AllowedLinks;
    public String[] allowedLinks
    {
        get => AllowedLinks;
        set { SetField(ref AllowedLinks, value, nameof(allowedLinks)); }
    }
    [JsonIgnore]
    public String[] ProfanityBlackList;
    public String[] profanityBlackList
    {
        get => ProfanityBlackList;
        set { SetField(ref ProfanityBlackList, value, nameof(profanityBlackList)); }
    }
    [JsonIgnore]
    public Boolean TimeFinished = false;
    public Boolean timeFinished
    {
        get => TimeFinished;
        set { SetField(ref TimeFinished, value, nameof(timeFinished)); }
    }
    [JsonIgnore]
    public Boolean MinigameOn = true;
    public Boolean minigameOn
    {
        get => MinigameOn;
        set { SetField(ref MinigameOn, value, nameof(minigameOn)); }
    }
    [JsonIgnore]
    public Boolean AdventureToggle = true;
    public Boolean adventureToggle
    {
        get => AdventureToggle;
        set { SetField(ref AdventureToggle, value, nameof(adventureToggle)); }
    }
    [JsonIgnore]
    public Boolean CustomGameAdventure = true;
    public Boolean customGameAdventure
    {
        get => CustomGameAdventure;
        set { SetField(ref CustomGameAdventure, value, nameof(customGameAdventure)); }
    }
    [JsonIgnore]
    public Boolean StartAdventure = false;
    [JsonIgnore]
    public Boolean startAdventure
    {
        get => StartAdventure;
        set { SetField(ref StartAdventure, value, nameof(startAdventure)); }
    }
    [JsonIgnore]
    public Boolean StartAdventureCustom = false;
    [JsonIgnore]
    public Boolean startAdventureCustom
    {
        get => StartAdventureCustom;
        set { SetField(ref StartAdventureCustom, value, nameof(startAdventureCustom)); }
    }
    [JsonIgnore]
    public Boolean WaitForAdventureCoolDown = false;
    [JsonIgnore]
    public Boolean waitForAdventureCoolDown
    {
        get => WaitForAdventureCoolDown;
        set { SetField(ref WaitForAdventureCoolDown, value, nameof(waitForAdventureCoolDown)); }
    }
    [JsonIgnore]
    public Boolean WaitForAdventureCoolDownCustom = false;
    [JsonIgnore]
    public Boolean waitForAdventureCoolDownCustom
    {
        get => WaitForAdventureCoolDownCustom;
        set { SetField(ref WaitForAdventureCoolDownCustom, value, nameof(waitForAdventureCoolDownCustom)); }
    }
    [JsonIgnore]
    public Boolean RaffleInProgress = false;
    public Boolean raffleInProgress
    {
        get => RaffleInProgress;
        set { SetField(ref RaffleInProgress, value, nameof(raffleInProgress)); }
    }
    [JsonIgnore]
    public Boolean AutoShoutoutOnHost = true;
    public Boolean autoShoutoutOnHost
    {
        get => AutoShoutoutOnHost;
        set { SetField(ref AutoShoutoutOnHost, value, nameof(autoShoutoutOnHost)); }
    }
    [JsonIgnore]
    public Boolean GenericProfanityFilterEnabled = true;
    public Boolean genericProfanityFilterEnabled
    {
        get => GenericProfanityFilterEnabled;
        set { SetField(ref GenericProfanityFilterEnabled, value, nameof(genericProfanityFilterEnabled)); }
    }
    [JsonIgnore]
    public Boolean OpenRockSnifferOnStartUp = false;
    public Boolean openRockSnifferOnStartUp
    {
        get => OpenRockSnifferOnStartUp;
        set { SetField(ref OpenRockSnifferOnStartUp, value, nameof(openRockSnifferOnStartUp)); }
    }
    [JsonIgnore]
    public TextAdventure _TextAdventure = new TextAdventure();
    public TextAdventure textAdventure
    {
        get => _TextAdventure;
        set { SetField(ref _TextAdventure, value, nameof(textAdventure)); }
    }
    [JsonIgnore]
    public int AutoFollowPayout = 0;
    public int autoFollowPayout
    {
        get => AutoFollowPayout;
        set { SetField(ref AutoFollowPayout, value, nameof(autoFollowPayout)); }
    }
    [JsonIgnore]
    public int AutoSubPayout = 0;
    public int autoSubPayout
    {
        get => AutoSubPayout;
        set { SetField(ref AutoSubPayout, value, nameof(autoSubPayout)); }
    }
    [JsonIgnore]
    public Currency _Currency;
    public Currency currency
    {
        get => _Currency;
        set { SetField(ref _Currency, value, nameof(currency)); }
    }
    [JsonIgnore]
    public Image _Image = new Image();
    public Image image
    {
        get => _Image;
        set { SetField(ref _Image, value, nameof(image)); }
    }
    [JsonIgnore]
    public SoundEffect _SoundEffect = new SoundEffect();
    public SoundEffect soundEffect
    {
        get => _SoundEffect;
        set { SetField(ref _SoundEffect, value, nameof(soundEffect)); }
    }
    [JsonIgnore]
    public Quote _quote = new Quote();
    public Quote quote
    {
        get => _quote;
        set { SetField(ref _quote, value, nameof(quote)); }
    }
    public RequestSystem requestSystem { get; set; } = new RequestSystem();
    [JsonIgnore]
    public List<Command> SfxCommandList = new List<Command>();
    public List<Command> sfxCommandList
    {
        get => SfxCommandList;
        set { SetField(ref SfxCommandList, value, nameof(sfxCommandList)); }
    }
    [JsonIgnore]
    public List<Command> UserCommandList = new List<Command>();
    public List<Command> userCommandList
    {
        get => UserCommandList;
        set { SetField(ref UserCommandList, value, nameof(userCommandList)); }
    }
    [JsonIgnore]
    public List<Command> TimerCommandList = new List<Command>();
    public List<Command> timerCommandList
    {
        get => TimerCommandList;
        set { SetField(ref TimerCommandList, value, nameof(timerCommandList)); }
    }
    [JsonIgnore]
    public List<Command> BotCommandList = new List<Command>();
    public List<Command> botCommandList
    {
        get => BotCommandList;
        set { SetField(ref BotCommandList, value, nameof(botCommandList)); }
    }
    [JsonIgnore]
    public List<Command> ImageCommandList = new List<Command>();
    public List<Command> imageCommandList
    {
        get => ImageCommandList;
        set { SetField(ref ImageCommandList, value, nameof(imageCommandList)); }
    }
    [JsonIgnore]
    public List<Command> HotkeyCommandList = new List<Command>();
    public List<Command> hotkeyCommandList
    {
        get => HotkeyCommandList;
        set { SetField(ref HotkeyCommandList, value, nameof(hotkeyCommandList)); }
    }
    [JsonIgnore]
    public List<Command> OverrideCommandList = new List<Command>();
    public List<Command> overrideCommandList
    {
        get => OverrideCommandList;
        set { SetField(ref OverrideCommandList, value, nameof(overrideCommandList)); }
    }
    [JsonIgnore]
    public List<Command> RewardCommandList = new List<Command>();
    public List<Command> rewardCommandList
    {
        get => RewardCommandList;
        set { SetField(ref RewardCommandList, value, nameof(rewardCommandList)); }
    }
    [JsonIgnore]
    public List<Command> CommandList = new List<Command>();
    public List<Command> commandList
    {
        get => CommandList;
        set { SetField(ref CommandList, value, nameof(commandList)); }
    }
    [JsonIgnore]
    public List<AdventureScenario> AdventureScenarioList = new List<AdventureScenario>();
    public List<AdventureScenario> adventureScenarioList
    {
        get => AdventureScenarioList;
        set { SetField(ref AdventureScenarioList, value, nameof(adventureScenarioList)); }
    }
    [JsonIgnore]
    public List<BotUser> Users = new List<BotUser>();
    public List<BotUser> users
    {
        get => Users;
        set { SetField(ref Users, value, nameof(users)); }
    }
    [JsonIgnore]
    public List<BotUser> Supporters = new List<BotUser>();
    [JsonIgnore]
    public List<BotUser> supporters
    {
        get => Supporters;
        set { SetField(ref Supporters, value, nameof(supporters)); }
    }
    [JsonIgnore]
    public List<String> RaffleUsers = new List<String>();
    public List<String> raffleUsers
    {
        get => RaffleUsers;
        set { SetField(ref RaffleUsers, value, nameof(raffleUsers)); }
    }
    [JsonIgnore]
    public List<String> ExtraCommandNames = new List<String>();
    public List<String> extraCommandNames
    {
        get => ExtraCommandNames;
        set { SetField(ref ExtraCommandNames, value, nameof(extraCommandNames)); }
    }
    [JsonIgnore]
    public String LinkTimeoutMessage = "Please get permission from a mod before posting links, $user!";
    public String linkTimeoutMessage
    {
        get => LinkTimeoutMessage;
        set { SetField(ref LinkTimeoutMessage, value, nameof(linkTimeoutMessage)); }
    }
    [JsonIgnore]
    public String ProfanityTimeoutMessage = "Please be more respectful, $user!";
    public String profanityTimeoutMessage
    {
        get => ProfanityTimeoutMessage;
        set { SetField(ref ProfanityTimeoutMessage, value, nameof(profanityTimeoutMessage)); }
    }
    [JsonIgnore]
    public Dictionary<String, String> Events = new Dictionary<String, String>();
    public Dictionary<String, String> events
    {
        get => Events;
        set { SetField(ref Events, value, nameof(events)); }
    }
    [JsonIgnore]
    public Command GetViewerComm;
    public Command getViewerComm
    {
        get => GetViewerComm;
        set { SetField(ref GetViewerComm, value, nameof(getViewerComm)); }
    }
    [JsonIgnore]
    public int GuiBackgroundColor = 3;
    public int guiBackgroundColor
    {
        get => GuiBackgroundColor;
        set { SetField(ref GuiBackgroundColor, value, nameof(guiBackgroundColor)); }
    }
    [JsonIgnore]
    public int GuiTextColor = 1;
    public int guiTextColor
    {
        get => GuiTextColor;
        set { SetField(ref GuiTextColor, value, nameof(guiTextColor)); }
    }
    [JsonIgnore]
    public int GuiTextBlockColor = 0;
    public int guiTextBlockColor
    {
        get => GuiTextBlockColor;
        set { SetField(ref GuiTextBlockColor, value, nameof(guiTextBlockColor)); }
    }
    [JsonIgnore]
    public int GuiInputFontStyle = 0;
    public int guiInputFontStyle
    {
        get => GuiInputFontStyle;
        set { SetField(ref GuiInputFontStyle, value, nameof(guiInputFontStyle)); }
    }
    [JsonIgnore]
    public int GuiButtonColor = 0;
    public int guiButtonColor
    {
        get => GuiButtonColor;
        set { SetField(ref GuiButtonColor, value, nameof(guiButtonColor)); }
    }
    public int counter1 = 0, counter2 = 0, counter3 = 0, counter4 = 0, counter5 = 0;
    [JsonIgnore]
    public List<String> gameUser { get; set; } = new List<String>();
    [JsonIgnore]
    public List<Double> gameGuess { get; set; } = new List<Double>();
    [JsonIgnore]
    public long gameStartTime { get; set; }
    [JsonIgnore]
    public CleverbotSession cleverbotSession = null;
    [JsonIgnore]
    public String Eventlog = "";
    [JsonIgnore]
    public MessageEmoteCollection channelEmotes;
    [JsonIgnore]
    public String eventlog
    {
        get => Eventlog;
        set { SetField(ref Eventlog, value, nameof(eventlog)); }
    }

    public async void botStartUpAsync()
    {
        try
        {
            channel = streamer;
            credentials = new ConnectionCredentials(botName, oauth);
            client = new TwitchClient(credentials, channel);
            api = new TwitchAPI(Utils.twitchClientID);
            service = new FollowerService(api);
            service.OnNewFollowersDetected += onNewFollower;
            service.SetChannelByName(channel);
            await service.StartService();
            TwitchPubSub pubsub = new TwitchPubSub();
            pubsub.OnBitsReceived += onPubSubBitsReceived;
            pubsub.Connect();
            client.OnJoinedChannel += onJoinedChannel;
            channelEmotes = client.ChannelEmotes;
            client.OnMessageReceived += onMessageReceived;
            client.OnWhisperReceived += onWhisperReceived;
            client.OnNewSubscriber += onNewSubscriber;
            client.OnGiftedSubscription += Client_OnGiftedSubscription;
            client.OnUserJoined += onUserJoined;
            client.OnUserLeft += onUserLeft;
            client.OnReSubscriber += onReSubscriber;
            client.OverrideBeingHostedCheck = true;
            client.OnBeingHosted += onBeingHosted;
            if (!reloadedAllFollowers)
            {
                checkAtBeginningAsync(true);
                reloadedAllFollowers = true;
            }
            else
            {
                checkAtBeginningAsync(false);
            }
            removeAllUselessUsers();
            setClasses();
            Console.WriteLine("DudeBot Version: " + Utils.version + " Release Date: " + Utils.releaseDate);
            writeToEventLog("DudeBot Version: " + Utils.version + "\nRelease Date: " + Utils.releaseDate);
            textAdventure.startAdventuring(new List<String>(), (int)textAdventure.adventureStartTime * 1000, adventureScenarioList);
            textAdventure.startAdventuringCustom(new List<String>(), textAdventure.customadventurejointime * 1000);
            resetAllCommands();
            threads();
            cleverbotSession = await CleverbotSession.NewSessionAsync(Utils.cleverbotIOuser, Utils.cleverbotIOkey);
            try
            {
                client.SendMessage(startupMessage);
            }
            catch (Exception)
            {
                Thread.Sleep(1000);
                client.Connect();
                client.SendMessage(startupMessage);
            }
        }
        catch (Exception e1)
        {
            Console.WriteLine(e1.ToString());
            Utils.errorReport(e1);
        }
    }

    private void Client_OnGiftedSubscription(object sender, OnGiftedSubscriptionArgs e)
    {
        if (!subMessage.Equals(""))
        {
            if (subMessage.Contains("$user"))
            {
                client.SendMessage(subMessage.Replace("$user", e.GiftedSubscription.DisplayName));
            }
            else
            {
                client.SendMessage(streamer + " : " + subMessage);
            }
        }
        if (!subMessageCommand.Equals(""))
        {
            String message = subMessageCommand;
            if (message.Contains("$user"))
            {
                message = message.Replace("$user", e.GiftedSubscription.DisplayName);
            }
            if (!message.StartsWith("!"))
            {
                message = "!" + message;
            }
            processMessage(null, message, streamer);
        }
        foreach (BotUser botUser in users)
        {
            if (botUser.username.Equals(e.GiftedSubscription.DisplayName, StringComparison.InvariantCultureIgnoreCase))
            {
                botUser.subCredits += currency.creditsPerSub;
                botUser.sub = true;
                botUser.points += autoSubPayout;
                botUser.months = 1;
                writeToEventLog("SUB: " + e.GiftedSubscription.DisplayName);
                break;
            }
        }
        if (requestSystem.autoPromoteToDonatorOnSub)
        {
            autoPromoteToDonator(e.GiftedSubscription.DisplayName);
        }
        else if (requestSystem.autoPromoteToDonatorOnSub)
        {
            autoPromoteToVIP(e.GiftedSubscription.DisplayName);
        }
    }

    public void removeAllUselessUsers()
    {
        List<BotUser> list = new List<BotUser>();
        foreach (BotUser botUser in users)
        {
            if (botUser.time > 10 || botUser.points > 10 || botUser.mod || botUser.moneyDonated > 0 || botUser.months > 0 || botUser.numRequests > 0 || botUser.bitsDonated > 0 || botUser.follower || botUser.sub)
            {
                list.Add(botUser);
            }
        }
        users = list;
    }

    private void onUserLeft(object sender, OnUserLeftArgs e)
    {
        writeToEventLog("PART: " + e.Username);
        if (backupUsersInChat.Contains(e.Username))
        {
            backupUsersInChat.Remove(e.Username);
        }
    }

    public List<String> getAllViewers(String streamer)
    {
        List<String> users = new List<String>();
        String info = Utils.callURL("http://tmi.twitch.tv/group/user/" + streamer + "/chatters");
        if (info.Equals(""))
        {
            return backupUsersInChat;
        }
        try
        {
            var a = JsonConvert.DeserializeObject<dynamic>(info);
            a = a["chatters"];
            JArray viewers = a["viewers"];
            JArray staff = a["staff"];
            JArray admins = a["admins"];
            JArray global_mods = a["global_mods"];
            JArray moderators = a["moderators"];
            if (moderators != null)
            {
                int len = moderators.Count;
                String[] items = moderators.Select(jv => (String)jv).ToArray();
                for (int i = 0; i < len; i++)
                {
                    if (!users.Contains(items[i]))
                    {
                        users.Add(items[i].ToString());
                    }
                }
            }
            if (viewers != null)
            {
                int len = viewers.Count;
                String[] items = viewers.Select(jv => (String)jv).ToArray();
                for (int i = 0; i < len; i++)
                {
                    if (!users.Contains(items[i]))
                    {
                        users.Add(items[i].ToString());
                    }
                }
            }
            if (staff != null)
            {
                int len = staff.Count;
                String[] items = staff.Select(jv => (String)jv).ToArray();
                for (int i = 0; i < len; i++)
                {
                    if (!users.Contains(items[i]))
                    {
                        users.Add(items[i].ToString());
                    }
                }
            }
            if (admins != null)
            {
                int len = admins.Count;
                String[] items = admins.Select(jv => (String)jv).ToArray();
                for (int i = 0; i < len; i++)
                {
                    if (!users.Contains(items[i]))
                    {
                        users.Add(items[i].ToString());
                    }
                }
            }
            if (global_mods != null)
            {
                int len = global_mods.Count;
                String[] items = global_mods.Select(jv => (String)jv).ToArray();
                for (int i = 0; i < len; i++)
                {
                    if (!users.Contains(items[i]))
                    {
                        users.Add(items[i].ToString());
                    }
                }
            }
        }
        catch (Exception e1)
        {
            Utils.errorReport(e1);
            Console.WriteLine(e1.ToString());
        }
        return users;
    }

    public void writeToEventLog(String str)
    {
        eventlog += Utils.getTime() + "---\n" + str + "\n\n";
    }

    private void onBeingHosted(object sender, OnBeingHostedArgs e)
    {
        if (autoShoutoutOnHost && !e.IsAutoHosted)
        {
            String message = "", message2 = "";
            if (e.Viewers < raidViewersRequired)
            {
                message = hostMessage;
                message2 = hostMessageCommand;
                writeToEventLog("HOST: " + e.HostedByChannel + " (" + e.Viewers + " viewers)");
            }
            else
            {
                message = raidMessage;
                message2 = raidMessageCommand;
                writeToEventLog("RAID: " + e.HostedByChannel + " (" + e.Viewers + " viewers)");
            }
            if (message.Contains("shoutout"))
            {
                message = message.Replace("shoutout", userVariables("$shoutout", "#" + streamer, streamer, e.HostedByChannel, "!shoutout " + e.HostedByChannel, true, null));
            }
            else if (message.Contains("$user"))
            {
                message = message.Replace("$user", e.HostedByChannel);
            }
            if (message.Contains("shoutout"))
            {
                message = message.Replace("$viewers", e.Viewers.ToString());
            }
            if (!message.Equals(""))
            {
                client.SendMessage(message);
            }
            if (!message2.Equals(""))
            {
                if (message2.Contains("shoutout"))
                {
                    message2 = message2.Replace("shoutout", userVariables("$shoutout", "#" + streamer, streamer, e.HostedByChannel, "!shoutout " + e.HostedByChannel, true, null));
                }
                else if (message2.Contains("$user"))
                {
                    message2 = message2.Replace("$user", e.HostedByChannel);
                }
                if (message2.Contains("shoutout"))
                {
                    message2 = message2.Replace("$viewers", e.Viewers.ToString());
                }
                if (!message2.StartsWith("!"))
                {
                    message2 = "!" + message2;
                }
                processMessage(null, message2, streamer);
            }
        }
    }

    private void onReSubscriber(object sender, OnReSubscriberArgs e)
    {
        String message = resubMessage;
        if (message.Contains("$user"))
        {
            message = message.Replace("$user", e.ReSubscriber.DisplayName);
        }
        if (message.Contains("$months"))
        {
            message = message.Replace("$months", e.ReSubscriber.Months.ToString());
        }
        if (!message.Equals(""))
        {
            client.SendMessage(message);
        }
        if (!resubMessageCommand.Equals(""))
        {
            message = resubMessageCommand;
            if (message.Contains("$user"))
            {
                message = message.Replace("$user", e.ReSubscriber.DisplayName);
            }
            if (message.Contains("$months"))
            {
                message = message.Replace("$months", e.ReSubscriber.Months.ToString());
            }
            if (!message.StartsWith("!"))
            {
                message = "!" + message;
            }
            processMessage(null, message, streamer);
        }
        foreach (BotUser botUser in users)
        {
            if (botUser.username.Equals(e.ReSubscriber.DisplayName, StringComparison.InvariantCultureIgnoreCase))
            {
                botUser.subCredits += currency.creditsPerSub;
                botUser.sub = true;
                botUser.points += autoSubPayout;
                botUser.months = e.ReSubscriber.Months;
                writeToEventLog("RESUB: " + e.ReSubscriber.DisplayName + " (" + e.ReSubscriber.Months + " months)");
                break;
            }
        }
        if (requestSystem.autoPromoteToDonatorOnSub)
        {
            autoPromoteToDonator(e.ReSubscriber.DisplayName);
        }
        else if (requestSystem.autoPromoteToDonatorOnSub)
        {
            autoPromoteToVIP(e.ReSubscriber.DisplayName);
        }
    }

    private void onPubSubBitsReceived(object sender, OnBitsReceivedArgs e)
    {
        String message = bitMessage;
        if (message.Contains("$user"))
        {
            message.Replace("$user", e.Username);
        }
        if (message.Contains("$bits"))
        {
            message.Replace("$bits", e.BitsUsed.ToString());
        }
        if (message.Contains("$totalbits"))
        {
            message.Replace("$totalbits", e.TotalBitsUsed.ToString());
        }
        client.SendMessage(message);
        if (!bitMessageCommand.Equals(""))
        {
            message = bitMessageCommand;
            if (message.Contains("$user"))
            {
                message.Replace("$user", e.Username);
            }
            if (message.Contains("$bits"))
            {
                message.Replace("$bits", e.BitsUsed.ToString());
            }
            if (message.Contains("$totalbits"))
            {
                message.Replace("$totalbits", e.TotalBitsUsed.ToString());
            }
            if (!message.StartsWith("!"))
            {
                message = "!" + message;
            }
            processMessage(null, message, streamer);
        }
        writeToEventLog("BITS RECEIVED: " + e.Username + " (" + e.BitsUsed + " bits)");
        BotUser user = getBotUser(e.Username);
        if (user != null)
        {
            user.bitsDonated = e.TotalBitsUsed;
        }
        if (requestSystem.autoPromoteToDonatorOnBitDonation && e.BitsUsed >= requestSystem.autoPromoteToDonatorOnBitDonationMinimum)
        {
            autoPromoteToDonator(e.Username);
        }
        else if (requestSystem.autoPromoteToVipOnBitDonation && e.BitsUsed >= requestSystem.autoPromoteToVipOnBitDonationMinimum)
        {
            autoPromoteToVIP(e.Username);
        }
    }

    public void autoPromoteToVIP(String user)
    {
        for (int j = 0; j < requestSystem.songList.Count; j++)
        {
            Song s = requestSystem.songList[j];
            if (s.requester.Equals(user, StringComparison.InvariantCultureIgnoreCase) && s.level.Equals(""))
            {
                requestSystem.songList.RemoveAt(j);
                requestSystem.addVip(channel, s.name, user, null);
                client.SendMessage("Song '" + s.name + "' has been promoted to VIP, " + user + "!");
                BotUser u = getBotUser(user);
                if (u != null)
                {
                    u.points += requestSystem.regSongCost;
                }
                return;
            }
        }
    }

    public void autoPromoteToDonator(String user)
    {
        for (int j = 0; j < requestSystem.songList.Count; j++)
        {
            Song s = requestSystem.songList[j];
            if (s.requester.Equals(user, StringComparison.InvariantCultureIgnoreCase) && s.level.Equals(""))
            {
                requestSystem.songList.RemoveAt(j);
                requestSystem.addDonator(channel, s.name, user, null);
                client.SendMessage("Song '" + s.name + "' has been promoted to $$$, " + user + "!");
                return;
            }
        }
        for (int j = 0; j < requestSystem.songList.Count; j++)
        {
            Song s = requestSystem.songList[j];
            if (s.requester.Equals(user, StringComparison.InvariantCultureIgnoreCase) && s.level.Equals("VIP"))
            {
                requestSystem.songList.RemoveAt(j);
                requestSystem.addDonator(channel, s.name, user, null);
                client.SendMessage("VIP Song '" + s.name + "' has been promoted to $$$, " + user + "!");
                return;
            }
        }
    }

    private void onNewFollower(object sender, OnNewFollowersDetectedArgs e)
    {
        if (!followerMessage.Equals(""))
        {
            String message = followerMessageCommand;
            if (followerMessage.Contains("$user"))
            {
                List<String> str = new List<String>();
                foreach (IFollow follower in e.NewFollowers)
                {
                    str.Add(follower.User.Name);
                }
                client.SendMessage(followerMessage.Replace("$user", String.Join(", ", str)));
            }
            else
            {
                client.SendMessage(followerMessage);
            }
        }
        if (!followerMessageCommand.Equals(""))
        {
            String message = followerMessageCommand;
            if (message.Contains("$user"))
            {
                List<String> str = new List<String>();
                foreach (IFollow follower in e.NewFollowers)
                {
                    str.Add(follower.User.Name);
                }
                message = message.Replace("$user", String.Join(", ", str));
            }
            if (!message.StartsWith("!"))
            {
                message = "!" + message;
            }
            processMessage(null, message, streamer);
        }
        foreach (IFollow follower in e.NewFollowers)
        {
            BotUser b = getBotUser(follower.User.Name);
            if (b != null)
            {
                b.follower = true;
                if (!b.receivedFollowPayout)
                {
                    b.points += autoFollowPayout;
                }
            }
            else
            {
                BotUser user = new BotUser(follower.User.Name, 0, false, true, false, 0, 0, null, 0, 0, 0);
                user.receivedFollowPayout = true;
                user.points += autoFollowPayout;
                users.Add(user);
            }
            writeToEventLog("FOLLOW: " + follower.User.Name);
        }
    }

    public async void checkAtBeginningAsync(Boolean onstart)
    {
        String channel_id = "";
        String info = Utils.callURL("https://api.twitch.tv/kraken/channels/" + channel);
        var a = JsonConvert.DeserializeObject<dynamic>(info);
        try
        {
            channel_id = a["_id"];
        }
        catch (Exception)
        {
            return;
        }
        if (onstart)
        {
            var channelFollowers = await api.Channels.v5.GetAllFollowersAsync(channel_id);
            foreach (var u in channelFollowers)
            {
                foreach (BotUser user in users)
                {
                
                    if (u.User.Name.Equals(user.username, StringComparison.InvariantCultureIgnoreCase))
                    {
                        user.follower = true;
                        break;
                    }
                }
                users.Add(new BotUser(u.User.Name, 0, false, true, false, 0, 0, null, 0, 0, 0));
            }
        }
        else
        {
            var channelFollowers = await api.Channels.v5.GetChannelFollowersAsync(channel_id);
            foreach (BotUser user in users)
            {
                foreach (var u in channelFollowers.Follows)
                {
                    if (u.User.Name.Equals(user.username, StringComparison.InvariantCultureIgnoreCase))
                    {
                        user.follower = true;
                        break;
                    }
                }
            }
        }
        /*
        var allSubscriptions = await api.Channels.v5.GetChannelSubscribersAsync(channel_id); // TODO
        foreach (BotUser user in users)
        {
            foreach (var u in allSubscriptions.Subscriptions)
            {
                if (u.User.Name.Equals(user.username, StringComparison.InvariantCultureIgnoreCase))
                {
                    user.sub = true;
                    if (!user.sub)
                    {
                        user.months = 0;
                    }
                    break;
                }
            }
        }
        */
    }

    private void onMessageReceived(object sender, OnMessageReceivedArgs e)
    {
        processMessage(e, "", "");
    }

    public void botDisconnect()
    {
        client.Disconnect();
        client.LeaveChannel(channel);
        writeToEventLog("BOT DISCONNECTED");
    }

    private void onJoinedChannel(object sender, OnJoinedChannelArgs e)
    {
        // TODO ?
    }

    private void onWhisperReceived(object sender, OnWhisperReceivedArgs e)
    {
        // TODO ?
    }

    private void onNewSubscriber(object sender, OnNewSubscriberArgs e)
    {
        if (!subMessage.Equals(""))
        {
            if (subMessage.Contains("$user"))
            {
                client.SendMessage(subMessage.Replace("$user", e.Subscriber.DisplayName));
            }
            else
            {
                client.SendMessage(streamer + " : " + subMessage);
            }
        }
        if (!subMessageCommand.Equals(""))
        {
            String message = subMessageCommand;
            if (message.Contains("$user"))
            {
                message = message.Replace("$user", e.Subscriber.DisplayName);
            }
            if (!message.StartsWith("!"))
            {
                message = "!" + message;
            }
            processMessage(null, message, streamer);
        }
        foreach (BotUser botUser in users)
        {
            if (botUser.username.Equals(e.Subscriber.DisplayName, StringComparison.InvariantCultureIgnoreCase))
            {
                botUser.subCredits += currency.creditsPerSub;
                botUser.sub = true;
                botUser.points += autoSubPayout;
                botUser.months = 1;
                writeToEventLog("SUB: " + e.Subscriber.DisplayName);
                break;
            }
        }
        if (requestSystem.autoPromoteToDonatorOnSub)
        {
            autoPromoteToDonator(e.Subscriber.DisplayName);
        }
        else if (requestSystem.autoPromoteToDonatorOnSub)
        {
            autoPromoteToVIP(e.Subscriber.DisplayName);
        }
    }

    public async void setClasses()
    {
        requestSystem.bot = this;
        quote.bot = this;
        if (currency != null)
        {
            currency.users = users;
        }
        if (spreadsheetId != "")
        {
            google = new GoogleHandler(spreadsheetId);
        }
        if (requestSystem.googleSheetSetlistId != "")
        {
            googleCustomSetlist = new GoogleHandler(requestSystem.googleSheetSetlistId);
        }
        if (adventureScenarioList.Count == 0)
        {
            adventureScenarioList = textAdventure.stockAdventures();
        }
        requestSystem.formattedTotalTime = requestSystem.formatTotalTime();
        requestSystem.songListLength = requestSystem.songList.Count;
        requestSystem.songsPlayedThisStream = 0;
        requestSystem.setSongsIfUserIsHere();
        requestSystem.setIndexesForSongs();
        await requestSystem.getCookieFromCF();
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
        hotkeyCommandList = getCommands("hotkey");
        overrideCommandList = getCommands("override");
        rewardCommandList = getCommands("reward");
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
        double start_time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        int i = 0;
        while (true)
        {
            try
            {
                if (timerCommandList.Count > 0)
                {
                    if ((DateTimeOffset.Now.ToUnixTimeMilliseconds() - start_time) >= (timerTotal * 60000))
                    {
                        start_time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
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
                        if (i >= timerCommandList.Count - 1)
                        {
                            i = 0;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Utils.errorReport(e);
                Console.WriteLine(e.ToString());
            }
            Thread.Sleep(5000);
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
                currency.bonusall(getAllViewers(streamer), true, 0);
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

    public void customAdventureThread(String sender)
    {
        textAdventure.startCustom(sender);
        String winner = textAdventure.selectWinnerCustom();
        String winMessage = textAdventure.customGameEndMessage;
        if (winMessage.Contains("$user"))
        {
            winMessage = winMessage.Replace("$user", winner);
        }
        int winningAmount = (textAdventure.usersCustom.Count * textAdventure.customadventurejoincost);
        if (winMessage.Contains("$amount"))
        {
            winMessage = winMessage.Replace("$amount", winningAmount.ToString() + " " + currency.currencyName);
        }
        client.SendMessage(winMessage);
        if (currency.toggle)
        {
            client.SendMessage(currency.bonus(winner, winningAmount));
        }
        startAdventureCustom = false;
        textAdventure.lastAdventureCustom = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        waitForAdventureCoolDownCustom = true;
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
        textAdventure.lastAdventure = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        waitForAdventureCoolDown = true;
    }

    public void imageThread(String message, String sender, OnMessageReceivedArgs e)
    {
        image.imageCOMMANDS(message, channel, getBotUser(sender), imageCommandList, this, e);
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
        waitForAdventureCoolDownCustom = false;
        minigameTriggered = false;
        startAdventure = false;
        StartAdventureCustom = false;
        textAdventure.allowUserAdds = true;
        textAdventure.enoughPlayers = false;
        textAdventure.allowUserAddsCustom = true;
        textAdventure.startTimerInMS = 0;
        textAdventure.startTimerInMSCustom = 0;
        image.imageStartTime = 0;
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
    
    public void processMessage(OnMessageReceivedArgs e, String mes, String sen)
    {
        String message = "";
        String sender = "";
        String noEmoteMessage = "";
        Boolean mod = false;
        if (e != null) {
            message = e.ChatMessage.Message;
            sender = e.ChatMessage.Username;
            noEmoteMessage = e.ChatMessage.EmoteReplacedMessage;
            mod = e.ChatMessage.IsModerator;
        }
        else
        {
            message = mes;
            sender = sen;
        }
        if (sender.Equals(streamer))
        {
            mod = true;
        }
        if (!sender.Equals(Utils.botMaker, StringComparison.InvariantCultureIgnoreCase) && !sender.Equals(streamer, StringComparison.InvariantCultureIgnoreCase) 
            && !mod && !message.StartsWith("!"))
        {
            if (!moderation(message, sender))
            {
                return;
            }
        }
        if (sender.Equals(botName, StringComparison.InvariantCultureIgnoreCase))
        {
            return;
        }
        String temp = message.ToLower();
        if (minigameTriggered && !timeFinished)
        {
            if (gameStartTime + (minigameTimer * 1000) < DateTimeOffset.Now.ToUnixTimeMilliseconds())
            {
                timeFinished = true;
            }
        }

        // SFX
        for (int i = 0; i < sfxCommandList.Count; i++)
        {
            if (temp.StartsWith(sfxCommandList[i].input[0]))
            {
                soundEffect.sfxCOMMANDS(message, channel, getBotUser(sender), sfxCommandList, this, e);
            }
        }

        // Images
        for (int i = 0; i < imageCommandList.Count; i++)
        {
            if (temp.StartsWith(imageCommandList[i].input[0]))
            {
                var t = new Thread(() => imageThread(message, sender, e));
                t.Start();
            }
        }
        // USER COMMANDS
        for (int i = 0; i < userCommandList.Count; i++)
        {
            String temp2 = temp + " ";
            if (temp2.StartsWith(userCommandList[i].input[0] + " ")
                    || temp.Equals(userCommandList[i].input[0], StringComparison.InvariantCultureIgnoreCase))
            {
                userCOMMANDS(message, channel, sender, e);
                return;
            }
        }
        // USER TIMER COMMANDS
        for (int i = 0; i < timerCommandList.Count; i++)
        {
            String temp2 = temp + " ";
            if (temp2.StartsWith(timerCommandList[i].input[0] + " ")
                    || temp.Equals(timerCommandList[i].input[0], StringComparison.InvariantCultureIgnoreCase))
            {
                userCOMMANDS(message, channel, sender, e);
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
                    requestSystem.requestCOMMAND(message, channel, sender, noEmoteMessage, e);
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
                    requestSystem.nextCOMMAND(message, channel, sender, e);
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
                    requestSystem.clearCOMMAND(message, channel, sender, e);
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
                    requestSystem.editCOMMAND(message, channel, sender, noEmoteMessage, e);
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
                    requestSystem.addvipCOMMAND(message, channel, sender, noEmoteMessage, e);
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
                    requestSystem.addtopCOMMAND(message, channel, sender, noEmoteMessage, e);
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
                    requestSystem.getTotalSongCOMMAND(message, channel, sender, e);
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
                    requestSystem.songlistCOMMAND(message, channel, sender, e);
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
                    requestSystem.getCurrentSongCOMMAND(message, channel, sender, e);
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
                    requestSystem.triggerRequestsCOMMAND(message, channel, sender, e);
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
                    requestSystem.getNextSongCOMMAND(message, channel, sender, e);
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
        if (message.Equals(currency.currencyCommand, StringComparison.InvariantCultureIgnoreCase) || message.Equals("!points", StringComparison.InvariantCultureIgnoreCase))
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
                        || mod)
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
                        || mod)
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
                                    getAllViewers(streamer), false, -1 * Int32.Parse(temp2)));
                        }
                        else
                        {
                            client.SendMessage(currency.bonusall(getAllViewers(streamer), false, Int32.Parse(temp2)));
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
                    || mod)
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
                    || mod)
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
                Console.WriteLine(e1.ToString());
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

        if (message.ToLower().Equals("!vipsong", StringComparison.InvariantCultureIgnoreCase))
        {
            if (currency.toggle)
            {
                if (currency.vipSongToggle)
                {
                    try
                    {
                        Song song = null;
                        foreach (Song s in requestSystem.songList)
                        {
                            if (s.requester.Equals(sender, StringComparison.InvariantCultureIgnoreCase) && s.level.Equals(""))
                            {
                                song = s;
                                break;
                            }
                        }
                        if (song == null)
                        {
                            client.SendMessage("You can promote your next song in the queue to a VIP song by typing '!vipsong', " +
                                "or you can request a new VIP song by typing '!vipsong artist - song'. A VIP song costs " + currency.vipSongCost
                                    + " " + currency.currencyName + " to redeem, " + sender + "!");
                            return;
                        }
                        int option = currency.vipsong(sender);
                        if (option == 1)
                        {
                            requestSystem.songList.Remove(song);
                            requestSystem.addVip(channel, song.name, sender, noEmoteMessage);
                            client.SendMessage(sender + " cashed in " + currency.vipSongCost
                                    + " " + currency.currencyName + " to upgrade their next song '" + song.name + "' to a VIP song!");
                        }
                        else if (option == 0)
                        {
                            client.SendMessage("You need " + currency.vipSongCost + " "
                                    + currency.currencyName + " to buy a VIP song, " + sender + "!");
                        }
                        else if (option == -1)
                        {
                            client.SendMessage("You may redeem a VIP song once every "
                                    + currency.vipRedeemCoolDownMinutes + " minutes, " + sender + "!");
                        }
                        else if (option == -2)
                        {
                            double timeLeft = Utils.getMinsRemaining(currency.vipCooldownOverallTracker, currency.vipRedeemCoolDownMinutesOverall);
                            client.SendMessage("The VIP redeem command is on cooldown, please wait " + timeLeft + " minutes, " + sender + "!");
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

        if (message.ToLower().StartsWith("!vipsong"))
        {
            if (currency.toggle)
            {
                if (currency.vipSongToggle)
                {
                    try
                    {
                        int option = currency.vipsong(sender);
                        if (option == 1)
                        {
                            requestSystem.addVip(channel, Utils.getFollowingText(message), sender, noEmoteMessage);
                            client.SendMessage(sender + " cashed in " + currency.vipSongCost
                                    + " " + currency.currencyName + " for a VIP song! '"
                                    + Utils.getFollowingText(message)
                                    + "' has been added as a VIP song to the song list!");
                        }
                        else if (option == 0)
                        {
                            client.SendMessage("You need " + currency.vipSongCost + " "
                                    + currency.currencyName + " to buy a VIP song, " + sender + "!");
                        }
                        else if (option == -1)
                        {
                            client.SendMessage("You may redeem a VIP song once every "
                                    + currency.vipRedeemCoolDownMinutes + " minutes, " + sender + "!");
                        }
                        else if (option == -2)
                        {
                            double timeLeft = Utils.getMinsRemaining(currency.vipCooldownOverallTracker, currency.vipRedeemCoolDownMinutesOverall);
                            client.SendMessage("The VIP redeem command is on cooldown, please wait " + timeLeft + " minutes, " + sender + "!");
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
                    requestSystem.editMySongCOMMAND(message, channel, sender, noEmoteMessage, e);
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
                    requestSystem.removeMySong(message, channel, sender, e);
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
                    requestSystem.addDonatorCOMMAND(message, channel, sender, noEmoteMessage, e);
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
                requestSystem.checkSongPositionCOMMAND(message, channel, sender, e);
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
                    requestSystem.randomizerCommand(message, channel, sender, e);
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
                    requestSystem.chooseRandomFavorite(message, channel, sender, noEmoteMessage, e);
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
            if (sender.Equals(streamer) || mod)
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
                viewerCOMMAND(message, channel, sender, e);
                return;
            }
        }
        // UndoNext COMMAND
        if (message.Equals("!undonext", StringComparison.InvariantCultureIgnoreCase) || message.Equals("!undoskip", StringComparison.InvariantCultureIgnoreCase))
        {
            if (sender.Equals(Utils.botMaker) || sender.Equals(streamer)
                    || mod)
            {
                if (requestSystem.lastSong != null)
                {
                    try
                    {
                        requestSystem.doNotWriteToHistory = true;
                        requestSystem.addtopCOMMAND(requestSystem.addtopComm.input[0] + " " + requestSystem.lastSong,
                                channel, sender, noEmoteMessage, e);
                        requestSystem.songsPlayedThisStream -= 1;
                        requestSystem.songsPlayedTotal -= 1;
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
                quote.quotesSystem(message, channel, sender, streamer, users, e);
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
                    || mod)
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
            if (sender.Equals(streamer) || mod
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
                                + minigameTimer + " seconds to enter a guess by typing '!guess amount'");
                        minigameTriggered = true;
                        gameStartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
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
            if (sender.Equals(streamer) || mod
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
                            if (guessingGamePayout != 0)
                            {
                                temp2 = guessingGamePayout.ToString();
                            }
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
            if (sender.Equals(Utils.botMaker) || sender.Equals(streamer) || mod)
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
            if (sender.Equals(Utils.botMaker) || sender.Equals(streamer) || mod)
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
                    if (textAdventure.lastAdventure + (textAdventure.adventureCoolDown * 60000) <= DateTimeOffset.Now.ToUnixTimeMilliseconds())
                    {
                        waitForAdventureCoolDown = false;
                    }
                    else
                    {
                        double timeLeft = Utils.getMinsRemaining(textAdventure.lastAdventure, textAdventure.adventureCoolDown);
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
                        if (textAdventure.choice == null)
                        { 
                            return;
                        }
                        client.SendMessage(textAdventure.choice.startMessage.Replace("$user", sender)
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
                                client.SendMessage("Please try again in a little bit, " + sender + "!");
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
        if (message.Equals(textAdventure.customGameCommand, StringComparison.InvariantCultureIgnoreCase))
        {
            if (customGameAdventure)
            {
                if (waitForAdventureCoolDownCustom)
                {
                    if (textAdventure.lastAdventureCustom + (textAdventure.customadventurecooldowntime * 60000) <= DateTimeOffset.Now.ToUnixTimeMilliseconds())
                    {
                        waitForAdventureCoolDownCustom = false;
                    }
                    else
                    {
                        double timeLeft = Math.Abs(Math.Ceiling((Double)((DateTimeOffset.Now.ToUnixTimeMilliseconds()
                                - (textAdventure.lastAdventureCustom + (textAdventure.customadventurecooldowntime * 60000)))
                                / 60000)));
                        if (timeLeft == 0.0)
                        {
                            timeLeft = 1.0;
                        }
                        if (timeLeft == 1.0)
                        {
                            client.SendMessage("We must prepare ourselves before another " + textAdventure.customGameCommand.Replace("!", "") + ". Please try again in "
                            + timeLeft + " minute, " + sender + "!");
                        }
                        else
                        {
                            client.SendMessage("We must prepare ourselves before another " + textAdventure.customGameCommand.Replace("!", "") + ". Please try again in "
                            + timeLeft + " minutes, " + sender + "!");
                        }
                        return;
                    }
                }
                if (!waitForAdventureCoolDownCustom)
                {
                    if (!startAdventureCustom)
                    {
                        if (textAdventure.customGameStartByModOnly && !(sender.Equals(Utils.botMaker) || sender.Equals(streamer)))
                        {
                            return;
                        }
                        startAdventureCustom = true;
                        String startMessage = textAdventure.customGameStartMessage;
                        if (startMessage.Contains("$user"))
                        {
                            startMessage = startMessage.Replace("$user", sender);
                        }
                        if (startMessage.Contains("$cost"))
                        {
                            startMessage = startMessage.Replace("$cost", textAdventure.customadventurejoincost.ToString() + " " + currency.currencyName);
                        }
                        client.SendMessage(startMessage);
                        var t = new Thread(() => customAdventureThread(sender));
                        t.Start();
                        return;
                    }
                    if (startAdventureCustom)
                    {
                        try
                        {
                            BotUser b = getBotUser(sender);
                            if (b == null || (b != null && b.points < textAdventure.customadventurejoincost))
                            {
                                client.SendMessage("You need " + textAdventure.customadventurejoincost + " " + currency.currencyName + " to join the game, " + sender + "!");
                                return;
                            }
                            int choice = (textAdventure.addUserCustom(sender));
                            if (choice == 0)
                            {
                                client.SendMessage("Please try again in a little bit, " + sender + "!");
                                startAdventureCustom = false;
                            }
                            else if (choice == 1)
                            {
                                b.points -= textAdventure.customadventurejoincost;
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
            if (sender.Equals(Utils.botMaker) || sender.Equals(streamer) || mod)
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
            requestSystem.removeSongCOMMAND(sender, channel, streamer, users, message, temp, e);
        }

        if (message.Trim().ToLower().StartsWith("!editcom !")
                || message.Trim().ToLower().StartsWith("!updatecom !"))
        {
            if (sender.Equals(streamer) || mod
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
            if (sender.Equals(streamer) || mod
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
            if (sender.Equals(streamer) || mod
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
            if (sender.Equals(streamer) || mod || sender.Equals(Utils.botMaker))
            {
                client.SendMessage("To promote a user's song, type '!promote @user', " + sender + "!");
                return;
            }
        }
        if (message.Trim().ToLower().StartsWith("!promote "))
        {
            requestSystem.promoteSongCommand(sender, channel, streamer, users, message, noEmoteMessage, e);
        }

        if (message.Trim().ToLower().Equals("!subcredits", StringComparison.InvariantCultureIgnoreCase))
        {
            client.SendMessage(currency.getSubCredits(sender));
            return;
        }

        if (message.Trim().ToLower().StartsWith("!givecredits"))
        {
            if (sender.Equals(streamer) || mod)
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
                    requestSystem.addDonator(channel, Utils.getFollowingText(message), sender, noEmoteMessage);
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

        if (message.Equals("!addfav", StringComparison.InvariantCultureIgnoreCase))
        {
            if (sender.Equals(streamer) || mod || sender.Equals(Utils.botMaker))
            {
                requestSystem.addCurrentSongToFavList();
                return;
            }
        }

        //Redeem rewards
        if (message.Equals("!redeem", StringComparison.InvariantCultureIgnoreCase) || message.Equals("!redeeminfo", StringComparison.InvariantCultureIgnoreCase))
        {
            List<String> list = new List<String>();
            foreach (Command c in rewardCommandList)
            {
                list.Add(c.input[0] + " (" + c.costToUse + ")");
            }
            client.SendMessage("To redeem a reward, type 'redeem [reward_name]'. Rewards: " + String.Join(", ", list));
            return;

        }
        if (message.StartsWith("!redeem ", StringComparison.InvariantCultureIgnoreCase))
        {
            for (int i = 0; i < rewardCommandList.Count; i++)
            {
                if (Utils.getFollowingText(message).Equals(rewardCommandList[i].input[0], StringComparison.InvariantCultureIgnoreCase))
                {
                    foreach (BotUser b in users)
                    {
                        if (b.username.Equals(sender, StringComparison.InvariantCultureIgnoreCase)) {
                            if (b.points >= rewardCommandList[i].costToUse)
                            {
                                b.points -= rewardCommandList[i].costToUse;
                                client.SendMessage(userVariables(rewardCommandList[i].output, channel, sender,
                                    rewardCommandList[i].output, rewardCommandList[i].output, false, e));
                                return;
                            }
                            else
                            {
                                client.SendMessage("This reward costs " + rewardCommandList[i].costToUse + " " + currency.currencyName + " to reedem. You cannot afford it, " + sender);
                                return;
                            }
                        }
                    }
                }
            }
        }
        if (message.Trim().ToLower().Equals("!modcoms", StringComparison.InvariantCultureIgnoreCase) || message.Trim().ToLower().Equals("!modcommands", StringComparison.InvariantCultureIgnoreCase))
        {
            if (sender.Equals(streamer) || mod)
            {
                client.SendMessage("Mod Command Guide: " + Utils.modCommandsLink);
                return;
            }
        }
    }

    private Boolean moderation(string message, string sender)
    {
        message = message.ToLower();
        if (genericProfanityFilterEnabled)
        {
            List<String> list = Utils.getBadWordList();
            foreach (String str in list)
            {
                if (message.Contains(str))
                {
                    client.SendMessage("/timeout " + sender + " 1");
                    client.SendMessage(profanityTimeoutMessage.Replace("$user", sender));
                    return false;
                }
            }
        }
        if (profanityBlackList != null)
        {
            foreach (String str in profanityBlackList)
            {
                if (message.Contains(str))
                {
                    client.SendMessage("/timeout " + sender + " 1");
                    client.SendMessage(profanityTimeoutMessage.Replace("$user", sender));
                    return false;
                }
            }
        }
        bool isUri = false;
        if (message.Contains("http:") || message.Contains("https:") || message.Contains("www.") || message.Contains(".com") ||
            message.Contains(".edu") || message.Contains(".org") || message.Contains(".net") || message.Contains(".int") ||
            message.Contains(".gov") || message.Contains(".mil") || message.Contains(".co.uk"))
        {
            isUri = true;
        }
        if (!allowLinks && isUri)
        {
            if (allowedLinks == null || (allowedLinks != null && allowedLinks.Length == 0))
            {
                client.SendMessage("/timeout " + sender + " 1");
                client.SendMessage(linkTimeoutMessage.Replace("$user", sender));
                return false;
            }
            foreach (String str in allowedLinks)
            {
                if ((allowedLinks.Contains("youtube") || allowedLinks.Contains("Youtube")) && message.Contains("youtu"))
                {
                    return true;
                }
                if (message.Contains(str.ToLower()))
                {
                    return true;
                }
            }
            client.SendMessage("/timeout " + sender + " 1");
            client.SendMessage(linkTimeoutMessage.Replace("$user", sender));
            return false;
        }

        // TODO : spam check

        return true;
    }

    [STAThread]
    public void read()
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
        int index = rand.Next(getAllViewers(streamer).Count);
        return getAllViewers(streamer)[index].ToString();
    }

    public void botInfoCOMMAND(String message, String channel, String sender)
    {
        if (message.Equals("!edudebot", StringComparison.InvariantCultureIgnoreCase) || message.Equals("!bot", StringComparison.InvariantCultureIgnoreCase)
                || message.Equals("!edude15000", StringComparison.InvariantCultureIgnoreCase) || message.Equals("!dudebot", StringComparison.InvariantCultureIgnoreCase))
        {
            client.SendMessage("Dudebot is a free bot programmed by Edude15000. "
                    + "If you would like to use Dudebot, the download link is here: http://dudebot.webs.com/ Make sure to join the DudeBot Discord also: https://discord.gg/NFehx5h" +
                    " Please checkout his band on Spotify here: https://open.spotify.com/album/1mleQbNkIbQ8kOG4yGmgBN?");
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
            line += " !sfx, ";
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
                        break;
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

    public Boolean checkUserLevel(String sender, int level, String channel, OnMessageReceivedArgs e)
    {
        if (level == 0 || sender.Equals(streamer))
        {
            return true;
        }
        if (level == 1 && (e.ChatMessage.IsModerator || e.ChatMessage.IsSubscriber || sender.Equals(streamer)))
        {
            return true;
        }
        if (level == 2 && e.ChatMessage.IsModerator)
        {
            return true;
        }
        if (level == 4 && e.ChatMessage.IsSubscriber)
        {
            return true;
        }
        return false;
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

    public void viewerCOMMAND(String message, String channel, String sender, OnMessageReceivedArgs e)
    {
        if (checkUserLevel(sender, getViewerComm.level, channel, e))
        {
            for (int i = 0; i < getViewerComm.input.Length; i++)
            {
                String temp = message.ToLower();
                if (temp.Equals(getViewerComm.input[i]))
                {
                    client.SendMessage("Current viewer count: "
                            + getAllViewers(streamer).Count);
                }
            }
        }
    }

    public void userCOMMANDS(String message, String channel, String sender, OnMessageReceivedArgs e)
    {
        for (int i = 0; i < userCommandList.Count; i++)
        {
            String temp = message.ToLower() + " ";
            if (temp.StartsWith(userCommandList[i].input[0] + " ")
                    && checkUserLevel(sender, userCommandList[i].level, channel, e))
            {
                BotUser b = getBotUser(sender);
                if (userCommandList[i].costToUse == 0 || b.points >= userCommandList[i].costToUse)
                {
                    if (userCommandList[i].costToUse != 0)
                    {
                        b.points -= userCommandList[i].costToUse;
                    }
                    client.SendMessage(userVariables(userCommandList[i].output, channel, sender,
                        Utils.getFollowingText(message), message, false, e));
                    break;
                }
            }
        }
        for (int i = 0; i < timerCommandList.Count; i++)
        {
            String temp = message.ToLower() + " ";
            if (temp.StartsWith(timerCommandList[i].input[0] + " "))
            {
                client.SendMessage(userVariables(timerCommandList[i].output, channel, sender,
                        Utils.getFollowingText(message), message, false, e));
                break;
            }
        }
    }

    public String userVariables(String response, String channel, String sender, String followingText, String message, Boolean ev, OnMessageReceivedArgs e)
    {
        if (followingText.StartsWith("@"))
        {
            followingText = followingText.Substring(1);
        }
        if (response.Contains("$viewers"))
        {
            response = response.Replace("$viewers", getAllViewers(streamer).Count.ToString());
        }
        if (response.Contains("$user ") || response.EndsWith("$user"))
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
            catch (IOException e1)
            {
                Utils.errorReport(e1);
                Console.WriteLine(e1.ToString());
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
            foreach (Command c in overrideCommandList)
            {
                if (c.overrideType == 0 && message.Contains(c.input[0]))
                {
                    return c.output;
                }
            }
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
                Boolean mod = false;
                if (e != null)
                {
                    mod = e.ChatMessage.IsModerator;
                }
                if (sender.Equals(Utils.botMaker) || sender.Equals(streamer) || sender.Equals(botName)
                        || mod)
                {
                    String info = Utils.callURL("https://api.twitch.tv/kraken/channels/" + followingText);
                    var a = JsonConvert.DeserializeObject<dynamic>(info);
                    try
                    {
                        String game = a["game"];
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
        if (response.Contains("$start"))
        {
            String info = Utils.callURL("https://api.twitch.tv/kraken/channels/" + streamer);
            var a = JsonConvert.DeserializeObject<dynamic>(info);
            try
            {
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
        if (response.Contains("$uptime"))
        {
            String info = Utils.callURL("https://api.twitch.tv/kraken/streams/" + streamer);
            var a = JsonConvert.DeserializeObject<dynamic>(info)["stream"];
            try
            {
                String start = a["created_at"];
                TimeSpan diff = DateTime.Now.ToUniversalTime() - Convert.ToDateTime(start);
                response = response.Replace("$uptime", Utils.timeConversion((int)diff.TotalSeconds));
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
                var a = JsonConvert.DeserializeObject<dynamic>(info);
                try
                {
                    String created = a["created_at"];
                    DateTime d = Convert.ToDateTime(created);
                    long millisFromEpoch = (long)(d - new DateTime(1970, 1, 1)).TotalMilliseconds;
                    long diff = ((DateTimeOffset.Now.ToUnixTimeMilliseconds() - millisFromEpoch) / 1000);
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
        if (response.Contains("$streamplays"))
        {
            response = response.Replace("$streamplays", requestSystem.songsPlayedThisStream.ToString());
        }
        if (response.Contains("$lifetimeplays"))
        {
            response = response.Replace("$lifetimeplays", requestSystem.songsPlayedTotal.ToString());
        }
        BotUser b = getBotUser(sender);
        if (b != null)
        {
            if (response.Contains("$userrank"))
            {
                response = response.Replace("$userrank", getBotUser(sender).rank);
            }
            if (response.Contains("$usertime"))
            {
                response = response.Replace("$usertime", Utils.timeConversion(getBotUser(sender).time));
            }
            if (response.Contains("$usersubcredits"))
            {
                response = response.Replace("$usersubcredits", getBotUser(sender).subCredits.ToString());
            }
            if (response.Contains("$userpoints"))
            {
                response = response.Replace("$userpoints", getBotUser(sender).points.ToString());
            }
            if (response.Contains("$usernumrequests"))
            {
                response = response.Replace("$usernumrequests", getBotUser(sender).numRequests.ToString());
            }
            if (response.Contains("$usermonths"))
            {
                response = response.Replace("$usermonths", getBotUser(sender).months.ToString());
            }
        }
        if (requestSystem.songList.Count != 0)
        {
            Song s = requestSystem.songList[0];
            if (response.Contains("$currentsonglevel") && s != null)
            {
                response = response.Replace("$currentsonglevel", s.level);
            }
            if (response.Contains("$currentsongyoutubelink") && s != null)
            {
                response = response.Replace("$currentsongyoutubelink", s.youtubeLink);
            }
            if (response.Contains("$currentsongyoutubetitle") && s != null)
            {
                response = response.Replace("$currentsongyoutubetitle", s.youtubeTitle);
            }
            if (response.Contains("$currentsongduration") && s != null)
            {
                response = response.Replace("$currentsongduration", s.formattedDuration);
            }
        }
        if (response.Contains("$cleverbot"))
        {
            if (cleverbotSession != null)
            {
                var t = new Thread(() => cleverbotThread(message));
                t.Start();
            }
            return "";
        }
        if (response.Contains("$accuracy") && File.Exists(Utils.accuracyFile))
        {
            String num = File.ReadAllText(Utils.accuracyFile);
            response = response.Replace("$accuracy", num);
            response = response.Replace("%", "");
        }
        return response;
    }

    public void cleverbotThread(String message)
    {
        client.SendMessage(cleverbotSession.Send(Utils.getFollowingText(message)));
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
        foreach (Command c in overrideCommandList)
        {
            if (c.overrideType == 1 && message.Contains(c.input[0]))
            {
                return c.output;
            }
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
        foreach (String userName in getAllViewers(streamer))
        {
            if (user.Equals(userName))
            {
                return true;
            }
        }
        return false;
    }

    private void onUserJoined(object s, OnUserJoinedArgs e)
    {
        String login = e.Username;
        writeToEventLog("JOIN: " + login);
        if (!backupUsersInChat.Contains(login))
        {
            backupUsersInChat.Add(login);
        }
        if (!containsUser(users, login))
        {
            users.Add(new BotUser(login, 0, false, false, false, 0, 0, null, 0, 0, 0));
        }
        if (events.ContainsKey(login))
        {
            client.SendMessage(userVariables(events[login], channel, login, events[login], events[login], true, null));
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