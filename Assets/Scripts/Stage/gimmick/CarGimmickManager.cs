using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CarGimmickManager : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("hoge")] private List<Transform> _listCars = default;
    [SerializeField, Tooltip("hoge")] private Transform _carStartPos = default;
    [SerializeField, Tooltip("hoge")] private Transform _carEndPos = default;
    [SerializeField, Tooltip("hoge")] private float _interval = 2.5f;
    [SerializeField, Tooltip("hoge")] private float _carMoveDuration = 5f;
    private Sequence _sequence = null;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    private void Start(){
        _sequence = DOTween.Sequence();

        for(int i = 0; i < _listCars.Count; i++)
        {
            int index = i;
            Transform car = _listCars[index];

            _sequence.AppendCallback(()=>
            {
                car.position = _carStartPos.position;
            });
            _sequence.Append(car.DOMove(_carEndPos.position, _carMoveDuration).SetEase(Ease.Linear)).SetLoops(-1, LoopType.Restart);
            _sequence.AppendInterval(_interval);
        }
    }

    private void OnDisable() {
        if(_sequence != null)
            _sequence.Kill();
    }
    // ---------- Public関数 ----------
    // ---------- Private関数 ----------
}
