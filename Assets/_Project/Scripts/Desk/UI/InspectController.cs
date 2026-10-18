using System;
using System.Collections.Generic;
using CivilJam.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // İnceleme modu (Papers, Please tarzı; Ahmet onayı, memory.md "İnceleme modu"). Masadaki İNCELE düğmesi modu açıp kapatır.
    // Mod açıkken belgeler sürüklenmez, karar düğmeleri pasif (DeskScreen); tıklanabilir alanlar (InspectSpot) vurgulanır.
    // 1. tıklama alanı seçer (aynı alana tekrar = seçimi kaldırır), 2. tıklama iki alanı karşılaştırır: aralarına çizgi çekilir,
    // ortasında sonuç görünür, resultHold sn sonra (ya da tıklayınca) temizlenir; mod açık kalır. Sağ tık ya da boş masaya
    // tıklamak seçimi temizler (Esc'e dokunulmaz: Onur'un menüsü). Arayüz kural bilmez: karar Core'dan gelir (Checker = ctx.Actions.Inspect, DeskScreen verir).
    // DeskScreen kökünde durur: alanlar onu GetComponentInParent ile bulur.
    public class InspectController : MonoBehaviour
    {
        private static readonly Color PressedTint = new Color(1f, 0.84f, 0.5f);

        [SerializeField] private Button toggleButton;
        [SerializeField] private Image toggleImage;
        [SerializeField] private InspectCatcher catcher;
        [SerializeField] private Graphic catcherGraphic;
        [Header("Karartma (mod açıkken; aydınlık kalanlar öne alınır)")]
        [SerializeField] private Image dim;
        [Tooltip("Mod açıkken karartmanın koyuluğu (0 = yok, 1 = simsiyah); Ahmet: \"çok azıcık\"")]
        [SerializeField, Range(0f, 1f)] private float dimAlpha = 0.2f;
        [Tooltip("Kararma / açılma süresi (sn)")]
        [SerializeField] private float dimFade = 0.15f;
        [Tooltip("Mod açıkken karartmanın üstüne alınanlar: müteahhit balonu (alanlar kendini, kaşe çekmecesini DeskScreen öne alır)")]
        [SerializeField] private GameObject[] keepLit;
        [Tooltip("Öne alınanların da üstünde kalması gereken katmanlar: İNCELE katmanı (düğme, çizgi, kaşe), diyalog katmanı")]
        [SerializeField] private GameObject[] topLayers;
        [SerializeField] private RectTransform overlay;   // belgelerin üstünde, tıklama almayan katman (çizgi + sonuç)
        [SerializeField] private RectTransform line;
        [SerializeField] private InspectResultView resultView; // Onur: sonuç kaşesi (renk, açıklama, giriş/çıkış hareketi ve ses kendisinde)
        [SerializeField] private float lineWidth = 4f;
        [Tooltip("Sonuç bu kadar sn görünür, sonra çizgi, sonuç ve seçim temizlenir (tıklayınca hemen)")]
        [SerializeField] private float resultHold = 1.6f;

        public bool IsOn { get; private set; }
        // (belge turu B2) Kaşe çekmecesi açıkken de aynı karartma gelir (SetFocus); aydınlık kalanları DeskScreen öne alır.
        public bool IsFocused { get; private set; }
        public event Action<bool> ModeChanged;
        public event Action Clicked; // alan seçildi / karşılaştırıldı (ses için)
        public event Action<InspectOutcome> Compared; // kaşe gösterildi (DeskScreen diyalog satırlarını oynatır)

        // İki alanın karşılaştırması (DeskScreen: ctx.Actions.Inspect + bugünkü proje + El Kitabı'nda seçili kat grubu).
        public Func<InspectTarget, InspectTarget, InspectOutcome> Checker { get; set; }

        private readonly List<InspectSpot> spots = new List<InspectSpot>();
        private InspectSpot first, second;
        private bool showing;
        private float shownAt;
        private float dimLevel; // karartma geçişi: 0 = yok, 1 = tam (dimAlpha)

        // Karartmanın üstü: masanın kök Canvas'ından iki fazla; alanların kağıt zemini bir fazla (her zaman alanın arkasında),
        // İNCELE ve diyalog katmanları üç fazla. Onur'un Esc menüsü 32760, hep en üstte kalır.
        internal int LiftOrder
        {
            get
            {
                var canvas = GetComponentInParent<Canvas>();
                return (canvas != null ? canvas.rootCanvas.sortingOrder : 0) + 2;
            }
        }

        private void Awake()
        {
            if (toggleButton != null) toggleButton.onClick.AddListener(() => SetMode(!IsOn));
            if (catcher != null) catcher.Clicked += ClearSelection;
            ApplyLook();
            ClearSelection();
        }

        // Bekleyen dosya yokken (müteahhit yok, karar sırası sürüyor) düğme görünmez ve mod kapanır.
        public void SetAvailable(bool available)
        {
            if (!available) SetMode(false);
            if (toggleButton != null) toggleButton.gameObject.SetActive(available);
        }

        public void SetMode(bool on)
        {
            ClearSelection();
            if (IsOn == on) return;
            IsOn = on;
            foreach (var spot in spots) spot.SetMode(on);
            // Karartmanın üstüne: müteahhit balonu (LiftOrder; kaşe çekmecesini DeskScreen alır); İNCELE ve diyalog katmanları üstünde:
            // kaşe öne alınmış satırın arkasında kalmaz, diyalog sürerken kilit katmanı öne alınmış alanları da örter.
            if (keepLit != null) foreach (var go in keepLit) InspectLift.Set(go, on, LiftOrder);
            if (topLayers != null) foreach (var go in topLayers) InspectLift.Set(go, on, LiftOrder + 1);
            if (IsFocused) LiftOverlay(); // kaşe çekmecesi açıkken İNCELE katmanı yüksekte kalır
            ApplyLook();
            ModeChanged?.Invoke(on);
        }

        // İnceleme modu dışında karartma (kaşe çekmecesi): alanlar vurgulanmaz, sadece masa hafif kararır. Çekmecenin "dışarı
        // tıklama" katmanı da karartmanın üstünde; İNCELE katmanı onun da üstüne alınır ki düğmeye basılınca mod açılsın
        // (mod açılınca DeskScreen çekmeceyi kapatır).
        public void SetFocus(bool on)
        {
            if (IsFocused == on) return;
            IsFocused = on;
            LiftOverlay();
        }

        private void LiftOverlay()
        {
            if (overlay == null) return;
            if (IsFocused) InspectLift.Set(overlay.gameObject, true, LiftOrder + 2);
            else InspectLift.Set(overlay.gameObject, IsOn, LiftOrder + 1); // modda topLayers ile aynı kat; ikisi de kapalıysa iner
        }

        internal void Register(InspectSpot spot)
        {
            if (!spots.Contains(spot)) spots.Add(spot);
            spot.SetMode(IsOn);
        }

        internal void Unregister(InspectSpot spot)
        {
            spots.Remove(spot);
            if (spot == first || spot == second) ClearSelection();
        }

        internal void SpotClicked(InspectSpot spot, PointerEventData.InputButton button)
        {
            if (!IsOn) return;
            if (button == PointerEventData.InputButton.Right || showing)
            {
                ClearSelection(); // sağ tık iptal; sonuç görünürken tıklama onu kapatır
                return;
            }
            if (button != PointerEventData.InputButton.Left || !spot.Available) return;

            if (first == null)
            {
                first = spot;
                spot.SetSelected(true);
                Clicked?.Invoke();
            }
            else if (first == spot)
            {
                spot.SetSelected(false);
                first = null;
            }
            else
            {
                second = spot;
                spot.SetSelected(true);
                Compare(first, second);
            }
        }

        private void Compare(InspectSpot a, InspectSpot b)
        {
            var outcome = Checker != null ? Checker(a.Target, b.Target) : null;
            var result = outcome != null ? outcome.Result : null;
            if (result == null) result = new InspectResult { Verdict = InspectVerdict.Unrelated, Title = Inspection.TitleUnrelated, Detail = "" };
            showing = true;
            shownAt = Time.unscaledTime;
            line.gameObject.SetActive(true);
            DrawLine(0f);
            if (resultView != null) resultView.Show(result, (Local(a) + Local(b)) * 0.5f); // çizginin ortası (katmanın merkezine göre)
            Compared?.Invoke(outcome);
        }

        private void Update()
        {
            UpdateDim();
            if (!showing) return;
            float t = Time.unscaledTime - shownAt;
            DrawLine(DeskMotion.EaseOut(t / DeskMotion.Base)); // çizgi ilk alandan ikinciye doğru uzar
            if (t >= resultHold) ClearSelection();
        }

        // Karartma mod açılınca dimFade sn'de gelir, kapanınca aynı sürede gider (Esc ile zaman dursa da: unscaled).
        private void UpdateDim()
        {
            if (dim == null) return;
            bool dimmed = IsOn || IsFocused;
            float target = dimmed ? 1f : 0f;
            if (dimLevel == target && dim.gameObject.activeSelf == dimmed) return;
            dimLevel = Mathf.MoveTowards(dimLevel, target, Time.unscaledDeltaTime / Mathf.Max(dimFade, 0.01f));
            ApplyDim();
        }

        private void ApplyDim()
        {
            if (dim == null) return;
            dim.color = new Color(0f, 0f, 0f, dimAlpha * DeskMotion.EaseOut(dimLevel));
            dim.gameObject.SetActive(dimLevel > 0f);
        }

        // Masa kapanırken (gün sonu) karartma beklemeden kalkar; tekrar açılınca kararmış başlamaz.
        private void OnDisable()
        {
            dimLevel = 0f; // odak (kaşe çekmecesi) DeskScreen.Hide'da çekmece kapanınca kalkar (burada DestroyImmediate yapılmaz)
            ApplyDim();
        }

        // Kağıtlar yay ile hareket ederken de uçlar alanların üstünde kalsın diye her kare yeniden çizilir.
        private void DrawLine(float progress)
        {
            if (first == null || second == null) return;
            Vector2 a = Local(first), b = Local(second);
            Vector2 d = b - a;
            line.anchoredPosition = a;
            line.sizeDelta = new Vector2(d.magnitude * Mathf.Clamp01(progress), lineWidth);
            line.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
        }

        private Vector2 Local(InspectSpot spot) => overlay.InverseTransformPoint(spot.WorldCenter);

        private void ClearSelection()
        {
            if (first != null) first.SetSelected(false);
            if (second != null) second.SetSelected(false);
            first = second = null;
            showing = false;
            if (line != null) line.gameObject.SetActive(false);
            if (resultView != null) resultView.Hide();
        }

        // Mod açıkken düğme basılı görünür (sarımsı, biraz küçük); boş masa tıklamayı yakalar.
        private void ApplyLook()
        {
            if (toggleImage != null) toggleImage.color = IsOn ? PressedTint : Color.white;
            if (toggleButton != null) toggleButton.transform.localScale = Vector3.one * (IsOn ? 0.94f : 1f);
            if (catcherGraphic != null) catcherGraphic.raycastTarget = IsOn;
        }
    }
}
