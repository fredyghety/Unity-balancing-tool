using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BalancingData))]
public class BalancingDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        var balancingData = (BalancingData)target;

        GUILayout.Space(10); // Add some spacing
        GUILayout.Label("Open Editor", EditorStyles.boldLabel);

        if (GUILayout.Button("Open Editor"))
        {
            BalancingTool.ShowWindowFromScriptableObject(balancingData);
        }
    }
}
