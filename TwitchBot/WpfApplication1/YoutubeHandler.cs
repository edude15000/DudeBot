using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class YoutubeHandler
{
    [JsonIgnore]
    public YoutubeHandler youtube;
    [JsonIgnore]
    YouTubeService youtubeService = new YouTubeService(new BaseClientService.Initializer()
    {
        ApiKey = Utils.googleApiKey,
        ApplicationName = "DudeBot"
    });
    
    public Video searchYoutubeByID(String videoId)
    {
        var videoRequest = youtubeService.Videos.List("snippet,statistics,contentDetails");
        videoRequest.Id = videoId;
        VideoListResponse listResponse = videoRequest.Execute();
        IList<Video> videoList = listResponse.Items;
        return videoList[0];
    }


    public Video searchYoutubeByTitle(String title)
    {
        var searchListRequest = youtubeService.Search.List("id,snippet");
        searchListRequest.Q = title;
        searchListRequest.MaxResults = 1;
        var searchListResponse = searchListRequest.Execute();
		return searchYoutubeByID(searchListResponse.Items[0].Id.VideoId);
    }
    
}
