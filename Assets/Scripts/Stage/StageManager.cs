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
    [SerializeField, Tooltip("ステージ")] List<SerializeList<GameStage>> _levelBundleList = default;
    // ---------- プロパティ ----------
    private bool _isInitialize = false;
    private GameStage _currentStage = null;
    private Action _onCliearCallback = default;
    private List<GameStage> _useGameStageList = default;
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
        int level_Bundle = PlayerPrefs.GetInt("Level_Bundle");

        // レベルバンドル。どちらのステージ順を用いるかABで分ける
        if(level_Bundle < _levelBundleList.Count)
            return _levelBundleList[level_Bundle].list;

        Debug.Log("レベルバンドルの値が異常かもです。：" + level_Bundle + ", " + _levelBundleList.Count);
        return _levelBundleList[0].list;
    }
    // ---------- Private関数 ----------
    private void OnClearCallback()
    {
        _onCliearCallback();
    }
}
