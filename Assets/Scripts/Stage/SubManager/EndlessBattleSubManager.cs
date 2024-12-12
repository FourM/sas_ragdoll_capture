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
    [SerializeField, Tooltip("パス")] private CinemachineSmoothPath _playerMovePath = default;
    private int instanceSegmentIndex = 0;
    private float nextInstancePos = 0;
    private Player _player = null;
    float _nextSegmentPosZ = 0f;
    // ---------- クラス変数宣言 -----------------------
    // ---------- インスタンス変数宣言 ------------------
    // ---------- Unity組込関数 -----------------------
    protected override void InitializeUnique(){
        int instanceSegmentIndex = 0;
        _player = GameDataManager.GetPlayer();
        while( _nextSegmentPosZ <= 100f)
        {
            EndlessBattleSegment segment = InstantiateSegment();
            if(instanceSegmentIndex == 1)
                nextInstancePos = segment.GetLength();
        }

        _player.GetMovePath().m_Path = _playerMovePath;
    }

    protected override void UpdateUnique()
    {
        if( nextInstancePos <= _player.transform.position.z )
        {
            EndlessBattleSegment segment = InstantiateSegment();
            nextInstancePos += segment.GetLength();

            // 後ろのセグメントを消す
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
        EndlessBattleSegment segment = Instantiate(_stagePrefabs[instanceSegmentIndex]);
        segment.transform.parent = this.transform;
        segment.transform.localPosition = new Vector3(0, 0, _nextSegmentPosZ);
        segment.Initialize();
        _nextSegmentPosZ += segment.GetLength();
        IndexNext();
        return segment;
    }
    private void IndexNext(){
        instanceSegmentIndex ++;
        instanceSegmentIndex %= _stagePrefabs.Count;
    }
}
