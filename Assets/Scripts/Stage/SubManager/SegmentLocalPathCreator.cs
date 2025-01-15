using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;

public class SegmentLocalPathCreator : MonoBehaviour
{
    // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    [SerializeField, Tooltip("パス")] private CinemachineSmoothPath _movePath = default;
    [SerializeField, Tooltip("参照する区画")] private EndlessBattleSegment _segment = default;
    [SerializeField, Tooltip("参照パス最小Index")] private int _refPathMin = 0;
    [SerializeField, Tooltip("参考パス最大Index、0未満なら全て参照")] private int _refPathMax = -1;
    [SerializeField, Tooltip("移動パス")] private List<EndlessBattlePath> _pathList = null;
    [SerializeField, Tooltip("パスの反転をするか")] private bool _isReverth = false;

    private List<CinemachineSmoothPath.Waypoint> _wayPointList = null;
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    private void Start(){
        _wayPointList = new List<CinemachineSmoothPath.Waypoint>(_movePath.m_Waypoints);
        // パスの追加
        _wayPointList = CreateWaypointToTransform(GetPathListTransform());
        _movePath.m_Waypoints = _wayPointList.ToArray();

        this.transform.position = GetPathListTransform()[0].position;
    }

    private void Update(){
        
    }
    // ---------- Public関数 -------------------------
    // ---------- Private関数 ------------------------
    // セグメントの方にあるパスリストを、使える形に変換
    private List<CinemachineSmoothPath.Waypoint> CreateWaypointToTransform(List<Transform> transforms)
    {
        List<CinemachineSmoothPath.Waypoint> retList = new List<CinemachineSmoothPath.Waypoint>();
        for(int i = 0; i < transforms.Count; i++)
        {
            CinemachineSmoothPath.Waypoint waypoint = CreateWaypointToTransform(transforms[i]);
            retList.Add( waypoint );
        }
        return retList;
    }
    private CinemachineSmoothPath.Waypoint CreateWaypointToTransform(Transform transforms)
    {
        CinemachineSmoothPath.Waypoint waypoint = new CinemachineSmoothPath.Waypoint();
        Vector3 pos = transforms.position;
        waypoint.position = pos;
        return waypoint;
    }

    private List<Transform> GetPathListTransform()
    {
        List<Transform> ret = _segment.GetPathListTransform();
        int min = _refPathMin;
        int max = _refPathMax;
        if(_refPathMax <= 0)
            max = ret.Count + _refPathMax;
        int count = max - min;

        // Debug.Log("ぎゃあああん:" + ret.Count + ", " + _refPathMin + ", " + _refPathMax + ", " + max + ", " + count);

        ret = ret.GetRange(0, count);

        if( _isReverth )
            ret.Reverse();

        return ret;
    }
}
