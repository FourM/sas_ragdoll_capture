using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;

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
    // private float _nextInstancePos = 0;
    private bool _initNextInstancePos = false;
    private Player _player = null;
    float _nextSegmentPos = 0f;
    private List<CinemachineSmoothPath.Waypoint> _wayPointList = null;
    private float _beforePathLength = 0;
    private int _initPathNum = 0; // 初期のパス数
    private bool _firstUpdateSegment = false;   // 一番最初のセグメント更新
    private EndlessBattleSegment _currentSegment = null; // 今のプレイヤーがいるセグメント
    private EndlessBattleSegment _beforeSegment = null; // 直前にプレイヤーがいたセグメント
    private int _currentSegmentIndex = 0; // 今のセグメントインデックス番号
    private int _laps = 0; // デバッグ用：周回回数
    private ClearLook _carrentClearLook = ClearLook.none;   // 今のセグメントをクリアしたらどこを見るか
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

        _nextSegmentPos = _playerMovePath.PathLength;
        // Debug.Log("_initPathNum" + _initPathNum);

        // ステージ生成
        int _instanceSegmentIndex = 0;
        int j = 0;
        while( (_playerMovePath.PathLength <= 100f || _playerMovePath.m_Waypoints.Length <= 8 ) && j < 15 )
        {
            EndlessBattleSegment segment = InstantiateSegment();
            j++;
        }
        Debug.Log("DistanceCacheIsValid:" + _playerMovePath.DistanceCacheIsValid() + ", m_Waypoints.Length" + _playerMovePath.m_Waypoints.Length);

        _currentSegmentIndex = 0;
        _currentSegment = _stagePrefabs[_currentSegmentIndex];
    }

    protected override void UpdateUnique()
    {
        float playerPos = _player.GetMovePath().m_Position;
        if( _nextSegmentPos < playerPos)
        {
            if(JudgeUpdateSegment())
            {
                // Debug.Log("セグメント更新！！");
                EndlessBattleSegment segment = InstantiateSegment();

                // 後ろのセグメントを消す
                DeleteSegment();
            }
            // 今プレイヤーがいるセグメントを更新
            GetCurrentSegment();
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
            if(GameDataManager.GameState == GameState.main)
            {
                if(_currentSegment.isAllKill())
                {
                    _player.SetState(PlayerState.move);
                    // Debug.Log("今の区画をクリア！" + _currentSegment.gameObject.name);
                    int index = _segmentList.IndexOf(_currentSegment);
                    EndlessBattleSegment segment = _segmentList[index + 1];

                    if(segment != null)
                    {
                        Transform lookAtTarget = segment.GetLookAtTarget();
                        _player.SetLookAtTarget(lookAtTarget);
                    }
                }
                else
                {
                    _player.SetState(PlayerState.battle);
                    Transform lookAtTarget = _currentSegment.GetLookAtTarget();
                    _player.SetLookAtTarget(lookAtTarget);
                }
            }
        }
        else
        {
            // Debug.Log("_currentSegmentがねぇんだぇどおお！？");
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
        _wayPointList.AddRange(CreateWaypointToTransform(segment.GetPathListTransform()));
        _playerMovePath.m_Waypoints = _wayPointList.ToArray();

        // このセグメントを増やしたことで増えたPathLengthを覚えさせておく
        segment.PathLength = GetSubPathLength();

        // 以降のセグメント生成の向き設定
        _segmentCreateHead.eulerAngles = segment.GetNextSegmentPos().eulerAngles;
        _segmentCreateHead.position = segment.GetNextSegmentPos().position;

        segment.gameObject.name = segment.gameObject.name + "_" + _laps;

        // 各パス通過時の設定
        List<EndlessBattlePath> endlessBattlePathList = segment.GetPathList();
        for(int i = 0; i < endlessBattlePathList.Count; i++)
        {
            EndlessBattlePath path = endlessBattlePathList[i];
            path.AddCallbackOnTriggerEnter(OnEnterPass(segment, path));
        }

        _segmentList.Add(segment);
        segment.AddCallbackOnTriggerEnter((Collider collider)=>
        {
            // Debug.Log("トリガーからセグメント更新:" + segment.gameObject.name);
            SetCurrentSegment(segment);
        });

        if(segment.GetLookAtTarget() != null)
        {
            Transform lookAtTarget = segment.GetLookAtTarget();
            Vector3 pos = lookAtTarget.transform.position;
            pos.y = _player.transform.position.y;
            lookAtTarget.transform.position = pos;
        }

        return segment;
    }
    
    // セグメント更新判定。trueなら新しいセグメントを追加生成&今一番最初のセグメントを消す
    // 今プレイヤーがいるセグメントの取得も行う
    private bool JudgeUpdateSegment()
    {
        // 一番最初のセグメントを消しても経過したパスが一定個数以上残りそうなら、最初のセグメントを消す
        float playerPos = _player.GetMovePath().m_Position;

        // 判定地点を超えた
        if( _nextSegmentPos < playerPos)
        {
            float length = 0f;
            int index = 0;
            int pathNum = 0;
            int doUpdatePathNum = 4;    // 経過したパスが何個残るならセグメントの更新を実行するか
            // bool retIsTrue = false;
            while(length <= _nextSegmentPos)
            {
                EndlessBattleSegment segment = _segmentList[index];
                // Debug.Log("判定中。NSPos" + _nextSegmentPos + ", Ppos:" + playerPos + ", length:" + length + ", pathNum:" + pathNum + ", index:" + index + ", PathLength:" + segment.PathLength);
                length += segment.PathLength;

                if(_nextSegmentPos < length)
                {
                    // Debug.Log("判定中。今プレイヤーが足を踏み入れたばかりのセグメントまできた:" + index);
                    break;
                }

                if( 0 < index)
                {
                    pathNum += segment.GetPathListTransform().Count;
                }
                else
                {
                    // Debug.Log("判定中。一番最初のセグメントのパス数はノーカン");
                }
                index++;

                if( doUpdatePathNum <= pathNum )
                {
                    // Debug.Log("セグメント更新判定がTRUE。NSPos:" + _nextSegmentPos + ", Ppos:" + playerPos + ", length:" + length + ", index:" + index + ", PathLength:" + segment.PathLength + ", name:" + _segmentList[index].gameObject.name);
                    return true;
                }

                if(_segmentList.Count <= index)
                {
                    // Debug.LogError("セグメントの更新がされないよ");
                    break;
                }
            }
            // 判定地点を更新
            if(index < _segmentList.Count)
            {
                // Debug.Log("セグメント更新判定がFALSE。NSPos:" + _nextSegmentPos + ", Ppos:" + playerPos + ", length:" + length + ", index:" + index + ", name:" + _segmentList[index].gameObject.name);
                _nextSegmentPos += _segmentList[index].PathLength;
                // _currentSegment = _segmentList[index];
                // _currentSegmentIndex = index;
                // Debug.Log("_currentSegment更新：" + _currentSegment.name + ", " + index);
            }
            else
                Debug.LogError("終点にいるのにセグメントの更新がされないよ！？");
        }
        return false;
    }
    private void GetCurrentSegment()
    {
        int index = 0;
        float length = 0f;
        float playerPos = _player.GetMovePath().m_Position;
        for(index = 0; index < _segmentList.Count; index++)
        {
            EndlessBattleSegment segment = _segmentList[index];
            length += segment.PathLength;

            if( playerPos < length)
                break;
        }
        if( _segmentList.Count <= index )
        {
            // Debug.Log("あれ");
            index = 0;
        }
        SetCurrentSegment(_segmentList[index]);
    }
    private void SetCurrentSegment(EndlessBattleSegment segment)
    {   
        _currentSegment = segment;
        _currentSegmentIndex = _segmentList.IndexOf(segment);
        
        if(_beforeSegment != _currentSegment)
        {
            if(_beforeSegment == null)
            {
                // Debug.Log("currentSegment更新:Null→" + _currentSegment.gameObject.name);
            }
            else
            {
                // Debug.Log("currentSegment更新:" + _beforeSegment.gameObject.name + "→" + _currentSegment.gameObject.name);
            }
            _beforeSegment = _currentSegment;
        }
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
        int pathNum = segment.GetPathListTransform().Count;
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

        // _currentSegmentIndex--;
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
            // CinemachineSmoothPath.Waypoint waypoint = new CinemachineSmoothPath.Waypoint();
            // Vector3 pos = transforms[i].position;
            // pos.y += _player.GetInitPos().y;
            // waypoint.position = pos;
            CinemachineSmoothPath.Waypoint waypoint = CreateWaypointToTransform(transforms[i]);
            retList.Add( waypoint );
        }
        return retList;
    }
    private CinemachineSmoothPath.Waypoint CreateWaypointToTransform(Transform transforms)
    {
        CinemachineSmoothPath.Waypoint waypoint = new CinemachineSmoothPath.Waypoint();
        Vector3 pos = transforms.position;
        pos.y += _player.GetInitPos().y;
        waypoint.position = pos;
        return waypoint;
    }

    // パスの長さ差分取得&更新
    private float GetSubPathLength()
    {
        float ret = _playerMovePath.PathLength - _beforePathLength;
        _beforePathLength = _playerMovePath.PathLength;
         
        return ret;
    }

    // プレイヤーがパスを通過した時の処理を返す
    private UnityAction<Collider> OnEnterPass( EndlessBattleSegment segment, EndlessBattlePath path )
    {
        SetCurrentSegment(segment);
        return (Collider collider)=>{

            // プレイヤーの移動状態の切り替え
            switch(path.EnterPlayerState)
            {
                case EnterPlayerState.battle:
                    if(!_currentSegment.isAllKill())
                        _player.SetState(PlayerState.battle);
                    break;
                case EnterPlayerState.move:
                    _player.SetState(PlayerState.move);
                    break;
            }
            if(_currentSegment.isAllKill())
                _player.SetState(PlayerState.move);

            // ここを通過したときプレイヤーは何を見るか
            switch(path.EnterLook)
            {
                case EnterLook.front:
                    _player.SetLookAtTarget(null);
                    break;
                case EnterLook.next:
                    
                    break;
                case EnterLook.look:
                    
                    _player.SetLookAtTarget(null);
                    break;
            }

            // ここを通過後、クリアしたらorクリアしてたらプレイヤーは何を見るか
            _carrentClearLook = path.ClearLook;
        };
    }
}
