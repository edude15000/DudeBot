using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Linq;
using System.Collections.ObjectModel;

public class GoogleHandler
{
    static string[] Scopes = { SheetsService.Scope.Spreadsheets };
    static string ApplicationName = "Google Sheets API .NET Quickstart";
    public String spreadsheetId;
    SheetsService service;
    
    public GoogleHandler(String id)
    {
        spreadsheetId = id;
        UserCredential credential;
        using (var stream =
                new FileStream(@"bin\client_secret.json", FileMode.Open, FileAccess.Read))
        {
            string credPath = Environment.GetFolderPath(
                Environment.SpecialFolder.Personal);
            credPath = Path.Combine(credPath, ".credentials/sheets.googleapis.com-dotnet-quickstart.json");
            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None
                ).Result;
        }

        service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });
    }
    
    public void setValue(String RowStart, ObservableCollection<Song> songlist, String file, List<String> songHistory)
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
        IList<IList<Object>> arrData2 = getData(songlist, file, songHistory);
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

    public IList<IList<Object>> getData(ObservableCollection<Song> songlist, String file, List<String> songHistory)
    {
        IList<IList<Object>> data = new List<IList<Object>>();
        try
        {
            data.Add(new List<Object>());
            if (file != null && file.Equals(@"bin\songList.json"))
            {
                data[0].Add("CURRENT QUEUE:");
                for (int i = 1; i <= songlist.Count; i++)
                {
                    if (songlist.Count == 0)
                    {
                        break;
                    }
                    data.Add(new List<Object>());
                    if (songlist[i - 1].level.Equals(""))
                    {
                        data[i].Add(songlist[i - 1].name + " (" + songlist[i - 1].requester + ")");
                    }
                    else
                    {
                        data[i].Add(songlist[i - 1].level + " " + songlist[i - 1].name + " (" + songlist[i - 1].requester + ")");
                    }
                }
            }
            else
            {
                for (int i = 1; i <= songHistory.Count; i++)
                {
                    data.Add(new List<Object>());
                    data[i].Add(songHistory[i-1]);
                }
                data.Reverse();
                data[0].Add("LAST PLAYED SONGS:");

            }
            return data;
        }
        catch (Exception e)
        {
            Utils.errorReport(e);
            Debug.WriteLine(e.ToString());
        }
        return null;
    }

    public List<String> readColumnA(String setlistSheetID)
    {
        List<String> list = new List<String>();
        try
        {
            ValueRange result = service.Spreadsheets.Values.Get(spreadsheetId, "A1:A10000").Execute();
            foreach (IList<Object> str in result.Values)
            {
                if (str[0] != null)
                {
                    String s = str[0].ToString();
                    if (s != null && s != "")
                    {
                        if (s.Length >= 8)
                        {
                            list.Add(s);
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
        }
        return list;
    }

    public List<String> readColumnAandB(String setlistSheetID)
    {
        List<String> list = new List<String>();
        try
        {
            ValueRange result = service.Spreadsheets.Values.Get(spreadsheetId, "A1:B10000").Execute();
            foreach (IList<Object> str in result.Values)
            {
                if (str[0] != null && str[1] != null) {
                    String s = str[0].ToString();
                    String s2 = str[1].ToString();
                    if (s != null && s != "" && s2 != null && s2 != "")
                    {
                        if ((s + " " + s2).Length >= 8)
                        {
                            list.Add(s + " " + s2);
                            list.Add(s2 + " " + s);
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
        }
        return list;
    }

    public void writeToGoogleSheets(Boolean updateAllSongs, ObservableCollection<Song> songs, List<String> songHistory)
    {
        try
        {
            setValue("A1", songs, Utils.songListFile, songHistory);
            if (updateAllSongs)
            {
                setValue("B1", songs, null, songHistory);
            }
        }
        catch (Exception e)
        {
            if (!e.ToString().Contains("The caller does not have permission[403]")) {
                Utils.errorReport(e);
                Debug.WriteLine(e.ToString());
            }
        }
    }
}
