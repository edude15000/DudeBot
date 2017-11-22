import java.io.File;
import java.io.PrintWriter;

public class Config {
	static String configFile = "config.txt";
	static String channel;
	static String oauth;
	static String startupMessage;
	static String line;
	static String botColor = "cadetblue";
	static TwitchBot bot;
	static String endMessage;

	public static void main(String[] args) throws Exception {
		bot = Utils.loadData(); // Sets up and connects bot object
		bot.botStartUp();
		bot.setVerbose(true);
		channel = bot.channel;
		oauth = bot.oauth;
		startupMessage = bot.startupMessage;
		endMessage = bot.endMessage;
		bot.connect("irc.twitch.tv", 6667, oauth);
		bot.joinChannel(channel);
		if (startupMessage != null) {
			bot.sendRawLine("PRIVMSG " + channel + " : /me " + startupMessage);
		} else {
			bot.sendRawLine("PRIVMSG " + channel + " : /me Hello!");
		}
		if (botColor != null) {
			bot.sendRawLine("PRIVMSG " + channel + " :" + "/color " + botColor);
		}
		File f = new File("song.txt");
		if (!f.exists()) {
			PrintWriter writer = new PrintWriter("song.txt", "UTF-8");
			writer.close();
		}
		File f2 = new File("temp.txt");
		if (!f2.exists()) {
			PrintWriter writer = new PrintWriter("temp.txt", "UTF-8");
			writer.close();
		}
		File f3 = new File("currentsong.txt");
		if (!f3.exists()) {
			PrintWriter writer = new PrintWriter("currentsong.txt", "UTF-8");
			writer.close();
		}
		bot.sendRawLine("CAP REQ :twitch.tv/membership");
	}

}