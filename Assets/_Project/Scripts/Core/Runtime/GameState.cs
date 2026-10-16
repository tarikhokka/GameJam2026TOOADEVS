using System;
using System.Collections.Generic;

namespace CivilJam.Core
{
    /// <summary>
    /// Kaydedilen tek şey. JsonUtility ile serileşir: sadece public alanlar, property yok.
    /// Kayıt gün sonunda alınır; yükleme sonraki günün DayIntro'sunu açar.
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

        /// <summary>Onaylanıp yapılmış tüm binalar (ayakta + yıkık). Afette ayakta olanlar değerlendirilir.</summary>
        public List<ProjectData> Buildings = new List<ProjectData>();

        // Hafta içi birikenler (haftalık hesaplaşmada sıfırlanır)
        public int ApprovedThisWeek;
        public int WindowRepairsThisWeek; // Kabadayı cam tamiri adedi

        // İstatistik / son koşulları
        public int TotalApproved;
        public int TotalBribesTaken;
        public int TotalCollapses;
        public int BribedCollapses;

        public EndingType Ending = EndingType.None;
    }
}
