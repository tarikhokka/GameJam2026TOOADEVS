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

            // 1) Açık dosya kart gibi kapanır.
            for (float t = 0; t < foldDuration; t += Time.unscaledDeltaTime)
            {
                sheet.FoldX = 1f - EaseIn(t / foldDuration);
                yield return null;
            }
            sheetHidden?.Invoke();
            sheet.FoldX = 1f;

            // 2) Aynı yerde kaşeli kapalı klasör açılır.
            var color = approved ? approvedColor : rejectedColor;
            titleText.text = title;
            stampText.text = approved ? "ONAYLANDI" : "REDDEDİLDİ";
            stampText.color = color;
            stampImage.color = new Color(color.r, color.g, color.b, 0.15f);
            folder.gameObject.SetActive(true);
            folder.localRotation = Quaternion.identity;
            folder.anchoredPosition = start;
            for (float t = 0; t < foldDuration; t += Time.unscaledDeltaTime)
            {
                folder.localScale = new Vector3(startScale * EaseOut(t / foldDuration), startScale, 1);
                yield return null;
            }
            folder.localScale = Vector3.one * startScale;

            // 3) Hafif geri çekilme (itmeden önce).
            Vector2 wound = start + new Vector2(0, -25);
            for (float t = 0; t < windUpDuration; t += Time.unscaledDeltaTime)
            {
                float k = EaseOut(t / windUpDuration);
                folder.anchoredPosition = Vector2.Lerp(start, wound, k);
                folder.localScale = Vector3.one * startScale * Mathf.Lerp(1f, 1.03f, k);
                yield return null;
            }

            // 4) Yay çizerek ellere: yavaş başlar, hızlanır, yumuşak durur; küçülür, hafif döner.
            Vector2 control = (wound + handsPosition) * 0.5f + new Vector2(0, arcHeight);
            float lifted = startScale * 1.03f;
            for (float t = 0; t < travelDuration; t += Time.unscaledDeltaTime)
            {
                float u = t / travelDuration;
                float k = EaseInOut(u);
                folder.anchoredPosition = Bezier(wound, control, handsPosition, k);
                folder.localScale = Vector3.one * Mathf.Lerp(lifted, handsScale, k);
                folder.localRotation = Quaternion.Euler(0, 0, -8f * Mathf.Sin(u * Mathf.PI));
                yield return null;
            }
            folder.anchoredPosition = handsPosition;
            folder.localScale = Vector3.one * handsScale;
            folder.localRotation = Quaternion.identity;

            // 5) Müteahhit alır: klasör masanın arkasındaki katmana geçip kenarın arkasına iner.
            folder.SetParent(behindLayer, false);
            Vector2 sunk = handsPosition + new Vector2(0, -sinkDistance);
            for (float t = 0; t < sinkDuration; t += Time.unscaledDeltaTime)
            {
                folder.anchoredPosition = Vector2.Lerp(handsPosition, sunk, EaseIn(t / sinkDuration));
                yield return null;
            }

            folder.gameObject.SetActive(false);
            Playing = false;
        }

        private static Vector2 Bezier(Vector2 a, Vector2 b, Vector2 c, float t)
        {
            float u = 1f - t;
            return u * u * a + 2f * u * t * b + t * t * c;
        }

        private static float EaseIn(float t) => t * t;
        private static float EaseOut(float t) => 1f - (1f - t) * (1f - t);
        private static float EaseInOut(float t) => t * t * (3f - 2f * t);
    }
}
