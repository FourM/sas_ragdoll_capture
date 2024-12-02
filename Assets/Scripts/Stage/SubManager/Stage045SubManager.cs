using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 巨人vs大砲
/// </summary>
public class Stage045SubManager : StageSubManager
{
    // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    [SerializeField, Tooltip("大砲")] private Transform _cannnonBody;
    [SerializeField, Tooltip("トリガー")] private ChildTrigger _childTrigger;
    [SerializeField, Tooltip("時間")] private float _rollDuration;
    [SerializeField, Tooltip("時間")] private float _shotDuration;
    [SerializeField, Tooltip("時間")] private ParticleSystem _effectShot;
    [SerializeField, Tooltip("打ち出す力：前")] private float _frontVelocity;
    [SerializeField, Tooltip("打ち出す力：上")] private float _upVelocity;
    [SerializeField, Tooltip("大砲の壁コライダー")] private List<GameObject> _collisions = default;
    [SerializeField, Tooltip("大砲の玉（見えない攻撃判定）")] private Rigidbody _dymmyCanonBall = default;
    [SerializeField, Tooltip("大砲の玉 開始位置")] private Transform _dymmyCanonBallStartPos = default;
    [SerializeField, Tooltip("大砲の発射音")] private AudioSource _soundCannonShot = null;
    [SerializeField, Tooltip("大砲の駆動音")] private AudioSource _soundCannonMove = null;
    [SerializeField, Tooltip("大砲の駆動音")] private AudioSource _soundCannonMove2 = null;
    [SerializeField, Tooltip("大砲の角度")] private Vector3 _cannonRotate = default;
    private bool _isShot = false;
    private Sequence _sequence = null;
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    // ---------- Public関数 -------------------------
    // ---------- Private関数 ------------------------
    protected override void InitializeUnique()
    {
        _childTrigger.AddCallbackOnCollisionEnter(TryShotCannon);
        _dymmyCanonBall.gameObject.SetActive(false);
    }

    // 衝突相手がHumanかの可否と情報を取得
    private void TryShotCannon(Collision collision)
    {
        if(_isShot)
            return;
        _isShot = true;

        Human human = null;
        HumanChild humanChild = null;
        CatchableObj collitionChatchableObj = GameDataManager.GetCatchableObj(collision.gameObject);
        
        if(collitionChatchableObj != null)
            humanChild = collitionChatchableObj.TryGetHumanChild();
        if(humanChild != null)
            human = humanChild.Gethuman();
        if( human == null )
            return;
        
        human.OnRelease();
        humanChild.OnRelease();
        Rigidbody humanRigidbody = human.GetParts(HumanParts.waist).GetRigidbody();
        humanRigidbody.velocity = Vector3.zero;
        humanRigidbody.isKinematic = true;
        humanRigidbody.useGravity = false;
        human.gameObject.SetActive(false);

        Sequence sequence = DOTween.Sequence();

        // ウィーンの音
        if(PlayerPrefs.GetInt("Effect_ON", 1) == 1)
            _soundCannonMove.PlayOneShot(_soundCannonMove.clip);

        // ガチャッ音を出す
        if(PlayerPrefs.GetInt("Effect_ON", 1) == 1)
        {
            sequence.AppendCallback(()=>{
                _soundCannonMove.PlayOneShot(_soundCannonMove.clip);
                DOVirtual.DelayedCall(_rollDuration - 0.1f, ()=>
                {
                    _soundCannonMove2.PlayOneShot(_soundCannonMove2.clip);
                });
            });
        }

        sequence.Append(_cannnonBody.DOLocalRotate(_cannonRotate, _rollDuration).SetEase(Ease.InOutBack).OnComplete(()=>
        {
            if(_soundCannonMove != null)
                _soundCannonMove.Stop();
        }));
        sequence.AppendInterval(_shotDuration);
        // 発射あ！！
        sequence.AppendCallback(()=>{
            _effectShot.Play();                             // エフェクト発生
            _dymmyCanonBallStartPos.parent = GameDataManager.GetStage().transform; // 射出開始位置の親を、大砲の外にしておく
            human.gameObject.SetActive(true);               // 非表示にしていたHumanを再表示
            _dymmyCanonBall.gameObject.SetActive(true);     //　ダミー砲弾を有効化
            _dymmyCanonBall.transform.parent = _dymmyCanonBallStartPos; // ダミー砲弾の親を射出開始位置にする
            _dymmyCanonBall.gameObject.layer = human.GetParts(HumanParts.waist).gameObject.layer; // ダミー砲弾のレイヤーを、捕まえた敵と同じにする
            _dymmyCanonBall.transform.localPosition = Vector3.zero; // ダミー砲弾の位置補正
            _dymmyCanonBall.transform.localEulerAngles = Vector3.zero; // ダミー砲弾の角度補正
            human.transform.parent = _dymmyCanonBall.transform;     // 敵の親をダミー砲弾にする
            human.transform.localPosition = Vector3.zero;           // 敵の位置補正
            human.SetPos(_dymmyCanonBall.transform.position);       // 敵の位置補正
            human.PartsActiion((HumanChild parts)=>{                // 敵の各部位にかかってる運動エネルギーをなくす
                parts.GetRigidbody().velocity = Vector3.zero;
                parts.GetRigidbody().useGravity = false;
            });
            human.GetRigidbody().useGravity = false;                // 敵本体にかかってる運動エネルギーをなくす
            human.GetRigidbody().velocity = Vector3.zero;
                                                                    // ダミー砲弾に初速設定
            Vector3 velocity = _dymmyCanonBall.transform.forward * _frontVelocity;
            velocity += _dymmyCanonBall.transform.up * _upVelocity;
            _dymmyCanonBall.velocity = velocity;

            // 大砲の壁の判定をなくす
            for(int i = 0; i < _collisions.Count; i++)
            {
                int index = i;
                _collisions[index].SetActive(false);
            }

            if(PlayerPrefs.GetInt("Effect_ON", 1) == 1)
                _soundCannonShot.PlayOneShot(_soundCannonShot.clip);
        });
        sequence.AppendInterval(0.5f);
        sequence.AppendCallback(()=>{
            human.transform.parent = GameDataManager.GetStage().transform;
            human.PartsActiion((HumanChild parts)=>{
                parts.GetRigidbody().velocity = _dymmyCanonBall.velocity;
                parts.GetRigidbody().useGravity = true;
            });
            human.GetRigidbody().useGravity = true;                
            human.GetRigidbody().velocity = _dymmyCanonBall.velocity;
            human.GetRigidbody().isKinematic = false;
            humanRigidbody.isKinematic = false;
            _dymmyCanonBall.gameObject.SetActive(false);
            human.OnBreak();
        });
        sequence.AppendInterval(0.3f);
        sequence.AppendCallback(()=>{
            for(int i = 0; i < _collisions.Count; i++)
            {
                int index = i;
                _collisions[index].SetActive(true);
            }
        });
        sequence.AppendInterval(1f);
        if(PlayerPrefs.GetInt("Effect_ON", 1) == 1)
            sequence.AppendCallback(()=>{ _soundCannonMove.PlayOneShot(_soundCannonMove.clip); });
        sequence.Append(_cannnonBody.DOLocalRotate(new Vector3(0, 90, -90), _rollDuration).SetEase(Ease.InOutBack).OnComplete(()=>
        {
            if(_soundCannonMove != null)
                _soundCannonMove.Stop();
        }));
        sequence.AppendCallback(()=>{
            _isShot = false;
        });
    }
}
