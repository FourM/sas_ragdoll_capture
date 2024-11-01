using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;
using DG.Tweening;
using System;
// using MoreMountains.NiceVibrations;

/// <summary>
/// インゲーム管理
/// </summary>

public class InGameManager : MonoBehaviour
{
    // ---------- 定数宣言 ----------
    private const float CATCH_OBJ_MASS = 2.0f;
    // ---------- ゲームオブジェクト参照変数宣言 ----------
    // ---------- プレハブ ----------
    // ---------- プロパティ ----------
    [SerializeField, Tooltip("ステージマネージャー")] private StageManager _stageManager = default;
    [SerializeField, Tooltip("おてて")] private Hand _hand;
    [SerializeField, Tooltip("おてての親")] private Transform _handParent;
    [SerializeField, Tooltip("見えないSpringJoint")] private SpringJoint _springjoint;
    [SerializeField, Tooltip("糸始点")] private Transform _webLineStartPos;
    [SerializeField, Tooltip("糸終点")] private Transform _webLineEndPos;
    [SerializeField, Tooltip("糸中間点")] private List<Transform> _webLineWayPosList;
    [SerializeField, Tooltip("糸終点")] private ObiRopeExtrudedRenderer _webRope;
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
    private bool _isCatch = false;
    private float _springPosZ = 0.0f;
    private Material _webRopeMaterial = default;
    private Color32 _webRopeColor = default;
    private float _catchObjMass = 1f;
    private bool _isInitialize = false;
    private bool _isTap = false; // 画面タップしてる
    // private bool _isClear = false;
    private CatchableObj _currentCatchObj = null;
    private Action _showAdAction = null;
    private Tween _handMoneTween = null;
    private Vector3 _beforeMousePos = default;
    private float _currentTotalFastSwipeAway = 0f; //　高速スワイプしてる距離
    private float _tapTime = 0f;
    private bool _isTapHand = false;   // イベント用：おててタップ
    private bool _isTapTarget = false;   // イベント用：ターゲットタップ
    private bool _isStageFirstTap = false;   // イベント用：ステージ開始初めてのタップ
    private Tween _tweenFalseLootAt = null;
    private bool _debugStageLoop = false;
    private bool _debugEnebleInste = true;
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
        // 操作の状態
        // 画面タップしたら、掴みを試行。
        if( Input.GetMouseButton(0))
        {
            if(!_isCatch)
                TryCatch();
            // イベント用：画面タップ！
            if(_isTap)
            {
                // イベント用：タップ時間取得
                _tapTime += Time.deltaTime;
            }
        }

        // 掴み中の挙動
        if(_isCatch && Input.GetMouseButton(0) && _springjoint.connectedBody != null)
        {
            HoldUpdate();
        }

        // リリースした時（物を掴んでいるが画面がタッチされてない時）
        if(!Input.GetMouseButton(0) || ( _isCatch && _springjoint.connectedBody == null ) )
        {
            ReleaseCatchObj();
        }

