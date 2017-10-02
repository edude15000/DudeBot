DudeBot Developer Setup Instructions

Follow the readme in the DudeBot folder first to set up a Twitch Bot account for testing and development.

Folders:
DudeBot - The actual redistributable DudeBot folder for users
Images - The Images project folder (.NET)
TwitchBot - The Twitch Bot project folder (Java)
TwitchBotGUI - The Twitch Bot GUI project folder (.NET) *Runs Java jar file within self
Updater - The Automatic Updater project folder (.NET)

How to update code:
Java :  In IDE (Eclipse), go to File > Export > Runnable JAR file
		Select the location of your bot.jar (mine is 'C:\git\DudeBotWorkspace\DudeBot\bin\bot.jar')
		Click Finish and yes to overwrite
		*Make sure to update the software version in Utils.java

.NET :	In IDE (Microsoft Visual Studio), go to Build > Batch Build...
		Make sure Release Any CPU us checked for Build and click Build
		Navigate to the built executable : TwitchBotGUI > WpfApplication1 > bin > Release
		Copy DudeBot.exe to DudeBot folder (overwriting old one)
		*For dudebotupdater.exe and Images.exe put them in the bin folder (overwriting old ones)
				
Current issues as of 8/24/2017:
.NET TwitchBotGUI needs MAJOR refactoring. 
Updating process needs refactoring - currently uses many text files, should just use one GSON file to share between Java and .NET
Auto updating will need to be changed as well - as of right now it checks dudebot.webs.com for current version to see if it is outdated.
	This will need to be switched. Also it uses DropBox to pull the files, this link will need to be changed because only I have access to it.
Full Java version GUI (For Mac users) *This means starting the whole GUI from scratch
YoutubePlayer needs to be properly implemented (deleted old project because it didn't work and it was 2 GB)

***If you make some worthwhile improvements and everything checks out, 
let me know and create a pull request and I'll make an update on the dudebot.webs.com site.