using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 擬似的な走る道路マネージャー
/// </summary>
public class MoveLoadManager : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("hoge")] private List<Transform> _listLoads = default;
    [SerializeField, Tooltip("終わりの位置")] private Transform _endPos = default;
    [SerializeField, Tooltip("各道路タイルの速度")] private Vector3 _speed;
    [SerializeField, Tooltip("前の道路タイルとの間隔")] private Vector3 _betweenSpace;
    [SerializeField, Tooltip("ローカル座標を参照にする？")] private bool _isLocalPos = false;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    private void FixedUpdate(){
        if(!_isLocalPos)
            MoveLoad();
        else
            MoveLoadLocal();
    }
    // ---------- Public関数 ----------
    // ---------- Private関数 ----------
    private void MoveLoad()
    {
        for(int i = 0; i < _listLoads.Count; i++)
        {
            Transform load = _listLoads[i];
            Vector3 currentPos = load.position;
            Vector3 newPos = currentPos + _speed;
                
            // 道路タイルを移動させる。終点に着いたら、前の道路タイルの後ろに瞬間移動させる
            if( (_endPos.position.x < currentPos.x && newPos.x <= _endPos.position.x)||
                (currentPos.x < _endPos.position.x && _endPos.position.x <= newPos.x)||
                (_endPos.position.y < currentPos.y && newPos.y <= _endPos.position.y)||
                (currentPos.y < _endPos.position.y && _endPos.position.y <= newPos.y)||
                (_endPos.position.z < currentPos.z && newPos.z <= _endPos.position.z)||
                (currentPos.z < _endPos.position.z && _endPos.position.z <= newPos.z))
            {
                Transform frontload = null;
                // 前の道路タイル
                if( 0 < i )
                    frontload = _listLoads[i - 1];
                else
                    frontload = _listLoads[_listLoads.Count - 1];
                
                load.position = frontload.position + _betweenSpace;
                if(i == 0)
                    load.position += _speed;

                // if(i == 0)
                //     Debug.Log("前のタイルの後ろに瞬間移動:" + _endPos.position + ", " + load.position);
            }
            else
            {
                load.position = newPos;
                // if(i == 0)
                //     Debug.Log("移動:" + _endPos.position + ", " + load.position);
            }
        }
    }

    private void MoveLoadLocal()
    {
        for(int i = 0; i < _listLoads.Count; i++)
        {
            Transform load = _listLoads[i];
            Vector3 currentPos = load.localPosition;
            Vector3 newPos = currentPos + _speed;
                
            // 道路タイルを移動させる。終点に着いたら、前の道路タイルの後ろに瞬間移動させる
            if( (_endPos.localPosition.x < currentPos.x && newPos.x <= _endPos.localPosition.x)||
                (currentPos.x < _endPos.localPosition.x && _endPos.localPosition.x <= newPos.x)||
                (_endPos.localPosition.y < currentPos.y && newPos.y <= _endPos.localPosition.y)||
                (currentPos.y < _endPos.localPosition.y && _endPos.localPosition.y <= newPos.y)||
                (_endPos.localPosition.z < currentPos.z && newPos.z <= _endPos.localPosition.z)||
                (currentPos.z < _endPos.localPosition.z && _endPos.localPosition.z <= newPos.z))
            {
                Transform frontload = null;
                // 前の道路タイル
                if( 0 < i )
                    frontload = _listLoads[i - 1];
                else
                    frontload = _listLoads[_listLoads.Count - 1];
                
                load.localPosition = frontload.localPosition + _betweenSpace;
                if(i == 0)
                    load.localPosition += _speed;

                // if(i == 0)
                //     Debug.Log("前のタイルの後ろに瞬間移動:" + _endPos.localPosition + ", " + load.localPosition);
            }
            else
            {
                load.localPosition = newPos;
                // if(i == 0)
                //     Debug.Log("移動:" + _endPos.localPosition + ", " + load.localPosition);
            }
        }
    }
}
