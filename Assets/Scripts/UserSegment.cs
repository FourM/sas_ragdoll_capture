using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Analytics;

// ABテストフラグ設定クラス。初回起動時にのみ実行する
public class UserSegment : MonoBehaviour
{
    // ---------- Public関数 ----------
    public void Initialize()
    {
        // 初回起動の処理済みか確認。0:未処理、1:処理済み
        int isInitializeUserSegment = PlayerPrefs.GetInt("isInitializeUserSegment", 0);
        if( isInitializeUserSegment == 1 )
            return;
        PlayerPrefs.SetInt("isInitializeUserSegment", 1);

        // HumanはバナナマンかStickManか
        int is_BananaMan = 1;
        // int is_BananaMan = Random.Range(0, 2);
        // Humanが死ぬ衝撃の強さ
        int killShockStrength = Random.Range(0, 2);
        // 操作キャラのおててをクモっぽくするか
        int is_SpiderSkin = 1;
        // int is_SpiderSkin = Random.Range(0, 2);
        // キャラクターが起き上がるか 0:立ち上がる、1：立ち上がらない
        int is_Recovery = Random.Range(0, 2);
        // レベルバンドル（ステージの順番）
        int level_Bundle = Random.Range(0, 2);
        // 一部ステージはギミックでないと倒せないか
        int gimmick_Kill = Random.Range(0, 2);

        

        // セーブデータに保存
        PlayerPrefs.SetInt("is_BananaMan", is_BananaMan);
        PlayerPrefs.SetInt("killShockStrength", killShockStrength);
        PlayerPrefs.SetInt("is_SpiderSkin", is_SpiderSkin);
        PlayerPrefs.SetInt("is_Recovery", is_Recovery);
        PlayerPrefs.SetInt("gimmick_Kill", gimmick_Kill);

        
        // ユーザープロパティ
        // Firebase.Analytics.FirebaseAnalytics.SetUserProperty("is_BananaMan",PlayerPrefs.GetInt("is_BananaMan").ToString());
        Firebase.Analytics.FirebaseAnalytics.SetUserProperty("killShockStrength",PlayerPrefs.GetInt("killShockStrength").ToString());
        Firebase.Analytics.FirebaseAnalytics.SetUserProperty("is_Recovery",PlayerPrefs.GetInt("is_Recovery").ToString());
        Firebase.Analytics.FirebaseAnalytics.SetUserProperty("Level_Bundle",PlayerPrefs.GetInt("level_Bundle").ToString());
        // Firebase.Analytics.FirebaseAnalytics.SetUserProperty("is_SpiderSkin",PlayerPrefs.GetInt("is_SpiderSkin").ToString());
        Firebase.Analytics.FirebaseAnalytics.SetUserProperty("Gimmick_Kill",PlayerPrefs.GetInt("gimmick_Kill").ToString());
    }
    // ---------- Private関数 ----------
}
