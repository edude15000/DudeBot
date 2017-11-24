import java.io.File;
import java.io.FileWriter;
import java.util.ArrayList;
import java.util.HashMap;

import com.google.gson.annotations.Expose;

public class Image {
	@Expose(serialize = true, deserialize = true)
	Long imageStartTime = (long) 0, imageOverallCoolDown, imageCoolDown;
	@Expose(serialize = true, deserialize = true)
	int imageDisplayTimeSeconds;
	@Expose(serialize = true, deserialize = true)
	boolean openImageWindowOnStart;
	@Expose(serialize = true, deserialize = true)
	public HashMap<String, Long> userCoolDowns = new HashMap<String, Long>();

	public void imageCOMMANDS(String message, String channel, String sender, ArrayList<Command> comList) {
		try {
			for (int i = 0; i < comList.size(); i++) {
				String temp = message.toLowerCase();
				if (temp.startsWith(comList.get(i).input[0])) {
					if (imageStartTime == 0
							|| (System.currentTimeMillis() >= imageStartTime + (imageOverallCoolDown * 1000))) {
						for (int j = 0; j < userCoolDowns.size(); j++) {
							if (userCoolDowns.get(sender) != null) {
								if (System.currentTimeMillis() >= userCoolDowns.get(sender) + (imageCoolDown * 1000)) {
									try {
										File f = new File(System.getProperty("java.io.tmpdir") + "dudebotimage.txt");
										FileWriter writer = new FileWriter(f, false);
										writer.write(comList.get(i).output);
										writer.close();
										userCoolDowns.put(sender, System.currentTimeMillis());
										imageStartTime = System.currentTimeMillis();
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
							File f = new File(System.getProperty("java.io.tmpdir") + "dudebotimage.txt");
							FileWriter writer = new FileWriter(f, false);
							writer.write(comList.get(i).output);
							writer.close();
							userCoolDowns.put(sender, System.currentTimeMillis());
							imageStartTime = System.currentTimeMillis();
						} catch (Exception e) {
							Utils.errorReport(e);
							System.out.println(e);
						}
						return;
					}
				}
			}
		} catch (Exception e) {
			Utils.errorReport(e);
			System.out.println(e);
		}
	}
}
