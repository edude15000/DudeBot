import java.io.IOException;
import java.util.List;

import com.google.api.client.http.HttpRequest;
import com.google.api.client.http.HttpRequestInitializer;
import com.google.api.client.http.javanet.NetHttpTransport;
import com.google.api.client.json.jackson2.JacksonFactory;
import com.google.api.services.youtube.YouTube;
import com.google.api.services.youtube.model.SearchListResponse;
import com.google.api.services.youtube.model.SearchResult;
import com.google.api.services.youtube.model.Video;
import com.google.api.services.youtube.model.VideoListResponse;

public class Youtube {
	YouTube youtube;

	Youtube() {
		youtube = new YouTube.Builder(new NetHttpTransport(), new JacksonFactory(), new HttpRequestInitializer() {
			public void initialize(HttpRequest request) throws IOException {
			}
		}).setApplicationName("video-test").build();
	}

	public Video searchYoutubeByID(String videoId) throws IOException {
		YouTube.Videos.List videoRequest = youtube.videos().list("snippet,statistics,contentDetails");
		videoRequest.setId(videoId);
		videoRequest.setKey("AIzaSyDU4bPym2G64rrPgk7B9a5L6LWtIyLhFQg");
		VideoListResponse listResponse = videoRequest.execute();
		List<Video> videoList = listResponse.getItems();
		return videoList.get(0);
	}

	public Video searchYoutubeByTitle(String title) throws IOException {
		YouTube.Search.List search = youtube.search().list("id,snippet");
		search.setKey("AIzaSyDU4bPym2G64rrPgk7B9a5L6LWtIyLhFQg");
		search.setQ(title);
		search.setType("video");
		search.setFields("items(id/kind,id/videoId,snippet/title,snippet/thumbnails/default/url)");
		search.setMaxResults((long) 1);
		SearchListResponse listResponse = search.execute();
		List<SearchResult> videoList = listResponse.getItems();
		return searchYoutubeByID(videoList.get(0).getId().getVideoId());
	}
}
