using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 柱と爆弾
/// </summary>
public class Stage020SubManager : StageSubManager
{
    // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    [SerializeField, Tooltip("ウォールシェルフ")] private Rigidbody _wallShelf;
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    public void Update()
    {
        float minAngle = 270f;
        float maxAngle = 356f;
        if( _wallShelf.transform.localEulerAngles.x < minAngle )
        {
            // Debug.Log("minAngle > !!:" + _wallShelf.transform.localEulerAngles.x);
            SetAngleX(minAngle);
        }
        if( maxAngle < _wallShelf.transform.localEulerAngles.x )
        {
            // Debug.Log("maxAngle < !!:" + _wallShelf.transform.localEulerAngles.x);
            SetAngleX(maxAngle);
        }
    }
    // ---------- Public関数 -------------------------
    // ---------- Private関数 ------------------------
    private void SetAngleX( float angleX )
    {
        Vector3 angle = _wallShelf.transform.localEulerAngles;
        angle.x = angleX;
        _wallShelf.transform.localEulerAngles = angle;
        
        Vector3 angularVelocity = _wallShelf.angularVelocity;
        angularVelocity.x = -angularVelocity.x;
        _wallShelf.angularVelocity = angularVelocity;
    }
}
