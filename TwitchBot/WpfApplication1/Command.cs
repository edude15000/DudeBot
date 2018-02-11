using Newtonsoft.Json;
using System;
using System.Diagnostics;
using WMPLib;
using WpfApplication1;

public class Command 
{
    public String output { get; set; }
    public String commandType { get; set; }
    public int level { get; set; } = 0;
    public String[] input { get; set; }
    public Boolean toggle { get; set; }
    public int volumeLevel { get; set; } = 100;
    public int costToUse { get; set; } = 0;
    public int overrideType { get; set; } = 0;
    [JsonIgnore]
    public WindowsMediaPlayer myplayer;

    public Command(String[] input, int level, String output, String commandType, Boolean toggle)
    {
        this.input = input;
        this.level = level;
        this.output = output;
        this.toggle = toggle;
        this.commandType = commandType;
        volumeLevel = 100;
        costToUse = 0;
        overrideType = 0;
    }

    [STAThread]
    public void playSound()
    {
        try
        {
            myplayer = new WindowsMediaPlayer();
            myplayer.URL = output;
            myplayer.settings.volume = volumeLevel;
            myplayer.controls.play();
        }
        catch (Exception e)
        {
            Utils.errorReport(e);
            Debug.WriteLine(e.ToString());
        }
    }

    [STAThread]
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