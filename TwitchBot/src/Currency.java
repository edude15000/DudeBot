import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.Iterator;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;
import java.util.Random;

import com.google.gson.annotations.Expose;

public class Currency {
	@Expose(serialize = true, deserialize = true)
	String currencyName, currencyCommand;
	@Expose(serialize = true, deserialize = true)
	int currencyPerMinute, maxGamble, gambleCoolDownMinutes, vipRedeemCoolDownMinutes, vipSongCost,
			subCreditRedeemCost = 1, creditsPerSub = 1, rankupUnitCost;
	@Expose(serialize = true, deserialize = true)
	boolean toggle, vipSongToggle, gambleToggle;
	@Expose(serialize = true, deserialize = true)
	LinkedHashMap<String, Integer> ranks = new LinkedHashMap<String, Integer>();
	List<BotUser> users;

	public Currency(List<BotUser> users) {
		this.users = users;
	}

	public String getSubCredits(String user) {
		BotUser botUser;
		if ((botUser = getBotUser(user)) != null) {
			return user + " has " + botUser.subCredits + " sub credits.";
		}
		return user + " has yet to gain any sub credits.";
	}

	public String bonusSubCredits(String user, int amount) {
		BotUser botUser;
		if ((botUser = getBotUser(user)) != null) {
			botUser.subCredits += amount;
			if (amount >= 0) {
				return user + " has been given " + amount + " sub credits!";
			} else {
				if (botUser.subCredits < 0) {
					botUser.subCredits = 0;
				}
				return amount + " sub credits have been taken away from " + user + ".";
			}
		}
		return "Failed to give " + user + " sub credits.";
	}

	public boolean redeemSubCredits(String user) {
		BotUser botUser;
		if ((botUser = getBotUser(user)) != null) {
			if (botUser.subCredits < subCreditRedeemCost) {
				return false;
			}
			botUser.subCredits -= subCreditRedeemCost;
			return true;
		}
		return false;
	}

	public String nextRank(String user) {
		try {
			int points = 0, time = 0;
			String currrank = null;
			BotUser botUser;
			if ((botUser = getBotUser(user)) != null) {
				points = botUser.points;
				time = botUser.time;
				currrank = botUser.rank;
			}
			if (rankupUnitCost == 0 || rankupUnitCost == 2) {
				String[] arr = getNextRankName(points, (time / 60));
				if (arr[0].equals("MAX_VAL")) {
					return "You are at max rank, " + user + "!";
				}
				if (arr[0].equals("unranked")) {
					return "There are currently no ranks in this stream.";
				}
				String units = "hours";
				if (rankupUnitCost == 0) {
					units = currencyName;
				}
				return "The next rank '" + arr[0] + "' is unlocked at " + arr[1] + " " + units;
			} else {
				String[] arr = null;
				if (currrank != null) {
					arr = getNextRankName(ranks.get(currrank), (time / 60));
				} else {
					arr = getNextRankName(0, 0);
				}
				if (arr[0].equals("MAX_VAL")) {
					return "You are at max rank, " + user + "!";
				}
				if (arr[0].equals("unranked")) {
					return "There are currently no ranks in this stream.";
				}
				return "The next rank '" + arr[0] + "' can be purchased for " + arr[1] + " " + currencyName
						+ "! To purchase this rank, type '!rankup'";
			}
		} catch (Exception e) {
			Utils.errorReport(e);
			e.printStackTrace();
		}
		return "";
	}

	public boolean writeToRanks(String user, String rankToBuy) {
		for (BotUser u : users) {
			if (u.username.equalsIgnoreCase(user)) {
				u.rank = rankToBuy;
				return true;
			}
		}
		users.add(new BotUser(user, 0, false, false, false, 0, 0, rankToBuy, 0, 0, 0));
		return true;
	}

	public int vipsong(String user) {
		try {
			int points = 0;
			BotUser botUser;
			if ((botUser = getBotUser(user)) != null) {
				if (botUser.vipCoolDown != 0) {
					if (botUser.vipCoolDown + (vipRedeemCoolDownMinutes * 60000) <= System.currentTimeMillis()) {
						botUser.vipCoolDown = 0;
					} else {
						return -1;
					}
				}
				points = botUser.points;
				if (points >= vipSongCost) {
					botUser.points = points - vipSongCost;
					botUser.vipCoolDown = System.currentTimeMillis();
					return 1;
				}
			}
		} catch (

		Exception e) {
			Utils.errorReport(e);
			e.printStackTrace();
		}
		return 0;
	}

