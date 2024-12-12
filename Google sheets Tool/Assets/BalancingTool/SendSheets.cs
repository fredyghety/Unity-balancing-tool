using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System.IO;
using System;

public class SendSheets : MonoBehaviour
{
    private SheetsService sheetsService;
    private string spreadsheetId = "1A4K6wkRWdpQyrUYolhA57O50h9P2yJM_kB9Gmcyt2KY"; // Replace with your spreadsheet ID

    // The range where you want to update data (e.g., "Sheet1!A1:C3")
    private string range = "Sheet1!A1:C3"; // Modify as needed

    // Path to your service account key file
    private string jsonPath = "Assets/BalancingTool/GoogleAPIS/unity-sheets-test-440013-c1b3cb3f81c5.json"; // Modify this with your actual path

 

    
}
