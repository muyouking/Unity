using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 单个KOC信息
/// </summary>
public class KOC : MonoBehaviour
{
    public Sprite sprite;
    public string simply;

    public KOC(Sprite sprite,string simply)
    {
        this.sprite = sprite;
        this.simply = simply;
    }
    public KOC()
    {

    }
}
