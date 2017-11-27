using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class Currency
{
    public String currencyName, currencyCommand;
    public int currencyPerMinute, maxGamble, gambleCoolDownMinutes, vipRedeemCoolDownMinutes, vipSongCost,
            subCreditRedeemCost = 1, creditsPerSub = 1, rankupUnitCost;
    public Boolean toggle, vipSongToggle, gambleToggle;
    public Dictionary<String, Int32> ranks = new Dictionary<String, Int32>();
    public List<BotUser> users;

    public Currency(List<BotUser> users)
    {
        this.users = users;
    }

    public String getSubCredits(String user)
    {
        BotUser botUser;
        if ((botUser = getBotUser(user)) != null)
        {
            return user + " has " + botUser.subCredits + " sub credits.";
        }
        return user + " has yet to gain any sub credits.";
    }

    public String bonusSubCredits(String user, int amount)
    {
        BotUser botUser;
        if ((botUser = getBotUser(user)) != null)
        {
            botUser.subCredits += amount;
            if (amount >= 0)
            {
                return user + " has been given " + amount + " sub credits!";
            }
            else
            {
                if (botUser.subCredits < 0)
                {
                    botUser.subCredits = 0;
                }
                return amount + " sub credits have been taken away from " + user + ".";
            }
        }
        return "Failed to give " + user + " sub credits.";
    }

    public Boolean redeemSubCredits(String user)
    {
        BotUser botUser;
        if ((botUser = getBotUser(user)) != null)
        {
            if (botUser.subCredits < subCreditRedeemCost)
            {
                return false;
            }
            botUser.subCredits -= subCreditRedeemCost;
            return true;
        }
        return false;
    }

    public String nextRank(String user)
    {
        try
        {
            int points = 0, time = 0;
            String currrank = null;
            BotUser botUser;
            if ((botUser = getBotUser(user)) != null)
            {
                points = botUser.points;
                time = botUser.time;
                currrank = botUser.rank;
            }
            if (rankupUnitCost == 0 || rankupUnitCost == 2)
            {
                String[] arr = getNextRankName(points, (time / 60));
                if (arr[0].Equals("MAX_VAL"))
                {
                    return "You are at max rank, " + user + "!";
                }
                if (arr[0].Equals("unranked"))
                {
                    return "There are currently no ranks in this stream.";
                }
                String units = "hours";
                if (rankupUnitCost == 0)
                {
                    units = currencyName;
                }
                return "The next rank '" + arr[0] + "' is unlocked at " + arr[1] + " " + units;
            }
            else
            {
                String[] arr = null;
                if (currrank != null)
                {
                    arr = getNextRankName(ranks[currrank], (time / 60));
                }
                else
                {
                    arr = getNextRankName(0, 0);
                }
                if (arr[0].Equals("MAX_VAL"))
                {
                    return "You are at max rank, " + user + "!";
                }
                if (arr[0].Equals("unranked"))
                {
                    return "There are currently no ranks in this stream.";
                }
                return "The next rank '" + arr[0] + "' can be purchased for " + arr[1] + " " + currencyName
                        + "! To purchase this rank, type '!rankup'";
            }
        }
        catch (Exception e)
        {
            Utils.errorReport(e);
            Debug.WriteLine(e.ToString());
        }
        return "";
    }

    public Boolean writeToRanks(String user, String rankToBuy)
    {
        foreach (BotUser u in users)
        {
            if (u.username.Equals(user, StringComparison.InvariantCultureIgnoreCase))
            {
                u.rank = rankToBuy;
                return true;
            }
        }
        users.Add(new BotUser(user, 0, false, false, false, 0, 0, rankToBuy, 0, 0, 0));
        return true;
    }

    public int vipsong(String user)
    {
        try
        {
            int points = 0;
            BotUser botUser;
            if ((botUser = getBotUser(user)) != null)
            {
                if (botUser.vipCoolDown != 0)
                {
                    if (botUser.vipCoolDown + (vipRedeemCoolDownMinutes * 60000) <= Environment.TickCount)
                    {
                        botUser.vipCoolDown = 0;
                    }
                    else
                    {
                        return -1;
                    }
                }
                points = botUser.points;
                if (points >= vipSongCost)
                {
                    botUser.points = points - vipSongCost;
                    botUser.vipCoolDown = Environment.TickCount;
                    return 1;
                }
            }
        }
        catch (Exception e)
        {
            Utils.errorReport(e);
            Debug.WriteLine(e.ToString());
        }
        return 0;
    }

    public String getLeaderBoards(String streamer, String botname)
    {
        Dictionary<String, Int32> currencies = new Dictionary<String, Int32>();
        Dictionary<String, Int32> hours = new Dictionary<String, Int32>();
        foreach (BotUser botUser in users)
        {
            if (!botUser.username.Equals("moobot", StringComparison.InvariantCultureIgnoreCase) && !botUser.username.Equals("revlobot", StringComparison.InvariantCultureIgnoreCase)
                    && !botUser.username.Equals("deepbot", StringComparison.InvariantCultureIgnoreCase) && !botUser.username.Equals("nightbot", StringComparison.InvariantCultureIgnoreCase)
                    && !botUser.username.Equals(streamer, StringComparison.InvariantCultureIgnoreCase) && !botUser.username.Equals(botname, StringComparison.InvariantCultureIgnoreCase))
            {
                currencies.Add(botUser.username, botUser.points);
                hours.Add(botUser.username, botUser.time);
            }
        }
        Dictionary<String, Int32> hoursSorted = sortHashMapByValuesMax(hours);
        Dictionary<String, Int32> currencySorted = sortHashMapByValuesMax(currencies);
        String result = "";
        if (hours.Count == 0)
        {
            return "No one has gained any time in the stream yet!";
        }
        List<String> userHoursList = new List<String>();
        foreach (KeyValuePair<String, Int32> entry in hoursSorted)
        {
            userHoursList.Add(entry.Key.ToString());
        }
        List<String> userCurrencyList = new List<String>();
        foreach (KeyValuePair<String, Int32> entry in currencySorted)
        {
            userCurrencyList.Add(entry.Key.ToString());
        }
        result += "TIME: ";
        if (hours.Count < 5)
        {
            for (int i = 0; i < hours.Count; i++)
            {
                result += "#" + (i + 1) + " " + userHoursList[i] + " ("
                        + convertToTime(hoursSorted[userHoursList[i]].ToString()) + ") ";
            }
        }
        else
        {
            for (int i = 0; i < 5; i++)
            {
                result += "#" + (i + 1) + " " + userHoursList[i] + " ("
                        + convertToTime(hoursSorted[userHoursList[i]].ToString()) + ") ";
            }
        }
        result += ", " + currencyName.ToUpper() + ": ";
        if (hours.Count < 5)
        {
            for (int i = 0; i < hours.Count; i++)
            {
                result += "#" + (i + 1) + " " + userCurrencyList[i] + " ("
                        + currencySorted[userCurrencyList[i]] + ") ";
            }
        }
        else
        {
            for (int i = 0; i < 5; i++)
            {
                result += "#" + (i + 1) + " " + userCurrencyList[i] + " ("
                        + currencySorted[userCurrencyList[i]] + ") ";
            }
        }
        return result;
    }

    public Dictionary<String, Int32> sortHashMapByValuesMax(Dictionary<String, Int32> passedMap) // TODO : TEST!
    {
        var sortedDict = from entry in passedMap orderby entry.Value descending select entry;
        return sortedDict.ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    public Dictionary<String, Int32> sortHashMapByValuesMin(Dictionary<String, Int32> passedMap) // TODO : TEST!
    {
        var sortedDict = from entry in passedMap orderby entry.Value ascending select entry;
        return sortedDict.ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    public String getRank(String user, String streamer, String botname)
    {
        int currentUserCurrency = -1, currentUserHours = -1;
        List<Int32> currencies = new List<Int32>();
        List<Int32> hours = new List<Int32>();
        BotUser botUser;
        if ((botUser = getBotUser(user)) != null)
        {
            currentUserCurrency = botUser.points;
            currentUserHours = botUser.time;
            if (!botUser.username.Equals("moobot", StringComparison.InvariantCultureIgnoreCase) && !botUser.username.Equals("revlobot", StringComparison.InvariantCultureIgnoreCase)
                    && !botUser.username.Equals("deepbot", StringComparison.InvariantCultureIgnoreCase) && !botUser.username.Equals("nightbot", StringComparison.InvariantCultureIgnoreCase)
                    && !botUser.username.Equals(streamer, StringComparison.InvariantCultureIgnoreCase) && !botUser.username.Equals(botname, StringComparison.InvariantCultureIgnoreCase))
            {
                currencies.Add(botUser.points);
                hours.Add(botUser.time);
            }
        }
        if (currentUserCurrency < 0)
        {
            return user + " has yet to gain points in the stream!";
        }
        currencies.Sort();
        currencies.Reverse();
        hours.Sort();
        hours.Reverse();
        int countCurrency = 1;
        for (int i = 0; i < currencies.Count; i++)
        {
            if (currencies[i].Equals(currentUserCurrency))
            {
                break;
            }
            countCurrency++;
        }
        int countHours = 1;
        for (int i = 0; i < hours.Count; i++)
        {
            if (hours[i].Equals(currentUserHours))
            {
                break;
            }
            countHours++;
        }
        return user + " is in place #" + countCurrency + " in " + currencyName + " with " + currentUserCurrency + " "
                + currencyName + " and in place #" + countHours + " in time spent in the stream with "
                + convertToTime(currentUserHours.ToString()) + "!";
    }

    public String bonus(String user, int amount)
    {
        try
        {
            BotUser botUser;
            if ((botUser = getBotUser(user)) != null)
            {
                botUser.points += amount;
                if (amount >= 0)
                {
                    return user + " has been given " + amount + " " + currencyName + "!";
                }
                else
                {
                    amount *= -1;
                    return amount + " " + currencyName + " has been taken away from " + user + "!";
                }
            }
        }
        catch (Exception g)
        {
            g.ToString();
            Utils.errorReport(g);
        }
        return "Failed to give points to " + user;
    }

    public String bonusall(int amount, List<String> usersHere, Boolean auto)
    {
        List<String> awarded = new List<String>();
        foreach (String s in usersHere)
        {
            foreach (BotUser botUser in users)
            {
                if (awarded.Contains(s))
                {
                    break;
                }
                if (botUser.username.Equals(s, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (auto)
                    {
                        botUser.time += 1;
                    }
                    if (botUser.points + amount < 0)
                    {
                        botUser.points = 0;
                    }
                    else
                    {
                        botUser.points += amount;
                    }
                    awarded.Add(s);
                    break;
                }
            }
        }
        if (!auto)
        {
            if (amount > -1)
            {
                return "Everyone has been given " + amount + " " + currencyName + "!";
            }
            else
            {
                return amount + " " + currencyName + " has been taken away from everyone!";
            }
        }
        else
        {
            return "";
        }
    }

    public String getCurrency(String user)
    {
        try
        {
            int points = 0, time = 0;
            String result = "";
            BotUser botUser;
            if ((botUser = getBotUser(user)) != null)
            {
                points = botUser.points;
                time = botUser.time;
                if (botUser.rank != null)
                {
                    result = "[" + botUser.rank + "]";
                }
            }
            if (ranks.Count != 0 && (rankupUnitCost == 0 || rankupUnitCost == 2))
            {
                result = "[" + getRankName(points, (time / 60)) + "]";
            }
            return result + " " + user + " has " + points + " " + currencyName + " and "
                    + convertToTime(time.ToString()) + " spent in stream!";
        }
        catch (Exception e)
        {
            Utils.errorReport(e);
            Debug.WriteLine(e.ToString());
        }
        return "An error has occurred trying to retrieve currency values for " + user;
    }

    public String rankup(String user)
    {
        try
        {
            int points = 0;
            String currrank = null;
            BotUser botUser;
            if ((botUser = getBotUser(user)) != null)
            {
                points = botUser.points;
                currrank = botUser.rank;
            }
            String[] arr = null;
            if (currrank != null)
            {
                arr = getNextRankName(ranks[currrank], 0);
            }
            else
            {
                arr = getNextRankName(0, 0);
            }
            if (arr[0].Equals("MAX_VAL"))
            {
                return "You are at max rank, " + user + "!";
            }
            if (arr[0].Equals("unranked"))
            {
                return "There are currently no ranks in this stream.";
            }
            int cost = Int32.Parse(arr[1]);
            if (points >= cost)
            {
                if (!writeToRanks(user, arr[0]))
                {
                    return "Failed to buy rank, please try again later.";
                }
                if (bonus(user, (cost * -1)).StartsWith("Failed"))
                {
                    return "Failed to buy rank, please try again later.";
                }
                return user + " has just purchased the rank '" + arr[0] + "' for " + arr[1] + " " + currencyName + "!";
            }
            else
            {
                return "You cannot afford to purchase this rank, " + user + ". '" + arr[0] + "' costs " + arr[1] + " "
                        + currencyName + "!";
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.ToString());
        }
        return "Failed to buy rank, please try again later.";
    }

    public String getRankName(int points, int hours)
    {
        Dictionary<String, Int32> sorted = sortHashMapByValuesMin(ranks);
        String result = "unranked";
        if (rankupUnitCost == 0)
        {
            foreach (int v in sorted.Values)
            {
                if (points >= v)
                {
                    result = getKeyFromValue(sorted, v);
                }
            }
        }
        else
        {
            foreach (int v in sorted.Values)
            {
                if (hours >= v)
                {
                    result = getKeyFromValue(sorted, v);
                }
            }
        }
        return result;
    }

    public static String getKeyFromValue(Dictionary<String, Int32> hm, int value)
    {
        foreach (String o in hm.Keys)
        {
            if (hm[o].Equals(value))
            {
                return o;
            }
        }
        return null;
    }

    public String[] getNextRankName(int points, int hours) // TODO : TEST!
    {
        Dictionary<String, Int32> sorted = sortHashMapByValuesMin(ranks);
        int cost = 0;
        if (rankupUnitCost == 0 || rankupUnitCost == 1)
        {
            foreach (int v in sorted.Values)
            {
                if (points >= v)
                {
                    cost = v;
                }
            }
        }
        else
        {
            foreach (int v in sorted.Values)
            {
                if (hours >= v)
                {
                    cost = v;
                }
            }
        }
        KeyValuePair<String, Int32>[] list = sorted.ToArray();
        KeyValuePair<String, Int32> a = new KeyValuePair<string, int>();
        for (int i = 0; i < list.Length; i++)
        {
            if (cost == 0)
            {
                break;
            }
            if (list[i].Value == cost)
            {
                if (list.Length - 1 > i)
                {
                    a = list[i + 1];
                }
                else
                {
                    a = new KeyValuePair<string, int>("null", 0);

                }
                break;
            }
        }
        if (a.Key == "null" && a.Value == 0)
        {
            String[] arr = { "MAX_VAL", "0" };
            return arr;
        }
        else
        {
            String[] arr = { a.Key, a.Value.ToString() };
            return arr;
        }
    }

    public String convertToTime(String time)
    {
        int temp = Int32.Parse(time);
        if (temp < 60)
        {
            return temp + " minutes";
        }
        else
        {
            return (temp / 60) + " hours";
        }
    }

    public String gamble(String user, int amount)
    {
        if (amount < 1)
        {
            return "You must gamble at least 1, " + user + "!";
        }
        if (amount > maxGamble)
        {
            return "The max gamble is " + maxGamble + ", " + user + "!";
        }
        try
        {
            int points = -1;
            BotUser botUser;
            if ((botUser = getBotUser(user)) != null)
            {
                points = botUser.points;
                if (botUser.gambleCoolDown != 0)
                {
                    if ((botUser.gambleCoolDown + (gambleCoolDownMinutes * 60000) <= Environment.TickCount))
                    {
                        botUser.gambleCoolDown = 0;
                    }
                    else
                    {
                        return "You may gamble once every " + gambleCoolDownMinutes + " minutes, " + user + "!";
                    }
                }
                if (botUser.gambleCoolDown == 0)
                {
                    if (points < amount)
                    {
                        return "You do not have enough " + currencyName + " to gamble that many, " + user + "!";
                    }
                    int roll = new Random().Next(101) + 1;
                    if (roll < 60)
                    {
                        botUser.points -= amount;
                        botUser.gambleCoolDown = Environment.TickCount;
                        return "Rolled " + roll + ". " + user + " has lost " + amount + " " + currencyName + ". " + user
                                + " now has " + botUser.points + " " + currencyName + ".";
                    }
                    else if (roll <= 98)
                    {
                        botUser.points += amount;
                        botUser.gambleCoolDown = Environment.TickCount;
                        return user + " rolled " + roll + " and has won " + amount + " " + currencyName + ". " + user
                                + " now has " + botUser.points + " " + currencyName + ".";
                    }
                    else
                    {
                        botUser.points += (2 * amount);
                        botUser.gambleCoolDown = Environment.TickCount;
                        return user + " rolled " + roll + " and has won " + (2 * amount) + " " + currencyName + ". "
                                + user + " now has " + botUser.points + " " + currencyName + ".";
                    }
                }
            }
        }
        catch (Exception e)
        {
            Utils.errorReport(e);
            Debug.WriteLine(e.ToString());
        }
        return "Could not gamble right now. Please try again later.";
    }

    public BotUser getBotUser(String username)
    {
        foreach (BotUser botUser in users)
        {
            if (botUser.username.Equals(username, StringComparison.InvariantCultureIgnoreCase))
            {
                return botUser;
            }
        }
        return null;
    }
}