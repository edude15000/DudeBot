public class BotUser {
	String username;
	int numRequests;
	boolean gaveSpot = false;
	boolean mod;
	boolean follower;
	boolean sub;
	int points;
	int time;
	String rank;
	long vipCoolDown;
	long gambleCoolDown;
	int subCredits;

	public BotUser(String username, int numRequests, boolean mod, boolean follower, boolean regular, int points,
			int time, String rank, long vipCoolDown, long gambleCoolDown, int subCredits) {
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