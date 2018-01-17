using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class YoutubeHandler
{
    [JsonIgnore]
    YouTubeService youtubeService = new YouTubeService(new BaseClientService.Initializer()
    {
        ApiKey = Utils.googleApiKey,
        ApplicationName = "DudeBot"
    });
    
    public Video searchYoutubeByID(String videoId)
    {
        try
        {
            var videoRequest = youtubeService.Videos.List("snippet,statistics,contentDetails");
            videoRequest.Id = videoId;
            VideoListResponse listResponse = videoRequest.Execute();
            IList<Video> videoList = listResponse.Items;
            return videoList[0];
        }
        catch (Exception)
        {
        }
        return null;
    }

    public Video searchYoutubeByTitle(String title, int maxDuration)
    {
        try
        {
            var searchListRequest = youtubeService.Search.List("id,snippet");
            title = title.Replace("-", "");
            title = Regex.Replace(title, @"\(.*?\)", "");
            title = Regex.Replace(title, @"\s{2,}", " ");
            searchListRequest.Q = title;
            searchListRequest.MaxResults = 2;
            var searchListResponse = searchListRequest.Execute();
            if (searchListResponse.Items[0] == null)
            {
                return null;
            }
            String id = searchListResponse.Items[0].Id.VideoId;
            Video vid = searchYoutubeByID(id);
            if (Utils.getDurationOfVideoInSeconds(vid.ContentDetails.Duration) > 2700 || Utils.getDurationOfVideoInSeconds(vid.ContentDetails.Duration) > (maxDuration * 60))
            {
                if (searchListResponse.Items[1] == null)
                {
                    return null;
                }
                id = searchListResponse.Items[1].Id.VideoId;
            }
            return searchYoutubeByID(id);
        }
        catch (Exception)
        {
        }
        return null;
    }
}
