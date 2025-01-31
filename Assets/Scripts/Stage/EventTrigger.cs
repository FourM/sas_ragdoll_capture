using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventTrigger : MonoBehaviour, IEventTrigger
{
    // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    [SerializeField] private InterfaceReference<IInitializer> _iInitializer;
    [field: SerializeField] public EventTriggerBase eventTriggerBase{ get; set; }
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    private void Awake()
    {
        this.eventTriggerBase.Init(this, this);
        _iInitializer.Value.AddOnInitialize(Initialize);
    }
    // ---------- Public関数 -------------------------
    // ---------- Private関数 ------------------------
    private void Initialize()
    {
        eventTriggerBase.OnEventTrigger?.Invoke();
    }
}
