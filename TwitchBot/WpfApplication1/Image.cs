using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class Image
{
    [JsonIgnore]
    public long imageStartTime { get; set; } = (long)0;
    public long imageOverallCoolDown { get; set; }
    public long imageCoolDown { get; set; }
    public int imageDisplayTimeSeconds { get; set; }
    public Boolean openImageWindowOnStart { get; set; }
    [JsonIgnore]
    public Dictionary<String, long> userCoolDowns { get; set; } = new Dictionary<String, long>();

    public void imageCOMMANDS(String message, String channel, String sender, List<Command> comList)
    {
        for (int i = 0; i < comList.Count; i++)
        {
            String temp = message.ToLower();
            if (temp.StartsWith(comList[i].input[0]))
            {
                if (imageStartTime == 0
                        || (Environment.TickCount >= imageStartTime + (imageOverallCoolDown * 1000)))
                {
                    for (int j = 0; j < userCoolDowns.Count; j++)
                    {
                        if (userCoolDowns[sender] != 0)
                        {
                            if (Environment.TickCount >= userCoolDowns[sender] + (imageCoolDown * 1000))
                            {
                                comList[i].playImage();
                                imageStartTime = Environment.TickCount;
                                userCoolDowns[sender] = Environment.TickCount;
                                return;
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                    comList[i].playImage();
                    imageStartTime = Environment.TickCount;
                    userCoolDowns[sender] = Environment.TickCount;
                    return;
                }
            }
        }
    }
}
