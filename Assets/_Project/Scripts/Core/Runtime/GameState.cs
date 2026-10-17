using System;
using System.Collections.Generic;

namespace CivilJam.Core
{
    /// <summary>
    /// Kaydedilen tek şey. JsonUtility ile serileşir: sadece public alanlar, property yok.
    /// Kayıt gün sonunda alınır.
    /// </summary>
    [Serializable]
    public class GameState
    {
        public int Seed;
        public int Day = 1;            // 1'den başlar
        public int Money;
        public int Debt;
        public float JailRisk;         // 0..1, kalıcı
        public float DisasterChance;   // Bu gün sonu afet olasılığı
        public int LastDisasterDay;    // 0 = henüz afet olmadı. Afet sadece DayDecided > LastDisasterDay olan binaları değerlendirir.

        /// <summary>Bu oyunda masaya gelmiş bina adları (aynı ad iki kez gelmesin).</summary>
        public List<string> UsedBuildingNames = new List<string>();

        /// <summary>Oyun başında kurulan ziyaret programı (memory.md "📅"): gün → kim, hangi tür. Boşsa eski gün gün üretim.</summary>
        public List<ScheduledDay> Schedule = new List<ScheduledDay>();

        /// <summary>Ç4: kabul edilen ofis hediyeleri (GiftSO.Id). Masa ekranı ofiste gösterir; kayıtla birlikte saklanır.</summary>
        public List<string> OfficeGifts = new List<string>();

        /// <summary>Ç4: müteahhitlerin bu oyundaki geçmişi (selamlama, zarf artışı).</summary>
        public List<ClientRecord> ClientHistory = new List<ClientRecord>();

        /// <summary>Onaylanıp yapılmış tüm binalar (ayakta + yıkık).</summary>
        public List<ProjectData> Buildings = new List<ProjectData>();

        // Gün içi (her gün sıfırlanır)
        public int ApprovedToday;
        public int WindowRepairsToday; // Kabadayı cam tamiri adedi

        // İstatistik / son koşulları
        public int TotalApproved;
        public int TotalBribesTaken;
        public int TotalCollapses;
        public int BribedCollapses;
        public int TotalDeaths;
        public int TotalInjured;
        public bool Jailed;

        public EndingType Ending = EndingType.None;
    }
}
