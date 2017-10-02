import java.io.BufferedReader;
import java.io.File;
import java.io.FileReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.PrintWriter;

public class Config implements Runnable {

	static TwitchBot bot;

	public static void main(String[] args) throws Exception {
		String configFile = "config.txt";
		String channel = null;
		String oauth = null;
		String startupMessage = null;
		String line, botColor = "cadetblue";
		try (BufferedReader br = new BufferedReader(new FileReader(configFile))) {
			while ((line = br.readLine()) != null) {
				if (line.contains("channel")) {
					channel = line.substring(line.lastIndexOf('=') + 1, line.length());
				} else if (line.contains("oauth")) {
					oauth = line.substring(line.lastIndexOf('=') + 1, line.length());
				} else if (line.contains("startupMessage")) {
					startupMessage = line.substring(line.lastIndexOf('=') + 1, line.length());
				} else if (line.contains("botColor=")) {
					botColor = line.substring(line.lastIndexOf('=') + 1, line.length());
				}
			}
		} catch (IOException e) {
			Utils.errorReport(e);
			e.printStackTrace();
		}
		bot = new TwitchBot();
		bot.setVerbose(true);
		bot.connect("irc.twitch.tv", 6667, oauth);
		bot.joinChannel(channel);
		bot.sendAction(channel, startupMessage);
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
		File f4 = new File(System.getProperty("java.io.tmpdir") + "backupdudebot.txt");
		if (!f4.exists()) {
			PrintWriter writer = new PrintWriter(System.getProperty("java.io.tmpdir") + "backupdudebot.txt", "UTF-8");
			writer.close();
		}
		File f5 = new File(System.getProperty("java.io.tmpdir") + "backupdudebotcommands.txt");
		if (!f5.exists()) {
			PrintWriter writer = new PrintWriter(System.getProperty("java.io.tmpdir") + "backupdudebotcommands.txt",
					"UTF-8");
			writer.close();
		}
		File f6 = new File(System.getProperty("java.io.tmpdir") + "backupdudebottimers.txt");
		if (!f6.exists()) {
			PrintWriter writer = new PrintWriter(System.getProperty("java.io.tmpdir") + "backupdudebottimers.txt",
					"UTF-8");
			writer.close();
		}
		File f7 = new File(System.getProperty("java.io.tmpdir") + "backupdudebotquotes.txt");
		if (!f7.exists()) {
			PrintWriter writer = new PrintWriter(System.getProperty("java.io.tmpdir") + "backupdudebotquotes.txt",
					"UTF-8");
			writer.close();
		}
		File f8 = new File(System.getProperty("java.io.tmpdir") + "backupothers.txt");
		if (!f8.exists()) {
			PrintWriter writer = new PrintWriter(System.getProperty("java.io.tmpdir") + "backupothers.txt", "UTF-8");
			writer.close();
		}
		File f9 = new File(System.getProperty("java.io.tmpdir") + "backupsfx.txt");
		if (!f9.exists()) {
			PrintWriter writer = new PrintWriter(System.getProperty("java.io.tmpdir") + "backupsfx.txt", "UTF-8");
			writer.close();
		}
		File f10 = new File(System.getProperty("java.io.tmpdir") + "backupusers.txt");
		if (!f10.exists()) {
			PrintWriter writer = new PrintWriter(System.getProperty("java.io.tmpdir") + "backupusers.txt", "UTF-8");
			writer.close();
		}
		File f11 = new File(System.getProperty("java.io.tmpdir") + "backupevents.txt");
		if (!f11.exists()) {
			PrintWriter writer = new PrintWriter(System.getProperty("java.io.tmpdir") + "backupevents.txt", "UTF-8");
			writer.close();
		}
		File f12 = new File(System.getProperty("java.io.tmpdir") + "backupcurrency.txt");
		if (!f12.exists()) {
			PrintWriter writer = new PrintWriter(System.getProperty("java.io.tmpdir") + "backupcurrency.txt", "UTF-8");
			writer.close();
		}
		File f13 = new File(System.getProperty("java.io.tmpdir") + "backupimages.txt");
		if (!f13.exists()) {
			PrintWriter writer = new PrintWriter(System.getProperty("java.io.tmpdir") + "backupimages.txt", "UTF-8");
			writer.close();
		}
		File f14 = new File(System.getProperty("java.io.tmpdir") + "lastsongsplayed.txt");
		if (!f14.exists()) {
			PrintWriter writer = new PrintWriter(System.getProperty("java.io.tmpdir") + "lastsongsplayed.txt", "UTF-8");
			writer.close();
		}
		File f15 = new File(System.getProperty("java.io.tmpdir") + "currentusers.txt");
		if (!f15.exists()) {
			PrintWriter writer = new PrintWriter(System.getProperty("java.io.tmpdir") + "currentusers.txt", "UTF-8");
			writer.close();
		}
		File f16 = new File(System.getProperty("java.io.tmpdir") + "backupranks.txt");
		if (!f16.exists()) {
			PrintWriter writer = new PrintWriter(System.getProperty("java.io.tmpdir") + "backupranks.txt", "UTF-8");
			writer.close();
		}
		bot.sendRawLine("CAP REQ :twitch.tv/membership");
		new Thread(new Config()).start();

	}

