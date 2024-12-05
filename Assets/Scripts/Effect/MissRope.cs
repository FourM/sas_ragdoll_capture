using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Obi;

/// <summary>
/// 何も掴めなかった時に射出されるロープ
/// </summary>
public class MissRope : MonoBehaviour
{
    // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    [SerializeField, Tooltip("位置リスト:根本→先端")] private List<Rigidbody> _pointList = default;
    [SerializeField, Tooltip("位置リスト:根本→先端")] private ObiRopeCursor _obiRopeCursor;
    [SerializeField, Tooltip("位置リスト:根本→先端")] private ObiRope _rope;
    [SerializeField, Tooltip("マテリアル")] private ObiRopeExtrudedRenderer _material;
    public float speed = 1;
    private List<Tween> _tweenList;
    private Transform _startPos = null;
    private bool _away = false;
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    private void OnDisable() {
        if(_tweenList != null)
        {
            for(int i = _tweenList.Count - 1; 0 <= i; i--)
            {
                _tweenList[i].Kill();
                _tweenList.RemoveAt(i);
            }
        }
    }
    void Update () {
		// if (Input.GetKey(KeyCode.W))
        // {
        //     Debug.Log("1:_rope.restLength: " + _rope.restLength);
		// 	_obiRopeCursor.ChangeLength(_rope.restLength - speed * Time.deltaTime);
        //     Debug.Log("2:_rope.restLength: " + _rope.restLength);
        // }
		// if (Input.GetKey(KeyCode.S))
        // {
        //     Debug.Log("1:_rope.restLength: " + _rope.restLength);
		// 	_obiRopeCursor.ChangeLength(_rope.restLength + speed * Time.deltaTime);
        //     Debug.Log("2:_rope.restLength: " + _rope.restLength);
        // }
        if(!_away && _startPos != null)
        {
            _pointList[0].transform.position = _startPos.transform.position;
            this.transform.position = _startPos.transform.position;
        }
	}
    // ---------- Public関数 -------------------------
    public void SetUp(Transform startPos, float waitTime, Material material)
    {
        _tweenList = new List<Tween>();
        _startPos = startPos;

        // 
        for(int i = 0; i < _pointList.Count; i++)
        {
            float magnitude = 10.0f * i / (_pointList.Count - 1);
            _pointList[i].velocity = this.transform.forward * magnitude;
        }

        Tween tween = DOVirtual.DelayedCall(waitTime, ()=>
        {
            for(int i = 0; i < _pointList.Count; i++)
            {
                _pointList[i].velocity = this.transform.forward * 10.0f;
            }
            _away = true;
        });
        _tweenList.Add(tween);

        // 一定時間が経ったら
        tween = DOVirtual.DelayedCall(7f, ()=>
        {
            // まだこれがあるなら、これを消す
            if(this.gameObject)
                Destroy(this.gameObject);
        });
        _tweenList.Add(tween);
        _material.material = material;
    }
    // ---------- Private関数 ------------------------
}
