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
    IPointerUpHandler  
{
    // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    // [SerializeField, Tooltip("hoge")] private int hoge = default;
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    // ---------- Public関数 -------------------------
    public UnityEvent onClick;  
    public UnityEvent onPointerUp;  
    public UnityEvent onPointerDown;  
    public bool isCommonAnimation = true;

    public void OnPointerClick(PointerEventData eventData)  
    {
        onClick?.Invoke();  
    }

    public void OnPointerDown(PointerEventData eventData)  
    {
        if(isCommonAnimation)
            CommonPointerDownAnimation();  
        onPointerDown?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)  
    {
        if(isCommonAnimation)
            CommonPointerUpAnimation();
        onPointerUp?.Invoke();
    }
    public void CommonPointerDownAnimation()
    {
        transform.DOScale(0.95f, 0.2f).SetEase(Ease.OutBack);  
    }
    public void CommonPointerUpAnimation()
    {
        transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);  
    }
    // ---------- Private関数 ------------------------
}
