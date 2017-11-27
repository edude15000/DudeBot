using System;
using System.Collections.Generic;
using System.Diagnostics;
using WMPLib;

public class SoundEffect // TODO : TEST!
{
    public int sfxTimer, SFXOverallCoolDown;
    public long SFXstartTime = 0;
    public Dictionary<String, long> userCoolDowns = new Dictionary<String, long>();

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
                                try
                                {
                                    WindowsMediaPlayer myplayer = new WindowsMediaPlayer();
                                    myplayer.URL = comList[i].output;
                                    myplayer.controls.play();
                                    userCoolDowns.Add(sender, Environment.TickCount);
                                    SFXstartTime = Environment.TickCount;
                                }
                                catch (Exception e)
                                {
                                    Utils.errorReport(e);
                                    Debug.WriteLine(e.ToString());
                                }
                                return;
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                    try
                    {
                        WindowsMediaPlayer myplayer = new WindowsMediaPlayer();
                        myplayer.URL = comList[i].output;
                        myplayer.controls.play();
                        userCoolDowns.Add(sender, Environment.TickCount);
                        SFXstartTime = Environment.TickCount;
                    }
                    catch (Exception e)
                    {
                        Utils.errorReport(e);
                        Debug.WriteLine(e.ToString());
                    }
                    return;
                }
            }
        }
    }
}
