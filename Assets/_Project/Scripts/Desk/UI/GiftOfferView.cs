using System;
using CivilJam.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // Hediye kartı (Ç4, Ahmet kararı): onaydan sonra, tepki balonuyla aynı anda "Rıfat Usta size bir ahşap bina maketi bıraktı."
    // + hediyenin notu ve görseli. Sadece "Kabul et" var (geri çevrilemez; para/risk etkisi yok). Kabul edilen eşya masada kalır
    // (OfficeGiftsView). Kağıt pencerelerle aynı piksel tasarım; açılış/kapanış PopupMotion.
    public class GiftOfferView : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Image giftImage;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private TMP_Text noteText;
        [SerializeField] private Button acceptButton;

        private Action accepted;

        public bool IsOpen => panelRoot.activeSelf && accepted != null;

        private void Awake() => acceptButton.onClick.AddListener(Accept);

        public void Open(GiftSO gift, string giverName, Action onAccepted)
        {
            accepted = onAccepted;
            giftImage.sprite = gift.Sprite;
            giftImage.enabled = gift.Sprite != null;
            string item = string.IsNullOrEmpty(gift.DisplayName) ? "hediye" : gift.DisplayName.ToLower(System.Globalization.CultureInfo.GetCultureInfo("tr-TR"));
            bodyText.text = $"{giverName} size bir <b>{item}</b> bıraktı.";
            noteText.text = string.IsNullOrWhiteSpace(gift.Note) ? "" : $"<i>“{gift.Note.Trim()}”</i>";
            PopupMotion.On(panelRoot).Show();
        }

        // Sessiz kapatma (masa kapanırken): animasyonsuz, geri çağrı çalışmaz.
        public void Close()
        {
            accepted = null;
            PopupMotion.On(panelRoot).HideInstant();
        }

        private void Accept()
        {
            var callback = accepted;
            accepted = null;
            PopupMotion.On(panelRoot).Hide();
            callback?.Invoke();
        }
    }
}
