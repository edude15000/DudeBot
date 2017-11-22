import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.PrintWriter;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.text.SimpleDateFormat;
import java.time.Instant;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Date;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Random;

import org.jibble.pircbot.PircBot;
import org.jibble.pircbot.User;
import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import com.google.gson.annotations.Expose;

public class TwitchBot extends PircBot {
	@Expose(serialize = true, deserialize = true)
	String oauth, streamer, channel, botName, followersTextFile, subTextFile, followerMessage, subMessage,
			minigameEndMessage, giveawaycommandname, spreadsheetId, botColor, endMessage, startupMessage;
	@Expose(serialize = true, deserialize = true)
	int counter1, counter2, counter3, counter4, counter5, minigameTimer;
	@Expose(serialize = true, deserialize = true)
	long gameStartTime;
	@Expose(serialize = true, deserialize = true)
	boolean minigameTriggered = false, timeFinished = false, minigameOn, amountResult, adventureToggle,
			startAdventure = false, waitForAdventureCoolDown = false, raffleInProgress = false, autoShoutoutOnHost,
			gambleToggle, currencyToggle;
	@Expose(serialize = true, deserialize = true)
	Youtube youtube;
	@Expose(serialize = true, deserialize = true)
	Google google;
	@Expose(serialize = true, deserialize = true)
	TextAdventure textAdventure;
	@Expose(serialize = true, deserialize = true)
	Currency currency;
	@Expose(serialize = true, deserialize = true)
	Image image;
	@Expose(serialize = true, deserialize = true)
	SoundEffect soundEffect;
	@Expose(serialize = true, deserialize = true)
	Quote quote;
	@Expose(serialize = true, deserialize = true)
	RequestSystem requestSystem;
	@Expose(serialize = true, deserialize = true)
	ArrayList<Command> sfxCommandList = new ArrayList<Command>(), userCommandList = new ArrayList<Command>(),
			timerCommandList = new ArrayList<Command>(), botCommandList = new ArrayList<Command>(),
			imageCommandList = new ArrayList<Command>(), commandList = new ArrayList<Command>();
	@Expose(serialize = true, deserialize = true)
	ArrayList<BotUser> users = new ArrayList<BotUser>();
	@Expose(serialize = true, deserialize = true)
	ArrayList<Double> gameGuess = new ArrayList<Double>();
	@Expose(serialize = true, deserialize = true)
	ArrayList<String> raffleUsers = new ArrayList<String>(), allHosts = new ArrayList<String>(),
			gameUser = new ArrayList<String>(), extraCommandNames = new ArrayList<>();
	@Expose(serialize = true, deserialize = true)
	Map<String, String> events = new HashMap<String, String>();
	@Expose(serialize = true, deserialize = true)
	Command getViewerComm;

	public void botStartUp() throws IOException { // Starts bot up, calls necessary threads and methods
		this.setName(botName);
		this.isConnected();
		setClasses();
		System.out.println("DudeBot Version: " + Utils.version + " Release Date: " + Utils.releaseDate);
		Utils.writeVersion();
		textAdventure.startAdventuring(new ArrayList<String>(), (int) textAdventure.adventureStartTime * 1000);
		threads();
		this.channel = "#" + streamer;
	}

	public void setClasses() { // Resets classes within bot using bot as passed variables to request system and
								// quote, resets google
		requestSystem.bot = this;
		quote.bot = this;
		currency.users = users;
		google = new Google();
		youtube = new Youtube();
		google.spreadsheetId = spreadsheetId;
	}

	public void resetAllCommands() { // Sets all command types for quicker type checking, sets all command names
		botCommandList = getCommands("bot");
		sfxCommandList = getCommands("sfx");
		userCommandList = getCommands("user");
		timerCommandList = getCommands("timer");
		imageCommandList = getCommands("image");
		extraCommandNames = setExtraCommandNames();
	}

	public void save() { // This may be unnecessary, cannot use 'this' within thread so calls this method
							// instead
		try {
			Utils.saveData(this);
		} catch (IOException e) {
			e.printStackTrace();
			Utils.errorReport(e);
		}
	}

	public void threads() {
		new Thread(new Runnable() {
			public void run() {
				try {
					Thread.sleep(1000);
				} catch (InterruptedException e1) {
					e1.printStackTrace();
					Utils.errorReport(e1);
				}
				int count = 0, timerTotal = 20;
				File f = new File(System.getProperty("java.io.tmpdir") + "dudebotkeyboardcontroller.txt");
				File f2 = new File(System.getProperty("java.io.tmpdir") + "dudebotsyncbot.txt");
				double start_time = System.currentTimeMillis();
				int i = 0;
				while (true) {
					try {
						String reader; // This section checks to see if GUI is still running. If not, closes bot.jar
						String pidInfo = "";
						Process p = Runtime.getRuntime()
								.exec(System.getenv("windir") + "\\system32\\" + "tasklist.exe");
						BufferedReader input = new BufferedReader(new InputStreamReader(p.getInputStream()));
						while ((reader = input.readLine()) != null) {
							pidInfo += reader;
						}
						input.close();

						if (!pidInfo.contains("DudeBot.exe")) {
							if (endMessage != null) {
								sendRawLine("PRIVMSG " + channel + " : /me " + endMessage);
							} else {
								sendRawLine("PRIVMSG " + channel + " : /me Goodbye!");
							}
							clearUpTempData();
							save();
							System.exit(1);
						}

						BufferedReader br = new BufferedReader(new FileReader(f)); // Clears autonextsong file and calls
																					// next
																					// song if needed
						if (br.readLine() != null) {
							br.close();
							requestSystem.nextSongAuto(channel, false);
							PrintWriter writer = new PrintWriter(f);
							writer.print("");
							writer.close();
						}
						br.close();
						BufferedReader br2 = new BufferedReader(new FileReader(f2)); // Clears sync file and reads GUI
																						// updated
																						// data (if new data exists)
						if (br2.readLine() != null) {
							br2.close();
							requestSystem.writeToCurrentSong(channel, false);
							PrintWriter writer2 = new PrintWriter(f2);
							writer2.print("");
							writer2.close();
							read();
						}
						br2.close();
						if ((System.currentTimeMillis() - start_time) >= (timerTotal * 60000)) { // Prints timed
																									// commands when
																									// needed
							start_time = System.currentTimeMillis();
							if (timerCommandList.get(i).output.equals("$songlist")) {
								try {
									requestSystem.songlistTimer(channel);
								} catch (NumberFormatException | IOException e) {
									Utils.errorReport(e);
									e.printStackTrace();
								}
							} else {
								sendRawLine("PRIVMSG " + channel + " :" + timerCommandList.get(i).output);
							}
							i++;
							if (i >= count) {
								i = 0;
							}
						}
						Thread.sleep(1000);
					} catch (Exception e) {
						Utils.errorReport(e);
						e.printStackTrace();
					}
				}
			}
		}).start();
		if (currency.toggle) {
			new Thread(new Runnable() { // Thread for calling bonus all every minute if currency is toggled
				public void run() {
					while (currency.toggle) {
						try {
							if (!currency.toggle) {
								break;
							}
							Thread.sleep(60000);
							currency.bonusall(currency.currencyPerMinute, Utils.getAllViewers(streamer), true);
						} catch (InterruptedException e) {
							e.printStackTrace();
							Utils.errorReport(e);
						}
					}
				}
			}).start();
		}
		if (autoShoutoutOnHost) {
			new Thread(new Runnable() { // Thread for shouting out hosts every 5 seconds

				public void run() {
					int count = 0;
					ArrayList<String> list;
					while (true) {
						try {
							if (count > 3) {
								count = 1;
								Thread.sleep(600000);
							}
							Thread.sleep(5000);
							list = getHostList();
							for (String s : list) {
								if (!allHosts.contains(s)) {
									allHosts.add(s);
									if (count > 0) {
										sendRawLine("PRIVMSG #" + streamer + " : Thanks for the host! "
												+ (userVariables("$shoutout", "#" + streamer, streamer, s,
														"!shoutout " + s, true)));
									}
								}
							}
							count = 1;
						} catch (InterruptedException e) {
							Utils.errorReport(e);
							count++;
						}
					}
				}

			}).start();
		}
		new Thread(new Runnable() { // Thread for writing all viewer names to text file for GUI to use every 8
									// seconds
			public void run() {
				while (true) {
					try {
						Thread.sleep(8000);
						List<String> users = Utils.getAllViewers(streamer);
						BufferedWriter writer = new BufferedWriter(
								new FileWriter(System.getProperty("java.io.tmpdir") + "currentusers.txt"));
						for (String s : users) {
							writer.write(s + "\r");
						}
						writer.close();
					} catch (Exception e) {
					}
				}
			}
		}).start();
		new Thread(new Runnable() { // Thread for displaying new follower, checks every 5 seconds
			public void run() {
				ArrayList<String> users = new ArrayList<String>();
				String lastFollower = "", currentFollower = "";
				int count = 0;
				while (true) {
					try {
						Thread.sleep(5000);
						if (followersTextFile != null && !followersTextFile.equals("")) {
							BufferedReader reader = new BufferedReader(new FileReader(followersTextFile));
							currentFollower = reader.readLine();
							if (currentFollower.contains(" ")) {
								currentFollower = currentFollower.substring(currentFollower.lastIndexOf(" ")).trim();
							}
							reader.close();
							if (count > 0 && !lastFollower.equals(currentFollower)
									&& !users.contains(currentFollower)) {
								if (followerMessage.contains("$user")) {
									sendRawLine("PRIVMSG #" + streamer + " : "
											+ followerMessage.replace("$user", currentFollower));
								} else {
									sendRawLine("PRIVMSG #" + streamer + " : " + followerMessage);
								}
								users.add(currentFollower);
							}
							lastFollower = currentFollower;
							count++;
						}
					} catch (Exception e) {
					}
				}
			}
		}).start();
		new Thread(new Runnable() { // Thread for displaying new sub, checks every 5 seconds
			public void run() {
				String lastSub = "", currentSub = "";
				ArrayList<String> subList = new ArrayList<>();
				int count = 0;
				while (true) {
					try {
						Thread.sleep(5000);
						if (subTextFile != null && !subTextFile.equals("")) {
							BufferedReader reader = new BufferedReader(new FileReader(subTextFile));
							currentSub = reader.readLine();
							if (currentSub.contains(" ")) {
								currentSub = currentSub.substring(currentSub.lastIndexOf(" ")).trim();
							}
							reader.close();
							if (count > 0 && !lastSub.equals(currentSub) && !subList.contains(currentSub)) {
								if (subMessage.contains("$user")) {
									sendRawLine(
											"PRIVMSG #" + streamer + " : " + subMessage.replace("$user", currentSub));
								} else {
									sendRawLine("PRIVMSG #" + streamer + " : " + subMessage);
								}
								for (BotUser botUser : users) {
									if (botUser.username.equalsIgnoreCase(currentSub)) {
										botUser.subCredits += currency.creditsPerSub;
										botUser.sub = true;
										subList.add(botUser.username);
										break;
									}
								}
							}
							lastSub = currentSub;
							count++;
						}
					} catch (Exception e) {
					}
				}
			}
		}).start();
		new Thread(new Runnable() { // Thread for saving all data every 10 seconds
			public void run() {
				while (true) {
					try {
						Thread.sleep(10000);
						save();
					} catch (Exception e) {
					}
				}
			}
		}).start();

	}

