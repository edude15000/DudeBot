using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public class Image
{
    public long imageStartTime = (long)0, imageOverallCoolDown, imageCoolDown;
    public int imageDisplayTimeSeconds;
    public Boolean openImageWindowOnStart;
    public Dictionary<String, long> userCoolDowns = new Dictionary<String, long>();

    public void imageCOMMANDS(String message, String channel, String sender, List<Command> comList)
    {
        try
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
                                    try
                                    {
                                        StreamWriter writer = new StreamWriter(Path.GetTempPath() + "dudebotimage.txt");
                                        writer.Write(comList[i].output);
                                        writer.Close();
                                        userCoolDowns.Add(sender, Environment.TickCount);
                                        imageStartTime = Environment.TickCount;
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
                            StreamWriter writer = new StreamWriter(Path.GetTempPath() + "dudebotimage.txt");
                            writer.Write(comList[i].output);
                            writer.Close();
                            userCoolDowns.Add(sender, Environment.TickCount);
                            imageStartTime = Environment.TickCount;
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
        catch (Exception e)
        {
            Utils.errorReport(e);
            Debug.WriteLine(e.ToString());
        }
    }
}
