using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敵の能動的行動ハンドラー（基本的にEndlessBattleHumanから一部機能を抜き出しただけ）
/// </summary>
public class HumanActiveActionHandler : MonoBehaviour
{
   // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    [SerializeField, Tooltip("Hub")] private HumanHub _humanHub = default;
    [SerializeField, Tooltip("トリガー")] private InterfaceReference<IEventTrigger> _iEventTrigger = default;
    // [SerializeField, Tooltip("プレイヤーが通過したらこれがアクションを起こすパス")] private EndlessBattlePath _triggerPath = default;
    [SerializeField, Tooltip("能動的アクション")] private HumanActiveAction _activeActionControllrer = null;
    private Shield _shield = null;
    private Human _human = null;
    private bool _isAttack = false;
    private bool _isDead = false;
    private float _attackCounter = 0f;
    private List<RectTransform> _shieldList = null;
    private EndlessBattleHumanState _state = EndlessBattleHumanState.idle;
    private EndlessBattleHumanState _beforState = EndlessBattleHumanState.idle;
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    private void Awake() {
        _humanHub.AddOnInitialize(Initialize);
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
                    _attackCounter = 1000000;
                }
            }
            else
            {
                GameDataManager.InGameMainEvent.OnEnemyAttackCansel();
                _isAttack = false;
            }
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
        // プレイヤーが近接攻撃範囲内に入ったら攻撃待機
        _human = _humanHub.GetActiveHuman();
        _shield = _human.GetHaveShield();

        _human.AddOnBreakCallback(()=>{
            if(!_isDead)
            {
                _isDead = true;
            }
        });

        // プレイヤーが指定のパスを通過したら能動的行動を始める（待機する）
        if(_iEventTrigger != null)
        {
            _iEventTrigger.Value.AddOnEventTrigger(()=>
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
}
