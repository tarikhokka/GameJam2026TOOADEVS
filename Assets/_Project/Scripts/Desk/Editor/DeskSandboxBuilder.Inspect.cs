using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk.Editor
{
    // İnceleme modu (Ahmet onayı, memory.md "İnceleme modu"): İNCELE düğmesi, belgelerin arkasında boş masa alanı (tıklayınca
    // seçim temizlenir), belgelerin üstünde çizgi + sonuç katmanı. InspectController DeskScreen kökünde (alanlar onu yukarıda arar).
    public static partial class DeskSandboxBuilder
    {
        private static readonly Color InspectLine = new Color(0.86f, 0.58f, 0.06f);
        private static readonly Vector2 InspectButtonSize = new Vector2(184, 46); // EEE btn_wood 420x100 oranı

        private static InspectController BuildInspect(RectTransform root, RectTransform papers)
        {
            // Boş masa: belgelerin arkasında (Papers'ın ilk çocuğu), masa kenarından aşağısı. Raycast'i sadece mod açıkken açık.
            var catcherRect = Anchor(NewRect("InspectCatcher", papers), Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0, -DeskEdgeY));
            catcherRect.SetAsFirstSibling();
            var catcherImage = AddImage(catcherRect, Color.clear);
            catcherImage.raycastTarget = false;
            var catcher = catcherRect.gameObject.AddComponent<InspectCatcher>();

            // Belgelerin üstünde: çizgi, sonuç kartı ve İNCELE düğmesi. Katman tıklama almaz (düğme hariç).
            var overlay = Stretch(NewRect("Inspect", root));

            var line = NewRect("Line", overlay);
            line.anchorMin = line.anchorMax = new Vector2(0.5f, 0.5f);
            line.pivot = new Vector2(0f, 0.5f); // ilk alandan ikinciye doğru uzar
            AddImage(line, InspectLine).raycastTarget = false;
            line.gameObject.SetActive(false);

            // Sonuç kaşesi (Onur, Docs/Onur/InspectResultView_baglanti.md): kartı, hareketi ve sesi kendisi kurar; tam ekran katmanın
            // çocuğu, konumu katmanın merkezine göre (çizginin ortası). Fontlar ve sesler burada bağlanır.
            var result = NewRect("Result", overlay);
            var resultView = result.gameObject.AddComponent<InspectResultView>();
            Wire(resultView, "titleFont", TitleFont);
            Wire(resultView, "detailFont", ValueFont);
            Wire(resultView, "click", FindClip("sfx_click"));
            Wire(resultView, "stamp", FindClip("sfx_stamp"));

            // İNCELE düğmesi: form ile lab raporunun arasında, masa kenarının üstünde (belgelerin hemen üstü).
            var homes = DocHomes();
            float gapX = 960f + homes.form + DocWidth("doc_form") * 0.5f + DocGap * 0.5f;
            var buttonRect = TopLeft(NewRect("InspectButton", overlay), InspectButtonSize, gapX - InspectButtonSize.x * 0.5f,
                DeskEdgeY - 10);
            var buttonImage = SpriteImage(buttonRect, FindSprite("btn_wood"), new Color(0.45f, 0.3f, 0.18f));
            buttonImage.raycastTarget = true;
            var button = buttonRect.gameObject.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            var colors = button.colors;
            colors.highlightedColor = new Color(0.92f, 0.92f, 0.92f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            button.colors = colors;
            var label = NewText("Label", buttonRect, "İNCELE", 22, LightText, TextAlignmentOptions.Center, FontStyles.Normal);
            label.font = BoldFont;
            label.characterSpacing = 4;
            Stretch(label.rectTransform);

            var controller = root.gameObject.AddComponent<InspectController>();
            Wire(controller, "toggleButton", button);
            Wire(controller, "toggleImage", buttonImage);
            Wire(controller, "catcher", catcher);
            Wire(controller, "catcherGraphic", catcherImage);
            Wire(controller, "overlay", overlay);
            Wire(controller, "line", line);
            Wire(controller, "resultView", resultView);
            return controller;
        }
    }
}
