using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
// using MoreMountains.NiceVibrations;

public class HumanChild : CatchableObj
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("アニメーション")] private Human _parentHuman = default;
    // [SerializeField, Tooltip("リジッドボディ")] private Rigidbody _rigidBody = default;
    [SerializeField, Tooltip("この部位が強い衝撃を受けたら死ぬか？")] private bool _isDeadable = false;
    [SerializeField, Tooltip("この部位に対応する壊れるパーツ")] private BreakableHumanoidParts _breakableParts = default;
    [SerializeField, Tooltip("パーツカテゴリ")] private HumanParts _partsType = HumanParts.none;
    [SerializeField, Tooltip("起きあがり接地判定距離")] private float _rayDistance = 1.1f;
    [SerializeField, Tooltip("足元がなくなった判定距離")] private float _rayDistance2 = 1.1f;
    private float _mass = 0f;
    private Vector3 _impactPos;
    private bool _isFixMass = false;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    private void Awake() {
        _parentHuman.AddOnInitialize(Initialize);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(0 < GameDataManager.GetMutekiTime())
            return;
        if(collision.gameObject.layer == this.gameObject.layer)
            return;
        if(collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
            return; 
        if(collision.gameObject.layer == LayerMask.NameToLayer("NotKillFloor"))
            return; 
        if(collision.gameObject.layer == LayerMask.NameToLayer("ThroughWall"))
            return;
        if(collision.gameObject.layer == LayerMask.NameToLayer("catchableNotKill"))
            return;
        // ギミックで倒す必要があるなら、床への激突では死なない
        if(collision.gameObject.layer == LayerMask.NameToLayer("Floor") && GameDataManager.IsGimmickKill())
            return;
        // ギミックで倒す必要があるなら、他Humanの激突では死なない
        if(collision.gameObject.layer == LayerMask.NameToLayer("BreakableParts") && GameDataManager.IsGimmickKill())
            return;
        // ギミックで倒す必要があるなら、他Humanの激突では死なない
        if(collision.gameObject.layer == LayerMask.NameToLayer("catchableParent") && GameDataManager.IsGimmickKill())
            return;
        // ギミックで倒す必要があるなら、他Humanの激突では死なない
        if(collision.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast") && GameDataManager.IsGimmickKill())
            return;
        // 直前まで接触していた
        if(_parentHuman.IsStayObject(collision.gameObject))
            return;

        // ギミックで倒す必要があるなら、能動的には死なない？
        if(GameDataManager.IsGimmickKill())
            return;
        
        bool isOtherHuman = false;
        bool isDead = _parentHuman.IsDead();

        // 致死判定に用いるパラメータ
        // float collisionSpeed = collision.impulse.magnitude;
        float collisionSpeed = GetBeforeVelocityMagnitude();
        collisionSpeed *= 0.95f;

        // Debug.Log("collisionSpeed:" + collisionSpeed);

        // 高速スワイプや、他のHumanかの情報を取得
        CatchableObj collitionChatchableObj = GameDataManager.GetCatchableObj(collision.gameObject);
        bool collisionFastSwiped = IsFastSwipedCollision(collision);
        if(collitionChatchableObj != null)
        {
            // 衝突相手が自分自分なら何もしない
            GameObject collitionParent = collitionChatchableObj.GetParent();
            if(collitionParent != null && collitionParent == GetParent())
                return;
            // 衝突相手が別のHumanなら、そのフラグを立てる
            isOtherHuman = IsCollisionHuman(collision);

            collisionSpeed += collitionChatchableObj.GetBeforeVelocityMagnitude();

            // 対象が、直前まで高速で振り回されていたか
            collisionFastSwiped = collitionChatchableObj.IsFastSwiped();
        }

        if(GameDataManager.IsGimmickKill())
        {
            // ギミックで倒す必要があるなら、他のHumanの激突では死なない
            if(isOtherHuman)
                return;
        }
        
        // 衝突位置を保存
        _impactPos = collision.GetContact(0).point;

        // 死ねる衝撃の強さ
        float killShockStrength = GameDataManager.GetKillShockStrength() * _parentHuman.GetToughness();
        // 捕まってるか、他の捕まえられる物に当たった時はしぬるラインをゆるくする
        if(isOtherHuman)
            killShockStrength = killShockStrength * 7f / 8f;
        
        // 致死衝撃を受けたか否か
        if( killShockStrength <= collisionSpeed || collisionFastSwiped)
        {
            // Debug.Log("collision.gameObject.layer:" + collision.gameObject.layer + ", " + collision.gameObject.name);
            // Debug.Log("killShockStrength:" + killShockStrength + ", " + collisionSpeed);
            // 致死衝撃を受けた処理
            OnBreak();
            if(isOtherHuman)
                collitionChatchableObj.OnBreak();
            // Debug.Log(";" + GameDataManager.IsGimmickKill() + ", " + isOtherHuman + ", " + collision.gameObject.name + ", " + collision.gameObject.layer);
        }
        else if( (killShockStrength / 2f ) <= collisionSpeed && this.gameObject.tag != collision.gameObject.tag)
        {
            // 衝撃を受けたエフェクト
            EffectManager.instance.PlayEffect(_impactPos, effectType.impactSmall);
        }

        if( !isDead)
            FirebaseManager.instance.EventCrashed(collisionSpeed, _parentHuman.IsDead());
    }

    // 直前まで触れていたオブジェクトとして登録
    private void OnCollisionStay(Collision collision)
    {
        _parentHuman.SetStayObjectDic(collision.gameObject);
    }

    //　自分(か衝突相手)が「高速スワイプされた」状態かチェック
    private bool IsFastSwipedCollision(Collision collision = null)
    {
        bool ret = _parentHuman.IsFastSwiped();
        // コリジョン対象の高速スワイプ状態の取得を試行。ダメならそのタイミングでリターンする
        if(collision == null)
            return ret;
        
        // CatchableObjか
        CatchableObj collitionChatchableObj = GameDataManager.GetCatchableObj(collision.gameObject);
        if(collitionChatchableObj == null)
            return ret;

        ret = ret || collitionChatchableObj.IsFastSwiped();
        // 相手もHumanなら、その親の状態も確認する
        Human human = collitionChatchableObj.TryGetParentHuman();
        if(human == null)
            return ret;
        ret = ret || human.IsFastSwiped();
        return ret;
    }

    protected override void StartUnique(){
        this.tag = _parentHuman.tag;
        SetParent(_parentHuman.gameObject);
        _impactPos = Vector3.zero;

        // パーツ登録
        if(_partsType != HumanParts.none)
            _parentHuman.SetParts(_partsType, this);

        // この部位の元々の重さを取得
        Rigidbody rigidbody = GetRigidbody();
        _mass = rigidbody.mass;
        rigidbody.useGravity = false;

        // エンドレスバトルなら前後移動ロックを外す
        if(GameDataManager.GameMode == GameMode.endlessBattle)
            rigidbody.constraints = RigidbodyConstraints.None;

        // 「各パーツの質量を変える」コールバックの処理設定
        _parentHuman.AddCallbackChangePartsMass(()=>
        {
            if(!_isFixMass){
                if(_parentHuman.IsCatch())
                {
                    // この部位が捕まった部位に近いなら重くする
                    if(_alternate != null && _alternate.IsCatch() )
                    {
                        if( _alternate == this )
                        {
                            rigidbody.mass = 2f;
                        }
                        else
                            rigidbody.mass = 0.7f;
                        // Debug.Log("応よ:" + this.gameObject.name);
                    }
                    // 捕まった部位から遠いなら軽くする
                    else
                    {
                        rigidbody.mass = 0.1f;
                        // Debug.Log("軽量化よ:" + this.gameObject.name);
                    }
                }
                else
                {
                    // 重さを元に戻す
                    rigidbody.mass = _mass;
                    // Debug.Log("戻すよ:" + this.gameObject.name);
                }
            }
            else
            {
                // Debug.Log("否よ:" + this.gameObject.name);
            }
        });

        _parentHuman.AddCallbackOnDesableAnimation(()=>
        {
            rigidbody.useGravity = true;
            
            if( _partsType == HumanParts.waist || _partsType == HumanParts.head)
            {
                rigidbody.velocity = _parentHuman.GetRigidbody().velocity;
            }
            else
            {
                float wait = 0.5f;
                rigidbody.velocity *= (1f-wait);
                rigidbody.velocity += _parentHuman.GetRigidbody().velocity * wait;
            }
        });

        _parentHuman.AddCallbackOnCatch(()=>
        {
            GetRigidbody().velocity += new Vector3(0, 1, 0);
        });

        if(_alternate == null)
            _alternate = this;
        _parentHuman.AddOnBreakCallback(()=>{
            this.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        });

        // ギミック側から処理をしたい時のコールバック
        _parentHuman.AddCallbackOnPartsActiion(( UnityAction<HumanChild> action)=>
        {
            action(this);
        });
    }

    protected override void UpdateUnique() {
        if(this.gameObject.layer != _parentHuman.GetChildLayer() && !_parentHuman.IsDead())
            this.gameObject.layer = _parentHuman.GetChildLayer();
        
        float scale = _parentHuman.transform.localScale.y;
        // float scale = 1f;

        // 起き上がり処理のオンオフ切り替え
        if(_partsType == HumanParts.waist )
        {
            Debug.DrawRay(this.transform.position, -Vector3.up * _rayDistance * scale, Color.red, 0.1f, false);
            RaycastHit hit;
            LayerMask mask = LayerMask.GetMask("Deafult", "catchable", "Floor", "ThroughWall", "catchableNotKill",
            "catchableThroughFloor", "catchableNoMutualConflicts", "catchableThroughWall", "NotKillFloor");

            // 起き上がり処理中でない & アニメーション中でない
            // if(!_parentHuman.IsFollow() && !_parentHuman.IsEnableAnimation())
            {
                // 足元の地面を検知
                if (Physics.Raycast(this.transform.position, -Vector3.up, out hit, _rayDistance * scale, mask))
                {
                    // 捕まっていない
                    if(!_parentHuman.IsCatch() )
                    {
                        // X軸も位置補正するかは、足場による
                        bool isMove = false;
                        // if(hit.transform.gameObject.layer == LayerMask.NameToLayer("catchable"))
                        //     isMove = true;
                        // if(hit.transform.gameObject.layer == LayerMask.NameToLayer("catchableNotKill"))
                        //     isMove = true;
                        // if(hit.transform.gameObject.layer == LayerMask.NameToLayer("catchableThroughFloor"))
                        //     isMove = true;
                        // if(hit.transform.gameObject.layer == LayerMask.NameToLayer("catchableNoMutualConflicts"))
                        //     isMove = true;
                        // if(hit.transform.gameObject.layer == LayerMask.NameToLayer("catchableThroughWall"))
                        //     isMove = true;

                        // 位置更新
                        _parentHuman.SetFollowBasePos(hit.point, isMove);
                        // 起きあがり
                        if(!_parentHuman.IsFollow() && !_parentHuman.IsEnableAnimation())
                            _parentHuman.SetIsPartsFollow(true);
                        // Debug.Log("床を検知！:" + hit.point + ", " + hit.transform.name);
                    }
                    // _parentHuman.SetIsGround(true);
                }
                else
                {
                    // _parentHuman.SetIsGround(false);
                }
            }

            // 起き上がり処理中orアニメーション中
            if(_parentHuman.IsFollow() || _parentHuman.IsEnableAnimation()) 
            {
                // 足元に床を検知できない
                if (!Physics.Raycast(this.transform.position, -Vector3.up, out hit, _rayDistance2 * scale, mask))
                {
                    _parentHuman.SetIsPartsFollow(false);
                    _parentHuman.DesableAnimation();
                    // _parentHuman.SetIsGround(false);
                    Vector3 velocity = GetRigidbody().velocity;
                    if( 0f < velocity.y )
                        velocity.y += 1.5f;
                    if( velocity.y <= 0f )
                        velocity.y -= 2f;
                    GetRigidbody().velocity = velocity;
                    // Debug.Log("起き上がらない！" );
                }
            }
        }

        if(IsCatch())
        {
            // Debug.Log("捕まってる！：" + this.transform.name);
        }
        // 壊れるパーツで、死んでる時に高速移動してたら壊れる
        if(_parentHuman.IsDead() && 10f <= GetRigidbody().velocity.magnitude && _breakableParts != null && !IsCatch() && (_alternate == null || !_alternate.IsCatch()))
        {
            // Debug.Log("壊れるぅ：" + this.transform.name);
            _breakableParts.Break(GetRigidbody().velocity);
            gameObject.SetActive(false);
            // if(_breakableParts != null && _breakableParts.transform.parent == this.transform)
            // {
            //     Debug.Log("逃げ遅れたで。2:" + _breakableParts.transform.name);
            // }
        }
    }
    public void SetImpactPos(Vector3 pos ){ _impactPos = pos; }
    public Human Gethuman(){ return _parentHuman; }
    // ---------- Public関数 ----------
    protected override void OnBreakUnique()
    { 
        bool _isCatched = IsCatch();
        CatchableObj alternate = GetAlternate();
        if(alternate != null)
            _isCatched |= alternate.IsCatch();

        // 体や頭など大事な部位が強い衝撃を受けたら死ぬ
        if(_isDeadable)
        {
            // 致死衝撃を受けたエフェクト発生
            if(!_parentHuman.IsDead())
                EffectManager.instance.PlayEffect(_impactPos, effectType.impact);
            _parentHuman.OnBreak();
            // MMVibrationManager.Haptic (HapticTypes.Success);
        }
        else if(!_parentHuman.IsDead())
            EffectManager.instance.PlayEffect(_impactPos, effectType.impactSmall);

        // これに対応する、壊れるパーツがあるならそれを破壊する
        if(_breakableParts != null && ( _isDeadable || _parentHuman.IsDead() ) && !_isCatched && (_alternate == null || !_alternate.IsCatch()))
        {
            // Debug.Log("壊れるぅ２:" + this.transform.name);
            _breakableParts.Break(GetRigidbody().velocity);

            gameObject.SetActive(false);
            if(_breakableParts != null && _breakableParts.transform.parent == this.transform)
            {
                Debug.Log("逃げ遅れたで。1:" + _breakableParts.transform.name);
            }
        }
    }

    public void SetIsFixMass( bool isFixMass ){ _isFixMass = isFixMass; }
    // ---------- Private関数 ----------
    protected override void OnCatchUnique()
    { 
        _parentHuman.OnCatch();
        _parentHuman.SetOnDoReleaseCallback(_onDoReleaseCallback);
    }
    protected override void OnReleaseUnique()
    { 
        _parentHuman.OnRelease();
        // _parentHuman.SetOnDoReleaseCallback(_onDoReleaseCallback);
    }
}

