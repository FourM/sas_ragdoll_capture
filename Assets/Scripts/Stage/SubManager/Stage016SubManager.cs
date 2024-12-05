using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 溶岩ステージ
/// </summary>
public class Stage016SubManager : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("ステージ")] private GameStage _stage;
    [SerializeField, Tooltip("トリガー")] private ChildTrigger _childTrigger;
    [SerializeField, Tooltip("煙エフェクト")] private List<ParticleSystem> _effectSmokeList;
    private bool _isInitialize = false;
    private bool _isLavaKill = false;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    private void Awake(){
        _stage.AddOnInitialize(Initialize);
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
        if( human.IsDead() )
            return;

        human.OnRelease();

        human.OnBreak();

        int index = _stage.GetHumanNum();

        ParticleSystem _effectSmoke = _effectSmokeList[index];

        Vector3 pos = other.transform.position;
        pos.y = _effectSmoke.transform.position.y;
        _effectSmoke.transform.position = pos;
        _effectSmoke.Play();
    }
}
