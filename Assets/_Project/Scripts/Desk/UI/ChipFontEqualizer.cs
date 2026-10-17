using TMPro;
using UnityEngine;

namespace CivilJam.Desk
{
    /// <summary>
    /// (EEE, 18 Ekim) El Kitabı'ndaki bir çip satırında (C16 … C35, Dolgu … Kaya) her çip kendi boyunu seçince satır
    /// dengesiz görünüyordu ("Yumuşak kil" minik, "Sert kil" kocaman). Satır yerleştikten sonra her yazının sığdığı boy
    /// ölçülür, en küçüğü hepsine sabitlenir. Sayfa ilk görününce bir kez çalışır.
    /// </summary>
    public class ChipFontEqualizer : MonoBehaviour
    {
        public TextMeshProUGUI[] Texts;
        int settleFrames;

        void LateUpdate()
        {
            if (Texts == null || Texts.Length == 0 || Texts[0] == null) { enabled = false; return; }
            if (Texts[0].rectTransform.rect.width < 2f) return; // yerleşim henüz hesaplanmadı
            if (settleFrames++ < 1) return;                    // bir kare daha bekle (yerleşim otursun)

            float size = float.MaxValue;
            foreach (var t in Texts)
            {
                t.enableAutoSizing = true;
                t.ForceMeshUpdate();
                size = Mathf.Min(size, t.fontSize);
            }
            foreach (var t in Texts)
            {
                t.enableAutoSizing = false;
                t.fontSize = size;
            }
            enabled = false;
        }
    }
}
