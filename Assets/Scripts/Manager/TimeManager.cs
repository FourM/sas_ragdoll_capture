using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 経過時間管理マネージャー。主に一定時間ごとにインステ広告を表示するために用いる
public class TimeManager : MonoBehaviour
{
    public int count;
    public float elapsedTime;
    public bool counter_flag = false;
    public int isAd;
    static public TimeManager instance;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        Application.targetFrameRate = 60;
        counter_flag = true;
  
        
    }
    void Update()
    {
        if (counter_flag == true)
        {
            elapsedTime += Time.deltaTime;
           
        }
    }
}