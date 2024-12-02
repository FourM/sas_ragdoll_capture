using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SaveDataManager
{
    private static int _currentStage = 0;

    // セーブデータのロード
    public static void LoadData()
    {
        _currentStage = PlayerPrefs.GetInt("currentStage", 0);
    }

    public static int GetCurrentStage(){ return PlayerPrefs.GetInt("currentStage", 0); }
    public static void SetCurrentStage( int currentStage ){ PlayerPrefs.SetInt("currentStage", currentStage); }
}
