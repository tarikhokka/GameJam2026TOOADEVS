namespace CivilJam.Core
{
    /// <summary>
    /// Oyun akışındaki ekran/durum. GameManager bunu değiştirir, ekranlar okur.
    /// Akış: DayIntro → Desk → Construction → (afet varsa Result/gazete, yoksa DaySummary) → DailyExpenses → Save → DayIntro | Ending.
    /// Deprem ayrı ekran değildir; oyuncu gazeteden öğrenir.
    /// </summary>
    public enum GamePhase
    {
        MainMenu,
        DayIntro,
        Desk,
        Construction,
        Result,        // Gazete (sadece afet olan gün)
        DaySummary,    // Gazetesiz gün özeti (afet olmayan gün)
        DailyExpenses,
        Save,
        Ending
    }

    /// <summary>Projede ölçülen yapı parametresi. Required/Reported/Actual bu türe göre karşılaştırılır.</summary>
    public enum ParamType
    {
        ConcreteStrength, // Beton dayanımı
        RebarRatio,       // Donatı oranı
        SoilQuality,      // Zemin etüdü
        SeismicDesign,    // Deprem yönetmeliği uyumu
        FireSafety        // Yangın güvenliği (fabrika)
    }

    public enum BuildingType
    {
        Residential,
        Factory
    }

    public enum DocumentKind
    {
        Application, // Proje başvurusu
        LabReport    // Laboratuvar raporu
    }

    /// <summary>Bedava ipucu türleri: belgeler arası tutarsızlık.</summary>
    public enum ClueType
    {
        ParcelMismatch,
        DateMismatch,
        UnknownLab,
        WrongStamp,
        WrongSignature
    }

    public enum ProjectVerdict
    {
        Pending,
        Approved,
        Rejected
    }

    public enum BuildingStatus
    {
        Standing,
        Collapsed
    }

    public enum EndingType
    {
        None,
        Good,          // Hedef güne yıkımsız/hapissiz ulaştı
        Bad,           // Mahkeme / hapis
        MiddleClosed,  // Borçtan ofis kapandı
        MiddleQuota    // Kota tutmadı
    }
}
