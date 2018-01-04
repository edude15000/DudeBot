DudeBot Developer Setup Instructions

Follow the readme in the DudeBot folder first to set up a Twitch Bot account for testing and development.

Folders:
DudeBot - The actual redistributable DudeBot folder for users
TwitchBot - The Twitch Bot project folder 
Updater - The Automatic Updater project folder (.NET)
DudeBotConfigUpdater - Config updater project (2.0 -> 3.0)

How to update code: In IDE (Microsoft Visual Studio), go to Build > Batch Build...
		Make sure Release Any CPU us checked for Build and click Build
		Navigate to the built executable : TwitchBotGUI > WpfApplication1 > bin > Release
		Copy DudeBot.exe to DudeBot folder (overwriting old one)
		*For dudebotupdater.exe and dudebotconfigupdater put them in the bin folder (overwriting old ones)
				