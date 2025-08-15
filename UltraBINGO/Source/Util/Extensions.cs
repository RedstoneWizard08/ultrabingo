using System.Threading.Tasks;
using TMPro;
using UltraBINGO.API;
using UnityEngine.UI;

namespace UltraBINGO.Util;

public static class Extensions {
    public static Task TryHandle<T>(this T? packet) where T : IncomingPacket {
        return packet?.Handle() ?? Task.CompletedTask;
    }

    public static void SetInteractable(this Button button, bool interactable) {
        button.interactable = interactable;
    }

    public static void SetText(this Text text, string value) {
        text.text = value;
    }
}