import com.google.gson.annotations.Expose;

public class Command {

	@Expose(serialize = true, deserialize = true)
	String output, commandType;
	@Expose(serialize = true, deserialize = true)
	int level;
	@Expose(serialize = true, deserialize = true)
	String[] input;
	@Expose(serialize = true, deserialize = true)
	boolean toggle;

	public Command(String[] input, int level, String output, String commandType, boolean toggle) {
		this.input = input;
		this.level = level;
		this.output = output;
		this.toggle = toggle;
		this.commandType = commandType;
	}

}