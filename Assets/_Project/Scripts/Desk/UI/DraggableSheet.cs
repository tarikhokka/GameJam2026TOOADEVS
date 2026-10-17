using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CivilJam.Desk
{
    // Masadaki kağıt (açık dosya ya da El Kitabı), Papers, Please hissiyle. Hareket dili DeskMotion'da (ui-ux-pro-max §7):
    // - Açılış: masadaki kitabın yönünden küçük ve saydam başlar, yayla (spring) yerine gelir, hafif yaylanıp oturur.
    // - Kapanış: süreli ve hızlanarak (ease-in) kitaba geri gider; girişten kısa sürer, sonra panel kapanır.
    // - Sürükleme: kağıt farenin altında birebir durur; hıza göre yatar, tutulunca kalkar (büyür, gölge uzar).
    //   Bırakınca hızıyla biraz kayar ve yayla durur. Basılan kağıt öne gelir.
    // - Yerleşim değişince (ortada / yan yana) yeni yerine yayla kayar. Her hareket yarıda kesilebilir: yeni komut eskisini değiştirir.
    // Butonlar tıklamayı kendisi alır; butonun üstünden başlayan sürükleme yine kağıdı taşır ve tıklama sayılmaz.
    public class DraggableSheet : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private const float MinVisible = 200f;   // kağıdın en az bu kadarı ekranda kalır
        private const float LiftScale = 1.04f;   // tutulunca büyüme
        private const float MaxTilt = 4f;        // sürüklerken en fazla yatma (derece)
        private const float TiltPerSpeed = 0.006f; // px/sn başına derece
        private const float ThrowTime = 0.06f;   // bırakınca hızıyla bu kadar sn'lik yol daha gider
        private const float MaxThrowSpeed = 3000f;
        private static readonly Vector2 ShadowRest = new Vector2(6, -6);
        private static readonly Vector2 ShadowLifted = new Vector2(22, -22);

        [SerializeField] private Transform layer;      // öne alınan kök (pencere kökü; "Papers" katmanında kardeşleriyle)
        [SerializeField] private RectTransform shadow; // kağıdın arkasındaki gölge (ilk çocuk, yerleşim dışı)
        [SerializeField] private CanvasGroup group;

        public event Action Grabbed;
        public event Action Dropped;

        // Yatay katlanma (1 = açık, 0 = kapanmış): dosya müteahhide verilirken kart gibi kapanır (FolderHandoffView).
        public float FoldX { get; set; } = 1f;

        // Yay durumu
        private Vector2 homePos, posVel;
        private float homeScale = 1f, scale = 1f, scaleVel;
        private float lift, liftVel;      // 0 = masada, 1 = elde
        private float tilt, tiltVel;
        private float alpha = 1f, targetAlpha = 1f;
        private bool dragging;
        private Vector2 dragVel;          // sürükleme hızı (px/sn, yumuşatılmış)

        // Süreli hareket (kapanış / müteahhide verme): yay yerine eğriyle, bitince done.
        private bool tweening;
        private float tweenTime, tweenDuration;
        private Func<float, float> tweenEase;
        private Vector2 fromPos, toPos;
        private float fromScale, toScale, fromAlpha, toAlpha, fromTilt, toTilt;
        private Action tweenDone;

        private Canvas canvas;

        private RectTransform Rect => (RectTransform)transform;

        public void OnPointerDown(PointerEventData eventData) => BringToFront();

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (canvas == null) canvas = GetComponentInParent<Canvas>().rootCanvas;
            if (tweening) return; // kapanırken tutulmaz
            dragging = true;
            posVel = Vector2.zero;
            dragVel = Vector2.zero;
            BringToFront();
            Grabbed?.Invoke();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!dragging) return;
            float canvasScale = canvas != null ? canvas.scaleFactor : 1f;
            Vector2 delta = eventData.delta / canvasScale;
            Rect.anchoredPosition = Clamp(Rect.anchoredPosition + delta); // farenin altında birebir
            float dt = Mathf.Max(Time.unscaledDeltaTime, 0.001f);
            dragVel = Vector2.Lerp(dragVel, delta / dt, 0.5f);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!dragging) return;
            dragging = false;
            Vector2 throwVel = Vector2.ClampMagnitude(dragVel, MaxThrowSpeed);
            homePos = Clamp(Rect.anchoredPosition + throwVel * ThrowTime); // bırakıldığı yerde, biraz ileride durur
            posVel = throwVel * 0.5f;
            Dropped?.Invoke();
        }

        // Yerleşimdeki yeri (ortada / yan yana). Kağıt görünürse yayla gider, değilse hemen oraya konur.
        public void SetHome(Vector2 position, float sheetScale)
        {
            homePos = position;
            homeScale = sheetScale;
            if (!isActiveAndEnabled) Snap();
        }

        // Açılış: home'un offset kadar uzağından (masadaki kitabın yönü) küçük, saydam ve hafif yamuk başlar; yayla oturur.
        public void AnimateIn(Vector2 offset)
        {
            tweening = false;
            tweenDone = null;
            dragging = false;
            FoldX = 1f;
            Rect.anchoredPosition = homePos + offset;
            posVel = Vector2.zero;
            scale = homeScale * 0.6f;
            scaleVel = 0f;
            alpha = 0f;
            targetAlpha = 1f;
            tilt = offset.x > 0 ? -6f : 6f;
            tiltVel = 0f;
            Apply();
        }

        // Kapanış: offset yönüne hızlanarak kayar, küçülür ve saydamlaşır; bitince done (panel o zaman kapanır).
        public void AnimateOut(Vector2 offset, Action done) =>
            StartTween(Rect.anchoredPosition + offset, scale * 0.8f, 0f, offset.x > 0 ? -6f : 6f,
                DeskMotion.Exit(DeskMotion.Slow), DeskMotion.EaseIn, done);

        // Verilen yere (ör. müteahhidin elleri) küçülerek gider; saydamlaşmaz, bitince done.
        public void AnimateTo(Vector2 position, float sheetScale, Action done) =>
            StartTween(position, sheetScale, 1f, 0f, DeskMotion.Slow, DeskMotion.EaseInOut, done);

        private void StartTween(Vector2 pos, float toScaleValue, float toAlphaValue, float toTiltValue, float duration,
            Func<float, float> ease, Action done)
        {
            dragging = false;
            tweening = true;
            tweenTime = 0f;
            tweenDuration = duration;
            tweenEase = ease;
            fromPos = Rect.anchoredPosition; toPos = pos;
            fromScale = scale; toScale = toScaleValue;
            fromAlpha = alpha; toAlpha = toAlphaValue;
            fromTilt = tilt; toTilt = toTiltValue;
            tweenDone = done;
        }

        private void Snap()
        {
            tweening = false;
            tweenDone = null;
            dragging = false;
            Rect.anchoredPosition = homePos;
            posVel = Vector2.zero;
            scale = homeScale;
            scaleVel = 0f;
            alpha = targetAlpha = 1f;
            tilt = tiltVel = 0f;
            lift = liftVel = 0f;
            Apply();
        }

        private void Update()
        {
            float dt = DeskMotion.Dt;

            if (tweening)
            {
                tweenTime += dt;
                float k = tweenEase(tweenTime / tweenDuration);
                Rect.anchoredPosition = Vector2.LerpUnclamped(fromPos, toPos, k);
                scale = Mathf.LerpUnclamped(fromScale, toScale, k);
                alpha = Mathf.Lerp(fromAlpha, toAlpha, k);
                tilt = Mathf.LerpUnclamped(fromTilt, toTilt, k);
                lift = Mathf.Lerp(lift, 0f, k);
                Apply();
                if (tweenTime >= tweenDuration)
                {
                    tweening = false;
                    homePos = toPos;
                    homeScale = toScale;
                    var done = tweenDone;
                    tweenDone = null;
                    done?.Invoke();
                }
                return;
            }

            const float f = DeskMotion.SheetFrequency, z = DeskMotion.SheetDamping;
            if (!dragging) Rect.anchoredPosition = DeskMotion.Spring(Rect.anchoredPosition, ref posVel, homePos, f, z, dt);

            lift = DeskMotion.Spring(lift, ref liftVel, dragging ? 1f : 0f, DeskMotion.HoverFrequency, 1f, dt);
            scale = DeskMotion.Spring(scale, ref scaleVel, homeScale * Mathf.Lerp(1f, LiftScale, lift), f, z, dt);
            alpha = Mathf.MoveTowards(alpha, targetAlpha, dt / DeskMotion.Base);

            if (dragging) dragVel = Vector2.Lerp(dragVel, Vector2.zero, 1f - Mathf.Exp(-8f * dt)); // fare durunca düzelir
            float tiltTarget = dragging ? Mathf.Clamp(-dragVel.x * TiltPerSpeed, -MaxTilt, MaxTilt) : 0f;
            tilt = DeskMotion.Spring(tilt, ref tiltVel, tiltTarget, f * 1.3f, 0.6f, dt);

            Apply();
        }

        private void Apply()
        {
            Rect.localScale = new Vector3(scale * FoldX, scale, 1f);
            Rect.localRotation = Quaternion.Euler(0, 0, tilt);
            if (group != null) group.alpha = alpha;
            if (shadow != null)
            {
                Vector2 d = Vector2.LerpUnclamped(ShadowRest, ShadowLifted, lift);
                shadow.offsetMin = d;
                shadow.offsetMax = d;
            }
        }

        // Pencere kökünü Papers katmanında en öne alır (diğer kağıtlar ve masa nesneleri arkada kalır).
        public void BringToFront()
        {
            if (layer != null) layer.SetAsLastSibling();
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
