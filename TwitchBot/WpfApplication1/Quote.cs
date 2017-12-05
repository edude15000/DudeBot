using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public class Quote
{
    public Boolean quotesModOnly { get; set; }
    public bool quotesOn { get; set; }
    public List<String> quotes { get; set; } = new List<String>();
    [JsonIgnore]
    public TwitchBot bot;

    public void triggerQuotes(String message, String channel, String sender)
    {
        if (message.Equals("!quotes off", StringComparison.InvariantCultureIgnoreCase)
                && (sender.Equals(bot.streamer, StringComparison.InvariantCultureIgnoreCase) || sender.Equals(Utils.botMaker, StringComparison.InvariantCultureIgnoreCase)))
        {
            quotesOn = false;
            bot.client.SendMessage("Quote system turned off!");
        }
        else if (message.Equals("!quotes on", StringComparison.InvariantCultureIgnoreCase) && sender.Equals(bot.streamer, StringComparison.InvariantCultureIgnoreCase))
        {
            quotesOn = false;
            bot.client.SendMessage("Quote system turned on!");
        }
        bot.read();
    }

    public void quotesSystem(String message, String channel, String sender, String streamer, List<BotUser> users)

    {
        String temp = message.ToLower();
        if (message.Equals("!totalquotes", StringComparison.InvariantCultureIgnoreCase) || message.Equals("!quotestotal", StringComparison.InvariantCultureIgnoreCase))
        {
            if (quotes != null)
            {
                if (quotes.Count < 1)
                {
                    bot.client.SendMessage("There are currently no quotes in this stream!");
                }
                else if (quotes.Count == 1)
                {
                    bot.client.SendMessage("There is 1 quote in this stream. Type '!quote 0' to display it!");
                }
                else
                {
                    bot.client.SendMessage("There are " + quotes.Count
                            + " quotes in this stream (quotes #0 - #" + (quotes.Count - 1) + ")!");
                }
                return;
            }
        }
        else if (message.Equals("!quote", StringComparison.InvariantCultureIgnoreCase))
        {
            getRandomQuote(channel);
            return;
        }
        else if (message.Contains("!quote "))
        {
            if (Utils.isInteger(Utils.getFollowingText(message)))
            {
                getQuoteByID(Int32.Parse(Utils.getFollowingText(message)), channel);
                return;
            }
        }
        else if (temp.StartsWith("!addquote ") || temp.StartsWith("!quoteadd "))
        {
            if (quotesModOnly)
            {
                if (sender.Equals(bot.streamer, StringComparison.InvariantCultureIgnoreCase) || Utils.checkIfUserIsOP(sender, channel, streamer, users)
                        || sender.Equals(Utils.botMaker))
                {
                    addQuote(Utils.getFollowingText(message), channel, sender);
                }
            }
            else
            {
                addQuote(Utils.getFollowingText(message), channel, sender);
            }
            return;
        }
        else if (message.Trim().ToLower().StartsWith("!removequote ")
                || message.Trim().ToLower().StartsWith("!deletequote "))
        {
            if (sender.Equals(bot.streamer, StringComparison.InvariantCultureIgnoreCase) || Utils.checkIfUserIsOP(sender, channel, streamer, users)
                    || sender.Equals(Utils.botMaker, StringComparison.InvariantCultureIgnoreCase))
            {
                removeQuote(message, channel, sender);
                return;
            }
        }
        else if (message.Trim().ToLower().StartsWith("!editquote ")
                || message.Trim().ToLower().StartsWith("!changequote "))
        {
            if (sender.Equals(bot.streamer, StringComparison.InvariantCultureIgnoreCase) || Utils.checkIfUserIsOP(sender, channel, streamer, users)
                    || sender.Equals(Utils.botMaker, StringComparison.InvariantCultureIgnoreCase))
            {
                editQuote(message, channel, sender);
                return;
            }
        }
    }

    public void removeQuote(String message, String channel, String sender)
    {
        int number = -1;
        try
        {
            number = Int32.Parse(Utils.getFollowingText(message));
            if (quotes.Count < 1)
            {
                bot.client.SendMessage("There are no quotes in this stream, " + sender + "!");
                return;
            }
            if (quotes.Count < number || number < 0)
            {
                bot.client.SendMessage("Quote #" + number + " does not exist, " + sender + "!");
                return;
            }
        }
        catch (Exception)
        {
            bot.client.SendMessage("To remove a quote, it must be in the form '!removequote #'");
            return;
        }
        try
        {
            quotes.RemoveAt(number);
            bot.client.SendMessage("Quote #" + number + " has been removed, " + sender + "!");
            return;
        }
        catch (Exception e)
        {
            Utils.errorReport(e);
            Debug.WriteLine(e.ToString());
            bot.client.SendMessage("Failed to remove quote, please try again.");
        }
        return;
    }

    public void editQuote(String message, String channel, String sender)
    {
        int number = -1;
        message = message.Substring(message.IndexOf(" ") + 1);
        try
        {
            number = Int32.Parse(message.Substring(0, message.IndexOf(" ")));
        }
        catch (Exception)
        {
            bot.client.SendMessage("Please type in the format '!editquote quote# new quote'");
            return;
        }
        if (number >= quotes.Count || number < 0)
        {
            bot.client.SendMessage("Quote #" + number + " does not exist, " + sender + "!");
            return;
        }
        String newQuote = message.Substring(message.IndexOf(" ") + 1);
        newQuote = formatQuote(newQuote);
        if (newQuote == null)
        {
            bot.client.SendMessage("Failed to add quote, please try again later, " + sender + "!");
            return;
        }
        try
        {
            if (newQuote.Contains("\r"))
            {
                newQuote = newQuote.Replace("\r", "");
            }
            quotes.Insert(number, newQuote);
            bot.client.SendMessage("Quote #" + number + " has been updated, " + sender + "!");
            return;
        }
        catch (Exception e)
        {
            Utils.errorReport(e);
            Debug.WriteLine(e.ToString());
            bot.client.SendMessage("Failed to edit quote, please try again.");
        }
        return;
    }

    public void addQuote(String quote, String channel, String sender)
    {
        quote = formatQuote(quote);
        if (quote == null)
        {
            bot.client.SendMessage("Failed to add quote, please try again later, " + sender + "!");
            return;
        }
        quotes.Add(quote);
        bot.client.SendMessage("Quote " + quote + " has been added, " + sender + "!");
    }


    public String formatQuote(String quote)
    {
        try
        {
            if (quote.Contains("\""))
            {
                quote = quote.Replace("\"", "");
            }
            if (quote.Contains("@"))
            {
                quote = quote.Replace("@", "");
            }
            if (quote.EndsWith(")") && quote.Contains("("))
            {
                String user = quote.Substring(quote.LastIndexOf("(") + 1, quote.Length - 1);
                quote = quote.Substring(0, quote.LastIndexOf("(") - 1);
                quote = "\" " + quote + " \"" + " -" + user + " " + Utils.getDate() + "\r";
            }
            else
            {
                quote = "\" " + quote + " \"" + " -" + bot.streamer + " " + Utils.getDate() + "\r";
            }
            return quote;
        }
        catch (Exception e)
        {
            Utils.errorReport(e);
            Debug.WriteLine(e.ToString());
        }
        return null;
    }

    public void getRandomQuote(String channel)
    {
        if (quotes.Count < 1)
        {
            return;
        }
        Random rand = new Random();
        int index = rand.Next(quotes.Count);
        bot.client.SendMessage("Quote #" + index + ": " + quotes[index]);
    }

    public void getQuoteByID(int ID, String channel)
    {
        if (ID <= quotes.Count - 1)
        {
            bot.client.SendMessage("Quote #" + ID + ": " + quotes[ID]);
        }
    }
}

