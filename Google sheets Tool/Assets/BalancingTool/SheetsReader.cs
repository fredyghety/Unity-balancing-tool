using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using SimpleJSON;

public class SheetsReader
{
    public static void GetSheetData(string sheetsLink, Action<Table> onComplete)
    {
        StartLoadingData(sheetsLink, onComplete);
    }

    private static void StartLoadingData(string sheetsLink, Action<Table> onComplete)
    {
        
        var EmptyObj = new GameObject("GetGoogleSheets");
        var getSheets = EmptyObj.AddComponent<GetSheets>();
        
        var connection = getSheets.StartCoroutine(getSheets.ObtainSheetConnection(sheetsLink, request =>
        {
            if (request != null)
            {
                //Debug.Log("Success: " + request.downloadHandler.text);
                string jsonString = request.downloadHandler.text;
                var json = JSON.Parse(jsonString);
                var newTable = LoadDataFromJson(json);
                onComplete?.Invoke(newTable); // Pass the table to the callback
            }
            else
            {
                Debug.LogError("Failed to obtain sheet data.");
                onComplete?.Invoke(null); // Pass null to indicate failure
            }

            // Clean up the temporary GameObject
            UnityEngine.Object.DestroyImmediate(EmptyObj);
        }));
    }

    //private static void ValidateRequest(UnityWebRequest request)
    //{
    //    if (request != null)
    //    {
    //        Debug.Log("Success: " + request.downloadHandler.text);
    //        string jsonString = request.downloadHandler.text;
    //        var json = JSON.Parse(jsonString);
    //        LoadDataFromJson(json);
    //    }
    //    else
    //    {
    //        Debug.LogError("Failed to obtain sheet data.");
            
    //    }
    //}

    private static Table LoadDataFromJson(JSONNode json)
    {
        var newTable = new Table();
        


        var categoryNames = new List<string>();
        var foundCategories = false;

        var values = json["values"];
        var y = 0;
        //iterates through all arrays called values
        foreach (var valueArray in values)
        {
            var newValueArray = JSON.Parse(valueArray.ToString());
            var keyValuePairList = new List<KeyValuePair<string, JSONNode>>();

            //iterates through rows
            foreach (var row in newValueArray)
            {
                y++;
                //if (timesSkippedRow < fromCell.y)
                //{
                //    timesSkippedRow++;
                //    continue;
                //}
              

                var stringList = new List<string>();
                var numberList = new List<float>();
                
                
                var newRow = new Row();
                var isCategoryRow = false;

                var x = 0;

                //iterates every value in the row
                foreach (var cellValue in row.Value)
                {
                    //Debug.Log(cellValue.ToString());
                    var value = cellValue.ToString();
                    if (!foundCategories)
                    {
                        var foundCategory = FindCategory(value);
                        if (foundCategory)
                        {
                            var newCategoryStringName = value.ToString();
                            newCategoryStringName = newCategoryStringName.Trim('[', ']',',');
                            isCategoryRow = true;
                            categoryNames.Add(newCategoryStringName);
                        }
                    }
                    else
                    {
                        if (x>=categoryNames.Count)
                        {
                            break;
                        }

                        var floatValueAsString = cellValue.Value.Value.ToString();
                        var isFloat = float.TryParse(floatValueAsString, out float result);

                        if (isFloat)
                        {
                            
                            var newCell = new Cell();
                            newCell.value = result;
                            newCell.x = x+1;
                            newRow.rowDictionary.Add( categoryNames[x],newCell);
                        }
                        x++;
                        
                    }
                    
                }



                if (isCategoryRow)
                {
                    foundCategories = true;
                }
                else if(foundCategories)
                {
                    newTable.rows.Add(newRow);
                    newRow.y = y;
                    
                }

            }


        }

        return newTable;
        //Debug.Log(newTable.rows.Count);
        //foreach (var row in newTable.rows)
        //{
        //    Debug.Log(row.rowDictionary.Count);
        //    foreach (var key in row.rowDictionary.Keys)
        //    {
        //        Debug.Log(row.rowDictionary[key].value);
        //    }
        //}
    }

    private static bool FindCategory(string cellValue)
    {
        if (cellValue.ToLower().Contains("#unity"))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
   
}
