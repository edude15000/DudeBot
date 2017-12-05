using System;

public class Command
{
    public String output { get; set; }
    public String commandType { get; set; }
    public int level { get; set; }
    public String[] input { get; set; }
    public Boolean toggle { get; set; }

    public Command(String[] input, int level, String output, String commandType, Boolean toggle)
    {
        this.input = input;
        this.level = level;
        this.output = output;
        this.toggle = toggle;
        this.commandType = commandType;
    }

}