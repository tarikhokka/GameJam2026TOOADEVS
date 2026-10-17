using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // Masadaki kitap (LAB RAPORU / EL KİTABI) ekrana yapıştırılmış UI gibi değil, masaya konmuş gibi dursun (EEE isteği):
    // iki katlı gölge (geniş yumuşak + kitabın değdiği yerde dar koyu temas gölgesi; ışık soldan, gölge sağa-aşağı),
    // hafif eğim (kökün dönüşü, builder verir); üzerine gelince yayla kalkar, temas gölgesi kaybolur. Perspektif ve ışık görselde (EEE).
    // Kök: [Shadow, Contact, Book (Image + Button)]; gölgeler kardeş olduğu için kitabın altında çizilir. Tıklamayı Button alır.
    public class DeskBookProp : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        // Hareket: DeskMotion hover yayı (çevik, hafif yaylanır; fare çabuk girip çıkarsa yön değiştirir).
        private const float HoverLift = 12f;      // px yukarı
        private const float HoverScale = 1.05f;
        private const float Flatten = 1f;         // EEE görseli zaten masaya yatık çizildi: basma yok (eski dik görsel için 0.9 idi)
        private const float ShadowAlpha = 0.3f;   // yatay yumuşak gölge (EEE: ~%30 koyu)
        private const float ShadowAlphaLifted = 0.18f;
        private const float ShadowSpreadLifted = 1.12f;
        private const float ContactAlpha = 0.4f;  // kitabın masaya değdiği alt kenarda dar gölge
        private static readonly Color WarmLight = Color.white; // ışık görselde (soldan sıcak); ek ton yok

        [SerializeField] private RectTransform book;
        [SerializeField] private Image bookImage;
        [SerializeField] private RectTransform shadow;
        [SerializeField] private Image shadowImage;
        [SerializeField] private RectTransform contact;
        [SerializeField] private Image contactImage;

        private static Sprite softShadow;
        private Vector2 bookRest, shadowRest;
        private float lift, liftVel, target;

        private void Awake()
        {
            bookRest = book.anchoredPosition;
            shadowRest = shadow.anchoredPosition;
            shadowImage.sprite = SoftShadow();
            if (contactImage != null) contactImage.sprite = SoftShadow();
            if (bookImage != null && bookImage.sprite != null) bookImage.color = WarmLight;
            Apply();
        }

        private void OnDisable()
        {
            lift = liftVel = target = 0f; // kitap gizlenip tekrar gelince kalkık kalmasın
            Apply();
        }

        public void OnPointerEnter(PointerEventData eventData) => target = 1f;
        public void OnPointerExit(PointerEventData eventData) => target = 0f;

        private void Update()
        {
            if (Mathf.Abs(lift - target) < 0.0005f && Mathf.Abs(liftVel) < 0.0005f) return;
            lift = DeskMotion.Spring(lift, ref liftVel, target, DeskMotion.HoverFrequency, DeskMotion.HoverDamping, DeskMotion.Dt);
            Apply();
        }

        private void Apply()
        {
            float s = Mathf.Lerp(1f, HoverScale, lift);
            book.anchoredPosition = bookRest + new Vector2(0, HoverLift * lift);
            book.localScale = new Vector3(s, s * Flatten, 1f); // pivot alt kenarda: basma masaya oturtur
            shadow.anchoredPosition = shadowRest + new Vector2(3f * lift, -3f * lift);
            shadow.localScale = Vector3.one * Mathf.Lerp(1f, ShadowSpreadLifted, lift);
            SetAlpha(shadowImage, Mathf.Lerp(ShadowAlpha, ShadowAlphaLifted, lift));
            if (contactImage != null) SetAlpha(contactImage, ContactAlpha * Mathf.Clamp01(1f - lift)); // kalkınca temas kalkar
        }

        private static void SetAlpha(Image image, float alpha)
        {
            var c = image.color;
            image.color = new Color(c.r, c.g, c.b, alpha);
        }

        // Kenarları yumuşak siyah elips (radyal geçiş). Art'ta gölge görseli yok; bir kez üretilir, iki kitap paylaşır.
        private static Sprite SoftShadow()
        {
            if (softShadow != null) return softShadow;
            const int w = 128, h = 64;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color32[w * h];
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float dx = (x + 0.5f) / w * 2f - 1f, dy = (y + 0.5f) / h * 2f - 1f;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.Clamp01(1f - d);
                a = a * a * (3f - 2f * a); // smoothstep: ortası koyu, kenara doğru kaybolur
                pixels[y * w + x] = new Color32(0, 0, 0, (byte)(a * 255));
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            softShadow = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
            softShadow.name = "DeskBookShadow";
            return softShadow;
        }
    }
}