	public void clearUpTempData() { // Resets all bot startup data
		gameStartTime = 0;
		waitForAdventureCoolDown = false;
		minigameTriggered = false;
		startAdventure = false;
		textAdventure.allowUserAdds = true;
		textAdventure.enoughPlayers = false;
		textAdventure.startTimerInMS = 0;
		image.imageStartTime = (long) 0;
		image.userCoolDowns.clear();
		soundEffect.SFXstartTime = 0;
		soundEffect.userCoolDowns.clear();
		requestSystem.favSongsPlayedThisStream.clear();
		requestSystem.doNotWriteToHistory = true;
		gameGuess.clear();
		for (BotUser botUser : users) {
			botUser.gaveSpot = false;
			botUser.vipCoolDown = 0;
			botUser.gambleCoolDown = 0;
		}

	}

	public ArrayList<Command> getCommands(String type) {
		ArrayList<Command> list = new ArrayList<>();
		for (Command c : commandList) {
			if (c.commandType.equals(type)) {
				list.add(c);
			}
		}
		return list;
	}

	public ArrayList<String> setExtraCommandNames() {
		String[] result = { "!givespot", "!regnext", "!nextreg", "!regularnext", "!nextregular", "!quote", "!addquote",
				"!quotes", "!minigame", "!startgame", "!guess", "!endgame", "!sfx", "!images", "!totalrequests",
				"!toprequester", "!setcounter1", "!setcounter2", "!setcounter3", "!setcounter4", "!setcounter5",
				"!addcom", "!botcolor", "!colorbot", "!adventure", "!bonus", "!bonusall", giveawaycommandname,
				"!gamble", "!currency", currency.currencyCommand, "!vipsong", "!vipsongon", "!vipsongoff", "!info",
				"!rank", "!join", "!leaderboards", "!removesong", "!promote", "!rankup", "!editquote", "!changequote",
				"!removequote", "!deletequote", "!subcredits", "!givecredits", "!subsong" };
		return new ArrayList<String>(Arrays.asList(result));
		// ADD TO AS NEEDED
	}

	public void addUserRequestAmount(String sender, boolean operator) throws FileNotFoundException, IOException {
		for (BotUser botUser : users) {
			if (botUser.username.equalsIgnoreCase(sender)) {
				if (operator) {
					botUser.numRequests += 1;
				} else {
					botUser.numRequests -= 1;
					if (botUser.numRequests < 0) {
						botUser.numRequests = 0;
					}
				}
				break;
			}
		}
		Utils.saveData(this);
	}

