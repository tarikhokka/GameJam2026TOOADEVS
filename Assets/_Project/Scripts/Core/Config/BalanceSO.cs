using UnityEngine;

namespace CivilJam.Core
{
    /// <summary>Tüm denge sayıları burada. Değerler öneri; ekip kararıyla sadece asset değişir.</summary>
    [CreateAssetMenu(menuName = "CivilJam/Balance", fileName = "Balance")]
    public class BalanceSO : ScriptableObject
    {
        [Header("Takvim")]
        public int TotalDays = 14;

        [Header("Para")]
        public int StartMoney = 5000;
        public int DebtLimit = 5000;      // Ofis kapanışı kuralı henüz kararlaştırılmadı
        public int ExtraTestCost = 300;   // Anında düşülür
        public int WindowRepairCost = 200;
        public Vector2Int ApprovalFeeRange = new Vector2Int(600, 1000);

        [Header("Afet olasılığı")]
        [Range(0f, 1f)] public float DisasterChanceStart = 0.50f;     // Taban (karar)
        [Range(0f, 1f)] public float DisasterChanceIncrement = 0.10f; // Afetsiz gün sonrası artış (öneri)
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

        [Header("Kota — KALDIRILDI (karar: kota yok, baskıyı giderler kurar)")]
        [Tooltip("Kullanmayın. Desk HUD'u bu alandan çıkınca silinecek.")]
        public int ApprovalQuota = 3;
    }
}
