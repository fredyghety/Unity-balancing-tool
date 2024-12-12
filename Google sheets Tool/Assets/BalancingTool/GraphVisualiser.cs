using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class GraphVisualiser 
{
    public AnimationCurve animationCurve;
    public List<Cell> values = new List<Cell>();
    public string categoryName;

    public GraphVisualiser()
    {
        animationCurve = new AnimationCurve(
            new Keyframe(0, 0),
            new Keyframe(1, 1),
            new Keyframe(2, 0)
        );
    }

    public void SetGraphValues(List<Cell> newValues, string newCategoryName)
    {
        values = newValues;
        categoryName = newCategoryName;

        var graphLength = newValues.Count;
        animationCurve.ClearKeys();
        animationCurve.postWrapMode = WrapMode.ClampForever;

        for (int i = 0; i < graphLength; i++)
        {
            animationCurve.AddKey(i,newValues[i].value);
        }

        for (int i = 0; i < animationCurve.keys.Length - 1; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(animationCurve, i, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyRightTangentMode(animationCurve, i, AnimationUtility.TangentMode.Linear);
        }
    }
}
