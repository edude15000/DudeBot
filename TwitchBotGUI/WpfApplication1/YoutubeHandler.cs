using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;

public class YoutubeHandler
{
    public YoutubeHandler youtube;
    YouTubeService youtubeService = new YouTubeService(new BaseClientService.Initializer()
    {
        ApiKey = "AIzaSyDU4bPym2G64rrPgk7B9a5L6LWtIyLhFQg",
        ApplicationName = "DudeBot"
    });
    
    public Video searchYoutubeByID(String videoId) // TODO : TEST!
    {
        var video = new Video();
        video.Id = videoId;
        return video;
    }


    public Video searchYoutubeByTitle(String title)
    {
        var searchListRequest = youtubeService.Search.List("id,snippet");
        searchListRequest.Q = title; // Replace with your search term.
        searchListRequest.MaxResults = 1;
        var searchListResponse = searchListRequest.Execute();
		return searchYoutubeByID(searchListResponse.Items[0].Id.VideoId);
    }
    
}
