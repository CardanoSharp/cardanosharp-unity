using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{

    public void SaveData(string name, string data)
    {
        PlayerPrefs.SetString(name, data);
        PlayerPrefs.Save();
    }

    public string GetData(string name)
    {
        return PlayerPrefs.GetString(name);
    }

    public bool Exists(string name)
    {
        return PlayerPrefs.HasKey(name);
    }

    public void DeleteData(string name) 
    { 
        PlayerPrefs.DeleteKey(name); 
    }
}
