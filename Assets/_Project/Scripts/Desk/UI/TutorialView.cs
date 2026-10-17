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
                "Önündeki üç belgeyi soldan sağa karşılaştır: <b>YAPI DENETİM FORMU</b>, <b>LABORATUVAR RAPORU</b> ve " +
                "<b>DENETÇİ EL KİTABI</b>. Kararı formdaki <b>ONAY / RED</b> düğmeleriyle verirsin."),
            new Card("Değerleri kontrol et",
                "Formdaki <b>KAT ADEDİ</b>'ni oku, El Kitabı'nın <b>HIZLI BAKIŞ</b> sayfasında kat grubunu seç.\n\n" +
                "Beyan edilen her değer yeşil alanda olmalı. Tek bir kırmızı bile varsa bina reddedilir."),
            new Card("Belgeler sahte olabilir",
                "Parsel, tarih ve müellif imzası formda ve lab raporunda aynı olmalı. Kaşedeki laboratuvar, raporun başındakiyle aynı olmalı ve El Kitabı'nın <b>LAB</b> sekmesindeki akredite listede bulunmalı.\n\n" +
                "Beyana güvenmiyorsan lab raporundan ücretli <b>EK TEST</b> iste: gerçek değerleri gösterir."),
            new Card("İmza senin",
                "Bazı müteahhitler masaya zarf bırakır. Karar senin.\n\n" +
                "Onayladığın her bina bir gün afetle sınanacak. Kuralları unutursan sağdaki <b>EL KİTABI</b>'na bak."),
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
