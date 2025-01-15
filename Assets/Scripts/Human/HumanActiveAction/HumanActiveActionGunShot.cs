using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// エンドレスバトル　敵の能動的行動　銃を撃つ
/// </summary>
public class HumanActiveActionGunShot : HumanActiveAction
{
    [SerializeField, Tooltip("銃")] private EnemyGun _enemyGun = null;
    [SerializeField, Tooltip("発射開始ディレイ")] private float _shotDelay = 0.7f;
    [SerializeField, Tooltip("対象の前を狙う補正")] private float _targetForward = 0f;
    [SerializeField, Tooltip("対象を狙ってくるか")] private bool _isAim = true;

    private Transform _lookTarget = null;
    private Vector3 _targetPrePos = default;
    private float _shotWait = 0f;
    private bool _isHaveGun = true;
    private float addY = 0.1f;

    protected override void IniiializeUnique()
    {
        if(_isAim)
        {
            _lookTarget = GameDataManager.GetPlayer().GetBulletTargetTransform();
            _targetPrePos = _lookTarget.position + _lookTarget.forward * _targetForward;
            _targetPrePos.y += addY;
        }
        else
        {
            _lookTarget = null;
        }
        _shotWait = _shotDelay;

        // 銃を取り上げられたら何もしなくなる
        _enemyGun.AddOnCatch(()=>
        {
            _isHaveGun = false;
        });

        _enemyGun.Initialize();
        _enemyGun.SetBurretParent(_human.transform.parent.parent);
        _enemyGun.SetTarget(_lookTarget, _targetForward);

        _human.AddOnInitialize(()=>
        {
            // 銃を持たせる
            Vector3 pos = _enemyGun.transform.localPosition;
            Vector3 ang = _enemyGun.transform.localEulerAngles;

            Transform hand = null;
            hand = _human.GetParts(HumanParts.handR).transform;
            if(hand != null)
                _enemyGun.transform.parent = hand;
            // else
            //     Debug.Log("handがNullだぞい");

            _human.AddOnBreakCallback(()=>
            {
                _enemyGun.transform.parent = this.transform;
                _enemyGun.GetRigidbody().useGravity = true;
                _enemyGun.GetRigidbody().isKinematic = false;
            });
            _enemyGun.transform.localPosition = pos;
            _enemyGun.transform.localEulerAngles = ang;
        });
        _enemyGun.SetHuman(_human);
    }
    protected override void StartActiveActionUnique()
    {

    }
    
    // プレイヤーに弾を撃ってくる
    protected override void FixedUpdateActiveActionUnique()
    {
        Vector3 lookPos = default;
        if(_isAim)
        {
            Vector3 currnetLookPos = _lookTarget.position + _lookTarget.forward * _targetForward;
            currnetLookPos.y += addY;
            // 射撃する位置を取得
            lookPos = LinePrediction(_enemyGun.GetShotPos(), currnetLookPos, _targetPrePos, _enemyGun.GetShotSpd());
            
            Vector3 HumanLookPos = lookPos;
            HumanLookPos.y = MoveTransform.position.y;
            // プレイヤーの方を見る
            MoveTransform.LookAt(HumanLookPos);
            _targetPrePos = currnetLookPos;
        }

        // 銃をまだ持ってたら
        if(_isHaveGun)
        {
            if(_isAim)
                _enemyGun.transform.LookAt(lookPos);

            // 時間計測
            _shotWait -= Time.deltaTime;
            // 発射あ！！
            if(_shotWait <= 0)
            {
                _enemyGun.ShotStart();
            }
        }
    }


    // 偏差射撃する振り向き方。　コードはネットからのコピペ。二次方程式の応用らしい。
    //線形予測射撃改良案
    public Vector3 LinePrediction(Vector3 shotPosition, Vector3 targetPosition, Vector3 targetPrePosition, float bulletSpeed)
    {
        //Unityの物理はm/sなのでm/flameにする
        bulletSpeed = bulletSpeed * Time.fixedDeltaTime;
        Vector3 v3_Mv = targetPosition - targetPrePosition;
        Vector3 v3_Pos = targetPosition - shotPosition;

        float A = Vector3.SqrMagnitude(v3_Mv) - bulletSpeed * bulletSpeed;
        float B = Vector3.Dot(v3_Pos, v3_Mv);
        float C = Vector3.SqrMagnitude(v3_Pos);

        //0割禁止
        if (A == 0 && B == 0)return targetPosition;
        if (A == 0 )return targetPosition + v3_Mv * (-C / B / 2);

        //虚数解はどうせ当たらないので絶対値で無視した
        float D = Mathf.Sqrt(Mathf.Abs(B * B - A * C));
        //予想位置を返す
        return targetPosition + v3_Mv * PlusMin((-B - D) / A, (-B + D) / A);
    }
    //プラスの最小値を返す(両方マイナスなら0)
    public float PlusMin(float a, float b)
    {
        if (a < 0 && b < 0) return 0;
        if (a < 0) return b;
        if (b < 0) return a;
        return a < b ? a : b;
    }
}
