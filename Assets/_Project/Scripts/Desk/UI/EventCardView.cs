using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // Masada beklenmedik bir olay olunca (ör. kabadayı camı kırdı) ortada açılan kart. "Tamam" ile kapanır.
    // Açıkken arkadaki masaya tıklanamaz.
    public class EventCardView : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private Button okButton;

        private Action onClosed;

        private void Awake() => okButton.onClick.AddListener(Dismiss);

        public void Open(string title, string body, Action closed)
        {
            titleText.text = title;
            bodyText.text = body;
            onClosed = closed;
            panelRoot.SetActive(true);
        }

        // Sessiz kapatma (masa kapanırken); geri çağrı çalışmaz.
        public void Close()
        {
            onClosed = null;
            panelRoot.SetActive(false);
        }

        private void Dismiss()
        {
            var callback = onClosed;
            Close();
            callback?.Invoke();
        }
    }
}
