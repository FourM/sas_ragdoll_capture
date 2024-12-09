using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessBattleSubManager : StageSubManager
{
    // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    [SerializeField, Tooltip("hoge")] private List<GameStage> _stagePrefabs = default;
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    protected override void InitializeUnique(){
        for(int i = 0; i < _stagePrefabs.Count; i++)
        {
            GameStage segment = Instantiate(_stagePrefabs[i]);
            segment.transform.parent = this.transform;
            segment.transform.localPosition = Vector3.zero;

            
        }
    }
    // ---------- Public関数 -------------------------
    // ---------- Private関数 ------------------------
}
