using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;

/// <summary>
/// エンドレスバトル　敵の能動的行動　パス移動
/// 一度中断されたらそれ以降は無効にする
/// </summary>
public class HumanActiveActionPathMove : HumanActiveAction
{
    [SerializeField, Tooltip("移動速度")] private float _walkSpd = 0.7f;
    [SerializeField, Tooltip("参照パス")] private CinemachineDollyCart _cinemachineDollyCart = default;
    private Transform _lookTarget = null;
    private bool _isStartPathMove = false;
    private bool _isActivePathMove = false;

    protected override void IniiializeUnique()
    {
        _lookTarget = GameDataManager.GetPlayer().transform;
    }
    protected override void StartActiveActionUnique()
    {
        if(!_isActivePathMove)
        {
            _isStartPathMove = true;
            _isActivePathMove = true;
        }
    }
    // 一時停止された時の処理。パス移動を無効化する
    protected override void PauseUnique()
    {
        _isActivePathMove = false;
        _cinemachineDollyCart.m_Speed = 0f;
    }
    
    // プレイヤーに向かって歩いてくる
    protected override void UpdateActiveActionUnique()
    {   
        if(_isActivePathMove)
        {
            // パスに沿って移動する
            _cinemachineDollyCart.m_Speed = _walkSpd;
        }
    }
}
