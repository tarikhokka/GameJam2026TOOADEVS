using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // 1. gün masa açılınca gösterilen kısa öğretici kartlar. Kapanana kadar masaya tıklanamaz.
    // Metinler taslak (EEE onaylayacak); kurallar El Kitabı ile aynı, yeni kural anlatmaz.
    public class TutorialView : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private TMP_Text pageText;
        [SerializeField] private Button nextButton;
        [SerializeField] private TMP_Text nextLabel;
        [SerializeField] private Button skipButton;
        // (EEE, 17 Ekim) Kart yeni yazı tiplerine geçer ve yerleşimi düzeltilir (başlık tek satıra sığar, düğmeler makul boyda).
        [SerializeField] private TMP_FontAsset titleFont; // PaytoneOne-Regular SDF
        [SerializeField] private TMP_FontAsset bodyFont;  // FiraSans-Regular SDF
        [SerializeField] private TMP_FontAsset boldFont;  // FiraSans-Bold SDF

        [SerializeField] private Card[] cards =
        {
            new Card("Ulusal Yapı Denetim Kurumu'na hoş geldin",
                "Müteahhitler sırayla gelip karşına oturacak. Senin imzan olmadan hiçbir bina yükselmez.\n\n" +
                "Önündeki üç belgeyi soldan sağa karşılaştır: <b>YAPI DENETİM FORMU</b>, <b>LABORATUVAR RAPORU</b> ve " +
                "<b>DENETÇİ EL KİTABI</b>. Kararı formdaki <b>ONAY / RED</b> düğmeleriyle verirsin."),
            new Card("Değerleri kontrol et",
                "Formdaki <b>KAT ADEDİ</b>'ne bak: El Kitabı'nın <b>HIZLI BAKIŞ</b> sayfası doğru kat grubunu kendisi gösterir.\n\n" +
                "Beyan edilen her değer yeşil alanda olmalı. Tek bir kırmızı bile varsa bina reddedilir."),
            new Card("Belgeler sahte olabilir",
                "Parsel, tarih ve mühendis imzası formda ve lab raporunda aynı olmalı. Kaşedeki laboratuvar El Kitabı'nın <b>LAB</b> sekmesindeki akredite listede bulunmalı.\n\n" +
                "Form ile lab raporunda bir <b>değer farklı</b> yazıyorsa ya yazım hatasıdır ya da biri yalan söylüyordur. " +
                "Hangisi olduğunu lab raporundaki ücretli <b>EK TEST</b> gösterir: gerçek değerleri ortaya çıkarır."),
            new Card("İmza senin",
                "Bir uyuşmazlık bulduğunda bazı müteahhitler masaya zarf bırakır. Almak da reddetmek de senin kararın.\n\n" +
                "Onayladığın her bina bir gün afetle sınanacak. Kuralları unutursan sağdaki <b>EL KİTABI</b>'na bak."),
            new Card("İncele ve karşılaştır", // EEE metni (ICERIK_TASLAK.md "Öğretici: İnceleme kartı")
                "Masadaki <b>İNCELE</b> düğmesine bas ve iki belgeden aynı bilgiye tıkla. Oyun sana <b>SORUN YOK</b>, " +
                "<b>UYUŞMAZLIK SAPTANDI</b> ya da <b>ALAKASIZ</b> der.\n\n" +
                "Bir uyuşmazlık bulursan müteahhide söylersin. İnceleme ücretsizdir; kararı yine sen verirsin."),
        };

        // (EEE, 17 Ekim) Geri düğmesi (Atla'nın kopyası, ilk kartta gizli) ve kartları istenince yeniden açan
        // "NASIL OYNANIR" düğmesi (masanın sağ üst köşesi, üst barın altı; kartın perdesinin altında kalır).
        [SerializeField] private Sprite helpSprite; // btn_help (EEE T1)

        public event Action PageChanged;

        private int page;
        private bool reopened; // oyuncu NASIL OYNANIR'la açtı: son kartta "Başla" yerine "Kapat"
        private Button backButton;

        private void Awake()
        {
            nextButton.onClick.AddListener(Next);
            skipButton.onClick.AddListener(Finish);
            StyleCard();
            BuildBackButton();
            BuildHelpButton();
        }

        private void BuildBackButton()
        {
            var go = Instantiate(skipButton.gameObject, skipButton.transform.parent);
            go.name = "BackButton";
            go.transform.SetSiblingIndex(skipButton.transform.GetSiblingIndex()); // Atla'nın soluna
            backButton = go.GetComponent<Button>();
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(Back);
            var label = go.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = "Geri";
            go.SetActive(false);
        }

        private void BuildHelpButton()
        {
            var parent = panelRoot.transform.parent;
            var go = new GameObject("HelpButton", typeof(RectTransform), typeof(Image), typeof(Button));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.SetSiblingIndex(panelRoot.transform.GetSiblingIndex()); // kartın perdesinin altında kalsın
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 1f);
            rt.sizeDelta = new Vector2(236, 56);
            rt.anchoredPosition = new Vector2(-24f, -84f); // üst barın hemen altı
            var image = go.GetComponent<Image>();
            image.sprite = helpSprite;
            image.color = helpSprite != null ? Color.white : new Color(0.12f, 0.1f, 0.09f, 0.92f);
            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.highlightedColor = new Color(0.92f, 0.92f, 0.92f); // ONAY / RED ile aynı (poliş B2)
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            button.colors = colors;
            button.onClick.AddListener(() =>
            {
                Open(fromHelp: true);
                PageChanged?.Invoke();
            });

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            var lrt = (RectTransform)labelGo.transform;
            lrt.SetParent(rt, false);
            // btn_help'te yazı yok, solda "?" ikonu (EEE T1): yazı ikonun sağına, Paytone One (poliş B2).
            lrt.anchorMin = new Vector2(helpSprite != null ? 0.24f : 0f, 0f); lrt.anchorMax = Vector2.one;
            lrt.offsetMin = new Vector2(0f, 4f); lrt.offsetMax = new Vector2(-10f, -4f);
            var label = labelGo.GetComponent<TextMeshProUGUI>();
            if (titleFont != null) label.font = titleFont;
            else if (boldFont != null) label.font = boldFont;
            label.enableAutoSizing = true;
            label.fontSizeMin = 14;
            label.fontSizeMax = 24;
            label.text = "NASIL OYNANIR";
            label.fontSize = 24;
            label.characterSpacing = 2;
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(1f, 0.96f, 0.86f);
            label.raycastTarget = false;
        }

        private void StyleCard()
        {
            // Başlık: tek satır, sığmazsa küçülür (eskiden iki satıra taşıp metnin üstüne biniyordu).
            if (titleFont != null) titleText.font = titleFont;
            titleText.fontStyle = FontStyles.Normal; // Paytone zaten kalın
            titleText.textWrappingMode = TextWrappingModes.NoWrap;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = 24;
            titleText.fontSizeMax = 44;
            SetLayout(titleText, 64, 0);

            if (bodyFont != null) { bodyText.font = bodyFont; pageText.font = bodyFont; }
            bodyText.lineSpacing = 6;
            SetLayout(bodyText, -1, 1); // kalan yükseklik metnin

            // Alt satır: sabit 64 px; düğmeler makul genişlikte.
            var footer = nextButton.transform.parent;
            SetLayout(footer, 64, 0);
            if (footer.parent != null && footer.parent.TryGetComponent<VerticalLayoutGroup>(out var cardLayout))
                cardLayout.childForceExpandHeight = false;
            SetWidth(skipButton, 150);
            SetWidth(nextButton, 190);
            foreach (var label in new[] { skipButton.GetComponentInChildren<TMP_Text>(), nextLabel })
            {
                if (label == null) continue;
                if (boldFont != null) label.font = boldFont;
                label.fontStyle = FontStyles.Normal;
                label.fontSize = 28;
            }
        }

        private static void SetLayout(Component target, float preferredHeight, float flexibleHeight)
        {
            var element = target.GetComponent<LayoutElement>();
            if (element == null) element = target.gameObject.AddComponent<LayoutElement>();
            element.preferredHeight = preferredHeight;
            element.minHeight = preferredHeight > 0 ? preferredHeight : -1;
            element.flexibleHeight = flexibleHeight;
        }

        private static void SetWidth(Component target, float width)
        {
            var element = target.GetComponent<LayoutElement>();
            if (element == null) element = target.gameObject.AddComponent<LayoutElement>();
            element.preferredWidth = width;
            element.flexibleHeight = 0;
        }

        public void Open(bool fromHelp = false)
        {
            if (cards == null || cards.Length == 0) return;
            reopened = fromHelp;
            page = 0;
            Draw();
            PopupMotion.On(panelRoot).Show(); // kart yaylanarak gelir
        }

        // Sessiz kapatma (masa kapanırken): animasyonsuz.
        public void Close() => PopupMotion.On(panelRoot).HideInstant();

        // Oyuncu kapattı: kart kısa ve hızlanarak kapanır.
        private void Finish()
        {
            PopupMotion.On(panelRoot).Hide();
            PageChanged?.Invoke();
            if (!reopened) CivilJam.Core.JuryHooks.RaiseTutorialFinished(); // (EEE) jüri ipucu: "J'ye bas"
        }

        private void Next()
        {
            if (page >= cards.Length - 1)
            {
                Finish();
                return;
            }
            page++;
            Draw();
            PageChanged?.Invoke();
        }

        private void Back()
        {
            if (page <= 0) return;
            page--;
            Draw();
            PageChanged?.Invoke();
        }

        private void Draw()
        {
            var card = cards[page];
            titleText.text = card.Title;
            bodyText.text = card.Body;
            pageText.text = $"{page + 1} / {cards.Length}";
            bool last = page == cards.Length - 1;
            nextLabel.text = last ? (reopened ? "Kapat" : "Başla") : "İleri";
            skipButton.gameObject.SetActive(!last);
            if (backButton != null) backButton.gameObject.SetActive(page > 0);
        }

        [Serializable]
        private class Card
        {
            public string Title;
            [TextArea(3, 8)] public string Body;

            public Card(string title, string body)
            {
                Title = title;
                Body = body;
            }
        }
    }
}
