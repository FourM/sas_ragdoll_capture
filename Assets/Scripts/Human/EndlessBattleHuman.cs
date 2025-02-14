using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public enum EndlessBattleHumanState
{
    idle,           //  アイドリング
    ActiveAction,   //  何かしらをトリガーに起こす能動的行動（プレイヤーに近づいてくるなど）
    attack,         //  攻撃
    guard,          //  ガード
    flinch          //  怯み
}
public class EndlessBattleHuman : MonoBehaviour, IAttacker
{
    // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    [SerializeField, Tooltip("Hub")] private HumanHub _humanHub = default;
    [SerializeField, Tooltip("Hub")] private ChildTrigger _attackTrigger = default;
    [SerializeField, Tooltip("Hub")] private ChildTrigger _lookTrigger = default;
    [SerializeField, Tooltip("Hub")] private float _attackTime = 0.3f;
    [SerializeField, Tooltip("攻撃アニメーション")] private RuntimeAnimatorController _attackAnimation = default;
    [SerializeField, Tooltip("プレイヤーが通過したらこれがアクションを起こすパス")] private EndlessBattlePath _triggerPath = default;
    [SerializeField, Tooltip("能動的アクション")] private HumanActiveAction _activeActionControllrer = null;
    [SerializeField, Tooltip("盾")] private Shield _shield = null;
    [SerializeField, Tooltip("こいつを見るか")] private bool _isLook = true;
    private Human _human = null;
    private bool _isAttack = false;
    private bool _isDead = false;
    private bool _AttackWait = false;
    private float _attackCounter = 0f;
    private List<RectTransform> _shieldList = null;
    private EndlessBattleHumanState _state = EndlessBattleHumanState.idle;
    private EndlessBattleHumanState _beforState = EndlessBattleHumanState.idle;
    [field: SerializeField] public AttackerBase AttackerBaseClass { get; set; } 
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    private void Awake() {
        _humanHub.AddOnInitialize(Initialize);
        AttackerBaseClass.Init(this, this);
    }
    private void Update()
    {
        if(_isAttack && GameDataManager.GameState != GameState.result)
        {
            if(IsCanAttack())
            {
                _attackCounter -= Time.deltaTime;
                if( _attackCounter <= 0 )
                {
                    GameDataManager.InGameMainEvent.OnEnemyAttackHit();
                    _attackCounter = 10000000;
                }
            }
            else
            {
                GameDataManager.InGameMainEvent.OnEnemyAttackCansel();
                _isAttack = false;
            }
        }
        // 攻撃できなくなったら攻撃待機を解除する
        if(_AttackWait && !IsCanAttack())
        {
            _human.RemoveActionChangeWaitCallBack(HumanAttack);
            _AttackWait = false;
        }

        if(GameDataManager.GameState != GameState.result)
        {
            // ステータス別の行動
            switch(_state)
            {
                case EndlessBattleHumanState.ActiveAction:
                    UpdateActiveAction();
                    break;
            }
        }

        // スローモーション判定
        AttackerBaseClass.CheckAttackConfirmed(()=>
        {
            if(_isAttack && _attackCounter < 0.5f)
                return true;
            return false;
        });
    }
    private void FixedUpdate() {
        if(GameDataManager.GameState == GameState.result)
            return;
         // ステータス別の行動
        switch(_state)
        {
            case EndlessBattleHumanState.ActiveAction:
                FixedUpdateActiveAction();
                break;
        }
    }

