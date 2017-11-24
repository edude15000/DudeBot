import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.io.Writer;
import java.text.DateFormat;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.NoSuchElementException;
import java.util.Random;
import java.util.TimeZone;

import com.google.api.services.youtube.model.Video;
import com.google.gson.annotations.Expose;

public class RequestSystem {
	TwitchBot bot;
	@Expose(serialize = true, deserialize = true)
	boolean vipSongToggle, mustFollowToRequest, requestsTrigger, displayIfUserIsHere, displayOneLine, whisperToUser,
			direquests, ylrequests, maxSongLength, doNotWriteToHistory = false;
	@Expose(serialize = true, deserialize = true)
	String subOnlyRequests, lastSong;
	@Expose(serialize = true, deserialize = true)
	int maxSonglistLength, numOfSongsToDisplay, numOfSongsInQueuePerUser, maxSongLengthInMinutes;
	@Expose(serialize = true, deserialize = true)
	String[] favSongs, bannedKeywords;
	@Expose(serialize = true, deserialize = true)
	ArrayList<String> favSongsPlayedThisStream = new ArrayList<String>();
	@Expose(serialize = true, deserialize = true)
	Command requestComm, songlistComm, getTotalComm, editComm, nextComm, addvipComm, addtopComm, adddonatorComm,
			getCurrentComm, clearComm, triggerRequestsComm, backupRequestAddComm, getNextComm, randomComm, favSongComm,
			editSongComm, removeSongComm, songPositionComm;

	public int getNumRequests(String sender) {
		for (BotUser user : bot.users) {
			if (user.username.equalsIgnoreCase(sender)) {
				return user.numRequests;
			}
		}
		return 0;
	}

	public void giveSpot(String message, String channel, String sender) throws FileNotFoundException, IOException {
		for (BotUser u : bot.users) {
			if (u.username.equalsIgnoreCase(sender) && !sender.equalsIgnoreCase(bot.streamer)) {
				if (u.gaveSpot) {
					bot.sendRawLine(
							"PRIVMSG " + channel + " :" + "You can only give another user your spot once per stream!");
					return;
				}
				try (BufferedReader br = new BufferedReader(new FileReader(Utils.songlistfile))) {
					String line, line2;
					int count = 1;
					boolean noSong = false;
					if (Integer.parseInt(getNumberOfSongs()) == 0) {
						bot.sendRawLine(
								"PRIVMSG " + channel + " :" + "You have no requests in the list, " + sender + "!");
						return;
					}
					String toWrite = "";
					while ((line = br.readLine()) != null) {
						if (line.contains(sender)) {
							if (line.startsWith("$$$")) {
								toWrite = "$$$\t";
							} else if (line.startsWith("VIP\t")) {
								toWrite = "VIP\t";
							}
							noSong = false;
							line2 = line;
							break;
						} else {
							noSong = true;
						}
						count++;
					}
					br.close();
					if (!noSong) {
						BufferedWriter writer = new BufferedWriter(new FileWriter(Utils.templistfile));
						BufferedReader reader = new BufferedReader(new FileReader(Utils.songlistfile));
						for (int i = 0; i < count - 1; i++) {
							writer.write(reader.readLine() + "\r");
						}
						String newUser = Utils.getFollowingText(message);
						if (newUser.contains("@")) {
							newUser = newUser.replace("@", "");
						}
						writer.write(toWrite + "Place Holder\t(" + newUser + ")\r");
						String previousSong = reader.readLine();
						while ((line2 = reader.readLine()) != null) {
							writer.write(line2 + "\r");
						}
						writer.close();
						reader.close();
						clear(channel, Utils.songlistfile);
						copyFile(Utils.templistfile, Utils.songlistfile);
						clear(channel, Utils.templistfile);
						if (previousSong.startsWith("$$$\t") || previousSong.startsWith("VIP\t")) {
							previousSong = previousSong.substring(previousSong.indexOf(' ') + 1);
						}
						bot.sendRawLine("PRIVMSG " + channel + " :" + "Your next request '" + previousSong
								+ "' has been changed to 'Place Holder' FOR " + newUser.toLowerCase() + "!");
						u.gaveSpot = true;
						return;
					} else {
						bot.sendRawLine("PRIVMSG " + channel + " :" + "You have no regular requests in the list, "
								+ sender + "!");
						return;
					}
				}
			}
		}
	}

	public void checkSongPositionCOMMAND(String message, String channel, String sender) {
		if (bot.checkUserLevel(sender, songPositionComm, channel)) {
			for (int i = 0; i < songPositionComm.input.length; i++) {
				String temp = message.toLowerCase();
				if (temp.equalsIgnoreCase(songPositionComm.input[i])) {
					try {
						String response = "";
						ArrayList<Integer> spots = checkPosition(message, channel, sender);
						if (spots.size() < 1) {
							bot.sendRawLine("PRIVMSG " + channel + " :"
									+ "You do not have any requests in the song list, " + sender + "!");
							return;
						}
						for (int j = 0; j < spots.size(); j++) {
							if (spots.get(j) == 0) {
								response += "You have a song playing right now, ";
							} else if (spots.get(j) == 1) {
								response += "You have a request next in line, ";
							} else {
								response += "You have a request in place # " + (spots.get(j) + 1) + ", ";
							}
						}
						bot.sendRawLine(
								"PRIVMSG " + channel + " :" + "Y" + response.toLowerCase().substring(1) + sender + "!");
					} catch (Exception e) {
						Utils.errorReport(e);
						e.printStackTrace();
					}
				}
			}
		}
	}

	public ArrayList<Integer> checkPosition(String message, String channel, String sender)
			throws FileNotFoundException, IOException {
		try (BufferedReader br = new BufferedReader(new FileReader(Utils.songlistfile))) {
			ArrayList<Integer> songs = new ArrayList<Integer>();
			String line;
			int count = 0;
			while ((line = br.readLine()) != null) {
				if (line.contains("(" + sender + ")")) {
					songs.add(count);
				}
				count++;
			}
			return songs;
		}
	}

	public void addDonatorCOMMAND(String message, String channel, String sender) throws InterruptedException {
		if (bot.checkUserLevel(sender, adddonatorComm, channel)) {
			for (int i = 0; i < adddonatorComm.input.length; i++) {
				String temp = message.toLowerCase();
				if (temp.startsWith(adddonatorComm.input[i]) && temp.contains(adddonatorComm.input[i] + " ")) {
					try {
						String input = Utils.getFollowingText(message);
						String youtubeID = null;
						Video ytvid = null;
						if (message.contains("www.") || message.contains("http://") || message.contains("http://www.")
								|| message.contains(".com") || message.contains("https://")) {
							if (message.contains("www.youtube.com/watch?v=")
									|| message.contains("www.youtube.com/watch?v=")) {
								youtubeID = message.substring(message.indexOf("=") + 1);
								try {
									ytvid = bot.youtube.searchYoutubeByID(youtubeID);
									if (ytvid == null) {
										bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid youtube URL, " + sender);
										return;
									}
								} catch (NoSuchElementException e) {
									bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid youtube URL, " + sender);
									return;
								}
							} else if (message.contains("https://youtu.be/")) {
								youtubeID = message.substring(message.lastIndexOf("/") + 1);
								try {
									ytvid = bot.youtube.searchYoutubeByID(youtubeID);
									if (ytvid == null) {
										bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid youtube URL, " + sender);
										return;
									}
								} catch (NoSuchElementException e) {
									bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid youtube URL, " + sender);
									return;
								}
							} else {
								bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid Request, " + sender);
								return;
							}
						}
						if (input.endsWith(")")) {
							String requester = input.substring(input.lastIndexOf("(") + 1, input.length() - 1).trim();
							input = input.substring(0, input.lastIndexOf("(")).trim();
							if (ytvid != null) {
								addDonator(channel, ytvid.getSnippet().getTitle(), requester);
								bot.sendRawLine(
										"PRIVMSG " + channel + " :" + "Donator Song '" + ytvid.getSnippet().getTitle()
												+ "' has been added to the song list, " + requester + "!");
							} else {
								addDonator(channel, input, requester);
								bot.sendRawLine("PRIVMSG " + channel + " :" + "Donator Song '" + input
										+ "' has been added to the song list, " + requester + "!");
							}
							bot.addUserRequestAmount(requester, true);
							writeToCurrentSong(channel, false);
						} else {
							if (ytvid != null) {
								addDonator(channel, ytvid.getSnippet().getTitle(), sender);
								bot.sendRawLine(
										"PRIVMSG " + channel + " :" + "Donator Song '" + ytvid.getSnippet().getTitle()
												+ "' has been added to the song list, " + sender + "!");
							} else {
								addDonator(channel, input, sender);
								bot.sendRawLine("PRIVMSG " + channel + " :" + "Donator Song '" + input
										+ "' has been added to the song list, " + sender + "!");
							}
							writeToCurrentSong(channel, false);
						}
					} catch (IOException e) {
						Utils.errorReport(e);
						e.printStackTrace();
					}
				}
			}
		}
	}

