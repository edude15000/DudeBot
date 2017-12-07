using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

public class TextAdventure
{
    [JsonIgnore]
    public List<String> users { get; set; } = new List<String>();
    [JsonIgnore]
    public Dictionary<String, String[]> text { get; set; } = new Dictionary<String, String[]>();
    [JsonIgnore]
    public String choice { get; set; } = null;
    public long startTimerInMS { get; set; }
    [JsonIgnore]
    public long adventureStartTime { get; set; }
    [JsonIgnore]
    public Boolean allowUserAdds { get; set; } = true;
    
    public Boolean enoughPlayers { get; set; } = false;
    public int adventurePointsMin { get; set; }
    public int adventurePointsMax { get; set; }
    public int adventureCoolDown { get; set; }

    public void startAdventuring(List<String> users, int startTimerInMS)
    {
        allowUserAdds = true;
        enoughPlayers = false;
        setUpText();
        this.users = users;
        this.startTimerInMS = startTimerInMS;
    }

    public void setUpText()
    {
        text.Clear();
        String[] a = { "Sadly, the spaceship everyone was on exploded, and the crew is now dead in space.",
                "Many casualties occured, but $user was lucky enough to escape with space coins!",
                "$users were able to successfully raid a group of space pirates and escaped with space coins!" };
        text.Add("$user wants to get a posse together to raid some space pirates.", a);
        String[] b = { "Everyone died, they were no match for the hoes.",
                "$user ran away while the hoes were distracted by the rest of group and manage to aquire some loots.",
                "$users were too badass and decimated the hoes while the rest were slaughtered." };
        text.Add("$user wants to be a badass and raid the castle of undead hoes.", b);
        String[] c = { "Sadly there is no intelligent life out there only brain slugs, the whole crew is dead.",
                "The spaceship crashes on a distant planet with only $user left, they find a utopia of sexy ladies and dudes that grant them riches.",
                "$users were actually corperate spies sent to sabatoge the mission, they succeeded and now reap the rewards." };
        text.Add("$user wants to be the captain of the a starship designed to find intelligent life on distant worlds.",
                c);
        String[] d = { "Everyone dies cause magical goo is just your ordinary toxic waste.",
                "$user was able to survive the harmful effects of the goo while the rest must endure the lives of misable abominations.  $user steals their wallets.",
                "$users become the most powerful beings in the universe and instead of using their powers for good they just use them for profit." };
        text.Add("$user wants to expose everyone to magical goo to make them super heroes.", d);
        String[] f = { "The jedi use thier cunning and force push spam to defeat everyone.",
                "At last $user has their revenge, at last the galaxy is theirs!",
                "At last $users have their revenge, at last the galaxy is theirs!" };
        text.Add("$user wants to get revenge on the jedi.", f);
        String[] g = {
                "People went and saw the latest pop act and didn't bother, congratulations everyone you killed rock.",
                "$user was singed by the record label and abandoned their bandmates to sell out.",
                "$users have formed the greatest rock band known to mankind and are planning a space tour." };
        text.Add("$user wants to get a mega rock band together for a amazing arena show.", g);
        String[] h = { "The dragon decimated the party, dragons are dicks.",
                "$user was valient and true, nothing could stop them in slaying the dragon, ashame what happened to the rest.",
                "$users deciced to go to the pub instead while everyone else was decimated but it was free ale night so it wasn't all bad." };
        text.Add("$user wants to slay the dragon and get it's loot.", h);
        String[] i = { "The petty uprising was crushed before it even began, hail to the king baby.",
                "$user dethroned the king and rather then set up a republic took the crown for themselves and betrayed all those who help them.",
                "$users were exiled but set up their own kingdom, a better kingdom with black jack and hookers." };
        text.Add(" $user wants to overthrow the evil king.", i);
        String[] j = {
                "Everyone lost all their money and are now forced to do horrible demeaning things to make it back home.",
                "$user won big at the black jack tables but everyone else owed mobsters money and are now burried somewhere in the desert.",
                "$users all won showcases and are living it large in their own private suites." };
        text.Add("$user wants to get a road trip together to go to Vegas and win big.", j);
        String[] k = { "Too late, Pinky and Brain have already done so.",
                "$user using a deadly 'lazer' was able to hold the world hostage and everyone submitted to them as supreme ruler.",
                "$users created an army of robotic badgers that brought the world to it's knees and gained them total supremacy." };
        text.Add("$user wants to take over the world.", k);

        // ADD AS NEEDED
    }

    public String getText()
    {
        choice = new List<String>(text.Keys)[new Random().Next(text.Count)];
        return choice;
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

    public String winningMessage(List<String> winners)
    {
        users.Clear();
        if (winners.Count == 0)
        {
            return text[choice][0];
        }
        else if (winners.Count == 1)
        {
            return text[choice][1];
        }
        else
        {
            return text[choice][2];
        }
    }
}