    private void OnDisable()
    {
        AttackerBaseClass.AttackEnd();
    }
    // ---------- Public関数 -------------------------
    // ---------- Private関数 ------------------------
    private void Initialize()
    {
        // プレイヤーが近接攻撃範囲内に入ったら攻撃待機
        _human = _humanHub.GetActiveHuman();
        _attackTrigger.AddCallbackOnTriggerEnter((Collider collider)=>{
            if(IsCanAttack())
            {
                _AttackWait = true;
                _human.AddActionChangeWaitCallBack(HumanAttack);
            }
        });
        // プレイヤーが近接攻撃範囲外に出たら攻撃待機を解除
        _lookTrigger.AddCallbackOnTriggerExit((Collider collider)=>{
            if(_AttackWait)
            {
                _human.RemoveActionChangeWaitCallBack(HumanAttack);
                _AttackWait = false;
            }
        });      

        _human.AddOnDamage(OnDamage);
        _human.AddOnBreakCallback(()=>{
            if(!_isDead)
            {
                GameDataManager.InGameMainEvent.EndlessBattleOnEnemyBreak(_human);
                _isDead = true;
            }
        });

        // プレイヤーが指定のパスを通過したら能動的行動を始める（待機する）
        if(_triggerPath != null)
        {
            _triggerPath.AddOnPassCallback(()=>
            { 
                _human.AddActionChangeWaitCallBack(()=>{
                    ChangeState(EndlessBattleHumanState.ActiveAction); 
                });
            });
        }

        _activeActionControllrer?.SetHuman(_human);
        _activeActionControllrer?.Iniiialize();


        // シールドを持っているなら
        if(_shield != null)
        {
            // シールドが取られた時のコールバック設定
            _shield.AddOnCatch(()=>
            {
                ChangeState(EndlessBattleHumanState.guard);
            });
            // 構えをやめた時のコールバック設定
            _shield.AddOnCompleteGuardEnd(()=>
            {
                ChangeState(_beforState);
            });
        }
        // 怯んだ時と、それが終わった時のコールバック設定
        _human.AddCallbackOnFlinch(()=>{
            ChangeState(EndlessBattleHumanState.flinch);
        });
        _human.AddCallbackOnFlinchEnd(()=>{
            ChangeState(_beforState);
        });
        _human.AddOnCatch(()=>{
            _isAttack = false;
            _attackCounter = 1000000;
        });
        _human.AddOnBreakCallback(()=>{
            _isAttack = false;
            _attackCounter = 1000000;
        });


        _human.AddOnInitialize(()=>{
            if(_isLook)
                AttackerBaseClass.LookTransform = _human.GetParts(HumanParts.head).transform;
            else
                AttackerBaseClass.LookTransform = null;
        });
    }
    private bool IsCanAttack()
    {
        bool ret = true;
        ret &= !_human.IsCatch();
        ret &= !_human.IsDead();
        ret &= _human.IsGround();
        return ret;
    }
    private void OnDamage(float damage)
    {

    }

    // ステータス変更
    private void ChangeState(EndlessBattleHumanState state)
    {
        if(_state == state)
            return;

        if(_state == EndlessBattleHumanState.ActiveAction)
        {
            _activeActionControllrer?.Pause();
        }

        _beforState = _state;
        _state = state;

        if(_state == EndlessBattleHumanState.attack)
        {
            _attackCounter = _attackTime;
            _isAttack = true;
        }
        else
        {
            _isAttack = false;
        }
        if(_state == EndlessBattleHumanState.ActiveAction)
        {
            _activeActionControllrer?.StartActiveAction();
        }
        // ガード以外のアニメーションに移行した時
        if(_state != EndlessBattleHumanState.guard)
        {
            // ガードをやめる待機処理を削除
            _shield?.CanselGuardEndTween();
        }
    }

    // 能動的行動を開始
    private void UpdateActiveAction()
    {
        if(IsCanAttack())
            _activeActionControllrer?.UpdateActiveAction();
    }
    // 能動的行動を開始
    private void FixedUpdateActiveAction()
    {
        if(IsCanAttack())
            _activeActionControllrer?.FixedUpdateActiveAction();
    }
    // 敵の近接攻撃
    private void HumanAttack()
    {
        // このHumanが攻撃できる状態にある
        if(IsCanAttack() && GameDataManager.GameState == GameState.main)
        {
            // Debug.Log("攻撃！:" + collider.name + ", " + collider.gameObject.layer);
            GameDataManager.InGameMainEvent.OnEnemyAttackStart(_human, _isLook);
            // InGameManager.instance.OnEnemyAttackStart()　と書くよりも、InGameManagerへの強い依存関係をなくせる

            _human.SetAnimatorController(_attackAnimation);
            _human.EnableAnimation();
            _human.SetIsCanGuard(false);
            ChangeState(EndlessBattleHumanState.attack);
        }
    }   
}
