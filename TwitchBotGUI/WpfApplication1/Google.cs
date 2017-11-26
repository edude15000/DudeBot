using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.IO;

public class Google
{
    private sealed String APPLICATION_NAME = "Google Sheets API Java Quickstart";
    private sealed static java.io.File DATA_STORE_DIR = new java.io.File(System.getProperty("user.home"),
            ".credentials/sheets.googleapis.com-java-quickstart");
    static FileDataStoreFactory DATA_STORE_FACTORY;
    private final JsonFactory JSON_FACTORY = JacksonFactory.getDefaultInstance();
	static HttpTransport HTTP_TRANSPORT;
    sealed List<String> SCOPES = Arrays.asList(SheetsScopes.SPREADSHEETS);
    public Sheet service;
    public String spreadsheetId;
    static {
		try 
        {
			HTTP_TRANSPORT = GoogleNetHttpTransport.newTrustedTransport();
			DATA_STORE_FACTORY = new FileDataStoreFactory(DATA_STORE_DIR);
} 
        catch (Exception t) 
        {
			t.ToString();
			System.exit(1);
		}
	}

	public Credential authorize() 
    {
        File i;
		i = new File("client_secret.json");
        GoogleClientSecrets clientSecrets = GoogleClientSecrets.load(JSON_FACTORY, new FileReader(in));
        GoogleAuthorizationCodeFlow flow = new GoogleAuthorizationCodeFlow.Builder(HTTP_TRANSPORT, JSON_FACTORY,
        clientSecrets, SCOPES).setDataStoreFactory(DATA_STORE_FACTORY).setAccessType("offline").build();
        Credential credential = new AuthorizationCodeInstalledApp(flow, new LocalServerReceiver()).authorize("user");
		return credential;
	}

	public Sheets getSheetsService()
{
    Credential credential = authorize();
		return new Sheets.Builder(HTTP_TRANSPORT, JSON_FACTORY, credential).setApplicationName(APPLICATION_NAME)
				.build();
	}

	public void setValue(String RowStart, String file)
{
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

	public List<List<Object>> getDataClear() {
		List<Object> data1 = new List<Object>();
        data1.Add("");
		List<List<Object>> data = new List<List<Object>>();
		for (int i = 0; i< 100; i++) {
			data.Add(data1);
		}
		return data;
	}

	public List<List<Object>> getData(String file)
{
    List<List<Object>> data = new List<List<Object>>();
    try
    {
        BufferedReader br = new BufferedReader(new FileReader(file));
        String line;
        int count = 0;
        data.add(new ArrayList<Object>());
        if (file.equals("song.txt"))
        {
            if (count == 0)
            {
                data.get(0).add("CURRENT QUEUE:");
                count++;
            }
            while ((line = br.readLine()) != null)
            {
                if (line.equals("Song list is empty"))
                {
                    br.close();
                    return null;
                }
                data.add(new ArrayList<Object>());
                data.get(count).add(line);
                count++;
            }
        }
        else
        {
            while ((line = br.readLine()) != null)
            {
                data.add(new ArrayList<Object>());
                data.get(count).add(line);
                count++;
            }
            Collections.reverse(data);
            data.get(0).add("LAST PLAYED SONGS:");
        }
        br.close();
        return data;
    }
    catch (Exception e) {
        Utils.errorReport(e);
        e.ToString();
    }
    return null;
    }

    public void writeToGoogleSheets(Boolean updateAllSongs, String songlistfile, String lastPlayedSongsFile)
    {
        try
        {
            setValue("A1", songlistfile);
            if (updateAllSongs)
            {
                setValue("B1", lastPlayedSongsFile);
            }
        }
        catch (Exception e)
        {
            Utils.errorReport(e);
            e.ToString();
        }
    }
}