	public void addDonator(String channel, String song, String requestedby) throws IOException {
		if (Integer.parseInt(getNumberOfSongs()) == 0) {
			Writer output;
			output = new BufferedWriter(new FileWriter(Utils.songlistfile, true));
			output.append("$$$\t" + song + "\t(" + requestedby + ")\r");
			output.close();
		} else if (Integer.parseInt(getNumberOfSongs()) == 1) {
			try (BufferedReader br2 = new BufferedReader(new FileReader(Utils.templistfile))) {
				copyToTemp(channel);
				String line2 = br2.readLine();
				BufferedWriter writer = new BufferedWriter(new FileWriter(Utils.songlistfile));
				if (line2.startsWith("VIP\t")) {
					line2 = line2.replace("VIP", "$$$");
					writer.write(line2 + "\r");
				} else if (line2.startsWith("$$$\t")) {
					writer.write(line2 + "\r");
				} else {
					writer.write("$$$\t" + line2 + "\r");
				}
				br2.readLine();
				writer.write("$$$\t" + song + "\t(" + requestedby + ")\r");
				clear(channel, Utils.templistfile);
				br2.close();
				writer.close();
			}
		} else {
			try (BufferedReader br3 = new BufferedReader(new FileReader(Utils.templistfile))) {
				copyToTemp(channel);
				String line2, line3, line4;
				line4 = br3.readLine();
				br3.close();
				if (line4.startsWith("$$$\t")) {
					BufferedReader br = new BufferedReader(new FileReader(Utils.templistfile));
					BufferedReader br2 = new BufferedReader(new FileReader(Utils.templistfile));
					BufferedWriter writer = new BufferedWriter(new FileWriter(Utils.songlistfile));
					int count = 0;
					while ((line2 = br.readLine()) != null) {
						if (line2.contains("$$$\t")) {
							writer.write(line2 + "\r");
							count++;
						}
					}
					writer.write("$$$\t" + song + "\t(" + requestedby + ")\r");
					for (int i = 0; i < count; i++) {
						br2.readLine();
					}
					while ((line3 = br2.readLine()) != null) {
						writer.write(line3 + "\r");
					}
					clear(channel, Utils.templistfile);
					br.close();
					br2.close();
					writer.close();
				} else {
					BufferedReader br = new BufferedReader(new FileReader(Utils.templistfile));
					BufferedWriter writer = new BufferedWriter(new FileWriter(Utils.songlistfile));
					line2 = br.readLine();
					if (line2.contains("VIP\t")) {
						writer.write(line2.replace("VIP", "$$$") + "\r");
					} else {
						writer.write("$$$\t" + line2 + "\r");
					}
					writer.write("$$$\t" + song + "\t(" + requestedby + ")\r");
					while ((line2 = br.readLine()) != null) {
						writer.write(line2 + "\r");
					}
					clear(channel, Utils.templistfile);
					br.close();
					writer.close();
				}
			}
		}
	}

	public void removeMySong(String message, String channel, String sender) throws FileNotFoundException, IOException {
		if (bot.checkUserLevel(sender, removeSongComm, channel)) {
			for (int i = 0; i < removeSongComm.input.length; i++) {
				String temp = message.toLowerCase();
				if (temp.equalsIgnoreCase(removeSongComm.input[i])) {
					removeRequesterSong(message, channel, sender);
				}
			}
		}
	}

	public void removeRequesterSong(String message, String channel, String sender)
			throws FileNotFoundException, IOException {
		try (BufferedReader br = new BufferedReader(new FileReader(Utils.songlistfile))) {
			String line, line2;
			int count = 1;
			boolean noSong = false;
			if (Integer.parseInt(getNumberOfSongs()) == 0) {
				bot.sendRawLine(
						"PRIVMSG " + channel + " :" + "You have no regular requests in the list, " + sender + "!");
				return;
			}
			while ((line = br.readLine()) != null) {
				if (line.contains("(" + sender + ")")) {
					noSong = false;
					line2 = line;
					break;
				} else {
					noSong = true;
				}
				count++;
			}
			br.close();
			if (!noSong) {
				BufferedWriter writer = new BufferedWriter(new FileWriter(Utils.templistfile));
				BufferedReader reader = new BufferedReader(new FileReader(Utils.songlistfile));
				for (int i = 0; i < count - 1; i++) {
					writer.write(reader.readLine() + "\r");
				}
				String songToDelete = reader.readLine();
				while ((line2 = reader.readLine()) != null) {
					writer.write(line2 + "\r");
				}
				writer.close();
				reader.close();
				clear(channel, Utils.songlistfile);
				copyFile(Utils.templistfile, Utils.songlistfile);
				clear(channel, Utils.templistfile);
				bot.sendRawLine("PRIVMSG " + channel + " :" + "Your next request '"
						+ songToDelete.substring(0, songToDelete.lastIndexOf("\t")) + "' has been removed, " + sender
						+ "!");
				bot.addUserRequestAmount(sender, false);
			} else {
				bot.sendRawLine("PRIVMSG " + channel + " :" + "You have no requests in the list, " + sender + "!");
			}
		}
	}

	public void editMySongCOMMAND(String message, String channel, String sender)
			throws FileNotFoundException, IOException {
		if (bot.checkUserLevel(sender, editSongComm, channel)) {
			for (int i = 0; i < editSongComm.input.length; i++) {
				String temp = message.toLowerCase();
				if (temp.equalsIgnoreCase(editSongComm.input[i])) {
					bot.sendRawLine("PRIVMSG " + channel + " :"
							+ "Please type an artist and song name after the command, " + sender);
				} else if (temp.startsWith(editSongComm.input[i]) && temp.contains(editSongComm.input[i] + " ")) {
					if (bannedKeywords != null) {
						for (int j = 0; j < bannedKeywords.length; j++) {
							if (temp.toLowerCase().contains(bannedKeywords[j].toLowerCase())) {
								bot.sendRawLine("PRIVMSG " + channel + " :" + "Song request contains a banned keyword '"
										+ bannedKeywords[j] + "' and cannot be added, " + sender + "!");
								return;
							}
						}
					}
					editRequesterSong(message, channel, sender);
				}
			}
		}
	}

