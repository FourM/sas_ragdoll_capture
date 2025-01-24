using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;

public class LookAtPathMove : PlayerPassThroughTriggerEvent
{
    [SerializeField, Tooltip("参照パス")] private CinemachineDollyCart _cinemachineDollyCart = default;
    [SerializeField, Tooltip("速度（プレイヤーと同じにする）")] private bool _isPlayerSpd = true;
    [SerializeField, Tooltip("速度（一定）")] private float _defaultSpd = 0f;

    protected override void InitializeUnique()
    {
        _cinemachineDollyCart.m_Speed = 0f;
    }

    // プレイヤーが指定のパスを通過した時のアクション
    protected override void UpdateUnique()
    {
        if(_isTrigger && _isPlayerSpd)
        {
            _cinemachineDollyCart.m_Speed = GameDataManager.GetPlayer().GetMovePath().m_Speed;
        }
    }
    // プレイヤーが指定のパスを通過した時のアクション
    protected override void OnTriggerUnique()
    {
        _cinemachineDollyCart.m_Speed = _defaultSpd;
    }   
}
