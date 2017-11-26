using System;
using System.Collections.Generic;
using System.IO;

public class RequestSystem
{
    public TwitchBot bot;
    public Boolean vipSongToggle, mustFollowToRequest, requestsTrigger, displayIfUserIsHere, displayOneLine, whisperToUser,
            direquests, ylrequests, maxSongLength, doNotWriteToHistory = false;
    public String subOnlyRequests, lastSong;
    public int maxSonglistLength, numOfSongsToDisplay, numOfSongsInQueuePerUser, maxSongLengthInMinutes;
    public String[] favSongs, bannedKeywords;
    public List<String> favSongsPlayedThisStream = new List<String>();
    public Command requestComm, songlistComm, getTotalComm, editComm, nextComm, addvipComm, addtopComm, adddonatorComm,
            getCurrentComm, clearComm, triggerRequestsComm, backupRequestAddComm, getNextComm, randomComm, favSongComm,
            editSongComm, removeSongComm, songPositionComm;

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
                    bot.sendRawLine(
                            "PRIVMSG " + channel + " :" + "You can only give another user your spot once per stream!");
                    return;
                }
                try
                {
                    StreamReader br = new StreamReader(Utils.songlistfile);
                    String line, line2;
                    int count = 1;
                    Boolean noSong = false;
                    if (Int32.Parse(getNumberOfSongs()) == 0)
                    {
                        bot.sendRawLine(
                                "PRIVMSG " + channel + " :" + "You have no requests in the list, " + sender + "!");
                        return;
                    }
                    String toWrite = "";
                    while ((line = br.ReadLine()) != null)
                    {
                        if (line.Contains(sender))
                        {
                            if (line.StartsWith("$$$"))
                            {
                                toWrite = "$$$\t";
                            }
                            else if (line.StartsWith("VIP\t"))
                            {
                                toWrite = "VIP\t";
                            }
                            noSong = false;
                            line2 = line;
                            break;
                        }
                        else
                        {
                            noSong = true;
                        }
                        count++;
                    }
                    br.Close();
                    if (!noSong)
                    {
                        StreamWriter StreamWriter = new StreamWriter(Utils.templistfile);
                        StreamReader reader = new StreamReader(Utils.songlistfile);
                        for (int i = 0; i < count - 1; i++)
                        {
                            StreamWriter.Write(reader.ReadLine() + "\r");
                        }
                        String newUser = Utils.getFollowingText(message);
                        if (newUser.Contains("@"))
                        {
                            newUser = newUser.Replace("@", "");
                        }
                        StreamWriter.Write(toWrite + "Place Holder\t(" + newUser + ")\r");
                        String previousSong = reader.ReadLine();
                        while ((line2 = reader.ReadLine()) != null)
                        {
                            StreamWriter.Write(line2 + "\r");
                        }
                        StreamWriter.Close();
                        reader.Close();

                        clear(channel, Utils.songlistfile);

                        copyFile(Utils.templistfile, Utils.songlistfile);

                        clear(channel, Utils.templistfile);
                        if (previousSong.StartsWith("$$$\t") || previousSong.StartsWith("VIP\t"))
                        {
                            previousSong = previousSong.Substring(previousSong.IndexOf(' ') + 1);
                        }
                        bot.sendRawLine("PRIVMSG " + channel + " :" + "Your next request '" + previousSong
                                + "' has been changed to 'Place Holder' FOR " + newUser.ToLower() + "!");
                        u.gaveSpot = true;
                        return;
                    }
                    else
                    {
                        bot.sendRawLine("PRIVMSG " + channel + " :" + "You have no regular requests in the list, "
                                + sender + "!");
                        return;
                    }
                }
                catch (Exception e)
                {
                    Utils.errorReport(e);
                    e.ToString();
                }
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
                            bot.sendRawLine("PRIVMSG " + channel + " :"
                                    + "You do not have any requests in the song list, " + sender + "!");
                            return;
                        }
                        for (int j = 0; j < spots.Count; j++)
                        {
                            if (spots.get(j) == 0)
                            {
                                response += "You have a song playing right now, ";
                            }
                            else if (spots.get(j) == 1)
                            {
                                response += "You have a request next in line, ";
                            }
                            else
                            {
                                response += "You have a request in place # " + (spots.get(j) + 1) + ", ";
                            }
                        }
                        bot.sendRawLine(
                                "PRIVMSG " + channel + " :" + "Y" + response.ToLower().Substring(1) + sender + "!");
                    }
                    catch (Exception e)
                    {
                        Utils.errorReport(e);
                        e.ToString();
                    }
                }
            }
        }
    }

    public List<Int32> checkPosition(String message, String channel, String sender)
    {
        List<Int32> songs = new List<Int32>();
        try
        {
            StreamReader br = new StreamReader(Utils.songlistfile);
            String line;
            int count = 0;
            while ((line = br.ReadLine()) != null)
            {
                if (line.Contains("(" + sender + ")"))
                {
                    songs.Add(count);
                }
                count++;
            }
        }
        catch (Exception e)
        {
            Utils.errorReport(e);
            e.ToString();
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
                        if (message.Contains("www.") || message.Contains("http://") || message.cCntains("http://www.")
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
                                        bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid youtube URL, " + sender);
                                        return;
                                    }
                                }
                                catch (Exception e)
                                {
                                    bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid youtube URL, " + sender);
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
                                        bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid youtube URL, " + sender);
                                        return;
                                    }
                                }
                                catch (Exception e)
                                {
                                    bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid youtube URL, " + sender);
                                    return;
                                }
                            }
                            else
                            {
                                bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid Request, " + sender);
                                return;
                            }
                        }
                        if (input.EndsWith(")"))
                        {
                            String requester = input.Substring(input.LastIndexOf("(") + 1, input.Length - 1).Trim();
                            input = input.Substring(0, input.LastIndexOf("(")).Trim();
                            if (ytvid != null)
                            {
                                addDonator(channel, ytvid.getSnippet().getTitle(), requester);
                                bot.sendRawLine(
                                        "PRIVMSG " + channel + " :" + "Donator Song '" + ytvid.getSnippet().getTitle()
                                                + "' has been added to the song list, " + requester + "!");
                            }
                            else
                            {
                                addDonator(channel, input, requester);
                                bot.sendRawLine("PRIVMSG " + channel + " :" + "Donator Song '" + input
                                        + "' has been added to the song list, " + requester + "!");
                            }
                            bot.addUserRequestAmount(requester, true);
                            writeToCurrentSong(channel, false);
                        }
                        else
                        {
                            if (ytvid != null)
                            {
                                addDonator(channel, ytvid.getSnippet().getTitle(), sender);
                                bot.sendRawLine(
                                        "PRIVMSG " + channel + " :" + "Donator Song '" + ytvid.getSnippet().getTitle()
                                                + "' has been added to the song list, " + sender + "!");
                            }
                            else
                            {
                                addDonator(channel, input, sender);
                                bot.sendRawLine("PRIVMSG " + channel + " :" + "Donator Song '" + input
                                        + "' has been added to the song list, " + sender + "!");
                            }
                            writeToCurrentSong(channel, false);
                        }
                    }
                    catch (IOException e)
                    {
                        Utils.errorReport(e);
                        e.ToString();
                    }
                }
            }
        }
    }

    public void addDonator(String channel, String song, String requestedby)
    {
        if (Int32.Parse(getNumberOfSongs()) == 0)
        {
            StreamWriter output;
            output = new StreamWriter(Utils.songlistfile, true);
            output.Write("$$$\t" + song + "\t(" + requestedby + ")\r");
            output.Close();
        }
        else if (Int32.Parse(getNumberOfSongs()) == 1)
        {
            try
            {
                StreamReader br2 = new StreamReader(Utils.templistfile);
                copyToTemp(channel);
                String line2 = br2.ReadLine();
                StreamWriter StreamWriter = new StreamWriter(Utils.songlistfile);
                if (line2.StartsWith("VIP\t"))
                {
                    line2 = line2.Replace("VIP", "$$$");
                    StreamWriter.Write(line2 + "\r");
                }
                else if (line2.StartsWith("$$$\t"))
                {
                    StreamWriter.Write(line2 + "\r");
                }
                else
                {
                    StreamWriter.Write("$$$\t" + line2 + "\r");
                }
                br2.ReadLine();
                StreamWriter.Write("$$$\t" + song + "\t(" + requestedby + ")\r");
                clear(channel, Utils.templistfile);
                br2.Close();
                StreamWriter.Close();
            }
            catch (Exception e)
            {
                Utils.errorReport(e);
                e.ToString();
            }
        }
        else
        {
            try
            {
                StreamReader br3 = new StreamReader(Utils.templistfile);
                copyToTemp(channel);
                String line2, line3, line4;
                line4 = br3.ReadLine();
                br3.Close();
                if (line4.StartsWith("$$$\t"))
                {
                    StreamReader br = new StreamReader(Utils.templistfile);
                    StreamReader br2 = new StreamReader(Utils.templistfile);
                    StreamWriter StreamWriter = new StreamWriter(Utils.songlistfile);
                    int count = 0;
                    while ((line2 = br.ReadLine()) != null)
                    {
                        if (line2.Contains("$$$\t"))
                        {
                            StreamWriter.Write(line2 + "\r");
                            count++;
                        }
                    }
                    StreamWriter.Write("$$$\t" + song + "\t(" + requestedby + ")\r");
                    for (int i = 0; i < count; i++)
                    {
                        br2.ReadLine();
                    }
                    while ((line3 = br2.ReadLine()) != null)
                    {
                        StreamWriter.Write(line3 + "\r");
                    }
                    clear(channel, Utils.templistfile);
                    br.Close();
                    br2.Close();
                    StreamWriter.Close();
                }
                else
                {
                    StreamReader br = new StreamReader(Utils.templistfile);
                    StreamWriter StreamWriter = new StreamWriter(Utils.songlistfile);
                    line2 = br.ReadLine();
                    if (line2.Contains("VIP\t"))
                    {
                        StreamWriter.Write(line2.Replace("VIP", "$$$") + "\r");
                    }
                    else
                    {
                        StreamWriter.Write("$$$\t" + line2 + "\r");
                    }
                    StreamWriter.Write("$$$\t" + song + "\t(" + requestedby + ")\r");
                    while ((line2 = br.ReadLine()) != null)
                    {
                        StreamWriter.Write(line2 + "\r");
                    }
                    clear(channel, Utils.templistfile);
                    br.Close();
                    StreamWriter.Close();
                }
            }
            catch (Exception e)
            {
                Utils.errorReport(e);
                e.ToString();
            }
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
        try
        {
            StreamReader br = new StreamReader(Utils.songlistfile);
            String line, line2;
            int count = 1;
            Boolean noSong = false;
            if (Int32.Parse(getNumberOfSongs()) == 0)
            {
                bot.sendRawLine(
                        "PRIVMSG " + channel + " :" + "You have no regular requests in the list, " + sender + "!");
                return;
            }
            while ((line = br.ReadLine()) != null)
            {
                if (line.Contains("(" + sender + ")"))
                {
                    noSong = false;
                    line2 = line;
                    break;
                }
                else
                {
                    noSong = true;
                }
                count++;
            }
            br.Close();
            if (!noSong)
            {
                StreamWriter StreamWriter = new StreamWriter(Utils.templistfile);
                StreamReader reader = new StreamReader(Utils.songlistfile);
                for (int i = 0; i < count - 1; i++)
                {
                    StreamWriter.Write(reader.ReadLine() + "\r");
                }
                String songToDelete = reader.ReadLine();
                while ((line2 = reader.ReadLine()) != null)
                {
                    StreamWriter.Write(line2 + "\r");
                }
                StreamWriter.Close();
                reader.Close();
                clear(channel, Utils.songlistfile);
                copyFile(Utils.templistfile, Utils.songlistfile);
                clear(channel, Utils.templistfile);
                bot.sendRawLine("PRIVMSG " + channel + " :" + "Your next request '"
                        + songToDelete.Substring(0, songToDelete.LastIndexOf("\t")) + "' has been removed, " + sender
                        + "!");
                bot.addUserRequestAmount(sender, false);
            }
            else
            {
                bot.sendRawLine("PRIVMSG " + channel + " :" + "You have no requests in the list, " + sender + "!");
            }
        }
        catch (Exception e)
        {
            Utils.errorReport(e);
            e.ToString();
        }
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
                    bot.sendRawLine("PRIVMSG " + channel + " :"
                            + "Please type an artist and song name after the command, " + sender);
                }
                else if (temp.StartsWith(editSongComm.input[i]) && temp.Contains(editSongComm.input[i] + " "))
                {
                    if (bannedKeywords != null)
                    {
                        for (int j = 0; j < bannedKeywords.Length; j++)
                        {
                            if (temp.ToLower().Contains(bannedKeywords[j].ToLower()))
                            {
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
    {
        try
        {
            StreamReader br = new StreamReader(Utils.songlistfile);
            String line, line2;
            int count = 1;
            Boolean noSong = false;
            Boolean writeVIP = false, writeDon = false;
            if (Int32.Parse(getNumberOfSongs()) == 0)
            {
                bot.sendRawLine("PRIVMSG " + channel + " :" + "You have no requests in the list, " + sender + "!");
                return;
            }
            while ((line = br.ReadLine()) != null)
            {
                if (line.Contains(sender))
                {
                    if (count == 1)
                    {
                        if (!Utils.checkIfUserIsOP(sender, channel, bot.streamer, bot.users)
                                && !sender.Equals(bot.streamer, StringComparison.InvariantCultureIgnoreCase) && !sender.Equals(Utils.botMaker, StringComparison.InvariantCultureIgnoreCase))
                        {
                            bot.sendRawLine("PRIVMSG " + channel + " :"
                                    + "Your song is currently playing. Please have a mod edit it, " + sender + "!");
                            return;
                        }
                    }
                    if (line.Contains("VIP\t"))
                    {
                        writeVIP = true;
                    }
                    if (line.Contains("$$$\t"))
                    {
                        writeDon = true;
                    }
                    noSong = false;
                    line2 = line;
                    break;
                }
                else
                {
                    noSong = true;
                }
                count++;
            }
            br.Close();
            if (!noSong)
            {
                StreamWriter StreamWriter = new StreamWriter(Utils.templistfile);
                StreamReader reader = new StreamReader(Utils.songlistfile);
                for (int i = 0; i < count - 1; i++)
                {
                    StreamWriter.Write(reader.ReadLine() + "\r");
                }
                if (writeDon)
                {
                    StreamWriter.Write("$$$\t" + Utils.getFollowingText(message) + "\t(" + sender + ")\r");
                }
                else if (writeVIP)
                {
                    StreamWriter.Write("VIP\t" + Utils.getFollowingText(message) + "\t(" + sender + ")\r");
                }
                else
                {
                    StreamWriter.Write(Utils.getFollowingText(message) + "\t(" + sender + ")\r");
                }
                String previousSong = reader.ReadLine();
                while ((line2 = reader.ReadLine()) != null)
                {
                    StreamWriter.Write(line2 + "\r");
                }
                StreamWriter.Close();
                reader.Close();
                clear(channel, Utils.songlistfile);
                copyFile(Utils.templistfile, Utils.songlistfile);
                clear(channel, Utils.templistfile);
                if (previousSong.StartsWith("VIP\t"))
                {
                    previousSong = previousSong.Replace("VIP\t", "");
                }
                if (previousSong.StartsWith("$$$\t"))
                {
                    previousSong = previousSong.Replace("$$$\t", "");
                }
                bot.sendRawLine("PRIVMSG " + channel + " :" + "Your next request '"
                        + previousSong.Substring(0, previousSong.LastIndexOf("\t")) + "' has been changed to '"
                        + Utils.getFollowingText(message) + "', " + sender + "!");
                writeToCurrentSong(channel, false);
            }
            else
            {
                bot.sendRawLine("PRIVMSG " + channel + " :" + "You have no requests in the list, " + sender + "!");
            }
        }
        catch (Exception e)
        {
            Utils.errorReport(e);
            e.ToString();
        }
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
                                                int index = rand.Next(favSongs.Length);
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
                                                e.ToString();
                                            }
                                        }
                                        else
                                        {
                                            addSong(channel, "streamer's Choice", sender);
                                        }
                                    }
                                    else
                                    {
                                        bot.sendRawLine("PRIVMSG " + channel + " :"
                                                + "You must follow the stream to request a song, " + sender);
                                    }
                                }
                                else
                                {
                                    if (numOfSongsInQueuePerUser == 1)
                                    {
                                        bot.sendRawLine("PRIVMSG " + channel + " :"
                                                + "You may only have 1 song in the queue at a time, " + sender + "!");
                                    }
                                    else
                                    {
                                        bot.sendRawLine("PRIVMSG " + channel + " :" + "You may only have "
                                                + numOfSongsInQueuePerUser + " songs in the queue at a time, " + sender
                                                + "!");
                                    }
                                }
                            }
                            catch (IOException e1)
                            {
                                Utils.errorReport(e1);
                                e1.ToString();
                            }
                        }
                        else
                        {
                            bot.sendRawLine("PRIVMSG " + channel + " :" + "Requests are currently off!");
                        }
                    }
                    else
                    {
                        bot.sendRawLine("PRIVMSG " + channel + " :" + "Song limit of " + maxSonglistLength
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
                        bot.sendRawLine("PRIVMSG " + channel + " :" + getNextSongTitle(channel));
                    }
                    catch (Exception e)
                    {
                        Utils.errorReport(e);
                        e.ToString();
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
            String line = "";
            try
            {
                StreamReader br = new StreamReader(Utils.songlistfile);
                br.ReadLine();
                if ((line = br.ReadLine()) != null)
                {
                    if (line.Contains("VIP\t") || line.Contains("$$$\t"))
                    {
                        line = line.Substring(line.IndexOf("\t") + 1, line.Length);
                    }
                    else
                    {
                        line = line.Substring(0, line.Length);
                    }
                }
            }
        return "Next up: " + line;
        }

    }

    public void triggerRequests(Boolean trigger, String channel)
    {
        if (trigger)
        {
            requestsTrigger = true;
            bot.sendRawLine("PRIVMSG " + channel + " :" + "Requests turned on!");
        }
        else
        {
            requestsTrigger = false;
            bot.sendRawLine("PRIVMSG " + channel + " :" + "Requests turned off!");
        }
    }

    public void randomizer(String channel)
    {
        String line, line2, line3 = "";
        Boolean writeVIP = false;
        if (Int32.Parse(getNumberOfSongs()) > 2)
        {
            Random rand = new Random();
            int randInt = rand.Next(Int32.Parse(getNumberOfSongs()) - 1) + 1;
            try
            {
                StreamReader br = new StreamReader(Utils.songlistfile);
                for (int i = 0; i < randInt; i++)
                {
                    line = br.ReadLine();
                    if (i == 0)
                    {
                        line3 = line.Substring(line.IndexOf("("));
                    }
                    if (i == 1 && ((line.StartsWith("VIP\t")) || (line.StartsWith("$$$\t"))))
                    {
                        writeVIP = true;
                    }
                }
                line = br.ReadLine();
                br.Close();
                if (line.Contains(line3))
                {
                    writeVIP = false;
                    randInt = rand.Next(Int32.Parse(getNumberOfSongs()) - 1) + 1;
                    StreamReader secondReader = new StreamReader(Utils.songlistfile);
                    for (int i = 0; i < randInt; i++)
                    {
                        line = secondReader.ReadLine();
                        if (i == 1 && ((line.StartsWith("VIP\t")) || (line.StartsWith("$$$\t"))))
                        {
                            writeVIP = true;
                        }
                    }
                    line = secondReader.ReadLine();
                    secondReader.Close();
                }
                copyToTemp(channel);
                StreamReader br2 = new StreamReader(Utils.templistfile);
                StreamWriter StreamWriter = new StreamWriter(Utils.songlistfile);
                if (writeVIP == true)
                {
                    if (line.StartsWith("VIP\t") || line.StartsWith("$$$\t"))
                    {
                        if (line.StartsWith("$$$\t"))
                        {
                            StreamWriter.Write(line + "\r");
                        }
                        else
                        {
                            String line4 = line.Replace("VIP", "$$$");
                            StreamWriter.Write(line4 + "\r");
                        }
                    }
                    else
                    {
                        StreamWriter.Write("$$$\t" + line + "\r");
                    }
                }
                else
                {
                    if (line.StartsWith("$$$\t"))
                    {
                        StreamWriter.Write(line + "\r");
                    }
                    else
                    {
                        StreamWriter.Write("$$$\t" + line + "\r");
                    }
                }
                br2.ReadLine();
                while ((line2 = br2.ReadLine()) != null)
                {
                    if (!line2.Equals(line))
                    {
                        StreamWriter.Write(line2 + "\r");
                    }
                    else
                    {
                        break;
                    }
                }
                while ((line2 = br2.ReadLine()) != null)
                {
                    StreamWriter.Write(line2 + "\r");
                }
                StreamWriter.Close();
                br2.Close();
                clear(channel, Utils.templistfile);
                bot.sendRawLine("PRIVMSG " + channel + " :" + getNextSong(channel));
            }
            catch (Exception e)
            {
                Utils.errorReport(e);
                e.ToString();
            }
        }
        else
        {
            bot.sendRawLine(
                    "PRIVMSG " + channel + " :" + "Song list must have 3 or more songs to choose a random one!");
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
                        e.ToString();
                    }
                }
            }
        }
    }

    public void writeToCurrentSong(String channel, Boolean nextCom)
    {
        StreamWriter output;
        output = new StreamWriter(Utils.currentSongFile, false);
        String line = getCurrentSongTitle(channel);
        if (line.StartsWith("VIP\t") || line.StartsWith("$$$\t"))
        {
            if (line.Contains("\t"))
            {
                line = line.Substring(line.IndexOf("\t") + 1, line.LastIndexOf("\t"));
            }
        }
        else
        {
            if (line.Contains("\t"))
            {
                line = line.Substring(0, line.IndexOf("\t"));
            }
        }
        output.Write(line);
        output.Close();

        StreamWriter output2;
        output2 = new StreamWriter(Utils.currentRequesterFile, false);
        String line2 = getCurrentSongTitle(channel);
        if (line2.Contains("\t"))
        {
            line2 = line2.Substring(line2.LastIndexOf("\t") + 2, line2.Length - 1);
        }
        if (line2.Equals("Song list is empty"))
        {
            line2 = "";
        }
        output2.Write(line2);
        output2.Close();
        if (bot.spreadsheetId != null)
        {
            bot.google.writeToGoogleSheets(nextCom, Utils.songlistfile, Utils.lastPlayedSongsFile);
        }
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
        try
        {
            StreamWriter output;
            output = new StreamWriter(Utils.lastPlayedSongsFile, true);
            
            output.Write(Utils.getDate() + " " + Utils.getTime() + " - " + lastSong + "\r");
            output.Close();
        }
        catch (Exception e)
        {
            Utils.errorReport(e);
            e.ToString();
        }
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
                        bot.sendRawLine("PRIVMSG " + channel + " :" + "Playing: " + getCurrentSongTitle(channel));
                    }
                    catch (Exception e)
                    {
                        Utils.errorReport(e);
                        e.ToString();
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
                        clear(channel, Utils.songlistfile);
                        bot.sendRawLine("PRIVMSG " + channel + " :" + "Song list has been cleared!");
                        writeToCurrentSong(channel, false);
                    }
                    catch (IOException e)
                    {
                        Utils.errorReport(e);
                        e.ToString();
                    }
                }
            }
        }
    }

    public void decrementUsersOnClear()
    {
        String line;
        int i = 0;
        String user = "";
        try {
            StreamReader br = new StreamReader(Utils.songlistfile);
            while ((line = br.ReadLine()) != null)
            {
                if (i != 0)
                {
                    user = line.Substring(line.LastIndexOf('\t')).Trim();
                    user = user.Replace(")", "");
                    user = user.Replace("(", "");
                    bot.addUserRequestAmount(user, false);
                }
                i++;
            }
            bot.read();
            br.Close();
        } catch (IOException e)
        {
            Utils.errorReport(e);
            e.ToString();
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
                            bot.sendRawLine("PRIVMSG " + channel + " :" + getNextSong(channel));
                        }
                        else
                        {
                            bot.sendRawLine("PRIVMSG " + channel + " :" + "There are no songs currently in the queue!");
                        }

                        writeToCurrentSong(channel, true);
                    }
                    catch (Exception e)
                    {
                        Utils.errorReport(e);
                        e.ToString();
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
            bot.sendRawLine("PRIVMSG " + channel + " :" + "VIP songs turned on!");
        }
        else
        {
            vipSongToggle = false;
            bot.sendRawLine("PRIVMSG " + channel + " :" + "VIP songs turned off!");
        }
    }

    public void nextRegularCOMMAND(String message, String channel, String sender)
    {
        if ((Int32.Parse(getNumberOfSongs()) == 0))
        {
            bot.sendRawLine("PRIVMSG " + channel + " :" + "There are no songs in the list!");
            return;
        }
        else
        {
            copyToTemp(channel);
            StreamReader br = new StreamReader(Utils.templistfile);
            br.ReadLine();
            String line, song = "";
            Boolean check = false;
            while ((line = br.ReadLine()) != null)
            {
                if (!line.StartsWith("$$$\t") && !line.StartsWith("VIP\t"))
                {
                    song = line;
                    check = true;
                    break;
                }
            }
            br.Close();
            if (check)
            {
                clear(channel, Utils.songlistfile);
                StreamWriter StreamWriter = new StreamWriter(Utils.songlistfile);
                StreamWriter.Write("$$$\t" + line + "\r");
                StreamReader br2 = new StreamReader(Utils.templistfile);
                br2.ReadLine();
                while ((line = br2.ReadLine()) != null)
                {
                    if (!line.Equals(song))
                    {
                        StreamWriter.Write(line + "\r");
                    }
                }
                StreamWriter.Close();
                br2.Close();
                bot.sendRawLine("PRIVMSG " + channel + " :" + getNextSong(channel));
                return;
            }
            else
            {
                copyFile(Utils.templistfile, Utils.songlistfile);
                nextSongAuto(channel, true);
                clear(channel, Utils.templistfile);
                return;
            }
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
                    bot.sendRawLine("PRIVMSG " + channel + " :" + "There are no standard requests in the list. "
                            + getNextSong(channel));
                }
                else
                {
                    bot.sendRawLine("PRIVMSG " + channel + " :" + getNextSong(channel));
                }
            }
            else
            {
                bot.sendRawLine("PRIVMSG " + channel + " :" + "There are no songs currently in the queue!");
            }

            writeToCurrentSong(channel, true);
        }
        catch (Exception e)
        {
            Utils.errorReport(e);
            e.ToString();
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
                    bot.sendRawLine("PRIVMSG " + channel + " :"
                            + "Please type an artist and song name after the command, " + sender);
                }
                else if (temp.StartsWith(editComm.input[i]) && temp.Contains(editComm.input[i] + " "))
                {
                    try
                    {
                        if (editCurrent(channel, Utils.getFollowingText(message), sender))
                        {
                            bot.sendRawLine("PRIVMSG " + channel + " :" + "Current song has been edited!");
                            writeToCurrentSong(channel, false);
                        }
                    }
                    catch (IOException e)
                    {
                        Utils.errorReport(e);
                        e.ToString();
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
                                        bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid youtube URL, " + sender);
                                        return;
                                    }
                                }
                                catch (Exception e)
                                {
                                    bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid youtube URL, " + sender);
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
                                        bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid youtube URL, " + sender);
                                        return;
                                    }
                                }
                                catch (Exception e)
                                {
                                    bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid youtube URL, " + sender);
                                    return;
                                }
                            }
                            else
                            {
                                bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid Request, " + sender);
                                return;
                            }
                        }
                        if (input.EndsWith(")"))
                        {
                            String requester = input.Substring(input.LastIndexOf("(") + 1, input.Length - 1).Trim();
                            input = input.Substring(0, input.LastIndexOf("(")).Trim();
                            if (ytvid != null)
                            {
                                addVip(channel, ytvid.getSnippet().getTitle(), requester);
                            }
                            else
                            {
                                addVip(channel, input, requester);
                            }
                            bot.sendRawLine("PRIVMSG " + channel + " :" + "VIP Song '" + input
                                    + "' has been added to the song list, " + requester + "!");
                            bot.addUserRequestAmount(requester, true);
                            writeToCurrentSong(channel, false);
                        }
                        else
                        {
                            if (ytvid != null)
                            {
                                addVip(channel, ytvid.getSnippet().getTitle(), sender);
                                bot.sendRawLine(
                                        "PRIVMSG " + channel + " :" + "VIP Song '" + ytvid.getSnippet().getTitle()
                                                + "' has been added to the song list, " + sender + "!");
                            }
                            else
                            {
                                addVip(channel, input, sender);
                                bot.sendRawLine("PRIVMSG " + channel + " :" + "VIP Song '" + input
                                        + "' has been added to the song list, " + sender + "!");
                            }
                            writeToCurrentSong(channel, false);
                        }
                    }
                    catch (IOException e)
                    {
                        Utils.errorReport(e);
                        e.ToString();
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
                                        bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid youtube URL, " + sender);
                                        return;
                                    }
                                }
                                catch (Exception e)
                                {
                                    bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid youtube URL, " + sender);
                                    return;
                                }
                            }
                            else if (message.Contains("https://youtu.be/"))
                            {
                                youtubeID = message.Substring(message.lastIndexOf("/") + 1);
                                try
                                {
                                    ytvid = bot.youtube.searchYoutubeByID(youtubeID);
                                    if (ytvid == null)
                                    {
                                        bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid youtube URL, " + sender);
                                        return;
                                    }
                                }
                                catch (Exception e)
                                {
                                    bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid youtube URL, " + sender);
                                    return;
                                }
                            }
                            else
                            {
                                bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid Request, " + sender);
                                return;
                            }
                        }
                        if (input.EndsWith(")"))
                        {
                            String requester = input.Substring(input.LastIndexOf("(") + 1, input.Length - 1).Trim();
                            input = input.Substring(0, input.LastIndexOf("(")).Trim();
                            if (ytvid != null)
                            {
                                addTop(channel, ytvid.getSnippet().getTitle(), requester);
                                bot.sendRawLine("PRIVMSG " + channel + " :" + "Song '" + ytvid.getSnippet().getTitle()
                                        + "' has been added to the top of the song list, " + requester + "!");
                            }
                            else
                            {
                                addTop(channel, input, requester);
                                bot.sendRawLine("PRIVMSG " + channel + " :" + "Song '" + input
                                        + "' has been added to the top of the song list, " + requester + "!");
                            }
                            bot.addUserRequestAmount(requester, true);
                            writeToCurrentSong(channel, false);
                        }
                        else
                        {
                            if (ytvid != null)
                            {
                                addTop(channel, ytvid.getSnippet().getTitle(), sender);
                                bot.sendRawLine("PRIVMSG " + channel + " :" + "Song '" + ytvid.getSnippet().getTitle()
                                        + "' has been added to the top of the song list, " + sender + "!");
                            }
                            else
                            {
                                addTop(channel, input, sender);
                                bot.sendRawLine("PRIVMSG " + channel + " :" + "Song '" + input
                                        + "' has been added to the top of the song list, " + sender + "!");
                            }
                            writeToCurrentSong(channel, false);
                        }
                    }
                    catch (IOException e)
                    {
                        Utils.errorReport(e);
                        e.ToString();
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
                        bot.sendRawLine("PRIVMSG " + channel + " :" + "The total number of songs in the queue is: "
                                + getNumberOfSongs());
                    }
                    catch (IOException e)
                    {
                        Utils.errorReport(e);
                        e.ToString();
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
                            bot.sendRawLine("PRIVMSG " + channel + " :" + "The song list is empty!");
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
                            bot.sendRawLine("PRIVMSG " + channel + " :"
                                    + "The full setlist can be found here: https://docs.google.com/spreadsheets/d/"
                                    + bot.spreadsheetId);
                        }
                    }
                    catch (IOException e)
                    {
                        Utils.errorReport(e);
                        e.ToString();
                    }
                }
            }
        }
    }

    public void songlistTimer(String channel)
    {
        if (Int32.Parse(getNumberOfSongs()) == 0)
        {
            bot.sendRawLine("PRIVMSG " + channel + " :" + "The song list is empty!");
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
                    commList = commList.Substring(1, commList.Length);
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
                    bot.sendRawLine("PRIVMSG " + channel + " :" + result);
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
                                bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid Request, " + sender);
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
                                    bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid youtube URL, " + sender);
                                    return;
                                }
                            }
                            catch (Exception e)
                            {
                                bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid youtube URL, " + sender);
                                return;
                            }
                        }
                        else
                        {
                            bot.sendRawLine("PRIVMSG " + channel + " :" + "Invalid Request, " + sender);
                            return;
                        }
                    }
                    if (bannedKeywords != null)
                    {
                        for (int j = 0; j < bannedKeywords.Length; j++)
                        {
                            if (ytvid != null)
                            {
                                temp = ytvid.getSnippet().getTitle();
                            }
                            if (temp.ToLower().contains(bannedKeywords[j].ToLower()))
                            {
                                bot.sendRawLine("PRIVMSG " + channel + " :" + "Song request contains a banned keyword '"
                                        + bannedKeywords[j] + "' and cannot be added, " + sender + "!");
                                return;
                            }
                        }
                    }
                    if (Int32.Parse(getNumberOfSongs()) < maxSonglistLength)
                    {
                        if (requestsTrigger)
                        {
                            if (Utils.getFollowingText(message).length < 100)
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
                                                            time = ytvid.getContentDetails().getDuration();
                                                            temp = ytvid.getSnippet().getTitle();
                                                        }
                                                        else
                                                        {
                                                            Video v = bot.youtube.searchYoutubeByTitle(
                                                                    Utils.getFollowingText(message));
                                                            time = v.getContentDetails().getDuration();
                                                            temp = v.getSnippet().getTitle();
                                                        }
                                                        time = time.Replace("PT", "");
                                                        if (time.Contains("H"))
                                                        {
                                                            bot.sendRawLine("PRIVMSG " + channel + " :" + temp
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
                                                            bot.sendRawLine("PRIVMSG " + channel + " :" + temp
                                                                    + " is longer than " + maxSongLengthInMinutes
                                                                    + " minutes, which is the limit for standard requests, "
                                                                    + sender);
                                                            return;
                                                        }
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        bot.sendRawLine("PRIVMSG " + channel + " :"
                                                                + "Failed to get video, please try again later.");
                                                        System.out.println(e);
                                                        Utils.errorReport(e);
                                                        return;
                                                    }
                                                }
                                                if (ytvid != null)
                                                {
                                                    addSong(channel, ytvid.getSnippet().getTitle(), sender);
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
                                                        bot.sendRawLine("PRIVMSG " + channel + " :"
                                                                + "Only youtube link requests are allowed, " + sender);
                                                    }
                                                }
                                            }
                                            catch (IOException e)
                                            {
                                                Utils.errorReport(e);
                                                e.ToString();
                                            }
                                        }
                                        else
                                        {
                                            bot.sendRawLine("PRIVMSG " + channel + " :"
                                                    + "You must follow the stream to request a song, " + sender);
                                        }
                                    }
                                    else
                                    {
                                        if (numOfSongsInQueuePerUser == 1)
                                        {
                                            bot.sendRawLine("PRIVMSG " + channel + " :"
                                                    + "You may only have 1 song in the queue at a time, " + sender
                                                    + "!");
                                        }
                                        else
                                        {
                                            bot.sendRawLine("PRIVMSG " + channel + " :" + "You may only have "
                                                    + numOfSongsInQueuePerUser + " songs in the queue at a time, "
                                                    + sender + "!");
                                        }
                                    }
                                }
                                catch (IOException e1)
                                {
                                    Utils.errorReport(e1);
                                    e1.ToString();
                                }
                            }
                            else
                            {
                                bot.sendRawLine("PRIVMSG " + channel + " :"
                                        + "Request input too long, please shorten request input, " + sender + "!");
                            }
                        }
                        else
                        {
                            bot.sendRawLine("PRIVMSG " + channel + " :" + "Requests are currently off!");
                        }
                    }
                    else
                    {
                        bot.sendRawLine("PRIVMSG " + channel + " :" + "Song limit of " + maxSonglistLength
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
                        bot.sendRawLine("PRIVMSG " + channel + " :" + subOnlyRequests.Replace("$user", sender));
                    }
                    else
                    {
                        bot.sendRawLine("PRIVMSG " + channel + " :" + subOnlyRequests);
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
        else
        {
            String line = "";
            try {
                StreamReader br = new StreamReader(Utils.songlistfile);
                if ((line = br.ReadLine()) != null)
                {
                    if (line.StartsWith("VIP\t") || line.StartsWith("$$$\t"))
                    {
                        line = line.Substring(line.IndexOf("\t") + 1, line.Length);
                    }
                    else
                    {
                        line = line.Substring(0, line.Length);
                    }
                }
            }
            catch (Exception e)
            {
                Utils.errorReport(e);
                e.ToString();
            }
            return line;
        }
	}

	public void songlist(String channel, String text)
    {
        if (Int32.Parse(getNumberOfSongs()) == 0)
        {
            bot.sendRawLine("PRIVMSG " + channel + " :" + "The song list is empty!");
        }
        if (!displayOneLine)
        {
            try {
                StreamReader br = new StreamReader(Utils.songlistfile);
                String line;
                String temp = "";
                int count = 1;
                bot.sendRawLine("PRIVMSG " + channel + " :" + text);
                while ((line = br.ReadLine()) != null)
                {
                    if (count < numOfSongsToDisplay + 1)
                    {
                        temp = count + ". " + line + " ";
                        bot.sendRawLine("PRIVMSG " + channel + " :" + temp);
                        count++;
                    }
                }
                br.Close();
            }
            catch (Exception e)
            {
                Utils.errorReport(e);
                e.ToString();
            }
        } else {
            String temp2 = "";
            try {
                StreamReader br = new StreamReader(Utils.songlistfile);
                String line;
                String temp = "";
                int count = 1;
                while ((line = br.ReadLine()) != null)
                {
                    if (count < numOfSongsToDisplay + 1)
                    {
                        temp = count + ". " + line + " ";
                        temp2 += temp;
                        count++;
                    }
                }
                bot.sendRawLine("PRIVMSG " + channel + " :" + text + " " + temp2);
                br.Close();
            }
            catch (Exception e)
            {
                Utils.errorReport(e);
                e.ToString();
            }
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
            copyToTemp(channel);
            doNotWriteToHistory = false;
            try {
                StreamReader br = new StreamReader(Utils.templistfile); // TODO
                br.ReadLine();
                String line;
                StreamWriter StreamWriter = new StreamWriter(Utils.songlistfile);
                while ((line = br.ReadLine()) != null)
                {
                    StreamWriter.Write(line + "\r");
                }
                br.Close();
                clear(channel, Utils.templistfile); // TODO
                StreamWriter.Close();
            }
            catch (Exception e)
            {
                Utils.errorReport(e);
                e.ToString();
            }
            return true;
        }   
    }

    public String getNextSong(String channel)
    {
        String line = null, song = null, requestedby = null;
        try {
            StreamReader br = new StreamReader(Utils.songlistfile);
            if ((line = br.ReadLine()) != null)
            {
                if (line.StartsWith("$$$\t") || line.StartsWith("VIP\t"))
                {
                    song = line.Substring(line.IndexOf("\t") + 1, line.LastIndexOf('\t'));
                }
                else
                {
                    song = line.Substring(0, line.LastIndexOf('\t'));
                }
                requestedby = line.Substring(line.LastIndexOf('\t') + 1, line.Length);
            }
        }
        catch (Exception e)
        {
            Utils.errorReport(e);
            e.ToString();
        }
        if (song == null)
        {
            return "There are no songs in the queue!";
        }
        else
        {
            if (displayIfUserIsHere)
            {
                if (bot.checkIfUserIsHere(requestedby, channel))
                {
                    if (whisperToUser)
                    {
                        String toWhisper = requestedby.Substring(1, requestedby.Length - 1);
                        if (!bot.streamer.ToLower().Equals(toWhisper.ToLower()))
                        {
                            bot.sendRawLine("PRIVMSG " + channel + " :/w " + toWhisper + " Your request '" + song
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
    }

    public String getNumberOfSongs()
    {
        try {
            StreamReader br = new StreamReader(Utils.songlistfile);
            int count = 0;
            while ((br.ReadLine()) != null)
            {
                count++;
            }
            return count.ToString();
        }
        catch (Exception e)
        {
            Utils.errorReport(e);
            e.ToString();
        }
        return "0";
    }

    public Boolean editCurrent(String channel, String newSong, String sender)
    {
        String line = null, requestedby = null;
        String prefix = "";
        if ((Int32.Parse(getNumberOfSongs()) == 0))
        {
            StreamWriter output;
            output = new StreamWriter(Utils.songlistfile, true);
            output.Write(newSong + "\t(" + sender + ")\r");
            output.Close();
            bot.sendRawLine("PRIVMSG " + channel + " :" + "Since there are no songs in the song list, song '" + newSong
                    + "' has been added to the song list, " + sender + "!");
            writeToCurrentSong(channel, false);
            return false;
        }
        try {
            StreamReader br = new StreamReader(Utils.songlistfile);
            if ((line = br.ReadLine()) != null)
            {
                if (line.StartsWith("VIP\t"))
                {
                    prefix = "VIP\t";
                }
                else if (line.StartsWith("$$$\t"))
                {
                    prefix = "$$$\t";
                }
                requestedby = line.Substring(line.LastIndexOf('\t') + 1, line.Length);
            }
        }
        catch (Exception e)
        {
            Utils.errorReport(e);
            e.ToString();
        }
        copyToTemp(channel);
        try {
            StreamReader br = new StreamReader(Utils.templistfile); // TODO
            br.ReadLine();
            String line2;
            StreamWriter StreamWriter = new StreamWriter(Utils.songlistfile);
            if (prefix.Equals(""))
            {
                StreamWriter.Write(newSong + "\t" + requestedby + "\r");
            }
            else
            {
                StreamWriter.Write(prefix + newSong + "\t" + requestedby + "\r");
            }
            while ((line2 = br.ReadLine()) != null)
            {
                StreamWriter.Write(line2 + "\r");
            }
            clear(channel, Utils.templistfile); // TODO
            StreamWriter.Close();
            br.Close();
        }
        catch (Exception e)
        {
            Utils.errorReport(e);
            e.ToString();
        }
        writeToCurrentSong(channel, false);
        return true;
    }
    
    public Boolean checkIfUserAlreadyHasSong(String user)
    {
        if (user.Equals(bot.streamer))
        {
            return false;
        }
        try {
            StreamReader br = new StreamReader(Utils.songlistfile);
            String line;
            int count = 0;
            while ((line = br.ReadLine()) != null)
            {
                if (line.Contains("\t(" + user) && (!line.StartsWith("$$$\t") && (!line.StartsWith("VIP\t"))))
                {
                    count++;
                }
            }
            if (count >= numOfSongsInQueuePerUser)
            {
                return true;
            }
            br.Close();
        }
        catch (Exception e)
        {
            Utils.errorReport(e);
            e.ToString();
        }
        return false;
    }

    public void addSong(String channel, String song, String requestedby)
    {
        StreamWriter output;
        output = new StreamWriter(Utils.songlistfile, true);
        output.Write(song + "\t(" + requestedby + ")\r");
        output.Close();
        bot.sendRawLine("PRIVMSG " + channel + " :" + "Song '" + song + "' has been added to the song list, "
                + requestedby + "!");
        bot.addUserRequestAmount(requestedby, true);
        writeToCurrentSong(channel, false);
    }

    public void addTop(String channel, String song, String requestedby)
    {
        copyToTemp(channel);
        try {
            StreamReader br = new StreamReader(Utils.templistfile); // TODO
            StreamReader br2 = new StreamReader(Utils.templistfile); // TODO
            String line2;
            StreamWriter StreamWriter = new StreamWriter(Utils.songlistfile);
            StreamWriter.Write("$$$\t" + song + "\t(" + requestedby + ")\r");
            while ((line2 = br.ReadLine()) != null)
            {
                StreamWriter.Write(line2 + "\r");
            }
            clear(channel, Utils.templistfile); // TODO
            br.Close();
            br2.Close();
            StreamWriter.Close();
        }
        catch (Exception e)
        {
            Utils.errorReport(e);
            e.ToString();
        }
    }

    public void addVip(String channel, String song, String requestedby)
    {
        if (Int32.Parse(getNumberOfSongs()) == 0)
        {
            StreamWriter output;
            output = new StreamWriter(Utils.songlistfile, true);
            output.Write("VIP\t" + song + "\t(" + requestedby + ")\r");
            output.Close();
        }
        else if (Int32.Parse(getNumberOfSongs()) == 1)
        {
            try {
                StreamReader br = new StreamReader(Utils.templistfile); // TODO
                copyToTemp(channel);
                StreamReader br2 = new StreamReader(Utils.templistfile); // TODO
                String line2 = br2.ReadLine();
                StreamWriter StreamWriter = new StreamWriter(Utils.songlistfile);
                if (line2.StartsWith("VIP\t") || line2.StartsWith("$$$\t"))
                {
                    StreamWriter.Write(line2 + "\r");
                }
                else
                {
                    StreamWriter.Write("VIP\t" + line2 + "\r");
                }
                br2.ReadLine();
                StreamWriter.Write("VIP\t" + song + "\t(" + requestedby + ")\r");
                clear(channel, Utils.templistfile); // TODO
                br.Close();
                br2.Close();
                StreamWriter.Close();
            }
            catch (Exception ioe)
            {
                Utils.errorReport(ioe);
                ioe.ToString();
            }
        }
        else
        {
            try {
                StreamReader br3 = new StreamReader(Utils.templistfile); // TODO
                String line2, line4;
                copyToTemp(channel);
                line4 = br3.ReadLine();
                br3.Close();
                if (line4.StartsWith("$$$\t"))
                {
                    StreamReader br4 = new StreamReader(Utils.templistfile); // TODO
                    StreamWriter writer2 = new StreamWriter(Utils.songlistfile);
                    while ((line2 = br4.ReadLine()) != null)
                    {
                        if (line2.StartsWith("$$$\t"))
                        {
                            writer2.Write(line2 + "\r");
                        }
                        else
                        {
                            break;
                        }
                    }
                    // CHECK IF NEXT NON $$$ IS VIP
                    if (line2.StartsWith("VIP\t"))
                    {
                        // IF YES : WRITE ALL VIP, WRITE SONG, WRITE REMAINING
                        if (line2 != null && line2.Contains("VIP\t"))
                        {
                            writer2.Write(line2 + "\r");
                        }
                        while ((line2 = br4.ReadLine()) != null)
                        {
                            if (line2.Contains("VIP\t"))
                            {
                                writer2.Write(line2 + "\r");
                            }
                            else
                            {
                                break;
                            }
                        }
                        writer2.Write("VIP\t" + song + "\t(" + requestedby + ")\r");
                        if (line2 != null)
                        {
                            writer2.Write(line2 + "\r");
                        }
                        while ((line2 = br4.ReadLine()) != null)
                        {
                            writer2.Write(line2 + "\r");
                        }
                    }
                    else
                    {
                        // IF NOT : WRITE SONG, WRITE REMAINING SONGS
                        writer2.Write("VIP\t" + song + "\t(" + requestedby + ")\r");

                        if (line2 != null)
                        {
                            writer2.Write(line2 + "\r");
                        }
                        while ((line2 = br4.ReadLine()) != null)
                        {
                            writer2.Write(line2 + "\r");
                        }
                    }
                    br4.Close();
                    writer2.Close();
                    clear(channel, Utils.templistfile); // TODO
                }
                else if (line4.StartsWith("VIP\t"))
                {
                    StreamReader br4 = new StreamReader(Utils.templistfile); // TODO
                    StreamWriter writer2 = new StreamWriter(Utils.songlistfile);
                    while ((line2 = br4.ReadLine()) != null)
                    {
                        if (line2.StartsWith("VIP\t"))
                        {
                            writer2.Write(line2 + "\r");
                        }
                        else
                        {
                            break;
                        }
                    }
                    writer2.Write("VIP\t" + song + "\t(" + requestedby + ")\r");
                    if (line2 != null)
                    {
                        writer2.Write(line2 + "\r");
                    }
                    while ((line2 = br4.ReadLine()) != null)
                    {
                        writer2.Write(line2 + "\r");
                    }
                    br4.Close();
                    writer2.Close();
                    clear(channel, Utils.templistfile); // TODO
                }
                else
                {
                    StreamReader br = new StreamReader(Utils.templistfile); // TODO
                    StreamWriter StreamWriter = new StreamWriter(Utils.songlistfile);
                    StreamWriter.Write("VIP\t" + br.ReadLine() + "\r");
                    StreamWriter.Write("VIP\t" + song + "\t(" + requestedby + ")\r");
                    while ((line2 = br.ReadLine()) != null)
                    {
                        StreamWriter.Write(line2 + "\r");
                    }
                    clear(channel, Utils.templistfile); // TODO
                    br.Close();
                    StreamWriter.Close();
                }
            }
            catch (Exception ioe)
            {
                Utils.errorReport(ioe);
                ioe.ToString();
            }
        }
    }

}
