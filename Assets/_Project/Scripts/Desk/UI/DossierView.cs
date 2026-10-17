using System;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // Açık dosya: LAB RAPORU kitabına tıklayınca ekranın ortasında açılır (belge + karar sütunu), oda hafifçe kararır.
    // "Kapat" ya da perdeye tıklama kapatır. Bileşen hep aktif bir objede; açılıp kapanan panelRoot.
    public class DossierView : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button backdropButton;

        public event Action CloseClicked;

        public bool IsOpen => panelRoot.activeSelf;

        private void Awake()
        {
            closeButton.onClick.AddListener(() => CloseClicked?.Invoke());
            backdropButton.onClick.AddListener(() => CloseClicked?.Invoke());
        }

        public void Open() => panelRoot.SetActive(true);

        public void Close() => panelRoot.SetActive(false);
    }
}
