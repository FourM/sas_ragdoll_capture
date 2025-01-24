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
    [SerializeField, Tooltip("ライフ演出アイコン")] private Transform _lifeEffectIconParent = default;
    [SerializeField, Tooltip("ライフ演出アイコン黄")] private Transform _lifeEffectIcon1 = default;
    [SerializeField, Tooltip("ライフ演出アイコン紫")] private Transform _lifeEffectIcon2 = default;
    [SerializeField, Tooltip("ライフ演出開始位置")] private Transform _lifeStartPos = default;
    [SerializeField, Tooltip("ライフ演出目的位置")] private Transform _lifeEndPos = default;
    private bool _isShow = true;
    private bool _beforeisShow = false;
    private bool _beforeIsActive = false;
    private bool _isActive = true;
    private int _currentViewLife = 0;
    private int _waitViewLife = 0;
    private Sequence _lifeUpSequence = null;
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    private void Awake(){
        _inGameUIManager.AddOnInitialize(Initialize);
    }
    private void Initialize()
    {
        GameDataManager.AddOnDebugChangeUserSegment((string value)=>
        {
            if(value == "UnlimitedMode_ON")
            {
                UpdateLifeView();
            }
        });

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
        GameDataManager.AddOnStageStart(UpdateLifeView);

        _isActive = IsActiveButton();
        _beforeisShow = IsShowButton();

        // ゲージ更新時の処理
        GameDataManager.AddOnUpdateEndlessLife(UpdateLifeView);
        _currentViewLife = SaveDataManager.GetEndlessLife();
        _waitViewLife = _currentViewLife;
        
        UpdateLifeView();
        SetViewLife(SaveDataManager.GetEndlessLife());

        _lifeEffectIcon1.localScale = Vector3.zero;
        _lifeEffectIcon2.localScale = Vector3.zero;
        _lifeUpSequence = null;
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

        // ボタンの表示チェック
        _isShow = IsShowButton();
        // ボタンの有効チェック
        _isActive = IsActiveButton();
        if(_isShow)
        {
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

            // ライフが増えた演出
            if( _waitViewLife < life)
            {
                PlayAnimationAddLife(_currentViewLife, life);
                _waitViewLife = life;
            }
            else if(_waitViewLife != life)
            {
                SetViewLife(life);
                _waitViewLife = life;
                _lifeUpSequence.Kill();
                InitLifeEffect();
            }
        }
        else
        {
            this.transform.localScale = Vector3.zero;
        }
        if(!_beforeIsActive && _isActive)
            PlayAnimationIsActiveButton();
        else if( !_beforeisShow && _isShow )
            PlayAnimationIsActiveButton();

        _beforeIsActive = _isActive;
        _beforeisShow = _isShow;
    }
    // ライフが増えた演出
    private void PlayAnimationAddLife(int fromLife, int toLife)
    {
        _lifeUpSequence = DOTween.Sequence();
        Vector3 initScale = _lifeIcon.localScale;

        for(int i = fromLife + 1; i <= toLife; i++)
        {
            int viewLife = i;

            _lifeUpSequence.AppendCallback(()=>
            {
                InitLifeEffect();
            });
            _lifeUpSequence.Append(_lifeEffectIcon2.DORotate(Vector3.zero, 0.3f).SetEase(Ease.OutBack));
            // seq.Join(_lifeEffectIcon1.DOScale(1f * 1.2f, 0.3f).SetEase(Ease.OutBack));
            _lifeUpSequence.Join(_lifeEffectIcon2.DOScale(1f * 1.3f, 0.3f).SetEase(Ease.OutBack));
            _lifeUpSequence.AppendInterval(0.2f);
            _lifeUpSequence.Append(_lifeEffectIconParent.DOMove(_lifeEndPos.position, 0.9f).SetEase(Ease.InBack));
            _lifeUpSequence.Join(_lifeEffectIconParent.DOScale(Vector3.one * 0.45f, 0.9f).SetEase(Ease.InQuad));
            _lifeUpSequence.AppendCallback(()=>{ 
                SetViewLife(viewLife);
                _lifeEffectIconParent.localScale = Vector3.zero;
                // _lifeUpSequence.AppendCallback(PlayAnimationIsActiveButton);
                PlayAnimationIsActiveButton();
            });
            // seq.AppendInterval(0.1f);
            _lifeUpSequence.Append(_lifeIcon.DOScale(initScale * 1.8f, 0.2f).SetEase(Ease.OutBack).SetLink(_lifeIcon.gameObject));
            _lifeUpSequence.Append(_lifeIcon.DOScale(initScale, 0.2f).SetEase(Ease.OutBack).SetLink(_lifeIcon.gameObject));
        }
        _lifeUpSequence.SetLink(_lifeStartPos.gameObject, LinkBehaviour.CompleteAndKillOnDisable);
    }
    private void InitLifeEffect()
    {
        _lifeEffectIconParent.position = _lifeStartPos.position;
        _lifeEffectIcon2.eulerAngles = new Vector3(0, 0, 45);
        _lifeEffectIconParent.localScale = Vector3.one;
        _lifeEffectIcon1.localScale = Vector3.zero;
        _lifeEffectIcon2.localScale = Vector3.zero;
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

    // ボタンの表示非表示チェック
    private bool IsShowButton()
    {
        // ABテストで無効化されているなら表示しない
        if(PlayerPrefs.GetInt("UnlimitedMode_ON") == 0)
        {   
            _lifeEffectIconParent.gameObject.SetActive(false);
            return false;
        }
        else
        {
            _lifeEffectIconParent.gameObject.SetActive(true);
        }

        // デバッグ：エンドレスモード無制限
        if(GameDataManager.DebugEndlessUnLimit)
            return true;
        // エンドレスモードなら、開始待機中なら表示する
        if(GameDataManager.GameMode == GameMode.endlessBattle)
        {
            if(GameDataManager.GameState == GameState.startWait)
                return true;
            else
                return false;
        }

        // 表示するステージまで来てないなら表示しない
        if(SaveDataManager.GetCurrentStage() < GameDataManager.ShowEndlessBattleButtonStage)
            return false;
        // ライフを1以上得たことがあるか
        // if( SaveDataManager.GetIsFirstLife() <= 0)   
        //     return false;
        return true;
    }

    // ボタンの有効化チェック　
    private bool IsActiveButton()
    {
        // デバッグ：エンドレスモード無制限
        if(GameDataManager.DebugEndlessUnLimit)
            return true;
        
        // 非表示なら無効化
        if(!_isShow)
            return false;

        // エンドレスモードなら無条件で有効化
        if(GameDataManager.GameMode == GameMode.endlessBattle )
            return true;
        // ライフが1もないなら無効化
        if( SaveDataManager.GetEndlessLife() <= 0 )
            return false;
        return true;
    }
    private void SetViewLife(int life)
    {
        _currentViewLife = life;
        _endlessLife.text = life + "";
    }
}
