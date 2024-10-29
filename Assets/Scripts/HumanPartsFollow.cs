using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// ラグドールからの起きあがり処理。各人体パーツを、ずっとアニメーションしてるゴーストに合わせる
/// </summary>
public class HumanPartsFollow : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("Human")] private Human _human = null;
    [SerializeField, Tooltip("ベースの位置")] private Transform _base = null;
    [SerializeField, Tooltip("腰の位置")] private Transform _waist = null;
    [SerializeField, Tooltip("頭")] private Transform _head = null;
    [SerializeField, Tooltip("首")] private Transform _neck = null;
    [SerializeField, Tooltip("追従パーツ")] private List<Rigidbody> _parts = null;
    [SerializeField, Tooltip("追従対象")] private HumanPartsFollow _followTarget = null;
    [SerializeField, Tooltip("元の位置に戻る力係数")] private float _followForceCoefficient = 10.0f;
    [SerializeField, Tooltip("元の位置に戻る力係数")] private float _followForceCoefficient2 = 100.0f;
    [SerializeField, Tooltip("元の角度に戻る力係数")] private float _followTorqueCoefficient = 100.0f;
    [SerializeField, Tooltip("元の角度に戻る力係数")] private Transform _debugPos = null;
    private bool _isFollow = false;
    private Vector3 _basePos = default;
    private float _followTime = 0f;
    private float _beforeRealTime = 0f;
    private float _realTime = 0f;
    private float _followStartTime = 0f;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    private void Start()
    {
        _basePos = _base.position;
    }
    private void FixedUpdate(){
        if(!_isFollow)
            return;
        _followStartTime -= Time.deltaTime;
        _followTime += Time.deltaTime * 0.05f + _followTime * 0.02f;
        _realTime += Time.deltaTime;
        _beforeRealTime = _realTime;

        if(_followStartTime <= 0f)
            _followStartTime = 0f;
        if(1f <= _followTime)
            _followTime = 1f;

        if(0f < _followStartTime )
            return;
        if(_parts == null)
            return;
        if(_followTarget == null)
            return;

        for(int i = 0; i < _parts.Count; i++)
        {
            int index = i;
            Rigidbody parts = _parts[index];
            Rigidbody followParts = _followTarget.GetParts(index);
            if( true )
            {
                Vector3 followForce = GetSubPos(index);
                if( parts.transform == _head || parts.transform == _waist || parts.transform == _neck)
                {
                    followForce *= _followForceCoefficient2 * _followTime;

                    if(parts.transform == _head || parts.transform == _neck)
                    {
                        Vector3 angle = parts.transform.eulerAngles * (1f - _followTime);
                        angle += followParts.transform.eulerAngles * _followTime;
                    }
                }
                else
                {
                    followForce *= _followForceCoefficient * _followTime;
                }

                if(parts.transform == _neck)
                    return;

                Vector3 followTorque = GetSubAngle(index);
                
                followTorque *= _followTorqueCoefficient * _followTime;

                // Debug.Log("追従してるよ！:" + parts.gameObject.name + ", " + followForce + ", " + followTorque);

                Vector3 movePos = parts.transform.position + GetSubPos(index) * _followTime * 0.2f;

                // 下から上にかかる力が掛かっているなら、下方向へ行く補正をなくす
                if( 0f < parts.velocity.y )
                {
                    if(followForce.y < 0)
                        followForce.y = 0;
                    if( movePos.y < parts.transform.position.y )
                        movePos.y = parts.transform.position.y;
                }
            
                parts.AddForce(followForce, ForceMode.Acceleration);
                parts.MovePosition(movePos);
                // parts.AddTorque(followTorque, ForceMode.Acceleration);

                // Vector3 angle = parts.transform.eulerAngles * (1f - _followTime);
                // angle += followParts.transform.eulerAngles * _followTime;

                // Quaternion rotate = Quaternion.Euler(angle.x, angle.y, angle.z);
                // parts.MoveRotation(rotate);
            }
            else
            {
                parts.transform.localPosition = followParts.transform.localPosition;
                parts.transform.localEulerAngles = followParts.transform.localEulerAngles;
                parts.velocity = Vector3.zero;
                parts.angularVelocity = Vector3.zero;
            }
        }
    }
    private Vector3 GetSubPos(int index)
    {
        Vector3 localPos = _parts[index].transform.position - _basePos;
        Vector3 followLocalPos = _followTarget.GetParts(index).transform.position - _followTarget.GetFollowBase().position;

        return followLocalPos - localPos;
    }
    private Vector3 GetSubAngle(int index)
    {
        // Vector3 localAngle = _parts[index].transform.eulerAngles - _base.eulerAngles;
        // Vector3 followAngle = _followTarget.GetParts(index).transform.eulerAngles - _followTarget.GetFollowBase().eulerAngles;

        Vector3 localAngle = _parts[index].transform.localEulerAngles;
        Vector3 followAngle = _followTarget.GetParts(index).transform.localEulerAngles;

        Vector3 subAngle = followAngle - localAngle;

        if( 180f < subAngle.x)
            subAngle.x -= 360f;
        if( subAngle.x < -180f )
            subAngle.x += 360f;
        if( 180f < subAngle.y)
            subAngle.y -= 360f;
        if( subAngle.y < -180f )
            subAngle.y += 360f;
        if( 180f < subAngle.z)
            subAngle.z -= 360f;
        if( subAngle.z < -180f )
            subAngle.z += 360f;

        return subAngle;
    }
    // ---------- Public関数 ----------
    public void SetFollowBasePos( Vector3 pos )
    { 
        // すでに起きあがりが始まっているなら、Y軸以外の中心地点を変えない。
        if(0.3f <= _realTime || _human.IsEnableAnimation())
        {
           _basePos.y = pos.y;
            return;
        }
        _basePos = pos; 
        if(_debugPos != null)
        {
            _debugPos.position = pos;
        }

        // 10 , 1 -> 10を1にする
        // Vector3 subPos = _basePos - _base.position;
        // _base.position = _basePos;
        // _waist.position += subPos;
    }

    public Transform GetFollowBase(){ return _base; }
    public Rigidbody GetParts(int index){ return _parts[index]; }
    public void SetIsFollow(bool isFollow)
    { 
        // 立ち上がらない設定なら何もしない
        if(PlayerPrefs.GetInt("is_Recovery") == 0)
            return;

        if(_isFollow == isFollow )
            return;
        if(isFollow)
        {
            
        }
        else
        {
            _followStartTime = 0.1f;
            _followTime = 0f;
            _realTime = 0;
        }
        _isFollow = isFollow; 
    }
    public bool IsFollow(){ return _isFollow; }
    // ---------- Private関数 ----------
}
