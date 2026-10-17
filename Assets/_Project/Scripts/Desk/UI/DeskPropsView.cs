using System;
using System.Collections;
using CivilJam.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // Masanın üstündekiler: müteahhit oturunca ortada (isimliğin önünde) YAPI DENETİM FORMU klasörü ve solda LAB RAPORU
    // kitabı belirir (tıklayınca kendi pencereleri açılır), rüşvet teklif edildiyse zarf (uyuşmazlık diyaloğundan sonra
    // müteahhidin elinden masaya yaylanarak düşer), üst barda müteahhit/proje adı,
    // gün bitince "Mesaiyi bitir". EL KİTABI kitabı da masada durur ama onu RulebookView dinler.
    public class DeskPropsView : MonoBehaviour
    {
        [SerializeField] private Button formFolder;
        [SerializeField] private GameObject formFolderRoot; // klasör + gölgesi (DeskBookProp)
        [SerializeField] private Button labBook;
        [SerializeField] private GameObject labBookRoot;    // kitap + gölgesi (DeskBookProp)
        [SerializeField] private GameObject envelopeRoot;
        [SerializeField] private TMP_Text envelopeText;
        [SerializeField] private TMP_Text clientTagText;
        [SerializeField] private Button finishButton;

        public event Action FormClicked;
        public event Action LabBookClicked;
        public event Action FinishClicked;
        public event Action EnvelopeRevealed; // zarf masaya geldi (kağıt sesi)

        private Vector2 envelopeRest;
        private Quaternion envelopeRestRotation;
        private bool envelopeRestKnown;
        private Coroutine envelopeAnim;

        private void Awake()
        {
            if (formFolder != null) formFolder.onClick.AddListener(() => FormClicked?.Invoke());
            if (labBook != null) labBook.onClick.AddListener(() => LabBookClicked?.Invoke());
            finishButton.onClick.AddListener(() => FinishClicked?.Invoke());
        }

        private GameObject FormObject => formFolderRoot != null ? formFolderRoot : formFolder != null ? formFolder.gameObject : null;
        private GameObject LabBookObject => labBookRoot != null ? labBookRoot : labBook != null ? labBook.gameObject : null;

        // bribeVisible: zarf masada mı (DeskScreen karar verir: teklif edildi, bekliyor ve diyalog bitti).
        public void Show(ProjectData project, ClientSO client, bool bribeVisible)
        {
            bool pending = project.Verdict == ProjectVerdict.Pending;
            FormObject?.SetActive(pending);
            LabBookObject?.SetActive(pending);
            SetEnvelope(bribeVisible);
            if (project.OffersBribe) envelopeText.text = $"ZARF\n<b>{DeskFormat.Money(project.BribeAmount)}</b>";

            string name = client != null ? client.DisplayName : "Başvuru sahibi";
            clientTagText.text = $"{name}  ·  {project.Name}";
            finishButton.gameObject.SetActive(false);
        }

        public void ShowEmpty(bool allDecided)
        {
            FormObject?.SetActive(false);
            LabBookObject?.SetActive(false);
            SetEnvelope(false);
            clientTagText.text = allDecided ? "Bugünkü başvurular bitti." : "";
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
    }
}
