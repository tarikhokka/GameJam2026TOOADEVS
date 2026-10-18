using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // Masadaki kağıt (açık dosya ya da El Kitabı), Papers, Please hissiyle. Hareket dili DeskMotion'da (ui-ux-pro-max §7):
    // - Açılış: masadaki kitabın yönünden küçük ve saydam başlar, yayla (spring) yerine gelir, hafif yaylanıp oturur.
    // - Kapanış: süreli ve hızlanarak (ease-in) kitaba geri gider; girişten kısa sürer, sonra panel kapanır.
    // - Sürükleme: kağıt farenin altında birebir durur; hıza göre yatar, tutulunca kalkar (büyür, gölge uzar).
    //   Bırakınca hızıyla biraz kayar ve yayla durur. Basılan kağıt öne gelir.
    // - Yerleşim değişince (ortada / yan yana) yeni yerine yayla kayar. Her hareket yarıda kesilebilir: yeni komut eskisini değiştirir.
    // Düğmenin üstünden başlayan basış sürükleme sayılmaz (tıklama her seferinde çalışır); kağıdın geri kalanından tutulunca sürüklenir.
    public class DraggableSheet : MonoBehaviour, IPointerDownHandler, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler,
        IEndDragHandler
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
        [Tooltip("Yeni hedef arayüzde belgeler masada sabit: sürükleme kapalı (açılış/kapanış hareketi kalır)")]
        [SerializeField] private bool draggable = true;

        [Tooltip("Masa kenarı (ebeveyn ortasına göre y, px): fare bunun üstüne, ofis / müteahhit tarafına çıkınca belge iade için ikon boyutuna küçülür. 540 - DeskEdgeY (380)")]
        [SerializeField] private float deskEdgeY = 160f;

        // İnceleme modu açıkken sürükleme kilitli (InspectController → DeskScreen); tıklama ve öne alma sürer.
        public bool Locked { get; set; }

        // (belge turu B1) İade: kaşe basıldıysa (DeskScreen.IsStamped) masa kenarının üstüne taşınan belge ikon boyutuna
        // (HandBackScale, home ölçeğine oran; PaperWindow.OpenFrom verir) küçülür, geri inince büyür. Ofis tarafında bırakılan
        // belge (kaşeden önce de) masadaki yerine döner; müteahhide verme 4. adımda.
        public Func<bool> CanHandBack { get; set; }
        public float HandBackScale { get; set; } = 0.3f;
        public bool OverOffice => dragging && overOffice;
        // Bırakılan yer müteahhit mi (PaperWindow verir). Kaşeliyse HandBackDropped yayınlanır, kağıdı PaperWindow götürür.
        public Func<PointerEventData, bool> IsHandBackTarget { get; set; }
        public event Action HandBackDropped;
        public event Action HandBackRefused; // kaşeden önce müteahhide bırakıldı (kağıt yerine döner)

        public event Action Grabbed;
        public event Action Dropped;

        // Yatay katlanma (1 = açık, 0 = kapanmış). Şu an kullanılmıyor (eski klasör teslimi kalktı); 1 kalır.
        public float FoldX { get; set; } = 1f;

        // Yay durumu
        private Vector2 homePos, posVel;
        private float homeScale = 1f, scale = 1f, scaleVel;
        private float lift, liftVel;      // 0 = masada, 1 = elde
        private float tilt, tiltVel;
        private float alpha = 1f, targetAlpha = 1f;
        private bool dragging;
        private Vector2 dragVel;          // sürükleme hızı (px/sn, yumuşatılmış)
        private Vector2 pointer;          // farenin ebeveyndeki yeri
        private Vector2 grab;             // tutulan noktanın kağıt ortasına uzaklığı (home ölçeğinde)
        private Vector2 dragStartHome;    // ofis tarafında bırakılırsa döneceği yer
        private bool overOffice;          // fare masa kenarının üstünde

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

        // Düğmenin (ONAY / RED / EK TEST, sekmeler, kat grubu…) üstünden başlayan basış asla sürükleme olmaz: aksi halde fare
        // birkaç piksel kayınca Unity sürüklemeyi başlatıp tıklamayı iptal eder (düğme "2-3 tıklamada bir" çalışır).
        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (!draggable || Locked || PressedOnSelectable(eventData)) eventData.pointerDrag = null;
        }

        private bool PressedOnSelectable(PointerEventData eventData)
        {
            var hit = eventData.pointerPressRaycast.gameObject;
            for (var t = hit != null ? hit.transform : null; t != null && t != transform; t = t.parent)
            {
                var selectable = t.GetComponent<Selectable>();
                if (selectable != null && selectable.IsInteractable()) return true;
            }
            return false;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (canvas == null) canvas = GetComponentInParent<Canvas>().rootCanvas;
            if (tweening || !draggable || Locked) return; // kapanırken tutulmaz; sabit belgede hiç
            dragging = true;
            posVel = Vector2.zero;
            dragVel = Vector2.zero;
            dragStartHome = homePos;
            pointer = PointerLocal(eventData);
            grab = (pointer - Rect.anchoredPosition) / ScaleRatio;
            overOffice = pointer.y > deskEdgeY;
            BringToFront();
            Grabbed?.Invoke();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!dragging) return;
            float canvasScale = canvas != null ? canvas.scaleFactor : 1f;
            Vector2 delta = eventData.delta / canvasScale;
            pointer = PointerLocal(eventData);
            overOffice = pointer.y > deskEdgeY;
            FollowPointer(); // farenin altında birebir (küçülürken tutulan nokta farenin altında kalır)
            float dt = Mathf.Max(Time.unscaledDeltaTime, 0.001f);
            dragVel = Vector2.Lerp(dragVel, delta / dt, 0.5f);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!dragging) return;
            dragging = false;
            if (overOffice)
            {
                overOffice = false;
                bool onTarget = IsHandBackTarget != null && IsHandBackTarget(eventData);
                if (onTarget && CanHandBack != null && CanHandBack())
                {
                    HandBackDropped?.Invoke(); // müteahhide verildi
                    return;
                }
                if (onTarget) HandBackRefused?.Invoke();
                // Ofis tarafında bırakıldı (müteahhide verilmedi ya da kaşe yok): masadaki yerine yayla döner, büyür.
                homePos = dragStartHome;
                posVel = Vector2.zero;
                Dropped?.Invoke();
                return;
            }
            Vector2 throwVel = Vector2.ClampMagnitude(dragVel, MaxThrowSpeed);
            homePos = Clamp(Rect.anchoredPosition + throwVel * ThrowTime); // bırakıldığı yerde, biraz ileride durur
            posVel = throwVel * 0.5f;
            Dropped?.Invoke();
        }

        private float ScaleRatio => homeScale > 0.0001f ? scale / homeScale : 1f;

        private bool Shrunk => dragging && overOffice && CanHandBack != null && CanHandBack();

        private void FollowPointer() => Rect.anchoredPosition = Clamp(pointer - grab * ScaleRatio);

        private Vector2 PointerLocal(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)Rect.parent, eventData.position,
                eventData.pressEventCamera, out var local);
            return local;
        }

        // Yerleşimdeki yeri (ortada / yan yana). Kağıt görünürse yayla gider, değilse hemen oraya konur.
        public void SetHome(Vector2 position, float sheetScale)
        {
            homePos = position;
            homeScale = sheetScale;
            if (!isActiveAndEnabled) Snap();
        }

        // Açılış: home'un offset kadar uzağından (masadaki kitabın yönü) küçük, saydam ve hafif yamuk başlar; yayla oturur.
        public void AnimateIn(Vector2 offset) => AnimateIn(offset, 0.6f, 0f);

        // startScale: home ölçeğine oranla başlangıç (ikondan açılışta ikon boyutu); startAlpha: başlangıç saydamlığı.
        public void AnimateIn(Vector2 offset, float startScale, float startAlpha)
        {
            tweening = false;
            tweenDone = null;
            dragging = false;
            FoldX = 1f;
            Rect.anchoredPosition = homePos + offset;
            posVel = Vector2.zero;
            scale = homeScale * startScale;
            scaleVel = 0f;
            alpha = startAlpha;
            targetAlpha = 1f;
            tilt = offset.x > 0 ? -6f : 6f;
            tiltVel = 0f;
            Apply();
        }

        // Kapanış: offset yönüne hızlanarak kayar, küçülür ve saydamlaşır; bitince done (panel o zaman kapanır).
        public void AnimateOut(Vector2 offset, Action done) =>
            StartTween(Rect.anchoredPosition + offset, scale * 0.8f, 0f, offset.x > 0 ? -6f : 6f,
                DeskMotion.Exit(DeskMotion.Slow), DeskMotion.EaseIn, done);

        // (belge turu B1) Müteahhide verildi: bulunduğu boyuttan biraz daha küçülüp müteahhide kayar ve kaybolur; bitince done.
        public void AnimateGive(Vector2 position, Action done) =>
            StartTween(position, scale * 0.6f, 0f, 0f, DeskMotion.Base, DeskMotion.EaseIn, done);

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
            float scaleTarget = Shrunk ? homeScale * HandBackScale : homeScale * Mathf.Lerp(1f, LiftScale, lift);
            scale = DeskMotion.Spring(scale, ref scaleVel, scaleTarget, f, z, dt);
            if (dragging) FollowPointer(); // ölçek değişirken tutulan nokta farenin altında kalsın
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
