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
    float _nextSegmentPosZ = 0f;
    float _nextSegmentPos = 0f;
    private List<CinemachineSmoothPath.Waypoint> _wayPointList = null;
    private float _beforePathLength = 0;
    private int _initPathNum = 0; // 初期のパス数
    private bool _firstUpdateSegment = false;   // 一番最初のセグメント更新
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

        // ステージ生成
        int _instanceSegmentIndex = 0;
        while( _playerMovePath.PathLength <= 100f && 8 <= _playerMovePath.m_Waypoints.Length )
        {
            EndlessBattleSegment segment = InstantiateSegment();
            if(_segmentList.Count == 1)
                _nextSegmentPos += segment.GetLength();
            // if( _playerMovePath.m_Waypoints.length)
            //     _nextInstancePos = segment.GetLength();
        }
    }

    protected override void UpdateUnique()
    {
        // if( _nextInstancePos <= _player.transform.position.z )
        if(JudgeUpdateSegment())
        {
            EndlessBattleSegment segment = InstantiateSegment();
            _nextInstancePos += segment.GetLength();

            // 後ろのセグメントを消す
            DeleteSegment();
        }
        Vector3 pos = _player.transform.position;
        pos.z += 30f;
        pos.y = -10f;
        pos.x = 0f;
        _ground.transform.position = pos;
    }
    // ---------- Public関数 -------------------------
    // ---------- Private関数 ------------------------
    private EndlessBattleSegment InstantiateSegment()
    {
        EndlessBattleSegment segment = Instantiate(_stagePrefabs[_instanceSegmentIndex]);
        segment.transform.parent = this.transform;
        segment.transform.position = _segmentCreateHead.position;
        segment.transform.eulerAngles = _segmentCreateHead.eulerAngles;
        // segment.transform.localPosition = new Vector3(0, 0, _nextSegmentPosZ);
        segment.Initialize();
        _nextSegmentPosZ += segment.GetLength();
        IndexNext();

        // パスの追加
        _wayPointList.AddRange(CreateWaypointToTransform(segment.GetPathList()));
        _playerMovePath.m_Waypoints = _wayPointList.ToArray();

        // このセグメントを増やしたことで増えたPathLengthを覚えさせておく
        segment.PathLength = GetSubPathLength();

        // 以降のセグメント生成の向き設定
        _segmentCreateHead.eulerAngles += segment.GetNextSegmentAddAngle();
        _segmentCreateHead.position += _segmentCreateHead.forward * segment.GetLength();

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

                if( doUpdatePathNum <= pathNum )
                    return true;

                index++;
                if(_segmentList.Count <= index)
                {
                    Debug.LogError("セグメントの更新がされないよ");
                    break;
                }
            }
            // 判定地点を更新
            if(index < _segmentList.Count)
                _nextSegmentPos += _segmentList[index].PathLength;
            else
                Debug.LogError("おやあ？");
        }
        return false;
    }
    private void DeleteSegment()
    {
        // 最初のセグメント更新なら、実行前から設定していたパスも消す
        if(!_firstUpdateSegment)
        {
            _wayPointList.RemoveRange(0, _initPathNum);
            _firstUpdateSegment = true;
        }
        // 一番最初のセグメント分のパスを消してから、一番最初のセグメントを消す
        EndlessBattleSegment segment = _segmentList[0];
        int pathNum = segment.GetPathList().Count;
        _wayPointList.RemoveRange(0, pathNum);
        _playerMovePath.m_Waypoints = _wayPointList.ToArray();

        // プレイヤーのパス移動位置を、消したパスの長さ分だけ戻す（これで今の位置を保持する）
        float length = segment.PathLength;
        _player.GetMovePath().m_Position -= length;

        Destroy(segment.gameObject);
        _segmentList.RemoveAt(0);
    }
    private void IndexNext(){
        _instanceSegmentIndex ++;
        _instanceSegmentIndex %= _stagePrefabs.Count;
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
