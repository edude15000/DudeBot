import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.Writer;
import java.net.URL;
import java.net.URLConnection;
import java.nio.charset.Charset;
import java.text.DateFormat;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Date;
import java.util.List;
import java.util.Random;
import java.util.Scanner;
import java.util.TimeZone;

import org.json.JSONArray;
import org.json.JSONObject;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;

public class Utils {
	static String version = "2.12.1"; // UPDATE AS NECESSARY
	static String releaseDate = "7/17/2017"; // UPDATE AS NECESSARY
	static String clientID = "c203ik5st5i3kde6amsageei2snaj1v";
	static String botMaker = "edude15000";
	static String songlistfile = "song.txt";
	static String templistfile = "temp.txt";
	static String currentSongFile = "currentsong.txt";
	static String currentRequesterFile = "currentrequester.txt";
	static String lastPlayedSongsFile = System.getProperty("java.io.tmpdir") + "lastsongsplayed.txt";

	public static TwitchBot loadData() throws IOException {
		GsonBuilder gsonBuilder = new GsonBuilder();
		gsonBuilder.excludeFieldsWithoutExposeAnnotation();

		Gson gson = gsonBuilder.create();
		BufferedReader br = new BufferedReader(new FileReader("UserData.json"));
		JsonParser parser = new JsonParser();
		try {
			JsonObject json = parser.parse(br).getAsJsonObject();
			br.close();
			return gson.fromJson(json, TwitchBot.class);
		} catch (Exception e) {
			System.out.println(e.toString());
		}
		return null;
	}

	public static void saveData(TwitchBot twitchBot) throws IOException {
		try {
			Writer writer = new FileWriter("UserData.json");
			GsonBuilder gsonBuilder = new GsonBuilder();

			gsonBuilder.excludeFieldsWithoutExposeAnnotation();
			gsonBuilder.setPrettyPrinting();
			gsonBuilder.create().toJson(twitchBot, writer);
			writer.close();
		} catch (Exception e) {
			System.out.println(e.toString());
		}
	}

	public static void errorReport(Exception e) {
		Writer output;
		try {
			output = new BufferedWriter(new FileWriter("errorlog.txt", true));
			Date date = new Date();
			DateFormat df = new SimpleDateFormat("HH:mm:ss");
			df.setTimeZone(TimeZone.getDefault());
			output.append(getDate() + " " + df.format(date) + " - DudeBot " + version + " Error : " + e + "\r");
			for (int i = 0; i < e.getStackTrace().length; i++) {
				output.append("..... " + e.getStackTrace()[i] + "\r");
			}
			output.append("\r");
			output.close();
		} catch (IOException e1) {
			e1.printStackTrace();
		}
	}

	public static String getDate() {
		Calendar now = Calendar.getInstance();
		String day = String.valueOf(now.get(Calendar.DAY_OF_MONTH));
		String month = String.valueOf(now.get(Calendar.MONTH));
		String year = String.valueOf(now.get(Calendar.YEAR));
		return (Integer.parseInt(month) + 1) + "/" + day + "/" + year;
	}

	static void writeVersion() throws IOException {
		BufferedWriter writer = new BufferedWriter(new FileWriter("version.txt"));
		writer.write(Utils.version);
		writer.close();
	}

	public static boolean isInteger(String s) {
		if (s.contains("-") || s.contains(" ")) {
			return false;
		}
		try {
			Integer.parseInt(s);
		} catch (NumberFormatException e) {
			return false;
		} catch (NullPointerException e) {
			return false;
		}
		return true;
	}

	public static boolean isDouble(String s) {
		if (s.contains("-") || s.contains(" ")) {
			return false;
		}
		try {
			Double.parseDouble(s);
		} catch (NumberFormatException e) {
			return false;
		} catch (NullPointerException e) {
			return false;
		}
		return true;
	}

	static double closest(double of, List<Double> in) {
		double min = Integer.MAX_VALUE;
		double closest = of;
		for (double v : in) {
			final double diff = Math.abs(v - of);
			if (diff < min) {
				min = diff;
				closest = v;
			}
		}
		return closest;
	}

