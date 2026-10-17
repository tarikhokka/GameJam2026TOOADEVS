using UnityEngine;

namespace CivilJam.Desk
{
    // Masa ekranının ortak hareket dili (ui-ux-pro-max §7 Animation): bütün animasyonlar aynı süre ve eğri setini kullanır.
    // - Varış yavaşlayarak (ease-out), çıkış hızlanarak (ease-in); çıkış girişin ~%65'i kadar sürer.
    // - Elle tutulan / yerine oturan şeyler yaylı (spring) hareket eder: hızı korur, hafif yaylanıp durur, her an yön değiştirebilir.
    // - Süreler Time.unscaledDeltaTime ile: oyun duraklasa da (Esc menüsü) yarım kalmaz.
    public static class DeskMotion
    {
        // Süre seti (sn). Mesafe/karmaşıklık arttıkça uzar.
        public const float Fast = 0.16f;   // küçük geri bildirim: balon, buton, kaşe
        public const float Base = 0.26f;   // kart / pencere açılışı
        public const float Slow = 0.42f;   // ekranı boydan geçen hareket (müteahhit, klasör yolculuğu)
        public const float ExitFactor = 0.65f;
        public static float Exit(float enter) => enter * ExitFactor;

        // Yay ayarları: frekans (Hz) yüksekse çabuk, sönüm < 1 ise hafif yaylanır.
        public const float SheetFrequency = 3.2f, SheetDamping = 0.78f; // kağıtlar: belirgin ama kısa yaylanma
        public const float HoverFrequency = 5f, HoverDamping = 0.7f;    // üzerine gelme: çevik

        public static float EaseOut(float t) { t = Mathf.Clamp01(t); float u = 1f - t; return 1f - u * u * u; }
        public static float EaseIn(float t) { t = Mathf.Clamp01(t); return t * t * t; }
        public static float EaseInOut(float t)
        {
            t = Mathf.Clamp01(t);
            return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) * 0.5f;
        }

        // Hedefi biraz geçip geri oturur (pop / yerine oturma). overshoot ~1.2 hafif, 1.7 belirgin.
        public static float EaseOutBack(float t, float overshoot = 1.2f)
        {
            t = Mathf.Clamp01(t) - 1f;
            return 1f + (overshoot + 1f) * t * t * t + overshoot * t * t;
        }

        // Yarı-örtük sönümlü yay (kararlı, kare hızından bağımsız). value/velocity çağıran tarafta tutulur.
        public static float Spring(float value, ref float velocity, float target, float frequency, float damping, float dt)
        {
            float omega = 2f * Mathf.PI * frequency;
            float f = 1f + 2f * dt * damping * omega;
            float oo = omega * omega, hoo = dt * oo, hhoo = dt * hoo;
            float det = 1f / (f + hhoo);
            float newValue = (f * value + dt * velocity + hhoo * target) * det;
            velocity = (velocity + hoo * (target - value)) * det;
            return newValue;
        }

        public static Vector2 Spring(Vector2 value, ref Vector2 velocity, Vector2 target, float frequency, float damping, float dt)
        {
            float vx = velocity.x, vy = velocity.y;
            var result = new Vector2(
                Spring(value.x, ref vx, target.x, frequency, damping, dt),
                Spring(value.y, ref vy, target.y, frequency, damping, dt));
            velocity = new Vector2(vx, vy);
            return result;
        }

        // Uzun karelerde (takılma) yay patlamasın.
        public static float Dt => Mathf.Min(Time.unscaledDeltaTime, 1f / 30f);
    }
}
