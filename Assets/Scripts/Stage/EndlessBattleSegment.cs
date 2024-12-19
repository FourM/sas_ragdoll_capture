using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class EndlessBattleSegment : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("ステージのId")] private String _stageId = "noName";
    [SerializeField, Tooltip("ABフラグによってはギミックで倒すステージか")] private bool _isGimmickKill = false;
    [SerializeField, Tooltip("ターゲットリスト")] private List<HumanHub> _targethumanhubList = default;
    [SerializeField, Tooltip("ターゲットリスト")] private List<CatchableObj> _HumanHandyList = default;
    [SerializeField, Tooltip("Humanの捕まる前の参考constraints")] private Rigidbody _rafConstraints = null;
    [SerializeField, Tooltip("長さ")] private float _length = 10.0f;
    [SerializeField, Tooltip("プレイヤーの移動パス")] private List<Transform> _pathList = null;
    [SerializeField, Tooltip("次のセグメントのアングル")] private Vector3 _nextSegmentAddAngle = default;
    [SerializeField, Tooltip("次のセグメントの位置")] private Transform _nextSegmentPos = null;
    [SerializeField, Tooltip("プレイヤーが見る位置")] private Transform _lookAtTarget = null;
    [SerializeField, Tooltip("トリガー")] private ChildTrigger _childTrigger = null;
    private List<Human> _targethumanList = default;
    private Action _onCliearCallback = default;
    private UnityEvent _onInitialize = null;
    private bool _isInitialize = false;
    private bool _isClear = false;
    private float _pathLength = -1f;
    public float PathLength
    { 
        get{ return _pathLength; }
        set{ if(_pathLength == -1f) _pathLength = value; } 
    }
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    private void Update(){
        // クリア
        if(_targethumanList.Count <= 0 && !_isClear)
        {
            _isClear = true;
            _onCliearCallback?.Invoke();
        }
    }
    // ---------- Public関数 ----------
    public void Initialize(){
        if(_isInitialize) return;
        _isInitialize = true;

        _targethumanList = new List<Human>();

        for(int i = 0; i < _targethumanhubList.Count; i++)
        {
            int index = i;

            // HumanHubから、有効なHumanを取得
            _targethumanhubList[index].Initialize(index);
            Human targetHuman = _targethumanhubList[index].GetActiveHuman();
            targetHuman.Initialize();
            // ターゲットのHumanをリストに追加
            _targethumanList.Add(targetHuman);
            // コールバック設定
            // targetHuman.AddOnBreakCallback(()=>
            // {
            //     _targethumanList.Remove(targetHuman);
            // });

            if(_rafConstraints != null)
                targetHuman.GetRigidbody().constraints = _rafConstraints.constraints;
        }

        // Humanが手に持つ物のリスト
        for(int i = 0; i < _HumanHandyList.Count; i++)
        {
            int index = i;

            CatchableObj obj = _HumanHandyList[index];

            Vector3 pos = obj.transform.localPosition;
            Vector3 ang = obj.transform.localEulerAngles;

            Transform hand = null;
            hand = _targethumanList[index].GetParts(HumanParts.handR).transform;
            if(hand != null)
                obj.transform.parent = hand;
            // else
            //     Debug.Log("handがNullだぞい");

            _targethumanList[index].AddOnBreakCallback(()=>
            {
                obj.transform.parent = this.transform;
                obj.GetRigidbody().useGravity = true;
                obj.GetRigidbody().isKinematic = false;
            });

            obj.transform.localPosition = pos;
            obj.transform.localEulerAngles = ang;
        }

        _onInitialize?.Invoke();

        if(_isGimmickKill && PlayerPrefs.GetInt("Gimmick_Kill", 1) == 1)
            GameDataManager.SetGimmickKill(true);
        else
            GameDataManager.SetGimmickKill(false);
    }
    public Human GetHuman(int index = 0)
    { 
        if(index < _targethumanList.Count)
            return _targethumanList[index]; 
        return null;
    }
    public int GetHumanNum(){ return _targethumanList.Count; }
    public void SetOnClearCallBack( Action onCliearCallback)
    {
        _onCliearCallback = onCliearCallback;
    }
    public void AddOnInitialize( UnityAction onInitialize)
    {
        if(_onInitialize == null)
            _onInitialize = new UnityEvent();
        _onInitialize.AddListener(onInitialize);
    }
    public float GetLength(){ return _length; }
    public string GetStageId(){ return _stageId; }
    public bool IsGimmickKill(){ return _isGimmickKill; }
    public Vector3 GetNextSegmentAddAngle(){ return _nextSegmentAddAngle; }
    public Transform GetLookAtTarget(){ return _lookAtTarget; }
    public Transform GetNextSegmentPos(){ return _nextSegmentPos; }
    public List<Transform> GetPathList()
    { 
        if(_pathList == null)
            _pathList = new List<Transform>();
        return _pathList; 
    }
    public bool isAllKill()
    {
        // Debug.Log("今の区画のクリア判定。人数：" + _targethumanList.Count);
        for(int i = 0; i < _targethumanList.Count; i++)
        {
            Human human = _targethumanList[i];
            // 生きてる&カメラ内にいるヤツが一人でもいたらNo
            if(human.IsVisible && !human.IsBroken())
                return false;
        }
        return true;
    }

    public void DestroyThis()
    {
        for(int i = 0; i < _targethumanList.Count; i++)
        {
            Human human = _targethumanList[i];
            if(human != null)
                human.DisableReady();

            // Debug.Log("わんたそ2");
        }
        Destroy(this.gameObject);
    }

    public void AddCallbackOnTriggerEnter(UnityAction<Collider> onTriggerEnter)
    {
        if(_childTrigger == null)
            return;
        // Debug.Log("トリガー設定:" + this.gameObject.name);
        _childTrigger.AddCallbackOnTriggerEnter(onTriggerEnter);
    }  
    // ---------- Private関数 ----------
}