	public void editRequesterSong(String message, String channel, String sender)
			throws FileNotFoundException, IOException {
		try (BufferedReader br = new BufferedReader(new FileReader(Utils.songlistfile))) {
			String line, line2;
			int count = 1;
			boolean noSong = false;
			boolean writeVIP = false, write$$$ = false;
			if (Integer.parseInt(getNumberOfSongs()) == 0) {
				bot.sendRawLine("PRIVMSG " + channel + " :" + "You have no requests in the list, " + sender + "!");
				return;
			}
			while ((line = br.readLine()) != null) {
				if (line.contains(sender)) {
					if (count == 1) {
						if (!Utils.checkIfUserIsOP(sender, channel, bot.streamer, bot.users)
								&& !sender.equalsIgnoreCase(bot.streamer) && !sender.equalsIgnoreCase(Utils.botMaker)) {
							bot.sendRawLine("PRIVMSG " + channel + " :"
									+ "Your song is currently playing. Please have a mod edit it, " + sender + "!");
							return;
						}
					}
					if (line.contains("VIP\t")) {
						writeVIP = true;
					}
					if (line.contains("$$$\t")) {
						write$$$ = true;
					}
					noSong = false;
					line2 = line;
					break;
				} else {
					noSong = true;
				}
				count++;
			}
			br.close();
			if (!noSong) {
				BufferedWriter writer = new BufferedWriter(new FileWriter(Utils.templistfile));
				BufferedReader reader = new BufferedReader(new FileReader(Utils.songlistfile));
				for (int i = 0; i < count - 1; i++) {
					writer.write(reader.readLine() + "\r");
				}
				if (write$$$) {
					writer.write("$$$\t" + Utils.getFollowingText(message) + "\t(" + sender + ")\r");
				} else if (writeVIP) {
					writer.write("VIP\t" + Utils.getFollowingText(message) + "\t(" + sender + ")\r");
				} else {
					writer.write(Utils.getFollowingText(message) + "\t(" + sender + ")\r");
				}
				String previousSong = reader.readLine();
				while ((line2 = reader.readLine()) != null) {
					writer.write(line2 + "\r");
				}
				writer.close();
				reader.close();
				clear(channel, Utils.songlistfile);
				copyFile(Utils.templistfile, Utils.songlistfile);
				clear(channel, Utils.templistfile);
				if (previousSong.startsWith("VIP\t")) {
					previousSong = previousSong.replace("VIP\t", "");
				}
				if (previousSong.startsWith("$$$\t")) {
					previousSong = previousSong.replace("$$$\t", "");
				}
				bot.sendRawLine("PRIVMSG " + channel + " :" + "Your next request '"
						+ previousSong.substring(0, previousSong.lastIndexOf("\t")) + "' has been changed to '"
						+ Utils.getFollowingText(message) + "', " + sender + "!");
				writeToCurrentSong(channel, false);
			} else {
				bot.sendRawLine("PRIVMSG " + channel + " :" + "You have no requests in the list, " + sender + "!");
			}
		}
	}

	public void chooseRandomFavorite(String message, String channel, String sender)
			throws NumberFormatException, FileNotFoundException, IOException {
		if (bot.checkUserLevel(sender, favSongComm, channel)) {
			for (int i = 0; i < favSongComm.input.length; i++) {
				if (message.equalsIgnoreCase(favSongComm.input[i])) {
					if (Integer.parseInt(getNumberOfSongs()) < maxSonglistLength) {
						if (requestsTrigger) {
							try {
								boolean check = true;
								if (!checkIfUserAlreadyHasSong(sender)) {
									if (mustFollowToRequest) {
										if (!Utils.checkIfUserIsFollowing(channel, sender, bot.streamer, bot.users)) {
											check = false;
										}
									}
									if (check) {
										if (favSongs != null) {
											try {
												Random rand = new Random();
												int index = rand.nextInt(favSongs.length);
												if (favSongsPlayedThisStream.contains(favSongs[index])) {
													addSong(channel, "streamer's Choice", sender);
												} else {
													addSong(channel, favSongs[index], sender);
												}
												favSongsPlayedThisStream.add(favSongs[index]);
											} catch (IOException e) {
												Utils.errorReport(e);
												e.printStackTrace();
											}
										} else {
											addSong(channel, "streamer's Choice", sender);
										}
									} else {
										bot.sendRawLine("PRIVMSG " + channel + " :"
												+ "You must follow the stream to request a song, " + sender);
									}
								} else {
									if (numOfSongsInQueuePerUser == 1) {
										bot.sendRawLine("PRIVMSG " + channel + " :"
												+ "You may only have 1 song in the queue at a time, " + sender + "!");
									} else {
										bot.sendRawLine("PRIVMSG " + channel + " :" + "You may only have "
												+ numOfSongsInQueuePerUser + " songs in the queue at a time, " + sender
												+ "!");
									}
								}
							} catch (IOException e1) {
								Utils.errorReport(e1);
								e1.printStackTrace();
							}
						} else {
							bot.sendRawLine("PRIVMSG " + channel + " :" + "Requests are currently off!");
						}
					} else {
						bot.sendRawLine("PRIVMSG " + channel + " :" + "Song limit of " + maxSonglistLength
								+ " has been reached, please try again later.");
					}
				}
			}
		}
	}

	public void getNextSongCOMMAND(String message, String channel, String sender) throws InterruptedException {
		if (bot.checkUserLevel(sender, getNextComm, channel)) {
			for (int i = 0; i < getNextComm.input.length; i++) {
				if (message.equalsIgnoreCase(getNextComm.input[i])) {
					try {
						bot.sendRawLine("PRIVMSG " + channel + " :" + getNextSongTitle(channel));
					} catch (NumberFormatException | IOException e) {
						try {
							Thread.sleep(1000);
							bot.sendRawLine("PRIVMSG " + channel + " :" + getNextSongTitle(channel));
						} catch (NumberFormatException | IOException e1) {
							Utils.errorReport(e1);
							e1.printStackTrace();
						}
					}
				}
			}
		}
	}

	public String getNextSongTitle(String channel) throws NumberFormatException, FileNotFoundException, IOException {
		if (Integer.parseInt(getNumberOfSongs()) < 2) {
			return "There is no next song in the song list!";
		} else {
			String line = "";
			try (BufferedReader br = new BufferedReader(new FileReader(Utils.songlistfile))) {
				br.readLine();
				if ((line = br.readLine()) != null) {
					if (line.contains("VIP\t") || line.contains("$$$\t")) {
						line = line.substring(line.indexOf("\t") + 1, line.length());
					} else {
						line = line.substring(0, line.length());
					}
				}
			}
			return "Next up: " + line;
		}
	}

	public void triggerRequests(boolean trigger, String channel) throws FileNotFoundException, IOException {
		if (trigger) {
			requestsTrigger = true;
			bot.sendRawLine("PRIVMSG " + channel + " :" + "Requests turned on!");
		} else {
			requestsTrigger = false;
			bot.sendRawLine("PRIVMSG " + channel + " :" + "Requests turned off!");
		}
	}

	public void randomizer(String channel) throws NumberFormatException, FileNotFoundException, IOException {
		String line, line2, line3 = "";
		boolean writeVIP = false;
		if (Integer.parseInt(getNumberOfSongs()) > 2) {
			Random rand = new Random();
			int randInt = rand.nextInt(Integer.parseInt(getNumberOfSongs()) - 1) + 1;
			try (BufferedReader br = new BufferedReader(new FileReader(Utils.songlistfile))) {
				for (int i = 0; i < randInt; i++) {
					line = br.readLine();
					if (i == 0) {
						line3 = line.substring(line.indexOf("("));
					}
					if (i == 1 && ((line.startsWith("VIP\t")) || (line.startsWith("$$$\t")))) {
						writeVIP = true;
					}
				}
				line = br.readLine();
				br.close();
				if (line.contains(line3)) {
					writeVIP = false;
					randInt = rand.nextInt(Integer.parseInt(getNumberOfSongs()) - 1) + 1;
					BufferedReader secondReader = new BufferedReader(new FileReader(Utils.songlistfile));
					for (int i = 0; i < randInt; i++) {
						line = secondReader.readLine();
						if (i == 1 && ((line.startsWith("VIP\t")) || (line.startsWith("$$$\t")))) {
							writeVIP = true;
						}
					}
					line = secondReader.readLine();
					secondReader.close();
				}
				copyToTemp(channel);
				BufferedReader br2 = new BufferedReader(new FileReader(Utils.templistfile));
				BufferedWriter writer = new BufferedWriter(new FileWriter(Utils.songlistfile));
				if (writeVIP == true) {
					if (line.startsWith("VIP\t") || line.startsWith("$$$\t")) {
						if (line.startsWith("$$$\t")) {
							writer.write(line + "\r");
						} else {
							String line4 = line.replace("VIP", "$$$");
							writer.write(line4 + "\r");
						}
					} else {
						writer.write("$$$\t" + line + "\r");
					}
				} else {
					if (line.startsWith("$$$\t")) {
						writer.write(line + "\r");
					} else {
						writer.write("$$$\t" + line + "\r");
					}
				}
				br2.readLine();
				while ((line2 = br2.readLine()) != null) {
					if (!line2.equals(line)) {
						writer.write(line2 + "\r");
					} else {
						break;
					}
				}
				while ((line2 = br2.readLine()) != null) {
					writer.write(line2 + "\r");
				}
				writer.close();
				br2.close();
				clear(channel, Utils.templistfile);
				bot.sendRawLine("PRIVMSG " + channel + " :" + getNextSong(channel));
			}
		} else {
			bot.sendRawLine(
					"PRIVMSG " + channel + " :" + "Song list must have 3 or more songs to choose a random one!");
		}
	}

