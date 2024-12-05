using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;

/// <summary>
/// 照準UI
/// </summary>
public class UIReticle : MonoBehaviour
{
    // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    [SerializeField, Tooltip("照準の位置")] private RectTransform _reticleRect = default;
    [SerializeField, Tooltip("照準の全体の透明度")] private CanvasGroup _reticleAlpha = default;
    [SerializeField, Tooltip("照準の中心の透明度")] private CanvasGroup _reticleCenterAlpha = default;
    [SerializeField, Tooltip("照準の外周の透明度")] private CanvasGroup _reticleAroundAlpha = default;
    [SerializeField, Tooltip("照準の中心の画像")] private Image _reticleCenterImage = default;
    [SerializeField, Tooltip("照準の外周の画像")] private Image _reticleAroundImage = default;
    private Sequence _releaseSequence = null;
    private List<Tween> _releaseTweens = null;
    private List<Tween> _tweens = null;
    private List<Tween> _colorTweens = null;
    private bool _isCatch = false;
    private bool _isCompReleaseAnimation = false;
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    // ---------- Public関数 -------------------------
    public void Initialize(){
        _tweens = new List<Tween>();
        _colorTweens = new List<Tween>();
        _releaseTweens = new List<Tween>();
        Tween tween = null;

        // SetIsCatch(true);
        SetIsCatch(false);
        // _reticleRect.sizeDelta = new Vector2(Screen.width, Screen.height);
        float posX = Screen.width / 2;
        float posY = Screen.height / 2;
        OnClick(new Vector2(posX, posY));
        tween = DOVirtual.DelayedCall(0.5f, ()=>{ SetIsCatch(false); });
        _colorTweens.Add(tween);
    }
    public void SetPos(Vector2 pos)
    {
        _reticleRect.anchoredPosition = pos;
    }
    public void OnClick(Vector2 pos)
    {
        ResetReleaseSeuence();
        ResetTapSeuence();
        _releaseSequence = DOTween.Sequence();
        SetPos(pos);
        Tween tween = null;
        float duration = 0.28f;

        _reticleCenterAlpha.alpha = 1f;
        _reticleAroundAlpha.alpha = 0f;
        // _reticleRect.localScale = Vector3.one * 2f;
        _reticleAroundAlpha.transform.localScale = Vector3.one * 2f;
        _reticleCenterAlpha.transform.localScale = Vector3.one * 2f;

        tween = DOVirtual.Float(
        from     : 0.0f,//Tween開始時の値
        to       : 1.0f,//終了時の値
        duration : duration,//Tween時間
        //値が変わった時の処理
        onVirtualUpdate: (tweenValue) => {
            _reticleAroundAlpha.alpha = tweenValue;
        });
        _tweens.Add(tween);
        tween = _reticleAroundAlpha.transform.DOScale(Vector3.one * 0.8f, duration).SetEase(Ease.OutBack);
        _tweens.Add(tween);
        tween = _reticleCenterAlpha.transform.DOScale(Vector3.one * 0.8f, duration).SetEase(Ease.OutBack);
        _tweens.Add(tween);

        // Debug.Log("画面タップ");

        _isCompReleaseAnimation = false;
    }
    public void SetIsCatch(bool isCatch)
    {
        float duration = 0.28f;
        if(isCatch == _isCatch)
            return;
        _isCatch = isCatch;
        ResetColorTweens();
        Tween tween = null;
        // Debug.Log("捕まえた？:" + isCatch);
        if(isCatch)
        {
            tween = _reticleCenterImage.DOColor( new Color32(255, 0, 0, 255), 0.001f );
            _colorTweens.Add(tween);
            tween = _reticleAroundImage.DOColor( new Color32(255, 255, 0, 255), 0.001f );
            _colorTweens.Add(tween);
            tween = _reticleRect.DOScale(Vector3.one * 0.75f, duration).SetEase(Ease.OutBack);
            _colorTweens.Add(tween);
            _reticleAroundImage.transform.localEulerAngles = new Vector3(0, 0, -30);
            tween = _reticleAroundImage.transform.DORotate( new Vector3(0, 0, 90), duration + 0.12f, RotateMode.Fast ).SetEase(Ease.OutBack);
            _colorTweens.Add(tween);
        }
        else
        {
            _reticleAroundImage.transform.localEulerAngles = new Vector3(0, 0, 0);
            tween = _reticleCenterImage.DOColor( new Color32(192, 192, 192, 228), duration );
            _colorTweens.Add(tween);
            tween = _reticleAroundImage.DOColor( new Color32(255, 255, 255, 228), duration );
            _colorTweens.Add(tween);
            tween = _reticleRect.DOScale(Vector3.one,duration).SetEase(Ease.OutBack);
            _colorTweens.Add(tween);
        }
    }
    public void OnMouseUp()
    {
        if(_isCompReleaseAnimation)
            return;
        _isCompReleaseAnimation = true;

        Tween tween = null;
        ResetReleaseSeuence();
        _releaseSequence = DOTween.Sequence();
        _releaseSequence.AppendInterval(0.4f);
        _releaseSequence.AppendCallback(()=>{
            ResetTapSeuence();
            SetIsCatch(false);
            tween = DOVirtual.Float(
            from     : 1.0f,//Tween開始時の値
            to       : 0.0f,//終了時の値
            duration : 0.3f,//Tween時間
            //値が変わった時の処理
            onVirtualUpdate: (tweenValue) => {
                _reticleCenterAlpha.alpha = tweenValue;
                _reticleAroundAlpha.alpha = tweenValue;
            });
            _releaseTweens.Add(tween);
        });
        _releaseSequence.Append(_reticleAroundAlpha.transform.DOScale(Vector3.one * 3f, 0.3f).SetEase(Ease.InBack));

        // Debug.Log("マウスアップ");
    }
    // ---------- Private関数 ------------------------
    private void ResetReleaseSeuence()
    {
        if(_releaseSequence != null)
        {
            _releaseSequence.Kill();
            _releaseSequence = null;
        }

        for(int i = _releaseTweens.Count - 1; 0 <= i; i--)
        {
            if(_releaseTweens[i] != null)
            {
                _releaseTweens[i].Kill();
                _releaseTweens.RemoveAt(i);
            }
        }
    }
    private void ResetTapSeuence()
    {
        for(int i = _tweens.Count - 1; 0 <= i; i--)
        {
            _tweens[i].Kill();
            _tweens.RemoveAt(i);
        }
    }
    private void ResetColorTweens()
    {
        for(int i = _colorTweens.Count - 1; 0 <= i; i--)
        {
            _colorTweens[i].Kill();
            _colorTweens.RemoveAt(i);
        }
    }
}
