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
    [SerializeField, Tooltip("HPバー")] private Slider _hpBar = default;
    [SerializeField, Tooltip("HPバ-の色")] private Image _hpBarFill = default;
    [SerializeField, Tooltip("ダメージ")] private Transform _damage = default;
    [SerializeField, Tooltip("ダメージ")] private TextMeshProUGUI _textDamage = default;
    private Human _activeHuman = null;
    private bool _isAttack = false;
    private bool _isAttackEnd = false;
    private bool _isSetHpBar = false;
    private bool _isDead = false;
    private Sequence _damageSeq = default;
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
                _hpBar.gameObject.SetActive(true);
            }
        }
        _hpBar.transform.rotation = Camera.main.transform.rotation;
        _damage.transform.rotation = Camera.main.transform.rotation;
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
            // Debug.Log("あーあ");
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
        _hpBar.value = 100f;
        _hpBar.gameObject.SetActive(false);
        _damage.localScale = Vector3.zero;
    }
    private bool IsCanAttack()
    {
        // return !_activeHuman.IsCatch() && !_activeHuman.IsDead() && _activeHuman.IsGround() && _activeHuman.IsEnableAnimation();
        return !_activeHuman.IsCatch() && !_activeHuman.IsDead() && _activeHuman.IsGround();
    }
    private void OnDamage(float damage)
    {
        // Debug.Log("OnDamage:1");
        _hpBar.value = _activeHuman.HP / _activeHuman.MaxHP * 100f;
        if(_hpBar.value <= 0 )
        {
            _hpBar.gameObject.SetActive(false);
        }
        else
        {
            _hpBar.gameObject.SetActive(true);
        }    
        
        if(_hpBar.value <= 25 )
            _hpBarFill.color = new Color32(255, 0, 0, 255);
        else if( _hpBar.value <= 50 )
            _hpBarFill.color = new Color32(255, 225, 0, 255);
        else
            _hpBarFill.color = new Color32(0, 225, 0, 255);


        if(_damageSeq != null)
        {
            _damageSeq.Kill();
        }
        _damageSeq = DOTween.Sequence();
        _textDamage.text = "" + Mathf.Round(damage);
        _damage.localScale = Vector3.zero;
        _textDamage.transform.localScale = Vector3.zero;
        _damageSeq.Append(_damage.DOScale(Vector3.one * 4, 0.2f).SetEase(Ease.Linear));
        _damageSeq.Join(_textDamage.transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack));
        _damageSeq.AppendInterval(1.5f);
        _damageSeq.Append(_damage.DOScale(Vector3.zero, 0.1f).SetEase(Ease.InBack));
    }
}
