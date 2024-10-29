using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GimmickGenerator : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("生成するもの")] private GameObject _generateGimmick = default;
    [SerializeField, Tooltip("生成する時間感覚")] private float _duration = 1.0f; 
    // [SerializeField, Tooltip("最大数")] private int _maxNum = 10; 

    private Tween _twinner = default; 
    // private List<GameObject> _gimmickList = 
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    private void Start(){
        _twinner = DOVirtual.DelayedCall(_duration, ()=>{
            GenarateGimmick();
        }).SetLoops(-1, LoopType.Restart);
    }

    private void OnDestroy()
    {
        _twinner.Kill();;
    }
    // ---------- Public関数 ----------
    // ---------- Private関数 ----------
    private void GenarateGimmick()
    {
        GameObject gimmick = Instantiate(_generateGimmick, Vector3.zero, Quaternion.identity);
        gimmick.transform.parent = this.transform;
        gimmick.transform.localPosition = Vector3.zero;
    }
}
