using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // İnceleme modu (Papers, Please tarzı; Ahmet onayı, memory.md "İnceleme modu"). Masadaki İNCELE düğmesi modu açıp kapatır.
    // Mod açıkken belgeler sürüklenmez, karar düğmeleri pasif (DeskScreen); tıklanabilir alanlar (InspectSpot) vurgulanır.
    // 1. tıklama alanı seçer (aynı alana tekrar = seçimi kaldırır), 2. tıklama iki alanı karşılaştırır: aralarına çizgi çekilir,
    // ortasında sonuç görünür, resultHold sn sonra (ya da tıklayınca) temizlenir; mod açık kalır. Sağ tık ya da boş masaya
    // tıklamak seçimi temizler (Esc'e dokunulmaz: Onur'un menüsü). Arayüz kural bilmez: karar Core'dan gelir (Core gelince bağlanır).
    // DeskScreen kökünde durur: alanlar onu GetComponentInParent ile bulur.
    public class InspectController : MonoBehaviour
    {
        private static readonly Color PressedTint = new Color(1f, 0.84f, 0.5f);

        [SerializeField] private Button toggleButton;
        [SerializeField] private Image toggleImage;
        [SerializeField] private InspectCatcher catcher;
        [SerializeField] private Graphic catcherGraphic;
        [SerializeField] private RectTransform overlay;   // belgelerin üstünde, tıklama almayan katman (çizgi + sonuç)
        [SerializeField] private RectTransform line;
        [SerializeField] private RectTransform resultRoot; // geçici sonuç kartı (Onur'un InspectResultView'u gelince o)
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private float lineWidth = 4f;
        [Tooltip("Sonuç bu kadar sn görünür, sonra çizgi, sonuç ve seçim temizlenir (tıklayınca hemen)")]
        [SerializeField] private float resultHold = 1.6f;

        public bool IsOn { get; private set; }
        public event Action<bool> ModeChanged;
        public event Action Clicked; // alan seçildi / karşılaştırıldı (ses için)

        private readonly List<InspectSpot> spots = new List<InspectSpot>();
        private InspectSpot first, second;
        private bool showing;
        private float shownAt;

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
            ApplyLook();
            ModeChanged?.Invoke(on);
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
                Clicked?.Invoke();
            }
        }

        // Adım 1 iskeleti: sonuç yerine iki alanın adı. Core gelince Inspection.Check + Onur'un sonuç görünümü.
        private void Compare(InspectSpot a, InspectSpot b)
        {
            showing = true;
            shownAt = Time.unscaledTime;
            line.gameObject.SetActive(true);
            if (resultText != null) resultText.text = $"KONTROL\n<size=70%>{a.Label}  —  {b.Label}</size>";
            if (resultRoot != null)
            {
                resultRoot.gameObject.SetActive(true);
                resultRoot.anchoredPosition = (Local(a) + Local(b)) * 0.5f;
                resultRoot.SetAsLastSibling();
            }
            DrawLine(0f);
        }

        private void Update()
        {
            if (!showing) return;
            float t = Time.unscaledTime - shownAt;
            DrawLine(DeskMotion.EaseOut(t / DeskMotion.Base)); // çizgi ilk alandan ikinciye doğru uzar
            if (resultRoot != null) resultRoot.localScale = Vector3.one * DeskMotion.EaseOutBack(t / DeskMotion.Base);
            if (t >= resultHold) ClearSelection();
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
            if (resultRoot != null) resultRoot.anchoredPosition = (a + b) * 0.5f;
        }

        private Vector2 Local(InspectSpot spot) => overlay.InverseTransformPoint(spot.WorldCenter);

        private void ClearSelection()
        {
            if (first != null) first.SetSelected(false);
            if (second != null) second.SetSelected(false);
            first = second = null;
            showing = false;
            if (line != null) line.gameObject.SetActive(false);
            if (resultRoot != null) resultRoot.gameObject.SetActive(false);
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