	public void randomizerCommand(String message, String channel, String sender)
			throws NumberFormatException, FileNotFoundException, IOException {
		if (bot.checkUserLevel(sender, randomComm, channel)) {
			for (int i = 0; i < randomComm.input.length; i++) {
				if (message.equalsIgnoreCase(randomComm.input[i])) {
					try {
						randomizer(channel);
						writeToCurrentSong(channel, false);
					} catch (NumberFormatException | IOException e) {
						try {
							Thread.sleep(1000);
							randomizer(channel);
							writeToCurrentSong(channel, false);
						} catch (InterruptedException e1) {
							Utils.errorReport(e1);
							e1.printStackTrace();
						}
					}
				}
			}
		}
	}

	public void writeToCurrentSong(String channel, Boolean nextCom) throws IOException {
		Writer output;
		output = new BufferedWriter(new FileWriter(Utils.currentSongFile, false));
		String line = getCurrentSongTitle(channel);
		if (line.startsWith("VIP\t") || line.startsWith("$$$\t")) {
			if (line.contains("\t")) {
				line = line.substring(line.indexOf("\t") + 1, line.lastIndexOf("\t"));
			}
		} else {
			if (line.contains("\t")) {
				line = line.substring(0, line.indexOf("\t"));
			}
		}
		output.write(line);
		output.close();

		Writer output2;
		output2 = new BufferedWriter(new FileWriter(Utils.currentRequesterFile, false));
		String line2 = getCurrentSongTitle(channel);
		if (line2.contains("\t")) {
			line2 = line2.substring(line2.lastIndexOf("\t") + 2, line2.length() - 1);
		}
		if (line2.equals("Song list is empty")) {
			line2 = "";
		}
		output2.write(line2);
		output2.close();
		if (bot.spreadsheetId != null) {
			bot.google.writeToGoogleSheets(nextCom, Utils.songlistfile, Utils.lastPlayedSongsFile);
		}
	}

	public void copyToTemp(String channel) throws IOException {
		FileInputStream instream = null;
		FileOutputStream outstream = null;
		try {
			File infile = new File(Utils.songlistfile);
			File outfile = new File(Utils.templistfile);
			instream = new FileInputStream(infile);
			outstream = new FileOutputStream(outfile);
			byte[] buffer = new byte[1024];
			int length;
			while ((length = instream.read(buffer)) > 0) {
				outstream.write(buffer, 0, length);
			}
			instream.close();
			outstream.close();
		} catch (Exception ioe) {
			Utils.errorReport(ioe);
			ioe.printStackTrace();
		}
		clear(channel, Utils.songlistfile);
	}

	public void clear(String channel, String file) throws IOException {
		Writer output;
		output = new BufferedWriter(new FileWriter(file));
		output.append("");
		output.close();
		writeToCurrentSong(channel, false);
	}

	public void copyFile(String f1, String f2) throws IOException {
		FileInputStream instream = null;
		FileOutputStream outstream = null;
		try {
			File infile = new File(f1);
			File outfile = new File(f2);
			instream = new FileInputStream(infile);
			outstream = new FileOutputStream(outfile);
			byte[] buffer = new byte[1024];
			int length;
			while ((length = instream.read(buffer)) > 0) {
				outstream.write(buffer, 0, length);
			}
			instream.close();
			outstream.close();
		} catch (IOException ioe) {
			Utils.errorReport(ioe);
			ioe.printStackTrace();
		}
	}

	public void appendToLastSongs(String channel, String lastSong) {
		try {
			Writer output;
			output = new BufferedWriter(new FileWriter(Utils.lastPlayedSongsFile, true));
			Date date = new Date();
			DateFormat df = new SimpleDateFormat("HH:mm:ss");
			df.setTimeZone(TimeZone.getDefault());
			output.write(Utils.getDate() + " " + df.format(date) + " - " + lastSong + "\r");
			output.close();
		} catch (Exception e) {
			Utils.errorReport(e);
			e.printStackTrace();
		}
	}

	public void getCurrentSongCOMMAND(String message, String channel, String sender) throws InterruptedException {
		if (bot.checkUserLevel(sender, getCurrentComm, channel)) {
			for (int i = 0; i < getCurrentComm.input.length; i++) {
				if (message.equalsIgnoreCase(getCurrentComm.input[i])) {
					try {
						bot.sendRawLine("PRIVMSG " + channel + " :" + "Playing: " + getCurrentSongTitle(channel));
					} catch (NumberFormatException | IOException e) {
						try {
							Thread.sleep(1000);
							bot.sendRawLine("PRIVMSG " + channel + " :" + "Playing: " + getCurrentSongTitle(channel));
						} catch (NumberFormatException | IOException e1) {
							Utils.errorReport(e1);
							e1.printStackTrace();
						}
					}
				}
			}
		}
	}

	public void triggerRequestsCOMMAND(String message, String channel, String sender)
			throws FileNotFoundException, IOException {
		if (bot.checkUserLevel(sender, triggerRequestsComm, channel)) {
			for (int i = 0; i < triggerRequestsComm.input.length; i++) {
				String temp = message.toLowerCase();
				if (temp.startsWith(triggerRequestsComm.input[i])
						&& temp.contains(triggerRequestsComm.input[i] + " ")) {
					if (Utils.getFollowingText(message).contains("on")) {
						triggerRequests(true, channel);
					} else if (Utils.getFollowingText(message).contains("off")) {
						triggerRequests(false, channel);
					}
				}
			}
		}
	}

	public void clearCOMMAND(String message, String channel, String sender) throws IOException {
		if (bot.checkUserLevel(sender, clearComm, channel)) {
			for (int i = 0; i < clearComm.input.length; i++) {
				String temp = message.toLowerCase();
				if (temp.equals(clearComm.input[i])) {
					try {
						clear(channel, Utils.songlistfile);
						bot.sendRawLine("PRIVMSG " + channel + " :" + "Song list has been cleared!");
						writeToCurrentSong(channel, false);
					} catch (IOException e) {
						Utils.errorReport(e);
						e.printStackTrace();
					}
				}
			}
		}
	}

	public void decrementUsersOnClear() {
		String line;
		int i = 0;
		String user = "";
		try (BufferedReader br = new BufferedReader(new FileReader(Utils.songlistfile))) {
			while ((line = br.readLine()) != null) {
				if (i != 0) {
					user = line.substring(line.lastIndexOf('\t')).trim();
					user = user.replace(")", "");
					user = user.replace("(", "");
					bot.addUserRequestAmount(user, false);
				}
				i++;
			}
			bot.read();
			br.close();
		} catch (IOException e) {
			Utils.errorReport(e);
			e.printStackTrace();
		}
	}

