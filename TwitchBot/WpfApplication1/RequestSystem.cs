using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

public class RequestSystem
{
    [JsonIgnore]
    public TwitchBot bot;
    public Boolean vipSongToggle { get; set; }
    public Boolean mustFollowToRequest { get; set; }
    public Boolean requestsTrigger { get; set; }
    public Boolean displayIfUserIsHere { get; set; }
    public Boolean displayOneLine { get; set; }
    public Boolean whisperToUser { get; set; }
    public Boolean direquests { get; set; }
    public Boolean ylrequests { get; set; }
    public Boolean maxSongLength { get; set; }
    public Boolean doNotWriteToHistory { get; set; } = false;
    public String subOnlyRequests { get; set; }
    public String lastSong { get; set; }
    public String formattedTotalTime { get; set; } = "";
    public int maxSonglistLength { get; set; }
    public int numOfSongsToDisplay { get; set; }
    public int numOfSongsInQueuePerUser { get; set; }
    public int maxSongLengthInMinutes { get; set; }
    public List<String> favSongs { get; set; }
    public String[] bannedKeywords { get; set; }
    [JsonIgnore]
    public List<String> favSongsPlayedThisStream { get; set; } = new List<String>();
    public List<String> songHistory { get; set; } = new List<String>();
    public Command requestComm { get; set; }
    public Command songlistComm { get; set; }
    public Command getTotalComm { get; set; }
    public Command editComm { get; set; }
    public Command nextComm { get; set; }
    public Command addvipComm { get; set; }
    public Command addtopComm { get; set; }
    public Command adddonatorComm { get; set; }
    public Command getCurrentComm { get; set; }
    public Command clearComm { get; set; }
    public Command triggerRequestsComm { get; set; }
    public Command backupRequestAddComm { get; set; }
    public Command getNextComm { get; set; }
    public Command randomComm { get; set; }
    public Command favSongComm { get; set; }
    public Command editSongComm { get; set; }
    public Command removeSongComm { get; set; }
    public Command songPositionComm { get; set; }
    public List<Song> songList { get; set; } = new List<Song>();

    public int getNumRequests(String sender)
    {
        foreach (BotUser user in bot.users)
        {
            if (user.username.Equals(sender, StringComparison.InvariantCultureIgnoreCase))
            {
                return user.numRequests;
            }
        }
        return 0;
    }

    public void giveSpot(String message, String channel, String sender)
    {
        foreach (BotUser u in bot.users)
        {
            if (u.username.Equals(sender, StringComparison.InvariantCultureIgnoreCase) && !sender.Equals(bot.streamer, StringComparison.InvariantCultureIgnoreCase))
            {
                if (u.gaveSpot)
                {
                    bot.client.SendMessage("You can only give another user your spot once per stream!");
                    return;
                }
                if (Int32.Parse(getNumberOfSongs()) == 0)
                {
                    bot.client.SendMessage("You have no requests in the list, " + sender + "!");
                    return;
                }
                for (int i = 0; i < songList.Count; i++)
                {
                    if (songList[i].requester.Equals(sender, StringComparison.InvariantCultureIgnoreCase))
                    {
                        String newUser = Utils.getFollowingText(message);
                        if (newUser.Contains("@"))
                        {
                            newUser = newUser.Replace("@", "");
                        }
                        String previousSong = songList[i].name;
                        songList[i].requester = newUser;
                        songList[i].name = "Place Holder";
                        bot.client.SendMessage("Your next request '" + previousSong
                           + "' has been changed to 'Place Holder' FOR " + newUser.ToLower() + "!");
                        u.gaveSpot = true;
                        return;
                    }
                }
                bot.client.SendMessage("You have no regular requests in the list, "
                        + sender + "!");
                return;
            }
        }
    }

    public void checkSongPositionCOMMAND(String message, String channel, String sender)
    {
        if (bot.checkUserLevel(sender, songPositionComm, channel))
        {
            for (int i = 0; i < songPositionComm.input.Length; i++)
            {
                String temp = message.ToLower();
                if (temp.Equals(songPositionComm.input[i], StringComparison.InvariantCultureIgnoreCase))
                {
                    try
                    {
                        String response = "";
                        List<Int32> spots = checkPosition(message, channel, sender);
                        if (spots.Count < 1)
                        {
                            bot.client.SendMessage("You do not have any requests in the song list, " + sender + "!");
                            return;
                        }
                        for (int j = 0; j < spots.Count; j++)
                        {
                            if (spots[j] == 0)
                            {
                                response += "You have a song playing right now, ";
                            }
                            else if (spots[j] == 1)
                            {
                                response += "You have a request next in line, ";
                            }
                            else
                            {
                                response += "You have a request in place # " + (spots[j] + 1) + ", ";
                            }
                        }
                        bot.client.SendMessage(response.ToLower().Substring(1) + sender + "!");
                    }
                    catch (Exception e)
                    {
                        Utils.errorReport(e);
                        Debug.WriteLine(e.ToString());
                    }
                }
            }
        }
    }

    public List<Int32> checkPosition(String message, String channel, String sender)
    {
        List<Int32> songs = new List<Int32>();
        for (int i = 0; i < songList.Count; i++)
        {
            if (songList[i].requester.Equals(sender, StringComparison.InvariantCultureIgnoreCase))
            {
                songs.Add(i);
            }
        }
        return songs;
    }

