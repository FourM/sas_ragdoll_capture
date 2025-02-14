using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 擬似多重クラス継承できるインターフェース : 初期化
/// [field: SerializeField] public InitializerBase Initializer { get; set; }    // 擬似多重継承先に書く必要がある処理1/2。グローバル変数定義。publicだけど基本的に外部からは使わない
/// Initializer.Init(this, this);  　　　　　　　　　　　　　　　　　　　　　　　　　　　// 擬似多重継承先に書く必要がある処理2/2。Awake内に書く
/// </summary>
public interface IInitializer
{
    //　必須枠　------------------------------
    // 擬似多重継承できるクラスの本体
    public InitializerBase Initializer{ get; }

    //自由枠------------------------------
    // プロパティ（継承先で書き換える気がなければvirtualじゃなくてもいい）.
    // public virtual float HP
    // {
    //      get { return Hoge.HP; }
    // }

    // public UnityEvent OnInitialize{ get { return Initializer.OnInitialize; } }
    public void OnInitialize(){ Initializer.OnInitialize(); }

    // メソッド
    public void AddOnInitialize( UnityAction onInitialize)
    {
        Initializer.AddOnInitialize(onInitialize);
    }
}