	public void nextCOMMAND(String message, String channel, String sender)
			throws NumberFormatException, FileNotFoundException, IOException {
		if (bot.checkUserLevel(sender, nextComm, channel)) {
			for (int i = 0; i < nextComm.input.length; i++) {
				String temp = message.toLowerCase();
				if (temp.equals(nextComm.input[i])) {
					try {
						if (nextSong(channel)) {
							bot.sendRawLine("PRIVMSG " + channel + " :" + getNextSong(channel));
						} else {
							bot.sendRawLine("PRIVMSG " + channel + " :" + "There are no songs currently in the queue!");
						}
						writeToCurrentSong(channel, true);
					} catch (Exception e) {
						Utils.errorReport(e);
						e.printStackTrace();
					}
				}
			}
		}
	}

	public void nextRegular(String message, String channel, String sender)
			throws NumberFormatException, FileNotFoundException, IOException, InterruptedException {
		nextRegularCOMMAND(message, channel, sender);
		writeToCurrentSong(channel, true);
	}

	public void triggerVIPs(boolean trigger, String channel) throws FileNotFoundException, IOException {
		if (trigger) {
			vipSongToggle = true;
			bot.sendRawLine("PRIVMSG " + channel + " :" + "VIP songs turned on!");
		} else {
			vipSongToggle = false;
			bot.sendRawLine("PRIVMSG " + channel + " :" + "VIP songs turned off!");
		}
	}

	public void nextRegularCOMMAND(String message, String channel, String sender)
			throws NumberFormatException, FileNotFoundException, IOException {
		if ((Integer.parseInt(getNumberOfSongs()) == 0)) {
			bot.sendRawLine("PRIVMSG " + channel + " :" + "There are no songs in the list!");
			return;
		} else {
			copyToTemp(channel);
			BufferedReader br = new BufferedReader(new FileReader(Utils.templistfile));
			br.readLine();
			String line, song = "";
			Boolean check = false;
			while ((line = br.readLine()) != null) {
				if (!line.startsWith("$$$\t") && !line.startsWith("VIP\t")) {
					song = line;
					check = true;
					break;
				}
			}
			br.close();
			if (check) {
				clear(channel, Utils.songlistfile);
				BufferedWriter writer = new BufferedWriter(new FileWriter(Utils.songlistfile));
				writer.write("$$$\t" + line + "\r");
				BufferedReader br2 = new BufferedReader(new FileReader(Utils.templistfile));
				br2.readLine();
				while ((line = br2.readLine()) != null) {
					if (!line.equals(song)) {
						writer.write(line + "\r");
					}
				}
				writer.close();
				br2.close();
				bot.sendRawLine("PRIVMSG " + channel + " :" + getNextSong(channel));
				return;
			} else {
				copyFile(Utils.templistfile, Utils.songlistfile);
				nextSongAuto(channel, true);
				clear(channel, Utils.templistfile);
				return;
			}
		}
	}

	public void nextSongAuto(String channel, Boolean check)
			throws NumberFormatException, FileNotFoundException, IOException {
		try {
			if (nextSong(channel)) {
				if (check) {
					bot.sendRawLine("PRIVMSG " + channel + " :" + "There are no standard requests in the list. "
							+ getNextSong(channel));
				} else {
					bot.sendRawLine("PRIVMSG " + channel + " :" + getNextSong(channel));
				}
			} else {
				bot.sendRawLine("PRIVMSG " + channel + " :" + "There are no songs currently in the queue!");
			}
			writeToCurrentSong(channel, true);
		} catch (NumberFormatException | IOException e) {
			Utils.errorReport(e);
			e.printStackTrace();
		}
	}

	public void editCOMMAND(String message, String channel, String sender) throws InterruptedException {
		if (bot.checkUserLevel(sender, editComm, channel)) {
			for (int i = 0; i < editComm.input.length; i++) {
				String temp = message.toLowerCase();
				if (temp.equalsIgnoreCase(editComm.input[i])) {
					bot.sendRawLine("PRIVMSG " + channel + " :"
							+ "Please type an artist and song name after the command, " + sender);
				} else if (temp.startsWith(editComm.input[i]) && temp.contains(editComm.input[i] + " ")) {
					try {
						if (editCurrent(channel, Utils.getFollowingText(message), sender)) {
							bot.sendRawLine("PRIVMSG " + channel + " :" + "Current song has been edited!");
							writeToCurrentSong(channel, false);
						}
					} catch (IOException e) {
						Utils.errorReport(e);
						e.printStackTrace();
					}
				}
			}
		}
	}

	public void addvipCOMMAND(String message, String channel, String sender) throws InterruptedException {
		if (bot.checkUserLevel(sender, addvipComm, channel)) {
			for (int i = 0; i < addvipComm.input.length; i++) {
				String temp = message.toLowerCase();
				if (temp.startsWith(addvipComm.input[i]) && temp.contains(addvipComm.input[i] + " ")) {
					try {
						String input = Utils.getFollowingText(message);
						String youtubeID = null;
						Video ytvid = null;
						if (message.contains("www.") || message.contains("http://") || message.contains("http://www.")
								|| message.contains(".com") || message.contains("https://")) {
							if (message.contains("www.youtube.com/watch?v=")
									|| message.contains("www.youtube.com/watch?v=")) {
								youtubeID = message.substring(message.indexOf("=") + 1);
								try {
									ytvid = bot.youtube.searchYoutubeByID(youtubeID);
									if (ytvid == null) {
										bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid youtube URL, " + sender);
										return;
									}
								} catch (NoSuchElementException e) {
									bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid youtube URL, " + sender);
									return;
								}
							} else if (message.contains("https://youtu.be/")) {
								youtubeID = message.substring(message.lastIndexOf("/") + 1);
								try {
									ytvid = bot.youtube.searchYoutubeByID(youtubeID);
									if (ytvid == null) {
										bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid youtube URL, " + sender);
										return;
									}
								} catch (NoSuchElementException e) {
									bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid youtube URL, " + sender);
									return;
								}
							} else {
								bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid Request, " + sender);
								return;
							}
						}
						if (input.endsWith(")")) {
							String requester = input.substring(input.lastIndexOf("(") + 1, input.length() - 1).trim();
							input = input.substring(0, input.lastIndexOf("(")).trim();
							if (ytvid != null) {
								addVip(channel, ytvid.getSnippet().getTitle(), requester);
							} else {
								addVip(channel, input, requester);
							}
							bot.sendRawLine("PRIVMSG " + channel + " :" + "VIP Song '" + input
									+ "' has been added to the song list, " + requester + "!");
							bot.addUserRequestAmount(requester, true);
							writeToCurrentSong(channel, false);
						} else {
							if (ytvid != null) {
								addVip(channel, ytvid.getSnippet().getTitle(), sender);
								bot.sendRawLine(
										"PRIVMSG " + channel + " :" + "VIP Song '" + ytvid.getSnippet().getTitle()
												+ "' has been added to the song list, " + sender + "!");
							} else {
								addVip(channel, input, sender);
								bot.sendRawLine("PRIVMSG " + channel + " :" + "VIP Song '" + input
										+ "' has been added to the song list, " + sender + "!");
							}
							writeToCurrentSong(channel, false);
						}
					} catch (IOException e) {
						Utils.errorReport(e);
						e.printStackTrace();
					}
				}
			}
		}
	}

