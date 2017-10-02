import java.io.BufferedReader;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collections;
import java.util.List;

import com.google.api.client.auth.oauth2.Credential;
import com.google.api.client.extensions.java6.auth.oauth2.AuthorizationCodeInstalledApp;
import com.google.api.client.extensions.jetty.auth.oauth2.LocalServerReceiver;
import com.google.api.client.googleapis.auth.oauth2.GoogleAuthorizationCodeFlow;
import com.google.api.client.googleapis.auth.oauth2.GoogleClientSecrets;
import com.google.api.client.googleapis.javanet.GoogleNetHttpTransport;
import com.google.api.client.http.HttpTransport;
import com.google.api.client.json.JsonFactory;
import com.google.api.client.json.jackson2.JacksonFactory;
import com.google.api.client.util.store.FileDataStoreFactory;
import com.google.api.services.sheets.v4.Sheets;
import com.google.api.services.sheets.v4.SheetsScopes;
import com.google.api.services.sheets.v4.model.BatchUpdateValuesRequest;
import com.google.api.services.sheets.v4.model.ValueRange;

public class Google {
	private final String APPLICATION_NAME = "Google Sheets API Java Quickstart";
	private final static java.io.File DATA_STORE_DIR = new java.io.File(System.getProperty("user.home"),
			".credentials/sheets.googleapis.com-java-quickstart");
	static FileDataStoreFactory DATA_STORE_FACTORY;
	private final JsonFactory JSON_FACTORY = JacksonFactory.getDefaultInstance();
	static HttpTransport HTTP_TRANSPORT;
	final List<String> SCOPES = Arrays.asList(SheetsScopes.SPREADSHEETS);
	String spreadsheetId;
	Sheets service;
	static {
		try {
			HTTP_TRANSPORT = GoogleNetHttpTransport.newTrustedTransport();
			DATA_STORE_FACTORY = new FileDataStoreFactory(DATA_STORE_DIR);
		} catch (Throwable t) {
			t.printStackTrace();
			System.exit(1);
		}
	}

	public Credential authorize() throws IOException {
		File in;
		in = new File("client_secret.json");
		GoogleClientSecrets clientSecrets = GoogleClientSecrets.load(JSON_FACTORY, new FileReader(in));
		GoogleAuthorizationCodeFlow flow = new GoogleAuthorizationCodeFlow.Builder(HTTP_TRANSPORT, JSON_FACTORY,
				clientSecrets, SCOPES).setDataStoreFactory(DATA_STORE_FACTORY).setAccessType("offline").build();
		Credential credential = new AuthorizationCodeInstalledApp(flow, new LocalServerReceiver()).authorize("user");
		return credential;
	}

	public Sheets getSheetsService() throws IOException {
		Credential credential = authorize();
		return new Sheets.Builder(HTTP_TRANSPORT, JSON_FACTORY, credential).setApplicationName(APPLICATION_NAME)
				.build();
	}

	public void setValue(String RowStart, String file) throws IOException {
		String range = RowStart;
		ValueRange oRange = new ValueRange();
		oRange.setRange(range);
		oRange.setValues(getDataClear());
		List<ValueRange> oList = new ArrayList<>();
		oList.add(oRange);
		BatchUpdateValuesRequest oRequest = new BatchUpdateValuesRequest();
		oRequest.setValueInputOption("RAW");
		oRequest.setData(oList);
		service.spreadsheets().values().batchUpdate(spreadsheetId, oRequest).execute();
		List<List<Object>> arrData2 = getData(file);
		ValueRange oRange2 = new ValueRange();
		oRange2.setRange(range);
		oRange2.setValues(arrData2);
		List<ValueRange> oList2 = new ArrayList<>();
		oList2.add(oRange2);
		BatchUpdateValuesRequest oRequest2 = new BatchUpdateValuesRequest();
		oRequest2.setValueInputOption("RAW");
		oRequest2.setData(oList2);
		service.spreadsheets().values().batchUpdate(spreadsheetId, oRequest2).execute();
	}

	public List<List<Object>> getDataClear() throws NumberFormatException, FileNotFoundException, IOException {
		List<Object> data1 = new ArrayList<Object>();
		data1.add("");
		List<List<Object>> data = new ArrayList<List<Object>>();
		for (int i = 0; i < 100; i++) {
			data.add(data1);
		}
		return data;
	}

	public List<List<Object>> getData(String file) {
		List<List<Object>> data = new ArrayList<List<Object>>();
		try {
			BufferedReader br = new BufferedReader(new FileReader(file));
			String line;
			int count = 0;
			data.add(new ArrayList<Object>());
			if (file.equals("song.txt")) {
				if (count == 0) {
					data.get(0).add("CURRENT QUEUE:");
					count++;
				}
				while ((line = br.readLine()) != null) {
					if (line.equals("Song list is empty")) {
						br.close();
						return null;
					}
					data.add(new ArrayList<Object>());
					data.get(count).add(line);
					count++;
				}
			} else {
				while ((line = br.readLine()) != null) {
					data.add(new ArrayList<Object>());
					data.get(count).add(line);
					count++;
				}
				Collections.reverse(data);
				data.get(0).add("LAST PLAYED SONGS:");
			}
			br.close();
			return data;
		} catch (NumberFormatException | IOException e) {
			Utils.errorReport(e);
			e.printStackTrace();
		}
		return null;
	}

	public void writeToGoogleSheets(Boolean updateAllSongs, String songlistfile, String lastPlayedSongsFile) {
		try {
			setValue("A1", songlistfile);
			if (updateAllSongs) {
				setValue("B1", lastPlayedSongsFile);
			}
		} catch (Exception e) {
			Utils.errorReport(e);
			e.printStackTrace();
		}
	}
}
