using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // Karar sonrası dosyanın müteahhide verilmesi. Bileşen odanın içinde, portre ile desk_front arasında durur.
    // Klasör yolculukta desk_front'un önündeki katmanda (frontLayer) gider, ellere varınca bu katmana geçip
    // masanın kenarının arkasına iner: müteahhit almış gibi görünür.
    // Sıra: açık dosya kart gibi kapanır → aynı yerde kaşeli kapalı klasör açılır → hafif geri çekilir →
    // yay çizerek küçülüp müteahhidin ellerine gider → masanın arkasına iner.
    public class FolderHandoffView : MonoBehaviour
    {
        [SerializeField] private RectTransform folder;     // kapalı klasör (pivot ortada)
        [SerializeField] private RectTransform frontLayer; // desk_front'un önü: yolculuk boyunca klasör burada, kesilmez
        [SerializeField] private TMP_Text titleText;     // kapakta proje adı
        [SerializeField] private TMP_Text stampText;     // ONAYLANDI / REDDEDİLDİ
        [SerializeField] private Image stampImage;
        [SerializeField] private Color approvedColor = new Color(0.22f, 0.5f, 0.27f);
        [SerializeField] private Color rejectedColor = new Color(0.66f, 0.2f, 0.18f);

        [Header("Hedef (oda px'i, oda ortası = 0,0)")]
        [Tooltip("Müteahhidin masa üstündeki elleri. Masa kenarı y = -210; klasörün alt kenarı vardığında kenarın üstünde olmalı")]
        [SerializeField] private Vector2 handsPosition = new Vector2(0, -110);
        [SerializeField] private float handsScale = 0.2f;
        [Tooltip("Ellere vardıktan sonra masanın arkasına iniş (px)")]
        [SerializeField] private float sinkDistance = 220f; // klasörün üstü de masa kenarının altına insin

        [Header("Süreler (sn)")]
        [SerializeField] private float foldDuration = 0.16f;
        [SerializeField] private float windUpDuration = 0.12f;
        [SerializeField] private float travelDuration = 0.55f;
        [SerializeField] private float catchDuration = 0.14f;
        [SerializeField] private float sinkDuration = 0.25f;
        [SerializeField] private float arcHeight = 220f;

        public bool Playing { get; private set; }

        private void Awake() => folder.gameObject.SetActive(false);

        // Ekran yarıda kapanırsa (gün sonu) klasör ortada kalmasın.
        private void OnDisable()
        {
            folder.gameObject.SetActive(false);
            Playing = false;
        }

        // sheet: açık dosyanın kağıdı (kapanma animasyonu ona uygulanır); sheetHidden: dosya paneli o anda kapatılır.
        public void Play(DraggableSheet sheet, bool approved, string title, Action sheetHidden)
        {
            StopAllCoroutines();
            StartCoroutine(Run(sheet, approved, title, sheetHidden));
        }

        private IEnumerator Run(DraggableSheet sheet, bool approved, string title, Action sheetHidden)
        {
            Playing = true;
            var behindLayer = (RectTransform)transform;
            var parent = frontLayer != null ? frontLayer : behindLayer; // iki katman da odayı kaplar: koordinatlar aynı
            folder.SetParent(parent, false);
            var sheetRect = (RectTransform)sheet.transform;

            // Başlangıç: kağıdın ekrandaki yeri ve boyutu, bu katmanın koordinatlarında.
            Vector2 start = parent.InverseTransformPoint(sheetRect.position);
            float startScale = sheetRect.lossyScale.y / parent.lossyScale.y;

            // 1) Açık dosya kart gibi kapanır: hızlanarak (kapak kapanır gibi).
            yield return Tween(foldDuration, k => sheet.FoldX = 1f - DeskMotion.EaseIn(k));
            sheetHidden?.Invoke();
            sheet.FoldX = 1f;

            // 2) Aynı yerde kaşeli kapalı klasör açılır: kapanmanın devamı, hafif yaylanarak oturur.
            var color = approved ? approvedColor : rejectedColor;
            titleText.text = title;
            stampText.text = approved ? "ONAYLANDI" : "REDDEDİLDİ";
            stampText.color = color;
            stampImage.color = new Color(color.r, color.g, color.b, 0.15f);
            folder.gameObject.SetActive(true);
            folder.localRotation = Quaternion.identity;
            folder.anchoredPosition = start;
            yield return Tween(foldDuration * 1.4f, k =>
                folder.localScale = new Vector3(startScale * DeskMotion.EaseOutBack(k, 1.4f), startScale, 1));

            // 3) Hafif geri çekilme (itmeden önce).
            Vector2 wound = start + new Vector2(0, -25);
            yield return Tween(windUpDuration, k =>
            {
                float e = DeskMotion.EaseInOut(k);
                folder.anchoredPosition = Vector2.Lerp(start, wound, e);
                folder.localScale = Vector3.one * startScale * Mathf.Lerp(1f, 1.03f, e);
            });

            // 4) Yay çizerek ellere: yavaş başlar, hızlanır, yumuşak durur; küçülür, hafif döner.
            Vector2 control = (wound + handsPosition) * 0.5f + new Vector2(0, arcHeight);
            float lifted = startScale * 1.03f;
            yield return Tween(travelDuration, k =>
            {
                float e = DeskMotion.EaseInOut(k);
                folder.anchoredPosition = Bezier(wound, control, handsPosition, e);
                folder.localScale = Vector3.one * Mathf.Lerp(lifted, handsScale, e);
                folder.localRotation = Quaternion.Euler(0, 0, -8f * Mathf.Sin(k * Mathf.PI));
            });

            // 5) Müteahhit yakalar: kısa bir sıkma (büyüyüp yerine oturur) — "aldı" anı.
            yield return Tween(catchDuration, k =>
                folder.localScale = Vector3.one * handsScale * (1f + 0.08f * Mathf.Sin(k * Mathf.PI)));

            // 6) Klasör masanın arkasındaki katmana geçip kenarın arkasına hızlanarak iner.
            folder.SetParent(behindLayer, false);
            Vector2 sunk = handsPosition + new Vector2(0, -sinkDistance);
            yield return Tween(sinkDuration, k => folder.anchoredPosition = Vector2.Lerp(handsPosition, sunk, DeskMotion.EaseIn(k)));

            folder.gameObject.SetActive(false);
            Playing = false;
        }

        // Süreli adım: her karede 0→1 ilerleme verir; son karede tam 1 uygulanır (adımlar arası sıçrama olmaz).
        private static IEnumerator Tween(float duration, Action<float> step)
        {
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                step(t / duration);
                yield return null;
            }
            step(1f);
        }

        private static Vector2 Bezier(Vector2 a, Vector2 b, Vector2 c, float t)
        {
            float u = 1f - t;
            return u * u * a + 2f * u * t * b + t * t * c;
        }
    }
}