	public String getLeaderBoards(String streamer, String botname) {
		HashMap<String, Integer> currencies = new HashMap<String, Integer>();
		HashMap<String, Integer> hours = new HashMap<String, Integer>();
		for (BotUser botUser : users) {
			if (!botUser.username.equalsIgnoreCase("moobot") && !botUser.username.equalsIgnoreCase("revlobot")
					&& !botUser.username.equalsIgnoreCase("deepbot") && !botUser.username.equalsIgnoreCase("nightbot")
					&& !botUser.username.equalsIgnoreCase(streamer) && !botUser.username.equalsIgnoreCase(botname)) {
				currencies.put(botUser.username, botUser.points);
				hours.put(botUser.username, botUser.time);
			}
		}
		Map<String, Integer> hoursSorted = sortHashMapByValuesMax(hours);
		Map<String, Integer> currencySorted = sortHashMapByValuesMax(currencies);
		String result = "";
		if (hours.isEmpty()) {
			return "No one has gained any time in the stream yet!";
		}
		ArrayList<String> userHoursList = new ArrayList<String>();
		for (Map.Entry<String, Integer> entry : hoursSorted.entrySet()) {
			userHoursList.add(entry.getKey());
		}
		ArrayList<String> userCurrencyList = new ArrayList<String>();
		for (Map.Entry<String, Integer> entry : currencySorted.entrySet()) {
			userCurrencyList.add(entry.getKey());
		}
		result += "TIME: ";
		if (hours.size() < 5) {
			for (int i = 0; i < hours.size(); i++) {
				result += "#" + (i + 1) + " " + userHoursList.get(i) + " ("
						+ convertToTime(String.valueOf(hoursSorted.get(userHoursList.get(i)))) + ") ";
			}
		} else {
			for (int i = 0; i < 5; i++) {
				result += "#" + (i + 1) + " " + userHoursList.get(i) + " ("
						+ convertToTime(String.valueOf(hoursSorted.get(userHoursList.get(i)))) + ") ";
			}
		}
		result += ", " + currencyName.toUpperCase() + ": ";
		if (hours.size() < 5) {
			for (int i = 0; i < hours.size(); i++) {
				result += "#" + (i + 1) + " " + userCurrencyList.get(i) + " ("
						+ currencySorted.get(userCurrencyList.get(i)) + ") ";
			}
		} else {
			for (int i = 0; i < 5; i++) {
				result += "#" + (i + 1) + " " + userCurrencyList.get(i) + " ("
						+ currencySorted.get(userCurrencyList.get(i)) + ") ";
			}
		}
		return result;
	}

	public LinkedHashMap<String, Integer> sortHashMapByValuesMax(HashMap<String, Integer> passedMap) {
		List<String> mapKeys = new ArrayList<>(passedMap.keySet());
		List<Integer> mapValues = new ArrayList<>(passedMap.values());
		Collections.sort(mapValues);
		Collections.reverse(mapValues);
		Collections.sort(mapKeys);
		Collections.reverse(mapKeys);
		LinkedHashMap<String, Integer> sortedMap = new LinkedHashMap<>();
		Iterator<Integer> valueIt = mapValues.iterator();
		while (valueIt.hasNext()) {
			Integer val = valueIt.next();
			Iterator<String> keyIt = mapKeys.iterator();

			while (keyIt.hasNext()) {
				String key = keyIt.next();
				Integer comp1 = passedMap.get(key);
				Integer comp2 = val;

				if (comp1.equals(comp2)) {
					keyIt.remove();
					sortedMap.put(key, val);
					break;
				}
			}
		}
		return sortedMap;
	}

	public LinkedHashMap<String, Integer> sortHashMapByValuesMin(HashMap<String, Integer> passedMap) {
		List<String> mapKeys = new ArrayList<>(passedMap.keySet());
		List<Integer> mapValues = new ArrayList<>(passedMap.values());
		Collections.sort(mapValues);
		Collections.sort(mapKeys);
		LinkedHashMap<String, Integer> sortedMap = new LinkedHashMap<>();
		Iterator<Integer> valueIt = mapValues.iterator();
		while (valueIt.hasNext()) {
			Integer val = valueIt.next();
			Iterator<String> keyIt = mapKeys.iterator();

			while (keyIt.hasNext()) {
				String key = keyIt.next();
				Integer comp1 = passedMap.get(key);
				Integer comp2 = val;

				if (comp1.equals(comp2)) {
					keyIt.remove();
					sortedMap.put(key, val);
					break;
				}
			}
		}
		return sortedMap;
	}

