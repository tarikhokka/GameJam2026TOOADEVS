using TMPro;
using UnityEngine;

namespace CivilJam.Desk
{
    /// <summary>
    /// (EEE, 18 Ekim) Form ve lab raporundaki yazılar masada küçük ve soluktu (jüri ayakta, arkadan da okuyabilsin).
    /// Belgenin altındaki her yazı en çok <c>factor</c> kat büyür ama kutusuna sığdığı kadar (TMP otomatik boyut):
    /// sığmayan kendi boyutunda kalır, taşma olmaz. Mürekkep renkli (koyu, renksiz) yazılar ayrıca koyulaşır ve kalınlaşır;
    /// renkli yazılara (EK TEST'in beyaz yazısı, kırmızı / yeşil kaşeler) dokunulmaz.
    /// Her yazıya bir kez uygulanır (havuzdan çoğaltılan satırlar işareti de kopyalar, tekrar işlenmez).
    /// </summary>
    public class DocTextBoost : MonoBehaviour
    {
        public static void Apply(Transform root, float factor)
        {
            if (root == null) return;
            foreach (var t in root.GetComponentsInChildren<TMP_Text>(true))
            {
                if (t.GetComponent<DocTextBoost>() != null) continue;
                t.gameObject.AddComponent<DocTextBoost>();
                float size = t.enableAutoSizing ? t.fontSizeMax : t.fontSize;
                float min = t.enableAutoSizing ? Mathf.Min(t.fontSizeMin, size) : size;
                t.enableAutoSizing = true;
                t.fontSizeMin = min;
                t.fontSizeMax = size * factor;

                Color.RGBToHSV(t.color, out _, out float s, out float v);
                if (s < 0.5f && v < 0.7f) // mürekkep: koyu kahve / gri
                {
                    var c = t.color;
                    t.color = new Color(c.r * 0.5f, c.g * 0.5f, c.b * 0.5f, Mathf.Max(c.a, 0.9f));
                    t.fontStyle |= FontStyles.Bold;
                    // Kalın stilde TMP "…" karakterini bulamıyor ve her seferinde uyarı yazıp kendisi Truncate'e geçiyordu;
                    // aynı sonucu baştan veriyoruz (davranış değişmez, Console'daki sarı uyarılar kalkar).
                    if (t.overflowMode == TextOverflowModes.Ellipsis) t.overflowMode = TextOverflowModes.Truncate;
                }
            }
        }
    }
}