	public void onMessage(String channel, String sender, String login, String hostname, String message) {
		if (!message.startsWith("!")) {
			return;
		}
		if (sender.equalsIgnoreCase(botName)) {
			return;
		}
		String temp = message.toLowerCase();
		if (minigameTriggered && !timeFinished) {
			if (gameStartTime + minigameTimer < System.currentTimeMillis()) {
				timeFinished = true;
			}
		}
		// SFX
		for (int i = 0; i < sfxCommandList.size(); i++) {
			if (temp.startsWith(sfxCommandList.get(i).input[0])) {
				new Thread(new Runnable() {
					public void run() {
						soundEffect.sfxCOMMANDS(message, channel, sender, sfxCommandList);
					}
				}).start();
			}
		}

		// Images
		for (int i = 0; i < imageCommandList.size(); i++) {
			if (temp.startsWith(imageCommandList.get(i).input[0])) {
				new Thread(new Runnable() {

					public void run() {
						image.imageCOMMANDS(message, channel, sender, imageCommandList);
					}
				}).start();
			}
		}

		// USER COMMANDS
		for (

				int i = 0; i < userCommandList.size(); i++) {
			if (temp.startsWith(userCommandList.get(i).input[0] + " ")
					|| temp.equalsIgnoreCase(userCommandList.get(i).input[0])) {
				userCOMMANDS(message, channel, sender);
				return;
			}
		}
		// USER TIMER COMMANDS
		for (int i = 0; i < timerCommandList.size(); i++) {
			if (temp.startsWith(timerCommandList.get(i).input[0] + " ")
					|| temp.equalsIgnoreCase(timerCommandList.get(i).input[0])) {
				userCOMMANDS(message, channel, sender);
				return;
			}
		}
		// SFX COMMAND
		if (temp.equals("!sfx")) {
			if (sfxCommandList.size() == 0) {
				return;
			}
			String line = "";
			for (int i = 0; i < sfxCommandList.size(); i++) {
				line = line + sfxCommandList.get(i).input[0] + ", ";
			}
			line = line.substring(0, line.length() - 2).trim();
			sendRawLine("PRIVMSG " + channel + " :" + "Sound Effects: " + line);
			return;
		}
		// Images COMMAND
		if (temp.equals("!images")) {
			if (imageCommandList.size() == 0) {
				return;
			}
			String line = "";
			for (int i = 0; i < imageCommandList.size(); i++) {
				line = line + imageCommandList.get(i).input[0] + ", ";
			}
			line = line.substring(0, line.length() - 2).trim();
			sendRawLine("PRIVMSG " + channel + " :" + "Images: " + line);
			return;
		}
		// REQUEST
		for (int i = 0; i < requestSystem.requestComm.input.length; i++) {
			if (temp.startsWith(requestSystem.requestComm.input[i] + " ")
					|| temp.equalsIgnoreCase(requestSystem.requestComm.input[i])) {
				try {
					requestSystem.requestCOMMAND(message, channel, sender);
				} catch (NumberFormatException | IOException e1) {
					Utils.errorReport(e1);
					e1.printStackTrace();
				}
				return;
			}
		}
		// SKIP TO NEXT SONG
		for (int i = 0; i < requestSystem.nextComm.input.length; i++) {
			if (temp.equalsIgnoreCase(requestSystem.nextComm.input[i])) {
				try {
					requestSystem.nextCOMMAND(message, channel, sender);
				} catch (NumberFormatException | IOException e1) {
					Utils.errorReport(e1);
					e1.printStackTrace();
				}
				return;
			}
		}
		// CLEAR
		for (int i = 0; i < requestSystem.clearComm.input.length; i++) {
			if (temp.equalsIgnoreCase(requestSystem.clearComm.input[i])) {
				try {
					requestSystem.clearCOMMAND(message, channel, sender);
				} catch (IOException e1) {
					Utils.errorReport(e1);
					e1.printStackTrace();
				}
				return;
			}
		}
		// EDIT COMMAND
		for (int i = 0; i < requestSystem.editComm.input.length; i++) {
			if (temp.startsWith(requestSystem.editComm.input[i] + " ")) {
				try {
					requestSystem.editCOMMAND(message, channel, sender);
				} catch (InterruptedException e1) {
					Utils.errorReport(e1);
					e1.printStackTrace();
				}
				return;
			}
		}
		// ADD VIP COMMAND
		for (int i = 0; i < requestSystem.addvipComm.input.length; i++) {
			if (temp.startsWith(requestSystem.addvipComm.input[i] + " ")) {
				try {
					requestSystem.addvipCOMMAND(message, channel, sender);
				} catch (InterruptedException e1) {
					Utils.errorReport(e1);
					e1.printStackTrace();
				}
				return;
			}
		}
		// ADD TOP COMMAND
		for (int i = 0; i < requestSystem.addtopComm.input.length; i++) {
			if (temp.startsWith(requestSystem.addtopComm.input[i] + " ")) {
				try {
					requestSystem.addtopCOMMAND(message, channel, sender);
				} catch (InterruptedException e1) {
					Utils.errorReport(e1);
					e1.printStackTrace();
				}
				return;
			}
		}
		// getTotalSongs COMMAND
		for (int i = 0; i < requestSystem.getTotalComm.input.length; i++) {
			if (temp.equalsIgnoreCase(requestSystem.getTotalComm.input[i])) {
				try {
					requestSystem.getTotalSongCOMMAND(message, channel, sender);
				} catch (InterruptedException e1) {
					Utils.errorReport(e1);
					e1.printStackTrace();
				}
				return;
			}
		}
		// SONG LIST
		for (int i = 0; i < requestSystem.songlistComm.input.length; i++) {
			if (temp.equalsIgnoreCase(requestSystem.songlistComm.input[i])) {
				try {
					requestSystem.songlistCOMMAND(message, channel, sender);
				} catch (InterruptedException e1) {
					Utils.errorReport(e1);
					e1.printStackTrace();
				}
				return;
			}
		}
		// CURRENT SONG
		for (int i = 0; i < requestSystem.getCurrentComm.input.length; i++) {
			if (message.equalsIgnoreCase(requestSystem.getCurrentComm.input[i])) {
				try {
					requestSystem.getCurrentSongCOMMAND(message, channel, sender);
				} catch (InterruptedException e1) {
					Utils.errorReport(e1);
					e1.printStackTrace();
				}
				return;
			}
		}
		// TRIGGER REQUESTS
		for (int i = 0; i < requestSystem.triggerRequestsComm.input.length; i++) {
			if (temp.startsWith(requestSystem.triggerRequestsComm.input[i] + " ")) {
				try {
					requestSystem.triggerRequestsCOMMAND(message, channel, sender);
				} catch (IOException e) {
					Utils.errorReport(e);
					e.printStackTrace();
				}
				return;
			}
		}
		// GET NEXT SONG
		for (int i = 0; i < requestSystem.getNextComm.input.length; i++) {
			if (message.equalsIgnoreCase(requestSystem.getNextComm.input[i])) {
				try {
					requestSystem.getNextSongCOMMAND(message, channel, sender);
				} catch (InterruptedException e1) {
					Utils.errorReport(e1);
					e1.printStackTrace();
				}
				return;
			}
		}
		// Currency Commands
		if (message.equalsIgnoreCase("!currency on")) {
			if (sender.equalsIgnoreCase(Utils.botMaker) || sender.equals(streamer)) {
				if (!currency.toggle) {
					try {
						triggerCurrency(true, channel);
					} catch (IOException e) {
						Utils.errorReport(e);
						e.printStackTrace();
					}
					new Thread(new Runnable() {
						public void run() {
							while (currency.toggle) {
								try {
									if (!currency.toggle) {
										break;
									}
									Thread.sleep(60000);
									currency.bonusall(currency.currencyPerMinute, Utils.getAllViewers(streamer), true);
								} catch (InterruptedException e) {
									Utils.errorReport(e);
									e.printStackTrace();
								}
							}
						}
					}).start();
					sendRawLine(
							"PRIVMSG " + channel + " :" + "The currency system has been turned on, " + sender + "!");
				} else {
					sendRawLine("PRIVMSG " + channel + " :" + "The currency system is already on, " + sender + "!");
				}
			}
			return;
		}
		if (message.equalsIgnoreCase("!currency off")) {
			if (sender.equalsIgnoreCase(Utils.botMaker) || sender.equals(streamer)) {
				try {
					triggerCurrency(false, channel);
				} catch (IOException e) {
					Utils.errorReport(e);
					e.printStackTrace();
				}
			}
			return;
		}
		if (message.equalsIgnoreCase(currency.currencyCommand)) {
			if (currency.toggle) {
				sendRawLine("PRIVMSG " + channel + " :" + currency.getCurrency(sender));
			}
			return;
		}
		if (message.equalsIgnoreCase("!rank")) {
			if (currency.toggle) {
				sendRawLine("PRIVMSG " + channel + " :" + currency.getRank(sender, streamer, botName));
				return;
			}
		}
		if (message.trim().startsWith("!bonus") && !message.trim().startsWith("!bonusall")) {
			if (currency.toggle) {
				if (sender.equalsIgnoreCase(Utils.botMaker) || sender.equalsIgnoreCase(streamer)
						|| Utils.checkIfUserIsOP(sender, channel, streamer, users)) {
					if (message.equalsIgnoreCase("!bonus")) {
						sendRawLine("PRIVMSG " + channel + " :" + "Type in the format '!bonus user amount'");
					}
					String[] tempArray = Utils.getFollowingText(message).split(" ");
					String neg = "";
					if (tempArray[1].startsWith("-")) {
						neg = "-";
						tempArray[1] = tempArray[1].substring(1);
					}
					if (Utils.isInteger(tempArray[1])) {
						if (tempArray[0] instanceof String) {
							if (tempArray.length == 2) {
								if (tempArray[0].startsWith("@")) {
									tempArray[0] = tempArray[0].replace("@", "");
								}
								if (neg.equals("-")) {
									sendRawLine("PRIVMSG " + channel + " :"
											+ currency.bonus(tempArray[0], -1 * Integer.parseInt(tempArray[1])));
								} else {
									sendRawLine("PRIVMSG " + channel + " :"
											+ currency.bonus(tempArray[0], Integer.parseInt(tempArray[1])));
								}
								return;
							}
						}
					}
					sendRawLine("PRIVMSG " + channel + " :" + "Type in the format '!bonus user amount'");
				}
				return;
			}
		}
		if (message.startsWith("!bonusall ")) {
			if (currency.toggle) {
				if (sender.equalsIgnoreCase(Utils.botMaker) || sender.equalsIgnoreCase(streamer)
						|| Utils.checkIfUserIsOP(sender, channel, streamer, users)) {
					String temp2 = Utils.getFollowingText(message).trim();
					String neg = "";
					if (temp2.startsWith("-")) {
						neg = "-";
						temp2 = temp2.substring(1);
					}
					if (Utils.isInteger(temp2)) {
						if (neg.equals("-")) {
							sendRawLine("PRIVMSG " + channel + " :" + currency.bonusall(-1 * Integer.parseInt(temp2),
									Utils.getAllViewers(streamer), false));
						} else {
							sendRawLine("PRIVMSG " + channel + " :"
									+ currency.bonusall(Integer.parseInt(temp2), Utils.getAllViewers(streamer), false));
						}
					} else {
						sendRawLine("PRIVMSG " + channel + " :" + "Please type in the format '!bonusall amount', "
								+ sender + "!");
					}
				}
				return;
			}
		}
		if (message.trim().equalsIgnoreCase("!gamble on")) {
			if (sender.equals(Utils.botMaker) || sender.equals(streamer)
					|| Utils.checkIfUserIsOP(sender, channel, streamer, users)) {
				try {
					triggerGamble(true, channel);
				} catch (IOException e) {
					Utils.errorReport(e);
					e.printStackTrace();
				}
			}
			return;
		}
		if (message.trim().equalsIgnoreCase("!gamble off")) {
			if (sender.equals(Utils.botMaker) || sender.equals(streamer)
					|| Utils.checkIfUserIsOP(sender, channel, streamer, users)) {
				try {
					triggerGamble(false, channel);
				} catch (IOException e) {
					Utils.errorReport(e);
					e.printStackTrace();
				}
			}
			return;
		}
		if (message.trim().startsWith("!gamble ") || message.trim().equals("!gamble")) {
			if (currency.toggle) {
				if (message.equalsIgnoreCase("!gamble")) {
					sendRawLine("PRIVMSG " + channel + " :"
							+ "To gamble points, type '!gamble <amount>'. You may gamble every "
							+ currency.gambleCoolDownMinutes
							+ " minutes. There is a  2% chance of tripling your gamble and 38% of doubling it, "
							+ sender + "!");
					return;
				}
				String temp2 = Utils.getFollowingText(message).trim();
				if (Utils.isInteger(temp2)) {
					sendRawLine("PRIVMSG " + channel + " :" + currency.gamble(sender, Integer.parseInt(temp2)));
				} else {
					sendRawLine("PRIVMSG " + channel + " :" + "Please type in the format '!gamble amount', " + sender
							+ "!");
				}
				return;
			}
		}
		if (message.equalsIgnoreCase("!vipsongoff")) {
			try {
				requestSystem.triggerVIPs(false, channel);
			} catch (IOException e) {
				Utils.errorReport(e);
				e.printStackTrace();
			}
			return;
		}
		if (message.equalsIgnoreCase("!vipsongon")) {
			try {
				requestSystem.triggerVIPs(true, channel);
			} catch (IOException e) {
				Utils.errorReport(e);
				e.printStackTrace();
			}
			return;
		}
		if (message.toLowerCase().startsWith("!vipsong") && !message.toLowerCase().equalsIgnoreCase("!vipsong")) {
			if (currency.toggle) {
				if (currency.vipSongToggle) {
					try {
						if (currency.vipsong(sender) == 1) {
							requestSystem.addVip(channel, Utils.getFollowingText(message), sender);
							sendRawLine("PRIVMSG " + channel + " :" + sender + " cashed in " + currency.vipSongCost
									+ " " + currency.currencyName + " for a VIP song! '"
									+ Utils.getFollowingText(message)
									+ "' has been added as a VIP song to the song list!");
						} else if (currency.vipsong(sender) == 0) {
							sendRawLine("PRIVMSG " + channel + " :" + "You need " + currency.vipSongCost + " "
									+ currency.currencyName + " to buy a VIP song, " + sender + "!");
						} else {
							sendRawLine("PRIVMSG " + channel + " :" + "You may redeem a VIP song once every "
									+ currency.vipRedeemCoolDownMinutes + " minutes, " + sender + "!");
						}
					} catch (Exception e) {
						Utils.errorReport(e);
						e.printStackTrace();
					}
				} else {
					sendRawLine("PRIVMSG " + channel + " :" + "VIP Songs are currently turned off, " + sender + "!");
				}
				return;
			}
		}
		if (message.equalsIgnoreCase("!info")) {
			if (currency.toggle) {
				String result = "You can check your amount of " + currency.currencyName + " by typing '"
						+ currency.currencyCommand + "'. ";
				if (currency.currencyPerMinute > 0) {
					result += "You gain " + currency.currencyPerMinute + " " + currency.currencyName
							+ " per minute while in the stream. ";
				}
				if (currency.gambleToggle) {
					result += "You can gamble by typing '!gamble <amount>'. ";
				}
				if (currency.vipSongToggle) {
					result += "You can cash in " + currency.vipSongCost + " " + currency.currencyName
							+ " for a VIP song by typing '!vipsong <song>'. ";
				}
				result += "You can check the leaderboards by typing '!leaderboards'. ";
				if (currency.rankupUnitCost == 1) {
					result += "You can check how much it costs to rank up by typing '!nextrank' and purchase the next rank by typing '!rankup'. ";
				}
				sendRawLine("PRIVMSG " + channel + " :" + result);
			}
			return;
		}
		if (message.equalsIgnoreCase("!leaderboards")) {
			if (currency.toggle) {
				String temp2 = currency.getLeaderBoards(streamer, botName);
				if (temp2.startsWith("bad ")) {
					Exception e = new Exception(temp2);
					Utils.errorReport(e);
				}
				sendRawLine("PRIVMSG " + channel + " :" + temp2);
			}
			return;
		}

		if (message.equalsIgnoreCase("!nextrank")) {
			if (currency.toggle) {
				if (currency.ranks.size() > 0) {
					String temp2 = currency.nextRank(sender);
					sendRawLine("PRIVMSG " + channel + " :" + temp2);
				} else {
					sendRawLine("PRIVMSG " + channel + " :" + "There are no ranks in this stream.");
				}
			}
			return;
		}

		if (message.equalsIgnoreCase("!rankup")) {
			if (currency.rankupUnitCost == 1) {
				if (currency.toggle) {
					if (currency.ranks.size() > 0) {
						String temp2 = currency.rankup(sender);
						sendRawLine("PRIVMSG " + channel + " :" + temp2);
					} else {
						sendRawLine("PRIVMSG " + channel + " :" + "There are no ranks in this stream.");
					}
				}
			}
			return;
		}

		// Bot Info
		botInfoCOMMAND(message, channel, sender);
		// List commands
		if (temp.equalsIgnoreCase("!commands")) {
			listCommands(message, channel, sender);
			return;
		}
		// Edit Requester Song
		for (int i = 0; i < requestSystem.editSongComm.input.length; i++) {
			if (temp.startsWith(requestSystem.editSongComm.input[i] + " ")) {
				try {
					requestSystem.editMySongCOMMAND(message, channel, sender);
				} catch (IOException e) {
					Utils.errorReport(e);
					e.printStackTrace();
				}
				return;
			}
		}
		// Remove Requester Song
		for (int i = 0; i < requestSystem.removeSongComm.input.length; i++) {
			if (temp.equalsIgnoreCase(requestSystem.removeSongComm.input[i])) {
				try {
					requestSystem.removeMySong(message, channel, sender);
				} catch (IOException e) {
					Utils.errorReport(e);
					e.printStackTrace();
				}
				return;
			}
		}
		// Add Donator Song
		for (int i = 0; i < requestSystem.adddonatorComm.input.length; i++) {
			if (temp.startsWith(requestSystem.adddonatorComm.input[i] + " ")) {
				try {
					requestSystem.addDonatorCOMMAND(message, channel, sender);
				} catch (InterruptedException e) {
					Utils.errorReport(e);
					e.printStackTrace();
				}
				return;
			}
		}
		// CHECK SONG POSITION
		for (int i = 0; i < requestSystem.songPositionComm.input.length; i++) {
			if (temp.equalsIgnoreCase(requestSystem.songPositionComm.input[i])) {
				requestSystem.checkSongPositionCOMMAND(message, channel, sender);
				return;
			}
		}
		// RANDOM NEXT SONG
		for (int i = 0; i < requestSystem.randomComm.input.length; i++) {
			if (message.equalsIgnoreCase(requestSystem.randomComm.input[i])) {
				try {
					requestSystem.randomizerCommand(message, channel, sender);
				} catch (NumberFormatException | IOException e1) {
					Utils.errorReport(e1);
					e1.printStackTrace();
				}
				return;
			}
		}
		// Random favorite song
		for (int i = 0; i < requestSystem.favSongComm.input.length; i++) {
			if (message.equalsIgnoreCase(requestSystem.favSongComm.input[i])) {
				try {
					requestSystem.chooseRandomFavorite(message, channel, sender);
				} catch (NumberFormatException | IOException e) {
					Utils.errorReport(e);
					e.printStackTrace();
				}
				return;
			}
		}
		// Next Regular Command
		if (message.toLowerCase().equalsIgnoreCase("!nextreg") || message.toLowerCase().equalsIgnoreCase("!nextregular")
				|| message.toLowerCase().equalsIgnoreCase("!regnext")
				|| message.toLowerCase().equalsIgnoreCase("!regularnext")) {
			if (sender.equals(streamer) || Utils.checkIfUserIsOP(sender, channel, streamer, users)) {
				try {
					requestSystem.nextRegular(message, channel, sender);
				} catch (IOException | NumberFormatException | InterruptedException e) {
					Utils.errorReport(e);
					e.printStackTrace();
				}
				return;
			}
		}
		// Give Song Command
		if (message.toLowerCase().startsWith("!givespot") || message.toLowerCase().startsWith("!givesong")) {
			try {
				requestSystem.giveSpot(message, channel, sender);
			} catch (IOException e) {
				Utils.errorReport(e);
				e.printStackTrace();
			}
			return;
		}
		// Viewers COMMAND
		for (int i = 0; i < getViewerComm.input.length; i++) {
			if (temp.equals(getViewerComm.input[i])) {
				viewerCOMMAND(message, channel, sender);
				return;
			}
		}
		// UndoNext COMMAND
		if (message.equalsIgnoreCase("!undonext") || message.equals("!undoskip")) {
			if (sender.equals(Utils.botMaker) || sender.equals(streamer)
					|| Utils.checkIfUserIsOP(sender, channel, streamer, users)) {
				if (requestSystem.lastSong != null) {
					try {
						requestSystem.doNotWriteToHistory = true;
						requestSystem.addtopCOMMAND(requestSystem.addtopComm.input[0] + " " + requestSystem.lastSong,
								channel, sender);
					} catch (InterruptedException e) {
						Utils.errorReport(e);
						e.printStackTrace();
					}
					requestSystem.lastSong = null;
				} else {
					sendRawLine("PRIVMSG " + channel + " :"
							+ "Either this command has just been called or there doesn't exist a previous song, "
							+ sender);
				}
			}
		}

		// Quotes System
		if (temp.equals("!quotes on") || temp.equals("!quotes off")) {
			try {
				quote.triggerQuotes(message, channel, sender);
			} catch (IOException e1) {
				Utils.errorReport(e1);
				e1.printStackTrace();
			}
			return;
		}
		if (quote.quotesOn == true) {
			try {
				quote.quotesSystem(message, channel, sender, streamer, users);
			} catch (InterruptedException e) {
				Utils.errorReport(e);
				e.printStackTrace();
			}
		}
		// Total Requests System
		if (message.toLowerCase().equalsIgnoreCase("!totalrequests")) {
			int num = requestSystem.getNumRequests(sender);
			if (num < 1) {
				sendRawLine(
						"PRIVMSG " + channel + " :" + "You have yet to request a song in this stream, " + sender + "!");
			} else if (num == 1) {
				sendRawLine("PRIVMSG " + channel + " :" + "You have requested " + num + " song in " + streamer
						+ "'s stream, " + sender + "!");
			} else {
				sendRawLine("PRIVMSG " + channel + " :" + "You have requested " + num + " songs in " + streamer
						+ "'s stream, " + sender + "!");
			}
			return;
		}
		if (message.toLowerCase().equalsIgnoreCase("!toprequester")) {
			int max = 0;
			String user = "";
			for (int i = 0; i < users.size(); i++) {
				if (users.get(i).numRequests > max && !users.get(i).username.equalsIgnoreCase("revlobot")
						&& !users.get(i).username.equalsIgnoreCase(streamer)) {
					max = users.get(i).numRequests;
					user = users.get(i).username;
				}
			}
			if (max == 1) {
				sendRawLine("PRIVMSG " + channel + " :" + "The top requester in " + streamer + "'s stream is " + user
						+ " with " + max + " request!");
			} else if (max > 1) {
				sendRawLine("PRIVMSG " + channel + " :" + "The top requester in " + streamer + "'s stream is " + user
						+ " with " + max + " requests!");
			} else {
				sendRawLine("PRIVMSG " + channel + " :" + "Could not find any requesters");
			}
			return;
		}
		// Reset counters
		if (message.toLowerCase().startsWith("!setcounter")) {
			if (sender.equalsIgnoreCase(streamer) || sender.equals(Utils.botMaker)) {
				String num = Utils.getFollowingText(message).trim();
				if (Utils.isInteger(num)) {
					if (message.toLowerCase().startsWith("!setcounter1")) {
						setCounter(message, channel, sender, 1, Integer.parseInt(num));
						return;
					} else if (message.toLowerCase().startsWith("!setcounter2")) {
						setCounter(message, channel, sender, 2, Integer.parseInt(num));
						return;
					} else if (message.toLowerCase().startsWith("!setcounter3")) {
						setCounter(message, channel, sender, 3, Integer.parseInt(num));
						return;
					} else if (message.toLowerCase().startsWith("!setcounter4")) {
						setCounter(message, channel, sender, 4, Integer.parseInt(num));
						return;
					} else if (message.toLowerCase().startsWith("!setcounter5")) {
						setCounter(message, channel, sender, 5, Integer.parseInt(num));
						return;
					}
				}
			}
		}
		// Raffle
		if (message.equalsIgnoreCase("!startraffle")) {
			if (sender.equalsIgnoreCase(Utils.botMaker) || sender.equalsIgnoreCase(streamer)
					|| Utils.checkIfUserIsOP(sender, channel, streamer, users)) {
				if (raffleInProgress) {
					sendRawLine("PRIVMSG " + channel + " :" + "A raffle is already in progress!");
				} else {
					raffleUsers.clear();
					raffleInProgress = true;
					sendRawLine("PRIVMSG " + channel + " :" + "A raffle has just started! Type '" + giveawaycommandname
							+ "' to join the raffle!");
				}
			}
			return;
		}
		if (message.toLowerCase().startsWith("!endraffle")) {
			if (raffleInProgress) {
				if (raffleUsers.size() > 0) {
					if (message.equalsIgnoreCase("!endraffle")) {
						sendRawLine("PRIVMSG " + channel + " :" + "The winner of the raffle is: "
								+ raffleUsers.get(new Random().nextInt(raffleUsers.size())));
					} else {
						if (Utils.isInteger(Utils.getFollowingText(message))) {
							for (int i = 0; i < Integer.parseInt(Utils.getFollowingText(message)); i++) {
								if (!raffleUsers.isEmpty()) {
									String win = raffleUsers.get(new Random().nextInt(raffleUsers.size()));
									sendRawLine("PRIVMSG " + channel + " :" + "The winner of the raffle is: " + win);
									raffleUsers.remove(win);
								}
							}
						} else {
							sendRawLine("PRIVMSG " + channel + " :"
									+ "Please type in the form of '!endraffle' to choose one winner or '!endraffle <number>' to choose multiple winners, "
									+ sender + "!");
							return;
						}
					}
				} else {
					sendRawLine("PRIVMSG " + channel + " :" + "No one joined the raffle!");
				}
			} else {
				sendRawLine("PRIVMSG " + channel + " :" + "There is no raffle in progress!");
				return;
			}
			raffleUsers.clear();
			raffleInProgress = false;
			return;
		}
		if (message.equalsIgnoreCase(giveawaycommandname)) {
			if (raffleInProgress) {
				for (String s : raffleUsers) {
					if (s.equals(sender)) {
						return;
					}
				}
				raffleUsers.add(sender);
			}
			return;
		}
		// Minigame
		if (message.startsWith("!minigame") || message.startsWith("!startgame")) {
			if (sender.equals(streamer) || Utils.checkIfUserIsOP(sender, channel, streamer, users)
					|| sender.equals(Utils.botMaker)) {
				if (message.equals("!minigame on")) {
					try {
						triggerMiniGame(true, channel);
					} catch (IOException e) {
						Utils.errorReport(e);
						e.printStackTrace();
					}
					return;
				}
				if (message.equals("!minigame off")) {
					try {
						triggerMiniGame(false, channel);
					} catch (IOException e) {
						Utils.errorReport(e);
						e.printStackTrace();
					}
					return;
				}
				if (message.startsWith("!startgame") && minigameOn) {
					if (minigameTriggered) {
						sendRawLine("PRIVMSG " + channel + " :" + "The guessing game is currently aleady in progress!");
						return;
					} else {
						gameGuess.clear();
						gameUser.clear();
						sendRawLine("PRIVMSG " + channel + " :"
								+ "The guessing game has started! You may only enter once! You have "
								+ (minigameTimer / 1000) + " seconds to enter a guess by typing '!guess amount'");
						minigameTriggered = true;
						gameStartTime = System.currentTimeMillis();
						timeFinished = false;
						return;
					}
				}
			}
		}
		if (message.trim().contains("!guess ") && !message.trim().equalsIgnoreCase("!guess") && minigameOn) {
			if (minigameTriggered) {
				if (timeFinished) {
					if (minigameEndMessage.contains("$user")) {
						sendRawLine("PRIVMSG " + channel + " :" + minigameEndMessage.replace("$user", sender));
					} else {
						sendRawLine("PRIVMSG " + channel + " :" + minigameEndMessage);
					}
					return;
				}
				for (int i = 0; i < gameUser.size(); i++) {
					if (gameUser.get(i).equals(sender)) {
						sendRawLine("PRIVMSG " + channel + " :" + "You already guessed, " + sender + "!");
						return;
					}
				}
				String temp2 = Utils.getFollowingText(message).trim();
				if (Utils.isDouble(temp2)) {
					gameGuess.add(Double.parseDouble(temp2));
					gameUser.add(sender);
					return;
				} else {
					sendRawLine("PRIVMSG " + channel + " :" + "Please enter a guess in the form of '!guess amount'");
					return;
				}
			} else {
				sendRawLine("PRIVMSG " + channel + " :" + "There is no current game in progress, " + sender + "!");
				return;
			}
		}

		if (message.trim().startsWith("!endgame")) {
			if (sender.equals(streamer) || Utils.checkIfUserIsOP(sender, channel, streamer, users)
					|| sender.equals(Utils.botMaker)) {
				if (message.trim().equalsIgnoreCase("!endgame") || message.trim().equalsIgnoreCase("!cancelgame")) {
					sendRawLine("PRIVMSG " + channel + " :" + "Current guessing game has been canceled!");
					timeFinished = false;
					minigameTriggered = false;
					gameGuess.clear();
					gameUser.clear();
					return;
				}
				if (message.trim().contains("!endgame ") && !message.equalsIgnoreCase("!endgame") && minigameOn) {
					String temp2 = Utils.getFollowingText(message).trim();
					if (gameGuess.isEmpty()) {
						sendRawLine("PRIVMSG " + channel + " :" + "No one guessed, therefore no one wins! :( ");
						timeFinished = false;
						minigameTriggered = false;
						gameGuess.clear();
						gameUser.clear();
						return;
					}
					if (Utils.isDouble(temp2)) {
						String winners = "";
						double winner = Utils.closest(Double.parseDouble(temp2), gameGuess);
						int j = 0;
						while (!gameGuess.get(j).equals(winner)) {
							j++;
						}
						double win = gameGuess.get(j);
						for (int i = 0; i < gameGuess.size(); i++) {
							if (gameGuess.get(i).equals(win)) {
								winners += gameUser.get(i) + ", ";
							}
						}
						winners = winners.substring(0, winners.length() - 2).trim();
						if (winners.contains(",")) {
							sendRawLine("PRIVMSG " + channel + " :" + "The winners are " + winners + " with guesses of "
									+ gameGuess.get(j) + "!");
						} else {
							sendRawLine("PRIVMSG " + channel + " :" + "The winner is " + winners + " with a guess of "
									+ gameGuess.get(j) + "!");
						}
						if (currency.toggle) {
							String[] winnersArray = winners.split(",");
							for (String s : winnersArray) {
								if (amountResult) {
									sendRawLine("PRIVMSG " + channel + " :"
											+ currency.bonus(s.trim(), Math.round((long) Double.parseDouble(temp2))));
								}
							}
						}
						timeFinished = false;
						minigameTriggered = false;
						gameGuess.clear();
						gameUser.clear();
					} else {
						sendRawLine("PRIVMSG " + channel + " :"
								+ "Please enter a winning amount in the form of '!endgame amount'");
					}
				}
			}
			return;
		}
		// Text Adventure
		if (message.equalsIgnoreCase("!adventure on")) {
			if (Utils.checkIfUserIsOP(sender, channel, streamer, users) || sender.equals(Utils.botMaker)
					|| sender.equals(streamer)) {
				if (!adventureToggle) {
					adventureToggle = true;
					sendRawLine("PRIVMSG " + channel + " :" + "Adventure system turned on!");
				} else {
					sendRawLine("PRIVMSG " + channel + " :" + "Adventure system is already on!");
				}
				return;
			}
		}
		if (message.equalsIgnoreCase("!adventure off")) {
			if (Utils.checkIfUserIsOP(sender, channel, streamer, users) || sender.equals(Utils.botMaker)
					|| sender.equals(streamer)) {
				if (adventureToggle) {
					adventureToggle = false;
					sendRawLine("PRIVMSG " + channel + " :" + "Adventure system turned off!");
				} else {
					sendRawLine("PRIVMSG " + channel + " :" + "Adventure system is already off!");
				}
				return;
			}
		}
		if (message.equalsIgnoreCase("!adventure") || message.equalsIgnoreCase("!join")) {
			if (adventureToggle) {
				if (waitForAdventureCoolDown) {
					if (textAdventure.adventureStartTime + (textAdventure.adventureCoolDown * 60000) <= System
							.currentTimeMillis()) {
						waitForAdventureCoolDown = false;
					} else {
						double timeLeft = Math.abs(Math.ceil((int) ((System.currentTimeMillis()
								- (textAdventure.adventureStartTime + (textAdventure.adventureCoolDown * 60000)))
								/ 60000)));
						if (timeLeft == 0.0) {
							timeLeft = 1.0;
						}
						if (timeLeft == 1.0) {
							sendRawLine("PRIVMSG " + channel + " :"
									+ "We must prepare ourselves before another adventure. Please try again in "
									+ timeLeft + " minute, " + sender + "!");
						} else {
							sendRawLine("PRIVMSG " + channel + " :"
									+ "We must prepare ourselves before another adventure. Please try again in "
									+ timeLeft + " minutes, " + sender + "!");
						}
						return;
					}
				}
				if (!waitForAdventureCoolDown) {
					if (!startAdventure) {
						startAdventure = true;
						sendRawLine("PRIVMSG " + "#" + streamer.toLowerCase() + " :"
								+ textAdventure.getText().replace("$user", sender)
								+ " Type '!adventure' or '!join' to join!");
						new Thread(new Runnable() {
							public void run() {
								textAdventure.start(sender);
								if (textAdventure.enoughPlayers) {
									ArrayList<String> winners = textAdventure.selectWinners();
									String winnerString = "";
									if (winners.size() > 0) {
										for (int i = 0; i < winners.size(); i++) {
											winnerString += winners.get(i) + ", ";
										}
										winnerString = winnerString.substring(0, winnerString.trim().length() - 1);
									}
									String winMessage = textAdventure.winningMessage(winners);
									if (winMessage.contains("$users")) {
										winMessage = winMessage.replace("$users", winnerString);
									}
									if (winMessage.contains("$user")) {
										winMessage = winMessage.replace("$user", winnerString);
									}
									sendRawLine("PRIVMSG " + channel + " :" + winMessage);
									int adventurePoints = textAdventure.adventurePointsMax;
									if (textAdventure.adventurePointsMax != textAdventure.adventurePointsMin) {
										adventurePoints = new Random().nextInt(
												textAdventure.adventurePointsMax - textAdventure.adventurePointsMin)
												+ textAdventure.adventurePointsMin;
									}
									if (currency.toggle) {
										for (String s : winners) {
											sendRawLine(
													"PRIVMSG " + channel + " :" + currency.bonus(s, adventurePoints));
										}
									}
								} else {
									sendRawLine("PRIVMSG " + channel + " :"
											+ "Not enough people joined the adventure, at least 3 people are required to start an adventure. Try again later!");
								}
								startAdventure = false;
								textAdventure.adventureStartTime = System.currentTimeMillis();
								waitForAdventureCoolDown = true;
							}
						}).start();
						return;
					}
					if (startAdventure) {
						try {
							if (textAdventure.addUser(sender) == 0) {
								sendRawLine("PRIVMSG " + channel + " :" + "Please try again in a little bit, " + sender
										+ "!");
								startAdventure = false;
							}
						} catch (Exception e) {
							Utils.errorReport(e);
							e.printStackTrace();
						}
					}
					return;
				}
			}
		}
		// Change Bot Color
		if (message.trim().startsWith("!botcolor ") || message.trim().startsWith("!colorbot ")) {
			if (Utils.checkIfUserIsOP(sender, channel, streamer, users) || sender.equals(Utils.botMaker)
					|| sender.equals(streamer)) {
				String[] colors = { "Blue", "BlueViolet", "CadetBlue", "Chocolate", "Coral", "DodgerBlue", "Firebrick",
						"GoldenRod", "Green", "HotPink", "OrangeRed", "Red", "SeaGreen", "SpringGreen", "YellowGreen" };
				String color = null;
				for (int i = 0; i < colors.length; i++) {
					if (colors[i].equalsIgnoreCase(Utils.getFollowingText(message).trim())) {
						color = Utils.getFollowingText(message).trim();
						break;
					}
				}
				if (color == null) {
					sendRawLine("PRIVMSG " + channel + " :"
							+ "Invalid color, colors available: Blue, BlueViolet, CadetBlue, Chocolate, Coral, DodgerBlue, Firebrick,GoldenRod, Green, HotPink, OrangeRed, Red, SeaGreen, SpringGreen, YellowGreen");
					return;
				}
				sendRawLine("PRIVMSG " + channel + " :" + "/color " + Utils.getFollowingText(message).trim());
				botColor = color;
			}
		}
		// Others
		if (message.trim().equalsIgnoreCase("!rocksmithunite")) {
			sendRawLine("PRIVMSG " + channel + " :"
					+ "Make sure to checkout RocksmithUnite at https://www.twitch.tv/team/rocksmithunite");
			return;
		}

		// Edit / Remove Songs
		if (message.trim().toLowerCase().startsWith("!removesong ")
				|| message.trim().toLowerCase().startsWith("!deletesong ")) {
			if (sender.equals(streamer) || Utils.checkIfUserIsOP(sender, channel, streamer, users)
					|| sender.equals(Utils.botMaker)) {
				int number = -1;
				try {
					number = Integer.parseInt(Utils.getFollowingText(message));
					if (Integer.parseInt(requestSystem.getNumberOfSongs()) == 0) {
						sendRawLine("PRIVMSG " + channel + " : The song list is empty, " + sender + "!");
						return;
					}
					if (Integer.parseInt(requestSystem.getNumberOfSongs()) < number || number == 0) {
						sendRawLine("PRIVMSG " + channel + " : Song #" + number + " does not exist, " + sender + "!");
						return;
					}
				} catch (Exception e) {
					sendRawLine(
							"PRIVMSG " + channel + " :" + "To remove a song, it must be in the form '!removesong #'");
					return;
				}
				try {
					Path path = Paths.get(Utils.songlistfile);
					List<String> fileContent = new ArrayList<>(Files.readAllLines(path, StandardCharsets.UTF_8));
					String temp2 = fileContent.get(number - 1);
					fileContent.remove(number - 1);
					sendRawLine("PRIVMSG " + channel + " : " + temp2 + " has been removed, " + sender + "!");
					Files.write(path, fileContent, StandardCharsets.UTF_8);
					google.writeToGoogleSheets(false, Utils.songlistfile, Utils.lastPlayedSongsFile);
					return;
				} catch (IOException e) {
					Utils.errorReport(e);
					e.printStackTrace();
				}
				sendRawLine("PRIVMSG " + channel + " : Song #" + number + " does not exist, " + sender + "!");
				return;
			}
		}

		if (message.trim().toLowerCase().startsWith("!editcom !")
				|| message.trim().toLowerCase().startsWith("!updatecom !")) {
			if (sender.equals(streamer) || Utils.checkIfUserIsOP(sender, channel, streamer, users)
					|| sender.equals(Utils.botMaker)) {
				String comm, response;
				try {
					comm = Utils.getFollowingText(message).substring(0, Utils.getFollowingText(message).indexOf(" "));
					response = Utils.getFollowingText(message)
							.substring(Utils.getFollowingText(message).indexOf(" ") + 1);
				} catch (Exception e) {
					sendRawLine("PRIVMSG " + channel + " :"
							+ "To edit a command, it must be in the form '!editcom !command response'");
					return;
				}
				for (Command command : commandList) {
					if (comm.equalsIgnoreCase(command.input[0])) {
						command.output = response;
						resetAllCommands();
						sendRawLine("PRIVMSG " + channel + " :" + comm + " has been edited, " + sender + "!");
						return;
					}
				}
				sendRawLine("PRIVMSG " + channel + " :" + comm + " does not exist, " + sender + "!");
				return;
			}
		}

		if (message.trim().toLowerCase().startsWith("!removecom !")
				|| message.trim().toLowerCase().startsWith("!deletecom !")) {
			if (sender.equals(streamer) || Utils.checkIfUserIsOP(sender, channel, streamer, users)
					|| sender.equals(Utils.botMaker)) {
				String comm;
				try {
					comm = Utils.getFollowingText(message);
				} catch (Exception e) {
					sendRawLine("PRIVMSG " + channel + " :"
							+ "To remove a command, it must be in the form '!removecom !command'");
					return;
				}
				for (Command command : commandList) {
					if (comm.equalsIgnoreCase(command.input[0])) {
						commandList.remove(command);
						resetAllCommands();
						sendRawLine("PRIVMSG " + channel + " :" + comm + " has been removed, " + sender + "!");
						return;
					}
				}
				sendRawLine("PRIVMSG " + channel + " :" + comm + " does not exist, " + sender + "!");
				return;
			}
		}

		if (message.trim().toLowerCase().startsWith("!addcom !")) {
			if (sender.equals(streamer) || Utils.checkIfUserIsOP(sender, channel, streamer, users)
					|| sender.equals(Utils.botMaker)) {
				String comm, response;
				try {
					comm = Utils.getFollowingText(message).substring(0, Utils.getFollowingText(message).indexOf(" "));
					response = Utils.getFollowingText(message)
							.substring(Utils.getFollowingText(message).indexOf(" ") + 1);
				} catch (Exception e) {
					sendRawLine("PRIVMSG " + channel + " :"
							+ "To add a command, it must be in the form '!addcom !command response'");
					return;
				}
				for (Command command : commandList) {
					if (comm.equalsIgnoreCase(command.input[0])) {
						sendRawLine("PRIVMSG " + channel + " :" + comm + " already exists, " + sender + "!");
						return;
					}
				}
				String[] str = { comm };
				commandList.add(new Command(str, 0, response, "user", true));
				sendRawLine("PRIVMSG " + channel + " :" + comm + " has been added, " + sender + "!");
				resetAllCommands();
				return;
			}
		}

		if (message.equalsIgnoreCase("!promote")) {
			if (sender.equals(streamer) || Utils.checkIfUserIsOP(sender, channel, streamer, users)
					|| sender.equals(Utils.botMaker)) {
				sendRawLine(
						"PRIVMSG " + channel + " : To promote a user's song, type '!promote @user', " + sender + "!");
				return;
			}
		}
		if (message.trim().toLowerCase().startsWith("!promote ")) {
			if (sender.equals(streamer) || Utils.checkIfUserIsOP(sender, channel, streamer, users)
					|| sender.equals(Utils.botMaker)) {
				String user = Utils.getFollowingText(message);
				Path path = Paths.get(Utils.songlistfile);
				List<String> fileContent;
				try {
					String line = "";
					fileContent = new ArrayList<>(Files.readAllLines(path, StandardCharsets.UTF_8));
					for (int j = 0; j < fileContent.size(); j++) {
						if (user.contains("@")) {
							user = user.replace("@", "");
						}
						line = fileContent.get(j);
						if (line.endsWith("(" + user + ")")) {
							String song = "";
							if (line.startsWith("VIP\t")) {
								song = line.substring(line.indexOf("\t") + 1, line.lastIndexOf("\t"));
								fileContent.remove(j);
								Files.write(path, fileContent, StandardCharsets.UTF_8);
								requestSystem.addDonator(channel, song, user);
								sendRawLine("PRIVMSG " + channel + " : VIP Song '" + song
										+ "' has been promoted to $$$, " + sender + "!");
								return;
							} else if (line.startsWith("$$$\t")) {
								sendRawLine("PRIVMSG " + channel + " : Cannot promote a $$$ song any higher, " + sender
										+ "!");
								return;
							} else {
								song = line.substring(0, line.indexOf("\t"));
								fileContent.remove(j);
								Files.write(path, fileContent, StandardCharsets.UTF_8);
								requestSystem.addVip(channel, song, user);
								sendRawLine("PRIVMSG " + channel + " : Song '" + song + "' has been promoted to VIP, "
										+ sender + "!");
								return;
							}
						}
					}
					sendRawLine("PRIVMSG " + channel + " : User '" + user + "' does not have a song in the list, "
							+ sender + "!");
				} catch (IOException e) {
					Utils.errorReport(e);
					e.printStackTrace();
				}
				return;
			}
		}

		if (message.trim().toLowerCase().equalsIgnoreCase("!subcredits")) {
			sendRawLine("PRIVMSG " + channel + " : " + currency.getSubCredits(sender));
			return;
		}

		if (message.trim().toLowerCase().startsWith("!givecredits")) {
			if (sender.equals(streamer) || Utils.checkIfUserIsOP(sender, channel, streamer, users)) {
				if (message.equalsIgnoreCase("!givecredits")) {
					sendRawLine("PRIVMSG " + channel + " :" + "Type in the format '!givecredits user amount'");
				}
				String[] tempArray = Utils.getFollowingText(message).split(" ");
				String neg = "";
				if (tempArray[1].startsWith("-")) {
					neg = "-";
					tempArray[1] = tempArray[1].substring(1);
				}
				if (Utils.isInteger(tempArray[1])) {
					if (tempArray[0] instanceof String) {
						if (tempArray.length == 2) {
							if (tempArray[0].startsWith("@")) {
								tempArray[0] = tempArray[0].replace("@", "");
							}
							if (neg.equals("-")) {
								sendRawLine("PRIVMSG " + channel + " :"
										+ currency.bonusSubCredits(tempArray[0], -1 * Integer.parseInt(tempArray[1])));
							} else {
								sendRawLine("PRIVMSG " + channel + " :"
										+ currency.bonusSubCredits(tempArray[0], Integer.parseInt(tempArray[1])));
							}
							return;
						}
					}
				}
				sendRawLine("PRIVMSG " + channel + " :" + "Type in the format '!givecredits user amount'");
			}
		}

		if (message.trim().toLowerCase().startsWith("!subsong")
				&& !message.toLowerCase().equalsIgnoreCase("!subsong")) {
			try {
				if (currency.redeemSubCredits(sender)) {
					requestSystem.addDonator(channel, Utils.getFollowingText(message), sender);
					sendRawLine("PRIVMSG " + channel + " :" + sender + " cashed in " + currency.subCreditRedeemCost
							+ " " + " sub credits for a sub song! '" + Utils.getFollowingText(message)
							+ "' has been added as a $$$ song to the song list!");
				} else {
					sendRawLine("PRIVMSG " + channel + " :" + "You need " + currency.subCreditRedeemCost + " "
							+ " sub credits to buy a $$$ song, " + sender + "!");
				}
			} catch (Exception e) {
				Utils.errorReport(e);
				e.printStackTrace();
			}
		}

	}