	static String timeConversion(int totalSeconds) {
		final int MINUTES_IN_AN_HOUR = 60;
		final int SECONDS_IN_A_MINUTE = 60;
		int seconds = totalSeconds % SECONDS_IN_A_MINUTE;
		int totalMinutes = totalSeconds / SECONDS_IN_A_MINUTE;
		int minutes = totalMinutes % MINUTES_IN_AN_HOUR;
		int hours = totalMinutes / MINUTES_IN_AN_HOUR;
		if (hours == 0 && minutes == 0) {
			if (seconds == 1) {
				return seconds + " second";
			} else {
				return seconds + " seconds";
			}
		} else if (hours == 0) {
			if (minutes == 1 && seconds == 1) {
				return minutes + " minute and " + seconds + " second";
			} else if (minutes == 1) {
				return minutes + " minute and " + seconds + " seconds";
			} else if (seconds == 1) {
				return minutes + " minutes and " + seconds + " second";
			} else {
				return minutes + " minutes and " + seconds + " seconds";
			}
		} else {
			if (hours == 1 && minutes == 1 && seconds == 1) {
				return hours + " hour, " + minutes + " minute, and " + seconds + " second";
			} else if (hours == 1 && minutes == 1) {
				return hours + " hour, " + minutes + " minute, and " + seconds + " seconds";
			} else if (hours == 1 && seconds == 1) {
				return hours + " hour, " + minutes + " minutes, and " + seconds + " second";
			} else if (minutes == 1 && seconds == 1) {
				return hours + " hours, " + minutes + " minute, and " + seconds + " second";
			} else if (hours == 1) {
				return hours + " hour, " + minutes + " minutes, and " + seconds + " seconds";
			} else if (minutes == 1) {
				return hours + " hours, " + minutes + " minute, and " + seconds + " seconds";
			} else if (seconds == 1) {
				return hours + " hours, " + minutes + " minutes, and " + seconds + " second";
			} else {
				return hours + " hours, " + minutes + " minutes, and " + seconds + " seconds";
			}
		}
	}

	static String timeConversionYears(long inputSeconds) {
		long days = (inputSeconds / (3600 * 24));
		if (days < 1) {
			return "less than 1 day";
		} else if (days < 30) {
			return days + " days";
		} else {
			int daysInt = (int) Math.floor(days);
			int months = daysInt / 30;
			if (daysInt < 365) {
				int remDays = daysInt % 30;
				if (remDays == 0) {
					if (months == 1) {
						return months + " month";
					} else {
						return months + " months";
					}
				} else {
					if (months == 1 && remDays == 1) {
						return months + " month and " + remDays + " day";
					} else if (months == 1) {
						return months + " month and " + remDays + " days";
					} else if (remDays == 1) {
						return months + " months and " + remDays + " day";
					} else {
						return months + " months and " + remDays + " days";
					}
				}
			} else {
				int years = months / 12;
				int remMonths = months % 12;
				if (remMonths == 0) {
					if (years == 1) {
						return years + " year";
					} else {
						return years + " years";
					}
				} else {
					if (years == 1 && remMonths == 1) {
						return years + " year and " + remMonths + " month";
					} else if (years == 1) {
						return years + " year and " + remMonths + " months";
					} else if (remMonths == 1) {
						return years + " years and " + remMonths + " month";
					} else {
						return years + " years and " + remMonths + " months";
					}
				}
			}
		}
	}

	public static int getRandomNumber(int x) {
		Random rand = new Random();
		return rand.nextInt(x);
	}

	public static boolean checkIfUserIsOP(String user, String channel, String streamer, List<BotUser> users) {
		for (BotUser u : users) {
			if (u.username.equalsIgnoreCase(user)) {
				if (u.mod == true) {
					return true;
				} else {
					String info = callURL("http://tmi.twitch.tv/group/user/" + streamer + "/chatters");
					try {
						String info2 = info.substring(info.indexOf('['), info.indexOf(']'));
						if (info2.contains(user)) {
							u.mod = true;
							return true;
						}
					} catch (Exception E) {
						return false;
					}
				}
			}
		}
		return false;
	}

