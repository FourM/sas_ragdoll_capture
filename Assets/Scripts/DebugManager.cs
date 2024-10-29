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
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("インゲームマネージャー")] private InGameManager _inGameManager = default;
    [SerializeField, Tooltip("手のスキンマネージャー")] private HandSkinManager _handSkinManager = default;
    [SerializeField, Tooltip("ステージマネージャー")] private StageManager _stageManager = default;
    [SerializeField, Tooltip("ポップアップ閉じる")] private Transform _debugPanel = default;
    [SerializeField, Tooltip("閉じるボタン")] private Button _closeBack = default;
    [SerializeField, Tooltip("デバッグモード解放キーボタンリスト")] private List<Button> _showDebugKeyButtons = default;
    [SerializeField, Tooltip("isBananaManテキスト")] private TextMeshProUGUI _textIsBananaMan = default;
    [SerializeField, Tooltip("isSpiderManテキスト")] private TextMeshProUGUI _textIsSpiderSkin = default;
    [SerializeField, Tooltip("killShockStrengthテキスト")] private TextMeshProUGUI _textKillShockStrength = default;
    [SerializeField, Tooltip("killShockStrengthテキスト2")] private TextMeshProUGUI _textKillShockStrength2 = default;

    [SerializeField, Tooltip("isBananaManトグル")] private Toggle _toggleIsBananaMan = default;
    [SerializeField, Tooltip("isSpiderManトグル")] private Toggle _toggleIsSpiderSkin = default;
     [SerializeField, Tooltip("killShockStrengthスライダー")] private Slider _sliderKillShockStrength = default;
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
            _toggleIsBananaMan.isOn = false;
        else
            _toggleIsBananaMan.isOn = true;
    
        if(PlayerPrefs.GetInt("level_Bundle", 1) == 0)
            _toggleIsSpiderSkin.isOn = false;
        else
            _toggleIsSpiderSkin.isOn = true;

        _sliderKillShockStrength.onValueChanged.AddListener(OnChangeValueSliderKillShockStrength);

        GameDataManager.UpdatekillShockStrength();
        UpdateText();
    }

    private void OnEnable(){
        UpdateText();
    }

    // ---------- Public関数 ----------
    public void ChangeIsBananaMan(bool is_Recovery)
    {
        int is_RecoveryInt = 0;
        if(is_Recovery)
            is_RecoveryInt = 1;

        PlayerPrefs.SetInt("is_Recovery", is_RecoveryInt);
        UpdateText();
        _inGameManager.UndoInGame();
    }
    public void ChangeIsSpiderSkin(bool level_Bundle)
    {
        int level_BundleInt = 0;
        if(level_Bundle)
            level_BundleInt = 1;

        PlayerPrefs.SetInt("level_Bundle", level_BundleInt); PlayerPrefs.SetInt("level_Bundle", level_BundleInt);
        _stageManager.ResetStageBundle();
        UpdateText();
        _inGameManager.UndoInGame();
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
    // ---------- Private関数 ----------
    private void UpdateText()
    {
        _textIsBananaMan.text = "is_Recovery" + PlayerPrefs.GetInt("is_Recovery", 1);
        _textIsSpiderSkin.text = "level_Bundle_" + PlayerPrefs.GetInt("level_Bundle", 1);
        _textKillShockStrength.text = "killShockStrength_" + PlayerPrefs.GetInt("killShockStrength", 0);

        float value = GameDataManager.GetKillShockStrength();
        _textKillShockStrength2.text = "killShockStrength:" + value.ToString("F1");
    }
}
