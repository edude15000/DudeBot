using System;

public class Command
{
    public String output, commandType;
    public int level;
    public String[] input;
    public Boolean toggle;

    public Command(String[] input, int level, String output, String commandType, Boolean toggle)
    {
        this.input = input;
        this.level = level;
        this.output = output;
        this.toggle = toggle;
        this.commandType = commandType;
    }

}