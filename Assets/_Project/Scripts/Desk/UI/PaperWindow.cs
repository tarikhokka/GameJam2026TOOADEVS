using System;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // Masadaki bir nesneden açılan kağıt pencere (Yapı Denetim Formu, Lab Raporu, El Kitabı). Perde yok: masa ve diğer
    // pencereler tıklanabilir kalır, oyuncu kağıtları yan yana koyup karşılaştırır (Papers, Please).
    // Açılınca masadaki nesnenin yönünden (fromOffset) kayarak gelir, kapanınca oraya döner; kağıt sürüklenebilir (DraggableSheet).
    // Yerleşimi DeskScreen.ArrangeSheets verir (SetHome). Bileşen hep aktif kökte; açılıp kapanan panelRoot.
    public class PaperWindow : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private DraggableSheet sheet;
        [SerializeField] private Button closeButton;
        [Tooltip("Açılış/kapanışta kağıdın geldiği yön: masadaki nesnenin tarafı (tuval px)")]
        [SerializeField] private Vector2 fromOffset = new Vector2(0, -900);

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
            sheet.BringToFront();
            OpenChanged?.Invoke(); // yerleşim önce (SetHome), kağıt sonra oraya kayar
            sheet.AnimateIn(fromOffset);
        }

        // instant: gün başı / ekran gizlenirken (animasyon bekleyemez). Normalde kağıt masadaki nesnesine geri kayar.
        public void Close(bool instant = false) => CloseWith(instant, () => sheet.AnimateOut(fromOffset, HidePanel));

        // Karar verildi: kağıt kapanıp kaşeli klasör olarak müteahhide gider (FolderHandoffView).
        public void HandTo(FolderHandoffView handoff, bool approved, string title)
        {
            if (handoff != null) CloseWith(false, () => handoff.Play(sheet, approved, title, HidePanel));
            else Close();
        }

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
