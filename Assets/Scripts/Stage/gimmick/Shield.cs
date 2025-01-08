using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class Shield : CatchableObj
{
    // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    [SerializeField, Tooltip("盾と腕のSpringJoint")] private SpringJoint _springjoint;
    [SerializeField, Tooltip("腕からの相対位置")] private Vector3 _localPos;
    [SerializeField, Tooltip("アニメーションコントローラー")] private RuntimeAnimatorController _guardAnim = null;

    private Human _human;
    private HumanChild _humanChild;
    private Tween _guardEndTween = null;
    private UnityEvent _OnCompleteGuardEnd;
    private RuntimeAnimatorController _beforeAnimator = null;
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    public void Update()
    {
        if(_human == null)
            _guardEndTween.Kill();
    }
    // ---------- Public関数 -------------------------
    public void SetUp(Transform parent, Human human, HumanChild humanChild)
    {
        _human = human;
        _humanChild = humanChild;

        _human.SetHaveShield(this);
        _human.AddOnCatch(()=>
        {
            RereaseShield();
        });

        Vector3 ang = this.transform.localEulerAngles;

        if(humanChild == null)
        {
            Debug.Log("腕が見つけられないよ");
            return;
        }

        if(_springjoint != null)
            _springjoint.connectedBody = humanChild.GetRigidbody();
        // 取った対象からの相対位置を設定。
        // _springjoint.connectedAnchor = pos;

        Transform hand = null;
        hand = humanChild.transform;
        if(hand != null)
            this.transform.parent = hand;

        _human.AddOnBreakCallback(()=>
        {
            this.transform.parent = parent;
            this.GetRigidbody().useGravity = true;
            this.GetRigidbody().isKinematic = false;
        });

        this.transform.localPosition = _localPos;
        this.transform.localEulerAngles = ang;

        this.gameObject.layer = _human.GetChildLayer();
    }
    public void CanselGuardEndTween()
    { 
        _guardEndTween?.Kill(); 
    }
    // ガードの構えを終えた時のコールバック設定
    public void AddOnCompleteGuardEnd( UnityAction onCompleteGuardEnd)
    {
        if(_OnCompleteGuardEnd == null)
            _OnCompleteGuardEnd = new UnityEvent();
        _OnCompleteGuardEnd.AddListener(onCompleteGuardEnd);
    }
    // ---------- Private関数 ------------------------
    // これが捕まった時の処理
    protected override void OnCatchUnique()
    {
        // 今のアニメーションを記憶
        _beforeAnimator = _human.GetCurrentAnimatorController();

        // Humanにガードアニメーションする
        _human.EnableAnimation();
        _human.SetAnimatorController(_guardAnim);

        // Humanをプレイヤーの方に向ける
        Vector3 lookPos = GameDataManager.GetPlayer().transform.position;
        lookPos.y = _human.transform.parent.position.y;
        _human.transform.parent.LookAt(lookPos);

        // 盾もプレイヤーの方に向ける
        lookPos.y = this.transform.position.y;
        this.transform.LookAt(lookPos);

        // 盾の位置をプレイヤーの前に置く
        this.transform.position = _human.transform.parent.position;
        this.transform.position += this.transform.forward * 1f;
        this.transform.position += this.transform.up * 1.0f;
        this.transform.localEulerAngles += new Vector3(0, 180, 0); 

        // ガードアニメーション終了後の処理
        _guardEndTween = DOVirtual.DelayedCall(0.5f, ()=>
        {
            // 直前のアニメーションに戻す
            _human.SetAnimatorController(_beforeAnimator);
        }).SetLink(this.gameObject).OnComplete(()=>{_OnCompleteGuardEnd?.Invoke();});

        RereaseShield();
    }
    private void RereaseShield()
    {
        // Humanから盾を解除する
        _human.SetHaveShield(null);
        GetRigidbody().constraints = RigidbodyConstraints.None;
        GetRigidbody().isKinematic = false;
        GetRigidbody().useGravity = true;
        this.transform.parent = _human.transform.parent;
    }
}