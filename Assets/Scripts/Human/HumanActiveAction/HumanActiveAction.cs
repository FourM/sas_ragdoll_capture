using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// エンドレスバトル　敵の能動的行動　継承元
/// </summary>
public abstract class HumanActiveAction : MonoBehaviour
{
    [SerializeField, Tooltip("能動的アクション")] private RuntimeAnimatorController _activeActionAnimation = null;

    protected Human _human = null;
    private bool _isChangeAnimation = true;
    // アニメーションを切り替えるか
    public bool isChangeAnimation{ get{ return _isChangeAnimation; } protected set{ _isChangeAnimation = value; } }

    protected Transform MoveTransform{ get{
        return GetMoveTransform();
    } }
    protected Vector3 MoveForward{ get{
        if(_human.IsEnableAnimation())
            return MoveTransform.forward;
        else
            return MoveTransform.up;
    } }

    public void Iniiialize()
    {
        IniiializeUnique();
    }

    public void SetHuman(Human human){ _human = human; }
    public void StartActiveAction()
    {
        if(_activeActionAnimation != null && _isChangeAnimation && !_human.IsDead())
        {
            _human.SetAnimatorController(_activeActionAnimation, false);
            if(_human.IsGround())
                _human.EnableAnimation();
        }
        StartActiveActionUnique();
    }
    // 能動的行動が一時停止した時の処理
    public void Pause()
    {
        PauseUnique();
    }
    public void UpdateActiveAction()
    {
        UpdateActiveActionUnique();
    }
    public void FixedUpdateActiveAction()
    {
        FixedUpdateActiveActionUnique();
    }

    protected virtual void IniiializeUnique(){}
    protected virtual void StartActiveActionUnique(){}
    protected virtual void UpdateActiveActionUnique(){}
    protected virtual void FixedUpdateActiveActionUnique(){}
    protected virtual void PauseUnique(){}
    
    private Transform GetMoveTransform()
    {
        if(_human == null)
        {
            Debug.Log("humanが未設定なんだけどおお！？");
            return null;
        }
        // Ragdoll無効中ならこれを返す
        if(_human.IsEnableAnimation())
            return this.transform;
        // Ragdoll有効中なら腰の位置を返す
        else
            return _human.GetParts(HumanParts.waist).transform;
    }
}