	public void addtopCOMMAND(String message, String channel, String sender) throws InterruptedException {
		if (bot.checkUserLevel(sender, addtopComm, channel)) {
			for (int i = 0; i < addtopComm.input.length; i++) {
				String temp = message.toLowerCase();
				if (temp.startsWith(addtopComm.input[i]) && temp.contains(addtopComm.input[i] + " ")) {
					try {
						String input = Utils.getFollowingText(message);
						String youtubeID = null;
						Video ytvid = null;
						if (message.contains("www.") || message.contains("http://") || message.contains("http://www.")
								|| message.contains(".com") || message.contains("https://")) {
							if (message.contains("www.youtube.com/watch?v=")
									|| message.contains("www.youtube.com/watch?v=")) {
								youtubeID = message.substring(message.indexOf("=") + 1);
								try {
									ytvid = bot.youtube.searchYoutubeByID(youtubeID);
									if (ytvid == null) {
										bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid youtube URL, " + sender);
										return;
									}
								} catch (NoSuchElementException e) {
									bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid youtube URL, " + sender);
									return;
								}
							} else if (message.contains("https://youtu.be/")) {
								youtubeID = message.substring(message.lastIndexOf("/") + 1);
								try {
									ytvid = bot.youtube.searchYoutubeByID(youtubeID);
									if (ytvid == null) {
										bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid youtube URL, " + sender);
										return;
									}
								} catch (NoSuchElementException e) {
									bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid youtube URL, " + sender);
									return;
								}
							} else {
								bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid Request, " + sender);
								return;
							}
						}
						if (input.endsWith(")")) {
							String requester = input.substring(input.lastIndexOf("(") + 1, input.length() - 1).trim();
							input = input.substring(0, input.lastIndexOf("(")).trim();
							if (ytvid != null) {
								addTop(channel, ytvid.getSnippet().getTitle(), requester);
								bot.sendRawLine("PRIVMSG " + channel + " :" + "Song '" + ytvid.getSnippet().getTitle()
										+ "' has been added to the top of the song list, " + requester + "!");
							} else {
								addTop(channel, input, requester);
								bot.sendRawLine("PRIVMSG " + channel + " :" + "Song '" + input
										+ "' has been added to the top of the song list, " + requester + "!");
							}
							bot.addUserRequestAmount(requester, true);
							writeToCurrentSong(channel, false);
						} else {
							if (ytvid != null) {
								addTop(channel, ytvid.getSnippet().getTitle(), sender);
								bot.sendRawLine("PRIVMSG " + channel + " :" + "Song '" + ytvid.getSnippet().getTitle()
										+ "' has been added to the top of the song list, " + sender + "!");
							} else {
								addTop(channel, input, sender);
								bot.sendRawLine("PRIVMSG " + channel + " :" + "Song '" + input
										+ "' has been added to the top of the song list, " + sender + "!");
							}
							writeToCurrentSong(channel, false);
						}
					} catch (IOException e) {
						Utils.errorReport(e);
						e.printStackTrace();
					}
				}
			}
		}
	}

	public void getTotalSongCOMMAND(String message, String channel, String sender) throws InterruptedException {
		if (bot.checkUserLevel(sender, getTotalComm, channel)) {
			for (int i = 0; i < getTotalComm.input.length; i++) {
				String temp = message.toLowerCase();
				if (temp.equals(getTotalComm.input[i])) {
					try {
						bot.sendRawLine("PRIVMSG " + channel + " :" + "The total number of songs in the queue is: "
								+ getNumberOfSongs());
					} catch (IOException e) {
						Utils.errorReport(e);
						e.printStackTrace();
					}
				}
			}
		}
	}

	public void songlistCOMMAND(String message, String channel, String sender) throws InterruptedException {
		if (bot.checkUserLevel(sender, songlistComm, channel)) {
			for (int i = 0; i < songlistComm.input.length; i++) {
				String temp = message.toLowerCase();
				if (temp.equals(songlistComm.input[i])) {
					try {
						if (Integer.parseInt(getNumberOfSongs()) == 0) {
							bot.sendRawLine("PRIVMSG " + channel + " :" + "The song list is empty!");
						} else if (numOfSongsToDisplay > Integer.parseInt(getNumberOfSongs())) {
							if (Integer.parseInt(getNumberOfSongs()) == 1) {
								String text = "The next song in the song list: ";
								songlist(channel, text);
							} else {
								String text = "The next " + Integer.parseInt(getNumberOfSongs())
										+ " songs in the song list: ";
								songlist(channel, text);
							}
						} else {
							String text = "The next " + numOfSongsToDisplay + " songs in the song list: ";
							songlist(channel, text);
						}
						if (bot.spreadsheetId != null) {
							bot.sendRawLine("PRIVMSG " + channel + " :"
									+ "The full setlist can be found here: https://docs.google.com/spreadsheets/d/"
									+ bot.spreadsheetId);
						}
					} catch (IOException e) {
						Utils.errorReport(e);
						e.printStackTrace();
					}
				}
			}
		}
	}

	public void songlistTimer(String channel) throws NumberFormatException, FileNotFoundException, IOException {
		if (Integer.parseInt(getNumberOfSongs()) == 0) {
			bot.sendRawLine("PRIVMSG " + channel + " :" + "The song list is empty!");
		} else if (numOfSongsToDisplay > Integer.parseInt(getNumberOfSongs())) {
			if (Integer.parseInt(getNumberOfSongs()) == 1) {
				String text = "The next song in the song list: ";
				songlist(channel, text);
			} else {
				String text = "The next " + Integer.parseInt(getNumberOfSongs()) + " songs in the song list: ";
				songlist(channel, text);
			}
		} else {
			String text = "The next " + numOfSongsToDisplay + " songs in the song list: ";
			songlist(channel, text);
		}
	}

