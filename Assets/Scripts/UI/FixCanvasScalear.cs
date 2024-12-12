using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FixCanvasScalear : MonoBehaviour
{
    [SerializeField, Tooltip("CanvasScaler")] private CanvasScaler _canvasScaler;

    private void Awake() {
        // _canvasScaler.referenceResolution = new Vector2(Screen.width, Screen.height);
    }
}
