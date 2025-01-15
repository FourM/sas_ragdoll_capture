/// <summary>
/// 拡張性に優れるカスタムボタン
/// </summary>
using DG.Tweening;  
using UnityEngine;  
using UnityEngine.EventSystems;  
using UnityEngine.Events;

public class CustomButton : MonoBehaviour,  
    IPointerClickHandler,  
    IPointerDownHandler,  
    IPointerUpHandler,
    IPointerEnterHandler
{
    // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    // ---------- Public関数 -------------------------
    public UnityEvent onClick;  
    public UnityEvent onPointerUp;  
    public UnityEvent onPointerDown;  
    public UnityEvent onPointerEnter;  
    public bool isCommonAnimation = true;
    public bool IsEnable = true;

    public void Awake() {
        onClick = new UnityEvent();
        onPointerUp = new UnityEvent();
        onPointerDown = new UnityEvent();
        onPointerEnter = new UnityEvent();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(!IsEnable)
            return;
        onPointerEnter?.Invoke();
    }

    public void OnPointerClick(PointerEventData eventData)  
    {
        if(!IsEnable)
            return;
        onClick?.Invoke();  
    }

    public void OnPointerDown(PointerEventData eventData)  
    {
        if(!IsEnable)
            return;
        if(isCommonAnimation)
            CommonPointerDownAnimation();  
        onPointerDown?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)  
    {
        if(!IsEnable)
            return;
        if(isCommonAnimation)
            CommonPointerUpAnimation();
        onPointerUp?.Invoke();
    }
    public void CommonPointerDownAnimation()
    {
        if(!IsEnable)
            return;
        transform.DOScale(0.95f, 0.2f).SetEase(Ease.OutBack).SetLink(this.gameObject);  
    }
    public void CommonPointerUpAnimation()
    {
        if(!IsEnable)
            return;
        transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack).SetLink(this.gameObject);  
    }
    // ---------- Private関数 ------------------------
}
