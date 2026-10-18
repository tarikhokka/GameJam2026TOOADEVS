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
        // (belge turu B1) Kaşeli belge müteahhide bırakıldı: kağıt müteahhide kayıp kaybolur, pencere kapanır.
        public event Action<PaperWindow> HandedBack;
        public event Action HandBackRefused; // kaşeden önce müteahhide bırakıldı (kağıt yerine döner)

        // Bırakma hedefi: müteahhidin portresi (ClientSceneView.DropArea; DeskScreen verir). Sadece görünürken hedef.
        public RectTransform HandBackTarget { get; set; }

        private bool open;

        public bool IsOpen => open;
        public DraggableSheet Sheet => sheet;

        private void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(() => CloseClicked?.Invoke());
            sheet.IsHandBackTarget = e => HandBackTarget != null && HandBackTarget.gameObject.activeInHierarchy
                && RectTransformUtility.RectangleContainsScreenPoint(HandBackTarget, e.position, e.pressEventCamera);
            sheet.HandBackDropped += GiveBack;
            sheet.HandBackRefused += () => HandBackRefused?.Invoke();
        }

        // Kağıt bırakıldığı yerden müteahhidin ortasına kayar ve kaybolur; pencere hemen kapalı sayılır.
        private void GiveBack()
        {
            if (!open) return;
            open = false;
            OpenChanged?.Invoke();
            var parent = (RectTransform)sheet.transform.parent;
            Vector2 target = HandBackTarget != null
                ? (Vector2)parent.InverseTransformPoint(HandBackTarget.TransformPoint(HandBackTarget.rect.center))
                : ((RectTransform)sheet.transform).anchoredPosition;
            sheet.AnimateGive(target, HidePanel);
            HandedBack?.Invoke(this);
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

        // (belge turu B1) Masadaki ikonundan açılır: kağıt ikonun yerinde, ikon boyutunda başlar ve yayla bugünkü yerine büyüyerek gelir.
        public void OpenFrom(RectTransform icon)
        {
            if (open) return;
            if (icon == null)
            {
                Open();
                return;
            }
            open = true;
            panelRoot.SetActive(true);
            sheet.SetHome(home, 1f);
            OpenChanged?.Invoke();

            var sheetRect = (RectTransform)sheet.transform;
            var parent = (RectTransform)sheetRect.parent;
            Vector2 iconCenter = parent.InverseTransformPoint(icon.TransformPoint(icon.rect.center));
            float iconWidth = icon.rect.width * icon.lossyScale.x / Mathf.Max(parent.lossyScale.x, 0.0001f);
            float startScale = Mathf.Clamp(iconWidth / Mathf.Max(sheetRect.rect.width, 1f), 0.1f, 1f);
            sheet.HandBackScale = startScale; // iade ederken de ikon boyutuna küçülür
            sheet.AnimateIn(iconCenter - home, startScale, 0.6f);
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
