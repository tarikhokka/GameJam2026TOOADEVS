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
        private static readonly Vector2 InspectButtonSize = new Vector2(160, 46); // EEE btn_inspect 420x120 oranı (poliş B2); küçük: lab raporunun başlığına ve portreye az biner (Oğuzhan)

        private static InspectController BuildInspect(RectTransform root, RectTransform papers)
        {
            // Boş masa: belgelerin arkasında (Papers'ın ilk çocuğu), masa kenarından aşağısı. Raycast'i sadece mod açıkken açık.
            var catcherRect = Anchor(NewRect("InspectCatcher", papers), Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0, -DeskEdgeY));
            catcherRect.SetAsFirstSibling();
            var catcherImage = AddImage(catcherRect, Color.clear);
            catcherImage.raycastTarget = false;
            var catcher = catcherRect.gameObject.AddComponent<InspectCatcher>();

            // Karartma (poliş B1, Ahmet K2): belgelerin hemen üstünde, İNCELE katmanının altında; tıklama almaz. Mod açıkken
            // ofis, portre, üst bar ve belgeler hafifçe kararır; İNCELE katmanı (düğme, çizgi, kaşe) ve diyalog katmanı bunun
            // üstünde olduğu için aydınlık kalır. Alfayı ve geçişi InspectController yönetir (kapalıyken pasif).
            var dimRect = Stretch(NewRect("InspectDim", root));
            var dim = AddImage(dimRect, new Color(0f, 0f, 0f, 0f));
            dim.raycastTarget = false;
            dimRect.gameObject.SetActive(false);

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

            // İNCELE düğmesi (yer: EEE, 17 Ekim): ortada, lab raporunun hemen üstünde. Görsel: btn_inspect (EEE T1, ONAY / RED ailesi; poliş B2).
            var homes = DocHomes();
            float labX = 960f + homes.lab;
            var buttonRect = TopLeft(NewRect("InspectButton", overlay), InspectButtonSize, labX - InspectButtonSize.x * 0.5f,
                DeskEdgeY - 10); // y 370-416: masa kenarında, lab raporu (y 420) başlamadan biter
            var buttonImage = SpriteImage(buttonRect, FindSprite("btn_inspect"), new Color(0.2f, 0.17f, 0.15f));
            buttonImage.raycastTarget = true;
            var button = buttonRect.gameObject.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            var colors = button.colors;
            colors.highlightedColor = new Color(0.92f, 0.92f, 0.92f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            button.colors = colors;
            // Görselde yazı yok (EEE T1): solda büyüteç ikonu, yazı ikonun sağındaki alana kodla (Paytone One, krem).
            var label = NewText("Label", buttonRect, "İNCELE", 28, LightText, TextAlignmentOptions.Center, FontStyles.Normal);
            label.font = TitleFont;
            label.characterSpacing = 2;
            label.enableAutoSizing = true;
            label.fontSizeMin = 12;
            label.fontSizeMax = 18;
            Anchor(label.rectTransform, new Vector2(0.27f, 0f), Vector2.one, new Vector2(0, 4), new Vector2(-10, -4));

            var controller = root.gameObject.AddComponent<InspectController>();
            Wire(controller, "toggleButton", button);
            Wire(controller, "toggleImage", buttonImage);
            Wire(controller, "catcher", catcher);
            Wire(controller, "catcherGraphic", catcherImage);
            Wire(controller, "dim", dim);
            Wire(controller, "overlay", overlay);
            Wire(controller, "line", line);
            Wire(controller, "resultView", resultView);
            SetObjects(controller, "topLayers", overlay.gameObject);
            return controller;
        }

        // Diyalog kurulduktan sonra (BuildPrefab): karartmanın üstüne alınacaklar. Müteahhit balonu bir kat, İNCELE katmanı ve
        // diyalog katmanı (kilit + denetçi balonu) iki kat yukarı (InspectController.SetMode). (belge turu B2: ONAY / RED kalktı)
        private static void WireInspectLift(InspectController controller, ClientSceneView scene, DeskDialogue dialogue)
        {
            var balloon = new UnityEditor.SerializedObject(scene).FindProperty("balloonRoot").objectReferenceValue as GameObject;
            SetObjects(controller, "keepLit", balloon);

            var overlay = new UnityEditor.SerializedObject(controller).FindProperty("overlay").objectReferenceValue as Component;
            SetObjects(controller, "topLayers", overlay != null ? overlay.gameObject : null, dialogue.gameObject);
        }

        // GameObject dizisi alanı (Wire tek nesne bağlar).
        private static void SetObjects(Object target, string field, params GameObject[] values)
        {
            var so = new UnityEditor.SerializedObject(target);
            var list = so.FindProperty(field);
            list.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
