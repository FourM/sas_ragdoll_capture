using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 反復横跳びする床
/// </summary>
public class RepeatableMove : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    // [SerializeField, Tooltip("動く床")] private IHumanGetter _humanGetter = null;
    [SerializeField, Tooltip("動く床")] private EndlessBattleSegment _endlessBattleSegment = default;
    [SerializeField, Tooltip("動く床")] private Transform _moveFloor = default;
    [SerializeField, Tooltip("動く床の移動幅")] private float _moveX = default;
    [SerializeField, Tooltip("動く床の移動時間")] private float _duration = default;
    [SerializeField, Tooltip("動く床の移動のイージング")] private Ease _ease = default;
    [SerializeField, Tooltip("動く床の移動のイージング")] private Transform _roller = default;
    [SerializeField, Tooltip("クランク")] private Transform _crank = default;
    [SerializeField, Tooltip("クランクが向く対象")] private Transform _crankLook = default;
    [SerializeField, Tooltip("クランクが向く対象")] private Transform _floorPos = default;
    private float _moveFloorPosY = 0f;
    private List<Tween> _tweenList = default;
    private Transform _humanParent = default;
    private Human _human = null;
    private Vector3 _moveFloorBasePos = default;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    private void Start()
    {
        Vector3 pos = _moveFloor.position;
       pos.x = -_moveX;
       _moveFloor.position = pos;
       _moveFloorPosY = _moveFloor.position.y;

        _tweenList = new List<Tween>();
       Tween tween = _roller.DOLocalRotate(new Vector3(0, 0, 360f), _duration * 2, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);  
       _tweenList.Add(tween);

       _humanParent = this.transform.parent;

       _moveFloorBasePos = _moveFloor.position;
    }
    private void Update()
    {
        // if(_humanGetter != null && _human == null)
        // {
        //     _human = _humanGetter.GetHuman(0);
            
        //     if(_human != null)
        //         SetHuman(_human, _humanGetter.GetHumanParent());
        // }
        if(_endlessBattleSegment != null && _human == null)
        {
            _human = _endlessBattleSegment.GetHuman(0);
            
            if(_human != null)
                SetHuman(_human, _endlessBattleSegment.GetHumanParent());
        }

        _crank.LookAt(_crankLook);
        Vector3 ang = _crank.localEulerAngles;
        _crank.localEulerAngles = ang;

        Vector3 pos = _floorPos.position;
        pos.y = _moveFloorPosY;
        _floorPos.position = pos;
        pos = _floorPos.localPosition;
        pos.y = 0f;
        _floorPos.localPosition = pos;
        
        pos = _moveFloorBasePos;
        pos.x = _floorPos.position.x;
        pos.z = _floorPos.position.z;
        _moveFloor.position = pos;
        _moveFloorBasePos = pos;
        // _moveFloor.position += _moveFloor.forward * 2.0f;
    }

    private void OnDisable() {
        if(_tweenList != null)
        {
            for(int i = 0; i < _tweenList.Count; i++)
            {
                _tweenList[i].Kill();
            }
        }
    }
    // ---------- Public関数 ----------
    public void SetHuman(Human human, Transform parent)
    {
       human.transform.parent = _moveFloor;
       human.transform.localEulerAngles = new Vector3(0, 180, 0);
       human.transform.localPosition = new Vector3(0, 1f, -1f);

       human.AddCallbackOnCatch(()=>
       {
            human.transform.parent = parent;
       });
    }
    // ---------- Private関数 ----------
}
