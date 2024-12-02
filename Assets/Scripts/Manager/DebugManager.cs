using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;
using TMPro;


[DefaultExecutionOrder(-10)]
public class DebugManager : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    [SerializeField, Tooltip("ステージ選択ボタンプレハブ")] private DebugStageSelectButton _stageSelectButtonPrefab = default;
    [SerializeField, Tooltip("ステージ選択ボタンプレハブ")] private DebugABTestButton _abTestButtonPrefab = default;
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("インゲームマネージャー")] private InGameManager _inGameManager = default;
    [SerializeField, Tooltip("インゲームUIマネージャー")] private InGameUIManager _inGameUIManager = default;
    [SerializeField, Tooltip("手のスキンマネージャー")] private HandSkinManager _handSkinManager = default;
    [SerializeField, Tooltip("ステージマネージャー")] private StageManager _stageManager = default;
    [SerializeField, Tooltip("ユーザープロパティ")] private UserSegment _userSegment = default;
    [SerializeField, Tooltip("ポップアップ閉じる")] private Transform _debugPanel = default;
    [SerializeField, Tooltip("閉じるボタン")] private Button _closeBack = default;
    [SerializeField, Tooltip("デバッグモード解放キーボタンリスト")] private List<Button> _showDebugKeyButtons = default;
    [SerializeField, Tooltip("ABテストボタンリスト親")] private RectTransform _abTestButtonContents = default;
    [SerializeField, Tooltip("killShockStrengthテキスト2")] private TextMeshProUGUI _textKillShockStrength2 = default;
    [SerializeField, Tooltip("killShockStrengthスライダー")] private Slider _sliderKillShockStrength = default;
    [SerializeField, Tooltip("ステージ選択ボタンリスト親")] private RectTransform _stageSelectButtonContents = default;
    [SerializeField, Tooltip("インステ広告費")] private TextMeshProUGUI _TextAdsManagerRevenue = default;
    [SerializeField, Tooltip("バナー広告費")] private TextMeshProUGUI _TextBannerRevenue = default;
    [SerializeField, Tooltip("バナー")] private StageBanner _banner = default;
    [SerializeField, Tooltip("広告マネージャー")] private AdsManager _adsManager = default;
    [SerializeField, Tooltip("バナー")] private List<int> _showDebugKeyList = default;
    private List<int> _inputShowDebugKeyList = default;
    // private List<int> _showDebugKeyList = default;
    private SerializedDictionary<List<int>> _userPropertyDic;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    private void Awake(){
        _inGameManager.AddOnInitialize(Initialize);
    }
    private void Initialize(){
        // デバッグモード起動のパスワード処理初期化
        // _showDebugKeyList = new List<int>(){ 1, 7, 9, 3, 2, 5, 6, 7 };
        // _showDebugKeyList = new List<int>(){ 1, 7, 9};
        // _showDebugKeyList2 = new List<int>(){ 1, 1, 1};
        _inputShowDebugKeyList = new List<int>();

        _userPropertyDic = _userSegment.DebugGetUserPropertyDictionary();
        
        // デバッグモード起動ボタン初期化
        for(int i = 0; i < _showDebugKeyButtons.Count; i++)
        {
            int keyNum = i;
            // キー入力時の処理
            _showDebugKeyButtons[keyNum].onClick.AddListener(()=>{
                // 記憶してる入力キーの数が、パスワードの文字数と同じになったら、一番古い記憶を消す
                if(_showDebugKeyList.Count <= _inputShowDebugKeyList.Count )
                    _inputShowDebugKeyList.RemoveAt(0);
                _inputShowDebugKeyList.Add(keyNum + 1);

                // 入力キーと表示キーが一致しているかチェック
                if(_inputShowDebugKeyList.Count == _showDebugKeyList.Count)
                {
                    bool _isMatch = true;
                    for(int i = 0; i < _inputShowDebugKeyList.Count; i++)
                    {
                        if(_inputShowDebugKeyList[i] != _showDebugKeyList[i])
                        {
                            _isMatch = false;
                            break;
                        }
                    }
                    // デバッグ画面表示
                    if(_isMatch)
                    {
                        _debugPanel.gameObject.SetActive(true);
                    }
                }
            });
        }

        // 閉じるボタン処理
        _closeBack.onClick.AddListener(()=>
        {
            _debugPanel.gameObject.SetActive(false);   
        });
        _debugPanel.gameObject.SetActive(false);


        // ABテストボタン初期化
        foreach (Transform child in _abTestButtonContents)
        {
            Destroy(child.gameObject);
        }
        _userPropertyDic = _userSegment.DebugGetUserPropertyDictionary();
        // Debug.Log("初期化!:" + _userPropertyDic.Keys.Count);
        foreach( string key in _userPropertyDic.Keys )
        {
            List<int> paramList = _userPropertyDic[key];
            if( 2 <= paramList.Count )
            {
                DebugABTestButton button = Instantiate(_abTestButtonPrefab);
                button.transform.parent = _abTestButtonContents;
                button.Initialize(key, _userPropertyDic[key], ()=>{
                    // ユーザープロパティ更新後の処理
                    _stageManager.ResetStageBundle();
                    ResetStageSelectButtons();
                    _inGameManager.UndoInGame();
                    GameDataManager.UpdatekillShockStrength();
                    UpdateText();
                });
            }
        }

        _sliderKillShockStrength.onValueChanged.AddListener(OnChangeValueSliderKillShockStrength);
        GameDataManager.UpdatekillShockStrength();

        _banner.AddOnLoadedCallback(()=>
        {
            UpdateText();
        });
        _adsManager.AddOnLoadedCallback(()=>{
            UpdateText();
        });

        UpdateText();
        ResetStageSelectButtons();
    }

    private void OnEnable(){
        UpdateText();
    }

    // ---------- Public関数 ----------
    // スライダー
    private void OnChangeValueSliderKillShockStrength(float value)
    {
        GameDataManager.SetKillShockStrength(value);
        // _textKillShockStrength2.text = "killShockStrength:" + value.ToString("F1");
        UpdateText();
    }
    public void ChangeShowUI(bool isShowUI)
    {
        GameDataManager.SetDebugIsShowUi(isShowUI);
        if(isShowUI)
        {
            _inGameUIManager.ShowInGameUI();
            _banner.showads();
        }
        else
        {
            _inGameUIManager.HideInGameUI();
            _banner.DebugHideAds();
        }
    }
    public void ChangeDebugStageLoop(bool isStageLoop)
    {
        _inGameManager.SetDebugStageLoop(isStageLoop);
    }
    public void ChangeDebugEnebleInste(bool enebleInste)
    {
        _inGameManager.SetDebugEnebleInste(enebleInste);
    }

    public void OnClickPrevStage()
    {
        int currentStageNum = SaveDataManager.GetCurrentStage();
        currentStageNum--;
        // ステージ番号が0を下回らないようにする
        if(currentStageNum < 0)
        {
            currentStageNum = _stageManager.GetEnableGameStageList().Count - 1;
            Debug.Log("おおっと");
        }
        SaveDataManager.SetCurrentStage(currentStageNum);
        _inGameManager.UndoInGame();
    }
    public void OnClickNextStage()
    {
        int currentStageNum = SaveDataManager.GetCurrentStage();
        currentStageNum++;
        SaveDataManager.SetCurrentStage(currentStageNum);
        _inGameManager.UndoInGame();
    }
    // ---------- Private関数 ----------
    private void UpdateText()
    {
        float value = GameDataManager.GetKillShockStrength();
        _textKillShockStrength2.text = "killShockStrength:" + value.ToString("F1");
        _sliderKillShockStrength.value = value;

        _inGameUIManager.UpdateReticleActive();

        // double revenue = _adsManager.GetAdRevenue();
        // _TextAdsManagerRevenue.text = "Ads revenue:" + revenue;
        // revenue = _banner.GetAdRevenue();
        // _TextBannerRevenue.text = "Banner revenue:" + revenue;
    }

    private void ResetStageSelectButtons()
    {
        foreach (Transform child in _stageSelectButtonContents)
        {
            Destroy(child.gameObject);
        }
        List<GameStage> stageList = _stageManager.GetEnableGameStageList();
        for(int i = 0; i < stageList.Count; i++)
        {
            int index = i;
            GameStage stage = stageList[index];

            DebugStageSelectButton stageSelectButton = Instantiate(_stageSelectButtonPrefab);
            stageSelectButton.transform.parent = _stageSelectButtonContents;
            stageSelectButton.transform.localScale = Vector3.one;

            // ギミックで倒す必要があることが有効になっているステージは赤文字にする
            if(stage.IsGimmickKill() && PlayerPrefs.GetInt("Gimmick_Kill", 1) == 1)
                stageSelectButton.SetTextColor(Color.red);

            stageSelectButton.SetStageNo(index);
            stageSelectButton.SetStageId(stage.GetStageId());
            stageSelectButton.SetOuButton(()=>
            {
                int currentStageNum = stageSelectButton.GetStageNo();
                SaveDataManager.SetCurrentStage(currentStageNum);
                _inGameManager.UndoInGame();
            });
        }
    }
}
