using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
using TMPro;

public class InGameUIManager : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("キャンバス")] private Canvas _canvas = default;
    [SerializeField, Tooltip("キャンバスグループ：メインモード")] private CanvasGroup _mainCanvasGroup = default;
    [SerializeField, Tooltip("キャンバスグループ：敵を倒した数")] private CanvasGroup _canvasGroupHumanKill = default;
    [SerializeField, Tooltip("キャンバススケーラー")] private CanvasScaler _canvasScaler = default;
    [SerializeField, Tooltip("やり直しボタン")] private Button _buttonUndo = default;
    [SerializeField, Tooltip("敵を倒した数")] private TextMeshProUGUI _humanKillNum = default;
    [SerializeField, Tooltip("照準")] private UIReticle _uiReticle = default;
    [SerializeField, Tooltip("エンドレスバトル：プレイヤーの進んだ位置")] private TextMeshProUGUI _score = default;
    [SerializeField, Tooltip("スコア背景")] private GameObject _storeBack = default;
    [SerializeField, Tooltip("ベストスコア")] private TextMeshProUGUI _bestScore = default;
    [SerializeField, Tooltip("「スタート」文字")] private TextMeshProUGUI _textContinue = default;
    [SerializeField, Tooltip("残りライフ")] private GameObject _lifeView = default;
    [SerializeField, Tooltip("ライフ文字")] private TextMeshProUGUI _textLife = default;
    [SerializeField, Tooltip("リザルトUI")] private EndlessBattleResultUIManager _endlessBattleResultUI = default;
    private bool _isInitialize = false;
    private UnityEvent _onInitialize = null;
    private UnityEvent _onHideUI = null;
    private UnityEvent _onShowUI = null;
    private float _posFix = 1.0f;
    private Vector2 _reticlePosShiftFix = default;
    private Sequence _humanKillNumSeq = null;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    // ---------- Public関数 ----------
    public void Initialize(){
        if(_isInitialize) return;
            _isInitialize = true;

        _posFix = _canvasScaler.referenceResolution.y / Screen.height;
        _reticlePosShiftFix = new Vector2(-Screen.width / 2, -Screen.height / 2);

        _uiReticle.Initialize();
        SetReticlePos(new Vector2(Screen.width / 2, Screen.height / 2));
        UpdateReticleActive();

        _onInitialize?.Invoke();
        _onInitialize?.RemoveAllListeners();

        ChangeGameMode( GameDataManager.GameMode );

        _endlessBattleResultUI.Initialize();

        GameDataManager.AddOnChangeGameState((GameState state)=>{
            if(state == GameState.main)
            {
                _textContinue.gameObject.SetActive(false);
                _lifeView.gameObject.SetActive(false);
            }
        });
        _textContinue.transform.DOScale(1.05f, 1f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetLink(_textContinue.gameObject);

        GameDataManager.AddOnUpdateEndlessLife(UpdateHumanKillView);

        _humanKillNum.text = SaveDataManager.GetHumanKillNum().ToString("000");

        GameDataManager.AddOnDebugChangeUserSegment((string value)=>
        {
            if(value == "UnlimitedMode_ON")
            {
                _canvasGroupHumanKill.alpha = (float)PlayerPrefs.GetInt("UnlimitedMode_ON");
            }
        });
        _canvasGroupHumanKill.alpha = (float)PlayerPrefs.GetInt("UnlimitedMode_ON");
    }

    public void ChangeGameMode(GameMode gameMode)
    {
        switch(gameMode)
        {
            case GameMode.main:
                _mainCanvasGroup.alpha = 1;
                _score.gameObject.SetActive(false);
                _storeBack.gameObject.SetActive(false);
                _bestScore.gameObject.SetActive(false);
                _textContinue.gameObject.SetActive(false);
                _lifeView.gameObject.SetActive(false);
                _buttonUndo.enabled = true;
                break;
            case GameMode.endlessBattle:
                _buttonUndo.enabled = false;
                _mainCanvasGroup.alpha = 0;
                _score.gameObject.SetActive(true);
                _storeBack.gameObject.SetActive(true);
                _bestScore.gameObject.SetActive(true);
                _textContinue.gameObject.SetActive(true);
                _lifeView.gameObject.SetActive(true);
                _textLife.text = SaveDataManager.GetEndlessLife() + "";
                _score.text = Mathf.Round(GameDataManager.GetPlayerMoveLength()) + "m";
                _bestScore.text = "BEST:" + Mathf.Round(SaveDataManager.GetEndlessBattleBestScore()) + "m";
                break; 
        }
    }

    public void SetOnClickButtonUndo( UnityAction onClick )
    {
        _buttonUndo.onClick.AddListener(onClick);
    }

    public void ShowInGameUI(){
        if(GameDataManager.DebugIsShowUi())
        {   
            ShowButtonUndo();
        }   
        _onShowUI?.Invoke();
    }
    public void HideInGameUI(){
        HideButtonUndo();
        _onHideUI?.Invoke();
    }

    public void SetReticlePos( Vector2 mousePos )
    {
        Vector2 pos = mousePos;

        pos.x = (pos.x + _reticlePosShiftFix.x) * _posFix;
        pos.y = (pos.y + _reticlePosShiftFix.y) * _posFix;

        _uiReticle.SetPos(pos);
    }
    public void OnClick( Vector2 mousePos )
    {
        _uiReticle.OnClick(mousePos);
    }
    public void OnMouseUp()
    {
        _uiReticle.OnMouseUp();
    }
    public void SetIsCatch(bool isCatch)
    {
        _uiReticle.SetIsCatch(isCatch);
    }
    public void UpdateReticleActive()
    {
        int aimOn = PlayerPrefs.GetInt("Aim_ON");
        if(aimOn == 0)
            _uiReticle.gameObject.SetActive(false);
        else if(aimOn == 1)
            _uiReticle.gameObject.SetActive(true);
        else
            Debug.LogError("Aim_ON が0と1以外です！:" + aimOn);
    }
    // 初期化時イベント設定
    public void AddOnInitialize( UnityAction onInitialize)
    {
        if(_onInitialize == null)
            _onInitialize = new UnityEvent();
        _onInitialize.AddListener(onInitialize);
    }
    
    public void AddOnShowUI( UnityAction onShow)
    {
        if(_onShowUI == null)
            _onShowUI = new UnityEvent();
        _onShowUI.AddListener(onShow);
    }
    public void AddOnHideUI( UnityAction onHide)
    {
        if(_onHideUI == null)
            _onHideUI = new UnityEvent();
        _onHideUI.AddListener(onHide);
    }
    // プレイヤーの移動距離表示設定
    public void SetTextPlayerMoveLength(float length)
    {
        _score.text = Mathf.Round(length) + "m";
        _endlessBattleResultUI.SetTextPlayerMoveLength(length);
    }

    public void ShowResult(bool isNewRecord)
    {
        _score.gameObject.SetActive(false);
        _storeBack.gameObject.SetActive(false);
        _bestScore.gameObject.SetActive(false);
        _endlessBattleResultUI.ShowResult(isNewRecord);
    }
    // ---------- Private関数 ----------
    private void ShowButtonUndo()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_buttonUndo.transform.DOScale(Vector3.one, 0.07f).SetEase(Ease.OutBack));
        sequence.AppendCallback(()=>
        {
            _buttonUndo.enabled = true;
        });
    }
    private void HideButtonUndo()
    {
        _buttonUndo.enabled = false;
        // _buttonUndo.transform.localScale = Vector3.zero;
        _buttonUndo.transform.DOScale(Vector3.zero, 0.07f).SetEase(Ease.InBack);
    }

    private void UpdateHumanKillView()
    {
        if(_humanKillNumSeq == null)
        {
            _humanKillNumSeq = DOTween.Sequence();
            _humanKillNumSeq.Append(_humanKillNum.transform.DOScale(Vector3.one * 1.1f, 0.2f).SetEase(Ease.InOutBack).SetLink(_humanKillNum.gameObject));
            _humanKillNumSeq.Append(_humanKillNum.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack).SetLink(_humanKillNum.gameObject));
            _humanKillNumSeq
                .Pause()
                .SetAutoKill(false)
                .SetLink(_humanKillNum.gameObject);
        }
        _humanKillNum.text = SaveDataManager.GetHumanKillNum().ToString("000");
        _humanKillNumSeq.Restart();
    }
}
