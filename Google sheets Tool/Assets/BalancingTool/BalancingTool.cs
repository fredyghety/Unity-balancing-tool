
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;

public class BalancingTool : EditorWindow
{
    private float _menuButtonWidth = 100;
    private float _menuButtonHeight = 20;

    private float _tabButtonWidth = 150;
    private float _tabButtonHeight = 35;

    private float _graphWidth = 150;
    private float _graphHeight = 150;

    private float _tableFieldWidth = 100;
    private float _tableFieldHeight = 25;

    private float _prestigeTableFieldWidth = 70;
    private float _prestigeTableFieldHeight = 20;

    private float _prestigeRangeMin;
    private float _prestigeRangeMax;

    private Vector2 _scrollPosition;

    [SerializeField] private BalancingTAB currentTab;

    private Action<Table> _dataImportedAction;

    [SerializeField] private Table _table;

    private List<Table> _prestigeTables = new List<Table>();

    private SheetsWriter _sheetsWriter;

    [SerializeField] private Dictionary<string, float> _editableFields = new Dictionary<string, float>();

    [SerializeField] public BalancingSettings _balancingSettings;

    //private SerializedObject _serializedObject;
    //private SerializedProperty _balancingSettingsProperty;
    //private SerializedProperty _editableFieldsProperty;
    //private SerializedProperty _tableProperty;

    [SerializeField] private BalancingDataVariables _balancingDataVariables;

    private BalancingData _currentBalancingDataScriptableObject;

    [SerializeField] private List<GraphVisualiser> _graphVisualisers;

    //[SerializeField] private List<>


    //private string SheetsLink = "https://sheets.googleapis.com/v4/spreadsheets/1A4K6wkRWdpQyrUYolhA57O50h9P2yJM_kB9Gmcyt2KY/values/side1?key=AIzaSyAcTxv2C0fgug_ESiIN9KZ7P3QHkaDFsHA";

    [MenuItem("Window/Balancing Tool")]
    public static void ShowWindow()
    {
        GetWindow<BalancingTool>("Balancing Tool");
    }

    public static void ShowWindowFromScriptableObject(BalancingData balancingData)
    {
        var newWindow = GetWindow<BalancingTool>("Balancing Tool");
        var balancingDataVariables = balancingData.balancingDataVariables;

        balancingDataVariables.table.SerializeTable();

        string path = "Assets/BalancingTool/windowData.json";
        string json = JsonUtility.ToJson(balancingDataVariables, true); // `true` for pretty printing
        File.WriteAllText(path, json);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        //
        newWindow.LoadProperties();
        newWindow._currentBalancingDataScriptableObject = balancingData;
    }

    private void OnEnable()
    {
        
        Debug.Log("BalancingTool enabled. Initializing...");//

        _dataImportedAction += OnImportedData;
        _sheetsWriter = new SheetsWriter();
        _sheetsWriter.Initialize();

        

        LoadProperties();
        
    }

