using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using UnityEngine.Events;

public class HumanHub : MonoBehaviour, IInitializer
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    [SerializeField, Tooltip("Humanリスト")] private List<Human> _ListHuman;
    [SerializeField, Tooltip("レイヤー")] private int _childLayer = 20;
    [SerializeField, Tooltip("アニメーションコントローラー")] private RuntimeAnimatorController _animeController = null;
    [SerializeField, Tooltip("怯みアニメーション")] private RuntimeAnimatorController _animFlinch = null;
    [SerializeField, Tooltip("盾")] private Shield _shield = null;
    [SerializeField, Tooltip("この敵に触れてなくても落ちることがあるか(崩れる床の上にいるやつとかはONにする)")] private bool _isFallable = true;
    [SerializeField, Tooltip("激しいアニメーションをするなどで床ダメで勝手に死なない(事故死)ロック。プレイヤーに捕まったり落下したりしたらOFFにする")] private bool _initIsFloorDead = true;
    [SerializeField, Tooltip("Humanリスト")] private ChildTrigger _showHumanCheckar;
    // ---------- プロパティ ----------
    private Human _activeHuman = null;
    private bool _isInitialize = false;
    private Vector3 _scale = default;
    // private UnityEvent _onInitialize = null;
    private bool _isVisibleCheck = false;
    private Transform _cameraTransform = null;
    private Transform _showHumanCheckarTransform = null;
    [field: SerializeField] public InitializerBase Initializer { get; set; }    // 擬似多重継承インターフェースクラスを用いるときに必要な処理1/2。publicだけど基本的に外部からは使わない
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    private void Awake()
    {
        Initializer.Init(this, this);  // 擬似多重継承インターフェースクラスを用いるときに必要な処理2/2。
    }
    private void Update()
    {
        // if(this.gameObject.name == "HumanHub EndlessBattle Mob1")
        // {
        //     Debug.Log(this.gameObject.name + ",チェックだお:" + _isVisibleCheck);    
        // }
        if(_isVisibleCheck && GameDataManager.GameMode != GameMode.main)
        {
            // if(this.gameObject.name == "HumanHub EndlessBattle Mob1")
            // {
            //     Debug.Log(this.gameObject.name + ",チェックするお:" + _isVisibleCheck);    
            // }
            TryShowHuman();
        }
    }
    // ---------- Public関数 ----------
    public void Initialize(int layer = 0)
    { 
        if(_isInitialize )
            return;
        _isInitialize = true;

        _childLayer = 20 + layer;

        _scale = this.transform.localScale;
        this.transform.localScale = Vector3.one;

        // int activeIndex = PlayerPrefs.GetInt("is_BananaMan",0);

        int activeIndex = 0;
        if(PlayerPrefs.GetInt("is_BananaMan") == 1)
        {
            if(PlayerPrefs.GetInt("Change_Background") == 0)
            {
                if(PlayerPrefs.GetInt("Change_Character") == 0)
                    activeIndex = 1;    // 今までのバナナマン
                else
                    activeIndex = 3;    // 調整モデル+今までの配色
            }
            else
            {
                if(PlayerPrefs.GetInt("Change_Character") == 0)
                    activeIndex = 2;    // 新しい配色今までのバナナマンモデル
                else
                    activeIndex = 4;    // 新配色＋調整モデル
            }
        }

        _activeHuman = Instantiate(_ListHuman[activeIndex]);
        _activeHuman.gameObject.SetActive(true);
        _activeHuman.transform.parent = this.transform;
        _activeHuman.transform.localPosition = Vector3.zero;
        _activeHuman.transform.localScale = _scale;
        _activeHuman.transform.localEulerAngles = Vector3.one;
        _activeHuman.SetChildLayer(_childLayer);
        _activeHuman.SetFlinchAnim(_animFlinch);
        _activeHuman.IsFallable = _isFallable;
        _activeHuman.InitIsFloorDead = _initIsFloorDead;

        if(_animeController != null)
            _activeHuman.SetAnimatorController(_animeController);
        else
            DOVirtual.DelayedCall(0.01f, ()=>{
                if(_activeHuman != null)
                    _activeHuman.DesableAnimation();
            });

        Transform ghost = _activeHuman.GetGhost();
        ghost.parent = this.transform;
        ghost.localScale = _scale;

        if(_shield != null)
            _activeHuman.AddOnInitialize(HaveShield);

        // _onInitialize?.Invoke();
        // _onInitialize?.RemoveAllListeners();
        Initializer.OnInitialize?.Invoke();
        Initializer.OnInitialize?.RemoveAllListeners();

        this.enabled = false;
        
        if(GameDataManager.GameMode != GameMode.main)
        {   
            _cameraTransform = Camera.main.transform;
            _showHumanCheckarTransform = _showHumanCheckar.transform;
            _activeHuman.gameObject.SetActive(false);
            _showHumanCheckar.AddOnWillRenderObject(()=>{
                // if(Camera.current.name != "Main Camera")
                // {
                //     if(Camera.current == null)
                //         Debug.Log("Camera.current is NULL!!");
                //     else
                //         Debug.Log("Camera.current is " + Camera.current.name);
                // }
                if (Camera.current != null && Camera.current.name != "Main Camera") 
                    return;

                _isVisibleCheck = true;
                this.enabled = true;
                TryShowHuman();
                // if(this.gameObject.name == "HumanHub EndlessBattle Mob1")
                // {
                //     Debug.Log(this.gameObject.name + ",視覚内に入ったお:" + this.enabled + ", " + _isVisibleCheck);    
                // }
            });
            _showHumanCheckar.AddOnBecameInVisible(()=>{
                _isVisibleCheck = false;
                this.enabled = false;
                // if(this.gameObject.name == "HumanHub EndlessBattle Mob1")
                // {
                //     Debug.Log(this.gameObject.name + ",見えないお:" + this.enabled + ", " + _isVisibleCheck);    
                // }
            });
        }
        else
        {
            _showHumanCheckar.gameObject.SetActive(false);
        }
    }

    private void TryShowHuman()
    {
        // if(this.gameObject.name == "HumanHub EndlessBattle Mob1")
        // {
        //     Debug.Log(this.gameObject.name + ",チェックを始めるお:" + _isVisibleCheck);    
        // }
        Vector3 dir = _showHumanCheckarTransform.position - _cameraTransform.position;
        LayerMask mask = LayerMask.GetMask("ShowHumanChecker", "Floor", "Default");
        if (Physics.Raycast(_cameraTransform.position, dir, out RaycastHit hit, dir.magnitude, mask, QueryTriggerInteraction.Ignore))
        {
            
            if (hit.transform == _showHumanCheckarTransform.transform)
            {
                // Debug.Log($"{gameObject.name} はカメラに本当に見えている！");
                _activeHuman.gameObject.SetActive(true);
                _showHumanCheckar.gameObject.SetActive(false);
                _isVisibleCheck = false;
            }
            else
            {
                // Debug.Log($"{gameObject.name} は視野内だけど遮蔽物に隠れている！:" + hit.transform.gameObject.name );

                _activeHuman.gameObject.SetActive(false);
                _showHumanCheckar.gameObject.SetActive(true);
            }
        }
    }


    public Human GetActiveHuman(){ return _activeHuman; }

    public void AddOnInitialize( UnityAction onInitialize)
    {
        // if(_onInitialize == null)
        //     _onInitialize = new UnityEvent();
        // _onInitialize.AddListener(onInitialize);
        Initializer.AddOnInitialize(onInitialize);
    }
    // ---------- Private関数 ----------
    private void HaveShield()
    {
        if(_shield == null)
            return;
        _shield.SetUp( this.transform, _activeHuman, _activeHuman.GetParts(HumanParts.forearmL) );
    }
}
