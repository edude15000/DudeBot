public class Command {

	String output;
	int level;
	String[] input;
	boolean toggle;
	String commandType;

	public Command(String[] input, int level, String output, String commandType, boolean toggle) {
		this.input = input;
		this.level = level;
		this.output = output;
		this.toggle = toggle;
		this.commandType = commandType;
	}

}