	public void read() {
		try {
			Config.bot = Utils.loadData();
		} catch (IOException e) {
			Utils.errorReport(e);
			e.printStackTrace();
		}
	}

	public void setCounter(String message, String channel, String sender, int counter, int value) {
		sendRawLine("PRIVMSG " + channel + " :" + "Counter" + counter + " has been set to " + value + "!");
		if (counter == 1) {
			counter1 = value;
		} else if (counter == 2) {
			counter2 = value;
		} else if (counter == 3) {
			counter3 = value;
		} else if (counter == 4) {
			counter4 = value;
		} else if (counter == 5) {
			counter5 = value;
		}
	}

	public String getRandomUser(String channel) {
		Random rand = new Random();
		int index = rand.nextInt(getUsers(channel).length);
		return getUsers(channel)[index].toString();
	}

	public void botInfoCOMMAND(String message, String channel, String sender) {
		if (message.equalsIgnoreCase("!edudebot") || message.equalsIgnoreCase("!bot")
				|| message.equalsIgnoreCase("!edude15000") || message.equalsIgnoreCase("!dudebot")) {
			sendRawLine("PRIVMSG " + channel + " :" + "Dudebot is a free bot programmed by Edude15000 using PircBot. "
					+ "If you would like to use Dudebot, the download link is here: http://dudebot.webs.com/ Make sure to join the DudeBot Discord also: https://discord.gg/NFehx5h");
		}
		if (message.equalsIgnoreCase("!version") || message.equalsIgnoreCase("!botversion")
				|| message.equalsIgnoreCase("!dudebotversion")) {
			sendRawLine("PRIVMSG " + channel + " :" + "Dudebot " + Utils.version + " (" + Utils.releaseDate + ")");
		}
		if (message.equalsIgnoreCase("!refresh")) {
			read();
		}
	}