	public String getRank(String user, String streamer, String botname) {
		int currentUserCurrency = -1, currentUserHours = -1;
		ArrayList<Integer> currencies = new ArrayList<Integer>();
		ArrayList<Integer> hours = new ArrayList<Integer>();
		BotUser botUser;
		if ((botUser = getBotUser(user)) != null) {
			currentUserCurrency = botUser.points;
			currentUserHours = botUser.time;
			if (!botUser.username.equalsIgnoreCase("moobot") && !botUser.username.equalsIgnoreCase("revlobot")
					&& !botUser.username.equalsIgnoreCase("deepbot") && !botUser.username.equalsIgnoreCase("nightbot")
					&& !botUser.username.equalsIgnoreCase(streamer) && !botUser.username.equalsIgnoreCase(botname)) {
				currencies.add(botUser.points);
				hours.add(botUser.time);
			}
		}
		if (currentUserCurrency < 0) {
			return user + " has yet to gain points in the stream!";
		}
		Collections.sort(currencies);
		Collections.reverse(currencies);
		Collections.sort(hours);
		Collections.reverse(hours);
		int countCurrency = 1;
		for (int i = 0; i < currencies.size(); i++) {
			if (currencies.get(i).equals(currentUserCurrency)) {
				break;
			}
			countCurrency++;
		}
		int countHours = 1;
		for (int i = 0; i < hours.size(); i++) {
			if (hours.get(i).equals(currentUserHours)) {
				break;
			}
			countHours++;
		}
		return user + " is in place #" + countCurrency + " in " + currencyName + " with " + currentUserCurrency + " "
				+ currencyName + " and in place #" + countHours + " in time spent in the stream with "
				+ convertToTime(String.valueOf(currentUserHours)) + "!";
	}

	public String bonus(String user, int amount) {
		try {
			BotUser botUser;
			if ((botUser = getBotUser(user)) != null) {
				botUser.points += amount;
				if (amount >= 0) {
					return user + " has been given " + amount + " " + currencyName + "!";
				} else {
					return amount + " " + currencyName + " has been taken away from " + user + "!";
				}
			}
		} catch (Exception g) {
			g.printStackTrace();
			Utils.errorReport(g);
		}
		return "Failed to give points to " + user;
	}

	public String bonusall(int amount, List<String> usersHere, boolean auto) {
		List<String> awarded = new ArrayList<>();
		for (String s : usersHere) {
			for (BotUser botUser : users) {
				if (awarded.contains(s)) {
					break;
				}
				if (botUser.username.equalsIgnoreCase(s)) {
					if (auto) {
						botUser.time += 1;
					}
					if (botUser.points + amount < 0) {
						botUser.points = 0;
					} else {
						botUser.points += amount;
					}
					awarded.add(s);
					break;
				}
			}
		}
		if (!auto) {
			if (amount > -1) {
				return "Everyone has been given " + amount + " " + currencyName + "!";
			} else {
				return amount + " " + currencyName + " has been taken away from everyone!";
			}
		} else {
			return "";
		}
	}

	public String getCurrency(String user) {
		try {
			int points = 0, time = 0;
			String result = "";
			BotUser botUser;
			if ((botUser = getBotUser(user)) != null) {
				points = botUser.points;
				time = botUser.time;
				if (botUser.rank != null) {
					result = "[" + botUser.rank + "]";
				}
			}
			if (ranks.size() != 0 && (rankupUnitCost == 0 || rankupUnitCost == 2)) {
				result = "[" + getRankName(points, (time / 60)) + "]";
			}
			return result + " " + user + " has " + points + " " + currencyName + " and "
					+ convertToTime(String.valueOf(time)) + " spent in stream!";
		} catch (Exception e) {
			Utils.errorReport(e);
			e.printStackTrace();
		}
		return "An error has occurred trying to retrieve currency values for " + user;
	}

	public String rankup(String user) {
		try {
			int points = 0;
			String currrank = null;
			BotUser botUser;
			if ((botUser = getBotUser(user)) != null) {
				points = botUser.points;
				currrank = botUser.rank;
			}
			String[] arr = null;
			if (currrank != null) {
				arr = getNextRankName(ranks.get(currrank), 0);
			} else {
				arr = getNextRankName(0, 0);
			}
			if (arr[0].equals("MAX_VAL")) {
				return "You are at max rank, " + user + "!";
			}
			if (arr[0].equals("unranked")) {
				return "There are currently no ranks in this stream.";
			}
			int cost = Integer.parseInt(arr[1]);
			if (points >= cost) {
				if (!writeToRanks(user, arr[0])) {
					return "Failed to buy rank, please try again later.";
				}
				if (bonus(user, (cost * -1)).startsWith("Failed")) {
					return "Failed to buy rank, please try again later.";
				}
				return user + " has just purchased the rank '" + arr[0] + "' for " + arr[1] + " " + currencyName + "!";
			} else {
				return "You cannot afford to purchase this rank, " + user + ". '" + arr[0] + "' costs " + arr[1] + " "
						+ currencyName + "!";
			}
		} catch (Exception e) {
			e.printStackTrace();
		}
		return "Failed to buy rank, please try again later.";
	}

