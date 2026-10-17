using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // Masadaki kağıt (açık dosya ya da El Kitabı), Papers, Please hissiyle:
    // - Açılınca masadaki kitabın yönünden kayarak gelir, kapanınca o yöne gider (AnimateIn / AnimateOut).
    // - Tutulunca hafifçe büyüyüp kalkar, gölgesi uzar, sürükleme hızına göre biraz yatar; bırakınca yerine oturur.
    // - Yerleşim değişince (ortada / yan yana) yeni yerine kayar (SetHome). Basılan kağıt öne gelir.
    // Butonlar tıklamayı kendisi alır; butonun üstünden başlayan sürükleme yine kağıdı taşır ve tıklama sayılmaz.
    public class DraggableSheet : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private const float MinVisible = 200f;   // kağıdın en az bu kadarı ekranda kalır
        private const float MoveSpeed = 14f;     // yerine kayma hızı (üstel yaklaşım)
        private const float LiftScale = 1.04f;   // tutulunca büyüme
        private const float MaxTilt = 4f;        // sürüklerken en fazla yatma (derece)
        private static readonly Vector2 ShadowRest = new Vector2(6, -6);
        private static readonly Vector2 ShadowLifted = new Vector2(22, -22);

        [SerializeField] private Transform layer;      // öne alınan kök (Dossier ya da Rulebook)
        [SerializeField] private Transform otherLayer; // diğer kağıdın kökü (ikisi kardeş)
        [SerializeField] private RectTransform shadow; // kağıdın arkasındaki gölge (ilk çocuk, yerleşim dışı)
        [SerializeField] private CanvasGroup group;

        public event Action Grabbed;
        public event Action Dropped;

        // Yatay katlanma (1 = açık, 0 = kapanmış): dosya müteahhide verilirken kart gibi kapanır (FolderHandoffView).
        public float FoldX { get; set; } = 1f;

        private Vector2 homePos;
        private float homeScale = 1f;
        private float scale = 1f;
        private float alpha = 1f, targetAlpha = 1f;
        private float lift;      // 0 = masada, 1 = elde
        private float tilt, targetTilt;
        private bool dragging;
        private Action arrived;  // AnimateOut bitince
        private Canvas canvas;

        private RectTransform Rect => (RectTransform)transform;

        public void OnPointerDown(PointerEventData eventData) => BringToFront();

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (canvas == null) canvas = GetComponentInParent<Canvas>().rootCanvas;
            dragging = true;
            arrived = null;
            BringToFront();
            Grabbed?.Invoke();
        }

        public void OnDrag(PointerEventData eventData)
        {
            float canvasScale = canvas != null ? canvas.scaleFactor : 1f;
            Vector2 delta = eventData.delta / canvasScale;
            Rect.anchoredPosition = Clamp(Rect.anchoredPosition + delta);
            targetTilt = Mathf.Clamp(-delta.x * 0.25f, -MaxTilt, MaxTilt);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            dragging = false;
            homePos = Rect.anchoredPosition; // bırakıldığı yerde kalır
            targetTilt = 0f;
            Dropped?.Invoke();
        }

        // Yerleşimdeki yeri (ortada / yan yana). Kağıt görünürse kayarak gider, değilse hemen oraya konur.
        public void SetHome(Vector2 position, float sheetScale)
        {
            homePos = position;
            homeScale = sheetScale;
            if (!isActiveAndEnabled) Snap();
        }

        // Açılış: home'un offset kadar uzağından (masadaki kitabın yönü) küçük ve saydam başlar, yerine kayar.
        public void AnimateIn(Vector2 offset)
        {
            arrived = null;
            dragging = false;
            FoldX = 1f;
            Rect.anchoredPosition = homePos + offset;
            scale = homeScale * 0.6f;
            alpha = 0f;
            targetAlpha = 1f;
            tilt = offset.x > 0 ? -6f : 6f;
            targetTilt = 0f;
            Apply();
        }

        // Kapanış: offset yönüne kayıp saydamlaşır, varınca done çağrılır (panel o zaman kapanır).
        public void AnimateOut(Vector2 offset, Action done)
        {
            dragging = false;
            homePos = Rect.anchoredPosition + offset;
            targetAlpha = 0f;
            targetTilt = offset.x > 0 ? -6f : 6f;
            arrived = done;
        }

        // Verilen yere (ör. müteahhidin elleri) küçülerek kayar; saydamlaşmaz, varınca done (panel kapanır).
        public void AnimateTo(Vector2 position, float sheetScale, Action done)
        {
            dragging = false;
            homePos = position;
            homeScale = sheetScale;
            targetAlpha = 1f;
            targetTilt = 0f;
            arrived = done;
        }

        private void Snap()
        {
            arrived = null;
            Rect.anchoredPosition = homePos;
            scale = homeScale;
            alpha = targetAlpha = 1f;
            tilt = targetTilt = 0f;
            lift = 0f;
            Apply();
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            float k = 1f - Mathf.Exp(-MoveSpeed * dt);

            if (!dragging) Rect.anchoredPosition = Vector2.Lerp(Rect.anchoredPosition, homePos, k);
            lift = Mathf.Lerp(lift, dragging ? 1f : 0f, k * 1.5f);
            scale = Mathf.Lerp(scale, homeScale * Mathf.Lerp(1f, LiftScale, lift), k);
            alpha = Mathf.Lerp(alpha, targetAlpha, k);
            if (dragging) targetTilt = Mathf.Lerp(targetTilt, 0f, k * 0.5f); // fare durunca düzelir
            tilt = Mathf.Lerp(tilt, targetTilt, k);
            Apply();

            if (arrived != null && (Rect.anchoredPosition - homePos).sqrMagnitude < 4f)
            {
                var done = arrived;
                arrived = null;
                done();
            }
        }

        private void Apply()
        {
            Rect.localScale = new Vector3(scale * FoldX, scale, 1f);
            Rect.localRotation = Quaternion.Euler(0, 0, tilt);
            if (group != null) group.alpha = alpha;
            if (shadow != null)
            {
                Vector2 d = Vector2.Lerp(ShadowRest, ShadowLifted, lift);
                shadow.offsetMin = d;
                shadow.offsetMax = d;
            }
        }

        private void BringToFront()
        {
            if (layer == null || otherLayer == null) return;
            int other = otherLayer.GetSiblingIndex();
            if (layer.GetSiblingIndex() < other) layer.SetSiblingIndex(other);
        }

        // Kağıt ortaya sabitli (anchor/pivot 0.5); ebeveyn tam ekran.
        private Vector2 Clamp(Vector2 position)
        {
            var parent = (RectTransform)Rect.parent;
            Vector2 half = parent.rect.size * 0.5f;
            Vector2 sheetHalf = Rect.rect.size * homeScale * 0.5f;
            Vector2 limit = half + sheetHalf - new Vector2(MinVisible, MinVisible);
            return new Vector2(Mathf.Clamp(position.x, -limit.x, limit.x), Mathf.Clamp(position.y, -limit.y, limit.y));
        }
    }
}
