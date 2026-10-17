using System;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // Masadaki belge (Yapı Denetim Formu, Lab Raporu, El Kitabı). Yeni hedef arayüz: üç belge masada sabit yerinde (home),
    // yan yana, düz; oyuncu isterse tutup taşır (yeni müteahhitte yine yerinden açılır). Açılınca aşağıdan (fromOffset) kayarak gelir, kapanınca oraya döner (DraggableSheet hareketi).
    // Bileşen hep aktif kökte; açılıp kapanan panelRoot.
    public class PaperWindow : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private DraggableSheet sheet;
        [SerializeField] private Button closeButton; // yeni düzende yok (boş)
        [Tooltip("Açılış/kapanışta kağıdın geldiği yön (tuval px)")]
        [SerializeField] private Vector2 fromOffset = new Vector2(0, -900);
        [Tooltip("Masadaki sabit yeri (tuval ortasına göre px)")]
        [SerializeField] private Vector2 home;

        public event Action CloseClicked;
        public event Action OpenChanged;

        private bool open;

        public bool IsOpen => open;
        public DraggableSheet Sheet => sheet;

        private void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(() => CloseClicked?.Invoke());
        }

        public void Open()
        {
            if (open) return;
            open = true;
            panelRoot.SetActive(true);
            sheet.SetHome(home, 1f); // sabit yeri; kağıt oraya kayar
            OpenChanged?.Invoke();
            sheet.AnimateIn(fromOffset);
        }

        // instant: gün başı / ekran gizlenirken (animasyon bekleyemez). Normalde kağıt masadaki nesnesine geri kayar.
        public void Close(bool instant = false) => CloseWith(instant, () => sheet.AnimateOut(fromOffset, HidePanel));

        public void SetHome(Vector2 position, float scale)
        {
            if (open) sheet.SetHome(position, scale);
        }

        private void CloseWith(bool instant, Action animate)
        {
            if (!open && !panelRoot.activeSelf) return;
            bool wasOpen = open;
            open = false;
            if (wasOpen) OpenChanged?.Invoke();

            if (instant || !isActiveAndEnabled || !panelRoot.activeSelf) panelRoot.SetActive(false);
            else animate();
        }

        private void HidePanel()
        {
            if (!open) panelRoot.SetActive(false);
        }
    }
}
