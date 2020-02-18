using UnityEngine.UI;
using UnityEngine;
using UniEasy;
using TMPro;

public class ShopElement : MonoBehaviour, IFastScrollElement<ShopData>
{
    public TMP_Text Count;
    public TMP_Text Price;
    public TMP_Text Message;
    public Image Background;
    public Image Icon;

    private ShopData shopData;
    private static readonly Color32 MsgBgColorLight = new Color32(0x00, 0x00, 0x00, 0x24);
    private static readonly Color32 CountBgColorLight = new Color32(0x42, 0x42, 0x42, 0x80);
    private static readonly Color32 MsgBgColorDark = new Color32(0x00, 0x00, 0x00, 0x48);
    private static readonly Color32 CountBgColorDark = new Color32(0xA9, 0xA9, 0xA9, 0x80);

    public ShopData GetData()
    {
        return shopData;
    }

    public void Scroll(int index, ShopData data, float size, int constraintCount, bool visible)
    {
        var rectTransform = transform as RectTransform;

        rectTransform.anchoredPosition = new Vector2(index / constraintCount * size, 0);
        float min = index % constraintCount;
        float max = (index + 1) % constraintCount;
        if (max <= 0) { max = constraintCount; }
        rectTransform.anchorMin = new Vector2(0, 1 - max / constraintCount);
        rectTransform.anchorMax = new Vector2(0, 1 - min / constraintCount);
        rectTransform.sizeDelta = new Vector2(size, 0);

        if (visible)
        {
            shopData = data;
            Count.text = data.Count;
            Price.text = data.Price;
            if (string.IsNullOrEmpty(data.Message))
            {
                Message.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                Message.text = data.Message;
                Message.transform.parent.gameObject.SetActive(true);
            }
            Background.color = GetColor(data.BackgroundColor);
            Icon.sprite = data.Icon;
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private Color GetColor(string htmlString)
    {
        Color color;
        if (ColorUtility.TryParseHtmlString(htmlString, out color))
        {
            return color;
        }
        return Color.clear;
    }
}
