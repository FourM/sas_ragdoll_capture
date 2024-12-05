using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Analytics;
using System;

// Firebaseマネージャー。イベント発火の関数を定義する。
public class FirebaseManager : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    public int isInit;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    public static FirebaseManager instance = null;
    // ---------- Unity組込関数 ----------
    private void Awake()
    {
        if(instance == null)
            instance = this;
        else    
            Destroy(this);
    }
    private void Start() {
    }
    // ---------- Public関数 ----------
    public void EventStageStart()
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("Stage_Start",
                            new Parameter("Stage", PlayerPrefs.GetInt("currentStage", 0) + 1));
        // Debug.Log("Stage_Start");
    }
    public void EventStageClear()
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("Stage_Clear",
                            new Parameter("Stage", PlayerPrefs.GetInt("currentStage", 0) + 1));
        // Debug.Log("Stage_Clear");
    }

    /// <summary>
    /// タッチしたイベント
    /// </summary>
    /// <param name="location"> 0:的をタップ、1:的外をタップ、2:手をタップ </param>
    /// <param name="is_thread"> 0:糸が出なかった、1:糸が出た</param>
    /// <param name="X_coordinate"> マウスタップの位置X</param>
    /// <param name="Y_coordinate"> マウスタップの位置Y</param>
    public void EventTapCount(int location, int is_thread, float X_coordinate, float Y_coordinate)
    {
        int stage = PlayerPrefs.GetInt("currentStage", 0) + 1;  // 現在のステージ
        double X_coordinateDouble = (double)X_coordinate;
        double X_coordinateDouble2 = Math.Round(X_coordinateDouble, 3);
        double Y_coordinateDouble = (double)Y_coordinate;
        double Y_coordinateDouble2 = Math.Round(Y_coordinateDouble, 3);

        Firebase.Analytics.FirebaseAnalytics.LogEvent("Tap_Count",
                            new Parameter("Stage", stage),
                            new Parameter("location", location),
                            new Parameter("is_thread", is_thread),
                            new Parameter("X_coordinate", X_coordinateDouble2),
                            new Parameter("Y_coordinate", Y_coordinateDouble2));
        // Debug.Log("Tap_Count:" + stage + ", " + location + ", " + is_thread + ", " + X_coordinate + ", " + Y_coordinate + ", " + X_coordinateDouble2 + ", " + Y_coordinateDouble2);
    }
    /// <summary>
    /// 指を離したイベント
    /// </summary>
    /// <param name="taptime">タップ時間</param>　
    /// <param name="is_defeat">画面タップしながら敵を倒したか</param>　
    public void EventTapRelese(float taptime, bool is_defeat, int location, int is_thread)
    {
        double taptimeDouble = (double)taptime;
        double taptimeDouble2 = Math.Round(taptimeDouble, 1);
        // string taptimeString = taptime.ToString("F1");          // タップ時間をStringにする
        int stage = PlayerPrefs.GetInt("currentStage", 0) + 1;  // 現在のステージ
        int is_defeatInt = Convert.ToInt32(is_defeat);

        Firebase.Analytics.FirebaseAnalytics.LogEvent("Tap_Release",
                            new Parameter("Stage", stage),
                            new Parameter("taptime", taptimeDouble2),
                            new Parameter("is_defeat", is_defeatInt),
                            new Parameter("location", location),
                            new Parameter("is_thread", is_thread));
        // Debug.Log("tap_release:" + stage + ", タップ時間：" + taptime + ", " + taptimeDouble2 + ", 倒した？：" + is_defeatInt + ", どこタップした？：" + location + ", 糸でた？：" + is_thread);
    }
    public void EventWatchInste(bool isWatch)
    {
        int watchInsteCount = -1;
        if(isWatch)
        {
            watchInsteCount = PlayerPrefs.GetInt("WatchInsteCount", 1);
        }

        FirebaseAnalytics.LogEvent("Watch_Inste", 
                            new Parameter("Stage", PlayerPrefs.GetInt("currentStage", 0) + 1),
                            new Parameter("CanWatch", isWatch.ToString()),
                            new Parameter("WatchInsteCount", watchInsteCount));
        // Debug.Log("isWatch:" + isWatch + ", " + watchInsteCount);
        if(isWatch)
        {
            watchInsteCount++;
            PlayerPrefs.SetInt("WatchInsteCount", watchInsteCount);
        }
    }
    // public void EventWatchInste(bool isWatch, double revenue)
    // {
    //     int watchInsteCount = -1;
    //     if(isWatch)
    //     {
    //         watchInsteCount = PlayerPrefs.GetInt("WatchInsteCount", 1);
    //     }

    //     FirebaseAnalytics.LogEvent("Watch_Inste", 
    //                         new Parameter("Stage", PlayerPrefs.GetInt("currentStage", 0) + 1),
    //                         new Parameter("CanWatch", isWatch.ToString()),
    //                         new Parameter("Revenue", revenue),
    //                         new Parameter("WatchInsteCount", watchInsteCount));
    //     // Debug.Log("isWatch:" + isWatch + ", " + watchInsteCount);
    //     if(isWatch)
    //     {
    //         watchInsteCount++;
    //         PlayerPrefs.SetInt("WatchInsteCount", watchInsteCount);
    //     }
    // }
    public void EventWatchBanner(bool isWatch)
    {
        FirebaseAnalytics.LogEvent("Watch_Banner", 
                            new Parameter("Stage", PlayerPrefs.GetInt("currentStage", 0) + 1),
                            new Parameter("CanWatch", isWatch.ToString()));
    }
    // public void EventWatchBanner(bool isWatch, double revenue)
    // {
    //     FirebaseAnalytics.LogEvent("Watch_Banner", 
    //                         new Parameter("Stage", PlayerPrefs.GetInt("currentStage", 0) + 1),
    //                         new Parameter("Revenue", revenue),
    //                         new Parameter("CanWatch", isWatch.ToString()));
    //     // Debug.Log("Watch_Inste、Revenue:" + revenue);
    // }
    public void EventReStart()
    {
        FirebaseAnalytics.LogEvent("Stage_Restart", 
                            new Parameter("Stage", PlayerPrefs.GetInt("currentStage", 0) + 1));
        // Debug.Log("Stage_Restart");
    }
    public void EventCrashed(float impact, bool death)
    {
        if(impact < 8f )
            return;
        int deathInt = 0;
        if(death)
            deathInt = 1;

        // Debug.Log("impact:" + impact.ToString("F1") + ", death:" + death);

        // FirebaseAnalytics.LogEvent("Crashed", 
        //                     new Parameter("Stage", PlayerPrefs.GetInt("currentStage", 0) + 1),
        //                     new Parameter("Impact", impact.ToString("F1")),
        //                     new Parameter("Death", deathInt));
    }
    // ---------- Private関数 ----------
    // ---------- Private関数 ----------
}
