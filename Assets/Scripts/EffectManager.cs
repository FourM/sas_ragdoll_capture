using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum effectType{
    impact = 0,
    impactSmall = 1,
}
public class EffectManager : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("hoge")] private List<Effects> _listEffecs = default;
    private bool _isInitialize = false;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    public static EffectManager instance = null;
    // ---------- Unity組込関数 ----------
    private void Awake()
    {
        if(instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }
    // ---------- Public関数 ----------
    public void Initialize()
    {
        if(_isInitialize)
            return;
        _isInitialize = true;
    }
    public void PlayEffect(Vector3 pos, effectType effectType)
    {
        int index = (int)effectType;
        if(index < 0 || _listEffecs.Count <= index)
        {
            Debug.Log("存在しない番号です：" + effectType + ", " + index);
            return;
        }

        _listEffecs[index].gameObject.SetActive(true);
        _listEffecs[index].PlayEffect(pos);
    }
    public void StopAllEffect()
    {
        for(int i = 0; i < _listEffecs.Count; i++)
        {
            _listEffecs[i].StopAllEffect();
            _listEffecs[i].gameObject.SetActive(false);
        }
    }
    // ---------- Private関数 ----------
}