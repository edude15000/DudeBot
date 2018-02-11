using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System.Threading;
using System.Collections.ObjectModel;
using NHunspell;
using Newtonsoft.Json.Linq;

public class Utils
{
    public static String version = "3.4.0"; // UPDATE AS NECESSARY
    public static String releaseDate = "2/11/2018"; // UPDATE AS NECESSARY
    public static String twitchClientID = "c203ik5st5i3kde6amsageei2snaj1v";
    public static String botMaker = "edude15000";
    public static String currentSongFile = @"bin\currentsong.txt";
    public static String currentRequesterFile = @"bin\currentrequester.txt";
    public static String userDataFile = @"bin\userData.json";
    public static String songListFile = @"bin\songList.json";
    public static String readmeFile = "README.txt";
    public static String notesFile = @"bin\notes.txt";
    public static String songListTextFile = @"bin\song.txt";
    public static String googleApiKey = "AIzaSyDU4bPym2G64rrPgk7B9a5L6LWtIyLhFQg";
    public static String customsForgeAuthKey = "880ea6a14ea49e853634fbdc5015a024";
    public static String cleverbotIOuser = "kwTO92eMLLzv5o7G";
    public static String cleverbotIOkey = "woNIQmZTDYfvm0uAg2QWyumoNfKjH37h";
    public static String modCommandsLink = "https://docs.google.com/document/d/1gc-Vabwssk6ekx3PpTVZvswmA5QEczX7WuKvnXX5AV0/edit?usp=sharing";
    public static String accuracyFile = @"bin\output\accuracy.txt";
    public static String backupUserDataFile = Path.GetTempPath() + @"\userData.json";

    public static List<String> genericEmoteList = new List<String>
    {
        
    };

    public static Dictionary<string, string> bandAcronyms = new Dictionary<string, string>
    {
        { "NLT", "Night Like This" },
        { "ABR", "August Burns Red" },
        { "ADTR", "A Day To Remember" },
        { "BMTH", "Bring Me The Horizon" },
        { "BFMV", "Bullet For My Valentine" },
        { "SOAD", "System of a Down" },
        { "A7X", "Avenged Sevenfold" },
        { "OMAM", "Of Mice & Men" },
        { "OM&M", "Of Mice & Men" },
        { "FFDP", "Five Finger Death Punch" },
        { "5FDP", "Five Finger Death Punch" },
        { "RHCP", "Red Hot Chili Peppers" },
        { "BTBAM", "Between The Buried And Me" },
        { "ELO", "Electric Light Orchestra" },
        { "RATM", "Rage Against The Machine" },
        { "GNR", "Guns Roses" },
        { "Guns N' Roses", "Guns Roses" },
        { "G'N'R", "Guns Roses" },
        { "NIN", "Nine Inch Nails" },
        { "MCR", "My Chemical Romance" },
        { "APC", "A Perfect Circle" },
        { "CCR", "Creedence Clearwater Revival" },
        { "ETF", "Escape the Fate" },
        { "3EB", "Third Eye Blind" },
        { "TEB", "Third Eye Blind" },
        { "KSE", "Killswitch Engage" },
        { "AILD", "As I Lay Dying" },
        { "PTH", "Protest The Hero" },
        { "WSS", "While She Sleeps" },
        { "INK", "Ice Nine Kills" },
        { "I9K", "Ice Nine Kills" },
        { "AA", "Asking Alexandria" },
        { "ACDC", "AC/DC" },
        { "MMI", "Miss May I" },
        { "QOTSA", "Queens of the Stone Age" },
        { "BOC", "Blue Oyster Cult" },
        { "STP", "Stone Temple Pilots" },
        { "BTO", "Bachman Turner Overdive" },
        { "SYL", "Strapping Young Lad" },
        { "CC", "Cannibal Corpse" },
        { "AE", "Arch Enemy" },
        { "DSO", "Diablo Swing Orchestra" },
        { "NeO", "Ne Obliviscaris" },
        { "TSO", "Trans Siberian Orchestra" },
        { "DGD", "Dance Gavin Dance" },
        { "TFOT", "The Fall Of Troy" },
        { "LTE", "Liquid Tension Experiment" },
        { "CTE", "Cage The Elephant" },
        { "CoCa", "Coheed & Cambria" },
        { "TTFAF", "Through The Fire And Flames" },
        { "DF", "Dragon Force" }

        // ADD AS NEEEDED
    };

