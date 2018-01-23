using System;

public class AdventureScenario
{
    public String startMessage { get; set; }
    public String allLosersMessage { get; set; }
    public String oneWinnerMessage { get; set; }
    public String multipleWinnersMessage { get; set; }
    public String name { get; set; }

    public AdventureScenario(String name, String startMessage, String allLosersMessage, String oneWinnerMessage, String multipleWinnersMessage)
    {
        this.startMessage = startMessage;
        this.allLosersMessage = allLosersMessage;
        this.oneWinnerMessage = oneWinnerMessage;
        this.multipleWinnersMessage = multipleWinnersMessage;
        this.name = name;
    }
}