using System;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // Açık dosya: LAB RAPORU kitabına tıklayınca kitabın yönünden (sol alt) kayarak gelir (belge + karar sütunu), oda hafifçe kararır.
    // "Kapat" ya da perdeye tıklama kapatır; kağıt kitaba geri kayar, panel sonra kapanır. Bileşen hep aktif bir objede.
    // El Kitabı da açıkken dosya küçülüp sola kayar (Papers, Please gibi yan yana karşılaştırma); bkz. DeskScreen.ArrangeSheets.
    // Kağıt fareyle sürüklenir (DraggableSheet).
    public class DossierView : MonoBehaviour
    {
        // 1920x1080 tuval: yan yana modda dosya (1560x940) %70 boyutta sol kenarda, El Kitabı sağda.
        private static readonly Vector2 CenterPos = new Vector2(0, -40);
        private static readonly Vector2 CompactPos = new Vector2(-394, -40);
        private const float CompactScale = 0.7f;
        private static readonly Vector2 FromLabBook = new Vector2(-500, -900); // masadaki LAB RAPORU kitabı sol altta
        // Karar sonrası dosya müteahhide verilir: koltuktaki portrenin masa üstündeki elleri (desk_bg'de y ~700, ortada).
        private static readonly Vector2 ClientHands = new Vector2(0, -160);
        private const float HandedScale = 0.15f;

        [SerializeField] private GameObject panelRoot;
        [SerializeField] private DraggableSheet sheet;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button backdropButton;

        public event Action CloseClicked;
        public event Action OpenChanged;

        private bool open;
        private bool compact;

        public bool IsOpen => open;
        public DraggableSheet Sheet => sheet;

        private void Awake()
        {
            closeButton.onClick.AddListener(() => CloseClicked?.Invoke());
            backdropButton.onClick.AddListener(() => CloseClicked?.Invoke());
        }

        public void Open()
        {
            if (open) return;
            open = true;
            panelRoot.SetActive(true);
            OpenChanged?.Invoke(); // yerleşim önce (SetCompact), kağıt sonra oraya kayar
            sheet.AnimateIn(FromLabBook);
        }

        // instant: gün başı / ekran gizlenirken (animasyon bekleyemez). Normalde kağıt LAB RAPORU kitabına geri kayar.
        public void Close(bool instant = false) => CloseWith(instant, () => sheet.AnimateOut(FromLabBook, HidePanel));

        // Karar verildi: dosya kapanıp kaşeli klasör olarak müteahhide gider (onaylı da reddedilmiş de olsa geri verilir).
        // handoff yoksa (eski prefab) kağıt küçülerek ellere kayar.
        public void HandToClient(FolderHandoffView handoff, bool approved, string title)
        {
            if (handoff != null) CloseWith(false, () => handoff.Play(sheet, approved, title, HidePanel));
            else CloseWith(false, () => sheet.AnimateTo(ClientHands, HandedScale, HidePanel));
        }

        private void CloseWith(bool instant, Action animate)
        {
            if (!open && !panelRoot.activeSelf) return;
            bool wasOpen = open;
            open = false;
            backdropButton.gameObject.SetActive(false);
            if (wasOpen) OpenChanged?.Invoke();

            if (instant || !isActiveAndEnabled || !panelRoot.activeSelf) panelRoot.SetActive(false);
            else animate();
        }

        private void HidePanel()
        {
            if (!open) panelRoot.SetActive(false);
        }

        // Yan yana modda perde kalkar: dosya kitabın üstüne sürüklenince perde kitabı örtmesin, oda kararmasın.
        public void SetCompact(bool isCompact)
        {
            compact = isCompact;
            backdropButton.gameObject.SetActive(open && !compact);
            if (open) sheet.SetHome(compact ? CompactPos : CenterPos, compact ? CompactScale : 1f);
        }
    }
}
