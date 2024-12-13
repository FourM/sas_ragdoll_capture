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
    [SerializeField, Tooltip("何も掴めなかった時のロープ射出")] private MissRope _missRopePrefab = default;
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("hoge")] private List<Effects> _listEffecs = default;
    private bool _isInitialize = false;
    private List<MissRope> _missRopeList = default;
    private int _missWebIndex = 0; 
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

        _missRopeList = new List<MissRope>();
        StopAllEffect();
    }
    public void PlayEffect(Vector3 pos, effectType effectType)
    {
        int index = (int)effectType;
        if(index < 0 || _listEffecs.Count <= index)
        {
            Debug.Log("存在しない番号です：" + effectType + ", " + index + ", _listEffecs.Count:" + _listEffecs.Count);
            return;
        }

        _listEffecs[index].gameObject.SetActive(true);
        _listEffecs[index].PlayEffect(pos);
    }

    public MissRope PlayMissRope(Transform startPos, Vector3 endPos, float waitTime, Material material)
    {
        MissRope missRope = Instantiate( _missRopePrefab );

        missRope.transform.parent = startPos.transform;
        missRope.transform.localEulerAngles = Vector3.zero;
        missRope.transform.localPosition = Vector3.zero;
        missRope.transform.LookAt(endPos);
        missRope.transform.localPosition = Vector3.zero;
        missRope.transform.parent = GameDataManager.GetStage().transform;
        // missRope.transform.parent = this.transform;
        missRope.SetUp( startPos, waitTime, material );

        // 場にある糸を最大n個までにする
        if(2 <= _missRopeList.Count)
        {
            MissRope deleteMissRope = _missRopeList[0];
            _missRopeList.RemoveAt(0);
            if(deleteMissRope != null && deleteMissRope.gameObject)
                Destroy(deleteMissRope.gameObject);
        }
        _missRopeList.Add(missRope);

        return missRope;
    }
    public void StopAllEffect()
    {
        if(!_isInitialize)
            return;
        for(int i = 0; i < _listEffecs.Count; i++)
        {
            _listEffecs[i].StopAllEffect();
            _listEffecs[i].gameObject.SetActive(false);
        }

        // 糸をカメラ外に移す
        for(int i = 0; i < _missRopeList.Count; i++)
        {
            MissRope deleteMissRope = _missRopeList[0];
            if(deleteMissRope != null && deleteMissRope.gameObject)
                Destroy(deleteMissRope.gameObject);
        }
        _missRopeList = new List<MissRope>();
    }
    // ---------- Private関数 ----------
}