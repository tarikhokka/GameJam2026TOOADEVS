using UnityEngine;

namespace CivilJam.Core
{
    /// <summary>Tüm denge sayıları burada. Değerler öneri; ekip kararıyla sadece asset değişir.</summary>
    [CreateAssetMenu(menuName = "CivilJam/Balance", fileName = "Balance")]
    public class BalanceSO : ScriptableObject
    {
        [Header("Takvim")]
        public int TotalDays = 10;

        [Header("Para")]
        public int StartMoney = 2500;
        public int DebtLimit = 2000;      // Borç bunu aşarsa ofis kapanır (Orta son)
        public int ExtraTestCost = 300;   // Anında düşülür
        public int WindowRepairCost = 200;
        [Header("Ziyaret programı (memory.md \"📅\")")]
        [Tooltip("Açıksa oyun başında bütün günlerin ziyaret programı kurulur: ana müteahhitler MainClientVisits kez, " +
                 "kalan yerler kişiliksiz karakterler; projelerin CompliantShare'i uygun. Kapalıysa eski gün gün üretim.")]
        public bool UseVisitSchedule;
        public int MainClientVisits = 3;
        [Range(0f, 1f)] public float CompliantShare = 0.5f;

        [Tooltip("Jüri vitrini (Ahmet kararı 18 Ekim ~03:30): 1. gün dürüst usta uygun projeyle gelir ve onaylanırsa hediye kesin; " +
                 "2. gün rüşvetçi zarflı projeyle ve kabadayı uygunsuz dosyayla gelir. Oranlar (14/14, 6×3 + 10×1, 1. gün en çok " +
                 "1 uygunsuz) aynen kalır. Sadece UseVisitSchedule açıkken.")]
        public bool ShowcaseEarlyDays;

        [Tooltip("Kişiliksiz müteahhitlerin (ClientSO.IsGeneric) zarf tutarı aralığı")]
        public Vector2Int GenericBribeRange = new Vector2Int(800, 1500);
        public Vector2Int ApprovalFeeRange = new Vector2Int(300, 500);

        [Header("Afet olasılığı")]
        [Range(0f, 1f)] public float DisasterChanceStart = 0.20f;     // Taban (karar, denge turu 17 Ekim)
        [Range(0f, 1f)] public float DisasterChanceIncrement = 0.15f; // Afetsiz gün sonrası artış (karar, denge turu 17 Ekim)
        [Range(0f, 1f)] public float DisasterChanceCap = 1.00f;       // Tavan (karar)
        public bool ResetChanceAfterDisaster = true;                  // Karar: afet olunca tabana döner
        [Tooltip("Jüri vitrini: o güne kadar hiç afet olmadıysa bu günün gecesi afet kesin (0 = kapalı). Önceki gecelerin zarı aynen atılır.")]
        public int GuaranteedDisasterByDay;

        [Header("Yıkılma")]
        public float StrengthNoise = 0.15f; // ±

        [Header("Kayıplar (sadece yıkılan binada)")]
        public Vector2Int ResidentsPerFloor = new Vector2Int(6, 10);
        public Vector2 DeathRatio = new Vector2(0.15f, 0.35f);
        public Vector2 InjuryRatio = new Vector2(0.3f, 0.5f);

        [Header("Hapis riski")]
        public float JailRiskPerCollapse = 0.15f;
        public float JailRiskPerBribedCollapse = 0.25f;

        // Kota yok (karar 2026-10-16 22:40): baskıyı günlük giderler kurar.
    }
}
