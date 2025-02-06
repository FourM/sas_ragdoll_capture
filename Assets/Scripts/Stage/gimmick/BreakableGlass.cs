using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableGlass : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField] private InterfaceReference<IInitializer> _iInitializer;
    [SerializeField, Tooltip("壊れる前のガラス")] private ChildTrigger _preBreakGrass = default;
    [SerializeField, Tooltip("壊れたガラス")] private GameObject _breakParts = default;
    private Rigidbody[] _rigidBodies = default;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    private void Awake()
    {
        _iInitializer.Value.AddOnInitialize(Initialize);
    }
    // ---------- Public関数 ----------
    // ---------- Private関数 ----------
    private void Initialize()
    {
        _rigidBodies = _breakParts.GetComponentsInChildren<Rigidbody>();
        _preBreakGrass.AddCallbackOnCollisionEnter((Collision collision)=>
        {
            _preBreakGrass.gameObject.SetActive(false);
            _breakParts.SetActive(true);

            StartCoroutine(SetPartsVelocity(collision.relativeVelocity));
        });
        _preBreakGrass.gameObject.SetActive(true);
        _breakParts.SetActive(false);
    }

    private IEnumerator SetPartsVelocity(Vector3 velocity)
    {
        // Debug.Log("破裂！" + _rigidBodies.Length + ", " + velocity);
        for(int i = 0; i < _rigidBodies.Length; i++)
        {
            Vector3 randomDirection = Random.onUnitSphere;
            velocity += randomDirection;
            _rigidBodies[i].velocity = velocity;
            // yield return null;
        }
        yield return null;
    }
}
