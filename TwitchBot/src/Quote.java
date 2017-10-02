import java.io.BufferedWriter;
import java.io.FileWriter;
import java.io.IOException;
import java.nio.charset.Charset;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.List;
import java.util.Random;

public class Quote {

	boolean quotesModOnly;
	boolean quotesOn;
	ArrayList<String> quotes = new ArrayList<String>();
	TwitchBot bot;

	public Quote(TwitchBot bot) {
		this.bot = bot;
	}

	public void triggerQuotes(String message, String channel, String sender) throws IOException {
		if (message.equalsIgnoreCase("!quotes off")
				&& (sender.equalsIgnoreCase(bot.streamer) || sender.equals(Utils.botMaker))) {
			quotesOn = false;
			Path path = Paths.get(Utils.configFile);
			Charset charset = StandardCharsets.UTF_8;
			String content = new String(Files.readAllBytes(path), charset);
			content = content.replaceAll("quotesOn=true", "quotesOn=false");
			Files.write(path, content.getBytes(charset));
			bot.sendRawLine("PRIVMSG " + channel + " :" + "Quote system turned off!");
		} else if (message.equalsIgnoreCase("!quotes on") && sender.equalsIgnoreCase(bot.streamer)) {
			quotesOn = false;
			Path path = Paths.get(Utils.configFile);
			Charset charset = StandardCharsets.UTF_8;
			String content = new String(Files.readAllBytes(path), charset);
			content = content.replaceAll("quotesOn=false", "quotesOn=true");
			Files.write(path, content.getBytes(charset));
			bot.sendRawLine("PRIVMSG " + channel + " :" + "Quote system turned on!");
		}
		bot.read();
	}

	public void quotesSystem(String message, String channel, String sender, String streamer, List<BotUser> users)
			throws InterruptedException {
		String temp = message.toLowerCase();
		if (message.equalsIgnoreCase("!totalquotes") || message.equalsIgnoreCase("!quotestotal")) {
			if (quotes != null) {
				if (quotes.size() < 1) {
					bot.sendRawLine("PRIVMSG " + channel + " :" + "There are currently no quotes in this stream!");
				} else if (quotes.size() == 1) {
					bot.sendRawLine("PRIVMSG " + channel + " :"
							+ "There is 1 quote in this stream. Type '!quote 0' to display it!");
				} else {
					bot.sendRawLine("PRIVMSG " + channel + " :" + "There are " + quotes.size()
							+ " quotes in this stream (quotes #0 - #" + (quotes.size() - 1) + ")!");
				}
				return;
			}
		} else if (message.equalsIgnoreCase("!quote")) {
			getRandomQuote(channel);
			return;
		} else if (message.contains("!quote ")) {
			if (Utils.isInteger(Utils.getFollowingText(message))) {
				getQuoteByID(Integer.parseInt(Utils.getFollowingText(message)), channel);
				return;
			}
		} else if (temp.startsWith("!addquote ") || temp.startsWith("!quoteadd ")) {
			if (quotesModOnly) {
				if (sender.equalsIgnoreCase(bot.streamer) || Utils.checkIfUserIsOP(sender, channel, streamer, users)
						|| sender.equals(Utils.botMaker)) {
					addQuote(Utils.getFollowingText(message), channel, sender);
				}
			} else {
				addQuote(Utils.getFollowingText(message), channel, sender);
			}
			return;
		} else if (message.trim().toLowerCase().startsWith("!removequote ")
				|| message.trim().toLowerCase().startsWith("!deletequote ")) {
			if (sender.equals(bot.streamer) || Utils.checkIfUserIsOP(sender, channel, streamer, users)
					|| sender.equals(Utils.botMaker)) {
				removeQuote(message, channel, sender);
				return;
			}
		} else if (message.trim().toLowerCase().startsWith("!editquote ")
				|| message.trim().toLowerCase().startsWith("!changequote ")) {
			if (sender.equals(bot.streamer) || Utils.checkIfUserIsOP(sender, channel, streamer, users)
					|| sender.equals(Utils.botMaker)) {
				editQuote(message, channel, sender);
				return;
			}
		}
	}

