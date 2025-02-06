using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crab : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    [SerializeField, Tooltip("トリガー")] private ChildTrigger _childTrigger;
    [SerializeField, Tooltip("トリガーリスト")] private List<ChildTrigger> _listChildTrigger;
    [SerializeField, Tooltip("蟹アニメーター")] private Animator _animator = null;
    [SerializeField, Tooltip("蟹の攻撃判定")] private GameObject _attackRect = default; 
    [SerializeField, Tooltip("攻撃発生")] private float _attackDuration = 0.5f; 
    [SerializeField, Tooltip("敵")] private List<HumanHub> _listHuman;
    private bool _isAttack = false;
    private Transform _target = null;
    // private Human _human = null;
    // ---------- プロパティ ----------
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    // ---------- Public関数 ----------
    // ---------- Private関数 ----------
    private void Awake()
    {
        for(int i = 0; i < _listHuman.Count; i++)
        {
            HumanHub humanHub = _listHuman[i];

            humanHub.AddOnInitialize(()=>
            {
                Human human = humanHub.GetActiveHuman();
                // human.AddOnCatch(()=>{ TryStartAttack(0f); });
                human.AddOnBreakCallback(()=>{ TryStartAttack(0f); });
            });
        }
    }
    private void Start()
    {
        _childTrigger.AddCallbackOnJointBreak(TryStartAttack);
        for(int i = 0; i < _listChildTrigger.Count; i++)
        {
            _listChildTrigger[i].AddCallbackOnTriggerEnter(TryStartAttack);
        }
        _attackRect.SetActive(false);
        enabled = false;
    }

    private void FixedUpdate() {
        _attackDuration -= Time.deltaTime;
        if(_attackDuration <= 0 )
            _attackRect.SetActive(true);
    }

    private void TryStartAttack(Collider collider)
    {
        TryStartAttack();
    }

    private void TryStartAttack(float onJointBreak)
    {
        TryStartAttack();
    }
    private void TryStartAttack()
    {
        if(_isAttack)
            return;
        _isAttack = true;

        _animator.SetBool("isAttacking", true);

        enabled = true;
    }
}