        if(Input.GetMouseButtonDown(0) || (!_isStageFirstTap && Input.GetMouseButton(0)))
        {
            _isTap = true;
            _tapTime = 0.0f;
            TryTouchHand();
            _isStageFirstTap = true;
            // Debug.Log("タップしたお！");
        }
    }
    public void FixedUpdate()
    {
        GameDataManager.UpdateMutekiTime();
    }
    // ---------- Public関数 ----------
    public void Initialize() {
        if(_isInitialize) return;
        _isInitialize = true;

        GameDataManager.ResetGamePlayData();

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
        // 糸を非表示
        SetEnableWebRope(false);

        _inGameUiManager.Initialize();
        // ステージ再読み込みボタンの処理設定
        _inGameUiManager.SetOnClickButtonUndo(()=>
        {
            UndoInGame();
            // イベント呼び出し
            FirebaseManager.instance.EventReStart();
        });

        _catchWeb.gameObject.SetActive(false);

        FirebaseManager.instance.EventStageStart();

        GameDataManager.UpdatekillShockStrength();


        // 「高速スワイプした！」判定の速度の補正。ABテストの死ぬ閾値に比例させる
        _fastSwipeSpeed *= GameDataManager.GetKillShockStrength() / 30f;

        // 「高速スワイプした！」判定の距離の補正。ABテストの死ぬ閾値に比例させる
        _fastSwipeAway *= GameDataManager.GetKillShockStrength() / 30f;

        // 「掴んでるやつが高速移動してる！」判定の速度の補正。ABテストの死ぬ閾値に比例させる。最低値5
        _fastCatchObjSpd = 5f + (_fastCatchObjSpd - 5f) * GameDataManager.GetKillShockStrength() / 30f;
    }

    public void SetTryShowInterstitialAdAction( Action action ){ _showAdAction = action; }
    public void UndoInGame()
    {
        if(!_isInitialize)
            return;
        _webLineEndPos.parent = this.transform;
        _stageManager.DeleteStage();
        _stageManager.StageLoad();
    }

    public void SetDebugStageLoop(bool isStageLoop)
    {
        _debugStageLoop = isStageLoop;
    }
    public void SetDebugEnebleInste(bool enebleInste)
    {
        _debugEnebleInste = enebleInste;
    }
    // 掴んでるものの差し替えを試行
    public bool TryChangeCatch(CatchableObj catchableObj)
    {
        Debug.Log("差し替え２");
        return CatchSetUp(catchableObj);;
    }

    
    // ---------- Private関数 ----------
    // イベント用：おててタッチ判定
    private void TryTouchHand()
    {
        // レイを飛ばす
        // レイが、掴めるものに当たったら物を掴んだ状態にする
        Camera mainCamera = Camera.main;/*使用するカメラを指定*/
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 15.0f, Color.green, 5, false);
        RaycastHit Hit;
        LayerMask mask = LayerMask.GetMask("Hand");
        // おててタッチ！
        if (Physics.Raycast(ray, out Hit, 10000, mask))
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
        // Debug.DrawRay(ray.origin, ray.direction * 15.0f, Color.green, 5, false);
        RaycastHit Hit;
        LayerMask mask = LayerMask.GetMask("catchable", "catchableChild","catchableThroughFloor", 
         "catchableNoMutualConflicts", "catchableThroughWall",
         "Human1", "Human2", "Human3", "Human4", "Human5", "Human6", "Human7", "Human8", "Human9", "Human10");
        // 捕まえた！
        if (Physics.Raycast(ray, out Hit, 10000, mask))
        {
            // VibrationManager.VibrateLong();
            // RIgidBodyがないなら無視
            if(Hit.rigidbody == null)
                return;

            Transform catchWebParent = null;

            // 糸を表示
            SetEnableWebRope(true);

            // イベント用：対象をタップした
            if(!_isTap)
                _isTapTarget = true;
            
            _isCatch = true;

            // 手のアニメーション
            _hand.ChangeAction(HandAction.attack);

            // 捕まえた時の糸表示
            Vector3 catchWebTargetScale = Vector3.one;
            _catchWeb.gameObject.SetActive(true);
            _catchWeb.localScale = Vector3.zero;

            // 取った対象のCatchableObj取得を試行
            CatchableObj catchableObj = GameDataManager.GetCatchableObj(Hit.transform.gameObject);
            GameDataManager.SetIsCatchSomething(true);
            GameDataManager.SetLookAtTransform(Hit.transform);

            if( catchableObj != null && catchableObj.TryGetAlternate() != null && catchableObj != catchableObj.TryGetAlternate())
            {
                _springjoint.connectedBody = catchableObj.GetRigidbody();
                catchableObj = catchableObj.TryGetAlternate();
                // 取った対象からの相対位置を設定。
                _springjoint.connectedAnchor = Vector3.zero;
                _webLineEndPos.parent = catchableObj.transform;
                _webLineEndPos.localPosition = Vector3.zero;
                _catchWeb.parent = catchableObj.GetCatchWebParent();
            }
            else
            {
                _webLineEndPos.position = Hit.point;
                _webLineEndPos.parent = Hit.transform;
                _springjoint.connectedBody = Hit.rigidbody;
                _springjoint.connectedAnchor = _webLineEndPos.localPosition;
                _catchWeb.parent = Hit.transform;
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
            _catchObjMass = _springjoint.connectedBody.mass;
            
            // 見えないSpringJointの位置変更
            _springPosZ = Hit.point.z - Camera.main.transform.position.z;  
            
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
        }
    }

    // private bool CatchSetUp(GameObject gameObject,Rigidbody rigidbody)
    // {

    // }

    // // 掴んでるものの差し替えを試行
    private bool CatchSetUp(CatchableObj catchableObj)
    {
        Debug.Log("差し替え！");
        return false;
        Vector3 catchWebTargetScale = Vector3.one;
        // 掴むもののを代行するか否か
        _springjoint.connectedBody = catchableObj.GetRigidbody();
        // 取った対象からの相対位置を設定。
        _springjoint.connectedAnchor = Vector3.zero;
        _webLineEndPos.parent = catchableObj.transform;
        _webLineEndPos.localPosition = Vector3.zero;
        _catchWeb.parent = catchableObj.GetCatchWebParent();

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
        _catchObjMass = _springjoint.connectedBody.mass;
        
        // CatchableObjが取得できていたなら、捕まえた時のコールバック呼び出し
        catchableObj.SetOnDoReleaseCallback(()=>
        {
            ReleaseCatchObj();
        });
        catchableObj.OnCatch();
        _currentCatchObj = catchableObj;
        
        catchWebTargetScale = catchableObj.GetWebScale();
        _catchWeb.localPosition = catchableObj.GetWebPosition();
        _catchWeb.localEulerAngles = catchableObj.GetWebRotate();
        catchWebTargetScale *= 0.055f;
        _catchWeb.DOScale(catchWebTargetScale, 0.3f).SetEase(Ease.OutBack);

        return true;
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
        // イベント発火
        if(_isTap)
        {
            _isTap = false;
            // タップしたイベント呼び出し 
            // location  {0なら的へのタップ、1なら的外へのタップ、2なら手へのタップ}
            // is_thread {0なら糸が出なかった、1なら糸が出た
            int location = 0;
            int is_thread = 0;
            if(_isTapTarget)    //　的タップ
                location = 0;
            else if( _isTapHand )
                location = 2;   // おててタップ
            else
                location = 1;   // その他タップ
            // 糸が出たぁ！
            if(_isCatch)
                is_thread = 1;
            FirebaseManager.instance.EventTapCount(location, is_thread);
            // 指を離したイベント呼び出し
            FirebaseManager.instance.EventTapRelese(_tapTime);

            _isTapTarget = false;
            _isTapHand = false;
        }

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
            catchedRigidBody.mass = _catchObjMass;
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
        _webLineEndPos.parent = this.transform;

        // 手を元の位置に戻す
        _handMoneTween = _handParent.DOLocalRotate(Vector3.zero, 0.2f).SetEase(Ease.InOutQuad);
        //　タップしてるフラグを下す
        _isTap = false;

        // 一定時間後、何も捕まえてなければ、「振り向くフラグ」を下す
        _tweenFalseLootAt = DOVirtual.DelayedCall(1f, ()=>{
            
            if( !_isCatch )
            {
                GameDataManager.SetIsCatchSomething(false);
            }
        });

        PlayRandomSound(_audioSouceWebShot, _listAudioClipWebRelease, 0.2f);
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
        GameDataManager.ResetGamePlayData();
        int currentStageNum = SaveDataManager.GetCurrentStage();
        if(!_debugStageLoop)
            currentStageNum++;
        SaveDataManager.SetCurrentStage(currentStageNum);
        _inGameUiManager.HideInGameUI();
        // ステージ進める
        DOVirtual.DelayedCall(2f, ()=>
        {
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
            _isStageFirstTap = false;
        });
    }

    private void PlayRandomSound(AudioSource audioSource, List<AudioClip> listAudioClips, float volumeScale = 1.0f)
    {
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
}
