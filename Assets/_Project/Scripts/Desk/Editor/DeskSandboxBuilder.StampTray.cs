using UnityEngine;

namespace CivilJam.Desk.Editor
{
    // Kaşe çekmecesi (belge turu B2; bileşen Onur'un, Docs/Onur/StampTrayView_baglanti.md): El Kitabı'nın sağında, ekranın sağ
    // kenarında kaşe düğmesi; basınca çekmece El Kitabı'nın üstüne sola doğru açılır. Kendi katmanında: belgelerin üstünde,
    // İNCELE karartmasının altında (çekmece açıkken karartmanın üstüne alınması 7. adımda). Görünümü bileşen kendisi kurar.
    public static partial class DeskSandboxBuilder
    {
        private static readonly Vector2 StampTraySize = new Vector2(640, 260); // Onur'un önerisi: düğme sağda 110x110, çekmece solunda
        private const float StampTrayRightX = 956f;  // tuval ortasına göre sağ kenar (ekranın sağ kenarına 4 px)
        private const float StampTrayCenterY = -100f; // düğmenin ortası: El Kitabı sekmelerinin (HIZLI BAKIŞ / LAB) altında

        private static StampTrayView BuildStampTray(RectTransform root)
        {
            var layer = Stretch(NewRect("StampTray", root));

            var rect = NewRect("Tray", layer);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.sizeDelta = StampTraySize;
            rect.anchoredPosition = new Vector2(StampTrayRightX, StampTrayCenterY);

            var tray = rect.gameObject.AddComponent<StampTrayView>();
            Wire(tray, "buttonSprite", FindSprite("btn_stamps"));
            Wire(tray, "traySprite", FindSprite("stamp_tray"));
            Wire(tray, "approveSprite", FindSprite("stamp_approve"));
            Wire(tray, "rejectSprite", FindSprite("stamp_reject"));
            Wire(tray, "titleFont", TitleFont);
            return tray;
        }
    }
}
