using System.Collections;
using CivilJam.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // Yapı Denetim Formu'nun üstüne basılan ONAYLANDI / REDDEDİLDİ kaşesi. Karar verilmiş formda kalıcı durur.
    public class DecisionStampView : MonoBehaviour
    {
        [SerializeField] private RectTransform stamp;
        [SerializeField] private Image stampImage;
        [SerializeField] private TMP_Text stampText;
        [SerializeField] private Color approvedColor = new Color(0.18f, 0.5f, 0.22f);
        [SerializeField] private Color rejectedColor = new Color(0.72f, 0.14f, 0.12f);
        [SerializeField] private float punchScale = 1.8f;
        [SerializeField] private float punchDuration = 0.18f;
        [Header("Yuvarlak mühür izi (ink_approve / ink_reject): varsa yazı yerine görsel")]
        [SerializeField] private Sprite approvedInk;
        [SerializeField] private Sprite rejectedInk;

        private bool animating;
        private Graphic Ink => stampImage.sprite != null ? stampImage : (Graphic)stampText; // basılırken belirginleşen

        public void Bind(ProjectVerdict verdict)
        {
            bool decided = verdict != ProjectVerdict.Pending;
            stamp.gameObject.SetActive(decided);
            if (!decided) return;

            bool approved = verdict == ProjectVerdict.Approved;
            var ink = approved ? approvedInk : rejectedInk;
            if (ink != null)
            {
                // Mühür izi görseli: renk ve yazı görselde; kutu ve yazı gizli.
                stampImage.sprite = ink;
                stampImage.preserveAspect = true;
                if (!animating) stampImage.color = Color.white;
                stampText.gameObject.SetActive(false);
            }
            else
            {
                var color = approved ? approvedColor : rejectedColor;
                stampImage.sprite = null;
                stampText.gameObject.SetActive(true);
                stampText.text = approved ? "ONAYLANDI" : "REDDEDİLDİ";
                stampText.color = color;
                stampImage.color = new Color(color.r, color.g, color.b, 0.12f);
            }
            if (!animating) stamp.localScale = Vector3.one;
        }

        public void Animate()
        {
            if (!stamp.gameObject.activeInHierarchy) return;
            StopAllCoroutines();
            StartCoroutine(Punch());
        }

        // Ekran animasyon ortasında kapanırsa kaşe büyük kalmasın.
        private void OnDisable()
        {
            animating = false;
            if (stamp != null) stamp.localScale = Vector3.one;
        }

        // Gerçek kaşe gibi (DeskMotion): havadan hızlanarak iner (ease-in) ve mürekkep belirginleşir, kağıda çarpınca
        // bir an basılır (%92), sonra yerine oturur (ease-out). Ses çarpma anına denk gelir (DeskScreen kaşe sesini hemen çalar).
        private IEnumerator Punch()
        {
            animating = true;
            var graphic = Ink;
            var ink = graphic.color;
            const float impact = 0.92f;
            float drop = punchDuration * 0.6f, settle = DeskMotion.Fast;

            for (float t = 0f; t < drop; t += Time.unscaledDeltaTime)
            {
                float k = DeskMotion.EaseIn(t / drop);
                stamp.localScale = Vector3.one * Mathf.Lerp(punchScale, impact, k);
                graphic.color = new Color(ink.r, ink.g, ink.b, Mathf.Lerp(0.3f, ink.a, k));
                yield return null;
            }
            graphic.color = ink;
            for (float t = 0f; t < settle; t += Time.unscaledDeltaTime)
            {
                stamp.localScale = Vector3.one * Mathf.Lerp(impact, 1f, DeskMotion.EaseOut(t / settle));
                yield return null;
            }
            stamp.localScale = Vector3.one;
            animating = false;
        }
    }
}
