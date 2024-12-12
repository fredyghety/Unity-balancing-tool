using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

public class BalancingToolSettings : EditorWindow
{
    private float _closeButtonWidth=100;
    private float _closeButtonHeight=30;

    private float _textFieldWidth = 200;
    private float _textFieldHeight = 20;

    private BalancingSettings _balancingSettings;
    private static BalancingTool _balancingTool;

    private string _sheetsLink;
    private string _pageName;
    private string _APIKey;
    private string _tag = "#Unity";
    private string _savePath = "Assets";

    public static BalancingToolSettings ShowPopup()
    {
    
        // Create and display the popup window
        var window = CreateInstance<BalancingToolSettings>();
        window.titleContent = new GUIContent("Settings");
        window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 500); 
        window.ShowUtility();

        return window;
    }

    public void Initialize(BalancingTool balancingTool)
    {
        _balancingTool = balancingTool;

        if (_balancingTool._balancingSettings!=null)
        {
            var oldSettings = _balancingTool._balancingSettings;
            _sheetsLink = oldSettings._sheetsLink;
            _pageName = oldSettings._pageName;
            _APIKey = oldSettings._APIKey;
            _tag = oldSettings._tag;
            _savePath = oldSettings._savePath;
        }
    }

    private void OnGUI()
    {
        //GUILayout.Label("This is a popup window.", EditorStyles.wordWrappedLabel);

        GUILayout.Label("Google Sheet Link.", EditorStyles.wordWrappedLabel);
        _sheetsLink = GUILayout.TextField(_sheetsLink, GUILayout.Width(_textFieldWidth), GUILayout.Height(_textFieldHeight));

        GUILayout.Label("Google Sheet Page Name.", EditorStyles.wordWrappedLabel);
        _pageName = GUILayout.TextField(_pageName, GUILayout.Width(_textFieldWidth), GUILayout.Height(_textFieldHeight));

        GUILayout.Label("Google API Key", EditorStyles.wordWrappedLabel);
        _APIKey = GUILayout.TextField(_APIKey, GUILayout.Width(_textFieldWidth), GUILayout.Height(_textFieldHeight));

        GUILayout.Space(10);

        GUILayout.Label("Category Tag", EditorStyles.wordWrappedLabel);
        _tag = GUILayout.TextField(_tag, GUILayout.Width(_textFieldWidth), GUILayout.Height(_textFieldHeight));

        GUILayout.Label("Scriptable Object Save Path", EditorStyles.wordWrappedLabel);
        _savePath = GUILayout.TextField(_savePath, GUILayout.Width(_textFieldWidth), GUILayout.Height(_textFieldHeight));

        GUILayout.Space(25);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Apply Settings", GUILayout.Width(_closeButtonWidth), GUILayout.Height(_closeButtonHeight)))
        {
            ApplySettings();
        }

        if (GUILayout.Button("Close", GUILayout.Width(_closeButtonWidth), GUILayout.Height(_closeButtonHeight)))
        {
            this.Close(); 
        }
        GUILayout.EndHorizontal();
    }

    private void ApplySettings()
    {
        //if (_balancingTool._balancingSettings == null)
        //{
        //    _balancingSettings = new BalancingSettings(_sheetsLink, _pageName, _APIKey, _tag, _savePath);


        //}
        if (_balancingTool == null)
        {
            
            return;
        }
        _balancingSettings = new BalancingSettings(_sheetsLink, _pageName, _APIKey, _tag, _savePath);
        _balancingTool._balancingSettings = _balancingSettings;
        _balancingTool.CheckIfSameTable();
        _balancingTool.SaveProperties();
    }
}
[System.Serializable]
public class BalancingSettings
{
    [SerializeField] public string _sheetsLink;
    [SerializeField] public string _sheetsID;
    [SerializeField] public string _pageName;
    [SerializeField] public string _APIKey;
    [SerializeField] public string _tag ="#Unity";
    [SerializeField] public string _savePath;

    public BalancingSettings(string sheetsLink, string pageName, string APIKey, string tag, string savePath)
    {
        _sheetsLink = sheetsLink;
        _pageName = pageName;
        _APIKey = APIKey;

        _tag = tag;

        _sheetsID = GetSheetID(sheetsLink);

        _savePath = savePath;

    }

    private string GetSheetID(string sheetsLink)
    {
        // Define the regex pattern to capture the string between "/d/" and "/edit?"
        string pattern = @"/d/([a-zA-Z0-9-_]+)/edit\?";

        // Use Regex.Match to find the part of the URL that matches this pattern
        Match match = Regex.Match(_sheetsLink, pattern);

        // If the match is successful, return the captured group (ID)
        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        // Return null or an empty string if the pattern is not found
        return null;
    }

    public string GetCombinedLink()
    {
        var combinedLink = $"https://sheets.googleapis.com/v4/spreadsheets/{_sheetsID}/values/{_pageName}?key={_APIKey}";
        return combinedLink;
    }
}