	public void requestCOMMAND(String message, String channel, String sender)
			throws NumberFormatException, FileNotFoundException, IOException {
		if (bot.checkUserLevel(sender, requestComm, channel)) {
			for (int i = 0; i < requestComm.input.length; i++) {
				String temp = message.toLowerCase();
				String commList = "";
				for (int j = 0; j < requestComm.input.length; j++) {
					commList += ", " + String.join("", requestComm.input[j]);
				}
				if (!commList.equalsIgnoreCase("")) {
					commList = commList.substring(1, commList.length());
				}
				if (temp.equals(requestComm.input[i])) {
					String result = "";
					if (mustFollowToRequest) {
						result += "You must be following the stream to request. ";
					}
					if (ylrequests && direquests) {
						result = "To request a song, type: " + commList + " [artist - song] OR [youtube link]";
					} else if (ylrequests) {
						result = "To request a song, type: " + commList + " [youtube link]";
					} else if (direquests) {
						result = "To request a song, type: " + commList + " [artist - song]";
					}
					bot.sendRawLine("PRIVMSG " + channel + " :" + result);
				} else if (temp.startsWith(requestComm.input[i]) && temp.contains(requestComm.input[i] + " ")) {
					String youtubeID = null;
					Video ytvid = null;
					if (message.contains("www.") || message.contains("http://") || message.contains("http://www.")
							|| message.contains(".com") || message.contains("https://")) {
						if (message.contains("www.youtube.com/watch?v=") || message.contains("www.youtube.com/watch?v=")
								|| message.contains("https://youtu.be/")) {
							if (!ylrequests) {
								bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid Request, " + sender);
								return;
							}
							if (message.contains("https://youtu.be/")) {
								youtubeID = message.substring(message.lastIndexOf("/") + 1);
							} else {
								youtubeID = message.substring(message.indexOf("=") + 1);
							}
							try {
								ytvid = bot.youtube.searchYoutubeByID(youtubeID);
								if (ytvid == null) {
									bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid youtube URL, " + sender);
									return;
								}
							} catch (Exception e) {
								bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid youtube URL, " + sender);
								return;
							}
						} else {
							bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid Request, " + sender);
							return;
						}
					}
					if (bannedKeywords != null) {
						for (int j = 0; j < bannedKeywords.length; j++) {
							if (ytvid != null) {
								temp = ytvid.getSnippet().getTitle();
							}
							if (temp.toLowerCase().contains(bannedKeywords[j].toLowerCase())) {
								bot.sendRawLine("PRIVMSG " + channel + " :" + "Song request contains a banned keyword '"
										+ bannedKeywords[j] + "' and cannot be added, " + sender + "!");
								return;
							}
						}
					}
					if (Integer.parseInt(getNumberOfSongs()) < maxSonglistLength) {
						if (requestsTrigger) {
							if (Utils.getFollowingText(message).length() < 100) {
								try {
									boolean check = true;
									if (!checkIfUserAlreadyHasSong(sender)) {
										if (mustFollowToRequest) {
											if (!Utils.checkIfUserIsFollowing(channel, sender, bot.streamer,
													bot.users)) {
												check = false;
											}
										}
										if (check) {
											try {
												if (maxSongLength) {
													try {
														String time;
														if (ytvid != null) {
															time = ytvid.getContentDetails().getDuration();
															temp = ytvid.getSnippet().getTitle();
														} else {
															Video v = bot.youtube.searchYoutubeByTitle(
																	Utils.getFollowingText(message));
															time = v.getContentDetails().getDuration();
															temp = v.getSnippet().getTitle();
														}
														time = time.replace("PT", "");
														if (time.contains("H")) {
															bot.sendRawLine("PRIVMSG " + channel + " :" + temp
																	+ " is longer than " + maxSongLengthInMinutes
																	+ " minutes, which is the limit for standard requests, "
																	+ sender);
															return;
														}
														int minutes = Integer
																.parseInt(time.substring(0, time.indexOf('M')));
														int seconds = Integer.parseInt(time
																.substring(time.indexOf('M') + 1, time.indexOf('S')));
														int songlengthmaxseconds = maxSongLengthInMinutes * 60;
														if (songlengthmaxseconds < ((minutes * 60) + seconds)) {
															bot.sendRawLine("PRIVMSG " + channel + " :" + temp
																	+ " is longer than " + maxSongLengthInMinutes
																	+ " minutes, which is the limit for standard requests, "
																	+ sender);
															return;
														}
													} catch (Exception e) {
														bot.sendRawLine("PRIVMSG " + channel + " :"
																+ "Failed to get video, please try again later.");
														System.out.println(e);
														Utils.errorReport(e);
														return;
													}
												}
												if (ytvid != null) {
													addSong(channel, ytvid.getSnippet().getTitle(), sender);
													return;
												} else {
													if (direquests) {
														addSong(channel, Utils.getFollowingText(message), sender);
													} else {
														bot.sendRawLine("PRIVMSG " + channel + " :"
																+ "Only youtube link requests are allowed, " + sender);
													}
												}
											} catch (IOException e) {
												Utils.errorReport(e);
												e.printStackTrace();
											}
										} else {
											bot.sendRawLine("PRIVMSG " + channel + " :"
													+ "You must follow the stream to request a song, " + sender);
										}
									} else {
										if (numOfSongsInQueuePerUser == 1) {
											bot.sendRawLine("PRIVMSG " + channel + " :"
													+ "You may only have 1 song in the queue at a time, " + sender
													+ "!");
										} else {
											bot.sendRawLine("PRIVMSG " + channel + " :" + "You may only have "
													+ numOfSongsInQueuePerUser + " songs in the queue at a time, "
													+ sender + "!");
										}
									}
								} catch (IOException e1) {
									Utils.errorReport(e1);
									e1.printStackTrace();
								}
							} else {
								bot.sendRawLine("PRIVMSG " + channel + " :"
										+ "Request input too long, please shorten request input, " + sender + "!");
							}
						} else {
							bot.sendRawLine("PRIVMSG " + channel + " :" + "Requests are currently off!");
						}
					} else {
						bot.sendRawLine("PRIVMSG " + channel + " :" + "Song limit of " + maxSonglistLength
								+ " has been reached, please try again later.");
					}
				}

			}
		} else {
			for (int i = 0; i < requestComm.input.length; i++) {
				String temp = message.toLowerCase();
				String commList = "";
				for (int j = 0; j < requestComm.input.length; j++) {
					commList += ", " + String.join("", requestComm.input[j]);
				}
				if (!commList.equalsIgnoreCase("")) {
					commList = commList.substring(1, commList.length());
				}
				if (temp.startsWith(requestComm.input[i]) && temp.contains(requestComm.input[i] + " ")) {
					if (subOnlyRequests.contains("$user")) {
						bot.sendRawLine("PRIVMSG " + channel + " :" + subOnlyRequests.replace("$user", sender));
					} else {
						bot.sendRawLine("PRIVMSG " + channel + " :" + subOnlyRequests);
					}
				}
			}
		}
	}

	public String getCurrentSongTitle(String channel) throws NumberFormatException, FileNotFoundException, IOException {
		if (Integer.parseInt(getNumberOfSongs()) == 0) {
			return "Song list is empty";
		} else {
			String line = "";
			try (BufferedReader br = new BufferedReader(new FileReader(Utils.songlistfile))) {
				if ((line = br.readLine()) != null) {
					if (line.startsWith("VIP\t") || line.startsWith("$$$\t")) {
						line = line.substring(line.indexOf("\t") + 1, line.length());
					} else {
						line = line.substring(0, line.length());
					}
				}
			}
			return line;
		}
	}

	public void songlist(String channel, String text) throws FileNotFoundException, IOException {
		if (Integer.parseInt(getNumberOfSongs()) == 0) {
			bot.sendRawLine("PRIVMSG " + channel + " :" + "The song list is empty!");
		}
		if (!displayOneLine) {
			try (BufferedReader br = new BufferedReader(new FileReader(Utils.songlistfile))) {
				String line;
				String temp = "";
				int count = 1;
				bot.sendRawLine("PRIVMSG " + channel + " :" + text);
				while ((line = br.readLine()) != null) {
					if (count < numOfSongsToDisplay + 1) {
						temp = count + ". " + line + " ";
						bot.sendRawLine("PRIVMSG " + channel + " :" + temp);
						count++;
					}
				}
			}
		} else {
			String temp2 = "";
			try (BufferedReader br = new BufferedReader(new FileReader(Utils.songlistfile))) {
				String line;
				String temp = "";
				int count = 1;
				while ((line = br.readLine()) != null) {
					if (count < numOfSongsToDisplay + 1) {
						temp = count + ". " + line + " ";
						temp2 += temp;
						count++;
					}
				}
				bot.sendRawLine("PRIVMSG " + channel + " :" + text + " " + temp2);
			}
		}
	}

	public boolean nextSong(String channel) throws NumberFormatException, FileNotFoundException, IOException {
		if ((Integer.parseInt(getNumberOfSongs()) == 0)) {
			return false;
		} else {
			if (!doNotWriteToHistory) {
				lastSong = getCurrentSongTitle(channel);
				appendToLastSongs(channel, lastSong);
			}
			copyToTemp(channel);
			doNotWriteToHistory = false;
			try (BufferedReader br = new BufferedReader(new FileReader(Utils.templistfile))) {
				br.readLine();
				String line;
				BufferedWriter writer = new BufferedWriter(new FileWriter(Utils.songlistfile));
				while ((line = br.readLine()) != null) {
					writer.write(line + "\r");
				}
				clear(channel, Utils.templistfile);
				writer.close();
			}
			return true;
		}
	}

	public String getNextSong(String channel) throws FileNotFoundException, IOException {
		String line = null, song = null, requestedby = null;
		try (BufferedReader br = new BufferedReader(new FileReader(Utils.songlistfile))) {
			if ((line = br.readLine()) != null) {
				if (line.startsWith("$$$\t") || line.startsWith("VIP\t")) {
					song = line.substring(line.indexOf("\t") + 1, line.lastIndexOf('\t'));
				} else {
					song = line.substring(0, line.lastIndexOf('\t'));
				}
				requestedby = line.substring(line.lastIndexOf('\t') + 1, line.length());
			}
		}
		if (song == null) {
			return "There are no songs in the queue!";
		} else {
			if (displayIfUserIsHere) {
				if (bot.checkIfUserIsHere(requestedby, channel)) {
					if (whisperToUser) {
						String toWhisper = requestedby.substring(1, requestedby.length() - 1);
						if (!bot.streamer.toLowerCase().equals(toWhisper.toLowerCase())) {
							bot.sendRawLine("PRIVMSG " + channel + " :/w " + toWhisper + " Your request '" + song
									+ "' is being played next in " + bot.streamer + "'s stream!");
						}
					}
					return "The next song is: '" + song + "' - " + requestedby + " HERE! :) ";
				} else {
					return "The next song is: '" + song + "' - " + requestedby;
				}
			} else {
				return "The next song is: '" + song + "' - " + requestedby;
			}
		}
	}

	public String getNumberOfSongs() throws FileNotFoundException, IOException {
		try (BufferedReader br = new BufferedReader(new FileReader(Utils.songlistfile))) {
			int count = 0;
			while ((br.readLine()) != null) {
				count++;
			}
			return Integer.toString(count);
		}
	}

