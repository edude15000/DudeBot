using System;
using System.Collections.Generic;

public class Quote
{
    public Boolean quotesModOnly, quotesOn;
    public List<String> quotes = new List<String>();
    public TwitchBot bot;

    public void triggerQuotes(String message, String channel, String sender)
    {
        if (message.Equals("!quotes off", StringComparison.InvariantCultureIgnoreCase)
                && (sender.Equals(bot.streamer, StringComparison.InvariantCultureIgnoreCase) || sender.Equals(Utils.botMaker, StringComparison.InvariantCultureIgnoreCase)))
        {
            quotesOn = false;
            bot.sendRawLine("PRIVMSG " + channel + " :" + "Quote system turned off!");
        }
        else if (message.Equals("!quotes on", StringComparison.InvariantCultureIgnoreCase) && sender.Equals(bot.streamer, StringComparison.InvariantCultureIgnoreCase))
        {
            quotesOn = false;
            bot.sendRawLine("PRIVMSG " + channel + " :" + "Quote system turned on!");
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
                    bot.sendRawLine("PRIVMSG " + channel + " :" + "There are currently no quotes in this stream!");
                }
                else if (quotes.Count == 1)
                {
                    bot.sendRawLine("PRIVMSG " + channel + " :"
                            + "There is 1 quote in this stream. Type '!quote 0' to display it!");
                }
                else
                {
                    bot.sendRawLine("PRIVMSG " + channel + " :" + "There are " + quotes.Count
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
                bot.sendRawLine("PRIVMSG " + channel + " : There are no quotes in this stream, " + sender + "!");
                return;
            }
            if (quotes.Count < number || number < 0)
            {
                bot.sendRawLine("PRIVMSG " + channel + " : Quote #" + number + " does not exist, " + sender + "!");
                return;
            }
        }
        catch (Exception e)
        {
            bot.sendRawLine("PRIVMSG " + channel + " :" + "To remove a quote, it must be in the form '!removequote #'");
            return;
        }
        try
        {
            quotes.RemoveAt(number);
            bot.sendRawLine("PRIVMSG " + channel + " :" + "Quote #" + number + " has been removed, " + sender + "!");
            return;
        }
        catch (Exception e)
        {
            Utils.errorReport(e);
            e.ToString();
            bot.sendRawLine("PRIVMSG " + channel + " :" + "Failed to remove quote, please try again.");
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
        catch (Exception e)
        {
            bot.sendRawLine("PRIVMSG " + channel + " :" + "Please type in the format '!editquote quote# new quote'");
            return;
        }
        if (number >= quotes.Count || number < 0)
        {
            bot.sendRawLine("PRIVMSG " + channel + " :" + "Quote #" + number + " does not exist, " + sender + "!");
            return;
        }
        String newQuote = message.Substring(message.IndexOf(" ") + 1);
        newQuote = formatQuote(newQuote);
        if (newQuote == null)
        {
            bot.sendRawLine(
                    "PRIVMSG " + channel + " :" + "Failed to add quote, please try again later, " + sender + "!");
            return;
        }
        try
        {
            if (newQuote.Contains("\r"))
            {
                newQuote = newQuote.Replace("\r", "");
            }
            quotes.Insert(number, newQuote);
            bot.sendRawLine("PRIVMSG " + channel + " :" + "Quote #" + number + " has been updated, " + sender + "!");
            return;
        }
        catch (Exception e)
        {
            Utils.errorReport(e);
            e.ToString();
            bot.sendRawLine("PRIVMSG " + channel + " :" + "Failed to edit quote, please try again.");
        }
        return;
    }

    public void addQuote(String quote, String channel, String sender)
    {
        quote = formatQuote(quote);
        if (quote == null)
        {
            bot.sendRawLine(
                    "PRIVMSG " + channel + " :" + "Failed to add quote, please try again later, " + sender + "!");
            return;
        }
        quotes.Add(quote);
        bot.sendRawLine("PRIVMSG " + channel + " :" + "Quote " + quote + " has been added, " + sender + "!");
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
            e.ToString();
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
        bot.sendRawLine("PRIVMSG " + channel + " :" + "Quote #" + index + ": " + quotes.get(index));
    }

    public void getQuoteByID(int ID, String channel)
    {
        if (ID <= quotes.Count - 1)
        {
            bot.sendRawLine("PRIVMSG " + channel + " :" + "Quote #" + ID + ": " + quotes.get(ID));
        }
    }
}

