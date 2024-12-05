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
    [SerializeField, Tooltip("エフェクトリスト")] private List<AudioSource> _listSound = default;
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

        if( _index < _listSound.Count )
        {
            AudioSource audio = _listSound[_index];
            if(PlayerPrefs.GetInt("Effect_ON", 1) == 1)
                audio.PlayOneShot(audio.clip);
        }

        _index++;
        _index %= _listEffect.Count;
    }
    public void StopAllEffect()
    {
        for(int i = 0; i < _listEffect.Count; i++)
        {
            _listEffect[i].Stop();
            if( i < _listSound.Count )
                _listSound[i].Stop();
        }
    }
    // ---------- Private関数 ----------
}