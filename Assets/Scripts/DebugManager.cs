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
    [SerializeField, Tooltip("ステージ選択ボタンプレハブ")] private DebugStageSelectButton _stageSelectButtonPrefabs = default;
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("インゲームマネージャー")] private InGameManager _inGameManager = default;
    [SerializeField, Tooltip("インゲームUIマネージャー")] private InGameUIManager _inGameUIManager = default;
    [SerializeField, Tooltip("手のスキンマネージャー")] private HandSkinManager _handSkinManager = default;
    [SerializeField, Tooltip("ステージマネージャー")] private StageManager _stageManager = default;
    [SerializeField, Tooltip("ポップアップ閉じる")] private Transform _debugPanel = default;
    [SerializeField, Tooltip("閉じるボタン")] private Button _closeBack = default;
    [SerializeField, Tooltip("デバッグモード解放キーボタンリスト")] private List<Button> _showDebugKeyButtons = default;
    [SerializeField, Tooltip("isRecoveryテキスト")] private TextMeshProUGUI _textIsRecovery = default;
    [SerializeField, Tooltip("isLevelBundleテキスト")] private TextMeshProUGUI _textIsLevelBundle = default;
    [SerializeField, Tooltip("killShockStrengthテキスト")] private TextMeshProUGUI _textKillShockStrength = default;
    [SerializeField, Tooltip("killShockStrengthテキスト2")] private TextMeshProUGUI _textKillShockStrength2 = default;
    [SerializeField, Tooltip("GimmickKillテキスト")] private TextMeshProUGUI _textGimmickKill = default;

    [SerializeField, Tooltip("isRecoveryトグル")] private Toggle _toggleIsRecovery = default;
    [SerializeField, Tooltip("isLevelBundleトグル")] private Toggle _toggleIsLevelBundle = default;
    [SerializeField, Tooltip("killShockStrengthスライダー")] private Slider _sliderKillShockStrength = default;
    [SerializeField, Tooltip("isGimmickKillトグル")] private Toggle _toggleGimmickKill = default;
    [SerializeField, Tooltip("ステージ選択ボタンリスト親")] private RectTransform _stageSelectButtonContents = default;
    // [SerializeField, Tooltip("killShockStrengthテキスト")] private TextMeshProUGUI _textKillShockStrength = "";
    private List<int> _inputShowDebugKeyList = default;
    private List<int> _showDebugKeyList = default;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    private void Start(){
        // デバッグモード起動のパスワード処理初期化
        // _showDebugKeyList = new List<int>(){ 1, 7, 9, 3, 2, 5, 6, 7 };
        _showDebugKeyList = new List<int>(){ 1, 7, 9};
        _inputShowDebugKeyList = new List<int>();
        
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

        if(PlayerPrefs.GetInt("is_Recovery", 1) == 0)
            _toggleIsRecovery.isOn = false;
        else
            _toggleIsRecovery.isOn = true;
    
        if(PlayerPrefs.GetInt("level_Bundle", 1) == 0)
            _toggleIsLevelBundle.isOn = false;
        else
            _toggleIsLevelBundle.isOn = true;

        _sliderKillShockStrength.onValueChanged.AddListener(OnChangeValueSliderKillShockStrength);

        GameDataManager.UpdatekillShockStrength();

        if(PlayerPrefs.GetInt("gimmick_Kill", 1) == 0)
            _toggleGimmickKill.isOn = false;
        else
            _toggleGimmickKill.isOn = true;
        

        UpdateText();
        ResetStageSelectButtons();
    }

    private void OnEnable(){
        UpdateText();
    }

    // ---------- Public関数 ----------
    public void ChangeIsRecovery(bool is_Recovery)
    {
        int is_RecoveryInt = 0;
        if(is_Recovery)
            is_RecoveryInt = 1;

        PlayerPrefs.SetInt("is_Recovery", is_RecoveryInt);
        UpdateText();
        _inGameManager.UndoInGame();
    }
    public void ChangeIsLevelBundle(bool level_Bundle)
    {
        int level_BundleInt = 0;
        if(level_Bundle)
            level_BundleInt = 1;

        PlayerPrefs.SetInt("level_Bundle", level_BundleInt);
        _stageManager.ResetStageBundle();
        UpdateText();
        _inGameManager.UndoInGame();
        ResetStageSelectButtons();
    }
    public void ChangekillShockStrength()
    {
        int killShockStrength = PlayerPrefs.GetInt("killShockStrength", 0);
        killShockStrength ++;
        killShockStrength %= 2;

        PlayerPrefs.SetInt("killShockStrength", killShockStrength);
        GameDataManager.UpdatekillShockStrength();
        UpdateText();
    }
    // スライダー
    public void OnChangeValueSliderKillShockStrength(float value)
    {
        GameDataManager.SetKillShockStrength(value);
        _textKillShockStrength2.text = "killShockStrength:" + value.ToString("F1");
        UpdateText();
    }
    public void ChangeGimmickKill(bool gimmick_Kill)
    {
        int gimmick_KillInt = 0;
        if(gimmick_Kill)
            gimmick_KillInt = 1;
        // Debug.Log("gimmick_Kill:" + gimmick_Kill);

        PlayerPrefs.SetInt("gimmick_Kill", gimmick_KillInt);
        _stageManager.ResetStageBundle();
        UpdateText();
        _inGameManager.UndoInGame();
        ResetStageSelectButtons();
    }
    public void ChangeShowUI(bool isShowUI)
    {
        if(isShowUI)
            _inGameUIManager.ShowInGameUI();
        else
            _inGameUIManager.HideInGameUI();
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
        _textIsRecovery.text = "is_Recovery_" + PlayerPrefs.GetInt("is_Recovery", 1);
        _textIsLevelBundle.text = "Level_Bundle_" + PlayerPrefs.GetInt("level_Bundle", 1);
        _textKillShockStrength.text = "killShockStrength_" + PlayerPrefs.GetInt("killShockStrength", 0);
        _textIsLevelBundle.text = "level_Bundle_" + PlayerPrefs.GetInt("level_Bundle", 1);
        _textGimmickKill.text = "Gmmick_Kill_" + PlayerPrefs.GetInt("gimmick_Kill", 1);

        float value = GameDataManager.GetKillShockStrength();
        _textKillShockStrength2.text = "killShockStrength:" + value.ToString("F1");
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

            DebugStageSelectButton stageSelectButton = Instantiate(_stageSelectButtonPrefabs);
            stageSelectButton.transform.parent = _stageSelectButtonContents;
            stageSelectButton.transform.localScale = Vector3.one;

            // ギミックで倒す必要があることが有効になっているステージは赤文字にする
            if(stage.IsGimmickKill() && PlayerPrefs.GetInt("gimmick_Kill", 1) == 1)
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
