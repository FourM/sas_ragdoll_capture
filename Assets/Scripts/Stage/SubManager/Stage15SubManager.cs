using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 大砲ステージ
/// </summary>
public class Stage15SubManager : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("ステージ")] private GameStage _stage;
    [SerializeField, Tooltip("大砲")] private Transform _cannnonBody;
    [SerializeField, Tooltip("トリガー")] private ChildTrigger _childTrigger;
    [SerializeField, Tooltip("時間")] private float _rollDuration;
    [SerializeField, Tooltip("時間")] private float _shotDuration;
    [SerializeField, Tooltip("時間")] private ParticleSystem _effectShot;
    [SerializeField, Tooltip("打ち出す力")] private Vector3 _shotVelocity;
    [SerializeField, Tooltip("打ち出す力")] private List<GameObject> _collisions = default;
    [SerializeField, Tooltip("大砲の玉（見えない攻撃判定）")] private Rigidbody _dymmyCanonBall = default;
    [SerializeField, Tooltip("大砲の玉　開始位置")] private Transform _dymmyCanonBallStartPos = default;
    [SerializeField, Tooltip("大砲の発射音")] private AudioSource _soundCannonShot = null;
    [SerializeField, Tooltip("大砲の駆動音")] private AudioSource _soundCannonMove = null;
    
    private bool _isInitialize = false;
    private bool _isShot = false;
    private Sequence _sequence = null;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    private void Awake(){
        _stage.AddOnInitialize(Initialize);
    }
    private void Initialize()
    {
        if(_isInitialize)
            return;
        _isInitialize = true;

        _childTrigger.AddCallbackOnCollisionEnter(TryShotCannon);
        _dymmyCanonBall.gameObject.SetActive(false);
    }

    private void OnDisable() {
        if(_sequence != null)
            _sequence.Kill();
    }
    // ---------- Public関数 ----------
    // ---------- Private関数 ----------
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
        Rigidbody rigidbody = human.GetParts(HumanParts.waist).GetRigidbody();
        rigidbody.velocity = Vector3.zero;
        rigidbody.isKinematic = true;
        rigidbody.useGravity = false;
        human.gameObject.SetActive(false);

        Sequence sequence = DOTween.Sequence();

        sequence.AppendCallback(()=>{ _soundCannonMove.PlayOneShot(_soundCannonMove.clip); });
        sequence.Append(_cannnonBody.DOLocalRotate(new Vector3(0, 90, 0), _rollDuration).SetEase(Ease.InOutBack).OnComplete(()=>
        {
            if(_soundCannonMove != null)
                _soundCannonMove.Stop();
        }));
        sequence.AppendInterval(_shotDuration);
        // 発射あ！！
        sequence.AppendCallback(()=>{
            _effectShot.Play();
            // human.GetParts(HumanParts.waist).GetRigidbody().velocity = _shotVelocity;
            _dymmyCanonBall.velocity = _shotVelocity;

            human.gameObject.SetActive(true);
            _dymmyCanonBall.gameObject.SetActive(true);
            // _dymmyCanonBall.transform.parent = human.GetParts(HumanParts.waist).transform;
            human.transform.parent = _dymmyCanonBall.transform;
            human.transform.localPosition = Vector3.zero;
            _dymmyCanonBall.transform.parent = _dymmyCanonBallStartPos;
            _dymmyCanonBall.gameObject.layer = human.GetParts(HumanParts.waist).gameObject.layer;
            _dymmyCanonBall.transform.localPosition = Vector3.zero;

            for(int i = 0; i < _collisions.Count; i++)
            {
                int index = i;
                _collisions[index].SetActive(false);
            }

            _soundCannonShot.PlayOneShot(_soundCannonShot.clip);
        });
        sequence.AppendInterval(0.3f);
        sequence.AppendCallback(()=>{
            human.transform.parent = GameDataManager.GetStage().transform;
            _dymmyCanonBall.gameObject.SetActive(false);
            human.OnBreak();
        });
        sequence.AppendInterval(0.5f);
        sequence.AppendCallback(()=>{
            for(int i = 0; i < _collisions.Count; i++)
            {
                int index = i;
                _collisions[index].SetActive(true);
            }
        });
        sequence.AppendInterval(1f);
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