	public boolean editCurrent(String channel, String newSong, String sender)
			throws FileNotFoundException, IOException {
		String line = null, requestedby = null;
		String prefix = "";
		if ((Integer.parseInt(getNumberOfSongs()) == 0)) {
			Writer output;
			output = new BufferedWriter(new FileWriter(Utils.songlistfile, true));
			output.append(newSong + "\t(" + sender + ")\r");
			output.close();
			bot.sendRawLine("PRIVMSG " + channel + " :" + "Since there are no songs in the song list, song '" + newSong
					+ "' has been added to the song list, " + sender + "!");
			writeToCurrentSong(channel, false);
			return false;
		}
		try (BufferedReader br = new BufferedReader(new FileReader(Utils.songlistfile))) {
			if ((line = br.readLine()) != null) {
				if (line.startsWith("VIP\t")) {
					prefix = "VIP\t";
				} else if (line.startsWith("$$$\t")) {
					prefix = "$$$\t";
				}
				requestedby = line.substring(line.lastIndexOf('\t') + 1, line.length());
			}
		}
		copyToTemp(channel);
		try (BufferedReader br = new BufferedReader(new FileReader(Utils.templistfile))) {
			br.readLine();
			String line2;
			BufferedWriter writer = new BufferedWriter(new FileWriter(Utils.songlistfile));
			if (prefix.equals("")) {
				writer.write(newSong + "\t" + requestedby + "\r");
			} else {
				writer.write(prefix + newSong + "\t" + requestedby + "\r");
			}
			while ((line2 = br.readLine()) != null) {
				writer.write(line2 + "\r");
			}
			clear(channel, Utils.templistfile);
			writer.close();
		}
		writeToCurrentSong(channel, false);
		return true;
	}

	public boolean checkIfUserAlreadyHasSong(String user) throws FileNotFoundException, IOException {
		if (user.equals(bot.streamer)) {
			return false;
		}
		try (BufferedReader br = new BufferedReader(new FileReader(Utils.songlistfile))) {
			String line;
			int count = 0;
			while ((line = br.readLine()) != null) {
				if (line.contains("\t(" + user) && (!line.startsWith("$$$\t") && (!line.startsWith("VIP\t")))) {
					count++;
				}
			}
			if (count >= numOfSongsInQueuePerUser) {
				return true;
			}
		}
		return false;

	}

	public void addSong(String channel, String song, String requestedby) throws IOException {
		Writer output;
		output = new BufferedWriter(new FileWriter(Utils.songlistfile, true));
		output.append(song + "\t(" + requestedby + ")\r");
		output.close();
		bot.sendRawLine("PRIVMSG " + channel + " :" + "Song '" + song + "' has been added to the song list, "
				+ requestedby + "!");
		bot.addUserRequestAmount(requestedby, true);
		writeToCurrentSong(channel, false);
	}

	public void addTop(String channel, String song, String requestedby) throws IOException {
		copyToTemp(channel);
		try (BufferedReader br = new BufferedReader(new FileReader(Utils.templistfile))) {
			BufferedReader br2 = new BufferedReader(new FileReader(Utils.templistfile));
			String line2;
			BufferedWriter writer = new BufferedWriter(new FileWriter(Utils.songlistfile));
			writer.write("$$$\t" + song + "\t(" + requestedby + ")\r");
			while ((line2 = br.readLine()) != null) {
				writer.write(line2 + "\r");
			}
			clear(channel, Utils.templistfile);
			br2.close();
			writer.close();
		}
	}

	public void addVip(String channel, String song, String requestedby) throws IOException {
		if (Integer.parseInt(getNumberOfSongs()) == 0) {
			Writer output;
			output = new BufferedWriter(new FileWriter(Utils.songlistfile, true));
			output.append("VIP\t" + song + "\t(" + requestedby + ")\r");
			output.close();
		} else if (Integer.parseInt(getNumberOfSongs()) == 1) {
			try (BufferedReader br = new BufferedReader(new FileReader(Utils.templistfile))) {
				copyToTemp(channel);
				BufferedReader br2 = new BufferedReader(new FileReader(Utils.templistfile));
				String line2 = br2.readLine();
				BufferedWriter writer = new BufferedWriter(new FileWriter(Utils.songlistfile));
				if (line2.startsWith("VIP\t") || line2.startsWith("$$$\t")) {
					writer.write(line2 + "\r");
				} else {
					writer.write("VIP\t" + line2 + "\r");
				}
				br2.readLine();
				writer.write("VIP\t" + song + "\t(" + requestedby + ")\r");
				clear(channel, Utils.templistfile);
				br2.close();
				writer.close();
			} catch (Exception ioe) {
				Utils.errorReport(ioe);
				ioe.printStackTrace();
			}
		} else {
			try (BufferedReader br3 = new BufferedReader(new FileReader(Utils.templistfile))) {
				String line2, line4;
				copyToTemp(channel);
				line4 = br3.readLine();
				br3.close();
				if (line4.startsWith("$$$\t")) {
					BufferedReader br4 = new BufferedReader(new FileReader(Utils.templistfile));
					BufferedWriter writer2 = new BufferedWriter(new FileWriter(Utils.songlistfile));
					while ((line2 = br4.readLine()) != null) {
						if (line2.startsWith("$$$\t")) {
							writer2.write(line2 + "\r");
						} else {
							break;
						}
					}
					// CHECK IF NEXT NON $$$ IS VIP
					if (line2.startsWith("VIP\t")) {
						// IF YES : WRITE ALL VIP, WRITE SONG, WRITE REMAINING
						if (line2 != null && line2.contains("VIP\t")) {
							writer2.write(line2 + "\r");
						}
						while ((line2 = br4.readLine()) != null) {
							if (line2.contains("VIP\t")) {
								writer2.write(line2 + "\r");
							} else {
								break;
							}
						}
						writer2.write("VIP\t" + song + "\t(" + requestedby + ")\r");
						if (line2 != null) {
							writer2.write(line2 + "\r");
						}
						while ((line2 = br4.readLine()) != null) {
							writer2.write(line2 + "\r");
						}
					} else {
						// IF NOT : WRITE SONG, WRITE REMAINING SONGS
						writer2.write("VIP\t" + song + "\t(" + requestedby + ")\r");

						if (line2 != null) {
							writer2.write(line2 + "\r");
						}
						while ((line2 = br4.readLine()) != null) {
							writer2.write(line2 + "\r");
						}
					}
					br4.close();
					writer2.close();
					clear(channel, Utils.templistfile);
				} else if (line4.startsWith("VIP\t")) {
					BufferedReader br4 = new BufferedReader(new FileReader(Utils.templistfile));
					BufferedWriter writer2 = new BufferedWriter(new FileWriter(Utils.songlistfile));
					while ((line2 = br4.readLine()) != null) {
						if (line2.startsWith("VIP\t")) {
							writer2.write(line2 + "\r");
						} else {
							break;
						}
					}
					writer2.write("VIP\t" + song + "\t(" + requestedby + ")\r");
					if (line2 != null) {
						writer2.write(line2 + "\r");
					}
					while ((line2 = br4.readLine()) != null) {
						writer2.write(line2 + "\r");
					}
					br4.close();
					writer2.close();
					clear(channel, Utils.templistfile);
				} else {
					BufferedReader br = new BufferedReader(new FileReader(Utils.templistfile));
					BufferedWriter writer = new BufferedWriter(new FileWriter(Utils.songlistfile));
					writer.write("VIP\t" + br.readLine() + "\r");
					writer.write("VIP\t" + song + "\t(" + requestedby + ")\r");
					while ((line2 = br.readLine()) != null) {
						writer.write(line2 + "\r");
					}
					clear(channel, Utils.templistfile);
					br.close();
					writer.close();
				}
			} catch (Exception ioe) {
				Utils.errorReport(ioe);
				ioe.printStackTrace();
			}
		}
	}

}
