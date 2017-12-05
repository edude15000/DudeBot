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
9. Press 'Apply', the GUI will restart and populate data. If the console says DISCONNECTED, 
	this means that you have entered in incorrect data. Please enter correct data for the
	three fields, click 'Apply', and restart the bot.
10. You are ready to go! When done streaming, close DudeBot.exe, this will also close the console.
	While you are streaming, leave the console running minimized.

NOTE: MAKE SURE DUDEBOT ACCOUNT IS MODDED IN CHANNEL!

*****OBS Integration
11. In OBS, create a new 'text' boxes, and in the 'Use Text From File',
	navigate to 'currentsong.txt' and 'currentrequester.txt'. These will dynamically show the 
	currently playing song and requester on stream!
	
NOTE: Every time you run DudeBot, backups of config.txt and commands.txt
	will be placed in your Windows Temp folder:
	%temp%/backupdudebot.txt
	%temp%/backupdudebotcommands.txt
 
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

******************************************************************************************************************************************

***Dudebot Commands
(All commands used are defaults, you can change the level of all of these commands in the GUI)
NOTE: There are three request lists (that apply in the following order): $$$(Donation), VIP, REG

!request artist - song			User requests a song

!request on / !requests off		Trigger Requests

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

!addvip artist - song (requesterName)	Add a VIP song to the VIP list using the requesterName as the requester (will keep the current playing song in the first postion)

!edit newArtist - newSong		Edit the currently playing song

!next							Skip to the next song in the list (after finishing the current one)

!clear							Clear the song list entirely

!currentsong					Display current playing song

!nextsong						Display next song in the list

!randomnext						Choose a random song from the song list to play next

!position				Displays the user's next regular requested song number in line

******************************************************************************************************************************************

***Custom Commands
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

Example:	!8ball		$8ball
Example:	!love		There's $randomnumber3% <3 between $user and $input
Example:	!harambe	Harambe would have loved $currentsong requested by $currentrequester !
Example:	!shoutout	Make sure to follow $input at twitch.tv/$input!
Example:	!viewers	Current viewer count: $viewers

******************************************************************************************************************************************

If you need help setting up DudeBot or have any questions, bugs, or feedback, 
please contact Edude15000 via private message on Twitch.
I will be continuing to update this bot as much as I can.

Thank you for using DudeBot :)