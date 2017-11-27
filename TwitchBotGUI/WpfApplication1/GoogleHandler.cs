using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Linq;

public class GoogleHandler
{
    public static string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
    public static string ApplicationName = "Google Sheets API .NET Quickstart";
    public String spreadsheetId;
    public SheetsService service;
    

    public GoogleHandler()
    {
        UserCredential credential;
        using (var stream =
               new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
        {
            string credPath = System.Environment.GetFolderPath(
                                System.Environment.SpecialFolder.Personal);
            credPath = Path.Combine(credPath, ".credentials/sheets.googleapis.com-dotnet-quickstart.json");
            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true)).Result;
        }
        service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });
    }
    
    public void setValue(String RowStart, String file)
    {
        String range = RowStart;
        ValueRange oRange = new ValueRange();
        oRange.Range = range;
        oRange.Values = getDataClear();
        List<ValueRange> oList = new List<ValueRange>();
        oList.Add(oRange);
        BatchUpdateValuesRequest oRequest = new BatchUpdateValuesRequest();
        oRequest.ValueInputOption ="RAW";
        oRequest.Data = oList;
        service.Spreadsheets.Values.BatchUpdate(oRequest, spreadsheetId).Execute();
        IList<IList<Object>> arrData2 = getData(file);
        ValueRange oRange2 = new ValueRange();
        oRange2.Range = range;
        oRange2.Values = arrData2;
        List<ValueRange> oList2 = new List<ValueRange>();
        oList2.Add(oRange2);
        BatchUpdateValuesRequest oRequest2 = new BatchUpdateValuesRequest();
        oRequest2.ValueInputOption = "RAW";
        oRequest2.Data = oList2;
        service.Spreadsheets.Values.BatchUpdate(oRequest2, spreadsheetId).Execute();
    }

    public IList<IList<Object>> getDataClear()
    {
        IList<Object> data1 = new List<Object>();
        data1.Add("");
        IList<IList<Object>> data = new List<IList<Object>>();
        for (int i = 0; i < 100; i++)
        {
            data.Add(data1);
        }
        return data;
    }

    public IList<IList<Object>> getData(String file)
    {
        IList<IList<Object>> data = new List<IList<Object>>();
        try
        {
            StreamReader br = new StreamReader(file);
            String line;
            int count = 0;
            data.Add(new List<Object>());
            if (file.Equals("song.txt"))
            {
                if (count == 0)
                {
                    data[0].Add("CURRENT QUEUE:");
                    count++;
                }
                while ((line = br.ReadLine()) != null)
                {
                    if (line.Equals("Song list is empty"))
                    {
                        br.Close();
                        return null;
                    }
                    data.Add(new List<Object>());
                    data[count].Add(line);
                    count++;
                }
            }
            else
            {
                while ((line = br.ReadLine()) != null)
                {
                    data.Add(new List<Object>());
                    data[count].Add(line);
                    count++;
                }
                data.Reverse();
                data[0].Add("LAST PLAYED SONGS:");
            }
            br.Close();
            return data;
        }
        catch (Exception e)
        {
            Utils.errorReport(e);
            Debug.WriteLine(e.ToString());
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
            Debug.WriteLine(e.ToString());
        }
    }
}
