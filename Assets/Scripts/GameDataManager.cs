using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public enum MenuType
{
    inGame,
}
/// <summary>
/// ゲーム用の一時保存データ管理
/// </summary>
public static class GameDataManager
{
    private static Dictionary<GameObject, CatchableObj> catchableObjDic = null;
    private static GameStage _stage = null;
    private static int _mutekiTime = 0;
    private static float _killShockStrength = 7f;
    private static bool _isCatchSomething = false;
    private static bool _waitEventStageStart = false;   // ステージスタートイベント待機状態か。新しいステージを始めたらOnになる。インステを見終わるか、インステが流れなかったらOffにしてイベントを発火させるようにする。
    private static Transform _lookAtTransform = null;
    private static Vector3 _lookAtShift = default;
    private static bool _isGimmickKill = false;
    private static bool _debugIsShowUi = true;
    private static bool _eventIsDefeat = false; // イベント用：画面から指を離した時、敵が死んていたか
    private static UnityEvent _onStageStart = null;
    private static GameMode _gameMode = GameMode.main;
    private static InGameMainEventManager _inGameMainEventManager;
    public static InGameMainEventManager InGameMainEvent{ get{ return _inGameMainEventManager; } }
    private static Player _player;
    
    public static GameMode GameMode{
        get{ return _gameMode; }
    }
    public static void SetGameMode(GameMode gameMode){ _gameMode = _gameMode; }

    // セーブデータのロード
    public static void ResetGamePlayData()
    {
        catchableObjDic = new Dictionary<GameObject, CatchableObj>();
        if(_stage != null)
            _stage = null;
        _mutekiTime = 10;
    }
    public static void SetInGameMainEventManager(InGameMainEventManager inGameMainEventManager){ _inGameMainEventManager = inGameMainEventManager; }
    
    // 捕まえられるパーツの登録と参照
    public static void AddCatchableObjDic(GameObject gObj, CatchableObj cObj)
    { 
        if(catchableObjDic == null)
            catchableObjDic = new Dictionary<GameObject, CatchableObj>();
        catchableObjDic.Add(gObj, cObj); 
    }
    public static CatchableObj GetCatchableObj(GameObject key)
    {  
        if(catchableObjDic == null)
            return null;
        if(catchableObjDic.TryGetValue(key, out CatchableObj ret))
            return ret;
        else
            return null;
    }
    // Human取得の試行
    public static Human TryGetHuman(GameObject key)
    {
        CatchableObj catchableObj = GetCatchableObj(key);
        if(catchableObj == null)
            return null;
        return catchableObj.TryGetParentHuman();
    }


    public static void SetStage(GameStage stage){ _stage = stage; }
    public static GameStage GetStage(){ return _stage; }
    public static void UpdateMutekiTime(){ _mutekiTime--; }
    public static int GetMutekiTime(){ return _mutekiTime; }
    public static void SetGimmickKill( bool isGimmickKill ){ _isGimmickKill = isGimmickKill; }
    public static bool IsGimmickKill(){ return _isGimmickKill; }
    public static void SetKillShockStrength(float killShockStrength){ _killShockStrength = killShockStrength; }
    public static float GetKillShockStrength(){ return _killShockStrength; }

    public static void UpdatekillShockStrength()
    {
        SetkillShockStrength(PlayerPrefs.GetInt("killShockStrength", 0));
    }

    // 何か捕まえている
    public static void SetIsCatchSomething(bool isCatchSomething){ _isCatchSomething = isCatchSomething; }
    public static void SetLookAtTransform( Transform lookAtTransform ){ _lookAtTransform = lookAtTransform; }
    public static void SetLookAtShift( Vector3 lookAtshift ){ _lookAtShift = lookAtshift; }
    public static Vector3 GetLookAtPos()
    { 
        if(_lookAtTransform == null)
        {
            SetIsCatchSomething(false);
            Debug.Log("何を見ればいいか登録されてないよ");
            return Vector3.zero;
        }
        return _lookAtTransform.position + _lookAtShift;
    }
    public static bool IsCatchSomething(){ return _isCatchSomething; }
    public static void SetkillShockStrength(int no)
    { 
        switch(no)
        {
            case 0:
                SetKillShockStrength(8f);
                break;
            case 1:
                SetKillShockStrength(10f);
                break;
            case 2:
                SetKillShockStrength(12f);
                break;
        }
    }

    public static bool WaitEventStageStart(){ return _waitEventStageStart; }
    public static void SetWaitEventStageStart(bool waitEventStageStart)
    { 
        _waitEventStageStart = waitEventStageStart;
    }
    public static void TryEventStageStart()
    { 
        if(_waitEventStageStart)
        {
            FirebaseManager.instance.EventStageStart();
            _onStageStart?.Invoke();
        }
        _waitEventStageStart = false;
    }
    public static void SetDebugIsShowUi(bool isShowUI){ _debugIsShowUi = isShowUI; }
    public static bool DebugIsShowUi(){ return _debugIsShowUi; }

    // イベント用：離した時、掴んでた敵は死んているか
    public static void SetIsDefeat(bool eventIsDefeat){ _eventIsDefeat = eventIsDefeat; }
    public static bool IsDefeat(){ return _eventIsDefeat; }   
    public static void AddOnStageStart(UnityAction onStageStart)
    { 
        if(_onStageStart == null)
            _onStageStart = new UnityEvent();
        _onStageStart.AddListener(onStageStart); 
    }   

    public static void SetPlayer(Player player){ _player = player; }
    public static Player GetPlayer(){ return _player; }
}