	public void listCommands(String message, String channel, String sender) {
		String temp = message.toLowerCase();
		String line = "!givespot, !totalrequests, !toprequester, !subcredits";
		if (sfxCommandList.size() > 0) {
			line += "!sfx, ";
		}
		if (imageCommandList.size() > 0) {
			line += "!images, ";
		}
		if (currency.toggle) {
			line += "!rank, !info, !gamble, " + currency.currencyCommand + ", ";
		}
		if (temp.equalsIgnoreCase("!commands")) {
			for (Command command : botCommandList) {
				for (int i = 0; i < command.input.length; i++) {
					if (command.level == 0) {
						line += command.input[i] + ", ";
					}
				}
			}
			sendRawLine("PRIVMSG " + channel + " :" + "DudeBot Commands: " + line.substring(0, line.length() - 2));
			line = "";
			for (int i = 0; i < userCommandList.size(); i++) {
				if (!userCommandList.get(i).output.equals("$shoutout")) {
					line += userCommandList.get(i).input[0] + ", ";
				}
			}
			for (int i = 0; i < timerCommandList.size(); i++) {
				if (!timerCommandList.get(i).output.contains("$shoutout")
						&& !timerCommandList.get(i).output.equals("$shoutout")) {
					line += timerCommandList.get(i).input[0] + ", ";
				}
			}
			if (line.length() >= 2) {
				sendRawLine("PRIVMSG " + channel + " :" + "Other Commands: " + line.substring(0, line.length() - 2));
			}
			read();
		}
	}