	@Override
	public void run() {
		String endMessage = null;
		String line, channel = "";
		String[] responses = new String[1000];
		int count = 0, timerTotal = 20;
		try {
			BufferedReader br = new BufferedReader(new FileReader("config.txt"));
			while ((line = br.readLine()) != null) {
				if (line.contains("timer=")) {
					timerTotal = Integer.parseInt(line.substring(line.indexOf("=") + 1));
				}
				if (line.contains("channel=")) {
					channel = line.substring(line.indexOf("=") + 1);
				}
			}
			br.close();
		} catch (Exception e) {
			Utils.errorReport(e);
			e.printStackTrace();
		}
		File f = new File(System.getProperty("java.io.tmpdir") + "dudebotkeyboardcontroller.txt");
		File f2 = new File(System.getProperty("java.io.tmpdir") + "dudebotsyncbot.txt");
		try {
			BufferedReader br = new BufferedReader(new FileReader("config.txt"));
			while ((line = br.readLine()) != null) {
				if (line.contains("channel=")) {
					channel = line.substring(line.indexOf("=") + 1);
				} else if (line.contains("endMessage=")) {
					endMessage = line.substring(line.lastIndexOf('=') + 1, line.length());
				}
			}
			br.close();
		} catch (Exception e) {
			Utils.errorReport(e);
			e.printStackTrace();
		}
		double start_time = System.currentTimeMillis();
		int i = 0;
		while (true) {
			try {
				String reader;
				String pidInfo = "";
				Process p = Runtime.getRuntime().exec(System.getenv("windir") + "\\system32\\" + "tasklist.exe");
				BufferedReader input = new BufferedReader(new InputStreamReader(p.getInputStream()));
				while ((reader = input.readLine()) != null) {
					pidInfo += reader;
				}
				input.close();
				if (!pidInfo.contains("DudeBot.exe")) {
					if (endMessage != null) {
						bot.sendAction(channel, endMessage);
					} else {
						bot.sendAction(channel, "Goodbye!");
					}
					System.exit(1);
				}
				BufferedReader br = new BufferedReader(new FileReader(f));
				if (br.readLine() != null) {
					br.close();
					bot.requestSystem.nextSongAuto(channel, false);
					PrintWriter writer = new PrintWriter(f);
					writer.print("");
					writer.close();
				}
				br.close();
				BufferedReader br2 = new BufferedReader(new FileReader(f2));
				if (br2.readLine() != null) {
					br2.close();
					bot.read();
					bot.requestSystem.writeToCurrentSong(channel, false);
					PrintWriter writer2 = new PrintWriter(f2);
					writer2.print("");
					writer2.close();
				}
				br2.close();
				if ((System.currentTimeMillis() - start_time) >= (timerTotal * 60000)) {
					start_time = System.currentTimeMillis();
					BufferedReader br3 = new BufferedReader(new FileReader("timedcommands.txt"));
					while ((line = br3.readLine()) != null) {
						if (!line.endsWith("\t0")) {
							responses[count] = line.substring(line.indexOf("\t") + 1);
							if (responses[count].endsWith("\t1")) {
								responses[count] = responses[count].substring(0, responses[count].length() - 2);
							}
							count++;
						}
					}
					br3.close();
					if (responses[i].equals("$songlist")) {
						try {
							bot.requestSystem.songlistTimer(channel);
						} catch (NumberFormatException | IOException e) {
							Utils.errorReport(e);
							e.printStackTrace();
						}
					} else {
						bot.sendRawLine("PRIVMSG " + channel + " :" + responses[i]);
					}
					i++;
					if (i >= count) {
						i = 0;
					}
				}
				Thread.sleep(1000);
			} catch (Exception e) {
				System.out.println("error");
				Utils.errorReport(e);
				e.printStackTrace();
			}
		}
	}
}