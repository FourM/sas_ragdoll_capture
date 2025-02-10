using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

/// <summary>
/// 擬似多重クラス継承できるインターフェース : プレイヤーを攻撃してくる要素
/// [field: SerializeField] public (本体クラス名) (本体クラス変数名) { get; set; }    // 擬似多重継承先に書く必要がある処理1/2。グローバル変数定義。publicだけど基本的に外部からは使わない
/// (本体クラス変数名).Init(this, this);  　　　　　　　　　　　　　　　　　　　　　　　　　 // 擬似多重継承先に書く必要がある処理2/2。Awake内に書く
/// </summary>
public interface IAttacker
{
    //　必須枠　------------------------------
    // 擬似多重継承できるクラスの本体
    public AttackerBase AttackerBaseClass{ get; }

    //自由枠------------------------------
    // プロパティ（継承先で書き換える気がなければvirtualじゃなくてもいい）.
    // public virtual float HP
    // {
    //      get { return Hoge.HP; }
    // }

    // public UnityEvent OnInitialize{ get { return AttackerBaseClass.OnInitialize; } }


    public void CheckAttackConfirmed(Func<bool> func = null)
    {
        AttackerBaseClass.CheckAttackConfirmed(func);
    }

    public void SetOnCheckAttackConfirmed(Func<bool> func)
    {
        AttackerBaseClass.SetOnCheckAttackConfirmed(func);
    }
}