	public boolean checkUserLevelCustomCommands(String sender, int level, String channel) {
		int holder = 0;
		if (sender.equalsIgnoreCase(Utils.botMaker)) {
			return true;
		}
		if (level == 0) {
			return true;
		}
		if (sender.equals(streamer)) {
			return true;
		}
		for (BotUser botUser : users) {
			if (botUser.username.equalsIgnoreCase(sender) && botUser.sub) {
				holder = 1;
				break;
			}
		}
		if (level <= holder) {
			return true;
		}
		if (Utils.checkIfUserIsOP(sender, channel, streamer, users)) {
			holder = 2;
		}
		if (level <= holder) {
			return true;
		}
		return false;
	}

	public boolean checkUserLevel(String sender, Command command, String channel) {
		int holder = 0;
		if (sender.equalsIgnoreCase(Utils.botMaker)) {
			return true;
		}
		if (command.level == 0) {
			return true;
		}
		if (sender.equals(streamer)) {
			return true;
		}
		for (BotUser botUser : users) {
			if (botUser.username.equalsIgnoreCase(sender) && botUser.sub) {
				holder = 1;
				break;
			}
		}
		if (command.level <= holder) {
			return true;
		}
		if (Utils.checkIfUserIsOP(sender, channel, streamer, users)) {
			holder = 2;
		}
		if (command.level <= holder) {
			return true;
		}
		return false;
	}

