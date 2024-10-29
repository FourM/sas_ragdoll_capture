using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum HandAction
{
    idle,
    attack,
}
/// <summary>
/// 手のアニメーション
/// </summary>
public class Hand : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    // [SerializeField, Tooltip("おてて")] private GameObject _hand = default;
    [SerializeField, Tooltip("おててのアニメーション")] private Animator _handAnimator = default;
    private string[] character_anim_parameter = {"Idle", "Attack"};
    private HandAction _handAction;    // 手の挙動
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    // 初期化
    private void Start(){
        ChangeAction(HandAction.idle);
    }

    // ---------- Public関数 ----------
    // ---------- Private関数 ----------
    // アニメーション変更
    public void ChangeAction(HandAction action)
    {
        if(_handAction == action)
            return;

        _handAction = action;
        foreach(string animParamin in character_anim_parameter)
        {
            _handAnimator.SetBool(animParamin, false);
        }
        switch(action)
        {
            case HandAction.idle:
                _handAnimator.SetBool("Idle", true);
                break;
            case HandAction.attack:
                _handAnimator.SetBool("Attack", true);
                break;
        }
    }
}
