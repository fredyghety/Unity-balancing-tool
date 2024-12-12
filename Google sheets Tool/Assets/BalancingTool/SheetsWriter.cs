using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Sheets.v4;
using System.IO;
using System;
using System.Net.Security;



public class SheetsWriter
{
    private SheetsService sheetsService;
    private  string spreadsheetId = "";

    // The range where you want to update data (e.g., "Sheet1!A1:C3")
    private string range = ""; // Modify as needed

    // Path to your service account key file
    private string jsonPath = ""; // Modify this with your actual path
    //private static string jsonPath = "Assets/BalancingTool/GoogleAPIS/unity-sheets-test-440013-c1b3cb3f81c5.json"; // Modify this with your actual path

    public void Initialize()
    {
        jsonPath = GetJsonFilePath();
        AuthenticateAndInitialize();
    }

    private string GetJsonFilePath()
    {
        string scriptFolderPath = Path.Combine(Application.dataPath, "BalancingTool/GoogleAPIS");
        string jsonFilePath = Path.Combine(scriptFolderPath, "unity-sheets-test-440013-c1b3cb3f81c5.json");

        //Debug.Log("JSON file path: " + jsonFilePath);
        return jsonFilePath;
    }

   public void UpdateSheetTable(BalancingSettings balancingSettings, Table table)
   {
        //var EmptyObj = new GameObject("SendGoogleSheets");
        //var sendSheets = EmptyObj.AddComponent<SendSheets>();
        spreadsheetId = balancingSettings._sheetsID;

        if (!File.Exists(jsonPath))
        {
            AuthenticateAndInitialize();
        }

        //sendSheets.StartCoroutine(sendSheets.PostTable(link, form));
        //AuthenticateAndInitialize();

        UpdateSheetData(table, balancingSettings);
    }

    // Authenticate and initialize the Google Sheets API client
    private void AuthenticateAndInitialize()
    {
        
            //chatgpt code for authenticating with the google sheets api
            GoogleCredential credential;

            using (var stream = new FileStream(jsonPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(SheetsService.Scope.Spreadsheets);
            }

            sheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Google Sheets API Example"
            });

            //Debug.Log("Google Sheets API initialized.");
        
        
    }

    // Update data in Google Sheets
    public async void UpdateSheetData(Table table, BalancingSettings settings)
    {
        var startColumn = 0;
        var endColumn = 0;
 
        var foundFirst = false;

        var values = new List<IList<object>>();

        foreach (var row in table.rows)
        {
            List<object> newRow = new List<object>();
            foreach (var key in row.rowDictionary.Keys)
            {
                var value = row.rowDictionary[key];
                newRow.Add(value.value.ToString());

                if (!foundFirst)
                {
                    startColumn = (int)value.x;
                    foundFirst = true;
                }
                
                endColumn = (int)value.x;
            }
            values.Add(newRow);
        }
        int startRow = table.rows[0].y;
        int endRow = table.rows[table.rows.Count - 1].y;

        range = ConvertToGoogleSheetsRange(startColumn,startRow,endColumn,endRow);
        
        var sheetName = settings._pageName;
        range = $"{sheetName}!{range}";
        Debug.Log(range);


        ValueRange body = new ValueRange()
        {
            Values = values
        };

        // Call the Sheets API to update the range
        
        var request = sheetsService.Spreadsheets.Values.Update(body, spreadsheetId, range);
        request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;

        try
        {
            var response = await request.ExecuteAsync();
            Debug.Log("Data updated successfully!");
        }
        catch (Exception e)
        {
            Debug.LogError("Error updating data: " + e.Message);
        }
    }

    private string ConvertToGoogleSheetsRange(int startColumn, int startRow, int endColumn, int endRow)
    {
        // Convert numeric columns to letter-based format
        string startColumnLetter = GetColumnLetter(startColumn);
        string endColumnLetter = GetColumnLetter(endColumn);

        // Construct the range in Google Sheets format
        return $"{startColumnLetter}{startRow}:{endColumnLetter}{endRow}";
    }

    private string GetColumnLetter(int columnNumber)
    {
        string columnLetter = "";
        while (columnNumber > 0)
        {
            int remainder = (columnNumber - 1) % 26;
            columnLetter = (char)(remainder + 'A') + columnLetter;
            columnNumber = (columnNumber - 1) / 26;
        }
        return columnLetter;
    }

    public void AppendTable(BalancingSettings balancingSettings, Table table)
    {
        //var EmptyObj = new GameObject("SendGoogleSheets");
        //var sendSheets = EmptyObj.AddComponent<SendSheets>();
        spreadsheetId = balancingSettings._sheetsID;

        if (!File.Exists(jsonPath))
        {
            AuthenticateAndInitialize();
        }

        //sendSheets.StartCoroutine(sendSheets.PostTable(link, form));
        //AuthenticateAndInitialize();

        AppendSheetData(table, balancingSettings);
    }

    // Append new data to Google Sheets
    public async void AppendSheetData(Table table, BalancingSettings balancingSettings)
    {
        var foundFirst = false;

        var values = new List<IList<object>>();

        List<object> keyRow = new List<object>();
        foreach (var key in table.rows[0].rowDictionary.Keys)
        {
            keyRow.Add(key);
        }
        values.Add(keyRow);

        foreach (var row in table.rows)
        {
            List<object> newRow = new List<object>();
            foreach (var key in row.rowDictionary.Keys)
            {
                var value = row.rowDictionary[key];
                newRow.Add(value.value.ToString());
            }
            values.Add(newRow);
        }

        ValueRange body = new ValueRange()
        {
            Values = values
        };

        // Call the Sheets API to append the data
        var request = sheetsService.Spreadsheets.Values.Append(body, spreadsheetId, balancingSettings._pageName);
        request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;

        try
        {
            var response = await request.ExecuteAsync();
            Debug.Log("Data appended successfully!");
        }
        catch (Exception e)
        {
            Debug.LogError("Error appending data: " + e.Message);
        }
    }
}
