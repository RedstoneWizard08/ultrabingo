using UnityEngine.UI;

namespace UltraBINGO.Util;

public static class Extensions {
    public static void SetInteractable(this Button button, bool interactable) {
        button.interactable = interactable;
    }

    public static void SetText(this Text text, string value) {
        text.text = value;
    }
}