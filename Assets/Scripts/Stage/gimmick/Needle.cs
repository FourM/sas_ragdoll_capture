using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Needle : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    // 触れた相手を倒す
    private void OnCollisionEnter(Collision collision)
    {
        CatchableObj catchableObj = GameDataManager.GetCatchableObj(collision.transform.gameObject);
        if(catchableObj != null )
            catchableObj.OnBreak();
    }
    // ---------- Public関数 ----------
    // ---------- Private関数 ----------
}
