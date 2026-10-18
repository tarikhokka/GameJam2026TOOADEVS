using System;
using System.Collections;
using CivilJam.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // Masanın üstündekiler: müteahhit oturunca solunda (masa kenarında) YAPI DENETİM FORMU ve LAB RAPORU küçük ikonları gelir
    // (belge turu B1; tıklayınca belge açılır), rüşvet teklif edildiyse sağında zarf (uyuşmazlık diyaloğundan sonra müteahhidin
    // elinden masaya yaylanarak düşer). Müteahhit / proje adı üst barda (DeskHeaderView, DeskScreen yazar).
    public class DeskPropsView : MonoBehaviour
    {
        [SerializeField] private GameObject formIcon; // masada yatan form (DeskBookProp kökü: gölge + görsel; görselde Button)
        [SerializeField] private GameObject labIcon;  // masada yatan lab raporu
        [SerializeField] private ClientSceneView clientScene; // ikonlar müteahhit oturunca gelir
        [SerializeField] private GameObject envelopeRoot;
        [SerializeField] private TMP_Text envelopeText;
        [SerializeField] private Button finishButton;

        public event Action FormIconClicked;
        public event Action LabIconClicked;
        public event Action FinishClicked;
        public event Action EnvelopeRevealed; // zarf masaya geldi (kağıt sesi)
        public event Action EnvelopeClicked;  // (A, Ahmet isteği) zarfa tıklandı → BribeEnvelopeView açılır

        public Vector3 EnvelopeWorld => envelopeRoot.transform.position;

        // Belge ikonlarının kökleri: belge buradan büyüyerek açılır, iade edilince buraya küçülür (belge turu B1).
        public RectTransform FormIcon => formIcon != null ? (RectTransform)formIcon.transform : null;
        public RectTransform LabIcon => labIcon != null ? (RectTransform)labIcon.transform : null;

        private Vector2 envelopeRest;
        private Quaternion envelopeRestRotation;
        private bool envelopeRestKnown;
        private Coroutine envelopeAnim;

        // İkonun masadaki yeri ve geliş animasyonu.
        private class Icon
        {
            public GameObject Root;
            public Vector2 Rest;
            public Quaternion RestRotation;
            public Coroutine Anim;
        }
        private Icon form, lab;

        private void Awake()
        {
            form = NewIcon(formIcon, () => FormIconClicked?.Invoke());
            lab = NewIcon(labIcon, () => LabIconClicked?.Invoke());
            finishButton.onClick.AddListener(() => FinishClicked?.Invoke());

            // (A, Ahmet isteği) Masadaki zarf tıklanabilir: açılınca içindeki para ve tutar görünür (BribeEnvelopeView).
            var envelopeImage = envelopeRoot.GetComponent<Image>();
            if (envelopeImage != null) envelopeImage.raycastTarget = true;
            var envelopeButton = envelopeRoot.GetComponent<Button>();
            if (envelopeButton == null) envelopeButton = envelopeRoot.AddComponent<Button>();
            if (envelopeImage != null) envelopeButton.targetGraphic = envelopeImage;
            envelopeButton.onClick.AddListener(() => EnvelopeClicked?.Invoke());
        }

        private static Icon NewIcon(GameObject root, Action clicked)
        {
            if (root == null) return null;
            var rt = (RectTransform)root.transform;
            var button = root.GetComponentInChildren<Button>(true);
            if (button != null) button.onClick.AddListener(() => clicked());
            return new Icon { Root = root, Rest = rt.anchoredPosition, RestRotation = rt.localRotation };
        }

        // bribeVisible: zarf masada mı (DeskScreen karar verir: teklif edildi, bekliyor ve diyalog bitti).
        // Belge ikonlarını DeskScreen ayrıca yönetir (SetDocIcons: belge açıkken ikonu masada değil).
        public void Show(ProjectData project, ClientSO client, bool bribeVisible)
        {
            SetEnvelope(bribeVisible);
            // (A, Ahmet kararı) Masada tutar görünmez; zarf açılınca (BribeEnvelopeView) görünür.
            envelopeText.text = "ZARF";

            finishButton.gameObject.SetActive(false);
        }

        public void ShowEmpty(bool allDecided)
        {
            SetDocIcons(false, false);
            SetEnvelope(false);
            // (EEE, A ve B onayı) "Mesaiyi bitir" artık gösterilmez: DeskScreen mesai zili + geçiş ekranıyla kendisi geçer.
            finishButton.gameObject.SetActive(false);
        }

        // Gizliyken görünür olursa zarf müteahhidin tarafından (yukarıdan, eğik, küçük) masaya düşer ve hafif yaylanıp oturur.
        private void SetEnvelope(bool visible)
        {
            var rt = (RectTransform)envelopeRoot.transform;
            if (!envelopeRestKnown)
            {
                envelopeRest = rt.anchoredPosition;
                envelopeRestRotation = rt.localRotation;
                envelopeRestKnown = true;
            }
            if (visible == envelopeRoot.activeSelf) return;
            if (envelopeAnim != null) StopCoroutine(envelopeAnim);
            envelopeAnim = null;
            rt.anchoredPosition = envelopeRest;
            rt.localRotation = envelopeRestRotation;
            rt.localScale = Vector3.one;
            envelopeRoot.SetActive(visible);
            if (!visible) return;
            if (isActiveAndEnabled) envelopeAnim = StartCoroutine(DropEnvelope(rt));
            EnvelopeRevealed?.Invoke();
        }

        private IEnumerator DropEnvelope(RectTransform rt)
        {
            var from = envelopeRest + new Vector2(-70f, 110f);
            var fromRotation = envelopeRestRotation * Quaternion.Euler(0, 0, 14f);
            for (float t = 0f; t < DeskMotion.Slow; t += Time.unscaledDeltaTime)
            {
                float k = DeskMotion.EaseOutBack(t / DeskMotion.Slow, 1.4f);
                rt.anchoredPosition = Vector2.LerpUnclamped(from, envelopeRest, k);
                rt.localRotation = Quaternion.SlerpUnclamped(fromRotation, envelopeRestRotation, k);
                rt.localScale = Vector3.one * Mathf.LerpUnclamped(0.7f, 1f, k);
                yield return null;
            }
            rt.anchoredPosition = envelopeRest;
            rt.localRotation = envelopeRestRotation;
            rt.localScale = Vector3.one;
            envelopeAnim = null;
        }

        // Belge ikonları masada mı (belge turu B1). Gizliyken görünür olursa müteahhit oturduktan sonra onun tarafından masaya
        // kayarak gelir (önce form, hemen ardından lab raporu); her çağrıda tekrar oynamaz.
        public void SetDocIcons(bool formVisible, bool labVisible)
        {
            SetIcon(form, formVisible, 0f);
            SetIcon(lab, labVisible, IconStagger);
        }

        private const float IconStagger = 0.08f; // lab raporu formun hemen ardından

        private void SetIcon(Icon icon, bool visible, float delay)
        {
            if (icon == null || visible == icon.Root.activeSelf) return;
            SnapIcon(icon);
            icon.Root.SetActive(visible);
            if (visible && isActiveAndEnabled) icon.Anim = StartCoroutine(SlideIcon(icon, (RectTransform)icon.Root.transform, delay));
        }

        // Animasyonu keser, ikonu masadaki yerine koyar.
        private void SnapIcon(Icon icon)
        {
            if (icon.Anim != null) StopCoroutine(icon.Anim);
            icon.Anim = null;
            var rt = (RectTransform)icon.Root.transform;
            rt.anchoredPosition = icon.Rest;
            rt.localRotation = icon.RestRotation;
            rt.localScale = Vector3.one;
            var group = icon.Root.GetComponent<CanvasGroup>();
            if (group != null)
            {
                group.alpha = 1f;
                group.blocksRaycasts = true;
            }
        }

        private void OnDisable()
        {
            // Ekran kapanınca yarım kalan geliş animasyonu ikonu görünmez / kaymış bırakmasın.
            if (form != null) SnapIcon(form);
            if (lab != null) SnapIcon(lab);
        }

        // Müteahhidin tarafından (sağ-yukarı, eğik, küçük) masaya kayar, hafif yaylanıp oturur. Müteahhit gelirken bekler
        // (görünmez durur), oturunca gelir.
        private IEnumerator SlideIcon(Icon icon, RectTransform rt, float delay)
        {
            var group = icon.Root.GetComponent<CanvasGroup>();
            if (group == null) group = icon.Root.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false; // görünmezken tıklanmasın
            yield return null; // aynı karede müteahhit Seat edildiyse IsMoving'i görelim
            while (clientScene != null && clientScene.IsMoving) yield return null;
            for (float t = 0f; t < delay; t += Time.unscaledDeltaTime) yield return null;
            group.blocksRaycasts = true;

            var from = icon.Rest + new Vector2(110f, 70f);
            var fromRotation = icon.RestRotation * Quaternion.Euler(0, 0, -12f);
            for (float t = 0f; t < DeskMotion.Slow; t += Time.unscaledDeltaTime)
            {
                float k = DeskMotion.EaseOutBack(t / DeskMotion.Slow, 1.3f);
                rt.anchoredPosition = Vector2.LerpUnclamped(from, icon.Rest, k);
                rt.localRotation = Quaternion.SlerpUnclamped(fromRotation, icon.RestRotation, k);
                rt.localScale = Vector3.one * Mathf.LerpUnclamped(0.75f, 1f, k);
                group.alpha = DeskMotion.EaseOut(t / DeskMotion.Fast);
                yield return null;
            }
            rt.anchoredPosition = icon.Rest;
            rt.localRotation = icon.RestRotation;
            rt.localScale = Vector3.one;
            group.alpha = 1f;
            icon.Anim = null;
        }
    }
}
