DudeBot is a free-to-use request bot for music streamers.
Edude15000 made this bot using PircBot, a prebuilt Java library.

*****YOU WILL NEED Java and .NET 4.5.2 or higher.

******************************************************************************************************************************************

*****Preparing DudeBot
1. Unpackage application to an easy to manage location (like Desktop).
2. Ensure you have Java installed on your computer, (REQUIRED)!
	You can install it free here: https://java.com/en/download/
3. Ensure you have .NET 4.5.2 or higher installed on your computer, you cannot run the GUI without it.
	You can install it free here: https://www.microsoft.com/en-us/download/details.aspx?id=42643
4. Create a new Twitch account for your bot (Unless you already have one to use)!
5. Ensure that your bot is logged into Twitch.
6. Run 'DudeBot.exe', you should get a first time user message.
7. Go to: http://www.twitchapps.com/tmi/ and Connect the bot with Twitch, 
	then copy the whole oauth code from the text box into the Oauth text block.
	Example : oauth:d9r7onmy0apujm8odkn20zq02w8915
8. Enter your Bot Name and Streamer Name into the appropriate fields.
9. Scroll to the bottom of the customization tab and press 'Apply', the GUI will restart and populate data. 
	If the console says DISCONNECTED, 
	this means that you have entered in incorrect data. Please enter correct data for the
	three fields, click 'Apply', and restart the bot.
10. You are ready to go!

NOTE: If the bot is crashing when you try to open it but you already have Java and .NET up to date, 
	make sure you give permissions to dudebot.exe and dudebotupdater.exe in your antivirus. 
	The bot will also copy the dudebotupdate.exe file to your %temp% directory, 
	so if you are still having problems, make sure to give that file permissions to run as well.

NOTE: MAKE SURE DUDEBOT ACCOUNT IS MODDED / SET AS BOT IN CHANNEL!

*****OBS Integration
11. In OBS, create a new 'text' boxes, and in the 'Use Text From File',
	navigate to 'currentsong.txt' and 'currentrequester.txt'. These will dynamically show the 
	currently playing song and requester on stream!
	
*****Images on Stream OBS STUDIO integration
12. Go to the Dashboard tab and click 'open images window.' In OBS STUDIO, create a new 'window capture' and in the Window dropbox, select '[Images.exe]: Images.'
	Leave Window Match Priority as Window Title. Right click on the newly created window capture and click 'filters.' Click on the '+' at the bottom left to add a new filter.
	Click 'chroma key' and for 'Key Color Type' select Green, then select 1 for Similarity, Smoothness, and Key Color Spill Reduction.
	You can check the 'Open window on start' checkbox so that it will automatically open the images window every time you start DudeBot.
	
NOTE: Every time you run DudeBot, backups will be placed in your Windows Temp folder %temp%/backupdudebot....

*****Setting up Google Sheets (You must have a Google Account for this!)
13. Go to https://docs.google.com/spreadsheets/u/0/ and create a new Google Sheet.
	You should have a link that looks like: https://docs.google.com/spreadsheets/d/1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms/edit.
	Take the middle section and copy it (1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms) and paste it into the 
	Google Sheet ID textbox on the Customization page. Close and restart DudeBot. You should get a pop up to login with your Google Account.
14. Click on the blue share button at the top right of the Google Sheets webpage, then press the get shareable link button. This will make it viewable to everyone.
 
******************************************************************************************************************************************

***Customization Options

Regular / Subs List: A list of all of your 'subs/regulars' you can set commands so that only
	members in this list are allowed to use them.
Favorite Songs List: A collection of songs that when users type the 'play song from favorites'
	command name (default: !requestfav,!songfav,!playfav), it will choose a random one from this
	list and add it to the song list.
Requests On: Toggle requests on/off.
Banned Keywords List: A list of banned keywords (if a request contains a keyword, it will not add it to the list!)
Must Follow to Request: If checked, the requester must follow you to request a song.
Display songlist in one line: If checked, all displayed songs from list will be displayed in one line.
Display if user is here: If checked, the next song command will show that the user is here if it is certain they are.
Max Song List Length: How many possible songs can be in the song list at once (excludes VIP songs).
Max Number of Songs in Song List Per User: I shouldn't have to explain this one.
Number of Songs Song List Displays: This one either.
RevloBot Minigame Reward Amount: If the checkbox is unchecked, will give winners specified amount of points, if checked then will give
winners the amount of points equal to the winning amount of the minigame (example: !endgame 75) would result in 75 points being rewarded.
Direct Input Requests: Allows users to request via direct input 'song - artist'.
Youtube Link Requests: Allows users to request via youtube links.
Song Duration Limit: Will check Youtube video of closest matched input and only allow requests that are less than or equal to given duration.

