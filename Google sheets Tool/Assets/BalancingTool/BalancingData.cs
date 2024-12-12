using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
[CreateAssetMenu(fileName = "BalancingData", menuName = "Balancing/BalancingData")]
public class BalancingData : ScriptableObject
{
    public Guid ID { get; private set; }
    public BalancingDataVariables balancingDataVariables;

    public void Initialize()
    {
        ID = new Guid();
    }
}
[System.Serializable]
public class BalancingDataVariables
{
    [SerializeField] public Table table;
    
    [SerializeField] public BalancingSettings balancingSettings;

    [SerializeField] public List<Table> prestigeTables;
}
