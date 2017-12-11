using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using WMPLib;

public class SoundEffect
{
    public int sfxTimer { get; set; }
    public int SFXOverallCoolDown { get; set; }
    [JsonIgnore]
    public long SFXstartTime { get; set; } = 0;
    [JsonIgnore]
    public Dictionary<String, long> userCoolDowns { get; set; } = new Dictionary<String, long>();

    public void sfxCOMMANDS(String message, String channel, String sender, List<Command> comList)
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
                        if (userCoolDowns[sender] != 0)
                        {
                            if (Environment.TickCount >= userCoolDowns[sender] + (sfxTimer * 1000))
                            {
                                comList[i].playSound();
                                userCoolDowns.Add(sender, Environment.TickCount);
                                SFXstartTime = Environment.TickCount;
                                return;
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                    comList[i].playSound();
                    userCoolDowns.Add(sender, Environment.TickCount);
                    SFXstartTime = Environment.TickCount;
                    return;
                }
            }
        }
    }

    
}
