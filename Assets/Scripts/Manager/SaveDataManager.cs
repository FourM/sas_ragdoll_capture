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

    // エンドレスステージを遊べるスタミナ
    public static int GetEndlessLife(){ return PlayerPrefs.GetInt("endlessLife", GameDataManager.InitEndlessBattleLife); }
    public static void SetEndlessLife( int value ){ PlayerPrefs.SetInt("endlessLife", value); }

    // エンドレスステージを遊べるスタミナ
    public static float GetEndlessLifeGuage(){ return PlayerPrefs.GetFloat("endlessLifeGuage", 0); }
    public static void SetEndlessLifeGuage( float value ){ PlayerPrefs.SetFloat("endlessLifeGuage", value); }

    // エンドレスステージを遊べるスタミナ
    public static int GetHumanKillNum(){ return PlayerPrefs.GetInt("humanKillNum", 0); }
    public static void SetHumanKillNum( int value ){ PlayerPrefs.SetInt("humanKillNum", value); }
    // エンドレスステージを遊んだ回数
    public static int GetPlayEndlessCount(){ return PlayerPrefs.GetInt("playEndlessCount", 0); }
    public static void SetPlayEndlessCount( int value ){ PlayerPrefs.SetInt("playEndlessCount", value); }
    // エンドレスバトル開放の演出をしたことがあるか
    public static int GetIsDirectFirstOpenEndlessBattle(){ return PlayerPrefs.GetInt("isDirectFirstOpenEndlessBattle", 0); }
    public static void SetIsDirectFirstOpenEndlessBattle( int value ){ PlayerPrefs.SetInt("isDirectFirstOpenEndlessBattle", value); }

    public static int GetEndlessStart(){ return PlayerPrefs.GetInt("Endless_Start_Stage", 0); }
    // リワードを有効にするか
    public static int GetIsRewarded(){ return PlayerPrefs.GetInt("Rewarded_Life_ON", 0); }
    // 敵はメガネをかけるか
    public static int IsEnemyglasses(){ return PlayerPrefs.GetInt("Enemyglasses", 0); }
}
