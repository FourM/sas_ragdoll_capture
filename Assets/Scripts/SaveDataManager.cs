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

    public static int GetLevelWebNum(){ return PlayerPrefs.GetInt("levelWebNum", 1); }
    public static void SetLevelWebNum( int currentStage ){ PlayerPrefs.SetInt("levelWebNum", currentStage); }

    public static int GetLevelStartPos(){ return PlayerPrefs.GetInt("LevelStartPos", 1); }
    public static void GetLevelStartPos( int level ){ PlayerPrefs.SetInt("LevelStartPos", level); }

    public static float GetEndlessBattleBestScore(){ return PlayerPrefs.GetFloat("EndlessBattleBestScore", 0); }
    public static void SetEndlessBattleBestScore( float score ){ PlayerPrefs.SetFloat("EndlessBattleBestScore", score); }
}
