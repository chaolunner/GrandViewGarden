using UnityEngine.UI;
using UnityEngine;
using UniEasy;

public class ShopView : FastScrollView<ShopElement, ShopData>
{
    public RectTransform Content;
    public Scrollbar Scrollbar;

    public override int GetElementCount()
    {
        return Mathf.CeilToInt((Content.parent as RectTransform).rect.width / ElementSize) * ConstraintCount;
    }

    public override void SetContentSize(float value)
    {
        Content.sizeDelta = new Vector2(value, Content.sizeDelta.y);
    }
}
