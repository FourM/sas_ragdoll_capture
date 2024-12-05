using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 反復横跳びする床
/// </summary>
public class Stage035SubManager : StageSubManager
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("動く床")] private Transform _moveFloor = default;
    [SerializeField, Tooltip("動く床の移動幅")] private float _moveX = default;
    [SerializeField, Tooltip("動く床の移動時間")] private float _duration = default;
    [SerializeField, Tooltip("回る板クランクの基盤付")] private Transform _roller = default;
    [SerializeField, Tooltip("回る板２")] private Transform _roller2 = default;
    [SerializeField, Tooltip("クランク")] private Transform _crank = default;
    [SerializeField, Tooltip("クランクが向く対象")] private Transform _crankLook = default;
    [SerializeField, Tooltip("クランクが向く対象")] private Transform _floorPos = default;
    private float _moveFloorPosY = 0f;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    protected override void InitializeUnique()
    {
       Human human = _stage.GetHuman(0);
       human.transform.parent = _moveFloor;

       human.AddCallbackOnCatch(()=>
       {
            human.transform.parent = _stage.transform;
       });
       human.AddOnBreakCallback(()=>
       {
            human.transform.parent = _stage.transform;
       });

        Vector3 pos = _moveFloor.position;
       pos.x = -_moveX;
       _moveFloor.position = pos;
       _moveFloorPosY = _moveFloor.position.y;

       Tween tween = _roller.DOLocalRotate(new Vector3(0, 0, -360f), _duration * 2, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);  
       _tweenList.Add(tween);
        tween = _roller2.DOLocalRotate(new Vector3(0, 0, 360f), _duration * 2, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);  
       _tweenList.Add(tween);
    }
    
    protected override void UpdateUnique()
    {
        _crank.LookAt(_crankLook);
        Vector3 ang = _crank.localEulerAngles;
        _crank.localEulerAngles = ang;

        Vector3 pos = _floorPos.position;
        pos.y = _moveFloorPosY;
        _floorPos.position = pos;
        pos = _floorPos.localPosition;
        // pos.x = 0f;
        // pos.z = 0f;
        pos.y = 0f;
        _floorPos.localPosition = pos;
        
        pos = _moveFloor.position;
        pos.x = _floorPos.position.x;
        pos.z = _floorPos.position.z;
        _moveFloor.position = pos;
    }
    // ---------- Public関数 ----------
    // ---------- Private関数 ----------
}