    public void addDonatorCOMMAND(String message, String channel, String sender)
    {
        if (bot.checkUserLevel(sender, adddonatorComm, channel))
        {
            for (int i = 0; i < adddonatorComm.input.Length; i++)
            {
                String temp = message.ToLower();
                if (temp.StartsWith(adddonatorComm.input[i]) && temp.Contains(adddonatorComm.input[i] + " "))
                {
                    try
                    {
                        String input = Utils.getFollowingText(message);
                        String youtubeID = null;
                        Video ytvid = null;
                        if (message.Contains("www.") || message.Contains("http://") || message.Contains("http://www.")
                                || message.Contains(".com") || message.Contains("https://"))
                        {
                            if (message.Contains("www.youtube.com/watch?v=")
                                    || message.Contains("www.youtube.com/watch?v="))
                            {
                                youtubeID = message.Substring(message.IndexOf("=") + 1);
                                try
                                {
                                    ytvid = bot.youtube.searchYoutubeByID(youtubeID);
                                    if (ytvid == null)
                                    {
                                        bot.client.SendMessage("Invalid youtube URL, " + sender);
                                        return;
                                    }
                                }
                                catch (Exception)
                                {
                                    bot.client.SendMessage("Invalid youtube URL, " + sender);
                                    return;
                                }
                            }
                            else if (message.Contains("https://youtu.be/"))
                            {
                                youtubeID = message.Substring(message.LastIndexOf("/") + 1);
                                try
                                {
                                    ytvid = bot.youtube.searchYoutubeByID(youtubeID);
                                    if (ytvid == null)
                                    {
                                        bot.client.SendMessage("Invalid youtube URL, " + sender);
                                        return;
                                    }
                                }
                                catch (Exception)
                                {
                                    bot.client.SendMessage("Invalid youtube URL, " + sender);
                                    return;
                                }
                            }
                            else
                            {
                                bot.client.SendMessage("Invalid Request, " + sender);
                                return;
                            }
                        }
                        if (input.EndsWith(")"))
                        {
                            String requester = input.Substring(input.LastIndexOf("(") + 1, input.Length - 1).Trim();
                            input = input.Substring(0, input.LastIndexOf("(")).Trim();
                            if (ytvid != null)
                            {
                                addDonator(channel, ytvid.Snippet.Title, requester);
                                bot.client.SendMessage("Donator Song '" + ytvid.Snippet.Title
                                                + "' has been added to the song list, " + requester + "!");
                            }
                            else
                            {
                                addDonator(channel, input, requester);
                                bot.client.SendMessage("Donator Song '" + input
                                        + "' has been added to the song list, " + requester + "!");
                            }
                            bot.addUserRequestAmount(requester, true);
                            writeToCurrentSong(channel, false);
                        }
                        else
                        {
                            if (ytvid != null)
                            {
                                addDonator(channel, ytvid.Snippet.Title, sender);
                                bot.client.SendMessage("Donator Song '" + ytvid.Snippet.Title
                                                + "' has been added to the song list, " + sender + "!");
                            }
                            else
                            {
                                addDonator(channel, input, sender);
                                bot.client.SendMessage("Donator Song '" + input
                                        + "' has been added to the song list, " + sender + "!");
                            }
                            writeToCurrentSong(channel, false);
                        }
                    }
                    catch (IOException e)
                    {
                        Utils.errorReport(e);
                        Debug.WriteLine(e.ToString());
                    }
                }
            }
        }
    }

    public void addDonator(String channel, String song, String requestedby)
    {
        if (Int32.Parse(getNumberOfSongs()) == 0)
        {
            songList.Add(new Song(song, requestedby, "$$$", bot));
        }
        else if (Int32.Parse(getNumberOfSongs()) == 1)
        {
            songList[0].level = "$$$";
            songList.Add(new Song(song, requestedby, "$$$", bot));
        }
        else
        {
            for (int i = 1; i < songList.Count; i++)
            {
                if (!songList[i].level.Equals("$$$"))
                {
                    songList.Insert(i, new Song(song, requestedby, "$$$", bot));
                    return;
                }
            }
            songList.Add(new Song(song, requestedby, "$$$", bot));
        }
    }

    public void removeMySong(String message, String channel, String sender)
    {
        if (bot.checkUserLevel(sender, removeSongComm, channel))
        {
            for (int i = 0; i < removeSongComm.input.Length; i++)
            {
                String temp = message.ToLower();
                if (temp.Equals(removeSongComm.input[i], StringComparison.InvariantCultureIgnoreCase))
                {
                    removeRequesterSong(message, channel, sender);
                }
            }
        }
    }

    public void removeRequesterSong(String message, String channel, String sender)
    {
        if (Int32.Parse(getNumberOfSongs()) == 0)
        {
            bot.client.SendMessage("You have no regular requests in the list, " + sender + "!");
            return;
        }
        for (int i = 0; i < songList.Count; i++)
        {
            if (songList[i].requester.Equals(sender, StringComparison.InvariantCultureIgnoreCase))
            {
                String songToDelete = songList[i].name;
                songList.RemoveAt(i);
                bot.client.SendMessage("Your next request '"
                    + songToDelete.Substring(0, songToDelete.LastIndexOf("\t")) + "' has been removed, " + sender
                    + "!");
                bot.addUserRequestAmount(sender, false);
                return;
            }
        }
        bot.client.SendMessage("You have no requests in the list, " + sender + "!");
    }