	public void removeQuote(String message, String channel, String sender) {
		int number = -1;
		try {
			number = Integer.parseInt(Utils.getFollowingText(message));
			if (quotes.size() < 1) {
				bot.sendRawLine("PRIVMSG " + channel + " : There are no quotes in this stream, " + sender + "!");
				return;
			}
			if (quotes.size() < number || number < 0) {
				bot.sendRawLine("PRIVMSG " + channel + " : Quote #" + number + " does not exist, " + sender + "!");
				return;
			}
		} catch (Exception e) {
			bot.sendRawLine("PRIVMSG " + channel + " :" + "To remove a quote, it must be in the form '!removequote #'");
			return;
		}
		try {
			Path path = Paths.get(Utils.quotesFile);
			List<String> fileContent = new ArrayList<>(Files.readAllLines(path, StandardCharsets.UTF_8));
			fileContent.remove(number);
			Files.write(path, fileContent, StandardCharsets.UTF_8);
			bot.read();
			bot.sendRawLine("PRIVMSG " + channel + " :" + "Quote #" + number + " has been removed, " + sender + "!");
			return;
		} catch (Exception e) {
			Utils.errorReport(e);
			e.printStackTrace();
			bot.sendRawLine("PRIVMSG " + channel + " :" + "Failed to remove quote, please try again.");
		}
		return;
	}

	public void editQuote(String message, String channel, String sender) {
		int number = -1;
		message = message.substring(message.indexOf(" ") + 1);
		try {
			number = Integer.parseInt(message.substring(0, message.indexOf(" ")));
		} catch (Exception e) {
			bot.sendRawLine("PRIVMSG " + channel + " :" + "Please type in the format '!editquote quote# new quote'");
			return;
		}
		if (number >= quotes.size() || number < 0) {
			bot.sendRawLine("PRIVMSG " + channel + " :" + "Quote #" + number + " does not exist, " + sender + "!");
			return;
		}
		String newQuote = message.substring(message.indexOf(" ") + 1);
		newQuote = formatQuote(newQuote);
		if (newQuote == null) {
			bot.sendRawLine(
					"PRIVMSG " + channel + " :" + "Failed to add quote, please try again later, " + sender + "!");
			return;
		}
		try {
			Path path = Paths.get(Utils.quotesFile);
			List<String> fileContent = new ArrayList<>(Files.readAllLines(path, StandardCharsets.UTF_8));
			System.out.println(number + " : " + newQuote);
			if (newQuote.contains("\r")) {
				newQuote = newQuote.replace("\r", "");
			}
			fileContent.set(number, newQuote);
			Files.write(path, fileContent, StandardCharsets.UTF_8);
			bot.read();
			bot.sendRawLine("PRIVMSG " + channel + " :" + "Quote #" + number + " has been updated, " + sender + "!");
			return;
		} catch (Exception e) {
			Utils.errorReport(e);
			e.printStackTrace();
			bot.sendRawLine("PRIVMSG " + channel + " :" + "Failed to edit quote, please try again.");
		}
		return;
	}

	public void addQuote(String quote, String channel, String sender) throws InterruptedException {
		try {
			quote = formatQuote(quote);
			if (quote == null) {
				bot.sendRawLine(
						"PRIVMSG " + channel + " :" + "Failed to add quote, please try again later, " + sender + "!");
				return;
			}
			BufferedWriter writer = new BufferedWriter(new FileWriter(Utils.quotesFile, true));
			writer.write(quote);
			writer.close();
			bot.sendRawLine("PRIVMSG " + channel + " :" + "Quote " + quote + " has been added, " + sender + "!");
			bot.read();
		} catch (IOException e) {
			Utils.errorReport(e);
			e.printStackTrace();
		}
	}

	public String formatQuote(String quote) {
		try {
			if (quote.contains("\"")) {
				quote = quote.replace("\"", "");
			}
			if (quote.contains("@")) {
				quote = quote.replace("@", "");
			}
			if (quote.endsWith(")") && quote.contains("(")) {
				String user = quote.substring(quote.lastIndexOf("(") + 1, quote.length() - 1);
				quote = quote.substring(0, quote.lastIndexOf("(") - 1);
				quote = "\" " + quote + " \"" + " -" + user + " " + Utils.getDate() + "\r";
			} else {
				quote = "\" " + quote + " \"" + " -" + bot.streamer + " " + Utils.getDate() + "\r";
			}
			return quote;
		} catch (Exception e) {
			Utils.errorReport(e);
			e.printStackTrace();
		}
		return null;
	}

	public void getRandomQuote(String channel) {
		if (quotes.size() < 1) {
			return;
		}
		Random rand = new Random();
		int index = rand.nextInt(quotes.size());
		bot.sendRawLine("PRIVMSG " + channel + " :" + "Quote #" + index + ": " + quotes.get(index));
	}

	public void getQuoteByID(int ID, String channel) {
		if (ID <= quotes.size() - 1) {
			bot.sendRawLine("PRIVMSG " + channel + " :" + "Quote #" + ID + ": " + quotes.get(ID));
		}
	}
}
