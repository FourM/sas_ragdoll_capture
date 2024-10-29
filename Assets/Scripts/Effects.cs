using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effects : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("エフェクトリスト")] private List<ParticleSystem> _listEffect = default;
    private int _index = 0;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    // ---------- Public関数 ----------
    public void PlayEffect(Vector3 pos)
    {
        ParticleSystem effect = _listEffect[_index];
        effect.transform.position = pos;
        effect.Play();

        _index++;
        _index %= _listEffect.Count;
    }
    public void StopAllEffect()
    {
        for(int i = 0; i < _listEffect.Count; i++)
        {
            _listEffect[i].Stop();
        }
    }
    // ---------- Private関数 ----------
}