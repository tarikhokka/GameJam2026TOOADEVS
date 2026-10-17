using CivilJam.Core;
using TMPro;
using UnityEngine;

namespace CivilJam.Flow
{
    /// <summary>
    /// Gün sonu kayıt ekranı (GamePhase.Save). Core bu fazda günü ilerletip kaydı zaten yazmış olur;
    /// ekran sadece "ofis kapandı, kayıt alındı" anını gösterir ve oyuncu sonraki güne geçer.
    /// (EEE, 17 Ekim — IMGUI yedek arayüzünün yerine.)
    /// </summary>
    public class SaveScreen : FlowScreen
    {
        [SerializeField] Sprite background;     // save_bg (gece, ışıkları sönmüş ofis)
        public override GamePhase Phase => GamePhase.Save;

        TextMeshProUGUI _title, _body, _stats, _buttonLabel;

        protected override void Build()
        {
            var root = UiKit.Stretch(transform, "Root");
            UiKit.Image(root, "Bg", background, background != null ? Color.white : UiKit.Dark, Vector2.zero, Vector2.one);

            UiKit.Image(root, "Card", null, new Color(UiKit.Paper.r, UiKit.Paper.g, UiKit.Paper.b, 0.95f),
                new Vector2(0.3f, 0.26f), new Vector2(0.7f, 0.78f));
            _title = UiKit.Text(root, "Title", "", 58, UiKit.Ink, TextAlignmentOptions.Center,
                new Vector2(0.32f, 0.66f), new Vector2(0.68f, 0.76f), true, 30);
            _title.fontStyle = FontStyles.Bold;
            UiKit.Image(root, "Rule", null, new Color(UiKit.Ink.r, UiKit.Ink.g, UiKit.Ink.b, 0.3f),
                new Vector2(0.35f, 0.655f), new Vector2(0.65f, 0.658f));
            _body = UiKit.Text(root, "Body", "", 34, UiKit.Ink, TextAlignmentOptions.Center,
                new Vector2(0.33f, 0.49f), new Vector2(0.67f, 0.64f), true, 20);
            _stats = UiKit.Text(root, "Stats", "", 30, new Color(UiKit.Ink.r, UiKit.Ink.g, UiKit.Ink.b, 0.75f), TextAlignmentOptions.Center,
                new Vector2(0.33f, 0.38f), new Vector2(0.67f, 0.48f), true, 18);

            // Kartın sağ altına yamuk basılmış kırmızı kaşe
            var stamp = UiKit.Text(root, "Stamp", "KAYDEDİLDİ", 46, new Color(0.72f, 0.16f, 0.12f, 0.85f), TextAlignmentOptions.Center,
                new Vector2(0.52f, 0.28f), new Vector2(0.69f, 0.37f), true, 24);
            stamp.fontStyle = FontStyles.Bold;
            stamp.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 12f);
            var frame = UiKit.Image(stamp.transform, "Frame", null, new Color(0.72f, 0.16f, 0.12f, 0.12f), Vector2.zero, Vector2.one);
            frame.transform.SetAsFirstSibling();

            var button = UiKit.Button(root, "Continue", "", Finish,
                new Vector2(0.39f, 0.16f), new Vector2(0.61f, 0.23f), 30f);
            _buttonLabel = button.GetComponentInChildren<TextMeshProUGUI>();
        }

        protected override void Refresh()
        {
            var s = Context.State;
            // Save fazına girerken Core günü bir artırıp kaydetmiş olur: biten gün = Day - 1.
            int finished = Mathf.Max(1, s.Day - 1);
            _title.text = $"{finished}. Gün kaydedildi";
            _body.text = "Dosyalar arşive kaldırıldı, ofisin ışıkları söndü.\nYarın masanda yeni başvurular olacak.";
            _stats.text = $"Kasa: {UiKit.Money(s.Money)}    ·    Borç: {UiKit.Money(s.Debt)}";
            if (_buttonLabel != null) _buttonLabel.text = $"{s.Day}. güne geç  →";
        }
    }
}
