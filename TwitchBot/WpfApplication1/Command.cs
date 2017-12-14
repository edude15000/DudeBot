using System;
using System.Diagnostics;
using WpfApplication1;

public class Command 
{
    public String output { get; set; }
    public String commandType { get; set; }
    public int level { get; set; }
    public String[] input { get; set; }
    public Boolean toggle { get; set; }
    public int volumeLevel { get; set; } = 100;
    public int costToUse { get; set; } = 0;


    public Command(String[] input, int level, String output, String commandType, Boolean toggle)
    {
        this.input = input;
        this.level = level;
        this.output = output;
        this.toggle = toggle;
        this.commandType = commandType;
        volumeLevel = 100;
        costToUse = 0;
    }

    public void playSound()
    {
        try
        {
            MainWindow.bot.myplayer.URL = output;
            MainWindow.bot.myplayer.settings.volume = volumeLevel;
            MainWindow.bot.myplayer.controls.play();
        }
        catch (Exception e)
        {
            Utils.errorReport(e);
            Debug.WriteLine(e.ToString());
        }
    }

    public void playImage()
    {
        try
        {
            App.imagesWindow.displayImage(output);
        }
        catch (Exception e)
        {
            Utils.errorReport(e);
            Debug.WriteLine(e.ToString());
        }
    }

}