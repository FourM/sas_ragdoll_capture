using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class EndlessBattleSubManager : StageSubManager
{
    // ---------- 定数宣言 ----------------------------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------------------------
    // ---------- プロパティ --------------------------
    [SerializeField, Tooltip("hoge")] private List<EndlessBattleSegment> _stagePrefabs = default;
    [SerializeField, Tooltip("地面")] private Transform _ground = default;
    [SerializeField, Tooltip("セグメント生成の先頭")] private Transform _segmentCreateHead = default;
    [SerializeField, Tooltip("パス")] private CinemachineSmoothPath _playerMovePath = default;
    private List<EndlessBattleSegment> _segmentList = default;
    private int _instanceSegmentIndex = 0;
    private float _nextInstancePos = 0;
    private bool _initNextInstancePos = false;
    private Player _player = null;
    float _nextSegmentPos = 0f;
    private List<CinemachineSmoothPath.Waypoint> _wayPointList = null;
    private float _beforePathLength = 0;
    private int _initPathNum = 0; // 初期のパス数
    private bool _firstUpdateSegment = false;   // 一番最初のセグメント更新
    private EndlessBattleSegment _currentSegment = null; // 今のセグメント
    private int _currentSegmentIndex = 0; // 今のセグメントインデックス番号
    private int _laps = 0; // デバッグ用：周回回数
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    protected override void InitializeUnique(){

        _segmentList = new List<EndlessBattleSegment>();

        // プレイヤー周りの設定
        _player = GameDataManager.GetPlayer();

        _player.GetMovePath().m_Path = _playerMovePath;

        CinemachineSmoothPath.Waypoint initWaypoint = new CinemachineSmoothPath.Waypoint();
        initWaypoint.position = _player.transform.position;
        // 初期パスの位置補正。一番最初のパス位置はプレイヤーの位置にする
        _playerMovePath.m_Waypoints[0] = initWaypoint;
        // 初期パスの位置補正。２個目のパス位置は初期プレイヤー位置からの相対位置
        _initPathNum = _playerMovePath.m_Waypoints.Length;
        for(int i = 1; i < _initPathNum; i++)
        {
            _playerMovePath.m_Waypoints[i].position += _player.GetInitPos();
        }
        _wayPointList = new List<CinemachineSmoothPath.Waypoint>(_playerMovePath.m_Waypoints);

        // Debug.Log("_initPathNum" + _initPathNum);

        // ステージ生成
        int _instanceSegmentIndex = 0;
        int j = 0;
        while( (_playerMovePath.PathLength <= 100f || _playerMovePath.m_Waypoints.Length <= 8 ) && j < 15 )
        {
            EndlessBattleSegment segment = InstantiateSegment();
            if(_segmentList.Count == 1)
                _nextSegmentPos += segment.GetLength();
            j++;
            // if( _playerMovePath.m_Waypoints.length)
            //     _nextInstancePos = segment.GetLength();
            // _playerMovePath.GetPathLength();
        }
        Debug.Log("DistanceCacheIsValid:" + _playerMovePath.DistanceCacheIsValid() + ", m_Waypoints.Length" + _playerMovePath.m_Waypoints.Length);

        _currentSegmentIndex = 0;
        _currentSegment = _stagePrefabs[_currentSegmentIndex];
    }

    protected override void UpdateUnique()
    {
        // 判定地点を超えた
        if( _nextSegmentPos < _player.GetMovePath().m_Position)
        {
            _player.SetState(PlayerState.battle);
        }

        // if( _nextInstancePos <= _player.transform.position.z )
        if(JudgeUpdateSegment())
        {
            // Debug.Log("セグメント更新！！");
            EndlessBattleSegment segment = InstantiateSegment();
            _nextInstancePos += segment.GetLength();

            // 後ろのセグメントを消す
            DeleteSegment();
        }
        Vector3 pos = _player.transform.position;
        pos += _player.transform.forward * 30f;
        pos.y = -10f;
        _ground.transform.position = pos;
        Vector3 angle = _ground.transform.eulerAngles;
        angle.y = _player.transform.eulerAngles.y;
        _ground.transform.eulerAngles = angle;

        

        if(_currentSegment != null )
        {
            // Debug.Log("");
            if(_currentSegment.isAllKill())
                _player.SetState(PlayerState.move);
        }
        else
        {

        }
    }
    // ---------- Public関数 -------------------------
    // ---------- Private関数 ------------------------
    // セグメント生成
    private EndlessBattleSegment InstantiateSegment()
    {
        _playerMovePath.InvalidateDistanceCache();

        EndlessBattleSegment segment = Instantiate(_stagePrefabs[_instanceSegmentIndex]);
        segment.transform.parent = this.transform;
        segment.transform.position = _segmentCreateHead.position;
        segment.transform.eulerAngles = _segmentCreateHead.eulerAngles;
        segment.Initialize();
        IndexNext();

        // パスの追加
        _wayPointList.AddRange(CreateWaypointToTransform(segment.GetPathList()));
        _playerMovePath.m_Waypoints = _wayPointList.ToArray();

        // このセグメントを増やしたことで増えたPathLengthを覚えさせておく
        segment.PathLength = GetSubPathLength();

        // 以降のセグメント生成の向き設定
        _segmentCreateHead.eulerAngles += segment.GetNextSegmentAddAngle();
        _segmentCreateHead.position += _segmentCreateHead.forward * segment.GetLength();

        segment.gameObject.name = segment.gameObject.name + "_" + _laps;

        _segmentList.Add(segment);

        return segment;
    }
    
    // セグメント更新判定。trueなら新しいセグメントを追加生成&今一番最初のセグメントを消す
    private bool JudgeUpdateSegment()
    {
        // 一番最初のセグメントを消しても経過したパスが一定個数以上残りそうなら、最初のセグメントを消す
        float PlayerPos = _player.GetMovePath().m_Position;

        // 判定地点を超えた
        if( _nextSegmentPos < PlayerPos)
        {
            float length = 0f;
            int index = 1;
            int pathNum = 0;
            int doUpdatePathNum = 4;    // 経過したパスが何個残るならセグメントの更新を実行するか
            while(length <= _nextSegmentPos)
            {
                EndlessBattleSegment segment = _segmentList[index];
                length += segment.PathLength;
                pathNum += segment.GetPathList().Count;
                index++;

                if( doUpdatePathNum <= pathNum )
                    return true;

                if(_segmentList.Count <= index)
                {
                    Debug.LogError("セグメントの更新がされないよ");
                    break;
                }
            }
            // 判定地点を更新
            if(index < _segmentList.Count)
            {
                _nextSegmentPos += _segmentList[index].PathLength;
                _currentSegment = _segmentList[index];
                Debug.Log("_currentSegment更新：" + _currentSegment.name + ", " + index);
            }
            else
                Debug.LogError("終点にいるのにセグメントの更新がされないよ！？");
        }
        return false;
    }
    private void DeleteSegment()
    {
        _playerMovePath.InvalidateDistanceCache();
        
        // 最初のセグメント更新なら、実行前から設定していたパスも消す
        if(!_firstUpdateSegment)
        {
            _wayPointList.RemoveRange(0, _initPathNum);
            _firstUpdateSegment = true;
        }
        // 現時点で最初のセグメントを、パスを消してから消す
        EndlessBattleSegment segment = _segmentList[0];
        int pathNum = segment.GetPathList().Count;
        _wayPointList.RemoveRange(0, pathNum);
        _playerMovePath.m_Waypoints = _wayPointList.ToArray();
        
        // プレイヤーのパス移動位置を、消したパスの長さ分だけ戻す（これで今の位置を保持する）
        // float length = segment.PathLength;
        float length = GetSubPathLength();
        // Debug.Log("length:" + length);
        _player.GetMovePath().m_Position += length;

        // Debug.Log("わんたそ3");
        segment.DestroyThis();
        _segmentList.RemoveAt(0);
    }
    private void IndexNext(){
        _instanceSegmentIndex ++;
        _instanceSegmentIndex %= _stagePrefabs.Count;
        if(_instanceSegmentIndex == 0)
        {
            _laps++;
        }
    }
    // セグメントの方にあるパスリストを、使える形に変換
    private List<CinemachineSmoothPath.Waypoint> CreateWaypointToTransform(List<Transform> transforms)
    {
        List<CinemachineSmoothPath.Waypoint> retList = new List<CinemachineSmoothPath.Waypoint>();
        for(int i = 0; i < transforms.Count; i++)
        {
            CinemachineSmoothPath.Waypoint waypoint = new CinemachineSmoothPath.Waypoint();
            Vector3 pos = transforms[i].position;
            pos.y += _player.GetInitPos().y;
            waypoint.position = pos;
            retList.Add( waypoint );
        }
        return retList;
    }

    // パスの長さ差分取得&更新
    private float GetSubPathLength()
    {
        float ret = _playerMovePath.PathLength - _beforePathLength;
        _beforePathLength = _playerMovePath.PathLength;
         
        return ret;
    }
}
