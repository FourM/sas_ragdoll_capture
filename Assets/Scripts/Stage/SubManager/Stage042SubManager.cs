using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Stage042SubManager : StageSubManager
{
    // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    [SerializeField, Tooltip("トリガー")] private ChildTrigger _switchTrigger = null;
    [SerializeField, Tooltip("ボタン押して下がる距離")] private float _moveY = 0.25f;
    [SerializeField, Tooltip("ボタンがONになった時のレンダラー")] private Renderer _switchRenderer = null;
    [SerializeField, Tooltip("ボタンがONになった時のマテリアル")] private Material _onSwitchMaterial = null;
    [SerializeField, Tooltip("スイッチオンの音")] private AudioSource _soundSwitchOn = null;


    [SerializeField, Tooltip("丸太")] private Transform _woodHammer = default;
    [SerializeField, Tooltip("丸太")] private AnimationCurve _animCurve = default;
    [SerializeField, Tooltip("丸太")] private float _duration = default;
    [SerializeField, Tooltip("丸太")] private float _woodPosY = default;
    private bool _isShot = false;
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    // ---------- Public関数 -------------------------
    // ---------- Private関数 ------------------------
    protected override void InitializeUnique()
    {
        _tweenList = new List<Tween>();
        _switchTrigger.AddCallbackOnTriggerEnter((Collider other)=>
        {
            if(_isShot)
                return;
            CatchableObj collitionChatchableObj = GameDataManager.GetCatchableObj(other.gameObject);
            if(collitionChatchableObj == null)
                return;
            CommonSwichOnEffect();


            Tween tween = _woodHammer.DOMoveY(_woodPosY, _duration).SetEase(_animCurve);
            _tweenList.Add(tween);

            _isShot = true;
        });
    }

    // スイッチの共通演出処理
    private void CommonSwichOnEffect()
    {
        // スイッチの見た目を変える
        _switchRenderer.material = _onSwitchMaterial;
        Vector3 pos = _switchTrigger.transform.position;
        pos.y -= _moveY;
        _switchTrigger.transform.position = pos;

        // Sequence sequence = DOTween.Sequence();

        // Debug.Log("_shotBall.transform.eulerAngles:" + _shotBall.transform.eulerAngles + ", " + _shotBall.transform.forward + ", " + _shotBall.transform.up);
        // Debug.Log(":" + _shotBall.transform.forward * _frontVelocity + ", " + _shotBall.transform.up * _upVelocity + ", " + velocity);
    }
}
