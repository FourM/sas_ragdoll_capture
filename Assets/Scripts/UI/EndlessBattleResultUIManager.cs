using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class EndlessBattleResultUIManager : MonoBehaviour
{
    // ---------- 定数宣言 ----------------------------
    private enum ResultState
    {
        hide,
        animation,
        continueWait
    }
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    [SerializeField, Tooltip("Finish!文字")] private RectTransform _textFinish = default;
    [SerializeField, Tooltip("黒背景大")] private CanvasGroup _blackBackBig = default;
    [SerializeField, Tooltip("黒背景小")] private CanvasGroup _blackBackSmall = default;
    [SerializeField, Tooltip("スコア")] private TextMeshProUGUI _score = default;
    [SerializeField, Tooltip("「続ける」文字")] private TextMeshProUGUI _textContinue = default;
    [SerializeField, Tooltip("続けるボタン")] private Button _continueButton = default;
    [SerializeField, Tooltip("新記録！")] private TextMeshProUGUI _textNewRecord = default;
    private Sequence _resultSeq = null;
    private Sequence _continueWaitSeq = null;
    private ResultState _state = ResultState.hide;
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    public void Initialize(){
        _state = ResultState.hide;
        InitView();
    }

    private void Update()
    {
        if(_state == ResultState.animation)
        {
            if(Input.GetMouseButtonDown(0))
            {
                _resultSeq.Kill(true);
            }
        }
    }
    // ---------- Public関数 -------------------------
    public void ShowResult(bool isNewRecord)
    {
        if(_resultSeq != null)
        {
            _resultSeq.Kill();
            _resultSeq = null;
        }
        if(_continueWaitSeq != null)
        {
            _continueWaitSeq.Kill();
            _continueWaitSeq = null;
        }
        InitView();
        _blackBackBig.alpha = 0f;
        _blackBackBig.gameObject.SetActive(true);
        _textFinish.gameObject.SetActive(true);
        _blackBackSmall.gameObject.SetActive(true);
        _textContinue.gameObject.SetActive(true);
        _score.gameObject.SetActive(true);
        _blackBackSmall.transform.localScale = Vector3.zero;
        _textFinish.localScale = Vector3.zero;
        _textFinish.localPosition = Vector3.zero;
        _textContinue.transform.localScale = Vector3.zero;
        _score.transform.localScale = Vector3.zero;
        _state = ResultState.animation;
        _textNewRecord.transform.localScale = Vector3.zero;

        float finishMoveY = Screen.height * 0.15f;

        _resultSeq = DOTween.Sequence();
        // 全体に黒背景表示
        _resultSeq.Append(_blackBackBig.DOFade(1, 0.1f).SetEase(Ease.Linear));
        // Finish文字表示
        _resultSeq.Append(_textFinish.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack));
        _resultSeq.AppendInterval(0.4f);
        // スコア表示用の黒帯演出
        _resultSeq.AppendCallback(()=>{
            _blackBackSmall.transform.localScale = new Vector3(0.0f, 0.02f, 1f);
        });
        _resultSeq.Append(_blackBackSmall.transform.DOScale(new Vector3(1f, 0.02f, 1f), 0.08f));
        _resultSeq.AppendInterval(0.2f);
        _resultSeq.Append(_blackBackSmall.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.Linear));
        // スコア表示
        _resultSeq.Join(_score.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack));
        _resultSeq.Join(_textFinish.DOAnchorPos(new Vector2(0, finishMoveY), 0.3f).SetEase(Ease.OutBack));
        // 新記録時の追加演出
        if(isNewRecord)
        {
            _resultSeq.AppendCallback(()=>{
                SeqToContinueWait();
                _state = ResultState.continueWait;
            });
            _resultSeq.AppendInterval(0.2f);
            _resultSeq.AppendCallback(()=>{
                _textNewRecord.transform.localScale = Vector3.one * 3f;
            });
            _resultSeq.Append(_textNewRecord.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack));
        }
        // しばらくしてから「タッチして続ける」文字を出す
        _resultSeq.OnComplete(()=>
        {
            SeqToContinueWait();
            _state = ResultState.continueWait;
        });
    }
    public void SetTextPlayerMoveLength(float length)
    {
        _score.text = Mathf.Round(length) + "m";
    }
    // ---------- Private関数 ------------------------
    // タッチして続ける待機状態へ移行
    private void SeqToContinueWait()
    {
        // すでにタッチ待機状態なら無視する
        if(_state == ResultState.continueWait)
            return;
        if(_continueWaitSeq != null)
        {
            _continueWaitSeq.Kill();
            _continueWaitSeq = null;
        }
        _continueWaitSeq = DOTween.Sequence();
        _continueWaitSeq.AppendInterval(0.7f);
        _continueWaitSeq.AppendCallback(()=>{
            _continueButton.gameObject.SetActive(true);
        });
        _continueWaitSeq.Append(_textContinue.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack));
        _continueWaitSeq.Append(_textContinue.transform.DOScale(Vector3.one * 0.95f, 1f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo));
    }
    private void InitView()
    {
        _textFinish.gameObject.SetActive(false);
        _blackBackBig.gameObject.SetActive(false);
        _blackBackSmall.gameObject.SetActive(false);
        _score.gameObject.SetActive(false);
        _textContinue.gameObject.SetActive(false);
        _continueButton.gameObject.SetActive(false);
        _continueButton.onClick.AddListener(Continue);
        _textNewRecord.transform.localScale = Vector3.zero;
    }
    private void Continue()
    {
        _state = ResultState.hide;
        InitView();
        _continueButton.gameObject.SetActive(false);
        // ライフがなければメインゲームモードに戻る
        if( SaveDataManager.GetEndlessLife() <= 0 )
        {
            GameDataManager.InGameMainEvent.ChangeGameMode(GameMode.main);
        }
        GameDataManager.InGameMainEvent.OnUndoInGame();
    }
}
