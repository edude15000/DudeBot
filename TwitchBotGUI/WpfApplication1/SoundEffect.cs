
using System;
using System.Collections.Generic;

public class SoundEffect
{
    public int sfxTimer, SFXOverallCoolDown;
    public long SFXstartTime = 0;
    public Dictionary<String, long> userCoolDowns = new Dictionary<String, long>();

    public void sfxCOMMANDS(String message, String channel, String sender, List<Command> comList)
    {
        for (int i = 0; i < comList.Count; i++)
        {
            String temp = message.ToLower();
            if (temp.StartsWith(comList.get(i).input[0]))
            {
                if (SFXstartTime == 0 || (System.currentTimeMillis() >= SFXstartTime + (SFXOverallCoolDown * 1000)))
                {
                    for (int j = 0; j < userCoolDowns.size(); j++)
                    {
                        if (userCoolDowns.get(sender) != null)
                        {
                            if (System.currentTimeMillis() >= userCoolDowns.get(sender) + (sfxTimer * 1000))
                            {
                                try
                                {
                                    FileInputStream fis = new FileInputStream(comList.get(i).output);
                                    Player playMP3 = new Player(fis);
                                    playMP3.play();
                                    userCoolDowns.put(sender, System.currentTimeMillis());
                                    SFXstartTime = System.currentTimeMillis();
                                }
                                catch (Exception e)
                                {
                                    Utils.errorReport(e);
                                    e.ToString();
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
                        FileInputStream fis = new FileInputStream(comList.get(i).output);
                        Player playMP3 = new Player(fis);
                        playMP3.play();
                        userCoolDowns.put(sender, System.currentTimeMillis());
                        SFXstartTime = System.currentTimeMillis();
                    }
                    catch (Exception e)
                    {
                        Utils.errorReport(e);
                        e.ToString();
                    }
                    return;
                }
            }
        }
    }
}
