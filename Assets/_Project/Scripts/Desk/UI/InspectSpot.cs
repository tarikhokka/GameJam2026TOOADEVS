using CivilJam.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // İnceleme modunda tıklanabilir alan (form / lab satırı, kaşe, El Kitabı satırı). Görünümler alanı çalışırken ekler (Mark).
    // Mod kapalıyken görünmez ve tıklama almaz (sürükleme ve düğmeler bugünkü gibi). Mod açıkken fare üstüne gelince ince sarı
    // çerçeve, seçilince sarı dolgu + kalın çerçeve. Hangi bilgiye ait olduğu Target (Core InspectTarget); karar Core'da (Inspection.Check).
    public class InspectSpot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private const float Pad = 4f;
        private static readonly Color Amber = new Color(0.96f, 0.72f, 0.1f);

        public InspectTarget Target { get; set; }
        public bool Available { get; set; } = true; // ör. ek test hücresi sadece test yapıldıysa

        private InspectController controller;
        private Image fill;
        private Image[] frame;
        private bool modeOn, hover, selected;

        // Verilen nesneyi tıklanabilir alan yapar (zaten öyleyse aynı bileşen döner).
        public static InspectSpot Mark(Component target, InspectTarget inspectTarget, bool available = true)
        {
            var spot = target.GetComponent<InspectSpot>();
            if (spot == null) spot = target.gameObject.AddComponent<InspectSpot>();
            spot.Target = inspectTarget;
            spot.Available = available;
            spot.Paint();
            return spot;
        }

        public Vector3 WorldCenter
        {
            get
            {
                var rt = (RectTransform)transform;
                return rt.TransformPoint(rt.rect.center);
            }
        }

        private void Awake()
        {
            // Dolgu: tıklama alanı da odur (mod kapalıyken raycast kapalı). Çerçeve: dört ince şerit, alanın biraz dışında.
            fill = Part("InspectFill", Vector2.zero, Vector2.one, new Vector2(-Pad, -Pad), new Vector2(Pad, Pad));
            fill.transform.SetAsFirstSibling();
            frame = new[]
            {
                Part("InspectTop", new Vector2(0, 1), Vector2.one, new Vector2(-Pad, Pad - 2), new Vector2(Pad, Pad)),
                Part("InspectBottom", Vector2.zero, new Vector2(1, 0), new Vector2(-Pad, -Pad), new Vector2(Pad, -Pad + 2)),
                Part("InspectLeft", Vector2.zero, new Vector2(0, 1), new Vector2(-Pad, -Pad), new Vector2(-Pad + 2, Pad)),
                Part("InspectRight", new Vector2(1, 0), Vector2.one, new Vector2(Pad - 2, -Pad), new Vector2(Pad, Pad)),
            };
            Paint();
        }

        private Image Part(string name, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(transform, false);
            go.AddComponent<LayoutElement>().ignoreLayout = true;
            var rt = (RectTransform)go.transform;
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            var image = go.AddComponent<Image>();
            image.raycastTarget = false;
            return image;
        }

        private void OnEnable()
        {
            controller = GetComponentInParent<InspectController>();
            if (controller != null) controller.Register(this);
        }

        private void OnDisable()
        {
            if (controller != null) controller.Unregister(this);
            hover = false;
        }

        internal void SetMode(bool on)
        {
            modeOn = on;
            hover = false;
            selected = false;
            Paint();
        }

        internal void SetSelected(bool on)
        {
            selected = on;
            Paint();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hover = true;
            Paint();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hover = false;
            Paint();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (modeOn && controller != null) controller.SpotClicked(this, eventData.button);
        }

        private void Paint()
        {
            if (fill == null) return;
            bool active = modeOn && Available;
            fill.raycastTarget = modeOn; // mod açıkken kullanılamayan alan da tıklamayı yutar (altındaki kağıt sürüklenmez zaten)
            fill.color = active && selected ? new Color(Amber.r, Amber.g, Amber.b, 0.28f) : Color.clear;

            float alpha = !active ? 0f : selected ? 1f : hover ? 0.85f : 0f;
            float width = selected ? 3f : 2f;
            foreach (var bar in frame)
            {
                bar.color = new Color(Amber.r, Amber.g, Amber.b, alpha);
                var rt = bar.rectTransform;
                // Şeritlerin kalınlığı (seçiliyken kalın): iç kenar dış kenardan width kadar içeride.
                if (rt.anchorMin.y == 1) rt.offsetMin = new Vector2(rt.offsetMin.x, Pad - width);
                else if (rt.anchorMax.y == 0) rt.offsetMax = new Vector2(rt.offsetMax.x, -Pad + width);
                else if (rt.anchorMax.x == 0) rt.offsetMax = new Vector2(-Pad + width, rt.offsetMax.y);
                else rt.offsetMin = new Vector2(Pad - width, rt.offsetMin.y);
            }
        }
    }
}
