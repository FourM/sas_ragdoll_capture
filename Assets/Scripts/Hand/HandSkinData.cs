using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;


[CreateAssetMenu(menuName = "ScriptableObject/HandSkinData", fileName = "HandSkin")]
public class HandSkinData : ScriptableObject
{
    public string name;
    public HandSkin leftHandPrefab;
    public HandSkin rightHandPrefab;
    public Sprite image;
    public Material webMaterial;
    public int needKillNum = 0;
    public float webTwist = 100f;
}