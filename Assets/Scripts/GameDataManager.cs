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
    // エンドレスバトルボタンの表示とライフゲージ獲得を始めるステージ数条件　このステージを上回っていても他の条件などを満たしてなければ表示されない
    private const int SHOW_ENDLESS_BATTLE_BUTTON_STAGE = 10;
    // エンドレスバトルの初期ライフ
    private const int INIT_ENDLESS_BATTLE_LIFE = 0;
    public static int ShowEndlessBattleButtonStage{ get{ return SHOW_ENDLESS_BATTLE_BUTTON_STAGE; } }
    public static int InitEndlessBattleLife{ get{ return INIT_ENDLESS_BATTLE_LIFE; } }

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
    private static UnityEvent<GameMode> _onChangeGameMode = null;
    private static UnityEvent<GameState> _onChangeGameState = null;
    private static UnityEvent<Human> _onHumanDie = null;
    private static UnityEvent _onUpdateEndlessLife = null;
    private static GameMode _gameMode = GameMode.main;
    private static GameState _gameState = GameState.main;
    private static InGameMainEventManager _inGameMainEventManager;
    public static InGameMainEventManager InGameMainEvent{ get{ return _inGameMainEventManager; } }
    private static Player _player;
    private static float _addPlayerMoveLength = 0f;    // プレイヤーが移動した距離の補正値
    private static bool _endlessUnLimit = false;   // デバッグ用：エンドレスバトルの制限解放
    
    private static UnityEvent<string> _onDebugChangeUserSegment = null; // デバッグ用：ユーザープロパティ変更時のコールバック(変更があったユーザープロパティ名)
    // private static GameMode _backUpGameMode = GameMode.main;
    public static bool DebugEndlessUnLimit{
        get{ return _endlessUnLimit; } set{ _endlessUnLimit = value; _onUpdateEndlessLife?.Invoke();}
    }
    
    public static GameMode GameMode{
        get{ return _gameMode; }
    }
    // public static GameMode BackUpGameMode{
    //     get{ Debug.Log("_backUpGameMode:" + _backUpGameMode); return _backUpGameMode; }
    // }
    public static void SetGameMode(GameMode gameMode){ _gameMode = gameMode;}
    public static GameState GameState{
        get{ return _gameState; }
    }
    public static void SetGameState(GameState gameState){ _gameState = gameState; }

    // セーブデータのロード
    public static void ResetGamePlayData()
    {
        catchableObjDic = new Dictionary<GameObject, CatchableObj>();
        if(_stage != null)
            _stage = null;
        _mutekiTime = 10;
        _addPlayerMoveLength = 0;
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
    public static void AddOnHumanDie(UnityAction<Human> callback)
    {
        if(_onHumanDie == null)
            _onHumanDie = new UnityEvent<Human>();
        _onHumanDie.AddListener(callback); 
    }
    public static void OnHumanDie(Human human)
    {
        _onHumanDie?.Invoke(human);
    }
    public static void AddOnChangeGameMode(UnityAction<GameMode> callback)
    {
        if(_onChangeGameMode == null)
            _onChangeGameMode = new UnityEvent<GameMode>();
        _onChangeGameMode.AddListener(callback); 
    }
    public static void OnChangeGameMode(GameMode gameMode)
    {
        _onChangeGameMode?.Invoke(gameMode);
    }
    public static void AddOnChangeGameState(UnityAction<GameState> callback)
    {
        if(_onChangeGameState == null)
            _onChangeGameState = new UnityEvent<GameState>();
        _onChangeGameState.AddListener(callback); 
    }
    public static void OnChangeGameState(GameState gameState)
    {
        _onChangeGameState?.Invoke(gameState);
    }

    public static void AddOnUpdateEndlessLife(UnityAction callback)
    {
        if(_onUpdateEndlessLife == null)
            _onUpdateEndlessLife = new UnityEvent();
        _onUpdateEndlessLife.AddListener(callback); 
    }

    public static void SetPlayer(Player player){ _player = player; }
    public static Player GetPlayer(){ return _player; }
    // プレイヤーが移動した距離の補正値
    public static void AddPlayerMoveLength(float length){ _addPlayerMoveLength += length; }
    public static void ResetPlayerMoveLength(){ _addPlayerMoveLength = 0; }
    // ゲーム開始時のプレイヤーが移動した距離の補正値
    public static float GetPlayerMoveLength()
    { 
        float length = (SaveDataManager.GetLevelStartPos() - 1) * 5f;
        return length + _addPlayerMoveLength;
    }

    public static void CountUpEnemyKill(float addGuage)
    {
        int killCount = SaveDataManager.GetHumanKillNum();
        killCount++;
        SaveDataManager.SetHumanKillNum(killCount);
        AddEndlessLifeGuage(addGuage);
    }

    // エンドレスバトルのライフゲージ更新
    public static void AddEndlessLifeGuage(float addGuage)
    {
        float guage = SaveDataManager.GetEndlessLifeGuage();
        guage += addGuage;
        int addLife = Mathf.FloorToInt(guage);
        guage -= addLife;
        SaveDataManager.SetEndlessLifeGuage(guage);

        AddEndlessLife(addLife);
    }
    // エンドレスバトルのライフ更新
    public static void AddEndlessLife(int addLife)
    {
        int life = SaveDataManager.GetEndlessLife();
        life += addLife;
        if(life < 0)
            life = 0;
        SaveDataManager.SetEndlessLife(life);
        // 初めてライフを得たフラグをON
        // if( 1 <= life )
        //     SaveDataManager.SetIsFirstLife(1);

        _onUpdateEndlessLife?.Invoke();
    }

    public static void AddOnDebugChangeUserSegment(UnityAction<string> callback)
    {
        if(_onDebugChangeUserSegment == null)
            _onDebugChangeUserSegment = new UnityEvent<string>();
        _onDebugChangeUserSegment.AddListener(callback);
    }
    public static void OnDebugChangeUserSegment(string propatyName)
    {
        _onDebugChangeUserSegment?.Invoke(propatyName);
    }
}