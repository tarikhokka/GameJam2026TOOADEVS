using CivilJam.Core;
using TMPro;
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
        private static readonly Color Paper = new Color(0.97f, 0.94f, 0.87f); // öne alınan alanın arkasındaki kağıt zemin

        public InspectTarget Target { get; set; }
        public bool Available { get; set; } = true; // ör. ek test hücresi sadece test yapıldıysa

        private InspectController controller;
        private Image fill;
        private Image[] frame;
        private Image back;        // mod açıkken alanın arkasında kağıt zemin (kararmış sayfada "aydınlık şerit")
        private bool backIsSibling; // alan kendisi yazı / görselse zemin önüne çizilmesin diye kardeşi olarak arkasında durur
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
            BuildBack();
            frame = new[]
            {
                Part("InspectTop", new Vector2(0, 1), Vector2.one, new Vector2(-Pad, Pad - 2), new Vector2(Pad, Pad)),
                Part("InspectBottom", Vector2.zero, new Vector2(1, 0), new Vector2(-Pad, -Pad), new Vector2(Pad, -Pad + 2)),
                Part("InspectLeft", Vector2.zero, new Vector2(0, 1), new Vector2(-Pad, -Pad), new Vector2(-Pad + 2, Pad)),
                Part("InspectRight", new Vector2(1, 0), Vector2.one, new Vector2(Pad - 2, -Pad), new Vector2(Pad, Pad)),
            };
            Paint();
        }

        // Kağıt zemin (poliş B1). Alan bir kapsayıcıysa (satır) ilk çocuğu; kendisi yazı / görselse (tarih, imza, hücre, kaşe,
        // El Kitabı satırı) çocuk önüne çizileceği için kardeşi olur, hemen arkasına konur ve boyutu her karede alana uydurulur.
        private void BuildBack()
        {
            // Görsel alan (kaşe) zaten kendisi aydınlık kalır; arkasına kare kağıt koyulmaz.
            var image = GetComponent<Image>();
            if (image != null && image.sprite != null) return;
            backIsSibling = GetComponent<Graphic>() != null && transform.parent != null;
            if (!backIsSibling)
            {
                back = Part("InspectBack", Vector2.zero, Vector2.one, new Vector2(-Pad, -Pad), new Vector2(Pad, Pad));
                back.transform.SetAsFirstSibling();
            }
            else
            {
                var go = new GameObject(name + "_InspectBack", typeof(RectTransform));
                go.transform.SetParent(transform.parent, false);
                go.transform.SetSiblingIndex(transform.GetSiblingIndex());
                go.AddComponent<LayoutElement>().ignoreLayout = true;
                back = go.AddComponent<Image>();
                back.raycastTarget = false;
            }
            back.color = Paper;
            back.gameObject.SetActive(false);
        }

        // Kardeş zemin alanın kendi dikdörtgenini (aynı ebeveyn, aynı çapalar) her yandan Pad kadar büyük kaplar. Alan bir yazıysa
        // kutusu değil yazının kendisi kaplanır (sağa yaslı "Tarih" kutusu soldaki başlığa taşıyordu).
        private void FitBack()
        {
            var src = (RectTransform)transform;
            var dst = back.rectTransform;
            var text = GetComponent<TMP_Text>();
            if (text != null && text.textInfo != null && text.textInfo.characterCount > 0)
            {
                var bounds = text.textBounds; // yazının kendi yerel uzayında
                dst.anchorMin = dst.anchorMax = dst.pivot = new Vector2(0.5f, 0.5f);
                dst.position = src.TransformPoint(bounds.center);
                dst.sizeDelta = (Vector2)bounds.size + new Vector2(4 * Pad, 2 * Pad);
                dst.localRotation = src.localRotation;
                dst.localScale = src.localScale;
                return;
            }
            dst.anchorMin = src.anchorMin;
            dst.anchorMax = src.anchorMax;
            dst.pivot = src.pivot;
            dst.sizeDelta = src.sizeDelta + new Vector2(2 * Pad, 2 * Pad);
            dst.anchoredPosition = src.anchoredPosition + (2 * src.pivot - Vector2.one) * Pad;
            dst.localRotation = src.localRotation;
            dst.localScale = src.localScale;
        }

        private void LateUpdate()
        {
            if (backIsSibling && back != null && back.gameObject.activeSelf) FitBack();
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
            // Kullanılabilir alan karartmanın üstüne çıkar (poliş B1); kullanılamayan (ör. yapılmamış ek test) karanlıkta kalır.
            if (controller != null) InspectLift.Set(gameObject, active, controller.LiftOrder);
            if (back != null)
            {
                if (backIsSibling && active) FitBack();
                back.gameObject.SetActive(active);
                // Kardeş zemin alanın bir kat altında öne alınır: karartmanın üstünde, yazının arkasında.
                if (backIsSibling && controller != null) InspectLift.Set(back.gameObject, active, controller.LiftOrder - 1);
            }
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
