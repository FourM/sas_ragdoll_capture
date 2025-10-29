using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyGunBurret : CatchableObj, IAttacker
{
    // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    [SerializeField, Tooltip("hoge")] private int hoge = default;
    [SerializeField, Tooltip("プレイヤーに対しての当たり判定")] private ChildTrigger _playerCollider = default;
    [SerializeField, Tooltip("敵に対しての当たり判定")] private ChildTrigger _enemyCollider = default;

    [field: SerializeField] public AttackerBase AttackerBaseClass { get; set; }

    private float _duration = 0;
    Transform _target = null;
    private Vector3 _targetPrePos = default;
    private float _spd = 0;
    private float _targetForward = 0f;
    private bool _playerAttacker = true;
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    protected override void AwakeUnique()
    {
        AttackerBaseClass.Init(this, this);
    }
    protected override void StartUnique()
    {
        // プレイヤーに対しての当たり判定を有効化
        _playerCollider.gameObject.SetActive(true);

        // 各当たり判定のイベント設定
        _playerCollider.AddCallbackOnCollisionEnter(OnPlayerCollision);
        _enemyCollider.AddCallbackOnCollisionEnter(OnEnemyCollision);
        // プレイヤーに捕まったらプレイヤーへの攻撃判定を無効化して、敵に対しての攻撃判定を有効化する
        AddOnCatch(()=>
        {
            Stall();
        });

        // 敵に対しての当たり判定を無効化しておく
        // _enemyCollider.gameObject.SetActive(false);
    }

    public void Stall( float multiVelocity = 0.5f )
    {
        _playerCollider.gameObject.SetActive(false);
        _enemyCollider.gameObject.SetActive(true);
        _target = null;
        GetRigidbody().useGravity = true;
        GetRigidbody().constraints = RigidbodyConstraints.None;
        GetRigidbody().linearVelocity *= multiVelocity;
        _playerAttacker = false;
    }

    public void FixedUpdate() {
        float attackHitTime = 1000000f;
        if(_target != null && GameDataManager.GameState != GameState.result)
        {
            Vector3 currentTargetPos = _target.position + _target.forward * _targetForward;
            // ターゲットとの予測着弾位置
            Vector3 targetPos = LinePrediction(transform.position, currentTargetPos, _targetPrePos, _spd);
            // 予測位置に向けたベクトル取得
            Vector3 posSub = (targetPos - this.transform.position);
            Vector3 ang = posSub.normalized;

            GetRigidbody().linearVelocity = ang * _spd;

            _targetPrePos = currentTargetPos;

            // 移動している方を向く
            float rotationSpeed = 0.5f;
            Quaternion targetRotation = Quaternion.LookRotation(GetRigidbody().linearVelocity);
            this.transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            // 目標地点に近くなったら追従を止める
            if(posSub.magnitude < 1.0f)
                _target = null;

            if(_spd != 0f)
                attackHitTime = posSub.magnitude / _spd;
        }

        // スローモーション判定
        AttackerBaseClass.CheckAttackConfirmed(()=>
        {
            if(GameDataManager.GameState == GameState.result)
                return false;
            if(!_playerAttacker)
                return false;
            if(attackHitTime < 0.5f)
                return true;
            return false;
        });
    }

    // セットアップ　発射位置、弾速、親、寿命
    public void SetUp(Vector3 pos, Vector3 spd, Transform parent, Quaternion ang, float duration = -1f, Transform target = null, float targetForward = 0f)
    {
        this.transform.position = pos;
        this.transform.parent = parent;
        this.transform.rotation = ang;

        _spd = spd.magnitude;

        GetRigidbody().AddForce(spd, ForceMode.VelocityChange);

        // 寿命の設定
        if( 0f < duration )
        {
            DOVirtual.DelayedCall(duration, ()=>{
                Destroy(this.gameObject);
            }).SetLink(this.gameObject);
        }

        _target = target;
        if(_target != null)
        {
            _targetPrePos = _target.position + _target.forward * _targetForward;
        }
        _targetForward = targetForward;

        // AttackerBaseClass.LookTransform = this.transform;
        AttackerBaseClass.LookTransform = null;
    }
    // ---------- Public関数 -------------------------
    // ---------- Private関数 ------------------------
    private void OnPlayerCollision( Collision collision )
    {
        if( collision.gameObject == GameDataManager.GetPlayer().gameObject && GameDataManager.GameState != GameState.result)
        {
            // 敵の攻撃がヒットした時の処理
            GameDataManager.InGameMainEvent.OnEnemyAttackHit();
        }
        else if(collision.gameObject.tag == "UnCatchable" || collision.gameObject.layer == LayerMask.NameToLayer("Default") || collision.gameObject.layer == LayerMask.NameToLayer("Floor"))
        {
            Stall(0.2f);
        }
    }
    // 敵にぶつかった時の処理　IronBallからろくに処理を確認せずにほぼまんまコピペ
    private void OnEnemyCollision( Collision collision )
    {
        if(GetRigidbody() == null)
            return;

        CatchableObj catchableObj = GameDataManager.GetCatchableObj(collision.transform.gameObject);
        GameObject parentObj = null;
        CatchableObj parentCatchableObj = null;

        if(catchableObj != null)
            parentObj = catchableObj.GetParent();
        if(parentObj != null)
            parentCatchableObj = GameDataManager.GetCatchableObj(parentObj);

        float killShockStrength = GameDataManager.GetKillShockStrength();

        HumanChild humanChild = null;

        if(catchableObj != null)
        {
            humanChild = catchableObj.TryGetHumanChild();

            bool isBroken = catchableObj.IsBroken();
            float collisionSpeed = GetBeforeVelocityMagnitude();
            // ぶっ壊す
            if(killShockStrength <= collisionSpeed)
            {
                if(humanChild != null)
                    humanChild.SetImpactPos(collision.GetContact(0).point);
                
                if(catchableObj.GetRigidbody() != null)
                {
                    catchableObj.GetRigidbody().linearVelocity = GetBeforeVelocity().normalized * 10f;
                }
                catchableObj.OnDamage(150);

                if(parentCatchableObj != null)
                    parentCatchableObj.OnBreak();
            }
            bool isKill = catchableObj.IsBroken();
        }
    }

    protected override void OnCatchUnique()
    {
        // _target = null;
        // GetRigidbody().useGravity = true;
        // GetRigidbody().constraints = RigidbodyConstraints.None;
        // GetRigidbody().velocity /= 2f;
        Stall();
    }

    protected override void OnDisableUnique()
    {
        AttackerBaseClass.AttackEnd();
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
