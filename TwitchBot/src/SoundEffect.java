import java.io.FileInputStream;
import java.util.ArrayList;
import java.util.HashMap;

import com.google.gson.annotations.Expose;

import javazoom.jl.player.Player;

public class SoundEffect {
	@Expose(serialize = true, deserialize = true)
	int sfxTimer, SFXOverallCoolDown;
	@Expose(serialize = true, deserialize = true)
	long SFXstartTime = 0;
	@Expose(serialize = true, deserialize = true)
	public HashMap<String, Long> userCoolDowns = new HashMap<String, Long>();

	public void sfxCOMMANDS(String message, String channel, String sender, ArrayList<Command> comList) {
		for (int i = 0; i < comList.size(); i++) {
			String temp = message.toLowerCase();
			if (temp.startsWith(comList.get(i).input[0])) {
				if (SFXstartTime == 0 || (System.currentTimeMillis() >= SFXstartTime + (SFXOverallCoolDown * 1000))) {
					for (int j = 0; j < userCoolDowns.size(); j++) {
						if (userCoolDowns.get(sender) != null) {
							if (System.currentTimeMillis() >= userCoolDowns.get(sender) + (sfxTimer * 1000)) {
								try {
									FileInputStream fis = new FileInputStream(comList.get(i).output);
									Player playMP3 = new Player(fis);
									playMP3.play();
									userCoolDowns.put(sender, System.currentTimeMillis());
									SFXstartTime = System.currentTimeMillis();
								} catch (Exception e) {
									Utils.errorReport(e);
									System.out.println(e);
								}
								return;
							} else {
								return;
							}
						}
					}
					try {
						FileInputStream fis = new FileInputStream(comList.get(i).output);
						Player playMP3 = new Player(fis);
						playMP3.play();
						userCoolDowns.put(sender, System.currentTimeMillis());
						SFXstartTime = System.currentTimeMillis();
					} catch (Exception e) {
						Utils.errorReport(e);
						System.out.println(e);
					}
					return;
				}
			}
		}
	}
}
