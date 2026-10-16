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
        public Vector2Int ApprovalFeeRange = new Vector2Int(300, 500);

        [Header("Afet olasılığı")]
        [Range(0f, 1f)] public float DisasterChanceStart = 0.20f;     // Taban (karar, denge turu 17 Ekim)
        [Range(0f, 1f)] public float DisasterChanceIncrement = 0.15f; // Afetsiz gün sonrası artış (karar, denge turu 17 Ekim)
        [Range(0f, 1f)] public float DisasterChanceCap = 1.00f;       // Tavan (karar)
        public bool ResetChanceAfterDisaster = true;                  // Karar: afet olunca tabana döner

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
