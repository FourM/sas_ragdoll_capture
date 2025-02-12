using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SimpleMoveGimmick : MonoBehaviour
{
    [SerializeField] private InterfaceReference<IInitializer> _iInitializer;
    [SerializeField, Tooltip("ディレイ")] private float _deray = 0f;
    [SerializeField, Tooltip("初期位置(Transform)")] private Transform _initPos = default;
    [SerializeField, Tooltip("初期位置(Vector3)")] private Vector3 _initPosVec3 = default;
    [SerializeField, Tooltip("目標位置(Transform)")] private Transform _targetPos = default;
    [SerializeField, Tooltip("目標位置(Vector3)")] private Vector3 _targetPosVec3 = default;
    [SerializeField, Tooltip("時間")] private float _duration = 0.5f;
    [SerializeField, Tooltip("イージング")] private Ease _ease  = Ease.OutCubic;
    [SerializeField, Tooltip("ベロシティ")] private Vector3 _velocity = default;
    [SerializeField, Tooltip("リジッドボディ")] private Rigidbody _rigidBody = default;

    Transform selfTransform = null;

    private void Awake()
    {
        _iInitializer.Value.AddOnInitialize(Initialize);
    }
    private void Initialize()
    {
        selfTransform = this.transform;
        if(_initPos != null)
        {
            if(_initPos != selfTransform)
                selfTransform.position = _initPos.position;
        }
        else
        {
            selfTransform.localPosition = _initPosVec3;
        }
    }

    public void StartMove()
    {
        StartCoroutine(Move());
    }

    private IEnumerator Move()
    {
        yield return new WaitForSeconds(_deray);

        if(_rigidBody == null)
        {
            Vector3 targetPos = Vector3.zero;
            if(_targetPos != null)
            {
                targetPos = _targetPos.position;
                selfTransform.DOMove(targetPos, _duration).SetEase(_ease).SetLink(this.gameObject);
            }
            else
            {
                targetPos = _targetPosVec3;
                selfTransform.DOLocalMove(targetPos, _duration).SetEase(_ease).SetLink(this.gameObject);
            }
        }
        else
        {
            _rigidBody.isKinematic = false;
            _rigidBody.useGravity = true;
            _rigidBody.velocity = _velocity;
        }
    }
}
