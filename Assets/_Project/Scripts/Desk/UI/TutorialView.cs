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
            new Card("Ulusal Yapı Denetim Kurumu'na hoş geldin",
                "Müteahhitler sırayla gelip karşına oturacak. Senin imzan olmadan hiçbir bina yükselmez.\n\n" +
                "Müteahhit masaya ortadaki <b>YAPI DENETİM FORMU</b>'nu ve soldaki mavi <b>LAB RAPORU</b>'nu bırakır. " +
                "Tıklayınca açılırlar; kağıtları sürükleyip yan yana koyabilirsin. Kararı formdaki <b>ONAY / RED</b> kutularıyla verirsin."),
            new Card("Değerleri kontrol et",
                "Formdaki kat sayısına bak, sağdaki yeşil <b>EL KİTABI</b>'nda o kat grubunun en düşük değerlerini bul.\n\n" +
                "Beyan edilen her değer bu sınırı karşılamalı. Tek bir değer bile eksikse bina reddedilir."),
            new Card("Belgeler sahte olabilir",
                "Parsel, tarih ve imza formda ve lab raporunda aynı olmalı. Kaşedeki laboratuvar, raporun başlığındakiyle aynı olmalı ve akredite listede bulunmalı.\n\n" +
                "Beyana güvenmiyorsan lab raporundan ücretli <b>ek test</b> iste: gerçek değerleri gösterir."),
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
