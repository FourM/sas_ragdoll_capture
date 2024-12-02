using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// コンテナ
/// </summary>
public class Stage046SubManager : StageSubManager
{
    // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    [SerializeField, Tooltip("トリガー")] private List<ChildTrigger> _childTriggerList;
    private int _ropeNum = 2;
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    // ---------- Public関数 -------------------------
    // ---------- Private関数 ------------------------
    protected override void InitializeUnique()
    {
        for(int i = 0; i < _childTriggerList.Count; i++)
        {
            _childTriggerList[i].AddCallbackOnJointBreak((float breakForce)=>
            {
                _ropeNum--;

                if(_ropeNum <= 0 && _stage != null && _stage.GetHuman(0) != null)
                    _stage.GetHuman(0).SetToughness(0.4f);
            });
        }
    }
}