	public ArrayList<String> getHostList() {
		ArrayList<String> result = new ArrayList<String>();
		try {
			String id = new JSONObject(Utils.callURL("https://api.twitch.tv/kraken/channels/" + streamer))
					.getString("_id");
			JSONArray info = new JSONObject(Utils.callURL("http://tmi.twitch.tv/hosts?include_logins=1&target=" + id))
					.getJSONArray("hosts");
			for (int i = 0; i < info.length(); i++) {
				result.add(info.getJSONObject(i).getString("host_login"));
			}
		} catch (Exception e) {
			System.out.println(e);
		}
		return result;
	}

	public void triggerCurrency(boolean trigger, String channel) throws FileNotFoundException, IOException {
		if (trigger) {
			currencyToggle = true;
			sendRawLine("PRIVMSG " + channel + " :" + "Currency system turned on!");
		} else {
			currencyToggle = false;
			sendRawLine("PRIVMSG " + channel + " :" + "Currency system turned off!");
		}
	}

	public void triggerMiniGame(boolean trigger, String channel) throws FileNotFoundException, IOException {
		if (trigger) {
			minigameOn = true;
			sendRawLine("PRIVMSG " + channel + " :" + "Minigame turned on!");
		} else {
			minigameOn = false;
			sendRawLine("PRIVMSG " + channel + " :" + "Minigame turned off!");
		}
	}

	public void triggerGamble(boolean trigger, String channel) throws FileNotFoundException, IOException {
		if (trigger) {
			gambleToggle = true;
			sendRawLine("PRIVMSG " + channel + " :" + "Gambling has been turned on!");
		} else {
			gambleToggle = false;
			sendRawLine("PRIVMSG " + channel + " :" + "Gambling has been turned off!");
		}
	}

	public void viewerCOMMAND(String message, String channel, String sender) {
		if (checkUserLevel(sender, getViewerComm, channel)) {
			for (int i = 0; i < getViewerComm.input.length; i++) {
				String temp = message.toLowerCase();
				if (temp.equals(getViewerComm.input[i])) {
					sendRawLine("PRIVMSG " + channel + " :" + "Current viewer count: "
							+ Utils.getNumberOfUsers(channel, streamer));
				}
			}
		}
	}

	public void userCOMMANDS(String message, String channel, String sender) {
		for (int i = 0; i < userCommandList.size(); i++) {
			String temp = message.toLowerCase();
			if (temp.startsWith(userCommandList.get(i).input[0])
					&& checkUserLevelCustomCommands(sender, userCommandList.get(i).level, channel)) {
				sendRawLine("PRIVMSG " + channel + " :" + userVariables(userCommandList.get(i).output, channel, sender,
						Utils.getFollowingText(message), message, false));
			}
		}
		for (int i = 0; i < timerCommandList.size(); i++) {
			String temp = message.toLowerCase();
			if (temp.equals(timerCommandList.get(i).input[0])) {
				sendRawLine("PRIVMSG " + channel + " :" + userVariables(timerCommandList.get(i).output, channel, sender,
						Utils.getFollowingText(message), message, false));
			}
		}
	}

