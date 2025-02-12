using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpearShotter : MonoBehaviour, IInitializer
{
    [SerializeField] private InterfaceReference<IInitializer> _iInitializer;
    [SerializeField, Tooltip("gameObject")] private ChildTrigger _trigger = default;
    [SerializeField, Tooltip("gameObject")] private List<SimpleMoveGimmick> _listSpear = default;
    [field: SerializeField] public InitializerBase Initializer { get; set; }

    private void Awake()
    {
        _iInitializer.Value.AddOnInitialize(Initialize);
        Initializer.Init(this, this);  
    }
    private void Initialize()
    {
        _trigger.AddCallbackOnTriggerEnter((Collider collider)=>{
            for(int i = 0; i < _listSpear.Count; i++)
            {
                _listSpear[i].StartMove();
            }
        });
        Initializer.OnInitialize?.Invoke();
        Initializer.OnInitialize?.RemoveAllListeners();
    }
    public void AddOnInitialize( UnityAction onInitialize)
    {
        Initializer.AddOnInitialize(onInitialize);
    }
}