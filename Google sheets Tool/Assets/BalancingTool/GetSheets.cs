using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GetSheets : MonoBehaviour
{
    public void StartObtainSheetConnection(string sheetsLink, System.Action<UnityWebRequest> callback)
    {
        StartCoroutine(ObtainSheetConnection(sheetsLink, callback));
    }

    public IEnumerator ObtainSheetConnection(string sheetsLink, System.Action<UnityWebRequest> callback)
    {
        UnityWebRequest www = UnityWebRequest.Get(sheetsLink);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("Error: " + www.error);
            callback(null);
        }
        else
        {
            callback(www);
        }
        //DestroyImmediate(gameObject);
    }
}
