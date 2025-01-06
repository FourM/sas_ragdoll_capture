using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class EndlessBattleHuman : MonoBehaviour
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
    [SerializeField, Tooltip("キャンバス")] private Canvas _canvas = default;
    [SerializeField, Tooltip("盾アイコンコンテナ")] private RectTransform _shieldContainer = default;
    [SerializeField, Tooltip("盾アイコンプレハブ")] private RectTransform _shieldIconPrefab = default;
    private Human _activeHuman = null;
    private bool _isAttack = false;
    private bool _isAttackEnd = false;
    private bool _isSetHpBar = false;
    private bool _isDead = false;
    private List<RectTransform> _shieldList = null;
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

        if(!_isSetHpBar)
        {
            if(_activeHuman != null && _activeHuman.GetParts(HumanParts.head) != null)
            {
                _canvas.transform.parent = _activeHuman.GetParts(HumanParts.head).transform;
                _attackTrigger.transform.parent = _activeHuman.GetParts(HumanParts.body).transform;
                _lookTrigger.transform.parent = _activeHuman.GetParts(HumanParts.body).transform;
                _isSetHpBar = true;
                InitShield();
            }
        }
        _shieldContainer.transform.rotation = Camera.main.transform.rotation;
        _canvas.transform.rotation = Camera.main.transform.rotation;
    }
    // ---------- Public関数 -------------------------
    // ---------- Private関数 ------------------------
    private void Initialize()
    {
        // Debug.Log("初期設定！");
        _activeHuman = _humanHub.GetActiveHuman();
        _attackTrigger.AddCallbackOnTriggerEnter((Collider collider)=>{
            // Debug.Log("あーあ");
            // このHumanが攻撃できる状態にある
            if(IsCanAttack())
            {
                // Debug.Log("攻撃！:" + collider.name + ", " + collider.gameObject.layer);
                GameDataManager.InGameMainEvent.OnEnemyAttackStart(_activeHuman);
                // InGameManager.instance.OnEnemyAttackStart()　と書くよりも、InGameManagerへの強い依存関係をなくせる

                _activeHuman.SetAnimatorController(_attackAnimation);
                _activeHuman.EnableAnimation();
                _isAttack = true;
            }
        });      
        _lookTrigger.AddCallbackOnTriggerEnter((Collider collider)=>{
            // このHumanが攻撃できる状態にある
            if(IsCanAttack())
            {
                GameDataManager.InGameMainEvent.OnEnemyLook(_activeHuman);
            }
        });      

        _activeHuman.AddOnDamage(OnDamage);
        _activeHuman.AddOnBreakCallback(()=>{
            if(!_isDead)
            {
                GameDataManager.InGameMainEvent.EndlessBattleOnEnemyBreak(_activeHuman);
                _isDead = true;
            }
        });
    }
    private bool IsCanAttack()
    {
        // return !_activeHuman.IsCatch() && !_activeHuman.IsDead() && _activeHuman.IsGround() && _activeHuman.IsEnableAnimation();
        return !_activeHuman.IsCatch() && !_activeHuman.IsDead() && _activeHuman.IsGround();
    }
    private void InitShield()
    {
        _shieldList = new List<RectTransform>();
        foreach( Transform child in _shieldContainer.transform)
        {
            Destroy(child.gameObject);
        }
        int hp = _activeHuman.MaxHP;
        for(int i = 0; i < (hp - 1); i++)
        {
            RectTransform shield = Instantiate(_shieldIconPrefab);
            shield.parent = _shieldContainer.transform;
            _shieldList.Add(shield);
            shield.transform.localScale = Vector3.one;
            shield.transform.localPosition = Vector3.zero;
            shield.transform.localEulerAngles = Vector3.zero;
        }
    }
    private void OnDamage(float damage)
    {
        int hp = _activeHuman.HP;
        for(int i = _shieldList.Count - 1; (hp - 1) <= i; i--)
        {
            if( 0 <= i)
                _shieldList[i].gameObject.SetActive(false);
        }
    }
}
