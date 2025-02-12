using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// エンドレスバトル　敵の能動的行動　歩き
/// </summary>
public class HumanActiveActionWalk : HumanActiveAction
{
    [SerializeField, Tooltip("移動速度")] private float _walkSpd = 0.7f;
    private Transform _lookTarget = null;
    private bool _isWalk = false;

    protected override void IniiializeUnique()
    {
        _lookTarget = GameDataManager.GetPlayer().transform;


        _human.AddOnCatch(()=>
        {
            PauseUnique();
            // _human.PartsActiion((HumanChild parts)=>
            // {
            //     parts.GetRigidbody().isKinematic = false;
            // });
        });
        _human.AddOnBreakCallback(()=>
        {
            PauseUnique();
        });
        _human.AddOnStand(()=>
        {
            _human.IsFloorDead = false;
            _isWalk = true;
            // アニメーション再開
            _human.EnableAnimation();
            // _human.GetRigidbody().velocity = Vector3.zero;
            // _human.GetRigidbody().angularVelocity = Vector3.zero;
            // _human.transform.DOLocalMove(Vector3.zero, 1f).SetLink(_human.gameObject);
            // _human.PartsActiion((HumanChild parts)=>
            // {
            //     Rigidbody rigidbody = parts.GetRigidbody();
            //     rigidbody.velocity = Vector3.zero;
            //     rigidbody.angularVelocity = Vector3.zero;
            //     rigidbody.isKinematic = true;
            // });
        });
    }
    protected override void StartActiveActionUnique()
    {
        _isWalk = true;
    }
    // 一時停止された時の処理。パス移動を無効化する
    protected override void PauseUnique()
    {
        _isWalk = false;
    }
    
    // プレイヤーに向かって歩いてくる
    protected override void UpdateActiveActionUnique()
    {
        if(!_isWalk)
            return;
        // Debug.Log("およ");
        Vector3 lookPos = _lookTarget.position;
        lookPos.y = MoveTransform.position.y;

        // プレイヤーの方を見る
        MoveTransform.LookAt(lookPos);
        // 前進する
        MoveTransform.position += MoveForward * Time.deltaTime * _walkSpd;

        // _human.GetRigidbody().velocity = Vector3.zero;
        // _human.GetRigidbody().angularVelocity = Vector3.zero;
        // _human.PartsActiion((HumanChild parts)=>
        // {
        //     parts.GetRigidbody().velocity = Vector3.zero;
        //     parts.GetRigidbody().angularVelocity = Vector3.zero;
        // });
    }
}
