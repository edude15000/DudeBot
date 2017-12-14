﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class SoundEffect : INotifyPropertyChanged
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
    public int SfxTimer = 60;
    public int sfxTimer
    {
        get => SfxTimer;
        set { SetField(ref SfxTimer, value, nameof(sfxTimer)); }
    }
    [JsonIgnore]
    public int _SFXOverallCoolDown = 0;
    public int SFXOverallCoolDown
    {
        get => _SFXOverallCoolDown;
        set { SetField(ref _SFXOverallCoolDown, value, nameof(SFXOverallCoolDown)); }
    }
    [JsonIgnore]
    public long SFXstartTime { get; set; } = 0;
    [JsonIgnore]
    public Dictionary<String, long> userCoolDowns { get; set; } = new Dictionary<String, long>();

    public void sfxCOMMANDS(String message, String channel, BotUser b, List<Command> comList)
    {
        for (int i = 0; i < comList.Count; i++)
        {
            String temp = message.ToLower();
            if (temp.StartsWith(comList[i].input[0]))
            {
                if (SFXstartTime == 0 || (Environment.TickCount >= SFXstartTime + (SFXOverallCoolDown * 1000)))
                {
                    for (int j = 0; j < userCoolDowns.Count; j++)
                    {
                        if (userCoolDowns[b.username] != 0)
                        {
                            if (Environment.TickCount >= userCoolDowns[b.username] + (sfxTimer * 1000))
                            {
                                if (b.points >= comList[i].costToUse)
                                {
                                    b.points -= comList[i].costToUse;
                                    comList[i].playSound();
                                    userCoolDowns.Add(b.username, Environment.TickCount);
                                    SFXstartTime = Environment.TickCount;
                                }
                                return;
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                    if (b.points >= comList[i].costToUse)
                    {
                        b.points -= comList[i].costToUse;
                        comList[i].playSound();
                        userCoolDowns.Add(b.username, Environment.TickCount);
                        SFXstartTime = Environment.TickCount;
                    }
                    return;
                }
            }
        }
    }

    
}
