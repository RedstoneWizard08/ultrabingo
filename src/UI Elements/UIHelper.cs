using TMPro;
using UltrakillBingoClient;
using UnityEngine;
using UnityEngine.UI;

using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.UI_Elements;

public static class UIHelper
{
    public static GameObject CreateText(string textString="Text", int fontSize=32,string idName="Text")
    { 
        GameObject text = new GameObject();
        text.name = idName;
        text.AddComponent<RectTransform>();
        text.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 50f);
        text.AddComponent<CanvasRenderer>();
        text.AddComponent<TextMeshProUGUI>();
        text.GetComponent<TextMeshProUGUI>().text = textString;
        text.GetComponent<TextMeshProUGUI>().font = AssetLoader.GameFont;
        text.GetComponent<TextMeshProUGUI>().fontSize = fontSize;
        text.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        text.GetComponent<TextMeshProUGUI>().color = Color.white;
        text.GetComponent<TextMeshProUGUI>().enableWordWrapping = false;
        return text;
    }
    
    public static GameObject CreateButtonLegacy(string buttonText = "Text",string buttonName = "Button",float rectX = 200f, float rectY = 50f, int fontSize = 32)
    { 
        ColorBlock colors = new ColorBlock()
        {
            normalColor = new Color(1,1,1,1),
            highlightedColor = new Color(1,1,1,0.502f),
            pressedColor = new Color(1,0,0,1),
            selectedColor = new Color(1,1,1,1),
            disabledColor = new Color(0,0,0,1f),
            colorMultiplier = 1f,
            fadeDuration = 0.1f
        }; 
            
        GameObject button = new GameObject();
        button.name = buttonName;
        button.AddComponent<RectTransform>();
        button.GetComponent<RectTransform>().sizeDelta = new Vector2(rectX, rectY);
        button.AddComponent<CanvasRenderer>();
        button.AddComponent<Image>();
        button.GetComponent<Image>().sprite = AssetLoader.UISprite;
        button.GetComponent<Image>().fillCenter = false;
        button.GetComponent<Image>().fillClockwise = true;
        button.GetComponent<Image>().type = Image.Type.Sliced;
        button.AddComponent<Button>();
        button.GetComponent<Button>().targetGraphic = (Graphic) button.GetComponent<Image>();
        
        GameObject text = CreateTextLegacy(buttonText,fontSize);
        button.GetComponent<Button>().colors = colors;
        text.GetComponent<RectTransform>().SetParent((Transform) button.GetComponent<RectTransform>());
        text.GetComponent<RectTransform>().sizeDelta = new Vector2(150f,50f);
        text.name = "Text";

        return button;
    }
        
    public static GameObject CreateTextLegacy(string textString="Text", int fontSize=32,string idName="Text")
    { 
        GameObject text = new GameObject();
        text.name = idName;
        text.AddComponent<RectTransform>();
        text.AddComponent<CanvasRenderer>();
        text.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 50f);
        text.AddComponent<Text>();
        text.GetComponent<Text>().text = textString;
        text.GetComponent<Text>().font = AssetLoader.GameFontLegacy;
        text.GetComponent<Text>().fontSize = fontSize;
        text.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
        text.GetComponent<Text>().color = Color.white;
        return text;
    }
}