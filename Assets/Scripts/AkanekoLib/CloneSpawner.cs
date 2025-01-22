using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// 消える時に自身の分身を作る関数。できるだけ呼ばれないのが望ましい
/// </summary>
public class CloneSpawner : MonoBehaviour
{
    private UnityEvent<CloneSpawner> _onDestroy = null;
    private bool isSceneReloading = false;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isSceneReloading = true;
    }

    private void OnDestroy()
    {
        TryCloneSpawn();
    }


    public void AddOnDestroy(UnityAction<CloneSpawner> callback)
    { 
        if(_onDestroy == null)
            _onDestroy = new UnityEvent<CloneSpawner>();
        _onDestroy.AddListener(callback); 
    }
    public void SetDestroyEvent(UnityEvent<CloneSpawner> callbackEvent)
    { 
        _onDestroy = callbackEvent;
    }

    // シーンリロード時でなければ分身を作成
    private void TryCloneSpawn()
    {
        if (!isSceneReloading)
        {
            CloneSpawner clone = Instantiate(this); 
            clone.SetDestroyEvent(_onDestroy);       

            _onDestroy?.Invoke(clone);
        }
    }
}