	public String userVariables(String response, String channel, String sender, String followingText, String message,
			boolean event) {
		if (followingText.startsWith("@")) {
			followingText = followingText.substring(1);
		}
		if (response.contains("$viewers")) {
			response = response.replace("$viewers", Utils.getNumberOfUsers(channel, streamer));
		}
		if (response.contains("$user")) {
			response = response.replace("$user", sender);
		}
		if (response.contains("$input")) {
			if ((message != followingText) || event) {
				response = response.replace("$input", followingText);
			} else {
				return "";
			}
		}
		if (response.contains("$length")) {
			try {
				response = response.replace("$length", requestSystem.getNumberOfSongs());
			} catch (IOException e) {
				Utils.errorReport(e);
				e.printStackTrace();
			}
		}
		if (response.contains("$randomuser")) {
			getRandomUser(channel);
		}
		if (response.contains("$currentsong")) {
			try {
				String line = requestSystem.getCurrentSongTitle(channel);
				if (line.contains("\t")) {
					line = line.substring(0, line.indexOf("\t"));
				}
				response = response.replace("$currentsong", line);
			} catch (NumberFormatException | IOException e) {
				Utils.errorReport(e);
				e.printStackTrace();
			}
		}
		if (response.contains("$currentrequester")) {
			try {
				String line = requestSystem.getCurrentSongTitle(channel);
				if (line.contains("\t")) {
					line = line.substring(line.lastIndexOf("\t") + 2, line.length() - 1);
				}
				response = response.replace("$currentrequester", line);
			} catch (NumberFormatException | IOException e) {
				Utils.errorReport(e);
				e.printStackTrace();
			}
		}
		if (response.contains("$randomuser")) {
			response = response.replace("$randomuser", getRandomUser(channel));
		}
		if (response.contains("$randomnumber3")) {
			if (message.contains("coffee") && sender.equalsIgnoreCase("hardrockangelart")) {
				response = response.replace("$randomnumber3", "1000");
			} else if (message.contains("guitar") || message.contains("bass")) {
				response = response.replace("$randomnumber3", String.valueOf(Utils.getRandomNumber(25) + 75));
			} else if (message.contains("frosk") && sender.equalsIgnoreCase("foopjohnson")
					|| message.contains("foop") && sender.equalsIgnoreCase("ninja_frosk")) {
				response = "0% Love, 100% Froskin' the Johnson!";
			} else if (message.contains("pizza") && sender.equalsIgnoreCase("nipplejesus")) {
				response = "110% PIZZA GOODNESS!";
			} else if (message.contains("pizza") && sender.equalsIgnoreCase("nipplejesus")) {
				response = "110% PIZZA GOODNESS!";
			} else if (message.contains("azeven") && sender.equalsIgnoreCase("SirBrutalify")) {
				response = "100% LOVE BABY!";
			} else if (message.contains("SirBrutalify") && sender.equalsIgnoreCase("azeven")) {
				response = "100% LOVE BABY!";
			} else {
				response = response.replace("$randomnumber3", String.valueOf(Utils.getRandomNumber(100)));
			}
		}
		if (response.contains("$randomnumber2"))

		{
			response = response.replace("$randomnumber2", String.valueOf(Utils.getRandomNumber(10)));
		}
		if (response.contains("$8ball")) {
			if ((message != followingText) || event) {
				response = response.replace("$8ball", "Magic 8-ball says..." + ballResponse(sender, streamer, message));
			} else {
				return "";
			}
		}
		if (response.contains("$streamer")) {
			response = response.replace("$streamer", streamer);
		}
		if (response.contains("$shoutout")) {
			if ((message != followingText) || event) {
				if (sender.equals(Utils.botMaker) || sender.equals(streamer)
						|| Utils.checkIfUserIsOP(sender, channel, streamer, users)) {
					String info = Utils.callURL("https://api.twitch.tv/kraken/channels/" + followingText);
					try {
						String game = new JSONObject(info).getString("game");
						if (game.equalsIgnoreCase("null")) {
							response = response.replace("$shoutout",
									"Make sure to follow " + followingText + " at twitch.tv/" + followingText);
						} else {
							response = response.replace("$shoutout", "Make sure to follow " + followingText
									+ " at twitch.tv/" + followingText + " ! They were last streaming " + game + "!");
						}
					} catch (Exception e) {
						response = response.replace("$shoutout",
								"Make sure to follow " + followingText + " at twitch.tv/" + followingText);
					}
				} else {
					response = "";
				}
			} else {
				response = "";
			}
		}
		if (response.contains("$start")) {
			String info = Utils.callURL("https://api.twitch.tv/kraken/channels/" + streamer);
			try {
				response = response.replace("$start", new SimpleDateFormat("MM/dd/yyyy")
						.format(new Date(Instant.parse(new JSONObject(info).getString("created_at")).toEpochMilli())));
			} catch (JSONException e) {
				response = "";
			}
		}
		if (response.contains("$uptime")) {
			String info = Utils.callURL("https://api.twitch.tv/kraken/streams/" + streamer);
			try {
				JSONObject json = new JSONObject(info);
				String name = json.getJSONObject("stream").getString("created_at");
				long millisFromEpoch = Instant.parse(name).toEpochMilli();
				response = response.replace("$uptime",
						Utils.timeConversion((int) ((System.currentTimeMillis() - millisFromEpoch) / 1000)));
			} catch (JSONException e) {
				response = "Stream is not currently online!";
			}
		}
		if (response.contains("$following")) {
			if (streamer.equals(sender)) {
				response = "You can't follow yourself! Kappa";
			} else {
				String info = Utils
						.callURL("https://api.twitch.tv/kraken/users/" + sender + "/follows/channels/" + streamer);
				try {
					JSONObject json = new JSONObject(info);
					String name = json.getString("created_at");
					long millisFromEpoch = Instant.parse(name).toEpochMilli();
					long diff = ((System.currentTimeMillis() - millisFromEpoch) / 1000);
					response = response.replace("$following", Utils.timeConversionYears(diff));
				} catch (JSONException e) {
					response = "";
				}
			}
		}
		if (response.contains("$counter1")) {
			response = response.replace("$counter1", String.valueOf(counter1 + 1));
			updateValue("counter1", counter1);
		}
		if (response.contains("$counter2")) {
			response = response.replace("$counter2", String.valueOf(counter2 + 1));
			updateValue("counter2", counter2);
		}
		if (response.contains("$counter3")) {
			response = response.replace("$counter3", String.valueOf(counter3 + 1));
			updateValue("counter3", counter3);
		}
		if (response.contains("$counter4")) {
			response = response.replace("$counter4", String.valueOf(counter4 + 1));
			updateValue("counter4", counter4);
		}
		if (response.contains("$counter5")) {
			response = response.replace("$counter5", String.valueOf(counter5 + 1));
			updateValue("counter5", counter5);
		}
		if (response.contains("$roulette")) {
			response = "";
			new Thread(new Runnable() {
				public void run() {
					sendAction(channel, "places the revolver to " + sender + "'s head...");
					try {
						Thread.sleep(3000);
					} catch (InterruptedException e) {
						e.printStackTrace();
						Utils.errorReport(e);
					}
					if ((new Random().nextInt(7) + 1) == 6) {
						sendRawLine("PRIVMSG " + channel + " :" + "/timeout " + sender + " 1");
						sendRawLine(
								"PRIVMSG " + channel + " :" + "The gun fires and " + sender + " lies dead in chat.");
					} else {
						sendRawLine("PRIVMSG " + channel + " :" + "The trigger is pulled, and the revolver clicks. "
								+ sender + " has lived to survive roulette!");
					}
				}
			}).start();
		}
		return response;
	}

	public void updateValue(String counter, int val) {
		if (counter == "counter1") {
			counter1 = val + 1;
		} else if (counter == "counter2") {
			counter2 = val + 1;
		} else if (counter == "counter3") {
			counter3 = val + 1;
		} else if (counter == "counter4") {
			counter4 = val + 1;
		} else if (counter == "counter5") {
			counter5 = val + 1;
		}
	}

	public String ballResponse(String sender, String streamer, String message) {
		message = message.toLowerCase();
		if (message.contains("edude15000") || message.contains("edude")) {
			return "Edude15000 is too sexy for this question, " + sender;
		}
		String line = "";
		Random rand = new Random();
		int index = rand.nextInt(21) + 1;
		if (index == 1) {
			line = "It is certain, " + sender;
		} else if (index == 2) {
			line = "It is decidedly so, " + sender;
		} else if (index == 3) {
			line = "Without a doubt, " + sender;
		} else if (index == 4) {
			line = "Yes, definitely, " + sender;
		} else if (index == 5) {
			line = "You may rely on it, " + sender;
		} else if (index == 6) {
			line = "As I see it, yes, " + sender;
		} else if (index == 7) {
			line = "Most likely, " + sender;
		} else if (index == 8) {
			line = "Outlook good, " + sender;
		} else if (index == 9) {
			line = "Yes, " + sender;
		} else if (index == 10) {
			line = "Signs point to yes, " + sender;
		} else if (index == 11) {
			line = "Reply hazy try again, " + sender;
		} else if (index == 12) {
			line = "Ask again later, " + sender;
		} else if (index == 13) {
			line = "Better not tell you now, " + sender;
		} else if (index == 14) {
			line = streamer + " doesn't think so, " + sender;
		} else if (index == 15) {
			line = streamer + " definitely thinks so, " + sender;
		} else if (index == 16) {
			line = "Don't count on it, " + sender;
		} else if (index == 17) {
			line = "My reply is no, " + sender;
		} else if (index == 18) {
			line = "My sources say no, " + sender;
		} else if (index == 19) {
			line = "Outlook not so good, " + sender;
		} else if (index == 20) {
			line = "Very doubtful, " + sender;
		}
		return line;
	}

	public boolean checkIfUserIsHere(String user, String channel) {
		for (User userName : getUsers(channel)) {
			if (user.equals("(" + userName.getNick() + ")")) {
				return true;
			}
		}
		return false;
	}

	@Override
	protected void onJoin(String channel, String sender, String login, String hostname) {
		if (!containsUser(users, login)) {
			BotUser u = new BotUser(login, 0, false, false, false, 0, 0, null, 0, 0, 0);
			users.add(u);
		}
		if (events.containsKey(login)) {
			sendRawLine("PRIVMSG " + channel + " :"
					+ userVariables(events.get(login), channel, login, events.get(login), events.get(login), true));
		}
	}

	public static boolean containsUser(List<BotUser> list, String user) {
		for (BotUser object : list) {
			if (object.username.equalsIgnoreCase(user)) {
				return true;
			}
		}
		return false;
	}

	public BotUser getBotUser(String username) {
		for (BotUser botUser : users) {
			if (botUser.username.equalsIgnoreCase(username)) {
				return botUser;
			}
		}
		return null;
	}

}