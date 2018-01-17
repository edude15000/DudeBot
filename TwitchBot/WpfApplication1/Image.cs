using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TwitchLib.Events.Client;

public class Image : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    protected bool SetField<T>(ref T field, T value, string propertyName)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
    [JsonIgnore]
    public long imageStartTime { get; set; } = 0;
    [JsonIgnore]
    public long ImageOverallCoolDown = 0;
    public long imageOverallCoolDown
    {
        get => ImageOverallCoolDown;
        set { SetField(ref ImageOverallCoolDown, value, nameof(imageOverallCoolDown)); }
    }
    [JsonIgnore]
    public long ImageCoolDown = 60;
    public long imageCoolDown
    {
        get => ImageCoolDown;
        set { SetField(ref ImageCoolDown, value, nameof(imageCoolDown)); }
    }
    [JsonIgnore]
    public int ImageDisplayTimeSeconds = 3;
    public int imageDisplayTimeSeconds
    {
        get => ImageDisplayTimeSeconds;
        set { SetField(ref ImageDisplayTimeSeconds, value, nameof(imageDisplayTimeSeconds)); }
    }
    [JsonIgnore]
    public Boolean OpenImageWindowOnStart = false;
    public Boolean openImageWindowOnStart
    {
        get => OpenImageWindowOnStart;
        set { SetField(ref OpenImageWindowOnStart, value, nameof(openImageWindowOnStart)); }
    }
    [JsonIgnore]
    public Dictionary<String, long> userCoolDowns { get; set; } = new Dictionary<String, long>();

    public void imageCOMMANDS(String message, String channel, BotUser b, List<Command> comList, TwitchBot bot, OnMessageReceivedArgs e)
    {
        for (int i = 0; i < comList.Count; i++)
        {
            String temp = message.ToLower();
            if (temp.StartsWith(comList[i].input[0]))
            {
                if (!bot.checkUserLevel(b.username, comList[i].level, channel, e))
                {
                    return;
                }
                if (imageStartTime == 0
                        || (DateTimeOffset.Now.ToUnixTimeMilliseconds() >= imageStartTime + (imageOverallCoolDown * 1000)))
                {
                    for (int j = 0; j < userCoolDowns.Count; j++)
                    {
                        if (userCoolDowns[b.username] != 0)
                        {
                            if (DateTimeOffset.Now.ToUnixTimeMilliseconds() >= userCoolDowns[b.username] + (imageCoolDown * 1000))
                            {
                                if (comList[i].costToUse == 0 || b.points >= comList[i].costToUse)
                                {
                                    if (comList[i].costToUse != 0)
                                    {
                                        b.points -= comList[i].costToUse;
                                    }
                                    comList[i].playImage();
                                    imageStartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                                    if (userCoolDowns.ContainsKey(b.username))
                                    {
                                        userCoolDowns.Remove(b.username);
                                    }
                                    userCoolDowns[b.username] = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                                }
                                return;
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                    if (comList[i].costToUse == 0 || b.points >= comList[i].costToUse)
                    {
                        if (comList[i].costToUse != 0)
                        {
                            b.points -= comList[i].costToUse;
                        }
                        comList[i].playImage();
                        imageStartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                        if (userCoolDowns.ContainsKey(b.username))
                        {
                            userCoolDowns.Remove(b.username);
                        }
                        userCoolDowns[b.username] = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    }
                    return;
                }
            }
        }
    }
}
