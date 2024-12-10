using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tucan : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    [SerializeField, Tooltip("トリガー")] private ChildTrigger _childTrigger;
    [SerializeField, Tooltip("鳥アニメーター")] private Animator _animator = null;
    [SerializeField, Tooltip("鳥の攻撃判定")] private GameObject _attackRect = default; 
    [SerializeField, Tooltip("猛獣")] private Transform _bird = default;
    private bool _isAttack = false;
    private Transform _target = null;
    // private Human _human = null;
    // ---------- プロパティ ----------
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    // ---------- Public関数 ----------
    // ---------- Private関数 ----------
    private void Start()
    {
        _childTrigger.AddCallbackOnTriggerEnter(TryStartAttack);
        _attackRect.SetActive(false);
    }
    private void FixedUpdate()
    {
        if(_isAttack && _target != null)
        {
            Vector3 direction = _target.position - _bird.position;
            direction.y = 0;

            var lookRotation = Quaternion.LookRotation(direction, Vector3.up);
            _bird.rotation = Quaternion.Lerp(_bird.rotation, lookRotation, 0.1f);
        }
    }

    private void TryStartAttack(Collider other)
    {
        Human human = GameDataManager.TryGetHuman(other.gameObject);
        if(human == null)
            return;
        Transform target = human.GetParts(HumanParts.head).transform; 
        if(_target == null )
            return;
        _target = target;

        if(_isAttack)
            return;
        _isAttack = true;

        _animator.SetBool("isAttacking", true);
        _attackRect.SetActive(true);
    }
}
