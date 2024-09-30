using TMPro;
using UltrakillBingoClient;
using UnityEngine;
using UnityEngine.UI;

using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.UI_Elements;

public static class UIHelper
{
    public static GameObject CreateInput()
    {
        GameObject input = new GameObject();
        input.name = "InputField";
        input.AddComponent<RectTransform>();
        input.AddComponent<CanvasRenderer>();
        input.AddComponent<Image>();
        input.AddComponent<TMP_InputField>();
        
        //Add sprite to img
        input.GetComponent<Image>().sprite = AssetLoader.UISprite;
        input.GetComponent<Image>().fillCenter = false;
        input.GetComponent<Image>().fillClockwise = true;
        input.GetComponent<Image>().type = Image.Type.Sliced;
        
        GameObject numArea = new GameObject();
        numArea.name = "Id Area";
        numArea.transform.SetParent(input.transform);
        numArea.AddComponent<RectTransform>();
        numArea.AddComponent<RectMask2D>();
        
        GameObject numAreaCaret = new GameObject();
        numAreaCaret.name = "Caret";
        numAreaCaret.transform.SetParent(numArea.transform);
        numAreaCaret.AddComponent<RectTransform>();
        numAreaCaret.AddComponent<CanvasRenderer>();
        numAreaCaret.AddComponent<TMP_SelectionCaret>();
        
        GameObject numAreaText = new GameObject();
        numAreaText.name = "Text";
        numAreaText.transform.SetParent(numArea.transform);
        numAreaText.AddComponent<RectTransform>();
        numAreaText.AddComponent<CanvasRenderer>();
        numAreaText.AddComponent<TextMeshProUGUI>();
        
        return input;
    }
    
    //NOTE - below code was borrowed from ZedDev's UKUIHelper, but with some things modified/removed to prevent errors.
        
        public static GameObject CreateButton(string buttonText = "Text",string buttonName = "Button",float rectX = 200f, float rectY = 50f, int fontSize = 32)
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
          button.AddComponent<CanvasRenderer>();
          button.AddComponent<Image>();
          
          //Add sprite to img
          button.GetComponent<Image>().sprite = AssetLoader.UISprite;
          button.GetComponent<Image>().fillCenter = false;
          button.GetComponent<Image>().fillClockwise = true;
          
          button.AddComponent<Button>();
          button.GetComponent<RectTransform>().sizeDelta = new Vector2(rectX, rectY);
          button.GetComponent<Image>().type = Image.Type.Sliced;
          button.GetComponent<Button>().targetGraphic = (Graphic) button.GetComponent<Image>();
          GameObject text = CreateText();
          button.GetComponent<Button>().colors = colors;
          text.name = "Text";
          text.GetComponent<RectTransform>().SetParent((Transform) button.GetComponent<RectTransform>());
          text.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
          text.GetComponent<TextMeshProUGUI>().text = buttonText;
          text.GetComponent<TextMeshProUGUI>().font = AssetLoader.gameFont;
          text.GetComponent<TextMeshProUGUI>().fontSize = fontSize;
          text.GetComponent<TextMeshProUGUI>().color = Color.white;
          text.GetComponent<TextMeshProUGUI>().enableWordWrapping = false;
          text.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
          return button;
        }

        public static GameObject CreateText(string textString="Text", int fontSize=32,string idName="Text")
        { 
          GameObject text = new GameObject();
          text.name = idName;
          text.AddComponent<RectTransform>();
          text.AddComponent<CanvasRenderer>();
          text.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 50f);
          text.AddComponent<TextMeshProUGUI>(); //Crashes here?
          text.GetComponent<TextMeshProUGUI>().text = textString;
          text.GetComponent<TextMeshProUGUI>().font = AssetLoader.gameFont;
          text.GetComponent<TextMeshProUGUI>().fontSize = fontSize;
          text.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
          text.GetComponent<TextMeshProUGUI>().color = Color.white;
          text.GetComponent<TextMeshProUGUI>().enableWordWrapping = false;
          return text;
        }
}