******************************************************************************************************************************************

***Dudebot Commands
(All commands used are defaults, you can change the level of all of these commands in the GUI)
NOTE: There are three request lists (that apply in the following order): $$$(Donation), VIP, REG

DOUBLE CLICK ON A SONG IN THE SONG LIST TO OPEN THE YOUTUBE VIDEO

!request artist - song			User requests a song

!requests on / !requests off		Trigger Requests

!requestfav						Play song from streamer's favorites list

!songlist						Display song list

!length							Display current number of songs in list

!viwers							Display current number of viewers in stream

!addtop artist - song			Add a song to the top of the song list

!addtop artist - song (requesterName)	Add a song to the top of the song list using the requesterName as the requester

!adddonator artist - song		Add a donation song

!adddonator artist - song (requesterName)	Add a donation song using the requesterName as the requester

!editsong newArtist - newSong	User edits their own next (non VIP) song

!wrongsong						User removes their own next (non VIP) song from the list 

!addvip artist - song			Add a VIP song to the VIP list (will keep the current playing song in the first postion)

!addvip artist - song (requesterName)	Add a VIP song to the VIP list using the requesterName as the requester (will keep the current playing song in the first position)

!edit newArtist - newSong		Edit the currently playing song

!next							Skip to the next song in the list (after finishing the current one)

!clear							Clear the song list entirely

!currentsong					Display current playing song

!nextsong						Display next song in the list

!randomnext						Choose a random song from the song list to play next

!position				Displays the user's next regular requested song number in line

!givespot @user				*Gives position in line to another user (CANT CHANGE COMMAND NAME!)

!regnext OR !nextreg OR !regularnext OR !nextregular	*If there exists a standard request in the list, it will move it to the now playing song (overwriting the previous now playing song), otherwise it will just call the !next command (CANT CHANGE COMMAND NAME!)

!botcolor color				Changes color of bot in chat (must be Twitch official color, MOD / STREAMER ONLY)

!removesong #				Deletes song in song place # (MOD / STREAMER ONLY)



******************************************************************************************************************************************

***Custom Commands & Timers
To add a custom command, type the command name in the first box, (!kappa) and then the response in the second box (Kappa).
To edit a custom command, click on the command in the list, edit the response and press 'Add/Edit'.
To delete a custom command, click on the command in the list, then press 'Delete'.

***Custom Variables
* If you use these variables in your commands, they will display the following response:
$viewers = Display the current viewer count.
$length = Number of songs currently in the song list.
$user =	Display user who triggered command.
$input = Display following text after command.
$currentsong = Display current song.
$currentrequester = Display current song requester.
$randomuser = Display a random user's name.
$streamer = Display streamer's name
$randomnumber3 = Display a random number between 1 digit and [3] digits (0 - 100).
$randomnumber2 = Display a random number between 1 digit and [2] digits (0 - 10).
$8ball = Displays a random generic 8ball response.
$shoutout = Displays generic shoutout message using following text as user (MOD ONLY)!
$uptime = Displays amount of time Dudebot has been open for.
$counter1 = Keeps a count for the number of times a command has been run.
$counter2 = Keeps a count for the number of times a command has been run.
$counter3 = Keeps a count for the number of times a command has been run.
$counter4 = Keeps a count for the number of times a command has been run.
$counter5 = Keeps a count for the number of times a command has been run.
$songlist = Displays songlist (Response MUST only be '$songlist', NOTHING ELSE!)
$following = Displays how long the use has been following the stream
$start = Displays Twitch join date of streamer
$roulette = 1/6 chance of timing the user out for 1 second (simulating kill)

Example:	!songlist	$songlist
Example:	!burp		$streamer has burped $counter1 times!
Example:	!8ball		$8ball
Example:	!love		There's $randomnumber3% <3 between $user and $input!
Example:	!harambe	Harambe would have loved $currentsong requested by $currentrequester!
Example:	!shoutout	Make sure to follow $input at twitch.tv/$input!
Example:	!viewers	Current viewer count: $viewers!
Example:	!uptime 	$streamer has been live for $uptime!
Example:	!cleverbot	$cleverbot

******************************************************************************************************************************************

