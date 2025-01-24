using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 擬似多重クラス継承できるインターフェース : 初期化
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

    public UnityEvent OnInitialize{ get { return Initializer.OnInitialize; } }

    // メソッド
    public void AddOnInitialize( UnityAction onInitialize)
    {
        Initializer.AddOnInitialize(onInitialize);
    }
}
