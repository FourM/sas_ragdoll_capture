using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Analytics;

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
        if(isInit==0){
            AddOpen();
            isInit=1;
        }
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
    public void EventTapCount(int location, int is_thread)
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("Tap_Count",
                            new Parameter("Stage", PlayerPrefs.GetInt("currentStage", 0) + 1),
                            new Parameter("location", location),
                            new Parameter("is_thread", is_thread));
        // Debug.Log("Tap_Count:" + location + ", " + is_thread);
    }
    /// <summary>
    /// 指を離したイベント
    /// </summary>
    /// <param name="taptime"></param>
    public void EventTapRelese(float taptime)
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("Tap_Release",
                            new Parameter("Stage", PlayerPrefs.GetInt("currentStage", 0) + 1),
                            new Parameter("taptime", taptime.ToString("F1")));
        // Debug.Log("tap_release:" + taptime.ToString("F1"));
    }
    public void EventWatchInste(bool isWatch)
    {
        FirebaseAnalytics.LogEvent("Watch_Inste", 
                            new Parameter("Stage", PlayerPrefs.GetInt("currentStage", 0) + 1),
                            new Parameter("CanWatch", isWatch.ToString()));
        // Debug.Log("Watch_Inste");
    }
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
    private void AddOpen()
    {
        // ユーザープロパティ取得
        Firebase.Analytics.FirebaseAnalytics.SetUserProperty("is_BananaMan",PlayerPrefs.GetInt("is_BananaMan", 1).ToString());
        Firebase.Analytics.FirebaseAnalytics.SetUserProperty("killShockStrength",PlayerPrefs.GetInt("killShockStrength", 0).ToString());
        Firebase.Analytics.FirebaseAnalytics.SetUserProperty("is_SpiderSkin",PlayerPrefs.GetInt("is_SpiderSkin", 0).ToString());
    }
}
