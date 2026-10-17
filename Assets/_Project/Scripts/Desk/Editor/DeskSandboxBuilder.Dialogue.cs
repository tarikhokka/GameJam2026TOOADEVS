using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk.Editor
{
    // Uyuşmazlık diyaloğu (Ahmet kararı 17 Ekim ~19:20): inceleme katmanının üstünde tam ekran tıklama katmanı (diyalog sürerken
    // masayı kilitler, tıklama satırı tamamlar / geçer) ve denetçi balonu (Onur'un InspectorBubbleView'u, Docs/Onur/InspectorBubbleView_baglanti.md).
    public static partial class DeskSandboxBuilder
    {
        private static DeskDialogue BuildDialogue(RectTransform root, ClientSceneView scene)
        {
            var layer = Stretch(NewRect("Dialogue", root));
            var blocker = AddImage(layer, Color.clear);
            blocker.raycastTarget = false; // sadece diyalog sürerken açık (DeskDialogue)

            // Denetçi balonu: kendi kağıdını, kuyruğunu, etiketini ve yazısını kurar. Masa tarafında, belgelerin üst kenarının hemen
            // üstünde, solda (müteahhit balonu sağda); konum katmanın merkezine göre. Lab raporunun üstündeki İNCELE düğmesine
            // (EEE, x ~760-1020) binmesin diye 600 px genişlikte ve x 160-760 arasında.
            var bubble = NewRect("InspectorBubble", layer);
            bubble.anchorMin = bubble.anchorMax = bubble.pivot = new Vector2(0.5f, 0.5f);
            bubble.anchoredPosition = new Vector2(-500f, 130f);
            var bubbleView = bubble.gameObject.AddComponent<InspectorBubbleView>();
            var bubbleSo = new UnityEditor.SerializedObject(bubbleView);
            bubbleSo.FindProperty("preferredWidth").floatValue = 600f;
            bubbleSo.ApplyModifiedPropertiesWithoutUndo();
            Wire(bubbleView, "bodyFont", ValueFont);
            Wire(bubbleView, "labelFont", BoldFont);
            Wire(bubbleView, "typingClip", FindClip("sfx_typing"));

            var dialogue = layer.gameObject.AddComponent<DeskDialogue>();
            Wire(dialogue, "blocker", blocker);
            Wire(dialogue, "client", scene);
            Wire(dialogue, "inspectorBubble", bubbleView);
            return dialogue;
        }
    }
}
