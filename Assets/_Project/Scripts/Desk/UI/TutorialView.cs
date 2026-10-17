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

        [SerializeField] private Card[] cards =
        {
            new Card("İmar Müdürlüğü'ne hoş geldin",
                "Müteahhitler sırayla gelip karşına oturacak. Senin imzan olmadan hiçbir bina yükselmez.\n\n" +
                "Dosyayı açmak için masadaki mavi <b>LAB RAPORU</b> kitabına tıkla; okuyup onayla ya da reddet."),
            new Card("Değerleri kontrol et",
                "Her dosyada iki belge var: <b>Başvuru</b> ve <b>Laboratuvar Raporu</b>.\n\n" +
                "Rapordaki beyan edilen her değer yönetmelikteki alt sınırı karşılamalı. Karşılamıyorsa bina yönetmeliğe aykırıdır."),
            new Card("Belgeler sahte olabilir",
                "Parsel, tarih ve imza iki belgede aynı olmalı. Kaşedeki laboratuvar, raporun başlığındakiyle aynı olmalı ve akredite listede bulunmalı.\n\n" +
                "Beyana güvenmiyorsan ücretli <b>ek test</b> iste: gerçek değerleri gösterir."),
            new Card("İmza senin",
                "Bazı müteahhitler dosyanın arasına zarf koyar. Karar senin.\n\n" +
                "Onayladığın her bina bir gün afetle sınanacak. Kuralları unutursan masadaki yeşil <b>EL KİTABI</b>'na bak."),
        };

        public event Action PageChanged;

        private int page;

        private void Awake()
        {
            nextButton.onClick.AddListener(Next);
            skipButton.onClick.AddListener(Finish);
        }

        public void Open()
        {
            if (cards == null || cards.Length == 0) return;
            page = 0;
            panelRoot.SetActive(true);
            Draw();
        }

        // Sessiz kapatma (masa kapanırken).
        public void Close() => panelRoot.SetActive(false);

        private void Finish()
        {
            Close();
            PageChanged?.Invoke();
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

        private void Draw()
        {
            var card = cards[page];
            titleText.text = card.Title;
            bodyText.text = card.Body;
            pageText.text = $"{page + 1} / {cards.Length}";
            bool last = page == cards.Length - 1;
            nextLabel.text = last ? "Başla" : "İleri";
            skipButton.gameObject.SetActive(!last);
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
