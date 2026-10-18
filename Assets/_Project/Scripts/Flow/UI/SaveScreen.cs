using CivilJam.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

            // (EEE, 17 Ekim) Kağıt kart + Paytone başlık + Fira Sans metin; düğme FlowScreen'de tahta olur.
            UiKit.Card(root, "Card", paperSprite, new Vector2(0.3f, 0.26f), new Vector2(0.7f, 0.78f));
            _title = UiKit.Text(root, "Title", "", 58, UiKit.Ink, TextAlignmentOptions.Center,
                new Vector2(0.32f, 0.66f), new Vector2(0.68f, 0.76f), true, 30);
            UiKit.StyleInk(_title, titleFont);
            UiKit.Image(root, "Rule", null, new Color(UiKit.Ink.r, UiKit.Ink.g, UiKit.Ink.b, 0.3f),
                new Vector2(0.35f, 0.655f), new Vector2(0.65f, 0.658f));
            _body = UiKit.Text(root, "Body", "", 34, UiKit.Ink, TextAlignmentOptions.Center,
                new Vector2(0.33f, 0.49f), new Vector2(0.67f, 0.64f), true, 20);
            _stats = UiKit.Text(root, "Stats", "", 30, new Color(UiKit.Ink.r, UiKit.Ink.g, UiKit.Ink.b, 0.75f), TextAlignmentOptions.Center,
                new Vector2(0.33f, 0.38f), new Vector2(0.67f, 0.48f), true, 18);
            UiKit.StyleInk(_body, bodyFont);
            UiKit.StyleInk(_stats, infoFont);

            // Kartın sağ altına yamuk basılmış kırmızı kaşe
            var stamp = _stamp = UiKit.Text(root, "Stamp", "KAYDEDİLDİ", 46, new Color(0.72f, 0.16f, 0.12f, 0.85f), TextAlignmentOptions.Center,
                new Vector2(0.52f, 0.28f), new Vector2(0.69f, 0.37f), true, 24);
            stamp.fontStyle = FontStyles.Bold;
            if (titleFont != null) { stamp.font = titleFont; stamp.fontStyle = FontStyles.Normal; } // kaşe yazısı Paytone
            stamp.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 12f);
            var frame = UiKit.Image(stamp.transform, "Frame", null, new Color(0.72f, 0.16f, 0.12f, 0.12f), Vector2.zero, Vector2.one);
            frame.transform.SetAsFirstSibling();
            _stampFrame = frame;

            // (Tarık, Onur'un incelemesi P0) Kayıt başarısızsa: "Tekrar dene" (aynı günü yeniden yazar, günü ilerletmez)
            _retry = UiKit.Button(root, "Retry", "Kaydı tekrar dene", Retry,
                new Vector2(0.39f, 0.235f), new Vector2(0.61f, 0.295f), 26f);
            _retry.gameObject.SetActive(false);

            var button = UiKit.Button(root, "Continue", "", Finish,
                new Vector2(0.39f, 0.16f), new Vector2(0.61f, 0.23f), 30f);
            _buttonLabel = button.GetComponentInChildren<TextMeshProUGUI>();
        }

        TextMeshProUGUI _stamp;
        Image _stampFrame;
        Button _retry;
        static readonly Color Red = new Color(0.72f, 0.16f, 0.12f, 0.85f);
        static readonly Color Gray = new Color(0.25f, 0.22f, 0.2f, 0.85f);

        // LastSave yoksa (eski akış / sandbox) başarılı sayılır.
        void ShowSaveResult(int finished)
        {
            var r = Context.LastSave;
            bool ok = r == null || r.Ok;
            _stamp.text = ok ? "KAYDEDİLDİ" : "KAYDEDİLEMEDİ";
            _stamp.color = ok ? Red : Gray;
            _stampFrame.color = new Color(_stamp.color.r, _stamp.color.g, _stamp.color.b, 0.12f);
            _retry.gameObject.SetActive(!ok);
            if (ok) return;
            _title.text = $"{finished}. Gün kaydedilemedi";
            _body.text = "Kayıt dosyası yazılamadı. Oyuna devam edebilirsin ama çıkarsan bu gün kaybolur." +
                (string.IsNullOrEmpty(r.Error) ? "" : $"\n<size=70%>({r.Error})</size>");
        }

        void Retry()
        {
            Context.RetrySave();
            Refresh();
        }

        protected override void Refresh()
        {
            var s = Context.State;
            // Save fazına girerken Core günü bir artırıp kaydetmiş olur: biten gün = Day - 1.
            int finished = Mathf.Max(1, s.Day - 1);
            _title.text = $"{finished}. Gün kaydedildi";
            _body.text = "Dosyalar arşive kaldırıldı, ofisin ışıkları söndü.\nYarın masanda yeni başvurular olacak.";
            _stats.text = $"Kasa: {UiKit.Money(s.Money)}    ·    Borç: {UiKit.Money(s.Debt)}";
            ShowSaveResult(finished);
            if (_buttonLabel != null) _buttonLabel.text = $"{s.Day}. güne geç  →";
        }
    }
}
