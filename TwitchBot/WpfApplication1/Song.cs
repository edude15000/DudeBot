using Google.Apis.YouTube.v3.Data;
using IgnitionHelper.CDLC;
using IgnitionHelper.Ignition;
using System;

public class Song
{
    public String name { get; set; } = "";
    public String requester { get; set; } = "";
    public String requesterIsHere { get; set; } = "[IN CHAT]";
    public String level { get; set; } = "";
    public String youtubeLink { get; set; } = "";
    public String youtubeTitle { get; set; } = "";
    public int durationInSeconds { get; set; } = 0;
    public int index { get; set; } = 0;
    public String tuning { get; set; } = "";
    public String customsForgeLink { get; set; } = "";
    public String parts { get; set; } = "";
    public String dlcCreator { get; set; } = "";
    public String formattedDuration { get; set; } = "";
    public Boolean officialSong { get; set; } = false;

    public Song(String name, String requester, String level, TwitchBot bot)
    {
        this.name = name;
        this.requester = requester;
        this.level = level;
        if (youtubeLink == null || youtubeLink == "")
        {
            try
            {
                Video s = bot.youtube.searchYoutubeByTitle(name);
                youtubeLink = "http://www.youtube.com/watch?v=" + s.Id;
                durationInSeconds = Utils.getDurationOfVideoInSeconds(s.ContentDetails.Duration);
                formattedDuration = TimeSpan.FromSeconds(durationInSeconds).ToString(@"hh\:mm\:ss");
                youtubeTitle = s.Snippet.Title;
            }
            catch
            {
            }
        }
        
    }

    public void setEntry(CDLCEntry entry, IgnitionSearch iS)
    {
        if (entry != null && !entry.noInfo)
        {
            try
            {
                customsForgeLink = iS.ResolveDownloadURL(entry.cdlcid).ToString();
                dlcCreator = entry.creator;
                tuning = entry.tuning.ToString();
                parts = entry.parts.ToString();
                if (entry.official)
                {
                    officialSong = true;
                }
            }
            catch
            {
            }
        }
    }
    
}