    public void editMySongCOMMAND(String message, String channel, String sender)
    {
        if (bot.checkUserLevel(sender, editSongComm, channel))
        {
            for (int i = 0; i < editSongComm.input.Length; i++)
            {
                String temp = message.ToLower();
                if (temp.Equals(editSongComm.input[i], StringComparison.InvariantCultureIgnoreCase))
                {
                    bot.client.SendMessage("Please type an artist and song name after the command, " + sender);
                }
                else if (temp.StartsWith(editSongComm.input[i]) && temp.Contains(editSongComm.input[i] + " "))
                {
                    if (bannedKeywords != null)
                    {
                        for (int j = 0; j < bannedKeywords.Length; j++)
                        {
                            if (temp.ToLower().Contains(bannedKeywords[j].ToLower()))
                            {
                                bot.client.SendMessage("Song request contains a banned keyword '"
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
    {
        if (Int32.Parse(getNumberOfSongs()) == 0)
        {
            bot.client.SendMessage("You have no requests in the list, " + sender + "!");
            return;
        }
        for (int i = 0; i < songList.Count; i++)
        {
            if (songList[i].requester.Equals(sender, StringComparison.InvariantCultureIgnoreCase))
            {
                if (i == 1)
                {
                    if (!Utils.checkIfUserIsOP(sender, channel, bot.streamer, bot.users)
                        && !sender.Equals(bot.streamer, StringComparison.InvariantCultureIgnoreCase) && !sender.Equals(Utils.botMaker, StringComparison.InvariantCultureIgnoreCase))
                    {
                        bot.client.SendMessage("Your song is currently playing. Please have a mod edit it, " + sender + "!");
                        return;
                    }
                }
                songList[i].name = Utils.getFollowingText(message);
                return;
            }
        }
        bot.client.SendMessage("You have no requests in the list, " + sender + "!");
    }

    public void chooseRandomFavorite(String message, String channel, String sender)
    {
        if (bot.checkUserLevel(sender, favSongComm, channel))
        {
            for (int i = 0; i < favSongComm.input.Length; i++)
            {
                if (message.Equals(favSongComm.input[i], StringComparison.InvariantCultureIgnoreCase))
                {
                    if (Int32.Parse(getNumberOfSongs()) < maxSonglistLength)
                    {
                        if (requestsTrigger)
                        {
                            try
                            {
                                Boolean check = true;
                                if (!checkIfUserAlreadyHasSong(sender))
                                {
                                    if (mustFollowToRequest)
                                    {
                                        if (!Utils.checkIfUserIsFollowing(channel, sender, bot.streamer, bot.users))
                                        {
                                            check = false;
                                        }
                                    }
                                    if (check)
                                    {
                                        if (favSongs != null)
                                        {
                                            try
                                            {
                                                Random rand = new Random();
                                                int index = rand.Next(favSongs.Count);
                                                if (favSongsPlayedThisStream.Contains(favSongs[index]))
                                                {
                                                    addSong(channel, "streamer's Choice", sender);
                                                }
                                                else
                                                {
                                                    addSong(channel, favSongs[index], sender);
                                                }
                                                favSongsPlayedThisStream.Add(favSongs[index]);
                                            }
                                            catch (IOException e)
                                            {
                                                Utils.errorReport(e);
                                                Debug.WriteLine(e.ToString());
                                            }
                                        }
                                        else
                                        {
                                            addSong(channel, "streamer's Choice", sender);
                                        }
                                    }
                                    else
                                    {
                                        bot.client.SendMessage("You must follow the stream to request a song, " + sender);
                                    }
                                }
                                else
                                {
                                    if (numOfSongsInQueuePerUser == 1)
                                    {
                                        bot.client.SendMessage("You may only have 1 song in the queue at a time, " + sender + "!");
                                    }
                                    else
                                    {
                                        bot.client.SendMessage("You may only have "
                                                + numOfSongsInQueuePerUser + " songs in the queue at a time, " + sender
                                                + "!");
                                    }
                                }
                            }
                            catch (IOException e1)
                            {
                                Utils.errorReport(e1);
                                Debug.WriteLine(e1.ToString());
                            }
                        }
                        else
                        {
                            bot.client.SendMessage("Requests are currently off!");
                        }
                    }
                    else
                    {
                        bot.client.SendMessage("Song limit of " + maxSonglistLength
                                + " has been reached, please try again later.");
                    }
                }
            }
        }
    }

    public void getNextSongCOMMAND(String message, String channel, String sender)
    {
        if (bot.checkUserLevel(sender, getNextComm, channel))
        {
            for (int i = 0; i < getNextComm.input.Length; i++)
            {
                if (message.Equals(getNextComm.input[i], StringComparison.InvariantCultureIgnoreCase))
                {
                    try
                    {
                        bot.client.SendMessage(getNextSongTitle(channel));
                    }
                    catch (Exception e)
                    {
                        Utils.errorReport(e);
                        Debug.WriteLine(e.ToString());
                    }
                }
            }
        }
    }

    public String getNextSongTitle(String channel)
    {
        if (Int32.Parse(getNumberOfSongs()) < 2)
        {
            return "There is no next song in the song list!";
        }
        else
        {
            return "Next up: " + songList[1].name;
        }
    }

    public void triggerRequests(Boolean trigger, String channel)
    {
        if (trigger)
        {
            requestsTrigger = true;
            bot.client.SendMessage("Requests turned on!");
        }
        else
        {
            requestsTrigger = false;
            bot.client.SendMessage("Requests turned off!");
        }
    }

    public void randomizer(String channel)
    {
        if (Int32.Parse(getNumberOfSongs()) > 2)
        {
            Random rand = new Random();
            int randInt = rand.Next(Int32.Parse(getNumberOfSongs()) - 1) + 1;
            Song s = songList[randInt];
            songList.RemoveAt(randInt);
            if (songList[1].level.Equals("VIP"))
            {
                s.level = "VIP";
            }
            if (songList[1].level.Equals("$$$"))
            {
                s.level = "$$$";
            }
            songList[0] = s;
            bot.client.SendMessage(getNextSong(channel));
        }
        else
        {
            bot.client.SendMessage("Song list must have 3 or more songs to choose a random one!");
        }
    }

    public void randomizerCommand(String message, String channel, String sender)
    {
        if (bot.checkUserLevel(sender, randomComm, channel))
        {
            for (int i = 0; i < randomComm.input.Length; i++)
            {
                if (message.Equals(randomComm.input[i], StringComparison.InvariantCultureIgnoreCase))
                {
                    try
                    {
                        randomizer(channel);
                        writeToCurrentSong(channel, false);
                    }
                    catch (Exception e)
                    {
                        Utils.errorReport(e);
                        Debug.WriteLine(e.ToString());
                    }
                }
            }
        }
    }

    public void writeToCurrentSong(String channel, Boolean nextCom)
    {
        for (int i = 1; i <= songList.Count; i++)
        {
            songList[i-1].index = i;
        }
        StreamWriter output;
        output = new StreamWriter(Utils.currentSongFile, false);
        output.Write(getCurrentSongTitle(channel));
        output.Close();
        output = new StreamWriter(Utils.currentRequesterFile, false);
        output.Write(getCurrentSongRequester(channel));
        output.Close();
        if (bot.spreadsheetId != null)
        {
            bot.google.writeToGoogleSheets(nextCom, songList, songHistory);
        }
        formattedTotalTime = formatTotalTime();
        Utils.saveSongs(songList);
    }

    public String formatTotalTime()
    {
        int totalSeconds = 0;
        foreach (Song s in songList)
        {
            totalSeconds += s.durationInSeconds;
        }
        return TimeSpan.FromSeconds(totalSeconds).ToString(@"hh\:mm\:ss");
    }

    public void clear(String channel, String file)
    {
        StreamWriter output;
        output = new StreamWriter(file);
        output.Write("");
        output.Close();
        writeToCurrentSong(channel, false);
    }

    public void appendToLastSongs(String channel, String lastSong)
    {
        songHistory.Add(Utils.getDate() + " " + Utils.getTime() + " - " + lastSong + "\r");
    }

    public void getCurrentSongCOMMAND(String message, String channel, String sender)
    {
        if (bot.checkUserLevel(sender, getCurrentComm, channel))
        {
            for (int i = 0; i < getCurrentComm.input.Length; i++)
            {
                if (message.Equals(getCurrentComm.input[i], StringComparison.InvariantCultureIgnoreCase))
                {
                    try
                    {
                        bot.client.SendMessage("Playing: " + getCurrentSongTitle(channel));
                    }
                    catch (Exception e)
                    {
                        Utils.errorReport(e);
                        Debug.WriteLine(e.ToString());
                    }
                }
            }
        }
    }

    public void triggerRequestsCOMMAND(String message, String channel, String sender)
    {
        if (bot.checkUserLevel(sender, triggerRequestsComm, channel))
        {
            for (int i = 0; i < triggerRequestsComm.input.Length; i++)
            {
                String temp = message.ToLower();
                if (temp.StartsWith(triggerRequestsComm.input[i])
                        && temp.Contains(triggerRequestsComm.input[i] + " "))
                {
                    if (Utils.getFollowingText(message).Contains("on"))
                    {
                        triggerRequests(true, channel);
                    }
                    else if (Utils.getFollowingText(message).Contains("off"))
                    {
                        triggerRequests(false, channel);
                    }
                }
            }
        }
    }

    public void clearCOMMAND(String message, String channel, String sender)
    {
        if (bot.checkUserLevel(sender, clearComm, channel))
        {
            for (int i = 0; i < clearComm.input.Length; i++)
            {
                String temp = message.ToLower();
                if (temp.Equals(clearComm.input[i]))
                {
                    try
                    {
                        clear(channel, Utils.songListFile);
                        songList.Clear();
                        bot.client.SendMessage("Song list has been cleared!");
                        writeToCurrentSong(channel, false);
                    }
                    catch (IOException e)
                    {
                        Utils.errorReport(e);
                        Debug.WriteLine(e.ToString());
                    }
                }
            }
        }
    }

    public void decrementUsersOnClear()
    {
        for (int i = 0; i < songList.Count; i++)
        {
            if (i != 0)
            {
                bot.addUserRequestAmount(songList[i].requester, false);
            }
        }
    }

    public void nextCOMMAND(String message, String channel, String sender)
    {
        if (bot.checkUserLevel(sender, nextComm, channel))
        {
            for (int i = 0; i < nextComm.input.Length; i++)
            {
                String temp = message.ToLower();
                if (temp.Equals(nextComm.input[i]))
                {
                    try
                    {
                        if (nextSong(channel))
                        {
                            bot.client.SendMessage(getNextSong(channel));
                        }
                        else
                        {
                            bot.client.SendMessage("There are no songs currently in the queue!");
                        }

                        writeToCurrentSong(channel, true);
                    }
                    catch (Exception e)
                    {
                        Utils.errorReport(e);
                        Debug.WriteLine(e.ToString());
                    }
                }
            }
        }
    }

    public void nextRegular(String message, String channel, String sender)
    {
        nextRegularCOMMAND(message, channel, sender);
        writeToCurrentSong(channel, true);
    }

    public void triggerVIPs(Boolean trigger, String channel)
    {
        if (trigger)
        {
            vipSongToggle = true;
            bot.client.SendMessage("VIP songs turned on!");
        }
        else
        {
            vipSongToggle = false;
            bot.client.SendMessage("VIP songs turned off!");
        }
    }

    public void nextRegularCOMMAND(String message, String channel, String sender)
    {
        if ((Int32.Parse(getNumberOfSongs()) == 0))
        {
            bot.client.SendMessage("There are no songs in the list!");
            return;
        }
        else
        {
            for (int i = 0; i < songList.Count; i++)
            {
                if (songList[i].level.Equals(""))
                {
                    Song s = songList[i];
                    songList.RemoveAt(i);
                    s.level = "$$$";
                    songList.Insert(0, s);
                    bot.client.SendMessage(getNextSong(channel));
                    return;
                }
            }
            nextSongAuto(channel, true);
        }
    }

    public void nextSongAuto(String channel, Boolean check)
    {
        try
        {
            if (nextSong(channel))
            {
                if (check)
                {
                    bot.client.SendMessage("There are no standard requests in the list. "
                            + getNextSong(channel));
                }
                else
                {
                    bot.client.SendMessage(getNextSong(channel));
                }
            }
            else
            {
                bot.client.SendMessage("There are no songs currently in the queue!");
            }

            writeToCurrentSong(channel, true);
        }
        catch (Exception e)
        {
            Utils.errorReport(e);
            Debug.WriteLine(e.ToString());
        }
    }

    public void editCOMMAND(String message, String channel, String sender)
    {
        if (bot.checkUserLevel(sender, editComm, channel))
        {
            for (int i = 0; i < editComm.input.Length; i++)
            {
                String temp = message.ToLower();
                if (temp.Equals(editComm.input[i], StringComparison.InvariantCultureIgnoreCase))
                {
                    bot.client.SendMessage("Please type an artist and song name after the command, " + sender);
                }
                else if (temp.StartsWith(editComm.input[i]) && temp.Contains(editComm.input[i] + " "))
                {
                    try
                    {
                        if (editCurrent(channel, Utils.getFollowingText(message), sender))
                        {
                            bot.client.SendMessage("Current song has been edited!");
                            writeToCurrentSong(channel, false);
                        }
                    }
                    catch (IOException e)
                    {
                        Utils.errorReport(e);
                        Debug.WriteLine(e.ToString());
                    }
                }
            }
        }
    }

    public void addvipCOMMAND(String message, String channel, String sender)
    {
        if (bot.checkUserLevel(sender, addvipComm, channel))
        {
            for (int i = 0; i < addvipComm.input.Length; i++)
            {
                String temp = message.ToLower();
                if (temp.StartsWith(addvipComm.input[i]) && temp.Contains(addvipComm.input[i] + " "))
                {
                    try
                    {
                        String input = Utils.getFollowingText(message);
                        String youtubeID = null;
                        Video ytvid = null;
                        if (message.Contains("www.") || message.Contains("http://") || message.Contains("http://www.")
                                || message.Contains(".com") || message.Contains("https://"))
                        {
                            if (message.Contains("www.youtube.com/watch?v=")
                                    || message.Contains("www.youtube.com/watch?v="))
                            {
                                youtubeID = message.Substring(message.IndexOf("=") + 1);
                                try
                                {
                                    ytvid = bot.youtube.searchYoutubeByID(youtubeID);
                                    if (ytvid == null)
                                    {
                                        bot.client.SendMessage("Invalid youtube URL, " + sender);
                                        return;
                                    }
                                }
                                catch (Exception)
                                {
                                    bot.client.SendMessage("Invalid youtube URL, " + sender);
                                    return;
                                }
                            }
                            else if (message.Contains("https://youtu.be/"))
                            {
                                youtubeID = message.Substring(message.LastIndexOf("/") + 1);
                                try
                                {
                                    ytvid = bot.youtube.searchYoutubeByID(youtubeID);
                                    if (ytvid == null)
                                    {
                                        bot.client.SendMessage("Invalid youtube URL, " + sender);
                                        return;
                                    }
                                }
                                catch (Exception)
                                {
                                    bot.client.SendMessage("Invalid youtube URL, " + sender);
                                    return;
                                }
                            }
                            else
                            {
                                bot.client.SendMessage("Invalid Request, " + sender);
                                return;
                            }
                        }
                        if (input.EndsWith(")"))
                        {
                            String requester = input.Substring(input.LastIndexOf("(") + 1, input.Length - 1).Trim();
                            input = input.Substring(0, input.LastIndexOf("(")).Trim();
                            if (ytvid != null)
                            {
                                addVip(channel, ytvid.Snippet.Title, requester);
                            }
                            else
                            {
                                addVip(channel, input, requester);
                            }
                            bot.client.SendMessage("VIP Song '" + input
                                    + "' has been added to the song list, " + requester + "!");
                            bot.addUserRequestAmount(requester, true);
                            writeToCurrentSong(channel, false);
                        }
                        else
                        {
                            if (ytvid != null)
                            {
                                addVip(channel, ytvid.Snippet.Title, sender);
                                bot.client.SendMessage("VIP Song '" + ytvid.Snippet.Title
                                                + "' has been added to the song list, " + sender + "!");
                            }
                            else
                            {
                                addVip(channel, input, sender);
                                bot.client.SendMessage("VIP Song '" + input
                                        + "' has been added to the song list, " + sender + "!");
                            }
                            writeToCurrentSong(channel, false);
                        }
                    }
                    catch (IOException e)
                    {
                        Utils.errorReport(e);
                        Debug.WriteLine(e.ToString());
                    }
                }
            }
        }
    }

    public void addtopCOMMAND(String message, String channel, String sender)
    {
        if (bot.checkUserLevel(sender, addtopComm, channel))
        {
            for (int i = 0; i < addtopComm.input.Length; i++)
            {
                String temp = message.ToLower();
                if (temp.StartsWith(addtopComm.input[i]) && temp.Contains(addtopComm.input[i] + " "))
                {
                    try
                    {
                        String input = Utils.getFollowingText(message);
                        String youtubeID = null;
                        Video ytvid = null;
                        if (message.Contains("www.") || message.Contains("http://") || message.Contains("http://www.")
                                || message.Contains(".com") || message.Contains("https://"))
                        {
                            if (message.Contains("www.youtube.com/watch?v=")
                                    || message.Contains("www.youtube.com/watch?v="))
                            {
                                youtubeID = message.Substring(message.IndexOf("=") + 1);
                                try
                                {
                                    ytvid = bot.youtube.searchYoutubeByID(youtubeID);
                                    if (ytvid == null)
                                    {
                                        bot.client.SendMessage("Invalid youtube URL, " + sender);
                                        return;
                                    }
                                }
                                catch (Exception)
                                {
                                    bot.client.SendMessage("Invalid youtube URL, " + sender);
                                    return;
                                }
                            }
                            else if (message.Contains("https://youtu.be/"))
                            {
                                youtubeID = message.Substring(message.LastIndexOf("/") + 1);
                                try
                                {
                                    ytvid = bot.youtube.searchYoutubeByID(youtubeID);
                                    if (ytvid == null)
                                    {
                                        bot.client.SendMessage("Invalid youtube URL, " + sender);
                                        return;
                                    }
                                }
                                catch (Exception)
                                {
                                    bot.client.SendMessage("Invalid youtube URL, " + sender);
                                    return;
                                }
                            }
                            else
                            {
                                bot.client.SendMessage("Invalid Request, " + sender);
                                return;
                            }
                        }
                        if (input.EndsWith(")"))
                        {
                            String requester = input.Substring(input.LastIndexOf("(") + 1, input.Length - 1).Trim();
                            input = input.Substring(0, input.LastIndexOf("(")).Trim();
                            if (ytvid != null)
                            {
                                addTop(channel, ytvid.Snippet.Title, requester);
                                bot.client.SendMessage("Song '" + ytvid.Snippet.Title
                                        + "' has been added to the top of the song list, " + requester + "!");
                            }
                            else
                            {
                                addTop(channel, input, requester);
                                bot.client.SendMessage("Song '" + input
                                        + "' has been added to the top of the song list, " + requester + "!");
                            }
                            bot.addUserRequestAmount(requester, true);
                            writeToCurrentSong(channel, false);
                        }
                        else
                        {
                            if (ytvid != null)
                            {
                                addTop(channel, ytvid.Snippet.Title, sender);
                                bot.client.SendMessage("Song '" + ytvid.Snippet.Title
                                        + "' has been added to the top of the song list, " + sender + "!");
                            }
                            else
                            {
                                addTop(channel, input, sender);
                                bot.client.SendMessage("Song '" + input
                                        + "' has been added to the top of the song list, " + sender + "!");
                            }
                            writeToCurrentSong(channel, false);
                        }
                    }
                    catch (IOException e)
                    {
                        Utils.errorReport(e);
                        Debug.WriteLine(e.ToString());
                    }
                }
            }
        }
    }

    public void getTotalSongCOMMAND(String message, String channel, String sender)
    {
        if (bot.checkUserLevel(sender, getTotalComm, channel))
        {
            for (int i = 0; i < getTotalComm.input.Length; i++)
            {
                String temp = message.ToLower();
                if (temp.Equals(getTotalComm.input[i]))
                {
                    try
                    {
                        bot.client.SendMessage("The total number of songs in the queue is: "
                                + getNumberOfSongs());
                    }
                    catch (IOException e)
                    {
                        Utils.errorReport(e);
                        Debug.WriteLine(e.ToString());
                    }
                }
            }
        }
    }

    public void songlistCOMMAND(String message, String channel, String sender)
    {
        if (bot.checkUserLevel(sender, songlistComm, channel))
        {
            for (int i = 0; i < songlistComm.input.Length; i++)
            {
                String temp = message.ToLower();
                if (temp.Equals(songlistComm.input[i]))
                {
                    try
                    {
                        if (Int32.Parse(getNumberOfSongs()) == 0)
                        {
                            bot.client.SendMessage("The song list is empty!");
                        }
                        else if (numOfSongsToDisplay > Int32.Parse(getNumberOfSongs()))
                        {
                            if (Int32.Parse(getNumberOfSongs()) == 1)
                            {
                                String text = "The next song in the song list: ";
                                songlist(channel, text);
                            }
                            else
                            {
                                String text = "The next " + Int32.Parse(getNumberOfSongs())
                                        + " songs in the song list: ";
                                songlist(channel, text);
                            }
                        }
                        else
                        {
                            String text = "The next " + numOfSongsToDisplay + " songs in the song list: ";
                            songlist(channel, text);
                        }
                        if (bot.spreadsheetId != null)
                        {
                            bot.client.SendMessage("The full setlist can be found here: https://docs.google.com/spreadsheets/d/"
                                    + bot.spreadsheetId);
                        }
                    }
                    catch (IOException e)
                    {
                        Utils.errorReport(e);
                        Debug.WriteLine(e.ToString());
                    }
                }
            }
        }
    }

    public void songlistTimer(String channel)
    {
        if (Int32.Parse(getNumberOfSongs()) == 0)
        {
            bot.client.SendMessage("The song list is empty!");
        }
        else if (numOfSongsToDisplay > Int32.Parse(getNumberOfSongs()))
        {
            if (Int32.Parse(getNumberOfSongs()) == 1)
            {
                String text = "The next song in the song list: ";
                songlist(channel, text);
            }
            else
            {
                String text = "The next " + Int32.Parse(getNumberOfSongs()) + " songs in the song list: ";
                songlist(channel, text);
            }
        }
        else
        {
            String text = "The next " + numOfSongsToDisplay + " songs in the song list: ";
            songlist(channel, text);
        }
    }

    public void requestCOMMAND(String message, String channel, String sender)
    {
        if (bot.checkUserLevel(sender, requestComm, channel))
        {
            for (int i = 0; i < requestComm.input.Length; i++)
            {
                String temp = message.ToLower();
                String commList = "";
                for (int j = 0; j < requestComm.input.Length; j++)
                {
                    commList += ", " + String.Join("", requestComm.input[j]);
                }
                if (!commList.Equals(""))
                {
                    commList = commList.Substring(1, commList.Length - 1);
                }
                if (temp.Equals(requestComm.input[i]))
                {
                    String result = "";
                    if (mustFollowToRequest)
                    {
                        result += "You must be following the stream to request. ";
                    }
                    if (ylrequests && direquests)
                    {
                        result = "To request a song, type: " + commList + " [artist - song] OR [youtube link]";
                    }
                    else if (ylrequests)
                    {
                        result = "To request a song, type: " + commList + " [youtube link]";
                    }
                    else if (direquests)
                    {
                        result = "To request a song, type: " + commList + " [artist - song]";
                    }
                    bot.client.SendMessage(result);
                }
                else if (temp.StartsWith(requestComm.input[i]) && temp.Contains(requestComm.input[i] + " "))
                {
                    String youtubeID = null;
                    Video ytvid = null;
                    if (message.Contains("www.") || message.Contains("http://") || message.Contains("http://www.")
                            || message.Contains(".com") || message.Contains("https://"))
                    {
                        if (message.Contains("www.youtube.com/watch?v=") || message.Contains("www.youtube.com/watch?v=")
                                || message.Contains("https://youtu.be/"))
                        {
                            if (!ylrequests)
                            {
                                bot.client.SendMessage("Invalid Request, " + sender);
                                return;
                            }
                            if (message.Contains("https://youtu.be/"))
                            {
                                youtubeID = message.Substring(message.LastIndexOf("/") + 1);
                            }
                            else
                            {
                                youtubeID = message.Substring(message.IndexOf("=") + 1);
                            }
                            try
                            {
                                ytvid = bot.youtube.searchYoutubeByID(youtubeID);
                                if (ytvid == null)
                                {
                                    bot.client.SendMessage("Invalid youtube URL, " + sender);
                                    return;
                                }
                            }
                            catch (Exception)
                            {
                                bot.client.SendMessage("Invalid youtube URL, " + sender);
                                return;
                            }
                        }
                        else
                        {
                            bot.client.SendMessage("Invalid Request, " + sender);
                            return;
                        }
                    }
                    if (bannedKeywords != null)
                    {
                        for (int j = 0; j < bannedKeywords.Length; j++)
                        {
                            if (ytvid != null)
                            {
                                temp = ytvid.Snippet.Title;
                            }
                            if (temp.ToLower().Contains(bannedKeywords[j].ToLower()))
                            {
                                bot.client.SendMessage("Song request contains a banned keyword '"
                                        + bannedKeywords[j] + "' and cannot be added, " + sender + "!");
                                return;
                            }
                        }
                    }
                    if (Int32.Parse(getNumberOfSongs()) < maxSonglistLength)
                    {
                        if (requestsTrigger)
                        {
                            if (Utils.getFollowingText(message).Length < 100)
                            {
                                try
                                {
                                    Boolean check = true;
                                    if (!checkIfUserAlreadyHasSong(sender))
                                    {
                                        if (mustFollowToRequest)
                                        {
                                            if (!Utils.checkIfUserIsFollowing(channel, sender, bot.streamer,
                                                    bot.users))
                                            {
                                                check = false;
                                            }
                                        }
                                        if (check)
                                        {
                                            try
                                            {
                                                if (maxSongLength)
                                                {
                                                    try
                                                    {
                                                        String time;
                                                        if (ytvid != null)
                                                        {
                                                            time = ytvid.ContentDetails.Duration;
                                                            temp = ytvid.Snippet.Title;
                                                        }
                                                        else
                                                        {
                                                            Video v = bot.youtube.searchYoutubeByTitle(
                                                                    Utils.getFollowingText(message));
                                                            time = v.ContentDetails.Duration;
                                                            temp = v.Snippet.Title;
                                                        }
                                                        time = time.Replace("PT", "");
                                                        if (time.Contains("H"))
                                                        {
                                                            bot.client.SendMessage(temp
                                                                    + " is longer than " + maxSongLengthInMinutes
                                                                    + " minutes, which is the limit for standard requests, "
                                                                    + sender);
                                                            return;
                                                        }
                                                        int minutes = Int32
                                                                .Parse(time.Substring(0, time.IndexOf('M')));
                                                        int seconds = Int32.Parse(time
                                                                .Substring(time.IndexOf('M') + 1, time.IndexOf('S')));
                                                        int songlengthmaxseconds = maxSongLengthInMinutes * 60;
                                                        if (songlengthmaxseconds < ((minutes * 60) + seconds))
                                                        {
                                                            bot.client.SendMessage(temp
                                                                    + " is longer than " + maxSongLengthInMinutes
                                                                    + " minutes, which is the limit for standard requests, "
                                                                    + sender);
                                                            return;
                                                        }
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        bot.client.SendMessage("Failed to get video, please try again later.");
                                                        Debug.WriteLine(e);
                                                        Utils.errorReport(e);
                                                        return;
                                                    }
                                                }
                                                if (ytvid != null)
                                                {
                                                    addSong(channel, ytvid.Snippet.Title, sender);
                                                    return;
                                                }
                                                else
                                                {
                                                    if (direquests)
                                                    {
                                                        addSong(channel, Utils.getFollowingText(message), sender);
                                                    }
                                                    else
                                                    {
                                                        bot.client.SendMessage("Only youtube link requests are allowed, " + sender);
                                                    }
                                                }
                                            }
                                            catch (IOException e)
                                            {
                                                Utils.errorReport(e);
                                                Debug.WriteLine(e.ToString());
                                            }
                                        }
                                        else
                                        {
                                            bot.client.SendMessage("You must follow the stream to request a song, " + sender);
                                        }
                                    }
                                    else
                                    {
                                        if (numOfSongsInQueuePerUser == 1)
                                        {
                                            bot.client.SendMessage("You may only have 1 song in the queue at a time, " + sender
                                                    + "!");
                                        }
                                        else
                                        {
                                            bot.client.SendMessage("You may only have "
                                                    + numOfSongsInQueuePerUser + " songs in the queue at a time, "
                                                    + sender + "!");
                                        }
                                    }
                                }
                                catch (IOException e1)
                                {
                                    Utils.errorReport(e1);
                                    Debug.WriteLine(e1.ToString());
                                }
                            }
                            else
                            {
                                bot.client.SendMessage("Request input too long, please shorten request input, " + sender + "!");
                            }
                        }
                        else
                        {
                            bot.client.SendMessage("Requests are currently off!");
                        }
                    }
                    else
                    {
                        bot.client.SendMessage("Song limit of " + maxSonglistLength
                                + " has been reached, please try again later.");
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < requestComm.input.Length; i++)
            {
                String temp = message.ToLower();
                String commList = "";
                for (int j = 0; j < requestComm.input.Length; j++)
                {
                    commList += ", " + String.Join("", requestComm.input[j]);
                }
                if (!commList.Equals(""))
                {
                    commList = commList.Substring(1, commList.Length);
                }
                if (temp.StartsWith(requestComm.input[i]) && temp.Contains(requestComm.input[i] + " "))
                {
                    if (subOnlyRequests.Contains("$user"))
                    {
                        bot.client.SendMessage(subOnlyRequests.Replace("$user", sender));
                    }
                    else
                    {
                        bot.client.SendMessage(subOnlyRequests);
                    }
                }
            }
        }
    }

    public String getCurrentSongTitle(String channel)
    {
        if (Int32.Parse(getNumberOfSongs()) == 0)
        {
            return "Song list is empty";
        }
        return songList[0].name;
    }

    public String getCurrentSongRequester(String channel)
    {
        if (Int32.Parse(getNumberOfSongs()) == 0)
        {
            return "Song list is empty";
        }
        return songList[0].requester;
    }

    public void songlist(String channel, String text)
    {
        if (Int32.Parse(getNumberOfSongs()) == 0)
        {
            bot.client.SendMessage("The song list is empty!");
        }
        if (!displayOneLine)
        {
            for (int i = 0; i < songList.Count; i++)
            {
                if (i < numOfSongsToDisplay)
                {
                    bot.client.SendMessage((i + 1) + ". " + songList[i].level + " " + songList[i].name + " (" + songList[i].requester + ")");
                }
            }
        }
        else
        {
            String temp = "";
            for (int i = 0; i < songList.Count; i++)
            {
                if (i < numOfSongsToDisplay)
                {
                    temp += (i + 1) + ". " + songList[i].level + " " + songList[i].name + " (" + songList[i].requester + ") ";
                }
            }
            bot.client.SendMessage(text + " " + temp);
        }
    }

    public Boolean nextSong(String channel)
    {
        if ((Int32.Parse(getNumberOfSongs()) == 0))
        {
            return false;
        }
        else
        {
            if (!doNotWriteToHistory)
            {
                lastSong = getCurrentSongTitle(channel);
                appendToLastSongs(channel, lastSong);
            }
            doNotWriteToHistory = false;
            songList.RemoveAt(0);
            return true;
        }
    }

    public String getNextSong(String channel)
    {
        if (Int32.Parse(getNumberOfSongs()) == 0)
        {
            return "There are no songs in the queue!";
        }
        String song = songList[0].name;
        String requestedby = songList[0].requester;
        if (displayIfUserIsHere)
        {
            if (bot.checkIfUserIsHere(requestedby, channel))
            {
                if (whisperToUser)
                {
                    String toWhisper = requestedby.Substring(1, requestedby.Length - 1);
                    if (!bot.streamer.ToLower().Equals(toWhisper.ToLower()))
                    {
                        bot.client.SendMessage("/w " + toWhisper + " Your request '" + song
                                + "' is being played next in " + bot.streamer + "'s stream!");
                    }
                }
                return "The next song is: '" + song + "' - " + requestedby + " HERE! :) ";
            }
            else
            {
                return "The next song is: '" + song + "' - " + requestedby;
            }
        }
        else
        {
            return "The next song is: '" + song + "' - " + requestedby;
        }

    }

    public String getNumberOfSongs()
    {
        return songList.Count.ToString();
    }

    public Boolean editCurrent(String channel, String newSong, String sender)
    {
        if ((Int32.Parse(getNumberOfSongs()) == 0))
        {
            songList.Add(new Song(newSong, sender, "", bot));
            bot.client.SendMessage("Since there are no songs in the song list, song '" + newSong
                    + "' has been added to the song list, " + sender + "!");
            writeToCurrentSong(channel, false);
            return false;
        }
        songList[0].name = newSong;
        writeToCurrentSong(channel, false);
        return true;
    }

    public Boolean checkIfUserAlreadyHasSong(String user)
    {
        if (user.Equals(bot.streamer))
        {
            return false;
        }
        int count = 0;
        for (int i = 0; i < songList.Count; i++)
        {
            if (songList[i].requester.Equals(user, StringComparison.InvariantCultureIgnoreCase) && (songList[i].level.Equals("")))
            {
                count++;
            }
        }
        if (count >= numOfSongsInQueuePerUser)
        {
            return true;
        }
        return false;
    }

    public void removeSongCOMMAND(String sender, String channel, String streamer, ObservableCollection<BotUser> users, String message, String temp)
    {
        if (sender.Equals(streamer) || Utils.checkIfUserIsOP(sender, channel, streamer, users)
                        || sender.Equals(Utils.botMaker))
        {
            int number = -1;
            try
            {
                number = Int32.Parse(Utils.getFollowingText(message));
                if (Int32.Parse(getNumberOfSongs()) == 0)
                {
                    bot.client.SendMessage("The song list is empty, " + sender + "!");
                    return;
                }
                if (Int32.Parse(getNumberOfSongs()) < number || number == 0)
                {
                    bot.client.SendMessage("Song #" + number + " does not exist, " + sender + "!");
                    return;
                }
            }
            catch
            {
                bot.client.SendMessage("To remove a song, it must be in the form '!removesong #'");
                return;
            }
            songList.RemoveAt(number);
            return;
        }
    }

    public void promoteSongCommand(String sender, String channel, String streamer, ObservableCollection<BotUser> users, String message)
    {
        if (sender.Equals(streamer) || Utils.checkIfUserIsOP(sender, channel, streamer, users)
                    || sender.Equals(Utils.botMaker))
        {
            String user = Utils.getFollowingText(message);
            if (user.Contains("@"))
            {
                user = user.Replace("@", "");
            }
            for (int j = 0; j < songList.Count; j++)
            {
                Song s = songList[j];
                if (s.requester.Equals(user, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (s.level.Equals("VIP"))
                    {
                        songList.RemoveAt(j);
                        addDonator(channel, s.name, user);
                        bot.client.SendMessage("VIP Song '" + s.name
                                + "' has been promoted to $$$, " + sender + "!");
                        return;
                    }
                    else if (s.level.Equals("$$$"))
                    {
                        bot.client.SendMessage("Cannot promote a $$$ song any higher, " + sender
                                + "!");
                        return;
                    }
                    else
                    {
                        songList.RemoveAt(j);
                        bot.requestSystem.addVip(channel, s.name, user);
                        bot.client.SendMessage("Song '" + s.name + "' has been promoted to VIP, "
                                + sender + "!");
                        return;
                    }
                }
            }
            bot.client.SendMessage(user + " does not have a song in the list, "
                    + sender + "!");
            return;
        }
    }

    public void addSong(String channel, String song, String requestedby)
    {
        songList.Add(new Song(song, requestedby, "", bot));
        bot.client.SendMessage("Song '" + song + "' has been added to the song list, "
                + requestedby + "!");
        bot.addUserRequestAmount(requestedby, true);
        writeToCurrentSong(channel, false);
    }

    public void insertSong(String song, String requestedby, int place)
    {
        String level = "";
        if (songList.Count > place)
        {
            level = songList[place].level;
        }
        if (place >= songList.Count)
        {
            songList.Add(new Song(song, requestedby, level, bot));
        }
        else
        {
            songList.Insert(place, new Song(song, requestedby, level, bot));
        }
        writeToCurrentSong(bot.channel, true);
    }

    public void addTop(String channel, String song, String requestedby)
    {
        songList.Insert(0, new Song(song, requestedby, "$$$", bot));
    }

    public void addVip(String channel, String song, String requestedby)
    {
        if (Int32.Parse(getNumberOfSongs()) == 0)
        {
            songList.Insert(0, new Song(song, requestedby, "VIP", bot));
        }
        else if (Int32.Parse(getNumberOfSongs()) == 1)
        {
            songList[0].level = "VIP";
            songList.Add(new Song(song, requestedby, "VIP", bot));
        }
        else
        {
            for (int i = 1; i < songList.Count; i++)
            {
                Song s = songList[i];
                if (s.level.Equals(""))
                {
                    songList.Insert(i, new Song(song, requestedby, "VIP", bot));
                    return;
                }
            }
            songList.Add(new Song(song, requestedby, "VIP", bot));
        }

    }

    public void addCurrentSongToFavList()
    {
        if (songList.Count == 0)
        {
            bot.client.SendMessage("There are no songs in the queue!");
            return;
        }
        String song = songList[0].name;
        song = song.Replace(',', ' ');
        favSongsPlayedThisStream.Add(song);
        favSongs.Add(song);
        bot.client.SendMessage(song + " has been added to the favorite songs list!");
    }


}
