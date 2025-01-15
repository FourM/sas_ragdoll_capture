using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class PlayerPassThroughTriggerEvent : MonoBehaviour
{
    [SerializeField, Tooltip("プレイヤーが通過したらイベントを起こすパス")] private EndlessBattlePath _triggerPath = default;
    protected bool _isTrigger = false;

    private void Awake()
    {
        AwakeUnique();
    } 
    private void Start() 
    { 
        _triggerPath.AddOnPassCallback(()=>{
            _isTrigger = true;
            OnTriggerUnique();
        });
        InitializeUnique();
    }
    private void Update() 
    {   
        UpdateUnique();
    }
    private void FixedUpdate() 
    {   
        FixedUpdateUnique();
    }

    protected virtual void AwakeUnique(){  }
    protected virtual void InitializeUnique(){  }
    protected virtual void OnTriggerUnique(){  }
    protected virtual void UpdateUnique(){  }
    protected virtual void FixedUpdateUnique(){  }
}
