using System;

public class BotUser 
{
    public String username { get; set; }
    public String rank { get; set; }
    public Boolean follower { get; set; }
    public Boolean sub { get; set; }
    public Boolean receivedFollowPayout { get; set; } = false;
    public Boolean mod { get; set; }
    public Boolean gaveSpot { get; set; } = false;
    public int time { get; set; }
    public int subCredits { get; set; }
    public int points { get; set; }
    public int numRequests { get; set; }
    public long months { get; set; } = 0;
    public long gambleCoolDown { get; set; }
    public long vipCoolDown { get; set; }

    public BotUser(String username, int numRequests, Boolean mod, Boolean follower, Boolean sub, int points,
            int time, String rank, long vipCoolDown, long gambleCoolDown, int subCredits)
    {
        this.username = username;
        this.numRequests = numRequests;
        this.mod = mod;
        this.follower = follower;
        this.sub = sub;
        this.points = points;
        this.time = time;
        this.rank = rank;
        this.vipCoolDown = vipCoolDown;
        this.gambleCoolDown = gambleCoolDown;
        this.subCredits = subCredits;
    }

}