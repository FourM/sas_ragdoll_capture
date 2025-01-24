using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 擬似多重クラス継承できるインターフェース用のクラス : 初期化
/// </summary>
[System.Serializable]
public class InitializerBase : IInitializer
{
    //　必須枠　------------------------------
    // ないといけないが実際はアクセスしない.
    // 一応自分自身を返すようにするがアクセスしたら例外を出してもいいと思う.
    public InitializerBase Initializer
    {
        get { return this; }
    }
    private MonoBehaviour MB { get; set; }
    // public IInitializer Iinitializer { get; set; }
    public IInitializer Iinitializer { get; set; }
    // private GameObject GO { get { return MB.gameObject; } }
    // private CancellationToken token { get { return MB.destroyCancellationToken; } }

    public void Init(MonoBehaviour mb, IInitializer initializer)
    {
        this.MB = mb;
        this.Iinitializer = initializer;
    }


    //　自由枠　------------------------------
    public UnityEvent OnInitialize = null;
    // メソッド
    public void AddOnInitialize( UnityAction onInitialize)
    {
        if(OnInitialize == null)
            OnInitialize = new UnityEvent();
        OnInitialize.AddListener(onInitialize);
    }

}
