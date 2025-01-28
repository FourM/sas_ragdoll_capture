using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;


public class GimmickMagma : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField] private InterfaceReference<IInitializer> _iInitializer;
    [SerializeField, Tooltip("トリガー")] private ChildTrigger _childTrigger;
    [SerializeField, Tooltip("煙エフェクト")] private List<ParticleSystem> _effectSmokeList;
    private bool _isInitialize = false;
    private bool _isLavaKill = false;
    private int _showSmokeNum = 0;
    private List<Human> _humanList = null;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    private void Awake(){
        _iInitializer.Value.AddOnInitialize(Initialize);
        _humanList = new List<Human>(); 
    }
    private void Initialize()
    {
        if(_isInitialize)
            return;
        _isInitialize = true;
        
        _childTrigger.AddCallbackOnTriggerEnter(TryPlaySmoke);
    }
    // private void OnDisable() {
        
    // }
    // ---------- Public関数 ----------
    // ---------- Private関数 ----------
    private void TryPlaySmoke(Collider other)
    {
        Human human = null;
        HumanChild humanChild = null;
        CatchableObj collitionChatchableObj = GameDataManager.GetCatchableObj(other.gameObject);
        
        if(collitionChatchableObj != null)
            humanChild = collitionChatchableObj.TryGetHumanChild();
        if(humanChild != null)
            human = humanChild.Gethuman();
        if( human == null )
            return;
        if( _humanList.Contains(human) ) 
            return;
        // 煙エフェクトを出し切ったら何もしない
        if(_effectSmokeList.Count <= _showSmokeNum)
            return;

        human.OnRelease();

        human.OnBreak();
        _humanList.Add(human);

        // int index = _stage.GetHumanNum();
        int index = _showSmokeNum;

        ParticleSystem _effectSmoke = _effectSmokeList[index];

        Vector3 pos = other.transform.position;
        pos.y = _effectSmoke.transform.position.y;
        _effectSmoke.transform.position = pos;
        _effectSmoke.Play();
        _showSmokeNum++;
    }
}