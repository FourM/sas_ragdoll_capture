using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敵の能動的行動ハンドラー（基本的にEndlessBattleHumanから一部機能を抜き出しただけ）
/// IAttackerインターフェースは近接攻撃用。クラスを分けれたら外す
/// </summary>
public class HumanActiveActionHandler : MonoBehaviour, IAttacker
{
   // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    [SerializeField, Tooltip("Hub")] private HumanHub _humanHub = default;
    [SerializeField, Tooltip("トリガー")] private InterfaceReference<IEventTrigger> _iEventTrigger = null;
    [SerializeField, Tooltip("能動的アクション")] private HumanActiveAction _activeActionControllrer = null;

    // プレイヤーが近くなったら殴りかかる処理　できればクラスを分けたい
    [SerializeField, Tooltip("Hub")] private ChildTrigger _attackTrigger = default;
    [SerializeField, Tooltip("プレイヤー検知トリガー")] private ChildTrigger _recognitionTrigger = default;
    [SerializeField, Tooltip("攻撃アニメーション")] private RuntimeAnimatorController _attackAnimation = default;
    [SerializeField, Tooltip("Hub")] private float _attackTime = 1.0f;
    [SerializeField, Tooltip("こいつを見るか")] private bool _isLook = true;

    // シールド
    private Shield _shield = null;
    private Human _human = null;
    private bool _isAttack = false;
    private bool _isInAttackRange = false;
    private bool _isDead = false;
    private float _attackCounter = 0f;
    private List<RectTransform> _shieldList = null;
    private EndlessBattleHumanState _state = EndlessBattleHumanState.idle;
    private EndlessBattleHumanState _beforState = EndlessBattleHumanState.idle;
    private bool _isActiveActionTriggered = false;

    // 近接攻撃に関するパラメータ
    private bool _AttackWait = false;
    [field: SerializeField] public AttackerBase AttackerBaseClass { get; set; } 
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    private void Awake() {
        _humanHub.AddOnInitialize(Initialize);
        // 近接攻撃に関する処理
        AttackerBaseClass.Init(this, this);
    }
    private void Update()
    {
        // 敵の近接攻撃
        if(_isAttack && GameDataManager.GameState != GameState.result)
        {
            if(IsCanAttack())
            {
                _attackCounter -= Time.deltaTime;
                if( _attackCounter <= 0 )
                {
                    GameDataManager.InGameMainEvent.OnEnemyAttackHit();
                    _attackCounter = 1000000;
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
            ChangeState(EndlessBattleHumanState.idle); 
            _human.AddActionChangeWaitCallBack(()=>{
                if(_isActiveActionTriggered)
                {
                    ChangeState(EndlessBattleHumanState.ActiveAction); 
                }
                else
                {
                    ChangeState(EndlessBattleHumanState.idle); 
                }
            });
        }
        // スローモーション判定
        AttackerBaseClass.CheckAttackConfirmed(()=>
        {
            if(_isAttack && _attackCounter < 0.5f)
                return true;
            return false;
        });


        
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
    // ---------- Public関数 -------------------------
    // ---------- Private関数 ------------------------
    private void Initialize()
    {
        _human = _humanHub.GetActiveHuman();
        _human.AddOnInitialize(()=>
        {
            _shield = _human.GetHaveShield();

            _human.AddOnBreakCallback(()=>{
                if(!_isDead)
                {
                    _isDead = true;
                }
            });

            // 特定の条件を満たしたら能動的行動を始める
            if(_iEventTrigger != null && _iEventTrigger.Value != null)
            {
                _iEventTrigger.Value.AddOnEventTrigger(()=>
                { 
                    _human.AddActionChangeWaitCallBack(()=>{
                        ChangeState(EndlessBattleHumanState.ActiveAction); 
                    });
                    _isActiveActionTriggered = true;
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
                    // Debug.Log("およよお？？");
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


            if(_attackTrigger != null)
            {
                // プレイヤーが近づいたら殴る処理
                _attackTrigger.AddCallbackOnTriggerEnter((Collider collider)=>{
                    if(IsCanAttack())
                    {
                        _AttackWait = true;
                        _isInAttackRange = true;
                        // 攻撃コマンドを待機させる
                        _human.AddActionChangeWaitCallBack(HumanAttack);
                    }
                });
                // プレイヤーが近接攻撃範囲外に出たら攻撃待機を解除
                _attackTrigger.AddCallbackOnTriggerExit((Collider collider)=>{
                    if(_AttackWait)
                    {
                        _isInAttackRange = false;
                        _human.RemoveActionChangeWaitCallBack(HumanAttack);
                        _AttackWait = false;
                    }
                }); 
            }

            if(_recognitionTrigger != null)
            {
                // プレイヤーを検知して行動を起こしてくる処理
                _recognitionTrigger.AddCallbackOnTriggerEnter((Collider collider)=>{
                    _human.AddActionChangeWaitCallBack(()=>{
                        ChangeState(EndlessBattleHumanState.ActiveAction); 
                    });
                    _isActiveActionTriggered = true;
                });
            }
            
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

        // 近接攻撃
        if(_state == EndlessBattleHumanState.attack)
        {
            if(_isInAttackRange )
            {
                _attackCounter = _attackTime;
                _isAttack = true;
            }
            else
            {
                ChangeState(EndlessBattleHumanState.idle);
            }
        }
        else
        {
            _attackCounter = 1000000;
            _isAttack = false;
        }

        if(_state == EndlessBattleHumanState.idle)
        {
            _human.SetAnimatorController(_humanHub.GetIdleAnimation());
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
            GameDataManager.InGameMainEvent.OnEnemyAttackStart(_human, _isLook);

            _human.SetAnimatorController(_attackAnimation);
            _human.EnableAnimation();
            _human.SetIsCanGuard(false);
            ChangeState(EndlessBattleHumanState.attack);
        }
    } 
}
