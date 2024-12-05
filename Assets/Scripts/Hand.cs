using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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
    [SerializeField, Tooltip("左手")] HandSkin _leftHand;    // 手
    [SerializeField, Tooltip("右手")] HandSkin _rightHand;    // 手
    private string[] character_anim_parameter = {"Idle", "Attack"};
    private HandAction _handAction;    // 手の挙動
    private Tween _handRotateTween = null;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    // 初期化
    private void Start(){
        ChangeAction(HandAction.idle);
    }

    // ---------- Public関数 ----------
    public HandSkin SetLeftHandSkin(HandSkin handSkinPrefab)
    {
        SetHandSkin(ref _leftHand, handSkinPrefab);
        _handAnimator = _leftHand.GetHandAnimator();
        return _leftHand;
    }
    public HandSkin SetRightHandSkin(HandSkin handSkinPrefab)
    {
        SetHandSkin(ref _rightHand, handSkinPrefab);
        return _rightHand;
    }
    // ---------- Private関数 ----------
    private HandSkin SetHandSkin(ref HandSkin handSkin, HandSkin handSkinPrefab )
    {
        Vector3 pos = handSkin.transform.localPosition;
        Vector3 scale = handSkin.transform.localScale;
        Vector3 angle = handSkin.transform.localEulerAngles;
        Transform parent = handSkin.transform.parent;

        Destroy(handSkin.gameObject);
        handSkin = Instantiate(handSkinPrefab);
        handSkin.transform.parent = parent;
        handSkin.transform.localPosition = pos;
        handSkin.transform.localScale = scale;
        handSkin.transform.localEulerAngles = angle;

        return handSkin;
    }
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
                HandRotate(new Vector3(345.04f,175.04f,127.11f));
                break;
            case HandAction.attack:
                _handAnimator.SetBool("Attack", true);
                HandRotate(new Vector3(345.04f,182.9f,140.2f));
                break;
        }
    }

    private void HandRotate( Vector3 rotate )
    {
        if(_handRotateTween != null)
        {
            _handRotateTween.Kill();
        }
        _handRotateTween = _handAnimator.transform.DOLocalRotate(rotate, 0.15f);
    }
}
