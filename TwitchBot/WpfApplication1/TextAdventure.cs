using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

public class TextAdventure : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    protected bool SetField<T>(ref T field, T value, string propertyName)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    [JsonIgnore]
    public List<String> users { get; set; } = new List<String>();
    [JsonIgnore]
    public List<String> usersCustom { get; set; } = new List<String>();
    [JsonIgnore]
    public AdventureScenario choice { get; set; } = null;
    [JsonIgnore]
    public long startTimerInMS { get; set; } = 0;
    [JsonIgnore]
    public long startTimerInMSCustom { get; set; } = 0;
    [JsonIgnore]
    public Boolean allowUserAdds { get; set; } = true;
    [JsonIgnore]
    public Boolean allowUserAddsCustom { get; set; } = true;
    [JsonIgnore]
    public Boolean enoughPlayers { get; set; } = false;
    [JsonIgnore]
    public long lastAdventure = 0;
    [JsonIgnore]
    public long lastAdventureCustom = 0;
    [JsonIgnore]
    public long AdventureStartTime = 90;
    public long adventureStartTime
    {
        get => AdventureStartTime;
        set { SetField(ref AdventureStartTime, value, nameof(adventureStartTime)); }
    }
    [JsonIgnore]
    public int AdventurePointsMin = 25;
    public int adventurePointsMin
    {
        get => AdventurePointsMin;
        set { SetField(ref AdventurePointsMin, value, nameof(adventurePointsMin)); }
    }
    [JsonIgnore]
    public int AdventurePointsMax = 75;
    public int adventurePointsMax
    {
        get => AdventurePointsMax;
        set { SetField(ref AdventurePointsMax, value, nameof(adventurePointsMax)); }
    }
    [JsonIgnore]
    public int AdventureCoolDown = 20;
    public int adventureCoolDown
    {
        get => AdventureCoolDown;
        set { SetField(ref AdventureCoolDown, value, nameof(adventureCoolDown)); }
    }
    [JsonIgnore]
    public String CustomGameCommand = "!heist";
    public String customGameCommand
    {
        get => CustomGameCommand;
        set { SetField(ref CustomGameCommand, value, nameof(customGameCommand)); }
    }
    [JsonIgnore]
    public Boolean CustomGameStartByModOnly = false;
    public Boolean customGameStartByModOnly
    {
        get => CustomGameStartByModOnly;
        set { SetField(ref CustomGameStartByModOnly, value, nameof(customGameStartByModOnly)); }
    }
    [JsonIgnore]
    public int Customadventurejoincost = 50;
    public int customadventurejoincost
    {
        get => Customadventurejoincost;
        set { SetField(ref Customadventurejoincost, value, nameof(customadventurejoincost)); }
    }
    [JsonIgnore]
    public int Customadventurecooldowntime = 20;
    public int customadventurecooldowntime
    {
        get => Customadventurecooldowntime;
        set { SetField(ref Customadventurecooldowntime, value, nameof(customadventurecooldowntime)); }
    }
    [JsonIgnore]
    public int Customadventurejointime = 90;
    public int customadventurejointime
    {
        get => Customadventurejointime;
        set { SetField(ref Customadventurejointime, value, nameof(customadventurejointime)); }
    }
    [JsonIgnore]
    public String CustomGameStartMessage = "$user has initiated a bank heist! Type '!heist' to join! It costs $cost to join!";
    public String customGameStartMessage
    {
        get => CustomGameStartMessage;
        set { SetField(ref CustomGameStartMessage, value, nameof(customGameStartMessage)); }
    }
    [JsonIgnore]
    public String CustomGameEndMessage = "$user manages to be the only one to make it out alive and runs away with $amount!";
    public String customGameEndMessage
    {
        get => CustomGameEndMessage;
        set { SetField(ref CustomGameEndMessage, value, nameof(customGameEndMessage)); }
    }

    public void startAdventuringCustom(List<String> users, int startTimerInMS)
    {
        allowUserAddsCustom = true;
        usersCustom = users;
        startTimerInMSCustom = startTimerInMS;
    }

    public void startAdventuring(List<String> users, int startTimerInMS, List<AdventureScenario> list)
    {
        allowUserAdds = true;
        enoughPlayers = false;
        choice = getRandomAdventure(list);
        this.users = users;
        this.startTimerInMS = startTimerInMS;
    }

    public List<AdventureScenario> stockAdventures()
    {
        return new List<AdventureScenario>
        {
            { new AdventureScenario("spacepirates", "$user wants to get a posse together to raid some space pirates.", 
            "Sadly, the spaceship everyone was on exploded, and the crew is now dead in space.",
            "Many casualties occured, but $user was lucky enough to escape with space coins!", 
            "$users were able to successfully raid a group of space pirates and escaped with space coins!") },
            { new AdventureScenario("undeadhoes", "$user wants to be a badass and raid the castle of undead hoes.",
            "Everyone died, they were no match for the hoes.",
            "$user ran away while the hoes were distracted by the rest of group and manage to aquire some loots.",
            "$users were too badass and decimated the hoes while the rest were slaughtered.") },
            { new AdventureScenario("starship", "$user wants to be the captain of the a starship designed to find intelligent life on distant worlds.",
            "Sadly there is no intelligent life out there only brain slugs, the whole crew is dead.",
            "The spaceship crashes on a distant planet with only $user left, they find a utopia of sexy ladies and dudes that grant them riches.",
            "$users were actually corperate spies sent to sabatoge the mission, they succeeded and now reap the rewards.") },
            { new AdventureScenario("magicalgoo", "$user wants to expose everyone to magical goo to make them super heroes.",
            "Everyone dies cause magical goo is just your ordinary toxic waste.",
            "$user was able to survive the harmful effects of the goo while the rest must endure the lives of misable abominations. $user steals their wallets.",
            "$users become the most powerful beings in the universe and instead of using their powers for good they just use them for profit.") },
            { new AdventureScenario("jedirevenge", "$user wants to get revenge on the jedi.",
            "The jedi use thier cunning and force push spam to defeat everyone.",
            "At last $user has their revenge, at last the galaxy is theirs!",
            "At last $users have their revenge, at last the galaxy is theirs!") },
            { new AdventureScenario("rockband", "$user wants to get a mega rock band together for a amazing arena show.",
            "People went and saw the latest pop act and didn't bother, congratulations everyone you killed rock.",
            "$user was singed by the record label and abandoned their bandmates to sell out.",
            "$users have formed the greatest rock band known to mankind and are planning a space tour.") },
            { new AdventureScenario("dragon", "$user wants to slay the dragon and get it's loot.",
            "The dragon decimated the party, dragons are dicks.",
            "$user was valient and true, nothing could stop them in slaying the dragon, ashame what happened to the rest.",
            "$users deciced to go to the pub instead while everyone else was decimated but it was free ale night so it wasn't all bad.") },
            { new AdventureScenario("evilking", "$user wants to overthrow the evil king.",
            "The petty uprising was crushed before it even began, hail to the king baby.",
            "$user dethroned the king and rather then set up a republic took the crown for themselves and betrayed all those who help them.",
            "$users were exiled but set up their own kingdom, a better kingdom with black jack and hookers.") },
            { new AdventureScenario("vegastrip", "$user wants to get a road trip together to go to Vegas and win big.",
            "Everyone lost all their money and are now forced to do horrible demeaning things to make it back home.",
            "$user won big at the black jack tables but everyone else owed mobsters money and are now burried somewhere in the desert.",
            "$users all won showcases and are living it large in their own private suites.") },
             { new AdventureScenario("conquerworld", "$user wants to take over the world.",
            "Too late, Pinky and Brain have already done so.",
            "$user using a deadly 'lazer' was able to hold the world hostage and everyone submitted to them as supreme ruler.",
            "$users created an army of robotic badgers that brought the world to it's knees and gained them total supremacy.") }
        };
    }

    public AdventureScenario getRandomAdventure(List<AdventureScenario> list)
    {
        return list[Utils.getRandomNumber(list.Count)];
    }

    public void start(String user)
    {
        allowUserAdds = true;
        enoughPlayers = false;
        users.Clear();
        addUser(user);
        try
        {
            Thread.Sleep((int)startTimerInMS);
        }
        catch (Exception e)
        {
            Utils.errorReport(e);
            Debug.WriteLine(e.ToString());
        }
        if (users.Count >= 3)
        {
            enoughPlayers = true;
        }
        allowUserAdds = false;
    }

    public void startCustom(String user)
    {
        allowUserAddsCustom = true;
        usersCustom.Clear();
        addUserCustom(user);
        try
        {
            Thread.Sleep((int)startTimerInMSCustom);
        }
        catch (Exception e)
        {
            Utils.errorReport(e);
            Debug.WriteLine(e.ToString());
        }
        allowUserAddsCustom = false;
    }

    public int addUser(String user)
    {
        foreach (String s in users)
        {
            if (s.Equals(user, StringComparison.InvariantCultureIgnoreCase))
            {
                return -1;
            }
        }
        if (allowUserAdds)
        {
            users.Add(user);
            return 1;
        }
        else
        {
            return 0;
        }
    }

    public int addUserCustom(String user)
    {
        foreach (String s in users)
        {
            if (s.Equals(user, StringComparison.InvariantCultureIgnoreCase))
            {
                return -1;
            }
        }
        if (allowUserAdds)
        {
            usersCustom.Add(user);
            return 1;
        }
        else
        {
            return 0;
        }
    }

    public List<String> selectWinners()
    {
        int chance = new Random().Next(100) + 1;
        int count = 0;
        if (chance <= 25)
        {
            count = 0;
        }
        else if (chance <= 50)
        {
            count = 1;
        }
        else if (chance <= 75)
        {
            count = 2;
        }
        else
        {
            count = 3;
        }
        List<String> listToReturn = new List<String>();
        for (int i = 0; i < count; i++)
        {
            String userToRemove = users[new Random().Next(users.Count - 1)];
            listToReturn.Add(userToRemove);
            users.Remove(userToRemove);
        }
        return listToReturn;
    }

    public String selectWinnerCustom()
    {
        return usersCustom[new Random().Next(usersCustom.Count - 1)]; ;
    }

    public String winningMessage(List<String> winners)
    {
        users.Clear();
        if (choice == null)
        {
            return "";
        }
        if (winners.Count == 0)
        {
            return choice.allLosersMessage;
        }
        else if (winners.Count == 1)
        {
            return choice.oneWinnerMessage;
        }
        else
        {
            return choice.multipleWinnersMessage;
        }
    }
}