using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 大砲で打ち上げる
/// </summary>
public class Stage033SubManager : StageSubManager
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("大砲")] private Transform _cannnonBody;
    [SerializeField, Tooltip("トリガー")] private ChildTrigger _childTrigger;
    [SerializeField, Tooltip("時間")] private float _standbyDuration;
    [SerializeField, Tooltip("時間")] private float _waitDuration;
    [SerializeField, Tooltip("時間")] private float _shotDuration;
    [SerializeField, Tooltip("時間")] private ParticleSystem _effectShot;
    [SerializeField, Tooltip("打ち出す力")] private Vector3 _shotVelocity;
    [SerializeField, Tooltip("大砲の壁")] private List<GameObject> _collisions = default;
    [SerializeField, Tooltip("大砲の玉（見えない攻撃判定）")] private Rigidbody _dymmyCanonBall = default;
    [SerializeField, Tooltip("大砲の玉 開始位置")] private Transform _dymmyCanonBallStartPos = default;
    [SerializeField, Tooltip("大砲の発射音")] private AudioSource _soundCannonShot = null;
    [SerializeField, Tooltip("大砲の駆動音")] private AudioSource _soundCannonMove = null;
    private bool _isShot = false;
    private Sequence _sequence = null;
    private float _cannnonBodyDefaultScale = 1f;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    private void OnDisableUnique() {
        if(_sequence != null)
            _sequence.Kill();
    }
    // ---------- Public関数 ----------
    // ---------- Private関数 ----------
    protected override void InitializeUnique()
    {
        _cannnonBodyDefaultScale = _cannnonBody.localScale.x;
        _childTrigger.AddCallbackOnCollisionEnter(TryShotCannon);
        _dymmyCanonBall.gameObject.SetActive(false);
        // Debug.Log("1");
        // Debug.Log("_childTrigger:" + _childTrigger.transform.name);
    }

    // 衝突相手がHumanかの可否と情報を取得
    private void TryShotCannon(Collision collision)
    {
        // Debug.Log("わわおおん");
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

        if(PlayerPrefs.GetInt("Effect_ON", 1) == 1)
            sequence.AppendCallback(()=>{ _soundCannonMove.PlayOneShot(_soundCannonMove.clip); });
        sequence.Append(_cannnonBody.DOScale(new Vector3(0.8f, 1.3f, 1f) * _cannnonBodyDefaultScale, _standbyDuration).SetEase(Ease.InBack));
        sequence.Append(_cannnonBody.DOScale(new Vector3(0.6f, 1.5f, 1f) * _cannnonBodyDefaultScale, _waitDuration).SetEase(Ease.OutQuad));
        sequence.AppendCallback(()=>{
            if(_soundCannonMove != null)
                _soundCannonMove.Stop();
        });
        // sequence.Append(_cannnonBody.DOScale(new Vector3(0.5f, 1.7f, 1f) * _cannnonBodyDefaultScale, _shotDuration * 0.1f).SetEase(Ease.OutQuad));
        // sequence.Append(_cannnonBody.DOScale(Vector3.one * _cannnonBodyDefaultScale, _shotDuration * 0.9f).SetEase(Ease.OutQuad));
        sequence.Append(_cannnonBody.DOScale(Vector3.one * _cannnonBodyDefaultScale, _shotDuration).SetEase(Ease.InBack));
        // 発射あ！！
        sequence.AppendCallback(()=>{
            _dymmyCanonBallStartPos.parent = GameDataManager.GetStage().transform;
            _effectShot.Play();
            human.gameObject.SetActive(true);
            _dymmyCanonBall.gameObject.SetActive(true);
            _dymmyCanonBall.transform.parent = _dymmyCanonBallStartPos;
            _dymmyCanonBall.gameObject.layer = human.GetParts(HumanParts.waist).gameObject.layer;
            _dymmyCanonBall.transform.localPosition = Vector3.zero;
            human.transform.parent = _dymmyCanonBall.transform;
            human.transform.localPosition = Vector3.zero;
            human.SetPos(_dymmyCanonBall.transform.position);
            human.PartsActiion((HumanChild parts)=>{
                parts.GetRigidbody().velocity = Vector3.zero;
                parts.GetRigidbody().useGravity = false;
            });
            human.GetRigidbody().useGravity = false;
            human.GetRigidbody().velocity = Vector3.zero;

            _dymmyCanonBall.velocity = _shotVelocity;

            for(int i = 0; i < _collisions.Count; i++)
            {
                int index = i;
                _collisions[index].SetActive(false);
            }

            if(PlayerPrefs.GetInt("Effect_ON", 1) == 1)
                _soundCannonShot.PlayOneShot(_soundCannonShot.clip);
        });
        sequence.Append(_cannnonBody.DOScale(new Vector3(1.4f, 0.6f, 1f) * _cannnonBodyDefaultScale, _shotDuration * 0.1f).SetEase(Ease.OutQuad));
        sequence.Append(_cannnonBody.DOScale(Vector3.one * _cannnonBodyDefaultScale, 0.2f).SetEase(Ease.InQuad));
        sequence.AppendInterval(0.3f);
        sequence.AppendCallback(()=>{
            // human.transform.parent = GameDataManager.GetStage().transform;
            // _dymmyCanonBall.gameObject.SetActive(false);
            // human.OnBreak();
            human.PartsActiion((HumanChild parts)=>{
                parts.GetRigidbody().useGravity = true;
            });
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
        sequence.AppendCallback(()=>{
            _isShot = false;
        });

    }
}