	public static boolean checkIfUserIsFollowing(String channel, String user, String streamer, List<BotUser> users)
			throws IOException {
		if (user.equals(streamer) || user.equals(Utils.botMaker)) {
			return true;
		}
		for (BotUser u : users) {
			if (u.username.equalsIgnoreCase(user)) {
				if (u.follower == true) {
					return true;
				} else {
					try {
						String check = callURL("https://api.twitch.tv/kraken/users/" + user.toString()
								+ "/follows/channels?limit=10000");
						Scanner sc = new Scanner(check);
						if (sc.nextLine().contains("Bad Request")) {
							System.out.println(
									"WARNING TWITCH KRAKEN API IS DOWN, CANNOT CHECK IF USER IS FOLLOWING! (ADDED SONG IF POSSIBLE ANYWAY)");
							sc.close();
							return true;
						}
						sc.close();
						if (check.contains(streamer)) {
							u.follower = true;
							return true;
						}
					} catch (RuntimeException e) {
						System.out.println(
								"WARNING TWITCH KRAKEN API IS DOWN, CANNOT CHECK IF USER IS FOLLOWING! (ADDED SONG IF POSSIBLE ANYWAY)");
						return true;
					}
				}
			}
		}
		return false;
	}

	public static String callURL(String myURL) {
		StringBuilder sb = new StringBuilder();
		URLConnection urlConn = null;
		InputStreamReader in = null;
		try {
			URL url = new URL(myURL);
			urlConn = url.openConnection();
			if (myURL.toLowerCase().contains("kraken")) {
				urlConn.setRequestProperty("Client-ID", Utils.clientID);
			}
			if (urlConn != null)
				urlConn.setReadTimeout(4000);
			if (urlConn != null && urlConn.getInputStream() != null) {
				in = new InputStreamReader(urlConn.getInputStream(), Charset.defaultCharset());
				BufferedReader bufferedReader = new BufferedReader(in);
				if (bufferedReader != null) {
					int cp;
					while ((cp = bufferedReader.read()) != -1) {
						sb.append((char) cp);
					}
					bufferedReader.close();
				}
			}
			in.close();
		} catch (Exception e) {
			return "";
		}
		return sb.toString();
	}

	public static String getNumberOfUsers(String channel, String streamer) {
		String info = callURL("http://tmi.twitch.tv/group/user/" + streamer + "/chatters");
		Scanner sc = new Scanner(info);
		String line;
		while ((line = sc.nextLine()) != null) {
			if (line.contains("chatter_count")) {
				break;
			}
		}
		sc.close();
		line = line.substring(line.indexOf(":") + 1, line.length() - 1);
		return line;
	}

	public static List<String> getAllViewers(String streamer) {
		ArrayList<String> users = new ArrayList<String>();
		String info = callURL("http://tmi.twitch.tv/group/user/" + streamer + "/chatters");
		try {
			JSONObject a = new JSONObject(info).getJSONObject("chatters");
			JSONArray viewers = a.getJSONArray("viewers");
			JSONArray staff = a.getJSONArray("staff");
			JSONArray admins = a.getJSONArray("admins");
			JSONArray global_mods = a.getJSONArray("global_mods");
			JSONArray moderators = a.getJSONArray("moderators");
			if (moderators != null) {
				int len = moderators.length();
				for (int i = 0; i < len; i++) {
					if (!users.contains(moderators.get(i))) {
						users.add(moderators.get(i).toString());
					}
				}
			}
			if (viewers != null) {
				int len = viewers.length();
				for (int i = 0; i < len; i++) {
					if (!users.contains(viewers.get(i))) {
						users.add(viewers.get(i).toString());
					}
				}
			}
			if (staff != null) {
				int len = staff.length();
				for (int i = 0; i < len; i++) {
					if (!users.contains(staff.get(i))) {
						users.add(staff.get(i).toString());
					}
				}
			}
			if (admins != null) {
				int len = admins.length();
				for (int i = 0; i < len; i++) {
					if (!users.contains(admins.get(i))) {
						users.add(admins.get(i).toString());
					}
				}
			}
			if (global_mods != null) {
				int len = global_mods.length();
				for (int i = 0; i < len; i++) {
					if (!users.contains(global_mods.get(i))) {
						users.add(global_mods.get(i).toString());
					}
				}
			}
		} catch (Exception ioe) {
		}
		return users;
	}

	public static String getFollowingText(String message) {
		return message.substring(message.indexOf(" ") + 1);
	}
}