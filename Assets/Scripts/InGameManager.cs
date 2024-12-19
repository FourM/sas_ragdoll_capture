using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;
using DG.Tweening;
using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
// using MoreMountains.NiceVibrations;

/// <summary>
/// インゲーム管理
/// </summary>

public enum GameMode
{
    main = 1000,
    endlessBattle = 2000
}
public enum GameState
{
    startWait = 0,
    main = 1000,
    endlessBattleEnemyAttack = 2000,
    result = 3000
}

public class InGameManager : MonoBehaviour, InGameMainEventManager
{
    // ---------- 定数宣言 ----------
    private const float CATCH_OBJ_MASS = 2.0f;
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("プレイヤー")] private Player _player = default;
    [SerializeField, Tooltip("プレイヤー")] private float _endlessBattlePlayerMoveSpd = 3f;
    [SerializeField, Tooltip("ステージマネージャー")] private StageManager _stageManager = default;
    [SerializeField, Tooltip("おてて")] private Hand _hand;
    [SerializeField, Tooltip("おてての親")] private Transform _handParent;
    [SerializeField, Tooltip("見えないSpringJoint")] private SpringJoint _springjoint;
    [SerializeField, Tooltip("糸始点")] private Transform _webLineStartPos;
    [SerializeField, Tooltip("糸始点")] private Transform _missWebLineStartPos;
    [SerializeField, Tooltip("糸終点")] private Transform _webLineEndPos;
    [SerializeField, Tooltip("糸中間点")] private List<Transform> _webLineWayPosList;
    [SerializeField, Tooltip("糸のレンダラー")] private ObiRopeExtrudedRenderer _webRope;
    [SerializeField, Tooltip("糸のレンダラー")] private List<ObiRopeExtrudedRenderer> _listCatchRpllWebRope;
    [SerializeField, Tooltip("インゲームのUIマネージャー")] private InGameUIManager _inGameUiManager = default;
    [SerializeField, Tooltip("捕まえた糸")] private Transform _catchWeb = default;
    [SerializeField, Tooltip("「高速スワイプした！」判定の速度")] private float _fastSwipeSpeed = 20f;
    [SerializeField, Tooltip("「高速スワイプした！」判定の距離")] private float _fastSwipeAway = 500f;
    [SerializeField, Tooltip("「掴んでるやつが高速移動してる！」判定の速度")] private float _fastCatchObjSpd = 10f;
    [SerializeField, Tooltip("糸を出す音")] private AudioSource _audioSouceWebShot = null;
    [SerializeField, Tooltip("音リスト")] private List<AudioClip> _listAudioClipWebShot = null;
    [SerializeField, Tooltip("音リスト")] private List<AudioClip> _listAudioClipWebRelease = null;
    [SerializeField, Tooltip("糸で捕まえる音")] private AudioSource _audioSouceWebCatch = null;
    [SerializeField, Tooltip("音リスト")] private List<AudioClip> _listAudioClipWebCatch = null;
    [SerializeField, Tooltip("風切音")] private AudioSource _audioSouceFastSwipe = null;
    [SerializeField, Tooltip("音リスト")] private List<AudioClip> _listAudioClipFastSwipe = null;
    [SerializeField, Tooltip("音リスト")] private ObiParticleAttachment _webStartAttachment = default;
    [SerializeField, Tooltip("ゲームモード")] private GameMode _gameMode = GameMode.main;
    [SerializeField, Tooltip("ゲームステート")] private GameState _gameState = GameState.main;
    private UnityEvent _onInitialize = null;
    private UnityEvent _onInitializeMaterialManager = null;
    private bool _isCatch = false;
    private float _springPosZ = 0.0f;
    private Material _webRopeMaterial = default;
    private Material _webRopeMaterial2 = default;
    private Color32 _webRopeColor = default;
    private float _catchObjMass = 1f;
    private bool _isInitialize = false;
    private bool _isTap = false; // 画面タップしてる
    // private bool _isClear = false;
    private CatchableObj _currentCatchObj = null;
    private Action _showAdAction = null;
    private Tween _handMoneTween = null;
    private Tween _notCatchTween = null;
    private Vector3 _beforeMousePos = default;
    private float _currentTotalFastSwipeAway = 0f; //　高速スワイプしてる距離
    private float _tapTime = 0f;
    private bool _isTapHand = false;   // イベント用：おててタップ
    private bool _isTapTarget = false;   // イベント用：ターゲットタップ
    private bool _isTaphuman = false;   // イベント用：敵タップ
    private bool _isStageFirstTap = false;   // イベント用：ステージ開始初めてのタップ
    private bool _is_thread = false;  // イベント用：糸が出たか
    private bool _isNotCatchShot = false;  //何も捕まえてない時の糸射出演出中か
    private bool _isNotCatchHandLookAt = false;  //何も捕まえてない時のおてて振り向きを有効にするか
    private Tween _tweenFalseLootAt = null;
    private bool _debugStageLoop = false;
    private bool _debugEnebleInste = true;
    private Vector2 _tapPos = default;   // イベント用：画面タップ
    private MissRope _lastMissRope = null;
    private bool _isShowUI = false;
    private int _showUINum = 0;
    private bool _isUITouch = false;
    private Vector3 _playerInitPos;
    private GameMode _currentGameMode = GameMode.main;
    public GameMode GameMode{
        get{ return _gameMode; }
        set{ 
            // if( _gameMode ==
            _gameMode = value; 
            GameDataManager.SetGameMode(value);
            if(_stageManager.IsInitialize)
                UndoInGame();
            switch(_gameMode)
            {
                case GameMode.main:
                    GameState = GameState.main;
                    break;
                case GameMode.endlessBattle:
                    GameState = GameState.startWait;
                    break; 
            }
        }
    }
    public GameState GameState{
        get{ return _gameState; }
        set{
            // if(_gameState == value) 
            //     return;
            Debug.Log("gameState変更!:" + _gameState);
            _gameState = value;
            GameDataManager.SetGameState(value);
            // _player.StopPathMove();
            OnChangeGameState();
        }
    }
    // ---------- クラス変数宣言 ----------
    // ---------- インスタンス変数宣言 ----------
    public static InGameManager instance = null;
    // ---------- Unity組込関数 ----------
    private void Awake()
    {
        if(instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }
    private void Update(){
        if(_currentGameMode != GameMode)
        {
            _currentGameMode = GameMode;
            UndoInGame();
        }

        // UIをクリックしたか
        if(EventSystem.current.currentSelectedGameObject != null && 
        EventSystem.current.currentSelectedGameObject.layer == LayerMask.NameToLayer("UI"))
        {
            _isUITouch = true;
        }
        else
        {
            _isUITouch = false;
        }
        switch(GameState)
        {
            case GameState.startWait:
                if(GameMode == GameMode.main)
                    GameState = GameState.main;
                if(!_isUITouch && Input.GetMouseButton(0) && _showUINum == 0)
                    GameState = GameState.main;
                break;
            case GameState.main:
            case GameState.endlessBattleEnemyAttack:
                if(GameMode == GameMode.endlessBattle)
                {   
                    if(_springPosZ < 5f)
                        _springPosZ = 5f;
                    if(_springPosZ < 6.5f)
                    {
                        _springPosZ += Time.deltaTime * 1f;
                        if( 6.5f <= _springPosZ )
                            _springPosZ = 6.5f;
                    }
                }
                InGameMainUpdate();  
                break;
            case GameState.result:
                // if(!_isUITouch && Input.GetMouseButton(0) && _showUINum == 0)
                //     GameState = GameState.startWait;
                break;
        }
    }
    public void FixedUpdate()
    {
        GameDataManager.UpdateMutekiTime();
    }
    // インスペクター上で値を変更した時の処理
    private void OnValidate()
    {
        // プロパティを経由して値を設定
        GameMode = _gameMode;
    }
    // ---------- Public関数 ----------
    public void Initialize() {
        
        if(_isInitialize) return;
        _isInitialize = true;

        // マテリアルマネージャー初期化
        _onInitializeMaterialManager?.Invoke();

        GameDataManager.ResetGamePlayData();
        GameDataManager.SetInGameMainEventManager(this);
        GameDataManager.SetGameMode(_gameMode);
        GameDataManager.SetPlayer(_player);

        // ゲームモード別の処理を初期化
        GameMode = GameMode;

        // ステージ初期化
        _stageManager.Iniiialize();
        _stageManager.SetOnClearCallBack(OnClearCallback);

        // おてての状態フラグ初期化
        _isCatch = false;
        _hand.ChangeAction(HandAction.idle);

        // マテリアルを複製（糸の表示非表示用）
        _webRopeMaterial = new Material(_webRope.material);
        _webRope.material = _webRopeMaterial;
        _webRopeColor = _webRopeMaterial.color;
        _webRopeMaterial2 = _webRope.material;
        // 糸を非表示
        SetEnableWebRope(false);

        _inGameUiManager.Initialize();
        // ステージ再読み込みボタンの処理設定
        _inGameUiManager.SetOnClickButtonUndo(()=>
        {
            UndoInGame();
            // イベント呼び出し
            FirebaseManager.instance.EventReStart();
            GameDataManager.SetIsCatchSomething(false);
        });

        _catchWeb.gameObject.SetActive(false);

        FirebaseManager.instance.EventStageStart();

        GameDataManager.UpdatekillShockStrength();

        EffectManager.instance.Initialize();


        // 「高速スワイプした！」判定の速度の補正。ABテストの死ぬ閾値に比例させる
        _fastSwipeSpeed *= GameDataManager.GetKillShockStrength() / 30f;

        // 「高速スワイプした！」判定の距離の補正。ABテストの死ぬ閾値に比例させる
        _fastSwipeAway *= GameDataManager.GetKillShockStrength() / 30f;

        // 「掴んでるやつが高速移動してる！」判定の速度の補正。ABテストの死ぬ閾値に比例させる。最低値5
        _fastCatchObjSpd = 5f + (_fastCatchObjSpd - 5f) * GameDataManager.GetKillShockStrength() / 30f;

        GameDataManager.AddOnStageStart(TryRequestReview);
        _onInitialize?.Invoke();

        _playerInitPos = _player.transform.position;
    }

    public void UpdateWebRopeMaterial(Material material)
    {       
        _webRope.gameObject.SetActive(false);
        _webRope.material = new Material(material);
        Destroy(_webRopeMaterial);
        _webRopeMaterial = _webRope.material;
        _webRopeColor = _webRopeMaterial.color;
        _webRopeMaterial2 = material;   // 色を変えないマテリアル。何もないとこをタップした時に出る糸とぐるぐる巻き糸に用いる
        _webRope.gameObject.SetActive(true);
        for(int i = 0; i < _listCatchRpllWebRope.Count; i++)
        {
            _listCatchRpllWebRope[i].material = material;
        }
        SetEnableWebRope(false);
    }

    // 初期化時イベント設定
    public void AddOnInitialize( UnityAction onInitialize)
    {
        if(_onInitialize == null)
            _onInitialize = new UnityEvent();
        _onInitialize.AddListener(onInitialize);
    }
    // 初期化時イベント設定
    public void AddOnInitializeMaterialManager( UnityAction onInitialize)
    {
        if(_onInitializeMaterialManager == null)
            _onInitializeMaterialManager = new UnityEvent();
        _onInitializeMaterialManager.AddListener(onInitialize);
    }

    public void SetTryShowInterstitialAdAction( Action action ){ _showAdAction = action; }
    public void UndoInGame()
    {
        if(!_isInitialize)
            return;
        if(GameMode == GameMode.endlessBattle)
            GameState = GameState.startWait;
        _webLineEndPos.parent = this.transform;
        _stageManager.DeleteStage();
        _stageManager.StageLoad();
        // 何もないとこを捕まえた時の挙動をキャンセル
        CanselNotCatchAction();

        _player.transform.position = _playerInitPos;
        _player.transform.localEulerAngles = Vector3.zero;
    }

    public void SetDebugStageLoop(bool isStageLoop)
    {
        _debugStageLoop = isStageLoop;
    }
    public void SetDebugEnebleInste(bool enebleInste)
    {
        _debugEnebleInste = enebleInste;
    }
    public void SetIsShowUI(bool isShowUI)
    {
        if(isShowUI)
            _showUINum++;
        else
            _showUINum--;

        if(_showUINum < 0)
            Debug.LogError("_showUINumが0未満になったよおおお!?:" + _showUINum);
    }

    // 敵の攻撃開始時の処理
    public void OnEnemyAttackStart()
    {
        if( GameMode == GameMode.endlessBattle)
        {
            ReleaseCatchObj();
            TapUp();
            GameState = GameState.endlessBattleEnemyAttack;
        }
    }
    // 敵の攻撃をキャンセルさせた時の演出
    public void OnEnemyAttackCansel()
    {
        if( GameMode == GameMode.endlessBattle)
        {
            GameState = GameState.main;
        }
    }
    // 敵になぐられた時の演出
    public void OnEnemyAttackHit()
    {
        if( GameMode == GameMode.endlessBattle)
        {
            GameState = GameState.startWait;
            UndoInGame();
            Debug.Log("ぎゃああ");
        }
    }

    // ---------- Private関数 ----------
    private void InGameMainUpdate()
    {
        // 操作の状態
        // 画面タップしたら、掴みを試行。
        if( Input.GetMouseButton(0) && _showUINum == 0 && !_isUITouch)
        {
            if(!_isCatch)
                TryCatch();
            // Debug.Log("_isCatch:" + _isCatch);
            if(!_isCatch && PlayerPrefs.GetInt("Aim_ON") == 1)
                NotCatchWebShot();

            // イベント用：画面タップ！
            if(_isTap)
            {
                // イベント用：タップ時間取得
                _tapTime += Time.deltaTime;
            }
            _inGameUiManager.SetReticlePos( Input.mousePosition );
        }

        // 掴み中の挙動
        if(_isCatch && Input.GetMouseButton(0) && _springjoint.connectedBody != null)
        {
            HoldUpdate();
        }

        // リリースした時（物を掴んでいるが画面がタッチされてない時）
        if(!Input.GetMouseButton(0) || ( _isCatch && (_springjoint.connectedBody == null || 0 < _showUINum )) )
        {
            ReleaseCatchObj();
        }

        if(!_isUITouch)
        {
            // UIを表示していない状態で、画面を押した or　ステージ開始時に長押ししていたら、「タップしてる！」フラグを立てる
            if(_showUINum == 0 && ( Input.GetMouseButtonDown(0) || (!_isStageFirstTap && Input.GetMouseButton(0))))
            {
                _isTap = true;
                _tapTime = 0.0f;
                TryTouchHand();
                _isStageFirstTap = true;
                _tapPos = Input.mousePosition;
                // Debug.Log("タップしたお！");
                GameDataManager.SetIsDefeat(false);
                _inGameUiManager.OnClick(_tapPos);
            }
        }
        if(Input.GetMouseButtonUp(0))
        {
            TapUp();
        }
    }

    // イベント用：おててタッチ判定
    private void TryTouchHand()
    {
        // レイを飛ばす
        // レイが、掴めるものに当たったら物を掴んだ状態にする
        Camera mainCamera = Camera.main;/*使用するカメラを指定*/
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 15.0f, Color.green, 5, false);
        RaycastHit hit;
        LayerMask mask = LayerMask.GetMask("Hand");
        // おててタッチ！
        if (Physics.Raycast(ray, out hit, 10000, mask))
        {
            _isTapHand = true;
        }
    }
    // つかみを試行
    private void TryCatch()
    {
        // レイを飛ばす
        // レイが、掴めるものに当たったら物を掴んだ状態にする
        Camera mainCamera = Camera.main;/*使用するカメラを指定*/
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 15.0f, Color.green, 5, false);
        RaycastHit hit;
        LayerMask mask = LayerMask.GetMask("catchable", "catchableChild","catchableThroughFloor", 
         "catchableNoMutualConflicts", "catchableThroughWall", "catchableNotKill",
         "Human1", "Human2", "Human3", "Human4", "Human5", "Human6", "Human7", "Human8", "Human9", "Human10");

        // 捕まえた！
        // if (Physics.Raycast(ray, out hit, 10000, mask))
        if(Physics.CapsuleCast( mainCamera.transform.position, mainCamera.transform.position, 0.3f, ray.direction, out hit, 10000, mask ))
        {
            // VibrationManager.VibrateLong();
            // RigidBodyがないなら無視
            if(hit.rigidbody == null)
                return;

            Transform catchWebParent = null;

            // 糸を表示
            SetEnableWebRope(true);

            // イベント用：対象をタップした
            if(!_isTap)
                _isTapTarget = true;

            _inGameUiManager.SetIsCatch(true);
            
            _isCatch = true;
            _is_thread = true;

            // 手のアニメーション
            _hand.ChangeAction(HandAction.attack);

            // 捕まえた時の糸表示
            Vector3 catchWebTargetScale = Vector3.one;
            _catchWeb.gameObject.SetActive(true);
            _catchWeb.localScale = Vector3.zero;

            // 取った対象のCatchableObj取得を試行
            CatchableObj catchableObj = GameDataManager.GetCatchableObj(hit.transform.gameObject);
            GameDataManager.SetIsCatchSomething(true);
            GameDataManager.SetLookAtTransform(hit.transform);

            // Humanをタップしたか否か
            bool isOtherCatchHuman = false; // Humanで、他の何かに捕まってる
            Human human = null;
            if(catchableObj != null) 
            {
                human = catchableObj.TryGetParentHuman();
                if(human != null)
                {
                    _isTaphuman = true; // イベント用：人をタップした
                    isOtherCatchHuman = human.IsOtherCatch();
                }
            }

            // 何もないとこを捕まえた時の挙動をキャンセル
            CanselNotCatchAction(false);

            // 捕まえたものがHumanChildならこちらを通る
            HumanChild humanChild = catchableObj.TryGetHumanChild();

            // if( (catchableObj != null && catchableObj.GetAlternate() != null) || _isTaphuman)
            if(humanChild != null )
            {
                // if(isOtherCatchHuman)   // 糸以外の何かに捕まっているHumanならここを通る
                //     catchableObj = human.GetParts(HumanParts.body);
                // else
                    catchableObj = catchableObj.GetAlternate();
                _springjoint.connectedBody = catchableObj.GetRigidbody();
                // 取った対象からの相対位置を設定。
                _springjoint.connectedAnchor = Vector3.zero;
                _webLineEndPos.parent = catchableObj.transform;
                _webLineEndPos.localPosition = Vector3.zero;
                if(catchableObj.FixConnectedAnchor())
                {
                    _springjoint.connectedAnchor = catchableObj.GetConnectedAnchor();
                    _webLineEndPos.localPosition = catchableObj.GetConnectedAnchor();
                }
                _catchWeb.parent = catchableObj.GetCatchWebParent();
                // Debug.Log("おっほほう");
            }
            // ギミックなどは基本的にこちらを通る
            else
            {
                _webLineEndPos.position = hit.point;
                _webLineEndPos.parent = hit.transform;
                _springjoint.connectedBody = hit.rigidbody;
                _springjoint.connectedAnchor = _webLineEndPos.localPosition;
                _catchWeb.parent = hit.transform;
                // Debug.Log("えっへへい");
            }

            GameDataManager.SetLookAtShift(_springjoint.connectedAnchor);
            if(_tweenFalseLootAt != null)
            {
                _tweenFalseLootAt.Kill();
                _tweenFalseLootAt = null;
            }
                
            // 掴んだオブジェクトが重かったら、スプリングの力を上げる
            if(10 <= _springjoint.connectedBody.mass)
                _springjoint.spring = _springjoint.connectedBody.mass * 45;
            else
                _springjoint.spring = 90;
            

            // 取った相手のRagDoll挙動安定化のため、重さを変える
            // _catchObjMass = _springjoint.connectedBody.mass;
            
            // 見えないSpringJointの位置変更
            // _springPosZ = hit.point.z - Camera.main.transform.position.z;  
            _springPosZ = (hit.point - Camera.main.transform.position).magnitude;  
            
            // CatchableObjが取得できていたなら、捕まえた時のコールバック呼び出し
            if(catchableObj != null)
            {
                catchableObj.SetOnDoReleaseCallback(()=>
                {
                    ReleaseCatchObj();
                });
                catchableObj.OnCatch();
                _currentCatchObj = catchableObj;
            }
            
            if(catchableObj != null )
            {
                catchWebTargetScale = catchableObj.GetWebScale();
                _catchWeb.localPosition = catchableObj.GetWebPosition();
                _catchWeb.localEulerAngles = catchableObj.GetWebRotate();
            }
            else
            {
                _catchWeb.localPosition = Vector3.zero;
            }
            catchWebTargetScale *= 0.055f;
            _catchWeb.DOScale(catchWebTargetScale, 0.3f).SetEase(Ease.OutBack);

            //-------------

            _handMoneTween.Kill();
            _handMoneTween = null;

            _beforeMousePos = Input.mousePosition;
            _currentTotalFastSwipeAway = 0f;

             // バイブレーションさせる
            VibrationManager.VibrateShort();
            PlayRandomSound(_audioSouceWebShot, _listAudioClipWebShot);
            PlayRandomSound(_audioSouceWebCatch, _listAudioClipWebCatch);

            GameDataManager.SetIsDefeat(false);
        }
    }

    // 何も掴めなかった時の糸射出
    private void NotCatchWebShot()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 endPos = Camera.main.transform.position + ray.direction * 10.0f;

        _handMoneTween.Kill();
        _handMoneTween = null;

        // おてて 
        if(_isNotCatchHandLookAt)
            _handParent.LookAt(endPos);

        float waitTime = 0.45f;
        if(!_isNotCatchShot && !_isTap)
        {
            // 手のアニメーション
            _hand.ChangeAction(HandAction.attack);
            if(_notCatchTween != null)
            {
                _notCatchTween.Kill();
                _notCatchTween = null;
            }
            _notCatchTween = DOVirtual.DelayedCall(waitTime, ()=>
            {
                _hand.ChangeAction(HandAction.idle);
                _isNotCatchHandLookAt = true;
                _isNotCatchShot = false;
                _lastMissRope = null;
                if(!_isTap)
                {
                    // 手を元の位置に戻す
                    _handMoneTween = _handParent.DOLocalRotate(Vector3.zero, 0.2f).SetEase(Ease.InOutQuad);
                }
            });
            // 糸射出
            _lastMissRope = EffectManager.instance.PlayMissRope( _missWebLineStartPos, endPos, waitTime, _webRopeMaterial2 );
            _isNotCatchShot = true;
            _isNotCatchHandLookAt = false;
            // 音を出す
            PlayRandomSound(_audioSouceWebShot, _listAudioClipWebShot);
        }
    }

    private Vector3 GetRandomVec3(float randomScale)
    {
        return new Vector3(UnityEngine.Random.Range(-randomScale, randomScale), UnityEngine.Random.Range(-randomScale, randomScale), UnityEngine.Random.Range(-randomScale, randomScale));
    }

    // 掴み中の動作
    private void HoldUpdate()
    {
        Vector2 mousePos = Input.mousePosition;  
        // スクリーン座標のZ値を5に変更  
        Vector3 screenPos = new Vector3(mousePos.x, mousePos.y, _springPosZ);  
        // ワールド座標に変換  
        Vector3 Pos = Camera.main.ScreenToWorldPoint(screenPos);  
        // ワールド座標を3Dオブジェクトの座標に適用  
        _springjoint.transform.position = Pos;  

        // 糸を表示
        _handParent.LookAt(_springjoint.transform);
        // // 糸終点の位置調整
        // _webLineEndPos.position = _springjoint.connectedBody.transform.position + _springjoint.connectedAnchor;

        // 糸経由点の位置(糸が自然にしなっているような演出用)
        for(int i = 0; i < _webLineWayPosList.Count; i++)
        {
            // 分母
            int denominator = _webLineWayPosList.Count + 1;
            // 分子
            int numerator = i + 1;

            // 引っ張り元の位置と終点の位置の間の位置をとる。この経由点が始点に近いほど引っ張り元に、終点に近いほど終点に近い位置をとる。
            Vector3 PosFactorA = _springjoint.transform.position * (denominator - numerator);
            Vector3 PosFactorB = _webLineEndPos.position * numerator;
            Vector3 PosFactorC = ( PosFactorA + PosFactorB ) / denominator;

            // 直前に取得した位置と始点の間の位置をとる。この経由点が始点に近いほど始点に、終点に近いほど直前に取得した値に近い位置をとる。
            Vector3 PosFactorD = _webLineStartPos.position * (denominator - numerator);
            Vector3 PosFactorE = PosFactorC * numerator;
            Vector3 pos = ( PosFactorD + PosFactorE ) / denominator;

            // 直前に取得した位置に設定する
            _webLineWayPosList[i].position = pos;
        }


        // 補助的な死亡判定。素早く一定距離をスワイプして、掴んでるやつも高速で動いているならOnになる。
        // Onになったら、一定時間、「何かにぶつかったら自身もそいつも死亡させる」フラグをOnにする。
        if( _currentCatchObj != null )
        {
            float swipeSpeed = (Input.mousePosition - _beforeMousePos).magnitude;
            if( _fastSwipeSpeed <= swipeSpeed)
            {       
                // Debug.Log("素早いスワイプ！:" + swipeSpeed);

                float _beforeTotalFastSwipeAway = _currentTotalFastSwipeAway;
                _currentTotalFastSwipeAway += swipeSpeed;

                float catchObjSpeed = _currentCatchObj.GetRigidbody().velocity.magnitude;
                // Debug.Log("掴んでるやつの速度:" + catchObjSpeed);
                // 素早いスワイプを維持して一定距離スワイプした
                if( _fastSwipeAway <= _currentTotalFastSwipeAway && _fastCatchObjSpd <= catchObjSpeed)
                {
                    // Debug.Log("一定距離を素早いスワイプ！:" + _currentTotalFastSwipeAway);
                    _currentCatchObj.FastSwipe();

                    Human human = _currentCatchObj.TryGetParentHuman();
                    if(human != null)
                        human.FastSwipe();
                        
                    if(_beforeTotalFastSwipeAway < _fastSwipeAway)
                        PlayRandomSound(_audioSouceFastSwipe, _listAudioClipFastSwipe, 0.8f);
                }
            }
            else
            {
                if( 0f < _currentTotalFastSwipeAway)
                    // Debug.Log("素早いスワイプじゃなくなったよ");
                _currentTotalFastSwipeAway = 0f;
            }
        }

        // タッチ位置保存
        _beforeMousePos = Input.mousePosition;
    }

    // 捕まえた奴を手放した時の挙動
    private void ReleaseCatchObj()
    {
        if(!_isCatch)   
            return;

        // スワイプ速度
        float swipeSpeed = (Input.mousePosition - _beforeMousePos).magnitude;

        _isCatch = false;
        Rigidbody catchedRigidBody = _springjoint.connectedBody;
        _springjoint.connectedBody = null;

        // 奥側へ弾き飛ばす
        if(catchedRigidBody != null)
        {
            // catchedRigidBody.mass = _catchObjMass;
        }

        _hand.ChangeAction(HandAction.idle);

        // 糸を非表示
        SetEnableWebRope(false);

        // 糸の経由点などの位置を初期化（演出用）
        _webLineEndPos.position = _webLineStartPos.position;
        for(int i = 0; i < _webLineWayPosList.Count; i++)
        {
            _webLineWayPosList[i].position = _webLineStartPos.position;
        }

        // 捕まえたオブジェクトが、離した時のコールバック呼び出し対象なら呼び出す
        if(catchedRigidBody != null && _currentCatchObj != null)
        {
            _currentCatchObj.OnRelease();
            _currentCatchObj = null;
        }

        _catchWeb.parent = this.transform;
        _catchWeb.gameObject.SetActive(false);
        // Debug.Log("わんたそ");
        _webLineEndPos.parent = this.transform;

        // 手を元の位置に戻す
        _handMoneTween = _handParent.DOLocalRotate(Vector3.zero, 0.2f).SetEase(Ease.InOutQuad);

        // 一定時間後、何も捕まえてなければ、「振り向くフラグ」を下す
        _tweenFalseLootAt = DOVirtual.DelayedCall(1f, ()=>{
            if( !_isCatch )
            {
                GameDataManager.SetIsCatchSomething(false);
            }
        });

        PlayRandomSound(_audioSouceWebShot, _listAudioClipWebRelease, 0.2f);

        _inGameUiManager.SetIsCatch(false);
    }

    // 画面から指を離したときの処理
    private void TapUp()
    {
        // イベント発火
        if(_isTap)
        {
            // Debug.Log("TapUp!");
            // タップしたイベント呼び出し 
            // location  {0なら敵をタップ、1なら的外へのタップ、2なら手へのタップ、3はギミックのタップ}
            // is_thread {0なら糸が出なかった、1なら糸が出た
            int location = 0;
            if(_isTapTarget)    
            {
                if(_isTaphuman)
                    location = 0;   //　敵タップ
                else
                    location = 3;   // ギミックをタップ
            }
            else if( _isTapHand )
                location = 2;   // おててタップ
            else
                location = 1;   // その他タップ

            bool is_defeat = GameDataManager.IsDefeat(); // 離した時、誰か敵は死んているか
            float tapPosX = _tapPos.x / Screen.width;
            float tapPosY = _tapPos.y / Screen.height;
            int is_thread = Convert.ToInt32(_is_thread);


            // 画面タップイベント呼び出し
            FirebaseManager.instance.EventTapCount(location, is_thread, tapPosX, tapPosY);
            // 指を離したイベント呼び出し
            FirebaseManager.instance.EventTapRelese(_tapTime, is_defeat, location, is_thread);

            _isTapTarget = false;
            _isTapHand = false;
            _isTaphuman = false;
            _is_thread = false;
            // _isNotCatchShot = false;
            // _isNotCatchHandLookAt = true;
            GameDataManager.SetIsDefeat(false);
        }
        else
        {
            // Debug.Log("TapUp!?");
        }
        //　タップしてるフラグを下す
        _isTap = false;
        _inGameUiManager.SetIsCatch(false);
        _inGameUiManager.OnMouseUp();

        // 糸を非表示
        SetEnableWebRope(false);
        // 手を元の位置に戻す
        if( !_isNotCatchShot )
        {
            _handMoneTween = _handParent.DOLocalRotate(Vector3.zero, 0.2f).SetEase(Ease.InOutQuad);
            _hand.ChangeAction(HandAction.idle);
        }
        // 一定時間後、何も捕まえてなければ、「振り向くフラグ」を下す
        _tweenFalseLootAt = DOVirtual.DelayedCall(1f, ()=>{
            if( !_isCatch )
            {
                GameDataManager.SetIsCatchSomething(false);
            }
        });
    }

    // 糸の表示非表示切り替え
    private void SetEnableWebRope(bool isEnable)
    {
        if(isEnable)
        {
            _webRopeMaterial.color = _webRopeColor;
        }
        else
        {
            _webRopeMaterial.color = new Color32(255,255,255, 0);
        }
    }

    // クリア時のコールバック
    private void OnClearCallback(){
    
        // _isClear = true;
        FirebaseManager.instance.EventStageClear();
        int currentStageNum = SaveDataManager.GetCurrentStage();
        if(!_debugStageLoop)
            currentStageNum++;
        SaveDataManager.SetCurrentStage(currentStageNum);
        _inGameUiManager.HideInGameUI();
        // ステージ進める
        DOVirtual.DelayedCall(2f, ()=>
        {
            GameDataManager.ResetGamePlayData();
            _webLineEndPos.parent = this.transform;
            _catchWeb.parent = this.transform;
            _catchWeb.gameObject.SetActive(false);
            _stageManager.DeleteStage();
            _stageManager.StageLoad();
            _inGameUiManager.ShowInGameUI();
            // ステージスタートイベント発火を待機状態にさせる。
            GameDataManager.SetWaitEventStageStart(true);
            // インステ広告表示試行
            if(_debugEnebleInste)
                _showAdAction?.Invoke();
            // FirebaseManager.instance.EventStageStart();
            ReleaseCatchObj();
            TapUp();
            _isStageFirstTap = false;
            GameDataManager.SetIsCatchSomething(false);
            // 何もないとこを捕まえた時の挙動をキャンセル
            CanselNotCatchAction();
            // _isClear = false;
        });
    }

    private void PlayRandomSound(AudioSource audioSource, List<AudioClip> listAudioClips, float volumeScale = 1.0f)
    {
        if(PlayerPrefs.GetInt("Effect_ON", 1) == 0)
            return;
        if(audioSource == null)
            return;
        if(listAudioClips == null)
            return;
        if(listAudioClips.Count <= 0)
            return;
        // 音を出す
        int index = UnityEngine.Random.Range(0, listAudioClips.Count);
        audioSource.PlayOneShot(listAudioClips[index], volumeScale);
    }

    // 何もないとこを捕まえた時の挙動をキャンセル
    private void CanselNotCatchAction(bool isIdle = true)
    {
        if(_notCatchTween != null)
        {
            _notCatchTween.Kill();
            _notCatchTween = null;
        }
        if(_lastMissRope != null)
            Destroy(_lastMissRope.gameObject);
        _isNotCatchHandLookAt = true;
        _isNotCatchShot = false;
        _lastMissRope = null;
        if(isIdle)
            _hand.ChangeAction(HandAction.idle);
        if(!_isTap)
        {
            // 手を元の位置に戻す
            _handMoneTween = _handParent.DOLocalRotate(Vector3.zero, 0.2f).SetEase(Ease.InOutQuad);
        }
    }

    // レビュー促進ポップアップ表示を試行
    private void TryRequestReview()
    {
        if(this != null && this.gameObject.activeSelf)
        {
            if((PlayerPrefs.GetInt("currentStage", 0) + 1) % 30 == 0)
            {
                StartCoroutine(InAppReviewManager.RequestReview());
                Debug.Log("Show InAppReview!!!");
            }
        }
    }

    // インゲームの状態が切り替わる時に呼び出される処理
    private void OnChangeGameState()
    {
        _player.SetState(PlayerState.stop);
        switch(GameState)
        {
            case GameState.startWait:
                _player.SetState(PlayerState.stop);
                break;
            case GameState.main:
                if(GameMode == GameMode.main)
                {
                    _player.SetState(PlayerState.stop);
                }
                if(GameMode == GameMode.endlessBattle)
                {
                    _player.SetState(PlayerState.move);
                }
                break;
            case GameState.endlessBattleEnemyAttack:
                if(GameMode == GameMode.endlessBattle)
                {
                    _player.SetState(PlayerState.stop);
                }
                break;
            case GameState.result:
                break;
        }
    }
}
