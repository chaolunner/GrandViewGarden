using UnityEngine;
using UniEasy;
using System;

[Serializable]
public struct ShopData : IFastScrollData
{
    public string Count;
    public string Price;
    public string Message;
    public string BackgroundColor;
    public Sprite Icon;
}
