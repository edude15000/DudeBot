import com.google.gson.annotations.Expose;

public class BotUser {
	@Expose(serialize = true, deserialize = true)
	String username;
	@Expose(serialize = true, deserialize = true)
	int numRequests;
	@Expose(serialize = true, deserialize = true)
	boolean gaveSpot = false;
	@Expose(serialize = true, deserialize = true)
	boolean mod;
	@Expose(serialize = true, deserialize = true)
	boolean follower;
	@Expose(serialize = true, deserialize = true)
	boolean sub;
	@Expose(serialize = true, deserialize = true)
	int points;
	@Expose(serialize = true, deserialize = true)
	int time;
	@Expose(serialize = true, deserialize = true)
	String rank;
	@Expose(serialize = true, deserialize = true)
	long vipCoolDown;
	@Expose(serialize = true, deserialize = true)
	long gambleCoolDown;
	@Expose(serialize = true, deserialize = true)
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