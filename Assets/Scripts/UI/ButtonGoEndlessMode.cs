using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class ButtonGoEndlessMode : MonoBehaviour
{
    // ---------- 定数宣言 ----------------------------
    const float GUASE_UP_SPD = 1f;
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
    private bool _isLifeUpAnimationLock = false;
    private bool _isGuageAnimationLock = false;
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
                UpdateLifeView(false);
            }
        });

        ChangeIcon(GameDataManager.GameMode);
        _button.onClick.AddListener(()=>
        {
            if( GameDataManager.GameMode != GameMode.endlessBattle)
            {
                GameDataManager.InGameMainEvent.ChangeGameMode(GameMode.endlessBattle);
                _lifeUpSequence.Complete();
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
        GameDataManager.AddOnStageStart(()=>{ UpdateLifeView(false); });

        _isActive = IsActiveButton();
        _beforeisShow = IsShowButton();

        // ゲージ更新時の処理
        GameDataManager.AddOnUpdateEndlessLife(()=>{ UpdateLifeView(false); });
        _currentViewLife = SaveDataManager.GetEndlessLife();
        _waitViewLife = _currentViewLife;
        
        if( SaveDataManager.GetIsDirectFirstOpenEndlessBattle() == 0)
            SetViewLife(0);
        else
            SetViewLife(SaveDataManager.GetEndlessLife());
        UpdateLifeView(true);

        _lifeEffectIcon1.localScale = Vector3.zero;
        _lifeEffectIcon2.localScale = Vector3.zero;
        _lifeUpSequence = null;

        if( SaveDataManager.GetIsDirectFirstOpenEndlessBattle() == 0)
        {
            _endlessLifeGuage.gameObject.SetActive(false);
        }
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
    private void UpdateLifeView(bool isInitialize = false, int fromlife = -1, float fromGuage = -1f)
    {
        int life = fromlife;
        float guage = fromGuage;

        if(fromlife < 0)
            life = SaveDataManager.GetEndlessLife();
        if(fromGuage < 0f)
            guage = SaveDataManager.GetEndlessLifeGuage();

        // まだエンドレスボタン未表示時のゲーム開始時状態
        if( SaveDataManager.GetIsDirectFirstOpenEndlessBattle() == 0 && isInitialize)
        {
            guage = 0f;
            life = 0;
        }
        // 

        // ボタンの表示チェック
        _isShow = IsShowButton();
        // ボタンの有効チェック
        _isActive = IsActiveButton();
        if(_isShow)
        {
            // ライフが増えた演出
            if( _waitViewLife < life)
            {
                // Debug.Log("アニメーションロック：" + _isLifeUpAnimationLock + ", " + _isGuageAnimationLock);
                if(!_isLifeUpAnimationLock && !_isGuageAnimationLock)
                {
                    _isLifeUpAnimationLock = true;
                    _isGuageAnimationLock = true;
                    PlayAnimationAddLife(_currentViewLife, life);
                    // Debug.Log("_isLifeUpAnimationLock更新：" + _isLifeUpAnimationLock);
                    _waitViewLife = life;
                }
            }
            // ゲージだけ更新
            else if(_waitViewLife == life)
            {
                if(!_isGuageAnimationLock)
                {
                    _lifeUpSequence.Complete();
                    float value = SaveDataManager.GetEndlessLifeGuage();
                    float duration = value - _endlessLifeGuage.value;
                    TweenUpdateViewLife(_endlessLifeGuage.value, value, duration);
                    InitLifeEffect();
                    UpdateActiveButton(_isActive);
                }
            }
            else if(_waitViewLife != life)
            {
                _lifeUpSequence.Complete();
                SetViewLife(life);
                SetViewGuage(SaveDataManager.GetEndlessLifeGuage());
                _waitViewLife = life;
                InitLifeEffect();
                UpdateActiveButton(_isActive);
                _endlessLifeGuage.value = guage;
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
    // ボタンの有効無効の更新
    private void UpdateActiveButton(bool isActive)
    {
        _isActive = isActive;
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
    }

    // ライフが増えた演出　新演出
    private void PlayAnimationAddLife(int fromLife, int toLife)
    {
        _lifeUpSequence = DOTween.Sequence();
        Vector3 initScale = _lifeIcon.localScale;
        int i = 0;
        int getLifeNum = toLife - fromLife;
        List<Transform> listLifeIcon = new List<Transform>();

        if( SaveDataManager.GetIsDirectFirstOpenEndlessBattle() == 0)
            _lifeUpSequence.AppendInterval(0.7f);

        float ratio = 0.9f;
        float guageSpd = (1 - Mathf.Pow(ratio, (float)getLifeNum)) / (1f - ratio);   // 獲得ライフ数が多いとゲージの上昇速度が速くなる。上昇速度はライフ数の比例ではなく等比数列的（ratioが0.8なら、1なら1倍、2なら1.8倍、3なら2.44倍...）
        _endlessLifeGuage.gameObject.SetActive(true);
        for(i = fromLife; i <= toLife - 1; i++)
        {
            // ゲージ上昇アニメーション
            float duration = 1f;
            float guageFrom = 0f;
            float guageTo = 1f;
            int index = i;
            // 最初は、ゲージがいっぱいになるまでの時間 = MaX - 今のゲージ
            if(index == fromLife)
            {
                guageFrom = _endlessLifeGuage.value;
                guageTo = 1f;
            }
            else if(index == toLife)
            {
                guageFrom = 0f;
                guageTo = SaveDataManager.GetEndlessLifeGuage();
            }
            else
            {
                guageFrom = 0f;
                guageTo = 1f;
            }
            // Debug.Log("index:" + index + "fromLife:" + fromLife + ", toLife:" + toLife + ", guageFrom:" + guageFrom + ", guageTo:" + guageTo);

            Transform lifeIcon = _lifeEffectIconParent;
            Vector3 ramdomToPos = Vector3.zero;
            // ハートアイコン生成
            if(index != toLife)
            {
                if(index != fromLife)
                {
                    lifeIcon = Instantiate(_lifeEffectIconParent);
                    lifeIcon.parent = _lifeEffectIconParent.parent;
                }
                else
                {
                    _lifeEffectIcon1.localScale = Vector3.zero;
                    _lifeEffectIcon2.localScale = Vector3.one;
                    _lifeEffectIcon2.eulerAngles = Vector3.zero;
                }
                listLifeIcon.Add(lifeIcon);
                lifeIcon.position = _lifeStartPos.position;
                lifeIcon.localScale = Vector3.zero;
                float angle = Random.Range(0f, 360f);
                float away = Random.Range(50f, 100f);
                ramdomToPos = _lifeStartPos.position + new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0) * away;
            }

            duration = guageTo - guageFrom;
            duration /= guageSpd;

            // シーケンス設定
            _lifeUpSequence.Append(TweenUpdateViewLife(guageFrom, guageTo, duration, index));
            _lifeUpSequence.AppendCallback(()=>
            {
                _endlessLifeGuage.value = 0f;
                if(lifeIcon != null && index != toLife)
                {
                    lifeIcon.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).SetLink(lifeIcon.gameObject);
                    lifeIcon.DOMove(ramdomToPos, 0.7f).SetEase(Ease.OutCubic).SetLink(lifeIcon.gameObject);
                }
                if( toLife - 1 <= index)
                {
                    // 最後の「ゲージがいっぱいまで溜まる」演出が終わったらゲージのロックを解除する
                    _isGuageAnimationLock = false;
                    // Debug.Log("_isGuageAnimationLock更新：" + toLife + ", " + index);
                }
            });
            _lifeUpSequence.AppendInterval(0.1f);
        }
        for( i = 0; i < listLifeIcon.Count; i++)
        {
            int index = i;
            Transform lifeIcon = listLifeIcon[index];
            
            _lifeUpSequence.AppendCallback(()=>{ 

                lifeIcon.DOScale(Vector3.one * 0.45f, 0.6f).SetEase(Ease.InQuad).SetLink(lifeIcon.gameObject);
                lifeIcon.DOMove(_lifeEndPos.position, 0.6f).SetEase(Ease.InBack).SetLink(lifeIcon.gameObject)
                .OnComplete(()=>{
                    SetViewLife(fromLife + index + 1);
                    if(index == 0)
                        lifeIcon.localScale = Vector3.zero;
                    else
                    {
                        listLifeIcon.Remove(lifeIcon);
                        Destroy(lifeIcon.gameObject);
                    }
                    PlayAnimationIsActiveButton();

                    // 初登場演出完了
                    if(index == listLifeIcon.Count - 1)
                    {
                        _isLifeUpAnimationLock = false;
                        _isGuageAnimationLock = false;
                        // Debug.Log("_isLifeUpAnimationLock更新：" + _isLifeUpAnimationLock);
                        SaveDataManager.SetIsDirectFirstOpenEndlessBattle(1);
                    }
                });
            });
            _lifeUpSequence.AppendInterval(0.1f);
        }
    }

    // ライフが増えた演出　今までのバックアップ
    private void PlayAnimationAddLifeStandard(int fromLife, int toLife)
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
            _lifeUpSequence.Join(_lifeEffectIcon2.DOScale(1f * 1.3f, 0.3f).SetEase(Ease.OutBack));
            _lifeUpSequence.AppendInterval(0.2f);
            _lifeUpSequence.Append(_lifeEffectIconParent.DOMove(_lifeEndPos.position, 0.9f).SetEase(Ease.InBack));
            _lifeUpSequence.Join(_lifeEffectIconParent.DOScale(Vector3.one * 0.45f, 0.9f).SetEase(Ease.InQuad));
            _lifeUpSequence.AppendCallback(()=>{ 
                SetViewLife(viewLife);
                _lifeEffectIconParent.localScale = Vector3.zero;
                PlayAnimationIsActiveButton();
            });
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
        seq.Append(this.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).SetLink(this.gameObject));
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
        // Debug.Log("エンドレス開始：" + GameDataManager.ShowEndlessBattleButtonStage + ", " + SaveDataManager.GetCurrentStage());
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
        if( _currentViewLife <= 0 )
            return false;
        return true;
    }

    private void SetViewLife(int life)
    {
        _currentViewLife = life;
        _endlessLife.text = life + "";
        UpdateActiveButton(IsActiveButton());
    }
    private void SetViewGuage(float guage)
    {
        _endlessLifeGuage.value = guage;
    }
    private Tween TweenUpdateViewLife(float start, float to, float duration, int index = -1)
    {
        // Debug.Log("index:" + index + "start:" + start + ", to:" + to );
        duration /= GUASE_UP_SPD;
        return DOVirtual.Float(
            from     : start,//Tween開始時の値
            to       : to,//終了時の値
            duration : duration,//Tween時間
            //値が変わった時の処理
            onVirtualUpdate: (tweenValue) => {
                _endlessLifeGuage.value = tweenValue;
            }
        ).SetEase(Ease.Linear).SetLink(_endlessLifeGuage.gameObject);
    }
}
