using Google.Apis.YouTube.v3.Data;
using IgnitionHelper.CDLC;
using IgnitionHelper.Ignition;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TwitchLib.Events.Client;

public class RequestSystem : INotifyPropertyChanged
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
        formattedTotalTime = formatTotalTime();
        songListLength = songList.Count;
        return true;
    }
    [JsonIgnore]
    public List<String> favSongsPlayedThisStream { get; set; } = new List<String>();
    [JsonIgnore]
    public Boolean doNotWriteToHistory = false;
    [JsonIgnore]
    public String lastSong { get; set; }
    [JsonIgnore]
    public TwitchBot bot;
    [JsonIgnore]
    public Dictionary<String, int> RandomNextUserSongWaits = new Dictionary<string, int>();
    [JsonIgnore]
    public Dictionary<String, int> randomNextUserSongWaits
    {
        get => RandomNextUserSongWaits;
        set { SetField(ref RandomNextUserSongWaits, value, nameof(randomNextUserSongWaits)); }
    }
    [JsonIgnore]
    public Boolean VipSongToggle = true;
    public Boolean vipSongToggle
    {
        get => VipSongToggle;
        set { SetField(ref VipSongToggle, value, nameof(vipSongToggle)); }
    }
    [JsonIgnore]
    public Boolean MustFollowToRequest = true;
    public Boolean mustFollowToRequest
    {
        get => MustFollowToRequest;
        set { SetField(ref MustFollowToRequest, value, nameof(mustFollowToRequest)); }
    }
    [JsonIgnore]
    public Boolean RequestsTrigger = true;
    public Boolean requestsTrigger
    {
        get => RequestsTrigger;
        set { SetField(ref RequestsTrigger, value, nameof(requestsTrigger)); }
    }
    [JsonIgnore]
    public Boolean DisplayIfUserIsHere = true;
    public Boolean displayIfUserIsHere
    {
        get => DisplayIfUserIsHere;
        set { SetField(ref DisplayIfUserIsHere, value, nameof(displayIfUserIsHere)); }
    }
    [JsonIgnore]
    public Boolean DisplayOneLine = true;
    public Boolean displayOneLine
    {
        get => DisplayOneLine;
        set { SetField(ref DisplayOneLine, value, nameof(displayOneLine)); }
    }
    [JsonIgnore]
    public Boolean WhisperToUser = true;
    public Boolean whisperToUser
    {
        get => WhisperToUser;
        set { SetField(ref WhisperToUser, value, nameof(whisperToUser)); }
    }
    [JsonIgnore]
    public Boolean Direquests = true;
    public Boolean direquests
    {
        get => Direquests;
        set { SetField(ref Direquests, value, nameof(direquests)); }
    }
    [JsonIgnore]
    public Boolean Ylrequests = true;
    public Boolean ylrequests
    {
        get => Ylrequests;
        set { SetField(ref Ylrequests, value, nameof(ylrequests)); }
    }
    [JsonIgnore]
    public Boolean MaxSongLength = false;
    public Boolean maxSongLength
    {
        get => MaxSongLength;
        set { SetField(ref MaxSongLength, value, nameof(maxSongLength)); }
    }
    [JsonIgnore]
    public String SubOnlyRequests = "Only subs can request right now, $user!";
    public String subOnlyRequests
    {
        get => SubOnlyRequests;
        set { SetField(ref SubOnlyRequests, value, nameof(subOnlyRequests)); }
    }
    [JsonIgnore]
    public String FormattedTotalTime;
    public String formattedTotalTime
    {
        get => FormattedTotalTime;
        set { SetField(ref FormattedTotalTime, value, nameof(formattedTotalTime)); }
    }
    [JsonIgnore]
    public Boolean OnlyDisplayLinkList = false;
    public Boolean onlyDisplayLinkList
    {
        get => OnlyDisplayLinkList;
        set { SetField(ref OnlyDisplayLinkList, value, nameof(onlyDisplayLinkList)); }
    }
    [JsonIgnore]
    public int SongListLength = 0;
    public int songListLength
    {
        get => SongListLength;
        set { SetField(ref SongListLength, value, nameof(songListLength)); }
    }
    [JsonIgnore]
    public int MaxSonglistLength = 100;
    public int maxSonglistLength
    {
        get => MaxSonglistLength;
        set { SetField(ref MaxSonglistLength, value, nameof(maxSonglistLength)); }
    }
    [JsonIgnore]
    public int NumOfSongsToDisplay = 8;
    public int numOfSongsToDisplay
    {
        get => NumOfSongsToDisplay;
        set { SetField(ref NumOfSongsToDisplay, value, nameof(numOfSongsToDisplay)); }
    }
    [JsonIgnore]
    public int NumOfSongsInQueuePerUser = 1;
    public int numOfSongsInQueuePerUser
    {
        get => NumOfSongsInQueuePerUser;
        set { SetField(ref NumOfSongsInQueuePerUser, value, nameof(numOfSongsInQueuePerUser)); }
    }
    [JsonIgnore]
    public int MaxSongLengthInMinutes = 10;
    public int maxSongLengthInMinutes
    {
        get => MaxSongLengthInMinutes;
        set { SetField(ref MaxSongLengthInMinutes, value, nameof(maxSongLengthInMinutes)); }
    }
    [JsonIgnore]
    public List<String> FavSongs = new List<String>();
    public List<String> favSongs
    {
        get => FavSongs;
        set { SetField(ref FavSongs, value, nameof(favSongs)); }
    }
    [JsonIgnore]
    public String[] BannedKeywords;
    public String[] bannedKeywords
    {
        get => BannedKeywords;
        set { SetField(ref BannedKeywords, value, nameof(bannedKeywords)); }
    }
    [JsonIgnore]
    public List<String> SongHistory = new List<String>();
    public List<String> songHistory
    {
        get => SongHistory;
        set { SetField(ref SongHistory, value, nameof(songHistory)); }
    }
    [JsonIgnore]
    public Command RequestComm;
    public Command requestComm
    {
        get => RequestComm;
        set { SetField(ref RequestComm, value, nameof(requestComm)); }
    }
    [JsonIgnore]
    public Command SonglistComm;
    public Command songlistComm
    {
        get => SonglistComm;
        set { SetField(ref SonglistComm, value, nameof(songlistComm)); }
    }
    [JsonIgnore]
    public Command GetTotalComm;
    public Command getTotalComm
    {
        get => GetTotalComm;
        set { SetField(ref GetTotalComm, value, nameof(getTotalComm)); }
    }
    [JsonIgnore]
    public Command EditComm;
    public Command editComm
    {
        get => EditComm;
        set { SetField(ref EditComm, value, nameof(editComm)); }
    }
    [JsonIgnore]
    public Command NextComm;
    public Command nextComm
    {
        get => NextComm;
        set { SetField(ref NextComm, value, nameof(nextComm)); }
    }
    [JsonIgnore]
    public Command AddvipComm;
    public Command addvipComm
    {
        get => AddvipComm;
        set { SetField(ref AddvipComm, value, nameof(addvipComm)); }
    }
    [JsonIgnore]
    public Command AddtopComm;
    public Command addtopComm
    {
        get => AddtopComm;
        set { SetField(ref AddtopComm, value, nameof(addtopComm)); }
    }
    [JsonIgnore]
    public Command AdddonatorComm;
    public Command adddonatorComm
    {
        get => AdddonatorComm;
        set { SetField(ref AdddonatorComm, value, nameof(adddonatorComm)); }
    }
    [JsonIgnore]
    public Command GetCurrentComm;
    public Command getCurrentComm
    {
        get => GetCurrentComm;
        set { SetField(ref GetCurrentComm, value, nameof(getCurrentComm)); }
    }
    [JsonIgnore]
    public Command ClearComm;
    public Command clearComm
    {
        get => ClearComm;
        set { SetField(ref ClearComm, value, nameof(clearComm)); }
    }
    [JsonIgnore]
    public Command TriggerRequestsComm;
    public Command triggerRequestsComm
    {
        get => TriggerRequestsComm;
        set { SetField(ref TriggerRequestsComm, value, nameof(triggerRequestsComm)); }
    }
    [JsonIgnore]
    public Command BackupRequestAddComm;
    public Command backupRequestAddComm
    {
        get => BackupRequestAddComm;
        set { SetField(ref BackupRequestAddComm, value, nameof(backupRequestAddComm)); }
    }
    [JsonIgnore]
    public Command GetNextComm;
    public Command getNextComm
    {
        get => GetNextComm;
        set { SetField(ref GetNextComm, value, nameof(getNextComm)); }
    }
    [JsonIgnore]
    public Command RandomComm;
    public Command randomComm
    {
        get => RandomComm;
        set { SetField(ref RandomComm, value, nameof(randomComm)); }
    }
    [JsonIgnore]
    public Command FavSongComm;
    public Command favSongComm
    {
        get => FavSongComm;
        set { SetField(ref FavSongComm, value, nameof(favSongComm)); }
    }
    [JsonIgnore]
    public Command EditSongComm;
    public Command editSongComm
    {
        get => EditSongComm;
        set { SetField(ref EditSongComm, value, nameof(editSongComm)); }
    }
    [JsonIgnore]
    public Command RemoveSongComm;
    public Command removeSongComm
    {
        get => RemoveSongComm;
        set { SetField(ref RemoveSongComm, value, nameof(removeSongComm)); }
    }
    [JsonIgnore]
    public Command SongPositionComm;
    public Command songPositionComm
    {
        get => SongPositionComm;
        set { SetField(ref SongPositionComm, value, nameof(songPositionComm)); }
    }
    [JsonIgnore]
    public ObservableCollection<Song> SongList = new ObservableCollection<Song>();
    public ObservableCollection<Song> songList
    {
        get => SongList;
        set { SetField(ref SongList, value, nameof(songList)); }
    }
    [JsonIgnore]
    public int SongsPlayedThisStream = 0;
    [JsonIgnore]
    public int songsPlayedThisStream
    {
        get => SongsPlayedThisStream;
        set { SetField(ref SongsPlayedThisStream, value, nameof(songsPlayedThisStream)); }
    }
    [JsonIgnore]
    public int SongsPlayedTotal = 0;
    public int songsPlayedTotal
    {
        get => SongsPlayedTotal;
        set { SetField(ref SongsPlayedTotal, value, nameof(songsPlayedTotal)); }
    }
    [JsonIgnore]
    public int MiniumSongsWaitPerUserOnRandom = 5;
    public int miniumSongsWaitPerUserOnRandom
    {
        get => MiniumSongsWaitPerUserOnRandom;
        set { SetField(ref MiniumSongsWaitPerUserOnRandom, value, nameof(miniumSongsWaitPerUserOnRandom)); }
    }
    [JsonIgnore]
    public int RegSongCost = 0;
    public int regSongCost
    {
        get => RegSongCost;
        set { SetField(ref RegSongCost, value, nameof(regSongCost)); }
    }
    [JsonIgnore]
    public Boolean CheckCustomsForge = false;
    public Boolean checkCustomsForge
    {
        get => CheckCustomsForge;
        set { SetField(ref CheckCustomsForge, value, nameof(checkCustomsForge)); }
    }
    [JsonIgnore]
    public Boolean CheckCustomsForgeLead = false;
    public Boolean checkCustomsForgeLead
    {
        get => CheckCustomsForgeLead;
        set { SetField(ref CheckCustomsForgeLead, value, nameof(checkCustomsForgeLead)); }
    }
    [JsonIgnore]
    public Boolean CheckCustomsForgeRhythm = false;
    public Boolean checkCustomsForgeRhythm
    {
        get => CheckCustomsForgeRhythm;
        set { SetField(ref CheckCustomsForgeRhythm, value, nameof(checkCustomsForgeRhythm)); }
    }
    [JsonIgnore]
    public Boolean CheckCustomsForgeBass = false;
    public Boolean checkCustomsForgeBass
    {
        get => CheckCustomsForgeBass;
        set { SetField(ref CheckCustomsForgeBass, value, nameof(checkCustomsForgeBass)); }
    }
    [JsonIgnore]
    public Boolean DisplayCFLink = false;
    public Boolean displayCFLink
    {
        get => DisplayCFLink;
        set { SetField(ref DisplayCFLink, value, nameof(displayCFLink)); }
    }
    [JsonIgnore]
    public int LowestTuning = 0;
    public int lowestTuning
    {
        get => LowestTuning;
        set { SetField(ref LowestTuning, value, nameof(lowestTuning)); }
    }
    [JsonIgnore]
    public Boolean RequireDD = false;
    public Boolean requireDD
    {
        get => RequireDD;
        set { SetField(ref RequireDD, value, nameof(requireDD)); }
    }
    [JsonIgnore]
    public int DefaultMinsSong = 4;
    public int defaultMinsSong
    {
        get => DefaultMinsSong;
        set { SetField(ref DefaultMinsSong, value, nameof(defaultMinsSong)); }
    }
    [JsonIgnore]
    public Boolean InChatEstimateTime = false;
    public Boolean inChatEstimateTime
    {
        get => InChatEstimateTime;
        set { SetField(ref InChatEstimateTime, value, nameof(inChatEstimateTime)); }
    }
    [JsonIgnore]
    public Boolean OpenCFLinkInsteadOfYoutube = false;
    public Boolean openCFLinkInsteadOfYoutube
    {
        get => OpenCFLinkInsteadOfYoutube;
        set { SetField(ref OpenCFLinkInsteadOfYoutube, value, nameof(openCFLinkInsteadOfYoutube)); }
    }
    [JsonIgnore]
    public Boolean CustomSetlistEnabled = false;
    public Boolean customSetlistEnabled
    {
        get => CustomSetlistEnabled;
        set { SetField(ref CustomSetlistEnabled, value, nameof(customSetlistEnabled)); }
    }
    [JsonIgnore]
    public String GoogleSheetSetlistId = "";
    public String googleSheetSetlistId
    {
        get => GoogleSheetSetlistId;
        set { SetField(ref GoogleSheetSetlistId, value, nameof(googleSheetSetlistId)); }
    }
    [JsonIgnore]
    public Boolean CheckBothColumnsForSetlist = false;
    public Boolean checkBothColumnsForSetlist
    {
        get => CheckBothColumnsForSetlist;
        set { SetField(ref CheckBothColumnsForSetlist, value, nameof(checkBothColumnsForSetlist)); }
    }
    [JsonIgnore]
    public Boolean AutoPromoteToVipOnSub = false;
    public Boolean autoPromoteToVipOnSub
    {
        get => AutoPromoteToVipOnSub;
        set { SetField(ref AutoPromoteToVipOnSub, value, nameof(autoPromoteToVipOnSub)); }
    }
    [JsonIgnore]
    public Boolean AutoPromoteToDonatorOnSub = false;
    public Boolean autoPromoteToDonatorOnSub
    {
        get => AutoPromoteToDonatorOnSub;
        set { SetField(ref AutoPromoteToDonatorOnSub, value, nameof(autoPromoteToDonatorOnSub)); }
    }
    [JsonIgnore]
    public Boolean AutoPromoteToVipOnBitDonation = false;
    public Boolean autoPromoteToVipOnBitDonation
    {
        get => AutoPromoteToVipOnBitDonation;
        set { SetField(ref AutoPromoteToVipOnBitDonation, value, nameof(autoPromoteToVipOnBitDonation)); }
    }
    [JsonIgnore]
    public Boolean AutoPromoteToDonatorOnBitDonation = false;
    public Boolean autoPromoteToDonatorOnBitDonation
    {
        get => AutoPromoteToDonatorOnBitDonation;
        set { SetField(ref AutoPromoteToDonatorOnBitDonation, value, nameof(autoPromoteToDonatorOnBitDonation)); }
    }
    [JsonIgnore]
    public int AutoPromoteToDonatorOnBitDonationMinimum = 1;
    public int autoPromoteToDonatorOnBitDonationMinimum
    {
        get => AutoPromoteToDonatorOnBitDonationMinimum;
        set { SetField(ref AutoPromoteToDonatorOnBitDonationMinimum, value, nameof(autoPromoteToDonatorOnBitDonationMinimum)); }
    }
    [JsonIgnore]
    public int AutoPromoteToVipOnBitDonationMinimum = 1;
    public int autoPromoteToVipOnBitDonationMinimum
    {
        get => AutoPromoteToVipOnBitDonationMinimum;
        set { SetField(ref AutoPromoteToVipOnBitDonationMinimum, value, nameof(autoPromoteToVipOnBitDonationMinimum)); }
    }
    public String cfUserName = "";
    public String cfPassword = "";
    [JsonIgnore]
    public String laravel_session = "";
    [JsonIgnore]
    public String community_pass_hash = "";
    [JsonIgnore]
    public String ipsconnect = "";
    [JsonIgnore]
    public IgnitionSearch search;
    [JsonIgnore]
    private static readonly Dictionary<int, string> tunings
        = new Dictionary<int, string>
        {
            { 0, "All" },
            { 1, "E_STANDARD" },
            { 2, "DROP_D" },
            { 3, "Eb_STANDARD" },
            { 4, "Eb_DROP_Db" },
            { 5, "D_STANDARD" },
            { 6, "D_DROP_C" },
            { 7, "Cs_STANDARD" },
            { 8, "Cs_DROP_B" },
            { 9, "C_STANDARD" },
            { 10, "C_DROP_Bb" },
            { 11, "B_STANDARD" },
            { 12, "B_DROP_A" },
            { 13, "Bb_STANDARD" },
            { 14, "Bb_DROP_Ab" },
            { 15, "A_STANDARD" },
            { 16, "Ab_STANDARD" },
            { 17, "G_STANDARD" },
            { 18, "LOW_Gb_STANDARD" },
            { 19, "LOW_F_STANDARD" }
        };
    
    public async Task<bool> getCookieFromCF()
    {
        if (!cfUserName.Equals("") && !cfPassword.Equals(""))
        {
            try
            {
                CookieContainer cookies = new CookieContainer();
                HttpClientHandler handler = new HttpClientHandler();
                handler.CookieContainer = cookies;
                HttpClient client = new HttpClient(handler);
                var values = new Dictionary<string, string>
                {
                    { "ips_username", cfUserName },
                    { "ips_password", cfPassword },
                    { "rememberMe", "1" },
                    { "auth_key", Utils.customsForgeAuthKey }
                };
                var content = new FormUrlEncodedContent(values);
                String uri = "https://customsforge.com/index.php?app=core&module=global&section=login&do=process";
                var response = await client.PostAsync(uri, content);
                IEnumerable<Cookie> responseCookies = cookies.GetCookies(new Uri(uri)).Cast<Cookie>();
                foreach (Cookie cookie in responseCookies)
                {
                    if (cookie.Name.Equals("-community-pass_hash"))
                    {
                        community_pass_hash = cookie.Value;
                    }
                    if (cookie.Name.StartsWith("ipsconnect_"))
                    {
                        ipsconnect = cookie.Name;
                    }
                    handler.CookieContainer.Add(cookie);
                }
                if (community_pass_hash.Equals(""))
                {
                    return false;
                }
                uri = "http://ignition.customsforge.com";
                response = await client.GetAsync(uri);
                responseCookies = cookies.GetCookies(new Uri(uri)).Cast<Cookie>();
                foreach (Cookie cookie in responseCookies)
                {
                    if (cookie.Name.Equals("laravel_session"))
                    {
                        laravel_session = cookie.Value;
                    }
                }
                if (laravel_session.Equals(""))
                {
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
            }
        }
        return false;
    }

    public CDLCEntry getSongFromIgnition(String songname)
    {
        if (!laravel_session.Equals("") && !community_pass_hash.Equals("") && !ipsconnect.Equals(""))
        {
            try
            {
                search = new IgnitionSearch(cfUserName, laravel_session, community_pass_hash, ipsconnect);
                songname = songname.Replace("-", "");
                songname = Regex.Replace(songname, @"\(.*?\)", "");
                songname = Regex.Replace(songname, @"\s{2,}", " ");
                songname = Utils.replaceAcronyms(songname);
                CDLCEntry entry = null;
                if (songname.ToLower().EndsWith("choice") || songname.ToLower().Contains("streamer") || songname.ToLower().Contains(bot.streamer.ToLower()))
                {
                    entry = new CDLCEntry();
                    entry.noInfo = true;
                    return entry;
                }
                if (songname.ToLower().StartsWith("any "))
                {
                    songname = songname.Substring(3).Trim();
                }
                CDLCEntryList results = search.Search(0, 50, songname);
                if (results.Count == 0)
                {
                    entry = new CDLCEntry();
                    entry.failed = true;
                    entry.failMessage = "Your requested song does not exist for Rocksmith yet";
                    return entry;
                }
                else
                {
                    if (results.data.First().artist.Equals(songname, StringComparison.InvariantCultureIgnoreCase))
                    {
                        entry = results.data.First();
                        entry.noInfo = true;
                        return entry;
                    }
                    Dictionary<CDLCEntry, int> entries = new Dictionary<CDLCEntry, int>();
                    foreach (CDLCEntry e in results)
                    {
                        entries.Add(e, Utils.LevenshteinDistance(Utils.formatSongTitle(songname), Utils.formatSongTitle(e.artist + " " + e.title)));
                    }
                    var sortedDict = entries.OrderBy(x => x.Value).Select(x => x.Key);
                    int lowestChange = entries[sortedDict.First()];
                    List<CDLCEntry> goodSongs = new List<CDLCEntry>();
                    foreach (KeyValuePair<CDLCEntry, int> en in entries)
                    {
                        if (en.Value == lowestChange)
                        {
                            goodSongs.Add(en.Key);
                        }
                    }
                    long dls = -1;
                    foreach (CDLCEntry e in goodSongs)
                    {
                        if (!checkEntryForInfo(e)) {
                            break;
                        }
                        if (e.downloadCount > dls)
                        {
                            dls = e.downloadCount;
                            entry = e;
                            entry.noInfo = false;
                        }
                    }
                }
                return entry;
            }
            catch (Exception e)
            {
                Utils.errorReport(e);
                Console.WriteLine(e.ToString());
                CDLCEntry entry = new CDLCEntry();
                entry.failed = true;
                entry.failMessage = "Could not connect to CustomsForge! :( ";
                return entry;
            }
        }
        Utils.errorReport(new Exception("Either CustomsForge Username or Password is incorrect!"));
        return null;
    }

    public Boolean checkEntryForInfo(CDLCEntry entry)
    {
        if (!entry.parts.HasFlag(Part.ALL)) {
            if (CheckCustomsForgeLead && !entry.parts.HasFlag(Part.LEAD))
            {
                entry = new CDLCEntry();
                entry.failed = true;
                entry.failMessage = "There does not exist a LEAD GUITAR path for this song";
                return false;
            }
            if (CheckCustomsForgeRhythm && !entry.parts.HasFlag(Part.RHYTHM))
            {
                entry = new CDLCEntry();
                entry.failed = true;
                entry.failMessage = "There does not exist a RHYTHM GUITAR path for this song";
                return false;
            }
            if (CheckCustomsForgeBass && !entry.parts.HasFlag(Part.BASS))
            {
                entry = new CDLCEntry();
                entry.failed = true;
                entry.failMessage = "There does not exist a BASS GUITAR path for this song";
                return false;
            }
        }
        if (lowestTuning != 0)
        {
            String tuning = entry.tuning.ToString();
            int currentSongTuningValue = 15;
            foreach (KeyValuePair<int, String> t in tunings)
            {
                if (t.Value.Equals(tuning))
                {
                    currentSongTuningValue = t.Key;
                    break;
                }
            }
            if (lowestTuning < currentSongTuningValue)
            {
                entry = new CDLCEntry();
                entry.failed = true;
                entry.failMessage = "Your requested song has a tuning that is out of range";
                return false;
            }
        }
        if (requireDD && !entry.dd)
        {
            entry = new CDLCEntry();
            entry.failed = true;
            entry.failMessage = "Your requested song does not have dynamic difficulty";
            return false;
        }
        if (bannedKeywords != null && bannedKeywords.Length > 0) {
            foreach (String s in bannedKeywords)
            {
                if (entry.artist.Contains(s) || entry.title.Contains(s))
                {
                    entry = new CDLCEntry();
                    entry.failed = true;
                    entry.failMessage = "Your requested song contains '" + s + "' which is a banned keyword and may not be requested";
                    return false;
                }
            }
        }
        return true;
    }

    public Boolean addSongToList(String song, String requestedby, String level, int place, String noEmoteMessage, Boolean checkCF, Boolean costsPoints)
    {
        CDLCEntry entry = null;
        Song s = new Song(song, requestedby, level, bot);
        if (checkCF && checkCustomsForge && cfUserName != "" && cfPassword != "")
        {
            try
            {
                entry = getSongFromIgnition(song); // TODO : FIX noEmoteMessage
                if (entry == null)
                {
                    bot.client.SendMessage("Your request either does not yet exist for Rocksmith for the streamer's preferred instrument or the tuning of the request is out of the streamer's preferred range. Please refine your search or request a different song, " + requestedby + "!");
                    return false;
                }
                String newSong = song;
                if (entry.failMessage.Equals("Your requested song does not exist for Rocksmith yet"))
                {
                    newSong = Utils.autoCorrect(song);
                }
                if (!song.Equals(newSong))
                {
                    entry = null;
                    song = newSong;
                    entry = getSongFromIgnition(song); // TODO : FIX noEmoteMessage
                }
                if (entry == null || (entry != null && entry.failMessage.Equals("Your requested song does not exist for Rocksmith yet")))
                {
                    bot.client.SendMessage("Your request either does not yet exist for Rocksmith for the streamer's preferred instrument or the tuning of the request is out of the streamer's preferred range. Please refine your search or request a different song, " + requestedby + "!");
                    return false;
                }
                foreach (Song checkedSong in songList)
                {
                    if (checkedSong.cfSongArtist.Equals(entry.artist, StringComparison.InvariantCultureIgnoreCase) && checkedSong.cfSongName.Equals(entry.title, StringComparison.InvariantCultureIgnoreCase))
                    {
                        bot.client.SendMessage("Your requested song is already in the request queue, " + requestedby + "!");
                        return false;
                    }
                }
                if (entry.failed)
                {
                    if (entry.failMessage == "")
                    {
                        bot.client.SendMessage("Your request either does not yet exist for Rocksmith for the streamer's preferred instrument or the tuning of the request is out of the streamer's preferred range. Please refine your search or request a different song, " + requestedby + "!");
                    }
                    else
                    {
                        bot.client.SendMessage(entry.failMessage + ", " + requestedby + "!");
                    }
                    return false;
                }
                else
                {
                    s.setEntry(entry, search);
                    if (s.officialSong)
                    {
                        bot.client.SendMessage("NOTE: '" + song + "' is an official song and has been added to the queue. " + requestedby + " please check with " + bot.streamer + " to make sure they have the song!");
                    }
                }
            }
            catch
            {
            }
        }
        else if (customSetlistEnabled && googleSheetSetlistId != "")
        {
            List<String> setListSongs = new List<String>();
            if (checkBothColumnsForSetlist)
            {
                setListSongs = bot.googleCustomSetlist.readColumnAandB(googleSheetSetlistId);
            }
            else
            {
                setListSongs = bot.googleCustomSetlist.readColumnA(googleSheetSetlistId);
            }
            if (setListSongs.Count > 0)
            {
                song = song.Replace("-", "");
                song = Regex.Replace(song, @"\(.*?\)", "");
                song = Regex.Replace(song, @"\s{2,}", " ");
                song = Utils.replaceAcronyms(song);
                // TODO : FIX noEmoteMessage
                Boolean foundSong = false;
                foreach (String str in setListSongs)
                {
                    String song2 = str.Replace("-", "");
                    song2 = Regex.Replace(song2, @"\(.*?\)", "");
                    song2 = Regex.Replace(song2, @"\s{2,}", " ");
                    song2 = Utils.replaceAcronyms(song2);
                    if (Utils.LevenshteinDistance(Utils.formatSongTitle(song2), Utils.formatSongTitle(song)) < 4)
                    {
                        foundSong = true;
                        s.cfSongName = str;
                        s.name = str;
                        break;
                    }
                }
                if (!foundSong)
                {
                    String newSong = Utils.autoCorrect(song);
                    if (!song.Equals(newSong))
                    {
                        song = newSong;
                        foreach (String str in setListSongs)
                        {
                            String song2 = str.Replace("-", "");
                            song2 = Regex.Replace(song2, @"\(.*?\)", "");
                            song2 = Regex.Replace(song2, @"\s{2,}", " ");
                            song2 = Utils.replaceAcronyms(song2);
                            if (Utils.LevenshteinDistance(Utils.formatSongTitle(song2), Utils.formatSongTitle(song)) < 4)
                            {
                                foundSong = true;
                                s.cfSongName = str;
                                s.name = str;
                                break;
                            }
                        }
                    }
                }
                if (!foundSong)
                {
                    bot.client.SendMessage("You requested a song that does not exist in the streamer's setlist. Please check https://docs.google.com/spreadsheets/d/"
                        + googleSheetSetlistId + " and try requesting again, " + requestedby + "!");
                    return false;
                }
            }
        }
        if (costsPoints)
        {
            BotUser sender = bot.getBotUser(requestedby);
            if (sender != null && sender.points >= regSongCost)
            {
                sender.points -= regSongCost;
            }
            else
            {
                bot.client.SendMessage("You need at least " + regSongCost + " " + bot.currency.currencyName + " to request a song, " + sender.username + "!");
                return false;
            }
        }
        if (place < 0)
        {
            songList.Add(s);
        }
        else
        {
            songList.Insert(place, s);
        }
        return true;
    }
    
    public int getNumRequests(String sender)
    {
        foreach (BotUser user in bot.users)
        {
            if (user.username.Equals(sender, StringComparison.InvariantCultureIgnoreCase))
            {
                return user.numRequests;
            }
        }
        return 0;
    }

    public void giveSpot(String message, String channel, String sender)
    {
        foreach (BotUser u in bot.users)
        {
            if (u.username.Equals(sender, StringComparison.InvariantCultureIgnoreCase) && !sender.Equals(bot.streamer, StringComparison.InvariantCultureIgnoreCase))
            {
                if (u.gaveSpot)
                {
                    bot.client.SendMessage("You can only give another user your spot once per stream!");
                    return;
                }
                if (Int32.Parse(getNumberOfSongs()) == 0)
                {
                    bot.client.SendMessage("You have no requests in the list, " + sender + "!");
                    return;
                }
                for (int i = 0; i < songList.Count; i++)
                {
                    if (songList[i].requester.Equals(sender, StringComparison.InvariantCultureIgnoreCase))
                    {
                        String newUser = Utils.getFollowingText(message);
                        if (newUser.Contains("@"))
                        {
                            newUser = newUser.Replace("@", "");
                        }
                        String previousSong = songList[i].name;
                        songList[i].requester = newUser;
                        songList[i].name = "Place Holder";
                        bot.client.SendMessage("Your next request '" + previousSong
                           + "' has been changed to 'Place Holder' FOR " + newUser.ToLower() + "!");
                        u.gaveSpot = true;
                        return;
                    }
                }
                bot.client.SendMessage("You have no regular requests in the list, "
                        + sender + "!");
                return;
            }
        }
    }

    public void checkSongPositionCOMMAND(String message, String channel, String sender, OnMessageReceivedArgs e)
    {
        if (bot.checkUserLevel(sender, songPositionComm.level, channel, e))
        {
            for (int i = 0; i < songPositionComm.input.Length; i++)
            {
                String temp = message.ToLower();
                if (temp.Equals(songPositionComm.input[i], StringComparison.InvariantCultureIgnoreCase))
                {
                    try
                    {
                        String response = "";
                        List<Int32> spots = checkPosition(message, channel, sender);
                        if (spots.Count < 1)
                        {
                            bot.client.SendMessage("You do not have any requests in the song list, " + sender + "!");
                            return;
                        }
                        for (int j = 0; j < spots.Count; j++)
                        {
                            if (spots[j] == 0)
                            {
                                response += "You have a song playing right now, ";
                            }
                            else if (spots[j] == 1)
                            {
                                response += "You have a request next in line, ";
                            }
                            else
                            {
                                response += "You have a request in place #" + (spots[j] + 1) + " (Estimated wait time: " + getEstimatedTimeUntilSong(spots[j] + 1) + "), ";
                            }
                        }
                        bot.client.SendMessage("Y" + response.ToLower().Substring(1) + sender + "!");
                    }
                    catch (Exception e1)
                    {
                        Utils.errorReport(e1);
                        Debug.WriteLine(e1.ToString());
                    }
                }
            }
        }
    }
    
    public String getEstimatedTimeUntilSong(int position)
    {
        int totalSeconds = 0;
        for (int i = 0; i < position - 1; i++)
        {
            if (!inChatEstimateTime || (inChatEstimateTime && !songList[i].requesterIsHere.Equals(""))) {
                if (songList[i].durationInSeconds == 0)
                {
                    totalSeconds += (60 * defaultMinsSong);
                }
                else
                {
                    totalSeconds += songList[i].durationInSeconds;
                }
            }
        }
        return Utils.timeConversion(totalSeconds);
    }

    public List<Int32> checkPosition(String message, String channel, String sender)
    {
        List<Int32> songs = new List<Int32>();
        for (int i = 0; i < songList.Count; i++)
        {
            if (songList[i].requester.Equals(sender, StringComparison.InvariantCultureIgnoreCase))
            {
                songs.Add(i);
            }
        }
        return songs;
    }

    public void addDonatorCOMMAND(String message, String channel, String sender, String noEmoteMessage, OnMessageReceivedArgs e)
    {
        if (bot.checkUserLevel(sender, adddonatorComm.level, channel, e))
        {
            for (int i = 0; i < adddonatorComm.input.Length; i++)
            {
                String temp = message.ToLower();
                if (temp.StartsWith(adddonatorComm.input[i]) && temp.Contains(adddonatorComm.input[i] + " "))
                {
                    try
                    {
                        String input = Utils.getFollowingText(message);
                        String youtubeID = null;
                        Video ytvid = null;
                        if (message.Contains("www.") || message.Contains("http://") || message.Contains("http://www.")
                                || message.Contains(".com") || message.Contains("https://"))
                        {
                            if (message.Contains("www.youtube.com/watch?v=")
                                    || message.Contains("www.youtube.com/watch?v="))
                            {
                                youtubeID = message.Substring(message.IndexOf("=") + 1);
                                try
                                {
                                    ytvid = bot.youtube.searchYoutubeByID(youtubeID);
                                    if (ytvid == null)
                                    {
                                        bot.client.SendMessage("Invalid youtube URL, " + sender);
                                        return;
                                    }
                                }
                                catch (Exception)
                                {
                                    bot.client.SendMessage("Invalid youtube URL, " + sender);
                                    return;
                                }
                            }
                            else if (message.Contains("https://youtu.be/"))
                            {
                                youtubeID = message.Substring(message.LastIndexOf("/") + 1);
                                try
                                {
                                    ytvid = bot.youtube.searchYoutubeByID(youtubeID);
                                    if (ytvid == null)
                                    {
                                        bot.client.SendMessage("Invalid youtube URL, " + sender);
                                        return;
                                    }
                                }
                                catch (Exception)
                                {
                                    bot.client.SendMessage("Invalid youtube URL, " + sender);
                                    return;
                                }
                            }
                            else
                            {
                                bot.client.SendMessage("Invalid Request, " + sender);
                                return;
                            }
                        }
                        if (input.EndsWith(")"))
                        {
                            String[] str = input.Split(' ');
                            String requester = str[str.Length - 1];
                            input = input.Replace(requester, "");
                            requester = requester.Replace("(", "").Replace(")", "");
                            if (ytvid != null)
                            {
                                addDonator(channel, ytvid.Snippet.Title, requester, noEmoteMessage);
                                bot.client.SendMessage("Donator Song '" + ytvid.Snippet.Title
                                                + "' has been added to the song list, " + requester + "!");
                            }
                            else
                            {
                                addDonator(channel, input, requester, noEmoteMessage);
                                bot.client.SendMessage("Donator Song '" + input
                                        + "' has been added to the song list, " + requester + "!");
                            }
                            bot.addUserRequestAmount(requester, true);
                            writeToCurrentSong(channel, false);
                        }
                        else
                        {
                            if (ytvid != null)
                            {
                                addDonator(channel, ytvid.Snippet.Title, sender, noEmoteMessage);
                                bot.client.SendMessage("Donator Song '" + ytvid.Snippet.Title
                                                + "' has been added to the song list, " + sender + "!");
                            }
                            else
                            {
                                addDonator(channel, input, sender, noEmoteMessage);
                                bot.client.SendMessage("Donator Song '" + input
                                        + "' has been added to the song list, " + sender + "!");
                            }
                            writeToCurrentSong(channel, false);
                        }
                    }
                    catch (IOException e1)
                    {
                        Utils.errorReport(e1);
                        Debug.WriteLine(e1.ToString());
                    }
                }
            }
        }
    }

    public void addDonator(String channel, String song, String requestedby, String noEmoteMessage)
    {
        if (Int32.Parse(getNumberOfSongs()) == 0)
        {
            addSongToList(song, requestedby, "$$$", -1, noEmoteMessage, true, false);
        }
        else if (Int32.Parse(getNumberOfSongs()) == 1)
        {
            if (addSongToList(song, requestedby, "$$$", -1, noEmoteMessage, true, false))
            {
                songList[0].level = "$$$";
            }
        }
        else
        {
            if (songList[0].level.Equals(""))
            {
                songList[0].level = "$$$";
            }
            for (int i = 1; i < songList.Count; i++)
            {
                if (!songList[i].level.Equals("$$$"))
                {
                    if (addSongToList(song, requestedby, "$$$", i, noEmoteMessage, true, false))
                    {
                        songList[0].level = "$$$";
                    }
                    writeToCurrentSong(bot.channel, true);
                    return;
                }
            }
            addSongToList(song, requestedby, "$$$", -1, noEmoteMessage, true, false);
        }
        writeToCurrentSong(bot.channel, true);
    }

    public void removeMySong(String message, String channel, String sender, OnMessageReceivedArgs e)
    {
        if (bot.checkUserLevel(sender, removeSongComm.level, channel, e))
        {
            for (int i = 0; i < removeSongComm.input.Length; i++)
            {
                String temp = message.ToLower();
                if (temp.Equals(removeSongComm.input[i], StringComparison.InvariantCultureIgnoreCase))
                {
                    removeRequesterSong(message, channel, sender);
                }
            }
        }
    }

    public void removeRequesterSong(String message, String channel, String sender)
    {
        if (Int32.Parse(getNumberOfSongs()) == 0)
        {
            bot.client.SendMessage("You have no regular requests in the list, " + sender + "!");
            return;
        }
        for (int i = 0; i < songList.Count; i++)
        {
            if (songList[i].requester.Equals(sender, StringComparison.InvariantCultureIgnoreCase))
            {
                String songToDelete = songList[i].name;
                songList.RemoveAt(i);
                bot.client.SendMessage("Your next request '"
                    + songToDelete + "' has been removed, " + sender
                    + "!");
                bot.addUserRequestAmount(sender, false);
                writeToCurrentSong(bot.channel, true);
                return;
            }
        }
        bot.client.SendMessage("You have no requests in the list, " + sender + "!");
    }

    public void editMySongCOMMAND(String message, String channel, String sender, String noEmoteMessage, OnMessageReceivedArgs e)
    {
        if (bot.checkUserLevel(sender, editSongComm.level, channel, e))
        {
            for (int i = 0; i < editSongComm.input.Length; i++)
            {
                String temp = message.ToLower();
                if (temp.Equals(editSongComm.input[i], StringComparison.InvariantCultureIgnoreCase))
                {
                    bot.client.SendMessage("Please type an artist and song name after the command, " + sender);
                }
                else if (temp.StartsWith(editSongComm.input[i]) && temp.Contains(editSongComm.input[i] + " "))
                {
                    if (bannedKeywords != null)
                    {
                        for (int j = 0; j < bannedKeywords.Length; j++)
                        {
                            if (temp.ToLower().Contains(bannedKeywords[j].ToLower()))
                            {
                                bot.client.SendMessage("Song request contains a banned keyword '"
                                        + bannedKeywords[j] + "' and cannot be added, " + sender + "!");
                                return;
                            }
                        }
                    }
                    editRequesterSong(message, channel, sender, noEmoteMessage, e);
                }
            }
        }
    }

    public void editRequesterSong(String message, String channel, String sender, String noEmoteMessage, OnMessageReceivedArgs e)
    {
        if (Int32.Parse(getNumberOfSongs()) == 0)
        {
            bot.client.SendMessage("You have no requests in the list, " + sender + "!");
            return;
        }
        for (int i = 0; i < songList.Count; i++)
        {
            if (songList[i].requester.Equals(sender, StringComparison.InvariantCultureIgnoreCase))
            {
                if (i == 0)
                {
                    if (!e.ChatMessage.IsModerator
                        && !sender.Equals(bot.streamer, StringComparison.InvariantCultureIgnoreCase) && !sender.Equals(Utils.botMaker, StringComparison.InvariantCultureIgnoreCase))
                    {
                        bot.client.SendMessage("Your song is currently playing. Please have a mod edit it, " + sender + "!");
                        return;
                    }
                }
                String previousSong = songList[i].name;
                String input = Utils.getFollowingText(message);
                String youtubeID = null;
                Video ytvid = null;
                try
                {
                    if (message.Contains("www.") || message.Contains("http://") || message.Contains("http://www.")
                            || message.Contains(".com") || message.Contains("https://"))
                    {
                        if (message.Contains("www.youtube.com/watch?v=")
                                || message.Contains("www.youtube.com/watch?v="))
                        {
                            youtubeID = message.Substring(message.IndexOf("=") + 1);
                            try
                            {
                                ytvid = bot.youtube.searchYoutubeByID(youtubeID);
                                if (ytvid == null)
                                {
                                    bot.client.SendMessage("Invalid youtube URL, " + sender);
                                    return;
                                }
                            }
                            catch (Exception)
                            {
                                bot.client.SendMessage("Invalid youtube URL, " + sender);
                                return;
                            }
                        }
                        else if (message.Contains("https://youtu.be/"))
                        {
                            youtubeID = message.Substring(message.LastIndexOf("/") + 1);
                            try
                            {
                                ytvid = bot.youtube.searchYoutubeByID(youtubeID);
                                if (ytvid == null)
                                {
                                    bot.client.SendMessage("Invalid youtube URL, " + sender);
                                    return;
                                }
                            }
                            catch (Exception)
                            {
                                bot.client.SendMessage("Invalid youtube URL, " + sender);
                                return;
                            }
                        }
                        else
                        {
                            bot.client.SendMessage("Invalid Request, " + sender);
                            return;
                        }
                    }
                }
                catch (Exception)
                {
                }
                if (ytvid != null)
                {
                    input = ytvid.Snippet.Title;
                }
                if (addSongToList(input, sender, songList[i].level, songList[i].index, noEmoteMessage, true, false))
                {
                    songList.Remove(songList[i]);
                    bot.client.SendMessage("Your next request '" + previousSong
                           + "' has been changed to '" + songList[i].name + "', " + sender + "!");
                    writeToCurrentSong(channel, false);
                }
                return;
            }
        }
        bot.client.SendMessage("You have no requests in the list, " + sender + "!");
    }

    public void chooseRandomFavorite(String message, String channel, String sender, String noEmoteMessage, OnMessageReceivedArgs e)
    {
        if (bot.checkUserLevel(sender, favSongComm.level, channel, e))
        {
            for (int i = 0; i < favSongComm.input.Length; i++)
            {
                if (message.Equals(favSongComm.input[i], StringComparison.InvariantCultureIgnoreCase))
                {
                    if (Int32.Parse(getNumberOfSongs()) < maxSonglistLength)
                    {
                        if (requestsTrigger)
                        {
                            try
                            {
                                Boolean check = true;
                                if (!checkIfUserAlreadyHasSong(sender))
                                {
                                    if (mustFollowToRequest && !e.ChatMessage.IsModerator && !sender.Equals(Utils.botMaker) && !sender.Equals(bot.streamer))
                                    {
                                        bot.checkAtBeginningAsync(false);
                                        BotUser u = bot.getBotUser(sender);
                                        if (u != null && !u.follower)
                                        {
                                            check = false;
                                        }
                                    }
                                    if (check)
                                    {
                                        if (favSongs != null)
                                        {
                                            try
                                            {
                                                Random rand = new Random();
                                                int index = rand.Next(favSongs.Count);
                                                if (favSongsPlayedThisStream.Contains(favSongs[index]))
                                                {
                                                    addSong(channel, "streamer's Choice", sender, noEmoteMessage);
                                                }
                                                else
                                                {
                                                    addSong(channel, favSongs[index] + " (FAV)", sender, noEmoteMessage);
                                                }
                                                favSongsPlayedThisStream.Add(favSongs[index]);
                                            }
                                            catch (IOException e1)
                                            {
                                                Utils.errorReport(e1);
                                                Debug.WriteLine(e1.ToString());
                                            }
                                        }
                                        else
                                        {
                                            addSong(channel, "streamer's Choice", sender, noEmoteMessage);
                                        }
                                    }
                                    else
                                    {
                                        bot.client.SendMessage("You must follow the stream to request a song, " + sender);
                                    }
                                }
                                else
                                {
                                    if (numOfSongsInQueuePerUser == 1)
                                    {
                                        bot.client.SendMessage("You may only have 1 song in the queue at a time, " + sender + "!");
                                    }
                                    else
                                    {
                                        bot.client.SendMessage("You may only have "
                                                + numOfSongsInQueuePerUser + " songs in the queue at a time, " + sender
                                                + "!");
                                    }
                                }
                            }
                            catch (IOException e1)
                            {
                                Utils.errorReport(e1);
                                Debug.WriteLine(e1.ToString());
                            }
                        }
                        else
                        {
                            bot.client.SendMessage("Requests are currently off!");
                        }
                    }
                    else
                    {
                        bot.client.SendMessage("Song limit of " + maxSonglistLength
                                + " has been reached, please try again later.");
                    }
                }
            }
        }
    }

    public void getNextSongCOMMAND(String message, String channel, String sender, OnMessageReceivedArgs e)
    {
        if (bot.checkUserLevel(sender, getNextComm.level, channel, e))
        {
            for (int i = 0; i < getNextComm.input.Length; i++)
            {
                if (message.Equals(getNextComm.input[i], StringComparison.InvariantCultureIgnoreCase))
                {
                    try
                    {
                        bot.client.SendMessage(getNextSongTitle(channel));
                    }
                    catch (Exception e1)
                    {
                        Utils.errorReport(e1);
                        Debug.WriteLine(e1.ToString());
                    }
                }
            }
        }
    }

    public String getNextSongTitle(String channel)
    {
        if (Int32.Parse(getNumberOfSongs()) < 2)
        {
            return "There is no next song in the song list!";
        }
        else
        {
            return "Next up: " + songList[1].name;
        }
    }

    public void triggerRequests(Boolean trigger, String channel)
    {
        if (trigger)
        {
            requestsTrigger = true;
            bot.client.SendMessage("Requests turned on!");
        }
        else
        {
            requestsTrigger = false;
            bot.client.SendMessage("Requests turned off!");
        }
    }

    public void randomizer(String channel)
    {
        if (Int32.Parse(getNumberOfSongs()) > 1)
        {
            Song s = null;
            for (int i = 0; i < 10; i++) {
                Random rand = new Random();
                int randInt = rand.Next(Int32.Parse(getNumberOfSongs()) - 1) + 1;
                s = songList[randInt];
                if (!randomNextUserSongWaits.ContainsKey(s.requester) || (randomNextUserSongWaits.ContainsKey(s.requester) && randomNextUserSongWaits[s.requester] == 0))
                {
                    break;
                }
            }
            songList.Remove(s);
            if (songList[1].level.Equals("VIP"))
            {
                s.level = "VIP";
            }
            if (songList[1].level.Equals("$$$"))
            {
                s.level = "$$$";
            }
            songList[0] = s;
            bot.client.SendMessage(getNextSong(channel));
            writeToCurrentSong(bot.channel, true);
            if (randomNextUserSongWaits.ContainsKey(s.requester))
            {
                randomNextUserSongWaits.Remove(s.requester);
            }
            randomNextUserSongWaits.Add(s.requester, miniumSongsWaitPerUserOnRandom);
        }
        else
        {
            bot.client.SendMessage("Song list must have 2 or more songs to choose a random one!");
        }
    }

    public void decrementRandomNextEligiblity()
    {
        List<string> keys = new List<string>(randomNextUserSongWaits.Keys);
        foreach (string key in keys)
        { 
            if (randomNextUserSongWaits[key] > 0)
            {
                randomNextUserSongWaits[key] -= 1;
            }
        }
    }

    public void randomizerCommand(String message, String channel, String sender, OnMessageReceivedArgs e)
    {
        if (bot.checkUserLevel(sender, randomComm.level, channel, e))
        {
            for (int i = 0; i < randomComm.input.Length; i++)
            {
                if (message.Equals(randomComm.input[i], StringComparison.InvariantCultureIgnoreCase))
                {
                    try
                    {
                        randomizer(channel);
                        writeToCurrentSong(channel, false);
                    }
                    catch (Exception e1)
                    {
                        Utils.errorReport(e1);
                        Debug.WriteLine(e1.ToString());
                    }
                }
            }
        }
    }

    public void setIndexesForSongs()
    {
        for (int i = 0; i < songList.Count; i++)
        {
            songList[i].index = i + 1;
        }
    }

    public void writeToCurrentSong(String channel, Boolean nextCom)
    {
        StreamWriter output;
        output = new StreamWriter(Utils.currentSongFile, false);
        if (getCurrentSongTitle(channel).Equals("Song list is empty"))
        {
            output.Write("");
        }
        else
        {
            output.Write(getCurrentSongTitle(channel));
        }
        output.Close();
        output = new StreamWriter(Utils.currentRequesterFile, false);
        if (getCurrentSongRequester(channel).Equals("Song list is empty"))
        {
            output.Write("");
        }
        else
        {
            output.Write(getCurrentSongRequester(channel));
        }
        output.Close();
        if (bot.google != null && bot.spreadsheetId != null)
        {
            bot.google.writeToGoogleSheets(nextCom, songList, songHistory);
        }
        formattedTotalTime = formatTotalTime();
        String songs = "";
        foreach(Song s in songList)
        {
            songs += s.name + " (" + s.requester + ")\r";
        }
        output = new StreamWriter(Utils.songListTextFile, false);
        output.Write(songs);
        output.Close();
        setSongsIfUserIsHere();
        setIndexesForSongs();
        Utils.saveSongs(songList);
    }

    public String formatTotalTime()
    {
        int totalSeconds = 0;
        for (int i = 0; i < songList.Count; i++)
        {
            if (!inChatEstimateTime || (inChatEstimateTime && !songList[i].requesterIsHere.Equals(""))) {
                if (songList[i].durationInSeconds == 0)
                {
                    totalSeconds += (60 * defaultMinsSong);
                }
                else
                {
                    totalSeconds += songList[i].durationInSeconds;
                }
            }
        }
        return TimeSpan.FromSeconds(totalSeconds).ToString(@"hh\:mm\:ss");
    }

    public void clear(String channel, String file)
    {
        StreamWriter output;
        output = new StreamWriter(file);
        output.Write("");
        output.Close();
        writeToCurrentSong(channel, false);
    }

    public void appendToLastSongs(String channel, String lastSong)
    {
        songHistory.Insert(0, Utils.getDate() + " " + Utils.getTime() + " - " + lastSong);
    }

    public void getCurrentSongCOMMAND(String message, String channel, String sender, OnMessageReceivedArgs e)
    {
        if (bot.checkUserLevel(sender, getCurrentComm.level, channel, e))
        {
            for (int i = 0; i < getCurrentComm.input.Length; i++)
            {
                if (message.Equals(getCurrentComm.input[i], StringComparison.InvariantCultureIgnoreCase))
                {
                    try
                    {
                        bot.client.SendMessage("Playing: " + getCurrentSongTitle(channel));
                    }
                    catch (Exception e1)
                    {
                        Utils.errorReport(e1);
                        Debug.WriteLine(e1.ToString());
                    }
                }
            }
        }
    }

    public void triggerRequestsCOMMAND(String message, String channel, String sender, OnMessageReceivedArgs e)
    {
        if (bot.checkUserLevel(sender, triggerRequestsComm.level, channel, e))
        {
            for (int i = 0; i < triggerRequestsComm.input.Length; i++)
            {
                String temp = message.ToLower();
                if (temp.StartsWith(triggerRequestsComm.input[i])
                        && temp.Contains(triggerRequestsComm.input[i] + " "))
                {
                    if (Utils.getFollowingText(message).Contains("on"))
                    {
                        triggerRequests(true, channel);
                    }
                    else if (Utils.getFollowingText(message).Contains("off"))
                    {
                        triggerRequests(false, channel);
                    }
                }
            }
        }
    }

    public void clearCOMMAND(String message, String channel, String sender, OnMessageReceivedArgs e)
    {
        if (bot.checkUserLevel(sender, clearComm.level, channel, e))
        {
            for (int i = 0; i < clearComm.input.Length; i++)
            {
                String temp = message.ToLower();
                if (temp.Equals(clearComm.input[i]))
                {
                    try
                    {
                        clear(channel, Utils.songListFile);
                        clear(channel, Utils.songListTextFile);
                        if (regSongCost > 0) {
                            for (int j = 1; j < songList.Count; j++)
                            {
                                if (songList[i].level.Equals("")) {
                                    BotUser u = bot.getBotUser(songList[i].requester);
                                    u.points += regSongCost;
                                }
                            }
                        }
                        songList.Clear();
                        bot.client.SendMessage("Song list has been cleared!");
                        writeToCurrentSong(channel, false);
                    }
                    catch (IOException e1)
                    {
                        Utils.errorReport(e1);
                        Debug.WriteLine(e1.ToString());
                    }
                }
            }
        }
    }

    public void decrementUsersOnClear()
    {
        for (int i = 0; i < songList.Count; i++)
        {
            if (i != 0)
            {
                bot.addUserRequestAmount(songList[i].requester, false);
            }
        }
    }

    public void nextCOMMAND(String message, String channel, String sender, OnMessageReceivedArgs e)
    {
        if (bot.checkUserLevel(sender, nextComm.level, channel, e))
        {
            for (int i = 0; i < nextComm.input.Length; i++)
            {
                String temp = message.ToLower();
                if (temp.Equals(nextComm.input[i]))
                {
                    try
                    {
                        if (nextSong(channel))
                        {
                            bot.client.SendMessage(getNextSong(channel));
                        }
                        else
                        {
                            bot.client.SendMessage("There are no songs currently in the queue!");
                        }
                        writeToCurrentSong(channel, true);
                    }
                    catch (Exception e1)
                    {
                        Utils.errorReport(e1);
                        Debug.WriteLine(e1.ToString());
                    }
                }
            }
        }
    }

    public void nextRegular(String message, String channel, String sender)
    {
        nextRegularCOMMAND(message, channel, sender);
        writeToCurrentSong(channel, true);
    }

    public void triggerVIPs(Boolean trigger, String channel)
    {
        if (trigger)
        {
            vipSongToggle = true;
            bot.client.SendMessage("VIP songs turned on!");
        }
        else
        {
            vipSongToggle = false;
            bot.client.SendMessage("VIP songs turned off!");
        }
    }

    public void nextRegularCOMMAND(String message, String channel, String sender)
    {
        if ((Int32.Parse(getNumberOfSongs()) == 0))
        {
            bot.client.SendMessage("There are no songs in the list!");
            return;
        }
        else
        {
            for (int i = 0; i < songList.Count; i++)
            {
                if (songList[i].level.Equals(""))
                {
                    Song s = songList[i];
                    songList.RemoveAt(i);
                    s.level = "$$$";
                    songList.Insert(0, s);
                    bot.client.SendMessage(getNextSong(channel));
                    return;
                }
            }
            nextSongAuto(channel, true);
        }
    }

    public void nextSongAuto(String channel, Boolean check)
    {
        try
        {
            if (nextSong(channel))
            {
                if (check)
                {
                    bot.client.SendMessage("There are no standard requests in the list. "
                            + getNextSong(channel));
                }
                else
                {
                    bot.client.SendMessage(getNextSong(channel));
                }
            }
            else
            {
                bot.client.SendMessage("There are no songs currently in the queue!");
            }

            writeToCurrentSong(channel, true);
        }
        catch (Exception e)
        {
            Utils.errorReport(e);
            Debug.WriteLine(e.ToString());
        }
    }

    public void editCOMMAND(String message, String channel, String sender, String noEmoteMessage, OnMessageReceivedArgs e)
    {
        if (bot.checkUserLevel(sender, editComm.level, channel, e))
        {
            for (int i = 0; i < editComm.input.Length; i++)
            {
                String temp = message.ToLower();
                if (temp.Equals(editComm.input[i], StringComparison.InvariantCultureIgnoreCase))
                {
                    bot.client.SendMessage("Please type an artist and song name after the command, " + sender);
                }
                else if (temp.StartsWith(editComm.input[i]) && temp.Contains(editComm.input[i] + " "))
                {
                    try
                    {
                        if (editCurrent(channel, Utils.getFollowingText(message), sender, noEmoteMessage))
                        {
                            bot.client.SendMessage("Current song has been edited!");
                            writeToCurrentSong(channel, false);
                        }
                    }
                    catch (IOException e1)
                    {
                        Utils.errorReport(e1);
                        Debug.WriteLine(e1.ToString());
                    }
                }
            }
        }
    }

    public void addvipCOMMAND(String message, String channel, String sender, String noEmoteMessage, OnMessageReceivedArgs e)
    {
        if (bot.checkUserLevel(sender, addvipComm.level, channel, e))
        {
            for (int i = 0; i < addvipComm.input.Length; i++)
            {
                String temp = message.ToLower();
                if (temp.StartsWith(addvipComm.input[i]) && temp.Contains(addvipComm.input[i] + " "))
                {
                    try
                    {
                        String input = Utils.getFollowingText(message);
                        String youtubeID = null;
                        Video ytvid = null;
                        if (message.Contains("www.") || message.Contains("http://") || message.Contains("http://www.")
                                || message.Contains(".com") || message.Contains("https://"))
                        {
                            if (message.Contains("www.youtube.com/watch?v=")
                                    || message.Contains("www.youtube.com/watch?v="))
                            {
                                youtubeID = message.Substring(message.IndexOf("=") + 1);
                                try
                                {
                                    ytvid = bot.youtube.searchYoutubeByID(youtubeID);
                                    if (ytvid == null)
                                    {
                                        bot.client.SendMessage("Invalid youtube URL, " + sender);
                                        return;
                                    }
                                }
                                catch (Exception)
                                {
                                    bot.client.SendMessage("Invalid youtube URL, " + sender);
                                    return;
                                }
                            }
                            else if (message.Contains("https://youtu.be/"))
                            {
                                youtubeID = message.Substring(message.LastIndexOf("/") + 1);
                                try
                                {
                                    ytvid = bot.youtube.searchYoutubeByID(youtubeID);
                                    if (ytvid == null)
                                    {
                                        bot.client.SendMessage("Invalid youtube URL, " + sender);
                                        return;
                                    }
                                }
                                catch (Exception)
                                {
                                    bot.client.SendMessage("Invalid youtube URL, " + sender);
                                    return;
                                }
                            }
                            else
                            {
                                bot.client.SendMessage("Invalid Request, " + sender);
                                return;
                            }
                        }
                        if (input.EndsWith(")"))
                        {
                            String[] str = input.Split(' ');
                            String requester = str[str.Length - 1];
                            input = input.Replace(requester, "");
                            requester = requester.Replace("(", "").Replace(")", "");
                            if (ytvid != null)
                            {
                                addVip(channel, ytvid.Snippet.Title, requester, noEmoteMessage);
                            }
                            else
                            {
                                addVip(channel, input, requester, noEmoteMessage);
                            }
                            bot.client.SendMessage("VIP Song '" + input
                                    + "' has been added to the song list, " + requester + "!");
                            bot.addUserRequestAmount(requester, true);
                            writeToCurrentSong(channel, false);
                        }
                        else
                        {
                            if (ytvid != null)
                            {
                                addVip(channel, ytvid.Snippet.Title, sender, noEmoteMessage);
                                bot.client.SendMessage("VIP Song '" + ytvid.Snippet.Title
                                                + "' has been added to the song list, " + sender + "!");
                            }
                            else
                            {
                                addVip(channel, input, sender, noEmoteMessage);
                                bot.client.SendMessage("VIP Song '" + input
                                        + "' has been added to the song list, " + sender + "!");
                            }
                            writeToCurrentSong(channel, false);
                        }
                    }
                    catch (IOException e1)
                    {
                        Utils.errorReport(e1);
                        Debug.WriteLine(e1.ToString());
                    }
                }
            }
        }
    }

    public void addtopCOMMAND(String message, String channel, String sender, String noEmoteMessage, OnMessageReceivedArgs e)
    {
        if (bot.checkUserLevel(sender, addtopComm.level, channel, e))
        {
            for (int i = 0; i < addtopComm.input.Length; i++)
            {
                String temp = message.ToLower();
                if (temp.StartsWith(addtopComm.input[i]) && temp.Contains(addtopComm.input[i] + " "))
                {
                    try
                    {
                        String input = Utils.getFollowingText(message);
                        String youtubeID = null;
                        Video ytvid = null;
                        if (message.Contains("www.") || message.Contains("http://") || message.Contains("http://www.")
                                || message.Contains(".com") || message.Contains("https://"))
                        {
                            if (message.Contains("www.youtube.com/watch?v=")
                                    || message.Contains("www.youtube.com/watch?v="))
                            {
                                youtubeID = message.Substring(message.IndexOf("=") + 1);
                                try
                                {
                                    ytvid = bot.youtube.searchYoutubeByID(youtubeID);
                                    if (ytvid == null)
                                    {
                                        bot.client.SendMessage("Invalid youtube URL, " + sender);
                                        return;
                                    }
                                }
                                catch (Exception)
                                {
                                    bot.client.SendMessage("Invalid youtube URL, " + sender);
                                    return;
                                }
                            }
                            else if (message.Contains("https://youtu.be/"))
                            {
                                youtubeID = message.Substring(message.LastIndexOf("/") + 1);
                                try
                                {
                                    ytvid = bot.youtube.searchYoutubeByID(youtubeID);
                                    if (ytvid == null)
                                    {
                                        bot.client.SendMessage("Invalid youtube URL, " + sender);
                                        return;
                                    }
                                }
                                catch (Exception)
                                {
                                    bot.client.SendMessage("Invalid youtube URL, " + sender);
                                    return;
                                }
                            }
                            else
                            {
                                bot.client.SendMessage("Invalid Request, " + sender);
                                return;
                            }
                        }
                        if (input.EndsWith(")"))
                        {
                            String[] str = input.Split(' ');
                            String requester = str[str.Length - 1];
                            input = input.Replace(requester, "");
                            requester = requester.Replace("(", "").Replace(")", "");
                            if (ytvid != null)
                            {
                                addTop(channel, ytvid.Snippet.Title, requester, noEmoteMessage);
                                bot.client.SendMessage("Song '" + ytvid.Snippet.Title
                                        + "' has been added to the top of the song list, " + requester + "!");
                            }
                            else
                            {
                                addTop(channel, input, requester, noEmoteMessage);
                                bot.client.SendMessage("Song '" + input
                                        + "' has been added to the top of the song list, " + requester + "!");
                            }
                            bot.addUserRequestAmount(requester, true);
                            writeToCurrentSong(channel, false);
                        }
                        else
                        {
                            if (ytvid != null)
                            {
                                addTop(channel, ytvid.Snippet.Title, sender, noEmoteMessage);
                                bot.client.SendMessage("Song '" + ytvid.Snippet.Title
                                        + "' has been added to the top of the song list, " + sender + "!");
                            }
                            else
                            {
                                addTop(channel, input, sender, noEmoteMessage);
                                bot.client.SendMessage("Song '" + input
                                        + "' has been added to the top of the song list, " + sender + "!");
                            }
                            writeToCurrentSong(channel, false);
                        }
                    }
                    catch (IOException e1)
                    {
                        Utils.errorReport(e1);
                        Debug.WriteLine(e1.ToString());
                    }
                }
            }
        }
    }

    public void getTotalSongCOMMAND(String message, String channel, String sender, OnMessageReceivedArgs e)
    {
        if (bot.checkUserLevel(sender, getTotalComm.level, channel, e))
        {
            for (int i = 0; i < getTotalComm.input.Length; i++)
            {
                String temp = message.ToLower();
                if (temp.Equals(getTotalComm.input[i]))
                {
                    try
                    {
                        bot.client.SendMessage("The total number of songs in the queue is: "
                                + getNumberOfSongs());
                    }
                    catch (IOException e1)
                    {
                        Utils.errorReport(e1);
                        Debug.WriteLine(e1.ToString());
                    }
                }
            }
        }
    }

    public void songlistCOMMAND(String message, String channel, String sender, OnMessageReceivedArgs e)
    {
        if (bot.checkUserLevel(sender, songlistComm.level, channel, e))
        {
            for (int i = 0; i < songlistComm.input.Length; i++)
            {
                String temp = message.ToLower();
                if (temp.Equals(songlistComm.input[i]))
                {
                    try
                    {
                        if (OnlyDisplayLinkList)
                        {
                            bot.client.SendMessage("The full setlist can be found here: https://docs.google.com/spreadsheets/d/"
                                   + bot.spreadsheetId);
                            return;
                        }
                        if (Int32.Parse(getNumberOfSongs()) == 0)
                        {
                            bot.client.SendMessage("The song list is empty!");
                        }
                        else if (numOfSongsToDisplay > Int32.Parse(getNumberOfSongs()))
                        {
                            if (Int32.Parse(getNumberOfSongs()) == 1)
                            {
                                String text = "The next song in the song list: ";
                                songlist(channel, text);
                            }
                            else
                            {
                                String text = "The next " + Int32.Parse(getNumberOfSongs())
                                        + " songs in the song list: ";
                                songlist(channel, text);
                            }
                        }
                        else
                        {
                            String text = "The next " + numOfSongsToDisplay + " songs in the song list: ";
                            songlist(channel, text);
                        }
                        if (bot.spreadsheetId != null)
                        {
                            bot.client.SendMessage("The full setlist can be found here: https://docs.google.com/spreadsheets/d/"
                                    + bot.spreadsheetId);
                        }
                    }
                    catch (IOException e1)
                    {
                        Utils.errorReport(e1);
                        Debug.WriteLine(e1.ToString());
                    }
                }
            }
        }
    }

    public void songlistTimer(String channel)
    {
        if (Int32.Parse(getNumberOfSongs()) == 0)
        {
            bot.client.SendMessage("The song list is empty!");
        }
        else if (numOfSongsToDisplay > Int32.Parse(getNumberOfSongs()))
        {
            if (Int32.Parse(getNumberOfSongs()) == 1)
            {
                String text = "The next song in the song list: ";
                songlist(channel, text);
            }
            else
            {
                String text = "The next " + Int32.Parse(getNumberOfSongs()) + " songs in the song list: ";
                songlist(channel, text);
            }
        }
        else
        {
            String text = "The next " + numOfSongsToDisplay + " songs in the song list: ";
            songlist(channel, text);
        }
    }

    public void requestCOMMAND(String message, String channel, String sender, String noEmoteMessage, OnMessageReceivedArgs e)
    {
        if (bot.checkUserLevel(sender, requestComm.level, channel, e))
        {
            for (int i = 0; i < requestComm.input.Length; i++)
            {
                String temp = message.ToLower();
                String commList = "";
                for (int j = 0; j < requestComm.input.Length; j++)
                {
                    commList += ", " + String.Join("", requestComm.input[j]);
                }
                if (!commList.Equals(""))
                {
                    commList = commList.Substring(1, commList.Length - 1);
                }
                if (temp.Equals(requestComm.input[i]))
                {
                    String result = "";
                    if (mustFollowToRequest)
                    {
                        result += "You must be following the stream to request. ";
                    }
                    if (ylrequests && direquests)
                    {
                        result = "To request a song, type: " + commList + " [artist - song] OR [youtube link]";
                    }
                    else if (ylrequests)
                    {
                        result = "To request a song, type: " + commList + " [youtube link]";
                    }
                    else if (direquests)
                    {
                        result = "To request a song, type: " + commList + " [artist - song]";
                    }
                    bot.client.SendMessage(result);
                }
                else if (temp.StartsWith(requestComm.input[i]) && temp.Contains(requestComm.input[i] + " "))
                {
                    String youtubeID = null;
                    Video ytvid = null;
                    if (message.Contains("www.") || message.Contains("http://") || message.Contains("http://www.")
                            || message.Contains(".com") || message.Contains("https://"))
                    {
                        if (message.Contains("www.youtube.com/watch?v=") || message.Contains("www.youtube.com/watch?v=")
                                || message.Contains("https://youtu.be/"))
                        {
                            if (!ylrequests)
                            {
                                bot.client.SendMessage("Invalid Request, " + sender);
                                return;
                            }
                            if (message.Contains("https://youtu.be/"))
                            {
                                youtubeID = message.Substring(message.LastIndexOf("/") + 1);
                            }
                            else
                            {
                                youtubeID = message.Substring(message.IndexOf("=") + 1);
                            }
                            if (youtubeID.Contains("&"))
                            {
                                youtubeID = youtubeID.Substring(0, youtubeID.IndexOf("&"));
                            }
                            try
                            {
                                ytvid = bot.youtube.searchYoutubeByID(youtubeID);
                                if (ytvid == null)
                                {
                                    bot.client.SendMessage("Invalid youtube URL, " + sender);
                                    return;
                                }
                            }
                            catch (Exception)
                            {
                                bot.client.SendMessage("Invalid youtube URL, " + sender);
                                return;
                            }
                        }
                        else
                        {
                            bot.client.SendMessage("Invalid Request, " + sender);
                            return;
                        }
                    }
                    if (bannedKeywords != null)
                    {
                        for (int j = 0; j < bannedKeywords.Length; j++)
                        {
                            if (ytvid != null)
                            {
                                temp = ytvid.Snippet.Title;
                            }
                            if (temp.ToLower().Contains(bannedKeywords[j].ToLower()))
                            {
                                bot.client.SendMessage("Song request contains a banned keyword '"
                                        + bannedKeywords[j] + "' and cannot be added, " + sender + "!");
                                return;
                            }
                        }
                    }
                    if (Int32.Parse(getNumberOfSongs()) < maxSonglistLength)
                    {
                        if (requestsTrigger)
                        {
                            if (Utils.getFollowingText(message).Length < 100)
                            {
                                try
                                {
                                    Boolean check = true;
                                    if (!checkIfUserAlreadyHasSong(sender))
                                    {
                                        if (sender.Equals(bot.streamer) || sender.Equals(Utils.botMaker))
                                        {
                                            check = true;
                                        }
                                        else if (mustFollowToRequest && !e.ChatMessage.IsModerator && !sender.Equals(Utils.botMaker))
                                        {
                                            bot.checkAtBeginningAsync(false);
                                            BotUser u = bot.getBotUser(sender);
                                            if (u != null && !u.follower)
                                            { 
                                                check = false;
                                            }
                                        }
                                        if (check)
                                        {
                                            try
                                            {
                                                if (maxSongLength)
                                                {
                                                    try
                                                    {
                                                        String time;
                                                        if (ytvid != null)
                                                        {
                                                            time = ytvid.ContentDetails.Duration;
                                                            temp = ytvid.Snippet.Title;
                                                        }
                                                        else
                                                        {
                                                            Video v = bot.youtube.searchYoutubeByTitle(
                                                                    Utils.getFollowingText(message), maxSongLengthInMinutes);
                                                            if (v == null)
                                                            {
                                                                bot.client.SendMessage(Utils.getFollowingText(temp)
                                                                    + " is longer than " + maxSongLengthInMinutes
                                                                    + " minutes, which is the limit for standard requests, "
                                                                    + sender);
                                                                return;
                                                            }
                                                            time = v.ContentDetails.Duration;
                                                            temp = v.Snippet.Title;
                                                        }
                                                        if ((maxSongLengthInMinutes * 60) < (Utils.getDurationOfVideoInSeconds(time)))
                                                        {
                                                            bot.client.SendMessage(Utils.getFollowingText(temp)
                                                                    + " is longer than " + maxSongLengthInMinutes
                                                                    + " minutes, which is the limit for standard requests, "
                                                                    + sender);
                                                            return;
                                                        }
                                                    }
                                                    catch (Exception e1)
                                                    {
                                                        bot.client.SendMessage("Failed to get video, please try again later.");
                                                        Debug.WriteLine(e1);
                                                        Utils.errorReport(e1);
                                                        return;
                                                    }
                                                }
                                                if (ytvid != null)
                                                {
                                                    addSong(channel, ytvid.Snippet.Title, sender, noEmoteMessage);
                                                    return;
                                                }
                                                else
                                                {
                                                    if (direquests)
                                                    {
                                                        addSong(channel, Utils.getFollowingText(message), sender, noEmoteMessage);
                                                    }
                                                    else
                                                    {
                                                        bot.client.SendMessage("Only youtube link requests are allowed, " + sender);
                                                    }
                                                }
                                            }
                                            catch (IOException e1)
                                            {
                                                Utils.errorReport(e1);
                                                Debug.WriteLine(e1.ToString());
                                            }
                                        }
                                        else
                                        {
                                            bot.client.SendMessage("You must follow the stream to request a song, " + sender);
                                        }
                                    }
                                    else
                                    {
                                        if (numOfSongsInQueuePerUser == 1)
                                        {
                                            bot.client.SendMessage("You may only have 1 song in the queue at a time, " + sender
                                                    + "!");
                                        }
                                        else
                                        {
                                            bot.client.SendMessage("You may only have "
                                                    + numOfSongsInQueuePerUser + " songs in the queue at a time, "
                                                    + sender + "!");
                                        }
                                    }
                                }
                                catch (IOException e1)
                                {
                                    Utils.errorReport(e1);
                                    Debug.WriteLine(e1.ToString());
                                }
                            }
                            else
                            {
                                bot.client.SendMessage("Request input too long, please shorten request input, " + sender + "!");
                            }
                        }
                        else
                        {
                            bot.client.SendMessage("Requests are currently off!");
                        }
                    }
                    else
                    {
                        bot.client.SendMessage("Song limit of " + maxSonglistLength
                                + " has been reached, please try again later.");
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < requestComm.input.Length; i++)
            {
                String temp = message.ToLower();
                String commList = "";
                for (int j = 0; j < requestComm.input.Length; j++)
                {
                    commList += ", " + String.Join("", requestComm.input[j]);
                }
                if (!commList.Equals(""))
                {
                    commList = commList.Substring(1, commList.Length);
                }
                if (temp.StartsWith(requestComm.input[i]) && temp.Contains(requestComm.input[i] + " "))
                {
                    if (subOnlyRequests.Contains("$user"))
                    {
                        bot.client.SendMessage(subOnlyRequests.Replace("$user", sender));
                    }
                    else
                    {
                        bot.client.SendMessage(subOnlyRequests);
                    }
                }
            }
        }
    }

    public String getCurrentSongTitle(String channel)
    {
        if (Int32.Parse(getNumberOfSongs()) == 0)
        {
            return "Song list is empty";
        }
        return songList[0].name;
    }

    public String getCurrentSongRequester(String channel)
    {
        if (Int32.Parse(getNumberOfSongs()) == 0)
        {
            return "Song list is empty";
        }
        return songList[0].requester;
    }

    public void songlist(String channel, String text)
    {
        if (Int32.Parse(getNumberOfSongs()) == 0)
        {
            bot.client.SendMessage("The song list is empty!");
        }
        if (!displayOneLine)
        {
            for (int i = 0; i < songList.Count; i++)
            {
                if (i < numOfSongsToDisplay)
                {
                    bot.client.SendMessage((i + 1) + ". " + songList[i].level + " " + songList[i].name + " (" + songList[i].requester + ")");
                }
            }
        }
        else
        {
            String temp = "";
            for (int i = 0; i < songList.Count; i++)
            {
                if (i < numOfSongsToDisplay)
                {
                    temp += (i + 1) + ". " + songList[i].level + " " + songList[i].name + " (" + songList[i].requester + ") ";
                }
            }
            bot.client.SendMessage(text + " " + temp);
        }
    }

    public Boolean nextSong(String channel)
    {
        if ((Int32.Parse(getNumberOfSongs()) == 0))
        {
            return false;
        }
        else
        {
            if (!doNotWriteToHistory)
            {
                lastSong = getCurrentSongTitle(channel);
                if (songList[0].youtubeTitle == null || songList[0].youtubeTitle.Equals(""))
                {
                    appendToLastSongs(channel, lastSong);
                }
                else
                {
                    appendToLastSongs(channel, songList[0].youtubeTitle);
                }
            }
            doNotWriteToHistory = false;
            songList.RemoveAt(0);
            songsPlayedThisStream += 1;
            songsPlayedTotal += 1;
            return true;
        }
    }

    public void setSongsIfUserIsHere()
    {
        if (songList.Count > 0)
        {
            List<String> str = bot.getAllViewers(bot.streamer);
            for (int i = 0; i < songList.Count; i++)
            {
                songList[i].requesterIsHere = "";
                if (songList[i].requester.Equals(bot.streamer, StringComparison.InvariantCultureIgnoreCase) || str.Contains(songList[i].requester))
                {
                    songList[i].requesterIsHere = "[IN CHAT]";
                }
            }
        }
    }

    public String getNextSong(String channel)
    {
        decrementRandomNextEligiblity();
        if (Int32.Parse(getNumberOfSongs()) == 0)
        {
            return "There are no songs in the queue!";
        }
        String song = songList[0].name;
        String requestedby = songList[0].requester;
        String output = "";
        if (displayIfUserIsHere)
        {
            if (bot.checkIfUserIsHere(requestedby, channel))
            {
                if (whisperToUser)
                {
                    String toWhisper = requestedby.Substring(1, requestedby.Length - 1);
                    if (!bot.streamer.ToLower().Equals(toWhisper.ToLower()))
                    {
                        bot.client.SendMessage("/w " + toWhisper + " Your request '" + song
                                + "' is being played next in " + bot.streamer + "'s stream!");
                    }
                }
                output = "The next song is: '" + song + "' - " + requestedby + " HERE! :)";
            }
            else
            {
                output = "The next song is: '" + song + "' - " + requestedby;
            }
        }
        else
        {
            output = "The next song is: '" + song + "' - " + requestedby;
        }
        if (displayCFLink && !SongList[0].officialSong && !songList[0].customsForgeLink.Equals(""))
        {
            Process.Start(songList[0].customsForgeLink);
        }
        return output;
    }

    public String getNumberOfSongs()
    {
        return songList.Count.ToString();
    }

    public Boolean editCurrent(String channel, String newSong, String sender, String noEmoteMessage)
    {
        if ((Int32.Parse(getNumberOfSongs()) == 0))
        {
            if (addSongToList(newSong, sender, "", -1, noEmoteMessage, true, false))
            {
                bot.client.SendMessage("Since there are no songs in the song list, song '" + newSong
                        + "' has been added to the song list, " + sender + "!");
                writeToCurrentSong(channel, false);
            }
            return false;
        }
        songList[0].name = newSong;
        songList[0].clearExtraData(bot);
        writeToCurrentSong(channel, false);
        return true;
    }

    public Boolean checkIfUserAlreadyHasSong(String user)
    {
        if (user.Equals(bot.streamer))
        {
            return false;
        }
        int count = 0;
        for (int i = 0; i < songList.Count; i++)
        {
            if (songList[i].requester.Equals(user, StringComparison.InvariantCultureIgnoreCase) && (songList[i].level.Equals("")))
            {
                count++;
            }
        }
        if (count >= numOfSongsInQueuePerUser)
        {
            return true;
        }
        return false;
    }

    public void removeSongCOMMAND(String sender, String channel, String streamer, List<BotUser> users, String message, String temp, OnMessageReceivedArgs e)
    {
        if (sender.Equals(streamer) || e.ChatMessage.IsModerator
                        || sender.Equals(Utils.botMaker))
        {
            int number = -1;
            try
            {
                number = Int32.Parse(Utils.getFollowingText(message));
                number -= 1;
                if (Int32.Parse(getNumberOfSongs()) == 0)
                {
                    bot.client.SendMessage("The song list is empty, " + sender + "!");
                    return;
                }
                if (Int32.Parse(getNumberOfSongs()) <= number || number < 0)
                {
                    bot.client.SendMessage("Song #" + number + " does not exist, " + sender + "!");
                    return;
                }
            }
            catch
            {
                bot.client.SendMessage("To remove a song, it must be in the form '!removesong #'");
                return;
            }
            String s = songList[number].name;
            songList.RemoveAt(number);
            bot.client.SendMessage("Song #" + (number + 1) + ", '" + s + "' has been removed from the queue, " + sender + "!");
            return;
        }
    }
    
    public void promoteSongCommand(String sender, String channel, String streamer, List<BotUser> users, String message, String noEmoteMessage, OnMessageReceivedArgs e)
    {
        if (sender.Equals(streamer) || e.ChatMessage.IsModerator
                    || sender.Equals(Utils.botMaker))
        {
            String user = Utils.getFollowingText(message);
            if (user.Contains("@"))
            {
                user = user.Replace("@", "");
            }
            for (int j = 0; j < songList.Count; j++)
            {
                Song s = songList[j];
                if (s.requester.Equals(user, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (s.level.Equals("VIP"))
                    {
                        songList.RemoveAt(j);
                        addDonator(channel, s.name, user, noEmoteMessage);
                        bot.client.SendMessage("VIP Song '" + s.name
                                + "' has been promoted to $$$, " + sender + "!");
                        return;
                    }
                    else if (s.level.Equals("$$$"))
                    {
                        bot.client.SendMessage("Cannot promote a $$$ song any higher, " + sender
                                + "!");
                        return;
                    }
                    else
                    {
                        songList.RemoveAt(j);
                        bot.requestSystem.addVip(channel, s.name, user, noEmoteMessage);
                        BotUser u = bot.getBotUser(user);
                        if (u != null)
                        {
                            u.points += regSongCost;
                        }
                        bot.client.SendMessage("Song '" + s.name + "' has been promoted to VIP, "
                                + sender + "!");
                        return;
                    }
                }
            }
            bot.client.SendMessage(user + " does not have a song in the list, "
                    + sender + "!");
            return;
        }
    }

    public void addSong(String channel, String song, String requestedby, String noEmoteMessage)
    {
        Boolean costs = false;
        if (regSongCost > 0)
        {
            costs = true;
        }
        if (addSongToList(song, requestedby, "", -1, noEmoteMessage, true, costs))
        {
            if (costs)
            {
                bot.client.SendMessage(requestedby + " has cashed in " + regSongCost + " " + bot.currency.currencyName + " and the song '" + song + 
                    "' has been added to the song list!");
            }
            else
            {
                bot.client.SendMessage("Song '" + song + "' has been added to the song list, "
                        + requestedby + "!");
            }
            bot.addUserRequestAmount(requestedby, true);
            writeToCurrentSong(channel, false);
        }
    }

    public void insertSong(String song, String requestedby, int place, String noEmoteMessage, Boolean checkCF)
    {
        String level = "";
        if (songList.Count > place)
        {
            level = songList[place].level;
        }
        if (place >= songList.Count)
        {
            addSongToList(song, requestedby, "", -1, noEmoteMessage, checkCF, false);
        }
        else
        {
            addSongToList(song, requestedby, "", place, noEmoteMessage, checkCF, false);
        }
        writeToCurrentSong(bot.channel, true);
    }

    public void addTop(String channel, String song, String requestedby, String noEmoteMessage)
    {
        addSongToList(song, requestedby, "$$$", 0, noEmoteMessage, true, false);
        writeToCurrentSong(bot.channel, true);
    }

    public void addVip(String channel, String song, String requestedby, String noEmoteMessage)
    {
        if (Int32.Parse(getNumberOfSongs()) == 0)
        {
            addSongToList(song, requestedby, "VIP", 0, noEmoteMessage, true, false);
        }
        else if (Int32.Parse(getNumberOfSongs()) == 1)
        {
            if (addSongToList(song, requestedby, "VIP", -1, noEmoteMessage, true, false))
            {
                songList[0].level = "VIP";
            }
        }
        else
        {
            if (songList[0].level.Equals(""))
            {
                songList[0].level = "VIP";
            }
            for (int i = 1; i < songList.Count; i++)
            {
                Song s = songList[i];
                if (s.level.Equals(""))
                {
                    addSongToList(song, requestedby, "VIP", i, noEmoteMessage, true, false);
                    writeToCurrentSong(bot.channel, true);
                    return;
                }
            }
            addSongToList(song, requestedby, "VIP", -1, noEmoteMessage, true, false);
        }
        writeToCurrentSong(bot.channel, true);
    }

    public void addCurrentSongToFavList()
    {
        if (songList.Count == 0)
        {
            bot.client.SendMessage("There are no songs in the queue!");
            return;
        }
        String song = songList[0].name;
        song = song.Replace(',', ' ');
        favSongsPlayedThisStream.Add(song);
        favSongs.Add(song);
        bot.client.SendMessage(song + " has been added to the favorite songs list!");
    }


}
