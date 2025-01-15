using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class ButtonGoEndlessMode : MonoBehaviour
{
    // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    [SerializeField, Tooltip("インゲームUIマネージャー")] private InGameUIManager _inGameUIManager = default;
    [SerializeField, Tooltip("ボタン")] private CustomButton _button = default;
    [SerializeField, Tooltip("アイコン")] private GameObject _goEndlessIcon = default;
    [SerializeField, Tooltip("アイコン")] private GameObject _goMainIcon = default;
    [SerializeField, Tooltip("ライフアイコン")] private Transform _lifeIcon = default;
    [SerializeField, Tooltip("ゲージ")] private Slider _endlessLifeGuage = default;
    [SerializeField, Tooltip("ライフ数")] private TextMeshProUGUI _endlessLife = default;
    [SerializeField, Tooltip("ボタン活性時のゲームオブジェクト")] private List<GameObject> _activeObjects = default;
    [SerializeField, Tooltip("ボタン非活性時のゲームオブジェクト")] private List<GameObject> _unActiveObjects = default;
    private bool _isShow = true;
    private bool _beforeIsActive = false;
    private bool _isActive = true;
    private int _currentDispLife = 0;
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    private void Awake(){
        _inGameUIManager.AddOnInitialize(Initialize);
    }
    private void Initialize()
    {
        ChangeIcon(GameDataManager.GameMode);
        _button.onClick.AddListener(()=>
        {
            if( GameDataManager.GameMode != GameMode.endlessBattle)
            {
                GameDataManager.InGameMainEvent.ChangeGameMode(GameMode.endlessBattle);
                // ChangeIcon(GameMode.endlessBattle);
            }
            else
            {
                GameDataManager.InGameMainEvent.ChangeGameMode(GameMode.main);
                // ChangeIcon(GameMode.main);
            }
        });
        GameDataManager.AddOnChangeGameMode(ChangeIcon);
        GameDataManager.AddOnChangeGameState(OnGameState);

        GameObject thisObject = this.gameObject;
        Transform thisTransform = this.transform;
        GameDataManager.AddOnStageStart(()=>
        {
            // 30ステージ以降に到達した
            if( 29 <= SaveDataManager.GetCurrentStage() && !_isShow)
            {
                // thisObject.SetActive(true);
                // thisTransform.localScale = Vector3.one;
                
                Sequence seq = DOTween.Sequence();
                seq.Append(thisTransform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).SetLink(thisObject));
                seq.SetLink(thisObject);
            }
        });
        if( SaveDataManager.GetCurrentStage() < 29)
        {
            // this.gameObject.SetActive(false);
            _isShow = false;
            this.transform.localScale = Vector3.zero;
        }

        _isActive = IsActiveButton();

        // ゲージ更新時の処理
        GameDataManager.AddOnUpdateEndlessLife(UpdateLifeView);
        _currentDispLife = SaveDataManager.GetEndlessLife();
        
        UpdateLifeView();
    }
    // ---------- Public関数 -------------------------
    // ---------- Private関数 ------------------------
    // ゲームモードに応じて表示更新
    private void ChangeIcon(GameMode gameMode)
    {
        if(gameMode == GameMode.endlessBattle)
        {
            _goMainIcon.SetActive(true);
            _goEndlessIcon.SetActive(false);
        }
        if(gameMode == GameMode.main)
        {
            _goMainIcon.SetActive(false);
            _goEndlessIcon.SetActive(true);
        }
    }
    private void OnGameState( GameState gameState )
    {
        if(( gameState == GameState.main && GameDataManager.GameMode == GameMode.main ) || gameState == GameState.startWait)
        {
            this.gameObject.SetActive(true);
        }
        else
        {
            this.gameObject.SetActive(false);
        }
        // _inGameUIManager.AddOnShowUI(()=>{ this.gameObject.SetActive(true); });
        // _inGameUIManager.AddOnHideUI(()=>{ this.gameObject.SetActive(false); });
    }
    // ライフやゲージの更新時に表示更新
    private void UpdateLifeView()
    {
        _endlessLifeGuage.value = SaveDataManager.GetEndlessLifeGuage();
        int life = SaveDataManager.GetEndlessLife();
        // ライフが増えた演出
        if( _currentDispLife < life )
        {
            PlayAnimationAddLife();
        }
        _currentDispLife = life;
        _endlessLife.text = life + "";

        // ライフが１以上ならアクティブにする
        _isActive = IsActiveButton();

        _button.IsEnable = _isActive;
        for( int i = 0; i < _activeObjects.Count; i++ )
        {
            _activeObjects[i].SetActive(_isActive);
        }
        for( int i = 0; i < _unActiveObjects.Count; i++ )
        {
            _unActiveObjects[i].SetActive(!_isActive);
        }

        if(_isActive)
        {
            this.transform.localScale = Vector3.one;
        }
        else
        {
            this.transform.localScale = Vector3.one * 0.9f;
        }
        if(!_beforeIsActive && _isActive)
            PlayAnimationIsActiveButton();

        _beforeIsActive = _isActive;
    }
    // ライフが増えた演出
    private void PlayAnimationAddLife()
    {
        Sequence seq = DOTween.Sequence();
        Vector3 initScale = _lifeIcon.localScale;

        seq.Append(_lifeIcon.DOScale(initScale * 1.8f, 0.3f).SetEase(Ease.InOutBack).SetLink(_lifeIcon.gameObject));
        seq.Append(_lifeIcon.DOScale(initScale, 0.3f).SetEase(Ease.OutBack).SetLink(_lifeIcon.gameObject));
    }
    // ボタンが有効化された演出
    private void PlayAnimationIsActiveButton()
    {
        Sequence seq = DOTween.Sequence();
        Vector3 initScale = this.transform.localScale;
        this.transform.localScale = Vector3.one * 0.9f;

        seq.Append(this.transform.DOScale(initScale * 1.1f, 0.3f).SetEase(Ease.InOutBack).SetLink(this.gameObject));
        seq.Append(this.transform.DOScale(initScale, 0.3f).SetEase(Ease.OutBack).SetLink(this.gameObject));
    }

    // ボタンの有効化チェック　
    private bool IsActiveButton()
    {
        bool isActive = false;
        if( 1 <= SaveDataManager.GetEndlessLife() 
        || GameDataManager.GameMode == GameMode.endlessBattle 
        || GameDataManager.DebugEndlessUnLimit )
            isActive = true;
        return isActive;
    }
}
