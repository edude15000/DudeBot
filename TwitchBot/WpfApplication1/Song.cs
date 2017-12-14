using Google.Apis.YouTube.v3.Data;
using System;

public class Song
{
    public String name { get; set; }
    public String requester { get; set; }
    public String level { get; set; } = "";
    public String youtubeLink { get; set; }
    public String youtubeTitle { get; set; }
    public int durationInSeconds { get; set; } = 0;
    public int index { get; set; } = 0;
    public String tuning { get; set; }
    public String customsForgeLink { get; set; }
    public String parts { get; set; }
    public String dlcCreator { get; set; }
    public String formattedDuration { get; set; } = "";

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
                youtubeLink = "http://www.youtube.com/watch?v=" + ((Video)s).Id;
                durationInSeconds = Utils.getDurationOfVideoInSeconds(((Video)s).ContentDetails.Duration);
                formattedDuration = TimeSpan.FromSeconds(durationInSeconds).ToString(@"hh\:mm\:ss");
                youtubeTitle = ((Video)s).Snippet.Title;
            }
            catch
            {
            }
        }

        // TODO : CF integration

    }
    
}
