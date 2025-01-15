using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;

public class LookAtPathMove : PlayerPassThroughTriggerEvent
{
    [SerializeField, Tooltip("参照パス")] private CinemachineDollyCart _cinemachineDollyCart = default;

    protected override void InitializeUnique()
    {
        _cinemachineDollyCart.m_Speed = 0f;
    }

    // プレイヤーが指定のパスを通過した時のアクション
    protected override void UpdateUnique()
    {
        if(_isTrigger)
        {
            _cinemachineDollyCart.m_Speed = GameDataManager.GetPlayer().GetMovePath().m_Speed;
        }
    }
}