***Currency System (Pretty much the same as RevloBot)
!currency on / off = Turns system on / off (streamer ONLY)
!info = Displays currency settings set for stream
!bonus @user amount = Gives the user amount of points (mods, streamer ONLY)
!bonusall amount = Gives all viewers amount of points (mods, streamer ONLY)
!gamble on / off = Toggles gambling
!gamble amount = Gambles points -> 1-60 = lose, 60-98 = win, 99-100 = win x2
!<currencyCommand> = Displays user's currency amount and time in stream
!vipsongon = Turns on VIP song rewards
!vipsongoff = Turns off VIP song rewards
!vipsong <song> = If user has enough points, it will add a vip song to the queue
!rank = Displays user's rank in the stream in points and time
!promote = Spends points to rank up (if set)
!nextrank = Displays how many points are required to rank up (if set)
!leaderboards = Displays leaderboard information
!subcredits = Displays user's amount of sub credits
!givecredits @user amount = Gives to / takes sub credit amount to / from user (mods, streamer ONLY)
!subsong <song> = Redeems a sub song using sub credits

***Images System (Displays image on window tied with OBS to be displayed on stream)
Open window on start : Opens images window when starting DudeBot
Cooldown per user (seconds) : Cooldown per user
Image display time (seconds) : Image display time on stream
Overall cooldown time (seconds) : Cooldown between image displays regardless of user

***Quotes System
All quote system commands are hard-coded!
!quotes on / off = Toggles quote system on / off. STREAMER ONLY
!addquote quote = Adds the quote using the streamer as the quoter. (!quoteadd also works) MOD AND STREAMER ONLY
!addquote quote (user) = Adds the quote using the 'user' as the quoter. (!quoteadd also works) MOD AND STREAMER ONLY
!quote = Displays random quote from quote list
!quote # = Displays quote by ID number
!editquote # new quote = Edits quote by input quote number (MOD / STREAMER ONLY)
!removequote # = Removes quote by number (MOD / STREAMER ONLY)

***Minigame System
All minigame system commands are hard-coded!
NOTE: # can be any integer or double (ex: 0, 0.7, 58.9, 40, 145212.185, etc...)
!minigame on / off = Toggles minigame system on / off. STREAMER ONLY
!startgame = Starts a guessing game. MOD AND STREAMER ONLY
!guess # = Adds a guess of that number for a user.
!endgame # = Ends the game with the number being the winning number (whoever guessed closest wins!). MOD AND STREAMER ONLY
!endgame OR !cancelgame = Ends game without a winner

***Adventure Game
!adventure on / off = Toggles adventuring
!adventure = starts/joins an adventure
User Join Time : Time allowed for other users to join the adventure after started (seconds)
Cool Down Time : Cool down time between adventures (minutes)

***Give Away
!startraffle = Starts raffle
!endraffle = Ends raffle / picks a winner at random
!endraffle # = Ends raffle / picks # winners at random
![Give Away Command Name] = Joins the raffle

***SFX System
DO NOT STORE MP3 FILES IN DUDEBOT DIRECTORY, THEY WILL BE DESTROYED ON UPDATE!
!sfx = Displays all sound effects in stream
Left most box : !<commandName>
Upload mp3 file (MP3 FILES ONLY)

***Events System
Leftmost box input user, rightmost box input message
Will display message when user joins stream!

***Lifetime Requests System
!totalrequests = Displays lifetime requests in current stream for current user
!toprequester = Displays top lifetime requester for current stream

***Counters
!setcounter1 value = Sets counter1 to value.
Example:	!setcounter 0 = sets counter1 to 0.
(Works for !setcounter2 and !setcounter3 as well)
(STREAMER ONLY)!

***PRESS F7 to auto skip to the next song in the list!

***Add Command
Type directly in chat:
!addcom	!command response
(MOD ONLY)

******************************************************************************************************************************************

***Revlobot VIP auto adding & rewarding
MAKE SURE TO GIVE SUPERMOD STATUS! OR ADDING AFTER MINIGAMES WILL NOT WORK!

If the VIP song reward in revlobot starts with "VIP" and the command to cash in points to get the VIP song reward starts with "VIP"
then Dudebot will parse Revlobot's response and add the VIP to its list automatically!

******************************************************************************************************************************************

If you need help setting up DudeBot or have any questions, bugs, or feedback, 
please contact Edude15000 via private message on Twitch.
I will be continuing to update this bot as much as I can.

Thank you for using DudeBot :)