using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Row 
{
    [SerializeField] public int y;
    public Dictionary<string, Cell> rowDictionary = new Dictionary<string, Cell>();

    [SerializeField] public List<RowPair> serializedRowDictionary = new List<RowPair>();

    public void SerializeRow()
    {
        serializedRowDictionary.Clear();
        foreach (var key in rowDictionary.Keys)
        {
            var rowpair = new RowPair(key, rowDictionary[key]);
            serializedRowDictionary.Add(rowpair);
        }
    }

    public void DeserializeRow()
    {
        foreach (var pair in serializedRowDictionary)
        {
            rowDictionary.Add(pair.key, pair.value);//
        }
    }
}

[System.Serializable]
public class RowPair{
    [SerializeField] public string key;
    [SerializeField] public Cell value;
    

    public RowPair(string key, Cell value)
    {
        this.key = key;
        this.value = value;
    }
}
