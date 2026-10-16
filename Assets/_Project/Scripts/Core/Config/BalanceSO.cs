using UnityEngine;

namespace CivilJam.Core
{
    /// <summary>Tüm denge sayıları burada. Değerler öneri; ekip kararıyla sadece asset değişir.</summary>
    [CreateAssetMenu(menuName = "CivilJam/Balance", fileName = "Balance")]
    public class BalanceSO : ScriptableObject
    {
        [Header("Takvim")]
        public int TotalDays = 14;
        public int DaysPerWeek = 7;

        [Header("Para")]
        public int StartMoney = 5000;
        public int DebtLimit = 5000;      // Aşılırsa ofis kapanır
        public int ExtraTestCost = 300;   // Anında düşülür
        public int WindowRepairCost = 200;

        [Header("Afet olasılığı")]
        [Range(0f, 1f)] public float DisasterChanceStart = 0.10f;
        [Range(0f, 1f)] public float DisasterChanceIncrement = 0.10f; // Afetsiz gün sonrası
        [Range(0f, 1f)] public float DisasterChanceCap = 0.80f;
        public bool ResetChanceAfterDisaster = true;

        [Header("Yıkılma")]
        public float StrengthNoise = 0.15f; // ±

        [Header("Hapis riski")]
        public float JailRiskPerCollapse = 0.15f;
        public float JailRiskPerBribedCollapse = 0.25f;

        [Header("Kota")]
        public int WeeklyApprovalQuota = 3;
    }
}
