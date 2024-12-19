using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessBattleHuman : MonoBehaviour
{
    // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    [SerializeField, Tooltip("Hub")] private HumanHub _humanHub = default;
    [SerializeField, Tooltip("Hub")] private ChildTrigger _childTrigger = default;
    [SerializeField, Tooltip("Hub")] private float _attackTime = 0.3f;
    [SerializeField, Tooltip("攻撃アニメーション")] private RuntimeAnimatorController _attackAnimation = default;
    private Human _activeHuman = null;
    private bool _isAttack = false;
    private bool _isAttackEnd = false;
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    private void Awake() {
        _humanHub.AddOnInitialize(Initialize);
    }
    private void Update()
    {
        if(_isAttack && !_isAttackEnd)
        {
            if(IsCanAttack())
            {
                _attackTime -= Time.deltaTime;
                if( _attackTime <= 0 )
                {
                    GameDataManager.InGameMainEvent.OnEnemyAttackHit();
                    _isAttackEnd = true;
                }
            }
            else
            {
                GameDataManager.InGameMainEvent.OnEnemyAttackCansel();
                _isAttack = false;
            }
        }
    }
    // ---------- Public関数 -------------------------
    // ---------- Private関数 ------------------------
    private void Initialize()
    {
        // Debug.Log("初期設定！");
        _activeHuman = _humanHub.GetActiveHuman();
        _childTrigger.transform.parent = _activeHuman.transform;
        _childTrigger.AddCallbackOnTriggerEnter((Collider collider)=>{
            // Debug.Log("あーあ");
            // このHumanが攻撃できる状態にある
            if(IsCanAttack())
            {
                // Debug.Log("攻撃！:" + collider.name + ", " + collider.gameObject.layer);
                GameDataManager.InGameMainEvent.OnEnemyAttackStart();
                // InGameManager.instance.OnEnemyAttackStart()　と書くよりも、InGameManagerへの強い依存関係をなくせる

                _activeHuman.SetAnimatorController(_attackAnimation);
                _isAttack = true;
            }
        });      
    }
    private bool IsCanAttack()
    {
        return !_activeHuman.IsCatch() && !_activeHuman.IsDead() && _activeHuman.IsGround() && _activeHuman.IsEnableAnimation();
    }
}
