using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// エンドレスバトル　敵の能動的行動　歩き
/// </summary>
public class HumanActiveActionWalk : HumanActiveAction
{
    private Transform _lookTarget = null;
    protected override void IniiializeUnique()
    {
        _lookTarget = GameDataManager.GetPlayer().transform;
    }
    protected override void StartActiveActionUnique()
    {

    }
    
    // プレイヤーに向かって歩いてくる
    protected override void UpdateActiveActionUnique()
    {
        Vector3 lookPos = _lookTarget.position;
        lookPos.y = MoveTransform.position.y;

        // プレイヤーの方を見る
        MoveTransform.LookAt(lookPos);
        // 前進する
        MoveTransform.position += MoveForward * Time.deltaTime * 0.7f;
    }
}
