using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

/// <summary>
/// 擬似多重クラス継承できるインターフェース用のクラス : プレイヤーを攻撃してくる要素
// 　[System.Serializable]をクラス名の上に書くことをお忘れなく。ビルドエラーは起きずとも実行時にエラーになる
/// </summary>
[System.Serializable]
public class AttackerBase : IAttacker
{
    //　必須枠　------------------------------
    // ないといけないが実際はアクセスしない.
    // 一応自分自身を返すようにするがアクセスしたら例外を出してもいいと思う.
    public AttackerBase AttackerBaseClass
    {
        get { return this; }
    }
    private MonoBehaviour MB { get; set; }
    // public IInitializer Iinitializer { get; set; }
    public IAttacker Interface { get; set; }
    // private GameObject GO { get { return MB.gameObject; } }
    // private CancellationToken token { get { return MB.destroyCancellationToken; } }

    public void Init(MonoBehaviour mb, IAttacker iface)
    {
        this.MB = mb;
        this.Interface = iface;
    }

    //　自由枠　------------------------------
    public GameObject Object{ get{ return MB.gameObject; } }
    private Func<bool> _onCheckAttackConfirmed = null;

    // Update文で呼んでもらう
    // もうすぐ攻撃が当たるかそうでないかの判定をして、そうならスローモーにする
    public void CheckAttackConfirmed(Func<bool> func = null)
    {
        Func<bool> judje = null;
        if(func != null)
            judje = func;
        else if( _onCheckAttackConfirmed != null )
            judje = _onCheckAttackConfirmed;
        else
            Debug.Log("スローモー判定が未設定だお");

        if(func())
        {
            EndlessBattleTimeScaleManager.RegistIAttacker(Interface);
        }
        else
        {
            EndlessBattleTimeScaleManager.UnRegistIAttacker(Interface);
        }
    }

    // オブジェクトが消える際の後片付け
    public void AttackEnd()
    {
        EndlessBattleTimeScaleManager.UnRegistIAttacker(Interface);
    }

    // もうすぐ攻撃が当たるかそうでないかの判定
    public void SetOnCheckAttackConfirmed(Func<bool> func)
    {
        _onCheckAttackConfirmed = func;
    }
}