	public String getRankName(int points, int hours) {
		LinkedHashMap<String, Integer> sorted = sortHashMapByValuesMin(ranks);
		String result = "unranked";
		if (rankupUnitCost == 0) {
			for (int v : sorted.values()) {
				if (points >= v) {
					result = getKeyFromValue(sorted, v);
				}
			}
		} else {
			for (int v : sorted.values()) {
				if (hours >= v) {
					result = getKeyFromValue(sorted, v);
				}
			}
		}
		return result;
	}

	public static String getKeyFromValue(Map<String, Integer> hm, int value) {
		for (Object o : hm.keySet()) {
			if (hm.get(o).equals(value)) {
				return (String) o;
			}
		}
		return null;
	}

	public String[] getNextRankName(int points, int hours) {
		Map<String, Integer> sorted = sortHashMapByValuesMin(ranks);
		int cost = 0;
		if (rankupUnitCost == 0 || rankupUnitCost == 1) {
			for (int v : sorted.values()) {
				if (points >= v) {
					cost = v;
				}
			}
		} else {
			for (int v : sorted.values()) {
				if (hours >= v) {
					cost = v;
				}
			}
		}
		Iterator<Entry<String, Integer>> it = sorted.entrySet().iterator();
		Entry<String, Integer> a = null;
		while (it.hasNext()) {
			a = it.next();
			if (cost == 0) {
				break;
			}
			if (a.getValue() == cost) {
				if (it.hasNext()) {
					a = it.next();
				} else {
					a = null;
				}
				break;
			}
		}
		if (a == null) {
			String[] arr = { "MAX_VAL", "0" };
			return arr;
		} else {
			String[] arr = { a.getKey(), String.valueOf(a.getValue()) };
			return arr;
		}
	}

	public String convertToTime(String time) {
		int temp = Integer.parseInt(time);
		if (temp < 60) {
			return temp + " minutes";
		} else {
			return (temp / 60) + " hours";
		}
	}

	public String gamble(String user, int amount) {
		if (amount < 1) {
			return "You must gamble at least 1, " + user + "!";
		}
		if (amount > maxGamble) {
			return "The max gamble is " + maxGamble + ", " + user + "!";
		}
		try {
			int points = -1;
			BotUser botUser;
			if ((botUser = getBotUser(user)) != null) {
				points = botUser.points;
				if (botUser.gambleCoolDown != 0) {
					if ((botUser.gambleCoolDown + (gambleCoolDownMinutes * 60000) <= System.currentTimeMillis())) {
						botUser.gambleCoolDown = 0;
					} else {
						return "You may gamble once every " + gambleCoolDownMinutes + " minutes, " + user + "!";
					}
				}
				if (botUser.gambleCoolDown == 0) {
					if (points < amount) {
						return "You do not have enough " + currencyName + " to gamble that many, " + user + "!";
					}
					int roll = new Random().nextInt(101) + 1;
					if (roll < 60) {
						botUser.points -= amount;
						botUser.gambleCoolDown = System.currentTimeMillis();
						return "Rolled " + roll + ". " + user + " has lost " + amount + " " + currencyName + ". " + user
								+ " now has " + botUser.points + " " + currencyName + ".";
					} else if (roll <= 98) {
						botUser.points += amount;
						botUser.gambleCoolDown = System.currentTimeMillis();
						return user + " rolled " + roll + " and has won " + amount + " " + currencyName + ". " + user
								+ " now has " + botUser.points + " " + currencyName + ".";
					} else {
						botUser.points += (2 * amount);
						botUser.gambleCoolDown = System.currentTimeMillis();
						return user + " rolled " + roll + " and has won " + (2 * amount) + " " + currencyName + ". "
								+ user + " now has " + botUser.points + " " + currencyName + ".";
					}
				}
			}
		} catch (Exception e) {
			Utils.errorReport(e);
			e.printStackTrace();
		}
		return "Could not gamble right now. Please try again later.";
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