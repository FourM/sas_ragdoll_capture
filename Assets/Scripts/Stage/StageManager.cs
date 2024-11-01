using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

/// <summary>
/// ステージ管理
/// </summary>
public class StageManager : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    [SerializeField, Tooltip("ステージ")] List<GameStage> _gameStageList = default;
    [SerializeField, Tooltip("ステージ")] List<GameStage> _gameStageList2 = default;
    // ---------- プロパティ ----------
    private bool _isInitialize = false;
    private GameStage _currentStage = null;
    private Action _onCliearCallback = default;
    [SerializeField, Tooltip("ステージ")] List<GameStage> _useGameStageList = default;
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    // ---------- Unity組込関数 ----------
    // ---------- Public関数 ----------
    public void Iniiialize()
    {
        if(_isInitialize) return;
        _isInitialize = true;

        ResetStageBundle();
        StageLoad();
    }
    public void ResetStageBundle()
    {
        // レベルバンドルリセット。どちらのステージ順を用いるかABで分ける
        _useGameStageList = GetEnableGameStageList();
    }
    // ステージ生成
    public void StageLoad()
    {
        int currentStageNum = SaveDataManager.GetCurrentStage() % _useGameStageList.Count;

        _currentStage = Instantiate(_useGameStageList[currentStageNum], Vector3.zero, Quaternion.identity);
        _currentStage.transform.parent = this.transform;
        _currentStage.transform.localScale = Vector3.one;
        _currentStage.SetOnClearCallBack(OnClearCallback);
        _currentStage.Initialize();

        GameDataManager.SetStage(_currentStage);
    }
    // ステージ削除
    public void DeleteStage()
    {
        if(_currentStage != null)
            Destroy(_currentStage.gameObject);
        EffectManager.instance.StopAllEffect();
    }
    public void SetOnClearCallBack( Action onCliearCallback)
    {
        _onCliearCallback = onCliearCallback;
    }
    public List<GameStage> GetEnableGameStageList()
    { 
        // レベルバンドル。どちらのステージ順を用いるかABで分ける
        if(PlayerPrefs.GetInt("level_Bundle") == 0)
            return _gameStageList;
        else
            return _gameStageList2;
    }
    // ---------- Private関数 ----------
    private void OnClearCallback()
    {
        _onCliearCallback();
    }
}
