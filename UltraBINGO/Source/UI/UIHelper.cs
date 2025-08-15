using TMPro;
using UltraBINGO.Util;
using UnityEngine;

namespace UltraBINGO.UI;

public static class UIHelper {
    public static GameObject CreateText(string textString = "Text", int fontSize = 32, string idName = "Text") {
        var text = new GameObject {
            name = idName
        };

        text.GetOrAddComponent<RectTransform>().sizeDelta = new Vector2(200f, 50f);
        text.AddComponent<CanvasRenderer>();

        var textMeshPro = text.GetOrAddComponent<TextMeshProUGUI>();

        textMeshPro.text = textString;
        textMeshPro.font = AssetLoader.GameFont;
        textMeshPro.fontSize = fontSize;
        textMeshPro.alignment = TextAlignmentOptions.Center;
        textMeshPro.color = Color.white;
        textMeshPro.enableWordWrapping = false;

        return text;
    }
}