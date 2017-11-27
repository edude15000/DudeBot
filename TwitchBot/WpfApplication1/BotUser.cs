using System;

public class BotUser
{ 
    public String username, rank;
    public Boolean follower, sub, mod, gaveSpot = false;
    public int time, subCredits, points, numRequests;
    public long gambleCoolDown, vipCoolDown;

    public BotUser(String username, int numRequests, Boolean mod, Boolean follower, Boolean regular, int points,
            int time, String rank, long vipCoolDown, long gambleCoolDown, int subCredits)
    {
        this.username = username;
        this.numRequests = numRequests;
        this.mod = mod;
        this.follower = follower;
        this.sub = regular;
        this.points = points;
        this.time = time;
        this.rank = rank;
        this.vipCoolDown = vipCoolDown;
        this.gambleCoolDown = gambleCoolDown;
        this.subCredits = subCredits;
    }

}