    private void LoadProperties()
    {
        string path = "Assets/BalancingTool/windowData.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);

            // Deserialize the JSON string back into your _balancingData object
            _balancingDataVariables = JsonUtility.FromJson<BalancingDataVariables>(json);
            _balancingDataVariables.table.DeserializeTable();
            _table = _balancingDataVariables.table;
            _balancingSettings = _balancingDataVariables.balancingSettings;

            foreach (var table in _balancingDataVariables.prestigeTables)
            {
                table.DeserializeTable();
            }
            _prestigeTables = _balancingDataVariables.prestigeTables;
            //Debug.Log("Data loaded successfully!");
        }
        else
        {
            Debug.LogError($"File not found at path: {path}");
        }

        
    }
    
    private void OnDisable()
    {
        _dataImportedAction -= OnImportedData;
        
    }

    //window code
    private void OnGUI()
    {
        // buttons at the top
        DrawMenuButtons();

        switch (currentTab)
        {
            case BalancingTAB.Table:
                //input fields in the middle
                DrawTableTab();
                break;
            case BalancingTAB.Graph:
                DrawGraphs();
                break;
            case BalancingTAB.Prestige:
                DrawPrestige();
                break;

        }

        //button tabs at the bottom
        DrawTabButtons();
    }

    private void OnImportedData(Table table)
    {
        _table = table;

        CheckIfSameTable();

        SaveProperties();
    } 

    public void CheckIfSameTable()
    {
        if (_currentBalancingDataScriptableObject == null)
        {
            return;
        }

        var settings = _balancingSettings;

        var cuSettings = _currentBalancingDataScriptableObject.balancingDataVariables.balancingSettings;
        if (settings._sheetsLink == cuSettings._sheetsLink && settings._pageName == cuSettings._pageName)
        {
            
        }
        else
        {
            _currentBalancingDataScriptableObject = null;
        }
    }
    
    public void SaveProperties()
    {
        _balancingDataVariables.table = _table;
        _balancingDataVariables.table.SerializeTable();
        _balancingDataVariables.balancingSettings = _balancingSettings;

        _balancingDataVariables.prestigeTables = _prestigeTables;
        foreach (var table in _balancingDataVariables.prestigeTables)
        {
            table.SerializeTable();
        }

        string path = "Assets/BalancingTool/windowData.json";
        string json = JsonUtility.ToJson(_balancingDataVariables, true); 
        File.WriteAllText(path, json);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();


        UpdateScriptableObject();
    }

    public void CreateScriptableObject()
    {
        var scriptableObjectPath = _balancingSettings._savePath + "/NewBalancingData.asset";

        var scriptableObject = AssetDatabase.LoadAssetAtPath<BalancingData>(scriptableObjectPath);

        if (scriptableObject != null)
        {
            if (_currentBalancingDataScriptableObject!=null) {
                if (scriptableObject.ID == _currentBalancingDataScriptableObject.ID)
                {
                    Debug.Log("scriptable object alredy exists at: " + scriptableObjectPath);
                    return;
                }
                var soSetttings = scriptableObject.balancingDataVariables.balancingSettings;
                var cuSettings = _currentBalancingDataScriptableObject.balancingDataVariables.balancingSettings;
                if (soSetttings._sheetsLink == cuSettings._sheetsLink && soSetttings._pageName == cuSettings._pageName)
                {
                    Debug.Log("scriptable object alredy exists at: " + scriptableObjectPath);
                    return;
                }
            }

            //same name error
            var nameIncrement = 0;

            do
            {
                nameIncrement++;
                scriptableObjectPath = _balancingSettings._savePath + $"/NewBalancingData{nameIncrement}.asset";
                scriptableObject = AssetDatabase.LoadAssetAtPath<BalancingData>(scriptableObjectPath);
            } while (scriptableObject != null);
            
        }

        try
        {
            _currentBalancingDataScriptableObject = ScriptableObject.CreateInstance<BalancingData>();
            _currentBalancingDataScriptableObject.balancingDataVariables = _balancingDataVariables;
            _currentBalancingDataScriptableObject.Initialize();

            AssetDatabase.CreateAsset(_currentBalancingDataScriptableObject, scriptableObjectPath);
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = _currentBalancingDataScriptableObject;
        }
        catch
        {
            Debug.Log("couldnt create scriptable object at path: "+ scriptableObjectPath);
        }

        
    }

    public void UpdateScriptableObject()
    {
        if (_currentBalancingDataScriptableObject == null)
        {
            return;
        }
        _currentBalancingDataScriptableObject.balancingDataVariables = _balancingDataVariables;
    }

    private void DrawMenuButtons()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("import", GUILayout.Width(_menuButtonWidth), GUILayout.Height(_menuButtonHeight)))
        {
            //run import data behaviour
            SheetsReader.GetSheetData(_balancingSettings.GetCombinedLink(), _dataImportedAction);
            
        }

        if (GUILayout.Button("export", GUILayout.Width(_menuButtonWidth), GUILayout.Height(_menuButtonHeight)))
        {
            //run export data behaviour
            ConvertFieldsToTable();
            _sheetsWriter.UpdateSheetTable(_balancingSettings, _table);
        }

        if (GUILayout.Button("settings", GUILayout.Width(_menuButtonWidth), GUILayout.Height(_menuButtonHeight)))
        {
            //run settings behaviour
            var newSettingsWindow = BalancingToolSettings.ShowPopup();
            newSettingsWindow.Initialize(this);
        }
        if (GUILayout.Button("create scriptable object", GUILayout.Width(_menuButtonWidth*1.5f), GUILayout.Height(_menuButtonHeight)))
        {
            //run scriptable object behaviour
            CreateScriptableObject();
        }
        if (GUILayout.Button("Save Graphs To Table", GUILayout.Width(_menuButtonWidth * 1.5f), GUILayout.Height(_menuButtonHeight)))
        {
            //save graphs data to table 
            SaveGraphDataToTable();
        }
        GUILayout.EndHorizontal();

        
    }

    private void ConvertFieldsToTable()
    {
        if (_editableFields == null)
        {
            return;
        }

        foreach (var row in _table.rows)
        {
            
            foreach (var key in row.rowDictionary.Keys)
            {
                var fieldKey = $"{row.GetHashCode()}_{key}";
                //var value = row.rowDictionary[key].value;
                //editableFields[fieldKey] = value;
                row.rowDictionary[key].value = _editableFields[fieldKey];
            }
        }

        
    }

    private void DrawTableTab()
    {
        
        if (_table == null)
        {
            //Debug.Log("no table to draw");
            return;
        }
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();

        //writes category names
        foreach (var row in _table.rows)
        {
            GUILayout.BeginHorizontal();

            foreach (var key in row.rowDictionary.Keys)
            {
                
                GUILayout.TextField(key.ToString(), GUILayout.Width(_tableFieldWidth), GUILayout.Height(_tableFieldHeight));
            }

            GUILayout.EndHorizontal();
            break;
        }
    
        //writes row variables
        foreach (var row in _table.rows)
        {
            GUILayout.BeginHorizontal();

            foreach (var key in row.rowDictionary.Keys)
            {
                // Create a key for the editable field dictionary
                string fieldKey = $"{row.GetHashCode()}_{key}";

                // Check if the field exists in the dictionary; if not, initialize it
                if (!_editableFields.ContainsKey(fieldKey))
                {
                    var value = row.rowDictionary[key];
                    _editableFields[fieldKey] = row.rowDictionary[key].value;
                }

                var newValue = GUILayout.TextField(_editableFields[fieldKey].ToString(), GUILayout.Width(_tableFieldWidth), GUILayout.Height(_tableFieldHeight));
                //var value = row.rowDictionary[key];
                //GUILayout.TextField(value.value.ToString()
                //, GUILayout.Width(100), GUILayout.Height(25));
                if (float.TryParse(newValue, out float parsedValue))
                {
                    _editableFields[fieldKey] = parsedValue; // Update the float value
                }
            }

            GUILayout.EndHorizontal(); 
        }

        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
    }

    private void DrawGraphs()
    {
        if (_graphVisualisers == null)
        {
            SetupGraphs();
        }
        //calculates graph width
        var windowRect = position;
        _graphWidth = windowRect.width /_graphVisualisers.Count;
        //draws graphs
        var combinedWidth = 10f;
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        foreach (var graph in _graphVisualisers)
        {
            var curveRect = GUILayoutUtility.GetRect(_graphWidth, _graphHeight, GUILayout.ExpandWidth(false));
            graph.animationCurve = EditorGUI.CurveField(curveRect, graph.animationCurve);

            GUI.Label(new Rect(combinedWidth, -20, _graphWidth, _graphHeight), graph.categoryName, GUI.skin.label);
            combinedWidth = combinedWidth += _graphWidth;
        }

        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
    }

    private void SaveGraphDataToTable()
    {
        foreach (var graph in _graphVisualisers)
        {
            var coloumn = _table.GetColoumnByCategory(graph.categoryName);
            for (int i = 0; i < coloumn.Count; i++)
            {
                coloumn[i].value = graph.animationCurve.keys[i].value;
            }
        }
        SaveProperties();
        SetupGraphs();
    }

    private void DrawTabButtons()
    {
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
         // Pushes everything above this point up
        if (GUILayout.Button("Table", GUILayout.Width(_tabButtonWidth), GUILayout.Height(_tabButtonHeight)))
        {
            //run table behaviour
            _editableFields.Clear();
            currentTab = BalancingTAB.Table;
        }
        if (GUILayout.Button("Graph", GUILayout.Width(_tabButtonWidth), GUILayout.Height(_tabButtonHeight)))
        {
            //run graph behaviour
            SetupGraphs();
            currentTab = BalancingTAB.Graph;
        }
        if (GUILayout.Button("Prestige", GUILayout.Width(_tabButtonWidth), GUILayout.Height(_tabButtonHeight)))
        {
            //run graph behaviour
            SetUpPresige();
            currentTab = BalancingTAB.Prestige;
        }
        GUILayout.EndHorizontal();
    }

    private void SetupGraphs()
    {
        if (_table ==null)
        {
            return;
        }
        if (_table.rows.Count == 0)
        {
            return;
        }
        var coloumnNames = _table.rows[0].rowDictionary.Keys;

        _graphVisualisers = new List<GraphVisualiser>();

        foreach (var cloumnName in coloumnNames)
        {
            var coloumn = _table.GetColoumnByCategory(cloumnName);
            var newGraphVisuals = new GraphVisualiser();
            newGraphVisuals.SetGraphValues(coloumn, cloumnName);
            _graphVisualisers.Add(newGraphVisuals);
        }
    }

    private void SetUpPresige()
    {

    }

    private void DrawPrestige()
    {
        if (_table == null)
        {
            //Debug.Log("no table to draw");
            return;
        }
        GUILayout.FlexibleSpace();

        EditorGUILayout.LabelField("Table", EditorStyles.boldLabel);


        DisplayTable(_table);

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Prestige Table", GUILayout.Width(_menuButtonWidth*1.5f), GUILayout.Height(_menuButtonHeight*1.1f)))
        {
            //run table behaviour for each prestige table
            AddNewPrestigeTable(_table, _prestigeRangeMin, _prestigeRangeMax);
            EditorUtility.SetDirty(this);
            Repaint();
        }
        if (GUILayout.Button("Clear Prestige Tables", GUILayout.Width(_menuButtonWidth * 1.5f), GUILayout.Height(_menuButtonHeight * 1.1f)))
        {
            //run table behaviour for each prestige table
            _prestigeTables.Clear();

        }
        if (GUILayout.Button("Save Prestige Tables", GUILayout.Width(_menuButtonWidth * 1.5f), GUILayout.Height(_menuButtonHeight * 1.1f)))
        {
            //run table behaviour for each prestige table
            SaveProperties();

        }
        if (GUILayout.Button("Append Tables To Sheets", GUILayout.Width(_menuButtonWidth * 1.5f), GUILayout.Height(_menuButtonHeight * 1.1f)))
        {
            //run table behaviour for each prestige table
            foreach (var table in _prestigeTables)
            {
                _sheetsWriter.AppendTable(_balancingSettings,table);
            }
            

        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("new minimum", EditorStyles.label, GUILayout.Width(80));
        var newMin = GUILayout.TextField(_prestigeRangeMin.ToString(), GUILayout.Width(_prestigeTableFieldWidth), GUILayout.Height(_prestigeTableFieldHeight));
        if (float.TryParse(newMin, out float parsedValueMin))
        {
            _prestigeRangeMin = parsedValueMin; // Update the float value
        }

        EditorGUILayout.LabelField("new maximum", EditorStyles.label, GUILayout.Width(90));
        var newMax = GUILayout.TextField(_prestigeRangeMax.ToString(), GUILayout.Width(_prestigeTableFieldWidth), GUILayout.Height(_prestigeTableFieldHeight));
        if (float.TryParse(newMax, out float parsedValueMax))
        {
            _prestigeRangeMax = parsedValueMax; // Update the float value
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(25);
        EditorGUILayout.LabelField("Prestige Tables", EditorStyles.boldLabel);

        //draw scrollview
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height - 300));

        

        for (int i = 0; i < _prestigeTables.Count; i++)
        {
            GUILayout.Space(15);
            EditorGUILayout.LabelField($"Prestige Table {i+1}", EditorStyles.boldLabel);
            DisplayTable(_prestigeTables[i]);//
        }
        
        //for (int i = 0; i < 50; i++)
        //{
        //    GUILayout.Box($"Item {i + 1}", GUILayout.Height(50));
        //}

        GUILayout.EndScrollView();

        GUILayout.FlexibleSpace();
    }

    private void AddNewPrestigeTable(Table fromTable, float newStart, float newEnd)
    {
        //_prestigeTables.Clear();

        var tableRange = fromTable.GetRangeOfTable();

        var newTable = new Table();
        foreach (var row in fromTable.rows)
        {
            var newRow = new Row();
            foreach (var cellKey in row.rowDictionary.Keys)
            {
                var newCell = new Cell();
                var cell = row.rowDictionary[cellKey];
                var newValue = MapValueToRange(cell.value,tableRange.x,tableRange.y,newStart, newEnd);
                newCell.value = newValue;
                newRow.rowDictionary.Add(cellKey, newCell);
            }
            newTable.rows.Add(newRow);
        }
        _prestigeTables.Add(newTable);
    }
    
    //chatgpt kode for at mappe en værdi fra en range til en anden
    private float MapValueToRange(float value, float from1, float to1, float from2, float to2)
    {
        //checks for dividing by 0 error
        if (Mathf.Abs(to1 - from1) < Mathf.Epsilon)
        {
            throw new System.ArgumentException("Input range must not be zero.");
        }

        // Map value from input range to output range
        return from2 + (value - from1) * (to2 - from2) / (to1 - from1);
    }

    private void DisplayTable(Table table)
    {
        GUILayout.BeginVertical();

        //writes category names
        foreach (var row in table.rows)
        {
            GUILayout.BeginHorizontal();

            foreach (var key in row.rowDictionary.Keys)
            {

                GUILayout.TextField(key.ToString(), GUILayout.Width(_prestigeTableFieldWidth), GUILayout.Height(_prestigeTableFieldHeight));
            }

            GUILayout.EndHorizontal();
            break;
        }

        //writes row variables
        foreach (var row in table.rows)
        {
            GUILayout.BeginHorizontal();

            foreach (var key in row.rowDictionary.Keys)
            {
                // Create a key for the editable field dictionary
                string fieldKey = $"{row.GetHashCode()}_{key}";

                // Check if the field exists in the dictionary; if not, initialize it
                if (!_editableFields.ContainsKey(fieldKey))
                {
                    var value = row.rowDictionary[key];
                    _editableFields[fieldKey] = row.rowDictionary[key].value;
                }

                //var newValue = GUILayout.TextField(_editableFields[fieldKey].ToString(), GUILayout.Width(_prestigeTableFieldWidth), GUILayout.Height(_prestigeTableFieldHeight));
                GUILayout.TextField(_editableFields[fieldKey].ToString(), GUILayout.Width(_prestigeTableFieldWidth), GUILayout.Height(_prestigeTableFieldHeight));
                //var value = row.rowDictionary[key];
                //GUILayout.TextField(value.value.ToString()
                //, GUILayout.Width(100), GUILayout.Height(25));
                //if (float.TryParse(newValue, out float parsedValue))
                //{
                //    _editableFields[fieldKey] = parsedValue; // Update the float value
                //}
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
    }
}



public enum BalancingTAB
{
    Table,
    Graph,
    Prestige
}