using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Table
{
    [SerializeField] public List<Row> rows = new List<Row>();

    public void SerializeTable()
    {
        foreach (var row in rows)
        {
            row.SerializeRow();
        }
    }

    public void DeserializeTable()
    {
        foreach (var row in rows)
        {
            row.DeserializeRow();
        }
    }

    public List<Cell> GetColoumnByCategory(string categoryName)
    {
        var cellList = new List<Cell>();
        foreach (var row in rows)
        {
            var cell = row.rowDictionary[categoryName];
            cellList.Add(cell);
        }
        return cellList;
    }

    public Vector2 GetRangeOfTable()
    {
        if (rows.Count == 0)
        {
            Debug.LogError("Cant get range of table when row count is 0");
        }

        var minValue = Mathf.Infinity;
        var maxValue = -Mathf.Infinity;

        foreach (var row in rows)
        {
            foreach (var key in row.rowDictionary.Keys)
            {
                var cell = row.rowDictionary[key];
                if (cell.value>maxValue)
                {
                    maxValue = cell.value;
                }
                if (cell.value < minValue)
                {
                    minValue = cell.value;
                }
            }
        }

        return new Vector2(minValue, maxValue);

    }
}
