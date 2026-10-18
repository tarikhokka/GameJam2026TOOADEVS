using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    /// <summary>
    /// Kendini kuran kaşe çekmecesi. Hedefi ve açılma iznini masadan alır; oyun kararı vermez.
    /// Kalıcı kaşe izi ve ses Stamped olayını dinleyen masaya aittir.
    /// Kökün konumunu builder belirler; düğme kökün sağında, çekmece solunda durur.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class StampTrayView : MonoBehaviour
    {
        [Header("T1 görselleri — boşsa kodla çizilir")]
        [SerializeField] Sprite buttonSprite;
        [SerializeField] Sprite traySprite;
        [SerializeField] Sprite approveSprite;
        [SerializeField] Sprite rejectSprite;
        [SerializeField] TMP_FontAsset titleFont;

        [Header("Hareket (saniye, unscaled)")]
        [SerializeField, Min(0.01f)] float slideDuration = 0.25f;
        [SerializeField, Min(0.01f)] float returnDuration = 0.18f;
        [SerializeField, Min(0.01f)] float pressDuration = 0.12f;

        public bool IsOpen { get; private set; }
        public event Action<bool> OpenChanged;
        public event Action<bool> Stamped;

        const float TrayWidth = 520f;
        static readonly Color Ink = new Color(0.23f, 0.18f, 0.13f);
        static readonly Color Paper = new Color(0.97f, 0.94f, 0.85f);
        static readonly Color Wood = new Color(0.48f, 0.31f, 0.17f);
        static readonly Color Green = new Color(0.20f, 0.52f, 0.23f);
        static readonly Color Red = new Color(0.72f, 0.17f, 0.13f);

        sealed class Stamp
        {
            public bool Approve;
            public RectTransform Rect;
            public Image Art, Shadow;
            public GameObject Fallback;
            public TMP_Text Label;
            public Vector2 Home;
        }

        RectTransform root, outside, tray, dragLayer;
        Image trayArt;
        CanvasGroup trayGroup;
        Button toggle;
        Stamp approve, reject, held;
        Func<RectTransform> target;
        Func<bool> canOpen;
        Vector2 grabOffset;
        float openness;
        bool resolving;
        Coroutine slide, motion;
        // Close/Hide/Bind during an event cancels the old operation, including its deferred callbacks.
        int operation;
        readonly Vector3[] corners = new Vector3[4];

        void Awake() => EnsureBuilt();

        public void Bind(Func<RectTransform> target, Func<bool> canOpen)
        {
            EnsureBuilt();
            Close(true);
            this.target = target;
            this.canOpen = canOpen;
            RefreshInput();
        }

        public void SetVisible(bool visible)
        {
            EnsureBuilt();
            if (!visible) Close(true);
            gameObject.SetActive(visible);
            if (visible) RefreshInput();
        }

        public void Close(bool instant)
        {
            EnsureBuilt();
            operation++;
            if (motion != null) { StopCoroutine(motion); motion = null; }
            resolving = false;
            held = null;
            Restore(approve);
            Restore(reject);
            bool changed = IsOpen;
            IsOpen = false;
            outside.gameObject.SetActive(false);
            trayGroup.blocksRaycasts = trayGroup.interactable = false;
            StartSlide(false, instant);
            RefreshInput();
            if (changed) OpenChanged?.Invoke(false);
        }

        void Update()
        {
            RefreshInput();
            if (IsOpen)
            {
                FitOutside();
                // Once impact has happened the permission normally becomes false (form is stamped).
                // Finish the return animation in that case; before impact Resolve rechecks permission.
                if (!resolving && !Allowed()) Close(false);
            }
        }

        bool Allowed()
        {
            var area = target?.Invoke();
            // Respect the host's input lock (e.g. disabled raycaster), without a Flow dependency.
            var caster = GetComponentInParent<GraphicRaycaster>();
            return isActiveAndEnabled && canOpen != null && canOpen()
                && (caster == null || caster.isActiveAndEnabled)
                && area != null && area.gameObject.activeInHierarchy
                && area.rect.width > 0f && area.rect.height > 0f;
        }

        void Toggle()
        {
            if (resolving || held != null) return;
            if (IsOpen) { Close(false); return; }
            if (!Allowed()) return;
            operation++;
            IsOpen = true;
            transform.SetAsLastSibling();
            FitOutside();
            outside.gameObject.SetActive(true);
            StartSlide(true, false);
            RefreshInput();
            OpenChanged?.Invoke(true);
        }

        void RefreshInput()
        {
            if (toggle == null) return;
            toggle.interactable = !resolving && held == null && Allowed();
            bool pick = IsOpen && openness >= 0.999f && !resolving && held == null && Allowed();
            trayGroup.interactable = trayGroup.blocksRaycasts = pick;
        }

        void BeginDrag(Stamp stamp, PointerEventData e)
        {
            if (e.button != PointerEventData.InputButton.Left || !IsOpen || openness < 0.999f
                || held != null || resolving || !Allowed()) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(dragLayer, e.position,
                e.pressEventCamera, out var pointer)) return;
            held = stamp;
            stamp.Rect.SetParent(dragLayer, true); // outside the tray's clipping rectangle
            grabOffset = (Vector2)stamp.Rect.localPosition - pointer;
            stamp.Rect.localScale = Vector3.one * 1.08f;
            stamp.Shadow.rectTransform.anchoredPosition = new Vector2(12f, -12f);
            RefreshInput();
        }

        void Drag(Stamp stamp, PointerEventData e)
        {
            if (held != stamp || resolving) return;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(dragLayer, e.position,
                e.pressEventCamera, out var pointer)) stamp.Rect.localPosition = pointer + grabOffset;
        }

        void EndDrag(Stamp stamp, PointerEventData e)
        {
            if (held != stamp || resolving) return;
            bool valid = Allowed();
            var area = valid ? target() : null;
            var targetCanvas = area != null ? area.GetComponentInParent<Canvas>() : null;
            var camera = targetCanvas != null && targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? targetCanvas.worldCamera : null;
            valid = valid && RectTransformUtility.RectangleContainsScreenPoint(area, e.position, camera);
            resolving = true;
            RefreshInput();
            motion = StartCoroutine(Resolve(stamp, valid, operation));
        }

        IEnumerator Resolve(Stamp stamp, bool valid, int token)
        {
            if (valid)
            {
                // Align the contact animation with the form, independent of the original grab point.
                Vector3 hit = dragLayer.InverseTransformPoint(target().TransformPoint(target().rect.center));
                hit.y += 52f; // align the rubber foot, rather than the wooden handle, with the ink area
                Vector3 from = stamp.Rect.localPosition;
                for (float t = 0f; t < pressDuration; t += DeskMotion.Dt)
                {
                    float k = DeskMotion.EaseIn(t / pressDuration);
                    stamp.Rect.localPosition = Vector3.Lerp(from, hit, k);
                    stamp.Rect.localScale = new Vector3(Mathf.Lerp(1.08f, 1f, k), Mathf.Lerp(1.08f, 0.90f, k), 1f);
                    yield return null;
                }
                if (token != operation) yield break;
                valid = Allowed(); // target/permission may have disappeared during the animation
                if (valid)
                {
                    stamp.Rect.localPosition = hit;
                    stamp.Rect.localScale = new Vector3(1f, 0.90f, 1f);
                    Stamped?.Invoke(stamp.Approve); // ink + sound belong to DeskScreen
                    if (token != operation) yield break; // listener may Close/Hide/Bind synchronously
                }
            }

            Vector3 start = stamp.Rect.localPosition;
            Vector3 startScale = stamp.Rect.localScale;
            for (float t = 0f; t < returnDuration; t += DeskMotion.Dt)
            {
                float k = DeskMotion.EaseOut(t / returnDuration);
                Vector3 home = dragLayer.InverseTransformPoint(tray.TransformPoint(stamp.Home));
                stamp.Rect.localPosition = Vector3.Lerp(start, home, k);
                stamp.Rect.localScale = Vector3.Lerp(startScale, Vector3.one, k);
                yield return null;
            }
            Restore(stamp);
            held = null;
            resolving = false;
            motion = null;
            if (valid || !Allowed()) Close(false);
            else RefreshInput(); // an invalid drop leaves the tray open
        }

        void StartSlide(bool open, bool instant)
        {
            if (slide != null) { StopCoroutine(slide); slide = null; }
            if (instant || !isActiveAndEnabled)
            {
                SetOpenness(open ? 1f : 0f);
                return;
            }
            slide = StartCoroutine(Slide(open ? 1f : 0f));
        }

        IEnumerator Slide(float to)
        {
            float from = openness;
            float duration = Mathf.Max(0.01f, slideDuration * Mathf.Abs(to - from));
            for (float t = 0f; t < duration; t += DeskMotion.Dt)
            {
                SetOpenness(Mathf.Lerp(from, to, DeskMotion.EaseOut(t / duration)));
                yield return null;
            }
            SetOpenness(to);
            slide = null;
            RefreshInput();
        }

        void SetOpenness(float value)
        {
            openness = value;
            tray.anchoredPosition = new Vector2(TrayWidth * (1f - value), 0f);
            trayGroup.alpha = Mathf.Clamp01(value * 3f);
        }

        void Restore(Stamp stamp)
        {
            if (stamp == null) return;
            stamp.Rect.SetParent(tray, false);
            stamp.Rect.anchorMin = stamp.Rect.anchorMax = new Vector2(0.5f, 0.5f);
            stamp.Rect.anchoredPosition = stamp.Home;
            stamp.Rect.localScale = Vector3.one;
            stamp.Rect.localRotation = Quaternion.identity;
            stamp.Shadow.rectTransform.anchoredPosition = new Vector2(5f, -5f);
        }

        void OnDisable()
        {
            operation++;
            StopAllCoroutines();
            slide = motion = null;
            held = null;
            resolving = false;
            if (tray == null) return;
            Restore(approve);
            Restore(reject);
            SetOpenness(0f);
            outside.gameObject.SetActive(false);
            trayGroup.interactable = trayGroup.blocksRaycasts = false;
            bool changed = IsOpen;
            IsOpen = false;
            if (changed) OpenChanged?.Invoke(false);
        }

        // The click catcher has no dim: the builder owns dimming/lifting the target and tray together.
        void FitOutside()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;
            ((RectTransform)canvas.rootCanvas.transform).GetWorldCorners(corners);
            Vector3 min = root.InverseTransformPoint(corners[0]);
            Vector3 max = root.InverseTransformPoint(corners[2]);
            outside.localPosition = (min + max) * 0.5f;
            outside.sizeDelta = max - min;
        }

        void EnsureBuilt()
        {
            if (tray != null) return;
            root = (RectTransform)transform;

            outside = Rect("Outside", root);
            var hit = outside.gameObject.AddComponent<Image>();
            hit.color = Color.clear;
            var dismiss = outside.gameObject.AddComponent<Button>();
            dismiss.transition = Selectable.Transition.None;
            dismiss.onClick.AddListener(() => { if (!resolving && held == null) Close(false); });
            outside.gameObject.SetActive(false);

            var clip = Rect("TrayClip", root);
            Place(clip, new Vector2(1f, 0.5f), new Vector2(-120f, 0f), new Vector2(TrayWidth, 240f));
            clip.pivot = new Vector2(1f, 0.5f);
            clip.gameObject.AddComponent<RectMask2D>();
            tray = Rect("Tray", clip);
            tray.sizeDelta = new Vector2(TrayWidth, 220f);
            trayArt = Image("Wood", tray);
            Stretch(trayArt.rectTransform);
            trayArt.raycastTarget = true; // the spaces between stamps are inside the drawer
            trayGroup = tray.gameObject.AddComponent<CanvasGroup>();
            approve = BuildStamp(true, new Vector2(-130f, 8f));
            reject = BuildStamp(false, new Vector2(130f, 8f));

            var button = Image("Toggle", root);
            Place(button.rectTransform, new Vector2(1f, 0.5f), new Vector2(-55f, 0f), new Vector2(110f, 110f));
            button.raycastTarget = true;
            toggle = button.gameObject.AddComponent<Button>();
            toggle.targetGraphic = button;
            toggle.navigation = new Navigation { mode = Navigation.Mode.None };
            toggle.onClick.AddListener(Toggle);
            var title = Text("Label", button.transform, "KAŞE", 19f);
            title.color = Paper;
            Place(title.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, -17f), new Vector2(120f, 30f));

            dragLayer = Rect("DragLayer", root);
            Stretch(dragLayer);
            ApplyAppearance();
            SetOpenness(0f);
            RefreshInput();
        }

        Stamp BuildStamp(bool accepted, Vector2 home)
        {
            var stamp = new Stamp { Approve = accepted, Home = home };
            stamp.Rect = Rect(accepted ? "Approve" : "Reject", tray);
            stamp.Rect.sizeDelta = new Vector2(140f, 168f);
            stamp.Shadow = Image("Shadow", stamp.Rect);
            Stretch(stamp.Shadow.rectTransform);
            stamp.Shadow.color = new Color(0f, 0f, 0f, 0.20f);
            stamp.Art = Image("Art", stamp.Rect);
            Stretch(stamp.Art.rectTransform);
            stamp.Art.raycastTarget = true;
            var fallback = Rect("Fallback", stamp.Art.transform);
            Stretch(fallback);
            stamp.Fallback = fallback.gameObject;
            var handle = Image("Handle", fallback);
            Place(handle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 32f), new Vector2(44f, 90f));
            handle.color = Wood;
            Outline(handle);
            var foot = Image("Foot", fallback);
            Place(foot.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(124f, 55f));
            foot.color = accepted ? Green : Red;
            Outline(foot);
            stamp.Label = Text("Label", stamp.Rect, accepted ? "ONAY" : "RED", 19f);
            Place(stamp.Label.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -39f), new Vector2(82f, 25f));
            var trigger = stamp.Art.gameObject.AddComponent<EventTrigger>();
            AddEvent(trigger, EventTriggerType.BeginDrag, e => BeginDrag(stamp, (PointerEventData)e));
            AddEvent(trigger, EventTriggerType.Drag, e => Drag(stamp, (PointerEventData)e));
            AddEvent(trigger, EventTriggerType.EndDrag, e => EndDrag(stamp, (PointerEventData)e));
            Restore(stamp);
            return stamp;
        }

        void ApplyAppearance()
        {
            var button = toggle.GetComponent<Image>();
            button.sprite = buttonSprite;
            button.preserveAspect = true;
            button.color = buttonSprite != null ? Color.white : Wood;
            trayArt.sprite = traySprite;
            trayArt.color = traySprite != null ? Color.white : Wood;
            ApplyStamp(approve, approveSprite);
            ApplyStamp(reject, rejectSprite);
            foreach (var label in GetComponentsInChildren<TMP_Text>(true))
                if (titleFont != null) label.font = titleFont;
        }

        static void ApplyStamp(Stamp stamp, Sprite sprite)
        {
            stamp.Art.sprite = stamp.Shadow.sprite = sprite;
            stamp.Art.color = sprite != null ? Color.white : Color.clear;
            stamp.Fallback.SetActive(sprite == null);
            stamp.Label.color = sprite != null ? Ink : Paper;
            stamp.Label.rectTransform.anchoredPosition = new Vector2(0f, sprite != null ? -24f : -40f);
        }

        static void AddEvent(EventTrigger trigger, EventTriggerType type, Action<BaseEventData> callback)
        {
            var entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(e => callback(e));
            trigger.triggers.Add(entry);
        }

        static RectTransform Rect(string name, Transform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            return rect;
        }

        static Image Image(string name, Transform parent)
        {
            var image = Rect(name, parent).gameObject.AddComponent<Image>();
            image.raycastTarget = false;
            return image;
        }

        TMP_Text Text(string name, Transform parent, string value, float size)
        {
            var text = Rect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = text.fontSizeMax = size;
            text.fontSizeMin = 14f;
            text.enableAutoSizing = true;
            text.fontStyle = FontStyles.Normal;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Ink;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            if (titleFont != null) text.font = titleFont;
            return text;
        }

        static void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        static void Outline(Image image)
        {
            var border = image.gameObject.AddComponent<Outline>();
            border.effectColor = Ink;
            border.effectDistance = new Vector2(2f, -2f);
        }

#if UNITY_EDITOR
        void Reset()
        {
            const string art = "Assets/_Project/Art/";
            if (buttonSprite == null) buttonSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(art + "btn_stamps.png");
            if (traySprite == null) traySprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(art + "stamp_tray.png");
            if (approveSprite == null) approveSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(art + "stamp_approve.png");
            if (rejectSprite == null) rejectSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(art + "stamp_reject.png");
            if (titleFont == null) titleFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/PaytoneOne-Regular SDF.asset");
            if (tray != null) ApplyAppearance();
        }
#endif
    }
}