    public static String replaceAcronyms(String str)
    {
        foreach (KeyValuePair<string, string> acronymPair in bandAcronyms)
        {
            if (str.StartsWith(acronymPair.Key + " ") || str.EndsWith(acronymPair.Key + " "))
            {
                str = str.Replace(acronymPair.Key, acronymPair.Value);
                break;
            }
        }
        return str;
    }

    public static String getChannelID(String channel)
    {
        String info = callURL("https://api.twitch.tv/kraken/channels/" + channel);
        var a = JsonConvert.DeserializeObject<dynamic>(info);
        try
        {
            return a["_id"];
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static string autoCorrect(String song)
    {
        String[] wordList = song.Split(' ');
        try
        {
            using (Hunspell hunspell = new Hunspell("bin/en_us.aff", "bin/en_us.dic"))
            {
                for (int i = 0; i < wordList.Length; i++)
                {
                    if (!hunspell.Spell(wordList[i]))
                    {
                        List<string> suggestions = hunspell.Suggest(wordList[i]);
                        if (suggestions.Count > 0)
                        {
                            wordList[i] = suggestions[0];
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            errorReport(e);
            Debug.WriteLine(e.ToString());
        }
        return string.Join(" ", wordList);
    }

    public static String formatSongTitle(String str)
    {
        if (!str.ToLower().Contains("ac/dc"))
        {
            str = str.Replace("/", " ");
        }
        str = str.Replace("\"", " ").Replace("'", " ").Replace(",", " ").Replace(".", " ").Replace(")", " ").Replace("(", " ").Replace("[", " ").Replace("]", " ");
        str = str.Replace(@"\", " ").Replace("!", " ").Replace("?", " ").Replace("@", " ").Replace("#", " ").Replace("$", " ").Replace("%", " ");
        str = str.Replace("^", " ").Replace("&", " ").Replace("*", " ").Replace("=", " ").Replace("-", " ").Replace("_", " ").Replace("+", " ").Replace("~", " ");
        str = str.Replace("`", " ").Replace(">", " ").Replace("<", " ").Replace("|", " ").Replace(":", " ").Replace(";", " ");
        return str.ToLower();
    }

    public static double getMinsRemaining(double tracker, double timeInMins)
    {
        return Math.Abs(Math.Ceiling(((DateTimeOffset.Now.ToUnixTimeMilliseconds() - (tracker + (timeInMins * 60000))) / 60000)));
    }

    public static int getDurationOfVideoInSeconds(String time)
    {
        TimeSpan youTubeDuration = System.Xml.XmlConvert.ToTimeSpan(time);
        return (youTubeDuration.Hours * 3600) + (youTubeDuration.Minutes * 60) + youTubeDuration.Seconds;
    }

    public static void saveSongs(ObservableCollection<Song> songs)
    {
        try
        {
            string json = JsonConvert.SerializeObject(songs, Formatting.Indented);
            StreamWriter sw = new StreamWriter(songListFile);
            sw.Write(json);
            sw.Close();
        }
        catch (Exception e)
        {
            Thread.Sleep(1000);
            try
            {
                string json = JsonConvert.SerializeObject(songs, Formatting.Indented);
                StreamWriter sw = new StreamWriter(songListFile);
                sw.Write(json);
                sw.Close();
            }
            catch (Exception)
            {
                errorReport(e);
                Debug.WriteLine(e.ToString());
            }
        }
    }

    public static TwitchBot loadData()
    {
        if (!File.Exists(userDataFile))
        {
            return null;
        }
        try
        {
            StreamReader r = new StreamReader(userDataFile);
            string json = r.ReadToEnd();
            r.Close();
            try
            {
                JContainer.Parse(json);
            }
            catch (Exception)
            {
                try
                {
                    r = new StreamReader(backupUserDataFile);
                    json = r.ReadToEnd();
                    r.Close();
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return JsonConvert.DeserializeObject<TwitchBot>(json);
        }
        catch (Exception e)
        {
            errorReport(e);
            Debug.WriteLine(e.ToString());
        }
        return null;
    }
    
    public static void saveData(TwitchBot twitchBot)
    {
        StreamWriter sw = new StreamWriter(userDataFile);
        try
        {
            string json = JsonConvert.SerializeObject(twitchBot, Formatting.Indented);
            try
            {
                JContainer.Parse(json);
            }
            catch (Exception)
            {
                StreamReader r = new StreamReader(backupUserDataFile);
                json = r.ReadToEnd();
                r.Close();
                sw.Write(json);
                sw.Close();
                saveSongs(twitchBot.requestSystem.songList);
                return;
            }
            sw.Write(json);
            sw.Close();
            saveSongs(twitchBot.requestSystem.songList);
            if (File.Exists(backupUserDataFile))
            {
                File.Delete(backupUserDataFile);
            }
            copyFile(userDataFile, backupUserDataFile);
        }
        catch (Exception)
        {
            Thread.Sleep(1000);
            try
            {
                string json = JsonConvert.SerializeObject(twitchBot, Formatting.Indented);
                sw.Write(json);
                sw.Close();
                saveSongs(twitchBot.requestSystem.songList);
                if (File.Exists(backupUserDataFile))
                {
                    File.Delete(backupUserDataFile);
                }
                copyFile(userDataFile, backupUserDataFile);
            }
            catch (Exception e)
            {
                errorReport(e);
                Debug.WriteLine(e.ToString());
            }
        }
    }

    public static void copyFile(String f1, String f2)
    {
        try
        {
            File.Copy(f1, f2);
        }
        catch (IOException e)
        {
            errorReport(e);
            Debug.WriteLine(e.ToString());
        }
    }

    public static void errorReport(Exception e)
    {
        try
        {
            StreamWriter output = new StreamWriter(@"bin\errorlog.txt", true);
            output.Write(getDate() + " " + getTime() + " - DudeBot " + version + " Error : " + e + "\r\r\r");
            output.Close();
        }
        catch (IOException e1)
        {
            Debug.WriteLine(e1.ToString());
        }
    }

    public static String getDate()
    {
        return DateTime.Now.ToString("MM/dd/yyyy");
    }

    public static String getTime()
    {
        return DateTime.Now.ToString("HH:mm:ss");
    }
    
    public static Boolean isInteger(String s)
    {
        if (s.Contains("-") || s.Contains(" "))
        {
            return false;
        }
        try
        {
            Int32.Parse(s);
        }
        catch (Exception)
        {
            return false;
        }
        return true;
    }

    public static Boolean isDouble(String s)
    {
        if (s.Contains("-") || s.Contains(" "))
        {
            return false;
        }
        try
        {
            Double.Parse(s);
        }
        catch (Exception)
        {
            return false;
        }
        return true;
    }

    public static double closest(double of, List<Double> i)
    {
        double min = Int32.MaxValue;
        double closest = of;
        foreach (double v in i)
        {
            double diff = Math.Abs(v - of);
            if (diff < min)
            {
                min = diff;
                closest = v;
            }
        }
        return closest;
    }

    public static String timeConversion(int totalSeconds)
    {
        int MINUTES_IN_AN_HOUR = 60;
        int SECONDS_IN_A_MINUTE = 60;
        int seconds = totalSeconds % SECONDS_IN_A_MINUTE;
        int totalMinutes = totalSeconds / SECONDS_IN_A_MINUTE;
        int minutes = totalMinutes % MINUTES_IN_AN_HOUR;
        int hours = totalMinutes / MINUTES_IN_AN_HOUR;
        if (hours == 0 && minutes == 0)
        {
            if (seconds == 1)
            {
                return seconds + " second";
            }
            else
            {
                return seconds + " seconds";
            }
        }
        else if (hours == 0)
        {
            if (minutes == 1 && seconds == 1)
            {
                return minutes + " minute and " + seconds + " second";
            }
            else if (minutes == 1)
            {
                return minutes + " minute and " + seconds + " seconds";
            }
            else if (seconds == 1)
            {
                return minutes + " minutes and " + seconds + " second";
            }
            else
            {
                return minutes + " minutes and " + seconds + " seconds";
            }
        }
        else
        {
            if (hours == 1 && minutes == 1 && seconds == 1)
            {
                return hours + " hour, " + minutes + " minute, and " + seconds + " second";
            }
            else if (hours == 1 && minutes == 1)
            {
                return hours + " hour, " + minutes + " minute, and " + seconds + " seconds";
            }
            else if (hours == 1 && seconds == 1)
            {
                return hours + " hour, " + minutes + " minutes, and " + seconds + " second";
            }
            else if (minutes == 1 && seconds == 1)
            {
                return hours + " hours, " + minutes + " minute, and " + seconds + " second";
            }
            else if (hours == 1)
            {
                return hours + " hour, " + minutes + " minutes, and " + seconds + " seconds";
            }
            else if (minutes == 1)
            {
                return hours + " hours, " + minutes + " minute, and " + seconds + " seconds";
            }
            else if (seconds == 1)
            {
                return hours + " hours, " + minutes + " minutes, and " + seconds + " second";
            }
            else
            {
                return hours + " hours, " + minutes + " minutes, and " + seconds + " seconds";
            }
        }
    }

    public static String timeConversionYears(long inputSeconds)
    {
        long days = (inputSeconds / (3600 * 24));
        if (days < 1)
        {
            return "less than 1 day";
        }
        else if (days < 30)
        {
            return days + " days";
        }
        else
        {
            int daysInt = (int)Math.Floor((decimal)days);
            int months = daysInt / 30;
            if (daysInt < 365)
            {
                int remDays = daysInt % 30;
                if (remDays == 0)
                {
                    if (months == 1)
                    {
                        return months + " month";
                    }
                    else
                    {
                        return months + " months";
                    }
                }
                else
                {
                    if (months == 1 && remDays == 1)
                    {
                        return months + " month and " + remDays + " day";
                    }
                    else if (months == 1)
                    {
                        return months + " month and " + remDays + " days";
                    }
                    else if (remDays == 1)
                    {
                        return months + " months and " + remDays + " day";
                    }
                    else
                    {
                        return months + " months and " + remDays + " days";
                    }
                }
            }
            else
            {
                int years = months / 12;
                int remMonths = months % 12;
                if (remMonths == 0)
                {
                    if (years == 1)
                    {
                        return years + " year";
                    }
                    else
                    {
                        return years + " years";
                    }
                }
                else
                {
                    if (years == 1 && remMonths == 1)
                    {
                        return years + " year and " + remMonths + " month";
                    }
                    else if (years == 1)
                    {
                        return years + " year and " + remMonths + " months";
                    }
                    else if (remMonths == 1)
                    {
                        return years + " years and " + remMonths + " month";
                    }
                    else
                    {
                        return years + " years and " + remMonths + " months";
                    }
                }
            }
        }
    }

    public static int getRandomNumber(int x)
    {
        Random rand = new Random();
        return rand.Next(x);
    }
    
    public static String callURL(String uri)
    {
        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            if (uri.ToLower().Contains("kraken"))
            {
                request.Headers.Add("Client-ID", twitchClientID);
            }
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
        catch (WebException)
        {
            return "";
        }
        catch (Exception e1)
        {
            errorReport(e1);
            Debug.WriteLine(e1.ToString());
            return "";
        }
    }
    
    public static String getFollowingText(String message)
    {
        return message.Substring(message.IndexOf(" ") + 1);
    }

    public static List<String> getBadWordList()
    {
        List<String> list = new List<String>();
        list.Add("blowjob");
        list.Add("blow job");
        list.Add("cunt");
        list.Add("dyke");
        list.Add("fag");
        list.Add("fudgepacker");
        list.Add("fudge packer");
        list.Add("homo");
        list.Add("jizz");
        list.Add("nigger");
        list.Add("nigga");
        list.Add("prick");
        list.Add("pube");
        list.Add("pubic");
        list.Add("pussy");
        list.Add("queer");
        list.Add("slut");
        list.Add("smegma");
        list.Add("twat");
        list.Add("whore");
        return list;
    }

    public static int LevenshteinDistance(string s, string t)
    {
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];
        if (n == 0)
        {
            return m;
        }
        if (m == 0)
        {
            return n;
        }
        for (int i = 0; i <= n; i++)
            d[i, 0] = i;
        for (int j = 0; j <= m; j++)
            d[0, j] = j;

        for (int j = 1; j <= m; j++)
            for (int i = 1; i <= n; i++)
                if (s[i - 1] == t[j - 1])
                    d[i, j] = d[i - 1, j - 1];
                else
                    d[i, j] = Math.Min(Math.Min(
                        d[i - 1, j] + 1,
                        d[i, j - 1] + 1),
                        d[i - 1, j - 1] + 1
                        );
        return d[n, m];
    }


}