using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class GameStage : MonoBehaviour
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
    [SerializeField, Tooltip("背景色")] private Color32 _backGroundColor = default;
    [SerializeField, Tooltip("メイン環境光を用いるか")] private bool _isMainLight = true;
    private List<Human> _targethumanList = default;
    private Action _onCliearCallback = default;
    private UnityEvent _onInitialize = null;
    private bool _isInitialize = false;
    private bool _isClear = false;
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
            targetHuman.AddOnBreakCallback(()=>
            {
                _targethumanList.Remove(targetHuman);
            });

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

        MaterialManager.instance.SetBackGroundColor(_backGroundColor);
        MaterialManager.instance.SetEnableMainLight(_isMainLight);
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
    public string GetStageId(){ return _stageId; }
    public bool IsGimmickKill(){ return _isGimmickKill; }
    // ---------- Private関数 ----------
}
